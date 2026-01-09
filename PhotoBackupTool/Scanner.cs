using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoBackupTool
{
    // 非线程安全的扫描器（返回列表），扫描逻辑可进一步抽象或异步化
    public class Scanner
    {
        private readonly BackupSettings settings;
        private readonly HashSet<string> jpegExtensions;
        private readonly HashSet<string> rawExtensions;
        private readonly HashSet<string> videoExtensions;

        public Scanner(BackupSettings settings)
        {
            this.settings = settings;
            jpegExtensions = new HashSet<string>(new[] { ".jpg", ".jpeg", ".png" }, StringComparer.OrdinalIgnoreCase);
            rawExtensions = new HashSet<string>(settings.SelectedRawFormats ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            videoExtensions = new HashSet<string>(settings.SelectedVideoFormats ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
        }

        public IList<BackupItem> Scan()
        {
            // 使用 EnumerateFiles 避免一次性把所有路径加载到内存，适用于大型目录或 NAS
            IEnumerable<string> filesEnum;
            try
            {
                filesEnum = Directory.EnumerateFiles(settings.SourcePath, "*.*", SearchOption.AllDirectories)
                    .Where(f => IsMediaFile(f));
            }
            catch
            {
                // 如果遍历根目录失败（权限等），返回空列表
                return new List<BackupItem>();
            }

            var list = new List<BackupItem>();
            foreach (var f in filesEnum)
            {
                string ext = Path.GetExtension(f);
                string fileName = Path.GetFileName(f);
                string dest = GetDestinationPath(f, ext, fileName);

                long len = 0;
                try { len = new FileInfo(f).Length; } catch { }

                list.Add(new BackupItem
                {
                    SourcePath = f,
                    DestinationPath = dest,
                    Length = len
                });
            }

            return list;
        }

        private bool IsMediaFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return jpegExtensions.Contains(extension) || rawExtensions.Contains(extension) || videoExtensions.Contains(extension);
        }

        private string GetDestinationPath(string sourceFile, string extension, string fileName)
        {
            if (settings.PreserveFolderStructure)
            {
                string relativePath = sourceFile.Substring(settings.SourcePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (jpegExtensions.Contains(extension))
                    return Path.Combine(settings.JpegDestinationPath, relativePath);
                if (rawExtensions.Contains(extension))
                    return Path.Combine(settings.RawDestinationPath, relativePath);
                return Path.Combine(settings.VideoDestinationPath ?? settings.JpegDestinationPath, relativePath);
            }
            else
            {
                if (jpegExtensions.Contains(extension))
                    return Path.Combine(settings.JpegDestinationPath, fileName);
                if (rawExtensions.Contains(extension))
                    return Path.Combine(settings.RawDestinationPath, fileName);
                return Path.Combine(settings.VideoDestinationPath ?? settings.JpegDestinationPath, fileName);
            }
        }
    }
}