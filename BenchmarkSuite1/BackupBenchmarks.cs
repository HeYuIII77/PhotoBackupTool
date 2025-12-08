using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.VSDiagnostics;

namespace PhotoBackupTool.Benchmarks
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

        [Params(2 * 1024 * 1024)] // size per file (2MB)
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
                    var buffer = new byte[FileSize];
                    rnd.NextBytes(buffer);
                    fs.Write(buffer, 0, buffer.Length);
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
                PreserveFolderStructure = false
            };
            var manager = new PhotoBackupTool.BackupManager(worker, settings);
            manager.PerformBackup();
        }
    }
}