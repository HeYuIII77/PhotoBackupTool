using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace PhotoBackupTool
{
    public partial class MainForm : Form
    {
        private BackgroundWorker backupWorker;
        private Dictionary<string, string[]> photoFormats;
        private Dictionary<string, string[]> videoFormats;

        public MainForm()
        {
            InitializeComponent();
            InitializePhotoFormats();
            InitializeVideoFormats();
            InitializeControls();
            InitializeBackgroundWorker();
            // 确保程序启动时存在 Error 文件夹
            try { EnsureErrorFolderExists(); } catch { }
        }

        private void btnOpenErrorFolder_Click(object sender, EventArgs e)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var errDir = Path.Combine(baseDir, "Error");
                if (!Directory.Exists(errDir))
                {
                    MessageBox.Show("错误文件夹不存在。", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                System.Diagnostics.Process.Start("explorer.exe", errDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开错误文件夹: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportLog_Click(object sender, EventArgs e)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var errDir = Path.Combine(baseDir, "Error");
                if (!Directory.Exists(errDir))
                {
                    MessageBox.Show("没有错误日志可导出。", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dlg = new FolderBrowserDialog())
                {
                    dlg.Description = "选择导出错误日志的目标文件夹";
                    dlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;

                    var destRoot = Path.Combine(dlg.SelectedPath, $"ErrorExport_{DateTime.Now:yyyyMMdd_HHmmss}");
                    CopyDirectory(errDir, destRoot);
                    MessageBox.Show($"错误日志已导出到: {destRoot}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出错误日志失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureErrorFolderExists()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var errDir = Path.Combine(baseDir, "Error");
            if (!Directory.Exists(errDir)) Directory.CreateDirectory(errDir);
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSub = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSub);
            }
        }

        private void InitializePhotoFormats()
        {
            photoFormats = new Dictionary<string, string[]>
            {
                { "JPEG", new[] { ".jpg", ".jpeg" } },
                { "Nikon RAW", new[] { ".nef", ".nrw" } },
                { "Canon RAW", new[] { ".cr2", ".cr3", ".crw" } },
                { "Sony RAW", new[] { ".arw", ".sr2", ".srf" } },
                { "Fujifilm RAW", new[] { ".raf" } },
                { "Panasonic RAW", new[] { ".rw2", ".raw" } },
                { "Olympus RAW", new[] { ".orf" } },
                { "Pentax RAW", new[] { ".pef", ".ptx" } },
                { "Sigma RAW", new[] { ".x3f" } },
                { "Adobe DNG", new[] { ".dng" } }
            };
        }

        private void InitializeVideoFormats()
        {
            videoFormats = new Dictionary<string, string[]>
            {
                { "MP4", new[] { ".mp4" } },
                { "AVI", new[] { ".avi" } },
                { "MKV", new[] { ".mkv" } },
                { "MOV", new[] { ".mov" } },
                { "WMV", new[] { ".wmv" } },
                { "FLV", new[] { ".flv" } },
                { "TS", new[] { ".ts" } },
                { "M4V", new[] { ".m4v" } },
                { "3GP", new[] { ".3gp" } },
                { "ASF", new[] { ".asf" } }
            };
        }

        private void InitializeControls()
        {
            //读取缓存并恢复
            txtJpegDestPath.Text = Properties.Settings.Default.JpegDestPath;
            txtRawDestPath.Text = Properties.Settings.Default.RawDestPath;
            txtVideoDestPath.Text = Properties.Settings.Default.VideoDestPath;
            // 恢复并行度
            try
            {
                nudParallelism.Value = Math.Max(1, Math.Min(32, Properties.Settings.Default.MaxDegreeOfParallelism));
            }
            catch { nudParallelism.Value = 1; }

            // 初始化RAW格式复选框
            foreach (var format in photoFormats.Keys)
            {
                if (format != "JPEG")
                {
                    int idx = chkListRawFormats.Items.Add(format, true);
                    // 恢复选中项
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.RawFormats))
                    {
                        var saved = Properties.Settings.Default.RawFormats.Split(';');
                        chkListRawFormats.SetItemChecked(idx, saved.Contains(format));
                    }
                }
            }

            // 初始化视频格式复选框（添加新控件 chkListVideoFormats）
            foreach (var format in videoFormats.Keys)
            {
                int idx = chkListVideoFormats.Items.Add(format, true);
                if (!string.IsNullOrEmpty(Properties.Settings.Default.VideoFormats))
                {
                    var saved = Properties.Settings.Default.VideoFormats.Split(';');
                    chkListVideoFormats.SetItemChecked(idx, saved.Contains(format));
                }
            }

            // 初始化重复文件处理选项
            cmbDuplicateAction.Items.Clear();
            cmbDuplicateAction.Items.Add("覆盖现有文件");
            cmbDuplicateAction.Items.Add("跳过现有文件");
            cmbDuplicateAction.Items.Add("重命名新文件");
            cmbDuplicateAction.SelectedIndex =0;

            // 设置工具提示
            toolTip.SetToolTip(txtSourcePath, "选择包含照片的源文件夹");
            toolTip.SetToolTip(txtJpegDestPath, "选择JPEG格式照片的备份目标文件夹");
            toolTip.SetToolTip(txtRawDestPath, "选择RAW格式照片的备份目标文件夹");
            toolTip.SetToolTip(chkListRawFormats, "选择要备份的RAW相机格式");
            toolTip.SetToolTip(txtVideoDestPath, "选择视频备份目标文件夹");
            toolTip.SetToolTip(chkListVideoFormats, "选择要备份的视频格式");

            //绑定更改事件，自动保存
            txtJpegDestPath.TextChanged += (s, e) => Properties.Settings.Default.JpegDestPath = txtJpegDestPath.Text;
            txtRawDestPath.TextChanged += (s, e) => Properties.Settings.Default.RawDestPath = txtRawDestPath.Text;
            txtVideoDestPath.TextChanged += (s, e) => Properties.Settings.Default.VideoDestPath = txtVideoDestPath.Text;
            chkListRawFormats.ItemCheck += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    var selected = new List<string>();
                    foreach (var item in chkListRawFormats.CheckedItems)
                        selected.Add(item.ToString());
                    Properties.Settings.Default.RawFormats = string.Join(";", selected);
                }));
            };
            chkListVideoFormats.ItemCheck += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    var selected = new List<string>();
                    foreach (var item in chkListVideoFormats.CheckedItems)
                        selected.Add(item.ToString());
                    Properties.Settings.Default.VideoFormats = string.Join(";", selected);
                }));
            };
        }

        private void InitializeBackgroundWorker()
        {
            backupWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            backupWorker.DoWork += BackupWorker_DoWork;
            backupWorker.ProgressChanged += BackupWorker_ProgressChanged;
            backupWorker.RunWorkerCompleted += BackupWorker_RunWorkerCompleted;
        }

        private void SaveParallelismSetting()
        {
            try
            {
                Properties.Settings.Default.MaxDegreeOfParallelism = (int)nudParallelism.Value;
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择源文件夹";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnSelectJpegDest_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择照片备份目标文件夹";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtJpegDestPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnSelectRawDest_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择RAW照片备份目标文件夹";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtRawDestPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnSelectVideoDest_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择视频备份目标文件夹";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtVideoDestPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnStartBackup_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            btnStartBackup.Enabled = false;
            lstLog.Items.Clear();
            // lstLog.Items.Add("开始备份..."); // 由 BackupManager 记录，避免重复

            var settings = new BackupSettings
            {
                SourcePath = txtSourcePath.Text,
                JpegDestinationPath = txtJpegDestPath.Text,
                RawDestinationPath = txtRawDestPath.Text,
                VideoDestinationPath = txtVideoDestPath.Text,
                SelectedRawFormats = GetSelectedRawFormats(),
                SelectedVideoFormats = GetSelectedVideoFormats(),
                DuplicateAction = (DuplicateAction)cmbDuplicateAction.SelectedIndex,
                PreserveFolderStructure = true, // 默认保留目录结构，可后续加UI选项
                MaxDegreeOfParallelism = (int)nudParallelism.Value
            };

            // 保存并行度到设置
            SaveParallelismSetting();
            backupWorker.RunWorkerAsync(settings);
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(txtSourcePath.Text))
            {
                MessageBox.Show("请选择源文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtJpegDestPath.Text))
            {
                MessageBox.Show("请选择JPEG目标文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtRawDestPath.Text))
            {
                MessageBox.Show("请选择RAW目标文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtVideoDestPath.Text))
            {
                MessageBox.Show("请选择视频目标文件夹", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!Directory.Exists(txtSourcePath.Text))
            {
                MessageBox.Show("源文件夹不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private List<string> GetSelectedRawFormats()
        {
            var selectedFormats = new List<string>();
            foreach (var item in chkListRawFormats.CheckedItems)
            {
                if (photoFormats.ContainsKey(item.ToString()))
                {
                    selectedFormats.AddRange(photoFormats[item.ToString()]);
                }
            }
            return selectedFormats;
        }

        private List<string> GetSelectedVideoFormats()
        {
            var selectedFormats = new List<string>();
            foreach (var item in chkListVideoFormats.CheckedItems)
            {
                if (videoFormats.ContainsKey(item.ToString()))
                {
                    selectedFormats.AddRange(videoFormats[item.ToString()]);
                }
            }
            return selectedFormats;
        }

        private void BackupWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var settings = (BackupSettings)e.Argument;
            var worker = (BackgroundWorker)sender;

            try
            {
                var backupManager = new BackupManager(worker, settings);
                backupManager.PerformBackup();
            }
            catch (Exception ex)
            {
                // 记录详细错误到 Error 文件夹
                try { ErrorLogger.LogException(ex, settings); } catch { }
                e.Result = ex;
            }
        }

        private void BackupWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // 支持两种 UserState：BackupProgressInfo 或 string
            if (e.UserState is BackupProgressInfo info)
            {
                // 总进度
                progressBar.Value = Math.Max(0, Math.Min(100, info.OverallPercent));
                lblProgress.Text = $"{info.OverallPercent}% - {info.FileName}";

                // 当前文件进度条
                if (this.progressBarFile != null)
                {
                    progressBarFile.Value = Math.Max(0, Math.Min(100, info.FilePercent));
                }

                if (this.lblFileProgress != null)
                {
                    lblFileProgress.Text = $"当前文件: {info.FileName} ({info.FilePercent}%)";
                }

                // 已用时间和 ETA
                var elapsed = TimeSpan.FromSeconds(info.ElapsedSeconds);
                var eta = info.EstimatedRemainingSeconds >= 0 ? TimeSpan.FromSeconds(info.EstimatedRemainingSeconds) : (TimeSpan?)null;
                if (this.lblElapsed != null)
                    lblElapsed.Text = $"已用时间：{elapsed:hh\\:mm\\:ss}";
                if (this.lblETA != null)
                    lblETA.Text = eta.HasValue ? $"预计剩余：{eta.Value:hh\\:mm\\:ss}" : "预计剩余：未知";

                // 不在每个块上写日志，避免日志冗余
            }
            else if (e.UserState is string msg)
            {
                progressBar.Value = Math.Max(0, Math.Min(100, e.ProgressPercentage));
                lblProgress.Text = $"{e.ProgressPercentage}% - {msg}";
                lstLog.Items.Add(msg);
                lstLog.SelectedIndex = lstLog.Items.Count - 1;
                lstLog.ClearSelected();
            }
            else
            {
                // fallback
                progressBar.Value = Math.Max(0, Math.Min(100, e.ProgressPercentage));
                lblProgress.Text = $"{e.ProgressPercentage}%";
            }
        }

        private void BackupWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStartBackup.Enabled = true;

            if (e.Error != null || e.Result is Exception)
            {
                var exception = e.Error ?? (Exception)e.Result;
                lstLog.Items.Add($"备份失败: {exception.Message}");
                MessageBox.Show($"备份过程中发生错误: {exception.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // BackupManager 已记录完成信息，避免重复
                MessageBox.Show("备份完成！", "完成",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            progressBar.Value = 0;
            lblProgress.Text = "准备就绪";
            if (this.progressBarFile != null) progressBarFile.Value = 0;
            if (this.lblFileProgress != null) lblFileProgress.Text = "当前文件: -";
            if (this.lblElapsed != null) lblElapsed.Text = "已用时间：00:00:00";
            if (this.lblETA != null) lblETA.Text = "预计剩余：00:00:00";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
            base.OnFormClosing(e);
        }
    }

    public enum DuplicateAction
    {
        Overwrite,
        Skip,
        Rename
    }

    public class BackupSettings
    {
        public string SourcePath { get; set; }
        public string JpegDestinationPath { get; set; }
        public string RawDestinationPath { get; set; }
        public string VideoDestinationPath { get; set; }
        public List<string> SelectedRawFormats { get; set; }
        public List<string> SelectedVideoFormats { get; set; }
        public DuplicateAction DuplicateAction { get; set; }
        public bool PreserveFolderStructure { get; set; } = true; // 新增，默认保留目录结构
        public int MaxDegreeOfParallelism { get; set; } = 1; // 新增，并行度，默认串行
    }

    // BackupProgressInfo 已在 BackupManager.cs 定义，避免重复定义移除此处类型
}
