using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PhotoBackupTool
{
    // 单个 worker：消费队列并执行复制。简洁实现，保留占位清理点与取消检查
    public class CopyWorker
    {
        private readonly BlockingCollection<BackupItem> queue;
        private readonly ProgressAggregator aggregator;
        private readonly int channelId;
        private readonly CancellationToken token;

        // 增量统计辅助（可外部注入引用以维护全局 inProgressTotal）
        private readonly ConcurrentDictionary<string, long> inProgressBytes;
        private readonly Action<long> addCompletedBytes;
        private readonly Action addProcessedFile;
        private readonly Stopwatch stopwatch;

        public CopyWorker(BlockingCollection<BackupItem> queue, ConcurrentDictionary<string, long> inProgressBytes, Action<long> addCompletedBytes, Action addProcessedFile, ProgressAggregator aggregator, int channelId, CancellationToken token, Stopwatch stopwatch)
        {
            this.queue = queue;
            this.aggregator = aggregator;
            this.channelId = channelId;
            this.token = token;
            this.inProgressBytes = inProgressBytes;
            this.addCompletedBytes = addCompletedBytes;
            this.addProcessedFile = addProcessedFile;
            this.stopwatch = stopwatch;
        }

        public async Task RunAsync()
        {
            try
            {
                foreach (var item in queue.GetConsumingEnumerable(token))
                {
                    if (token.IsCancellationRequested) break;

                    bool placeholderCreated = false;
                    string placeholder = null;
                    try
                    {
                        // 确保目录
                        var dir = Path.GetDirectoryName(item.DestinationPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        // 处理重复：此处简化为重命名策略（可替换为原子 CreateNew 占位）
                        if (File.Exists(item.DestinationPath))
                        {
                            placeholder = GetUniqueFileName(item.DestinationPath);
                        }
                        var dest = placeholder ?? item.DestinationPath;

                        await CopyWithProgress(item.SourcePath, dest, item.Length).ConfigureAwait(false);

                        // 标记已完成字节并记录已处理文件数（先累加已完成，再移除 in-progress，避免瞬时空窗导致总体已完成字节回退）
                        addCompletedBytes(item.Length);
                        try { addProcessedFile(); } catch { }
                        try { inProgressBytes.TryRemove(item.SourcePath, out _); } catch { }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        try { ErrorLogger.LogException(ex, null); } catch { }
                    }
                    finally
                    {
                        // 清理占位 0 字节文件
                        if (placeholderCreated && placeholder != null)
                        {
                            try
                            {
                                if (File.Exists(placeholder) && new FileInfo(placeholder).Length == 0) File.Delete(placeholder);
                            }
                            catch { }
                        }
                    }
                }
            }
            finally
            {
                // 通道任务完成或取消后清除该通道 snapshot，避免 UI 保留已结束的通道信息
                try { aggregator.UpdateChannel(channelId, null); } catch { }
            }
        }

        private async Task CopyWithProgress(string source, string dest, long totalBytes)
        {
            const int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize];
            long copied = 0;
            using (var rs = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var ws = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
            {
                int read;
                // throughput delta tracking
                long lastCopiedForThroughput = 0;
                double lastTimeSeconds = stopwatch?.Elapsed.TotalSeconds ?? 0.0;

                while ((read = await rs.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                {
                    token.ThrowIfCancellationRequested();
                    await ws.WriteAsync(buffer, 0, read, token).ConfigureAwait(false);
                    copied += read;

                    // 更新 in-progress map（保持最后值）
                    inProgressBytes.AddOrUpdate(source, copied, (k, old) => copied);

                    // 计算窗口速率：基于 delta bytes / delta time
                    var nowSeconds = stopwatch?.Elapsed.TotalSeconds ?? 0.0;
                    var deltaBytes = copied - lastCopiedForThroughput;
                    var deltaTime = nowSeconds - lastTimeSeconds;
                    double throughput = 0.0;
                    if (deltaTime > 0 && deltaBytes > 0)
                        throughput = deltaBytes / deltaTime;

                    // 更新 last values for next window
                    if (deltaTime > 0)
                    {
                        lastCopiedForThroughput = copied;
                        lastTimeSeconds = nowSeconds;
                    }

                    var info = new BackupProgressInfo
                    {
                        ChannelId = channelId,
                        FileName = Path.GetFileName(source),
                        FilePercent = totalBytes > 0 ? (int)(copied * 100 / totalBytes) : 0,
                        ThroughputBytesPerSecond = throughput,
                        ElapsedSeconds = nowSeconds,
                        ProcessedFiles = 0,
                        TotalFiles = 0,
                        OverallPercent = 0
                    };
                    aggregator.UpdateChannel(channelId, info);
                }
            }
        }

        private string GetUniqueFileName(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string name = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);
            int i = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(directory, $"{name}_{i++}{ext}");
            } while (File.Exists(candidate));
            return candidate;
        }
    }
}