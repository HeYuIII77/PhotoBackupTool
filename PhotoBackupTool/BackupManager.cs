using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoBackupTool
{
    public class BackupProgressInfo
    {
        public int OverallPercent { get; set; }
        public int FilePercent { get; set; }
        public string FileName { get; set; }
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
            Directory.CreateDirectory(settings.JpegDestinationPath);
            Directory.CreateDirectory(settings.RawDestinationPath);
            if (!string.IsNullOrEmpty(settings.VideoDestinationPath))
                Directory.CreateDirectory(settings.VideoDestinationPath);

            var allFiles = Directory.GetFiles(settings.SourcePath, "*.*", SearchOption.AllDirectories)
                .Where(f => IsMediaFile(f)).ToArray();

            int totalFiles = allFiles.Length;

            long totalBytesAll = 0;
            foreach (var f in allFiles)
            {
                try { totalBytesAll += new FileInfo(f).Length; } catch { }
            }

            long cumulativeBytes = 0;
            var stopwatch = Stopwatch.StartNew();

            worker.ReportProgress(0, "开始备份...");

            if (settings.MaxDegreeOfParallelism <= 1)
            {
                int processedFiles = 0;
                foreach (var sourceFile in allFiles)
                {
                    if (worker.CancellationPending)
                        break;

                    try
                    {
                        await ProcessFileAsync(sourceFile, totalFiles, () => Interlocked.Read(ref cumulativeBytes), totalBytesAll, stopwatch);
                        try { Interlocked.Add(ref cumulativeBytes, new FileInfo(sourceFile).Length); } catch { }
                        Interlocked.Increment(ref processedFiles);
                        worker.ReportProgress(CalculateProgress(processedFiles, totalFiles), $"已复制: {Path.GetFileName(sourceFile)}");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            else
            {
                // 更激进的并行策略：按目标盘符分组，限制每盘并发
                var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var f in allFiles)
                {
                    string dest = GetDestinationRoot(f);
                    if (!groups.ContainsKey(dest)) groups[dest] = new List<string>();
                    groups[dest].Add(f);
                }

                var semaphores = new Dictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);
                int perDiskConcurrency = Math.Max(1, settings.MaxDegreeOfParallelism / Math.Max(1, groups.Count));
                foreach (var g in groups.Keys)
                    semaphores[g] = new SemaphoreSlim(perDiskConcurrency);

                var tasks = new List<Task>();
                int processedFiles = 0;

                foreach (var kv in groups)
                {
                    var root = kv.Key;
                    var list = kv.Value;
                    foreach (var sourceFile in list)
                    {
                        if (worker.CancellationPending) break;
                        var sem = semaphores[root];
                        await sem.WaitAsync();
                        var t = Task.Run(async () =>
                        {
                            try
                            {
                                try
                                {
                                    await ProcessFileAsync(sourceFile, totalFiles, () => Interlocked.Read(ref cumulativeBytes), totalBytesAll, stopwatch);
                                }
                                catch (Exception ex)
                                {
                                    ErrorLogger.LogException(ex, settings, totalFiles, processedFiles, sourceFile);
                                    throw;
                                }

                                try { Interlocked.Add(ref cumulativeBytes, new FileInfo(sourceFile).Length); } catch { }
                                Interlocked.Increment(ref processedFiles);
                                worker.ReportProgress(CalculateProgress(processedFiles, totalFiles), $"已复制: {Path.GetFileName(sourceFile)}");
                            }
                            finally
                            {
                                sem.Release();
                            }
                        });
                        tasks.Add(t);
                    }
                }

                await Task.WhenAll(tasks);
            }

            stopwatch.Stop();
            worker.ReportProgress(100, "备份完成！");
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

        private async Task ProcessFileAsync(string sourceFile, int totalFiles, Func<long> getCumulativeBytes, long totalBytesAll, Stopwatch stopwatch)
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

            await CopyFileWithProgressAsync(sourceFile, destinationPath, getCumulativeBytes, totalBytesAll, stopwatch);
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

        private async Task CopyFileWithProgressAsync(string source, string dest, Func<long> getCumulativeBytes, long totalBytesAll, Stopwatch stopwatch)
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
                        ElapsedSeconds = elapsed,
                        EstimatedRemainingSeconds = remainingSeconds
                    };

                    worker.ReportProgress(overallPercent, info);
                }
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
