using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundWorker = System.ComponentModel.BackgroundWorker;

namespace PhotoBackupTool
{
    public class BackupProgressInfo
    {
        public int OverallPercent { get; set; }
        public int FilePercent { get; set; }
        public string FileName { get; set; }
        public int ChannelId { get; set; } = -1; // -1 表示无通道信息
        public int ProcessedFiles { get; set; }
        public int TotalFiles { get; set; }
        public double ThroughputBytesPerSecond { get; set; }
        public double ElapsedSeconds { get; set; }
        public double EstimatedRemainingSeconds { get; set; }
    }

    public class BackupManager
    {
        private readonly BackgroundWorker worker;
        private readonly BackupSettings settings;
        private readonly HashSet<string> jpegExtensions;
        private readonly HashSet<string> rawExtensions;
        private readonly HashSet<string> videoExtensions;
        private int bufferSize = 4 * 1024 * 1024; // 4MB 默认缓冲

        public BackupManager(BackgroundWorker worker, BackupSettings settings)
        {
            this.worker = worker;
            this.settings = settings;
            jpegExtensions = new HashSet<string>(new[] { ".jpg", ".jpeg", ".png" }, StringComparer.OrdinalIgnoreCase);
            rawExtensions = new HashSet<string>(settings.SelectedRawFormats ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            videoExtensions = new HashSet<string>(settings.SelectedVideoFormats ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
        }

        public void PerformBackup()
        {
            // 调用异步实现并同步等待
            PerformBackupAsync().GetAwaiter().GetResult();
        }

        private async Task PerformBackupAsync()
        {
            // 1) 预建目录等（保留原逻辑）
            Directory.CreateDirectory(settings.JpegDestinationPath);
            Directory.CreateDirectory(settings.RawDestinationPath);
            if (!string.IsNullOrEmpty(settings.VideoDestinationPath)) Directory.CreateDirectory(settings.VideoDestinationPath);

            // 2) 扫描
            var scanner = new Scanner(settings);
            var items = scanner.Scan();

            // 3) 统计总字节
            long totalBytesAll = items.Sum(i => i.Length);

            // 4) 聚合器
            var stopwatch = Stopwatch.StartNew();
            var inProgressBytes = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            long completedBytes = 0;
            long processedFiles = 0;
            void AddCompleted(long b) => Interlocked.Add(ref completedBytes, b);
            void AddProcessedFile() => Interlocked.Increment(ref processedFiles);

            var aggregator = new ProgressAggregator(worker, () => Interlocked.Read(ref completedBytes), () => inProgressBytes.Values.Sum(), stopwatch, 150, totalBytesAll, () => (int)Interlocked.Read(ref processedFiles), items.Count);
            aggregator.Start();

            // 5) Scheduler：创建固定数量的通道（等于并行度），并将任务轮询分配到各通道
            int totalChannels = Math.Max(1, settings.MaxDegreeOfParallelism);
            try { worker.ReportProgress(0, $"CHANNELS:{totalChannels}"); } catch { }

            var queues = new List<BlockingCollection<BackupItem>>(totalChannels);
            int approxPerQueue = Math.Max(100, Math.Max(1, items.Count() / totalChannels));
            for (int i = 0; i < totalChannels; i++)
                queues.Add(new BlockingCollection<BackupItem>(boundedCapacity: approxPerQueue));

            int assign = 0;
            foreach (var it in items)
            {
                queues[assign].Add(it);
                assign = (assign + 1) % totalChannels;
            }

            foreach (var q in queues) q.CompleteAdding();

            var cts = new CancellationTokenSource();
            var consumerTasks = new List<Task>();
            for (int ch = 0; ch < totalChannels; ch++)
            {
                var q = queues[ch];
                var workerTask = new CopyWorker(q, inProgressBytes, AddCompleted, AddProcessedFile, aggregator, ch, cts.Token, stopwatch).RunAsync();
                consumerTasks.Add(workerTask);
            }

            // 6) 等待完成
            await Task.WhenAll(consumerTasks).ConfigureAwait(false);

            // 7) 停止聚合器并最终报告
            aggregator.Stop();
            stopwatch.Stop();
            try
            {
                var elapsed = stopwatch.Elapsed;
                var avgSpeed = elapsed.TotalSeconds > 0 ? (completedBytes / elapsed.TotalSeconds) : 0;
                var elapsedStr = elapsed.ToString(@"hh\:mm\:ss");
                worker.ReportProgress(100, $"备份完成！ 文件数: {processedFiles}, 用时: {elapsedStr}, 平均速度: {avgSpeed / (1024.0*1024.0):F2} MB/s");
            } catch { }
        }

        private string GetDestinationRoot(string sourceFile)
        {
            var ext = Path.GetExtension(sourceFile);
            string destPath;
            if (jpegExtensions.Contains(ext)) destPath = settings.JpegDestinationPath;
            else if (rawExtensions.Contains(ext)) destPath = settings.RawDestinationPath;
            else destPath = settings.VideoDestinationPath ?? settings.JpegDestinationPath;

            try { return Path.GetPathRoot(destPath) ?? destPath; } catch { return destPath; }
        }

        private bool IsMediaFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return jpegExtensions.Contains(extension) || rawExtensions.Contains(extension) || videoExtensions.Contains(extension);
        }

        // 原有的 ProcessFileAsync（保留）
        private async Task ProcessFileAsync(string sourceFile, int totalFiles, Func<long> getCumulativeBytes, long totalBytesAll, Stopwatch stopwatch, int channelId = -1)
        {
            var extension = Path.GetExtension(sourceFile);
            var fileName = Path.GetFileName(sourceFile);
            string destinationPath = GetDestinationPath(sourceFile, extension, fileName);

            if (File.Exists(destinationPath))
            {
                destinationPath = HandleDuplicateFile(destinationPath);
                if (destinationPath == null)
                    return;
            }

            // 确保目标目录存在（处理保留目录结构时可能需要创建多级目录）
            try
            {
                var destDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
            }
            catch (Exception ex)
            {
                // 记录并抛出，交由上层处理
                ErrorLogger.LogException(ex, settings, totalFiles, 0, sourceFile);
                throw;
            }

            await CopyFileWithProgressAsync(sourceFile, destinationPath, getCumulativeBytes, totalBytesAll, stopwatch, channelId);
        }

        // 新增：兼容 9 参数重载（在并行改进调用处使用）
        private async Task ProcessFileAsync(
            string sourceFile,
            int totalFiles,
            ConcurrentDictionary<string, long> inProgressBytes,
            Func<long> getCompletedBytes,
            long totalBytesAll,
            Stopwatch stopwatch,
            ConcurrentDictionary<int, BackupProgressInfo> latestProgressByChannel,
            CancellationToken token,
            int channelId = -1)
        {
            token.ThrowIfCancellationRequested();

            var extension = Path.GetExtension(sourceFile);
            var fileName = Path.GetFileName(sourceFile);
            string destinationPath = GetDestinationPath(sourceFile, extension, fileName);

            if (File.Exists(destinationPath))
            {
                // 使用现有非原子重命名策略（保持与当前代码兼容）
                destinationPath = HandleDuplicateFile(destinationPath);
                if (destinationPath == null)
                    return;
            }

            // 确保目标目录存在（处理保留目录结构时可能需要创建多级目录）
            try
            {
                var destDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
            }
            catch (Exception ex)
            {
                // 记录并抛出，交由上层处理
                ErrorLogger.LogException(ex, settings, totalFiles, 0, sourceFile);
                throw;
            }

            await CopyFileWithProgressAsync(sourceFile, destinationPath, inProgressBytes, getCompletedBytes, totalBytesAll, stopwatch, latestProgressByChannel, token, channelId).ConfigureAwait(false);
        }

        private string GetDestinationPath(string sourceFile, string extension, string fileName)
        {
            if (settings.PreserveFolderStructure)
            {
                string relativePath = sourceFile.Substring(settings.SourcePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (jpegExtensions.Contains(extension))
                    return Path.Combine(settings.JpegDestinationPath, relativePath);
                else if (rawExtensions.Contains(extension))
                    return Path.Combine(settings.RawDestinationPath, relativePath);
                else
                    return Path.Combine(settings.VideoDestinationPath ?? settings.JpegDestinationPath, relativePath);
            }
            else
            {
                if (jpegExtensions.Contains(extension))
                    return Path.Combine(settings.JpegDestinationPath, fileName);
                else if (rawExtensions.Contains(extension))
                    return Path.Combine(settings.RawDestinationPath, fileName);
                else
                    return Path.Combine(settings.VideoDestinationPath ?? settings.JpegDestinationPath, fileName);
            }
        }

        private async Task CopyFileWithProgressAsync(string source, string dest, Func<long> getCumulativeBytes, long totalBytesAll, Stopwatch stopwatch, int channelId = -1)
        {
            // 使用异步读写以提高并发吞吐
            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
            {
                long totalBytes = sourceStream.Length;
                long copiedBytes = 0;
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    if (worker.CancellationPending)
                    {
                        try { destStream.Close(); File.Delete(dest); } catch { }
                        throw new OperationCanceledException();
                    }

                    await destStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                    copiedBytes += bytesRead;

                    double overallCompletedBytes = getCumulativeBytes() + copiedBytes;
                    int overallPercent = totalBytesAll > 0 ? (int)(overallCompletedBytes * 100 / totalBytesAll) : 0;
                    int filePercent = totalBytes > 0 ? (int)(copiedBytes * 100 / totalBytes) : 0;

                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    double speed = elapsed > 0 ? (overallCompletedBytes / elapsed) : 0; // bytes/sec
                    double remainingSeconds = 0;
                    if (speed > 0 && totalBytesAll > overallCompletedBytes)
                        remainingSeconds = (totalBytesAll - overallCompletedBytes) / speed;

                    var info = new BackupProgressInfo
                    {
                        OverallPercent = overallPercent,
                        FilePercent = filePercent,
                        FileName = Path.GetFileName(source),
                        ChannelId = channelId,
                        ThroughputBytesPerSecond = speed,
                        ElapsedSeconds = elapsed,
                        EstimatedRemainingSeconds = remainingSeconds
                    };

                    worker.ReportProgress(overallPercent, info);
                }
            }
        }

        // 新增：CopyFileWithProgressAsync 的并行版（支持 inProgressBytes 与 per-channel snapshot）
        private async Task CopyFileWithProgressAsync(
            string source,
            string dest,
            ConcurrentDictionary<string, long> inProgressBytes,
            Func<long> getCompletedBytes,
            long totalBytesAll,
            Stopwatch stopwatch,
            ConcurrentDictionary<int, BackupProgressInfo> latestProgressByChannel,
            CancellationToken token,
            int channelId = -1)
        {
            token.ThrowIfCancellationRequested();

            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
            {
                long totalBytes = sourceStream.Length;
                long copiedBytes = 0;
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    if (token.IsCancellationRequested || worker.CancellationPending)
                    {
                        try { destStream.Close(); File.Delete(dest); } catch { }
                        throw new OperationCanceledException();
                    }

                    await destStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                    copiedBytes += bytesRead;

                    // 更新 in-progress bytes（按文件路径），仅保留最新值
                    inProgressBytes.AddOrUpdate(source, copiedBytes, (k, old) => copiedBytes);

                    // 计算总体已完成字节（已完成 + in-progress）
                    long inProgressSum = 0;
                    foreach (var v in inProgressBytes.Values) inProgressSum += v;
                    long overallCompletedBytes = getCompletedBytes() + inProgressSum;

                    int overallPercent = totalBytesAll > 0 ? (int)(overallCompletedBytes * 100 / totalBytesAll) : 0;
                    int filePercent = totalBytes > 0 ? (int)(copiedBytes * 100 / totalBytes) : 0;

                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    double speed = elapsed > 0 ? (overallCompletedBytes / elapsed) : 0; // bytes/sec
                    double remainingSeconds = 0;
                    if (speed > 0 && totalBytesAll > overallCompletedBytes)
                        remainingSeconds = (totalBytesAll - overallCompletedBytes) / speed;

                    var info = new BackupProgressInfo
                    {
                        OverallPercent = overallPercent,
                        FilePercent = filePercent,
                        FileName = Path.GetFileName(source),
                        ChannelId = channelId,
                        ThroughputBytesPerSecond = speed,
                        ElapsedSeconds = elapsed,
                        EstimatedRemainingSeconds = remainingSeconds
                    };

                    // 保留每通道最新状态，供限频上报器汇总
                    latestProgressByChannel[channelId] = info;
                }

                // 文件复制完成：移除 inProgress 记录，已完成字节将由调用方累加到 completedBytes
                long removed;
                inProgressBytes.TryRemove(source, out removed);
            }
        }

        private string HandleDuplicateFile(string destinationPath)
        {
            switch (settings.DuplicateAction)
            {
                case DuplicateAction.Overwrite:
                    return destinationPath;
                case DuplicateAction.Skip:
                    return null;
                case DuplicateAction.Rename:
                    return GetUniqueFileName(destinationPath);
                default:
                    return destinationPath;
            }
        }

        private string GetUniqueFileName(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            int counter = 1;
            string newFilePath;
            do
            {
                newFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{counter}{extension}");
                counter++;
            } while (File.Exists(newFilePath));

            return newFilePath;
        }

        private int CalculateProgress(int processed, int total)
        {
            return total == 0 ? 0 : (int)((double)processed / total * 100);
        }
    }
}
