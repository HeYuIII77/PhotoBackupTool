using System.Drawing;
using System.Windows.Forms;

namespace PhotoBackupTool
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnSelectSource = new System.Windows.Forms.Button();
            this.btnSelectJpegDest = new System.Windows.Forms.Button();
            this.btnSelectRawDest = new System.Windows.Forms.Button();
            this.btnSelectVideoDest = new System.Windows.Forms.Button();
            this.btnStartBackup = new System.Windows.Forms.Button();
            this.txtSourcePath = new System.Windows.Forms.TextBox();
            this.txtJpegDestPath = new System.Windows.Forms.TextBox();
            this.txtRawDestPath = new System.Windows.Forms.TextBox();
            this.txtVideoDestPath = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblJpegDest = new System.Windows.Forms.Label();
            this.lblRawDest = new System.Windows.Forms.Label();
            this.lblVideoDest = new System.Windows.Forms.Label();
            this.cmbDuplicateAction = new System.Windows.Forms.ComboBox();
            this.lblDuplicateAction = new System.Windows.Forms.Label();
            this.chkListRawFormats = new System.Windows.Forms.CheckedListBox();
            this.chkListVideoFormats = new System.Windows.Forms.CheckedListBox();
            this.lblRawFormats = new System.Windows.Forms.Label();
            this.lblVideoFormats = new System.Windows.Forms.Label();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.lblParallelHelp = new System.Windows.Forms.Label();
            this.progressBarFile = new System.Windows.Forms.ProgressBar();
            this.lblFileProgress = new System.Windows.Forms.Label();
            this.lblElapsed = new System.Windows.Forms.Label();
            this.lblETA = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.grpFolders = new System.Windows.Forms.GroupBox();
            this.grpFormatSelection = new System.Windows.Forms.GroupBox();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.lblParallelism = new System.Windows.Forms.Label();
            this.cmbParallelism = new System.Windows.Forms.ComboBox();
            this.btnOpenErrorFolder = new System.Windows.Forms.Button();
            this.btnExportLog = new System.Windows.Forms.Button();
            this.grpProgress = new System.Windows.Forms.GroupBox();
            this.lblTotalProgress = new System.Windows.Forms.Label();
            this.grpFolders.SuspendLayout();
            this.grpFormatSelection.SuspendLayout();
            this.grpSettings.SuspendLayout();
            this.grpProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSelectSource
            // 
            this.btnSelectSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnSelectSource.Location = new System.Drawing.Point(748, 22);
            this.btnSelectSource.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectSource.Name = "btnSelectSource";
            this.btnSelectSource.Size = new System.Drawing.Size(106, 28);
            this.btnSelectSource.TabIndex = 2;
            this.btnSelectSource.Text = "浏览...";
            this.btnSelectSource.UseVisualStyleBackColor = true;
            this.btnSelectSource.Click += new System.EventHandler(this.btnSelectSource_Click);
            // 
            // btnSelectJpegDest
            // 
            this.btnSelectJpegDest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnSelectJpegDest.Location = new System.Drawing.Point(748, 57);
            this.btnSelectJpegDest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectJpegDest.Name = "btnSelectJpegDest";
            this.btnSelectJpegDest.Size = new System.Drawing.Size(106, 28);
            this.btnSelectJpegDest.TabIndex = 4;
            this.btnSelectJpegDest.Text = "浏览...";
            this.btnSelectJpegDest.UseVisualStyleBackColor = true;
            this.btnSelectJpegDest.Click += new System.EventHandler(this.btnSelectJpegDest_Click);
            // 
            // btnSelectRawDest
            // 
            this.btnSelectRawDest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnSelectRawDest.Location = new System.Drawing.Point(748, 92);
            this.btnSelectRawDest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectRawDest.Name = "btnSelectRawDest";
            this.btnSelectRawDest.Size = new System.Drawing.Size(106, 28);
            this.btnSelectRawDest.TabIndex = 6;
            this.btnSelectRawDest.Text = "浏览...";
            this.btnSelectRawDest.UseVisualStyleBackColor = true;
            this.btnSelectRawDest.Click += new System.EventHandler(this.btnSelectRawDest_Click);
            // 
            // btnSelectVideoDest
            // 
            this.btnSelectVideoDest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnSelectVideoDest.Location = new System.Drawing.Point(748, 127);
            this.btnSelectVideoDest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectVideoDest.Name = "btnSelectVideoDest";
            this.btnSelectVideoDest.Size = new System.Drawing.Size(106, 28);
            this.btnSelectVideoDest.TabIndex = 8;
            this.btnSelectVideoDest.Text = "浏览...";
            this.btnSelectVideoDest.UseVisualStyleBackColor = true;
            this.btnSelectVideoDest.Click += new System.EventHandler(this.btnSelectVideoDest_Click);
            // 
            // btnStartBackup
            // 
            this.btnStartBackup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnStartBackup.FlatAppearance.BorderSize = 0;
            this.btnStartBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartBackup.ForeColor = System.Drawing.Color.White;
            this.btnStartBackup.Location = new System.Drawing.Point(608, 533);
            this.btnStartBackup.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnStartBackup.Name = "btnStartBackup";
            this.btnStartBackup.Size = new System.Drawing.Size(280, 50);
            this.btnStartBackup.TabIndex = 10;
            this.btnStartBackup.Text = "开始备份";
            this.btnStartBackup.UseVisualStyleBackColor = false;
            this.btnStartBackup.Click += new System.EventHandler(this.btnStartBackup_Click);
            // 
            // txtSourcePath
            // 
            this.txtSourcePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtSourcePath.Location = new System.Drawing.Point(127, 25);
            this.txtSourcePath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.ReadOnly = true;
            this.txtSourcePath.Size = new System.Drawing.Size(615, 23);
            this.txtSourcePath.TabIndex = 1;
            // 
            // txtJpegDestPath
            // 
            this.txtJpegDestPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtJpegDestPath.Location = new System.Drawing.Point(127, 60);
            this.txtJpegDestPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtJpegDestPath.Name = "txtJpegDestPath";
            this.txtJpegDestPath.ReadOnly = true;
            this.txtJpegDestPath.Size = new System.Drawing.Size(615, 23);
            this.txtJpegDestPath.TabIndex = 3;
            // 
            // txtRawDestPath
            // 
            this.txtRawDestPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtRawDestPath.Location = new System.Drawing.Point(127, 95);
            this.txtRawDestPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtRawDestPath.Name = "txtRawDestPath";
            this.txtRawDestPath.ReadOnly = true;
            this.txtRawDestPath.Size = new System.Drawing.Size(615, 23);
            this.txtRawDestPath.TabIndex = 5;
            // 
            // txtVideoDestPath
            // 
            this.txtVideoDestPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtVideoDestPath.Location = new System.Drawing.Point(127, 130);
            this.txtVideoDestPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtVideoDestPath.Name = "txtVideoDestPath";
            this.txtVideoDestPath.ReadOnly = true;
            this.txtVideoDestPath.Size = new System.Drawing.Size(615, 23);
            this.txtVideoDestPath.TabIndex = 7;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(15, 51);
            this.progressBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(549, 25);
            this.progressBar.TabIndex = 9;
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSource.Location = new System.Drawing.Point(15, 30);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(106, 17);
            this.lblSource.TabIndex = 17;
            this.lblSource.Text = "源文件夹路径：";
            // 
            // lblJpegDest
            // 
            this.lblJpegDest.AutoSize = true;
            this.lblJpegDest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblJpegDest.Location = new System.Drawing.Point(15, 65);
            this.lblJpegDest.Name = "lblJpegDest";
            this.lblJpegDest.Size = new System.Drawing.Size(106, 17);
            this.lblJpegDest.TabIndex = 16;
            this.lblJpegDest.Text = "照片目标路径：";
            // 
            // lblRawDest
            // 
            this.lblRawDest.AutoSize = true;
            this.lblRawDest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblRawDest.Location = new System.Drawing.Point(15, 100);
            this.lblRawDest.Name = "lblRawDest";
            this.lblRawDest.Size = new System.Drawing.Size(110, 17);
            this.lblRawDest.TabIndex = 15;
            this.lblRawDest.Text = "RAW目标路径：";
            // 
            // lblVideoDest
            // 
            this.lblVideoDest.AutoSize = true;
            this.lblVideoDest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblVideoDest.Location = new System.Drawing.Point(15, 135);
            this.lblVideoDest.Name = "lblVideoDest";
            this.lblVideoDest.Size = new System.Drawing.Size(106, 17);
            this.lblVideoDest.TabIndex = 14;
            this.lblVideoDest.Text = "视频目标路径：";
            // 
            // cmbDuplicateAction
            // 
            this.cmbDuplicateAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDuplicateAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.cmbDuplicateAction.FormattingEnabled = true;
            this.cmbDuplicateAction.Items.AddRange(new object[] {
            "覆盖现有文件",
            "跳过现有文件",
            "重命名新文件"});
            this.cmbDuplicateAction.Location = new System.Drawing.Point(15, 52);
            this.cmbDuplicateAction.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbDuplicateAction.Name = "cmbDuplicateAction";
            this.cmbDuplicateAction.Size = new System.Drawing.Size(250, 24);
            this.cmbDuplicateAction.TabIndex = 8;
            // 
            // lblDuplicateAction
            // 
            this.lblDuplicateAction.AutoSize = true;
            this.lblDuplicateAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblDuplicateAction.Location = new System.Drawing.Point(12, 31);
            this.lblDuplicateAction.Name = "lblDuplicateAction";
            this.lblDuplicateAction.Size = new System.Drawing.Size(113, 17);
            this.lblDuplicateAction.TabIndex = 13;
            this.lblDuplicateAction.Text = "重复文件处理：";
            // 
            // chkListRawFormats
            // 
            this.chkListRawFormats.CheckOnClick = true;
            this.chkListRawFormats.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.chkListRawFormats.FormattingEnabled = true;
            this.chkListRawFormats.Location = new System.Drawing.Point(311, 60);
            this.chkListRawFormats.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkListRawFormats.Name = "chkListRawFormats";
            this.chkListRawFormats.Size = new System.Drawing.Size(250, 184);
            this.chkListRawFormats.TabIndex = 7;
            // 
            // chkListVideoFormats
            // 
            this.chkListVideoFormats.CheckOnClick = true;
            this.chkListVideoFormats.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.chkListVideoFormats.FormattingEnabled = true;
            this.chkListVideoFormats.Location = new System.Drawing.Point(21, 61);
            this.chkListVideoFormats.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkListVideoFormats.Name = "chkListVideoFormats";
            this.chkListVideoFormats.Size = new System.Drawing.Size(250, 184);
            this.chkListVideoFormats.TabIndex = 7;
            // 
            // lblRawFormats
            // 
            this.lblRawFormats.AutoSize = true;
            this.lblRawFormats.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblRawFormats.Location = new System.Drawing.Point(308, 33);
            this.lblRawFormats.Name = "lblRawFormats";
            this.lblRawFormats.Size = new System.Drawing.Size(88, 17);
            this.lblRawFormats.TabIndex = 14;
            this.lblRawFormats.Text = "RAW格式：";
            // 
            // lblVideoFormats
            // 
            this.lblVideoFormats.AutoSize = true;
            this.lblVideoFormats.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblVideoFormats.Location = new System.Drawing.Point(21, 29);
            this.lblVideoFormats.Name = "lblVideoFormats";
            this.lblVideoFormats.Size = new System.Drawing.Size(83, 17);
            this.lblVideoFormats.TabIndex = 15;
            this.lblVideoFormats.Text = "视频格式：";
            // 
            // lstLog
            // 
            this.lstLog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.ItemHeight = 15;
            this.lstLog.Location = new System.Drawing.Point(15, 137);
            this.lstLog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(250, 139);
            this.lstLog.TabIndex = 11;
            // 
            // lblParallelHelp
            // 
            this.lblParallelHelp.AutoSize = true;
            this.lblParallelHelp.BackColor = System.Drawing.Color.LightGray;
            this.lblParallelHelp.Cursor = System.Windows.Forms.Cursors.Help;
            this.lblParallelHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblParallelHelp.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblParallelHelp.Location = new System.Drawing.Point(248, 95);
            this.lblParallelHelp.Name = "lblParallelHelp";
            this.lblParallelHelp.Size = new System.Drawing.Size(17, 17);
            this.lblParallelHelp.TabIndex = 20;
            this.lblParallelHelp.Text = "?";
            this.toolTip.SetToolTip(this.lblParallelHelp, "• 单个机械硬盘：1（串行）或 2–3，避免寻道抖动。\n• 单个 NVMe/SSD 或多盘并行写入：4–16 视情况提高吞吐。\n• 目标为网络存储（NAS/SMB" +
        "）：一般 4–8，受网络带宽与 NAS 并发能力影响。\n• 如果目标多个磁盘/目标路径，适当提高并行度能更明显提升性能；如果都是同一慢盘，过高会反而变慢。");
            // 
            // progressBarFile
            // 
            this.progressBarFile.Location = new System.Drawing.Point(0, 0);
            this.progressBarFile.Name = "progressBarFile";
            this.progressBarFile.Size = new System.Drawing.Size(100, 23);
            this.progressBarFile.TabIndex = 0;
            // 
            // lblFileProgress
            // 
            this.lblFileProgress.Location = new System.Drawing.Point(0, 0);
            this.lblFileProgress.Name = "lblFileProgress";
            this.lblFileProgress.Size = new System.Drawing.Size(100, 23);
            this.lblFileProgress.TabIndex = 0;
            // 
            // lblElapsed
            // 
            this.lblElapsed.AutoSize = true;
            this.lblElapsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblElapsed.Location = new System.Drawing.Point(15, 80);
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new System.Drawing.Size(115, 15);
            this.lblElapsed.TabIndex = 22;
            this.lblElapsed.Text = "已用时间：00:00:00";
            // 
            // lblETA
            // 
            this.lblETA.AutoSize = true;
            this.lblETA.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblETA.Location = new System.Drawing.Point(200, 80);
            this.lblETA.Name = "lblETA";
            this.lblETA.Size = new System.Drawing.Size(115, 15);
            this.lblETA.TabIndex = 23;
            this.lblETA.Text = "预计剩余：00:00:00";
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblProgress.Location = new System.Drawing.Point(15, 30);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(83, 17);
            this.lblProgress.TabIndex = 12;
            this.lblProgress.Text = "总体进度：";
            // 
            // grpFolders
            // 
            this.grpFolders.Controls.Add(this.lblSource);
            this.grpFolders.Controls.Add(this.txtSourcePath);
            this.grpFolders.Controls.Add(this.btnSelectSource);
            this.grpFolders.Controls.Add(this.lblJpegDest);
            this.grpFolders.Controls.Add(this.txtJpegDestPath);
            this.grpFolders.Controls.Add(this.btnSelectJpegDest);
            this.grpFolders.Controls.Add(this.lblRawDest);
            this.grpFolders.Controls.Add(this.txtRawDestPath);
            this.grpFolders.Controls.Add(this.btnSelectRawDest);
            this.grpFolders.Controls.Add(this.lblVideoDest);
            this.grpFolders.Controls.Add(this.txtVideoDestPath);
            this.grpFolders.Controls.Add(this.btnSelectVideoDest);
            this.grpFolders.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.grpFolders.Location = new System.Drawing.Point(20, 20);
            this.grpFolders.Name = "grpFolders";
            this.grpFolders.Size = new System.Drawing.Size(868, 170);
            this.grpFolders.TabIndex = 24;
            this.grpFolders.TabStop = false;
            this.grpFolders.Text = "文件夹设置";
            // 
            // grpFormatSelection
            // 
            this.grpFormatSelection.Controls.Add(this.lblRawFormats);
            this.grpFormatSelection.Controls.Add(this.chkListRawFormats);
            this.grpFormatSelection.Controls.Add(this.lblVideoFormats);
            this.grpFormatSelection.Controls.Add(this.chkListVideoFormats);
            this.grpFormatSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.grpFormatSelection.Location = new System.Drawing.Point(20, 200);
            this.grpFormatSelection.Name = "grpFormatSelection";
            this.grpFormatSelection.Size = new System.Drawing.Size(580, 262);
            this.grpFormatSelection.TabIndex = 25;
            this.grpFormatSelection.TabStop = false;
            this.grpFormatSelection.Text = "文件格式选择";
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.lblParallelHelp);
            this.grpSettings.Controls.Add(this.lblParallelism);
            this.grpSettings.Controls.Add(this.cmbParallelism);
            this.grpSettings.Controls.Add(this.btnOpenErrorFolder);
            this.grpSettings.Controls.Add(this.btnExportLog);
            this.grpSettings.Controls.Add(this.lblDuplicateAction);
            this.grpSettings.Controls.Add(this.cmbDuplicateAction);
            this.grpSettings.Controls.Add(this.lstLog);
            this.grpSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.grpSettings.Location = new System.Drawing.Point(608, 200);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(280, 326);
            this.grpSettings.TabIndex = 26;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "备份设置与日志";
            // 
            // lblParallelism
            // 
            this.lblParallelism.AutoSize = true;
            this.lblParallelism.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblParallelism.Location = new System.Drawing.Point(15, 95);
            this.lblParallelism.Name = "lblParallelism";
            this.lblParallelism.Size = new System.Drawing.Size(120, 17);
            this.lblParallelism.TabIndex = 18;
            this.lblParallelism.Text = "并行度 (线程数)：";
            // 
            // cmbParallelism
            // 
            this.cmbParallelism.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbParallelism.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.cmbParallelism.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16"});
            this.cmbParallelism.Location = new System.Drawing.Point(145, 93);
            this.cmbParallelism.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbParallelism.Name = "cmbParallelism";
            this.cmbParallelism.Size = new System.Drawing.Size(92, 24);
            this.cmbParallelism.TabIndex = 19;
            // 
            // btnOpenErrorFolder
            // 
            this.btnOpenErrorFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnOpenErrorFolder.Location = new System.Drawing.Point(14, 283);
            this.btnOpenErrorFolder.Name = "btnOpenErrorFolder";
            this.btnOpenErrorFolder.Size = new System.Drawing.Size(120, 28);
            this.btnOpenErrorFolder.TabIndex = 21;
            this.btnOpenErrorFolder.Text = "查看异常信息文件";
            this.btnOpenErrorFolder.UseVisualStyleBackColor = true;
            this.btnOpenErrorFolder.Click += new System.EventHandler(this.btnOpenErrorFolder_Click);
            // 
            // btnExportLog
            // 
            this.btnExportLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnExportLog.Location = new System.Drawing.Point(145, 283);
            this.btnExportLog.Name = "btnExportLog";
            this.btnExportLog.Size = new System.Drawing.Size(120, 28);
            this.btnExportLog.TabIndex = 22;
            this.btnExportLog.Text = "导出异常信息";
            this.btnExportLog.UseVisualStyleBackColor = true;
            this.btnExportLog.Click += new System.EventHandler(this.btnExportLog_Click);
            // 
            // grpProgress
            // 
            this.grpProgress.Controls.Add(this.lblTotalProgress);
            this.grpProgress.Controls.Add(this.lblProgress);
            this.grpProgress.Controls.Add(this.progressBar);
            this.grpProgress.Controls.Add(this.lblETA);
            this.grpProgress.Controls.Add(this.lblElapsed);
            this.grpProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.grpProgress.Location = new System.Drawing.Point(20, 468);
            this.grpProgress.Name = "grpProgress";
            this.grpProgress.Size = new System.Drawing.Size(580, 115);
            this.grpProgress.TabIndex = 27;
            this.grpProgress.TabStop = false;
            this.grpProgress.Text = "进度信息";
            // 
            // lblTotalProgress
            // 
            this.lblTotalProgress.AutoSize = true;
            this.lblTotalProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalProgress.Location = new System.Drawing.Point(409, 80);
            this.lblTotalProgress.Name = "lblTotalProgress";
            this.lblTotalProgress.Size = new System.Drawing.Size(51, 15);
            this.lblTotalProgress.TabIndex = 24;
            this.lblTotalProgress.Text = "0/0 文件";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 599);
            this.Controls.Add(this.grpProgress);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.grpFormatSelection);
            this.Controls.Add(this.grpFolders);
            this.Controls.Add(this.btnStartBackup);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "照片与视频备份工具";
            this.grpFolders.ResumeLayout(false);
            this.grpFolders.PerformLayout();
            this.grpFormatSelection.ResumeLayout(false);
            this.grpFormatSelection.PerformLayout();
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.grpProgress.ResumeLayout(false);
            this.grpProgress.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSelectSource;
        private System.Windows.Forms.Button btnSelectJpegDest;
        private System.Windows.Forms.Button btnSelectRawDest;
        private System.Windows.Forms.Button btnSelectVideoDest;
        private System.Windows.Forms.Button btnStartBackup;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.TextBox txtJpegDestPath;
        private System.Windows.Forms.TextBox txtRawDestPath;
        private System.Windows.Forms.TextBox txtVideoDestPath;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblJpegDest;
        private System.Windows.Forms.Label lblRawDest;
        private System.Windows.Forms.Label lblVideoDest;
        private System.Windows.Forms.ComboBox cmbDuplicateAction;
        private System.Windows.Forms.Label lblDuplicateAction;
        private System.Windows.Forms.CheckedListBox chkListRawFormats;
        private System.Windows.Forms.CheckedListBox chkListVideoFormats;
        private System.Windows.Forms.Label lblRawFormats;
        private System.Windows.Forms.Label lblVideoFormats;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ProgressBar progressBarFile;
        private System.Windows.Forms.Label lblFileProgress;
        private System.Windows.Forms.Label lblElapsed;
        private System.Windows.Forms.Label lblETA;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.GroupBox grpFolders;
        private System.Windows.Forms.GroupBox grpFormatSelection;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.GroupBox grpProgress;
        private System.Windows.Forms.Label lblTotalProgress;
        private System.Windows.Forms.ComboBox cmbParallelism;
        private System.Windows.Forms.Label lblParallelism;
        private System.Windows.Forms.Label lblParallelHelp;
        private System.Windows.Forms.Button btnOpenErrorFolder;
        private System.Windows.Forms.Button btnExportLog;
    }
}