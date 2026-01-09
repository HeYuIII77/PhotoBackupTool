
using System;

namespace PhotoBackupTool
{
    public class BackupItem
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long Length { get; set; }
        // 可选：原始相对路径、文件扩展名等
    }
}