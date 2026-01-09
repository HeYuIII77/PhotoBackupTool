using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PhotoBackupTool
{
    // 简化版：每 channel 保留最新 snapshot，定期聚合并调用 ReportProgress
    public class ProgressAggregator : IDisposable
    {
        private readonly BackgroundWorker worker;
        private readonly ConcurrentDictionary<int, BackupProgressInfo> channelSnapshots;
        private readonly Func<long> getCompletedBytes;
        private readonly Func<long> getInProgressBytesTotal;
        private readonly Func<int> getProcessedFiles;
        private readonly int totalFilesCount;
        private readonly Stopwatch stopwatch;
        private readonly CancellationTokenSource cts;
        private readonly int throttleMs;
        private Task reporter;
        private readonly long totalBytesAll;
        private readonly ConcurrentDictionary<int, long> lastChannelReportMs;
        private readonly int perChannelIntervalMs = 200; // 每通道上报最小间隔（ms）
        private double avgSpeedBytesPerSecond;
        private double speedEmaAlpha = 0.2;
        private long lastOverallCompletedBytes;
        private double lastOverallTimeSeconds;
        private readonly int summaryIntervalMs = 5000;
        private long lastSummaryReportMs;
        public volatile BackupProgressInfo LatestOverall; // UI 可直接读取

        public ProgressAggregator(BackgroundWorker worker, Func<long> getCompletedBytes, Func<long> getInProgressBytesTotal, Stopwatch stopwatch, int throttleMs, long totalBytesAll, Func<int> getProcessedFiles, int totalFilesCount)
        {
            this.worker = worker;
            this.getCompletedBytes = getCompletedBytes;
            this.getInProgressBytesTotal = getInProgressBytesTotal;
            this.stopwatch = stopwatch;
            this.throttleMs = Math.Max(50, Math.Min(200, throttleMs <= 0 ? 150 : throttleMs));
            channelSnapshots = new ConcurrentDictionary<int, BackupProgressInfo>();
            lastChannelReportMs = new ConcurrentDictionary<int, long>();
            this.getProcessedFiles = getProcessedFiles;
            this.totalFilesCount = totalFilesCount;
            cts = new CancellationTokenSource();
            this.totalBytesAll = totalBytesAll;
        }

        public void Start()
        {
            var token = cts.Token;
            reporter = Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(throttleMs, token).ConfigureAwait(false);
                        long overallCompleted = getCompletedBytes() + getInProgressBytesTotal();
                        int overallPercent = totalBytesAll > 0 ? (int)(overallCompleted * 100 / totalBytesAll) : 0;

                        // 计算瞬时速度并更新 EMA 平滑速度
                        double nowSeconds = stopwatch.Elapsed.TotalSeconds;
                        double deltaTime = nowSeconds - lastOverallTimeSeconds;
                        long deltaBytes = overallCompleted - lastOverallCompletedBytes;
                        double instantSpeed = 0;
                        if (deltaTime > 0) instantSpeed = deltaBytes / deltaTime;

                        if (avgSpeedBytesPerSecond <= 0)
                            avgSpeedBytesPerSecond = instantSpeed;
                        else
                            avgSpeedBytesPerSecond = avgSpeedBytesPerSecond * (1 - speedEmaAlpha) + instantSpeed * speedEmaAlpha;

                        lastOverallTimeSeconds = nowSeconds;
                        lastOverallCompletedBytes = overallCompleted;

                        var best = channelSnapshots.Values.OrderByDescending(p => p?.ElapsedSeconds ?? 0).FirstOrDefault();

                        var info = new BackupProgressInfo
                        {
                            OverallPercent = overallPercent,
                            FilePercent = best?.FilePercent ?? 0,
                            FileName = best?.FileName ?? string.Empty,
                            ChannelId = best?.ChannelId ?? -1,
                            ProcessedFiles = getProcessedFiles != null ? getProcessedFiles() : (best?.ProcessedFiles ?? 0),
                            TotalFiles = totalFilesCount,
                            // 使用 EMA 平滑后的总体吞吐率作为报告速度
                            ThroughputBytesPerSecond = avgSpeedBytesPerSecond,
                            ElapsedSeconds = nowSeconds,
                            EstimatedRemainingSeconds = 0
                        };

                        if (info.ThroughputBytesPerSecond > 0 && totalBytesAll > overallCompleted)
                            info.EstimatedRemainingSeconds = (totalBytesAll - overallCompleted) / info.ThroughputBytesPerSecond;

                        // 为了使 UI 能显示已处理文件数/总文件数，从外部读取一次（用 getCompletedBytes 之外的方式）
                        LatestOverall = info;
                        try { worker.ReportProgress(info.OverallPercent, info); } catch { }

                        // 逐通道上报各自 snapshot（带节流），避免 UI 过频刷新
                        try
                        {
                            var nowMs = (long)stopwatch.ElapsedMilliseconds;
                            var snaps = channelSnapshots.ToArray();
                            foreach (var kv in snaps)
                            {
                                var ch = kv.Key;
                                var snap = kv.Value;
                                if (snap == null) continue;
                                long lastMs = 0;
                                lastChannelReportMs.TryGetValue(ch, out lastMs);
                                if (nowMs - lastMs >= perChannelIntervalMs)
                                {
                                    try { worker.ReportProgress(info.OverallPercent, snap); } catch { }
                                    lastChannelReportMs[ch] = nowMs;
                                }
                            }
                        }
                        catch { }

                        // 周期性在日志中输出摘要：已处理文件数 / 总文件数、用时、平均速度
                        try
                        {
                            var nowMs = (long)stopwatch.ElapsedMilliseconds;
                            if (nowMs - lastSummaryReportMs >= summaryIntervalMs)
                            {
                                int processed = getProcessedFiles != null ? getProcessedFiles() : 0;
                                var elapsed = TimeSpan.FromSeconds(nowSeconds);
                                var avgSpeed = avgSpeedBytesPerSecond / (1024.0 * 1024.0);
                                var summary = $"已处理: {processed}/{totalFilesCount} 文件, 用时: {elapsed:hh\\:mm\\:ss}, 平均速度: {avgSpeed:F2} MB/s";
                                try { worker.ReportProgress(info.OverallPercent, summary); } catch { }
                                lastSummaryReportMs = nowMs;
                            }
                        }
                        catch { }
                    }
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        public void Stop()
        {
            try
            {
                cts.Cancel();
                reporter?.Wait(300);
            }
            catch { }
            // 清理所有通道 snapshot
            try
            {
                foreach (var k in channelSnapshots.Keys) channelSnapshots.TryRemove(k, out _);
                foreach (var k in lastChannelReportMs.Keys) lastChannelReportMs.TryRemove(k, out _);
            }
            catch { }
        }

        public void UpdateChannel(int channelId, BackupProgressInfo snapshot)
        {
            if (snapshot == null) channelSnapshots.TryRemove(channelId, out _);
            else channelSnapshots[channelId] = snapshot;
        }

        public void Dispose()
        {
            Stop();
            try { cts.Dispose(); } catch { }
        }
    }
}