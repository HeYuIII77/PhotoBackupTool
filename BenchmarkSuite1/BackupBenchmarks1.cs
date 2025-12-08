using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.VSDiagnostics;

namespace BenchmarkSuite1
{
    [CPUUsageDiagnoser]
    public class BackupBenchmarks
    {
        private string tempRoot;
        private string sourceDir;
        private string jpegDest;
        private string rawDest;
        private string videoDest;
        private BackgroundWorker worker;
        [Params(10)] // number of files
        public int FileCount { get; set; }

        [Params(100 * 1024 * 1024)] // size per file (100MB)
        public int FileSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            tempRoot = Path.Combine(Path.GetTempPath(), "PhotoBackupBench", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempRoot);
            sourceDir = Path.Combine(tempRoot, "source");
            jpegDest = Path.Combine(tempRoot, "jpeg");
            rawDest = Path.Combine(tempRoot, "raw");
            videoDest = Path.Combine(tempRoot, "video");
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(jpegDest);
            Directory.CreateDirectory(rawDest);
            Directory.CreateDirectory(videoDest);
            // create test files
            var rnd = new Random(12345);
            for (int i = 0; i < FileCount; i++)
            {
                var path = Path.Combine(sourceDir, $"file_{i:D4}.jpg");
                using (var fs = File.Create(path))
                {
                    var buffer = new byte[1024 * 1024]; // write 1MB blocks
                    int remaining = FileSize;
                    while (remaining > 0)
                    {
                        int toWrite = Math.Min(buffer.Length, remaining);
                        rnd.NextBytes(buffer);
                        fs.Write(buffer, 0, toWrite);
                        remaining -= toWrite;
                    }
                }
            }

            // create a BackgroundWorker acceptable to BackupManager
            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            try
            {
                Directory.Delete(tempRoot, true);
            }
            catch
            {
            }
        }

        [Benchmark]
        public void RunBackup()
        {
            var settings = new PhotoBackupTool.BackupSettings
            {
                SourcePath = sourceDir,
                JpegDestinationPath = jpegDest,
                RawDestinationPath = rawDest,
                VideoDestinationPath = videoDest,
                SelectedRawFormats = new System.Collections.Generic.List<string>(),
                SelectedVideoFormats = new System.Collections.Generic.List<string>(),
                DuplicateAction = PhotoBackupTool.DuplicateAction.Overwrite,
                PreserveFolderStructure = false,
                MaxDegreeOfParallelism = 1
            };
            var manager = new PhotoBackupTool.BackupManager(worker, settings);
            manager.PerformBackup();
        }
    }
}