using System;
using System.IO;
using System.Text;

namespace PhotoBackupTool
{
    internal static class ErrorLogger
    {
        private static readonly object fileLock = new object();

        public static void LogException(Exception ex, BackupSettings settings = null, int totalFiles = 0, int currentIndex = 0, string fileName = null)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var errDir = Path.Combine(baseDir, "Error");
                if (!Directory.Exists(errDir)) Directory.CreateDirectory(errDir);

                var logFile = Path.Combine(errDir, $"error_{DateTime.Now:yyyyMMdd}.log");

                var sb = new StringBuilder();
                sb.AppendLine("------------------------------");
                sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                if (settings != null)
                {
                    sb.AppendLine($"SourcePath: {settings.SourcePath}");
                    sb.AppendLine($"JpegDestinationPath: {settings.JpegDestinationPath}");
                    sb.AppendLine($"RawDestinationPath: {settings.RawDestinationPath}");
                    sb.AppendLine($"VideoDestinationPath: {settings.VideoDestinationPath}");
                    sb.AppendLine($"MaxDegreeOfParallelism: {settings.MaxDegreeOfParallelism}");
                }
                // 机器与用户信息
                try
                {
                    sb.AppendLine($"MachineName: {Environment.MachineName}");
                    sb.AppendLine($"UserName: {Environment.UserName}");
                    sb.AppendLine($"OSVersion: {Environment.OSVersion}");
                }
                catch { }
                if (totalFiles > 0)
                {
                    sb.AppendLine($"Progress: {currentIndex}/{totalFiles}");
                }
                if (!string.IsNullOrEmpty(fileName))
                    sb.AppendLine($"File: {fileName}");

                sb.AppendLine("Exception:");
                sb.AppendLine(ex.ToString());
                sb.AppendLine();

                lock (fileLock)
                {
                    File.AppendAllText(logFile, sb.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // swallow logging exceptions
            }
        }
    }
}
