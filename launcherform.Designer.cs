namespace LEHuDModLauncher
{
    partial class Launcherform
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcherform));
            panel1 = new Panel();
            buttonAttachLog = new Button();
            radioGamepad = new RadioButton();
            radioKb = new RadioButton();
            checkBoxStartUpMessage = new CheckBox();
            checkBoxKeepOpen = new CheckBox();
            textGameVersion = new TextBox();
            buttonGetGameFolder = new Button();
            buttonOffline = new Button();
            label3 = new Label();
            textGamePath = new TextBox();
            buttonOnline = new Button();
            buttonBrowseGameFolder = new Button();
            statusStrip = new StatusStrip();
            toolStripProgress = new ToolStripProgressBar();
            statusStripLabel = new ToolStripStatusLabel();
            toolStripStatus = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            installToolStripMenuItem = new ToolStripMenuItem();
            changelogToolStripMenuItem = new ToolStripMenuItem();
            checkForNewVersionToolStripMenuItem = new ToolStripMenuItem();
            toolStripTheme = new ToolStripMenuItem();
            checkBoxHideConsole = new CheckBox();
            panel2 = new Panel();
            buttonStartupMessage = new Button();
            statusStrip.SuspendLayout();
            menuStrip1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.None;
            panel1.Location = new Point(408, -474);
            panel1.Margin = new Padding(2);
            panel1.Name = "panel1";
            panel1.Size = new Size(127, 73);
            panel1.TabIndex = 6;
            // 
            // buttonAttachLog
            // 
            buttonAttachLog.Anchor = AnchorStyles.None;
            buttonAttachLog.Location = new Point(28, 153);
            buttonAttachLog.Margin = new Padding(2);
            buttonAttachLog.Name = "buttonAttachLog";
            buttonAttachLog.Size = new Size(186, 28);
            buttonAttachLog.TabIndex = 19;
            buttonAttachLog.Text = "Attach log console window";
            buttonAttachLog.UseVisualStyleBackColor = true;
            buttonAttachLog.Click += buttonAttachLog_Click;
            // 
            // radioGamepad
            // 
            radioGamepad.Anchor = AnchorStyles.None;
            radioGamepad.AutoSize = true;
            radioGamepad.Location = new Point(260, 114);
            radioGamepad.Margin = new Padding(2);
            radioGamepad.Name = "radioGamepad";
            radioGamepad.Size = new Size(76, 19);
            radioGamepad.TabIndex = 17;
            radioGamepad.Text = "Gamepad";
            radioGamepad.UseVisualStyleBackColor = true;
            radioGamepad.CheckedChanged += radioGamepad_CheckedChanged;
            // 
            // radioKb
            // 
            radioKb.Anchor = AnchorStyles.None;
            radioKb.AutoSize = true;
            radioKb.Checked = true;
            radioKb.Location = new Point(260, 90);
            radioKb.Margin = new Padding(2);
            radioKb.Name = "radioKb";
            radioKb.Size = new Size(116, 19);
            radioKb.TabIndex = 16;
            radioKb.TabStop = true;
            radioKb.Text = "Keyboard/mouse";
            radioKb.UseVisualStyleBackColor = true;
            radioKb.CheckedChanged += radioKb_CheckedChanged;
            // 
            // checkBoxStartUpMessage
            // 
            checkBoxStartUpMessage.AccessibleName = "checkBoxKeepOpen";
            checkBoxStartUpMessage.Anchor = AnchorStyles.None;
            checkBoxStartUpMessage.AutoSize = true;
            checkBoxStartUpMessage.Checked = true;
            checkBoxStartUpMessage.CheckState = CheckState.Checked;
            checkBoxStartUpMessage.Location = new Point(28, 115);
            checkBoxStartUpMessage.Margin = new Padding(2);
            checkBoxStartUpMessage.Name = "checkBoxStartUpMessage";
            checkBoxStartUpMessage.Size = new Size(150, 19);
            checkBoxStartUpMessage.TabIndex = 15;
            checkBoxStartUpMessage.Text = "Show mod info on start";
            checkBoxStartUpMessage.UseVisualStyleBackColor = true;
            checkBoxStartUpMessage.CheckedChanged += checkBoxStartUpMessage_CheckedChanged;
            // 
            // checkBoxKeepOpen
            // 
            checkBoxKeepOpen.AccessibleName = "checkBoxKeepOpen";
            checkBoxKeepOpen.Anchor = AnchorStyles.None;
            checkBoxKeepOpen.AutoSize = true;
            checkBoxKeepOpen.Checked = true;
            checkBoxKeepOpen.CheckState = CheckState.Checked;
            checkBoxKeepOpen.Location = new Point(28, 91);
            checkBoxKeepOpen.Margin = new Padding(2);
            checkBoxKeepOpen.Name = "checkBoxKeepOpen";
            checkBoxKeepOpen.Size = new Size(175, 19);
            checkBoxKeepOpen.TabIndex = 13;
            checkBoxKeepOpen.Text = "Keep open on Game Launch";
            checkBoxKeepOpen.UseVisualStyleBackColor = true;
            checkBoxKeepOpen.CheckedChanged += checkBoxKeepOpen_CheckedChanged;
            // 
            // textGameVersion
            // 
            textGameVersion.Anchor = AnchorStyles.None;
            textGameVersion.CausesValidation = false;
            textGameVersion.Location = new Point(260, 56);
            textGameVersion.Margin = new Padding(2);
            textGameVersion.Name = "textGameVersion";
            textGameVersion.ReadOnly = true;
            textGameVersion.ShortcutsEnabled = false;
            textGameVersion.Size = new Size(154, 23);
            textGameVersion.TabIndex = 11;
            textGameVersion.TextAlign = HorizontalAlignment.Center;
            // 
            // buttonGetGameFolder
            // 
            buttonGetGameFolder.Anchor = AnchorStyles.None;
            buttonGetGameFolder.Location = new Point(28, 222);
            buttonGetGameFolder.Margin = new Padding(2);
            buttonGetGameFolder.Name = "buttonGetGameFolder";
            buttonGetGameFolder.Size = new Size(123, 28);
            buttonGetGameFolder.TabIndex = 11;
            buttonGetGameFolder.Text = "Get GameFolder";
            buttonGetGameFolder.UseVisualStyleBackColor = true;
            buttonGetGameFolder.Click += buttonGetGameFolder_Click;
            // 
            // buttonOffline
            // 
            buttonOffline.Anchor = AnchorStyles.None;
            buttonOffline.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            buttonOffline.ForeColor = SystemColors.WindowText;
            buttonOffline.Location = new Point(427, 25);
            buttonOffline.Margin = new Padding(2);
            buttonOffline.Name = "buttonOffline";
            buttonOffline.Size = new Size(211, 54);
            buttonOffline.TabIndex = 1;
            buttonOffline.Text = "Start game OFFLINE";
            buttonOffline.UseVisualStyleBackColor = true;
            buttonOffline.Click += buttonOffline_Click;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.None;
            label3.AutoSize = true;
            label3.Location = new Point(260, 34);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(131, 15);
            label3.TabIndex = 10;
            label3.Text = "Game Version detected:";
            // 
            // textGamePath
            // 
            textGamePath.Anchor = AnchorStyles.None;
            textGamePath.Location = new Point(28, 265);
            textGamePath.Margin = new Padding(2);
            textGamePath.Name = "textGamePath";
            textGamePath.Size = new Size(469, 23);
            textGamePath.TabIndex = 1;
            textGamePath.TextChanged += textPath_TextChanged;
            // 
            // buttonOnline
            // 
            buttonOnline.Anchor = AnchorStyles.None;
            buttonOnline.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            buttonOnline.ForeColor = SystemColors.WindowText;
            buttonOnline.Location = new Point(28, 25);
            buttonOnline.Margin = new Padding(2);
            buttonOnline.Name = "buttonOnline";
            buttonOnline.Size = new Size(213, 54);
            buttonOnline.TabIndex = 0;
            buttonOnline.Text = "Start game ONLINE (no mod)";
            buttonOnline.UseVisualStyleBackColor = true;
            buttonOnline.Click += buttonOnline_Click;
            // 
            // buttonBrowseGameFolder
            // 
            buttonBrowseGameFolder.Anchor = AnchorStyles.None;
            buttonBrowseGameFolder.FlatAppearance.BorderSize = 0;
            buttonBrowseGameFolder.FlatStyle = FlatStyle.Flat;
            buttonBrowseGameFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonBrowseGameFolder.Location = new Point(508, 256);
            buttonBrowseGameFolder.Margin = new Padding(2);
            buttonBrowseGameFolder.Name = "buttonBrowseGameFolder";
            buttonBrowseGameFolder.Size = new Size(36, 32);
            buttonBrowseGameFolder.TabIndex = 3;
            buttonBrowseGameFolder.Text = ". . .";
            buttonBrowseGameFolder.UseVisualStyleBackColor = true;
            buttonBrowseGameFolder.Click += buttonBrowse_Click;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripProgress, statusStripLabel, toolStripStatus });
            statusStrip.Location = new Point(0, 347);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new Padding(1, 0, 10, 0);
            statusStrip.Size = new Size(662, 23);
            statusStrip.TabIndex = 4;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripProgress
            // 
            toolStripProgress.Name = "toolStripProgress";
            toolStripProgress.Size = new Size(70, 17);
            // 
            // statusStripLabel
            // 
            statusStripLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            statusStripLabel.Name = "statusStripLabel";
            statusStripLabel.Size = new Size(0, 18);
            // 
            // toolStripStatus
            // 
            toolStripStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            toolStripStatus.Name = "toolStripStatus";
            toolStripStatus.Size = new Size(0, 18);
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { installToolStripMenuItem, changelogToolStripMenuItem, checkForNewVersionToolStripMenuItem, toolStripTheme });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(4, 1, 0, 1);
            menuStrip1.Size = new Size(662, 25);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // installToolStripMenuItem
            // 
            installToolStripMenuItem.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            installToolStripMenuItem.ForeColor = Color.FromArgb(192, 64, 0);
            installToolStripMenuItem.Name = "installToolStripMenuItem";
            installToolStripMenuItem.Size = new Size(122, 23);
            installToolStripMenuItem.Text = "Install/Reinstall";
            installToolStripMenuItem.ToolTipText = "Install the modloader and mod";
            installToolStripMenuItem.Click += installToolStripMenuItem_Click;
            // 
            // changelogToolStripMenuItem
            // 
            changelogToolStripMenuItem.Enabled = false;
            changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            changelogToolStripMenuItem.Size = new Size(77, 23);
            changelogToolStripMenuItem.Text = "Changelog";
            changelogToolStripMenuItem.Visible = false;
            // 
            // checkForNewVersionToolStripMenuItem
            // 
            checkForNewVersionToolStripMenuItem.Enabled = false;
            checkForNewVersionToolStripMenuItem.Name = "checkForNewVersionToolStripMenuItem";
            checkForNewVersionToolStripMenuItem.Size = new Size(136, 23);
            checkForNewVersionToolStripMenuItem.Text = "Check for new version";
            checkForNewVersionToolStripMenuItem.Visible = false;
            // 
            // toolStripTheme
            // 
            toolStripTheme.Alignment = ToolStripItemAlignment.Right;
            toolStripTheme.Name = "toolStripTheme";
            toolStripTheme.Size = new Size(56, 23);
            toolStripTheme.Text = "Theme";
            toolStripTheme.ToolTipText = "Switch between Dark and Light mode";
            toolStripTheme.Click += toolStripMenuItem1_Click;
            // 
            // checkBoxHideConsole
            // 
            checkBoxHideConsole.Anchor = AnchorStyles.None;
            checkBoxHideConsole.AutoSize = true;
            checkBoxHideConsole.Location = new Point(28, 185);
            checkBoxHideConsole.Margin = new Padding(2);
            checkBoxHideConsole.Name = "checkBoxHideConsole";
            checkBoxHideConsole.Size = new Size(204, 19);
            checkBoxHideConsole.TabIndex = 20;
            checkBoxHideConsole.Text = "Hide mod loader console window";
            checkBoxHideConsole.UseVisualStyleBackColor = true;
            checkBoxHideConsole.CheckedChanged += checkBoxHideConsole_CheckedChanged;
            // 
            // panel2
            // 
            panel2.Controls.Add(buttonStartupMessage);
            panel2.Controls.Add(buttonBrowseGameFolder);
            panel2.Controls.Add(buttonOnline);
            panel2.Controls.Add(checkBoxHideConsole);
            panel2.Controls.Add(buttonOffline);
            panel2.Controls.Add(label3);
            panel2.Controls.Add(checkBoxStartUpMessage);
            panel2.Controls.Add(buttonGetGameFolder);
            panel2.Controls.Add(radioKb);
            panel2.Controls.Add(buttonAttachLog);
            panel2.Controls.Add(textGamePath);
            panel2.Controls.Add(checkBoxKeepOpen);
            panel2.Controls.Add(textGameVersion);
            panel2.Controls.Add(radioGamepad);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 25);
            panel2.Margin = new Padding(2);
            panel2.Name = "panel2";
            panel2.Size = new Size(662, 322);
            panel2.TabIndex = 22;
            // 
            // buttonStartupMessage
            // 
            buttonStartupMessage.Anchor = AnchorStyles.None;
            buttonStartupMessage.Location = new Point(260, 153);
            buttonStartupMessage.Margin = new Padding(2);
            buttonStartupMessage.Name = "buttonStartupMessage";
            buttonStartupMessage.Size = new Size(154, 28);
            buttonStartupMessage.TabIndex = 21;
            buttonStartupMessage.Text = "Show latest mod info";
            buttonStartupMessage.UseVisualStyleBackColor = true;
            buttonStartupMessage.Click += buttonStartupMessage_Click;
            // 
            // Launcherform
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(662, 370);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(2);
            MaximizeBox = false;
            MaximumSize = new Size(678, 409);
            MdiChildrenMinimizedAnchorBottom = false;
            MinimumSize = new Size(678, 409);
            Name = "Launcherform";
            Opacity = 0.97D;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "launcherform";
            FormClosing += launcherform_FormClosing;
            Load += Launcherform_Load;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox textGamePath;
        private Button buttonBrowseGameFolder;
        public ToolStripStatusLabel statusStripLabel;
        public StatusStrip statusStrip;
        public ToolStripProgressBar toolStripProgress;
        private Panel panel1;
        private Button buttonOffline;
        private Button buttonOnline;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem installToolStripMenuItem;
        private ToolStripMenuItem changelogToolStripMenuItem;
        private ToolStripMenuItem checkForNewVersionToolStripMenuItem;
        private TextBox textGameVersion;
        private Label label3;
        private Button buttonGetGameFolder;
        private CheckBox checkBoxKeepOpen;
        private CheckBox checkBoxStartUpMessage;
        private RadioButton radioGamepad;
        private RadioButton radioKb;
        private ToolStripMenuItem toolStripTheme;
        private ToolStripStatusLabel toolStripStatus;
        private Button buttonAttachLog;
        private CheckBox checkBoxHideConsole;
        private Panel panel2;
        private Button buttonStartupMessage;
    }
}