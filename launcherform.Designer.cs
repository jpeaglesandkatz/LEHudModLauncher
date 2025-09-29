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
            panel1 = new Panel();
            statusStrip = new StatusStrip();
            statusStripLabel = new ToolStripStatusLabel();
            toolStripStatus = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            toolStripTheme = new ToolStripMenuItem();
            toolStripOptions = new ToolStripMenuItem();
            toolstripAutoUpdate = new ToolStripMenuItem();
            toolstripCheckModUpdate = new ToolStripMenuItem();
            checkBoxStartUpMessage = new ToolStripMenuItem();
            checkBoxKeepOpen = new ToolStripMenuItem();
            checkBoxHideConsole = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            checkForLauncherUpdateNowToolStripMenuItem = new ToolStripMenuItem();
            toolstripForceModeUpdate = new ToolStripMenuItem();
            forceInstallModToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            buttonAttachLog = new ToolStripMenuItem();
            buttonStartupMessage = new ToolStripMenuItem();
            buttonOffline = new MaterialSkin.Controls.MaterialButton();
            materialTextBoxPath = new MaterialSkin.Controls.MaterialTextBox2();
            buttonOnline = new MaterialSkin.Controls.MaterialButton();
            materialButtonBrowse = new MaterialSkin.Controls.MaterialButton();
            radioKb = new MaterialSkin.Controls.MaterialRadioButton();
            radioGamepad = new MaterialSkin.Controls.MaterialRadioButton();
            materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            textGameVersion = new MaterialSkin.Controls.MaterialTextBox();
            pictureCheck = new PictureBox();
            materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            pictureCheckLoader = new PictureBox();
            buttonGetGameFolder = new MaterialSkin.Controls.MaterialButton();
            materialButton1 = new MaterialSkin.Controls.MaterialButton();
            StatusModInstalled = new MaterialSkin.Controls.MaterialLabel();
            pictureModInstalled = new PictureBox();
            labelVersion = new MaterialSkin.Controls.MaterialLabel();
            statusStrip.SuspendLayout();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureCheck).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureCheckLoader).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureModInstalled).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.None;
            panel1.Location = new Point(556, -850);
            panel1.Name = "panel1";
            panel1.Size = new Size(181, 122);
            panel1.TabIndex = 6;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(24, 24);
            statusStrip.Items.AddRange(new ToolStripItem[] { statusStripLabel, toolStripStatus });
            statusStrip.Location = new Point(0, 559);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(915, 35);
            statusStrip.TabIndex = 4;
            statusStrip.Text = "";
            // 
            // statusStripLabel
            // 
            statusStripLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            statusStripLabel.Name = "statusStripLabel";
            statusStripLabel.Size = new Size(0, 28);
            // 
            // toolStripStatus
            // 
            toolStripStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            toolStripStatus.Name = "toolStripStatus";
            toolStripStatus.Size = new Size(92, 28);
            toolStripStatus.Text = "";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripTheme, toolStripOptions, buttonAttachLog, buttonStartupMessage });
            menuStrip1.Location = new Point(0, 72);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(915, 33);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "";
            // 
            // toolStripTheme
            // 
            toolStripTheme.Alignment = ToolStripItemAlignment.Right;
            toolStripTheme.BackColor = Color.Transparent;
            toolStripTheme.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStripTheme.Name = "toolStripTheme";
            toolStripTheme.Size = new Size(86, 29);
            toolStripTheme.Text = "";
            toolStripTheme.ToolTipText = "Switch between Dark and Light mode";
            toolStripTheme.Click += toolStripMenuItem1_Click;
            // 
            // toolStripOptions
            // 
            toolStripOptions.Alignment = ToolStripItemAlignment.Right;
            toolStripOptions.BackColor = Color.Transparent;
            toolStripOptions.DropDownItems.AddRange(new ToolStripItem[] { toolstripAutoUpdate, toolstripCheckModUpdate, checkBoxStartUpMessage, checkBoxKeepOpen, checkBoxHideConsole, toolStripSeparator1, checkForLauncherUpdateNowToolStripMenuItem, toolstripForceModeUpdate, forceInstallModToolStripMenuItem, toolStripSeparator2 });
            toolStripOptions.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStripOptions.Image = Properties.Resources.settings_dark;
            toolStripOptions.Name = "toolStripOptions";
            toolStripOptions.Size = new Size(119, 29);
            toolStripOptions.Text = "Options";
            // 
            // toolstripAutoUpdate
            // 
            toolstripAutoUpdate.CheckOnClick = true;
            toolstripAutoUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolstripAutoUpdate.Name = "toolstripAutoUpdate";
            toolstripAutoUpdate.Size = new Size(539, 34);
            toolstripAutoUpdate.Text = "Automatically check for launcher update";
            toolstripAutoUpdate.CheckStateChanged += toolstripAutoUpdate_CheckStateChanged;
            // 
            // toolstripCheckModUpdate
            // 
            toolstripCheckModUpdate.Checked = true;
            toolstripCheckModUpdate.CheckOnClick = true;
            toolstripCheckModUpdate.CheckState = CheckState.Checked;
            toolstripCheckModUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolstripCheckModUpdate.Name = "toolstripCheckModUpdate";
            toolstripCheckModUpdate.Size = new Size(539, 34);
            toolstripCheckModUpdate.Text = "Automatically install and update modloader and mod";
            toolstripCheckModUpdate.CheckedChanged += toolstripCheckModUpdate_CheckedChanged;
            // 
            // checkBoxStartUpMessage
            // 
            checkBoxStartUpMessage.CheckOnClick = true;
            checkBoxStartUpMessage.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBoxStartUpMessage.Name = "checkBoxStartUpMessage";
            checkBoxStartUpMessage.Size = new Size(539, 34);
            checkBoxStartUpMessage.Text = "Show mod info on start";
            checkBoxStartUpMessage.CheckedChanged += checkBoxStartUpMessage_CheckedChanged_2;
            // 
            // checkBoxKeepOpen
            // 
            checkBoxKeepOpen.CheckOnClick = true;
            checkBoxKeepOpen.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBoxKeepOpen.Name = "checkBoxKeepOpen";
            checkBoxKeepOpen.Size = new Size(539, 34);
            checkBoxKeepOpen.Text = "Keep open on Game Launch";
            checkBoxKeepOpen.CheckStateChanged += checkBoxKeepOpen_CheckStateChanged;
            // 
            // checkBoxHideConsole
            // 
            checkBoxHideConsole.CheckOnClick = true;
            checkBoxHideConsole.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBoxHideConsole.Name = "checkBoxHideConsole";
            checkBoxHideConsole.Size = new Size(539, 34);
            checkBoxHideConsole.Text = "Hide melonloader console window";
            checkBoxHideConsole.CheckStateChanged += checkBoxHideConsole_CheckStateChanged;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(536, 6);
            // 
            // checkForLauncherUpdateNowToolStripMenuItem
            // 
            checkForLauncherUpdateNowToolStripMenuItem.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkForLauncherUpdateNowToolStripMenuItem.Name = "checkForLauncherUpdateNowToolStripMenuItem";
            checkForLauncherUpdateNowToolStripMenuItem.Size = new Size(539, 34);
            checkForLauncherUpdateNowToolStripMenuItem.Text = "Check for launcher update now";
            checkForLauncherUpdateNowToolStripMenuItem.Click += checkForLauncherUpdateNowToolStripMenuItem_Click;
            // 
            // toolstripForceModeUpdate
            // 
            toolstripForceModeUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolstripForceModeUpdate.Name = "toolstripForceModeUpdate";
            toolstripForceModeUpdate.Size = new Size(539, 34);
            toolstripForceModeUpdate.Text = "Force install modloader";
            toolstripForceModeUpdate.Click += toolstripForceModeUpdate_Click;
            // 
            // forceInstallModToolStripMenuItem
            // 
            forceInstallModToolStripMenuItem.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            forceInstallModToolStripMenuItem.Name = "forceInstallModToolStripMenuItem";
            forceInstallModToolStripMenuItem.Size = new Size(539, 34);
            forceInstallModToolStripMenuItem.Text = "Force install latest mod";
            forceInstallModToolStripMenuItem.Click += forceInstallModToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(536, 6);
            // 
            // buttonAttachLog
            // 
            buttonAttachLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonAttachLog.Name = "buttonAttachLog";
            buttonAttachLog.Size = new Size(300, 29);
            buttonAttachLog.Text = "Attach melonloader log window";
            buttonAttachLog.Click += buttonAttachLog_Click_2;
            // 
            // buttonStartupMessage
            // 
            buttonStartupMessage.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonStartupMessage.Name = "buttonStartupMessage";
            buttonStartupMessage.Size = new Size(208, 29);
            buttonStartupMessage.Text = "Show latest mod info";
            buttonStartupMessage.Click += buttonStartupMessage_Click_1;
            // 
            // buttonOffline
            // 
            buttonOffline.AccessibleName = "buttonOffline";
            buttonOffline.AutoSize = false;
            buttonOffline.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOffline.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonOffline.Depth = 0;
            buttonOffline.FlatStyle = FlatStyle.Flat;
            buttonOffline.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonOffline.HighEmphasis = true;
            buttonOffline.Icon = null;
            buttonOffline.Location = new Point(514, 145);
            buttonOffline.Margin = new Padding(6, 10, 6, 10);
            buttonOffline.MouseState = MaterialSkin.MouseState.HOVER;
            buttonOffline.Name = "buttonOffline";
            buttonOffline.NoAccentTextColor = Color.Empty;
            buttonOffline.Size = new Size(282, 77);
            buttonOffline.TabIndex = 23;
            buttonOffline.Text = "Start game OFFLINE";
            buttonOffline.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonOffline.UseAccentColor = true;
            buttonOffline.UseVisualStyleBackColor = true;
            buttonOffline.Click += materialButton1_Click;
            // 
            // materialTextBoxPath
            // 
            materialTextBoxPath.AnimateReadOnly = false;
            materialTextBoxPath.BackgroundImageLayout = ImageLayout.None;
            materialTextBoxPath.CharacterCasing = CharacterCasing.Normal;
            materialTextBoxPath.Depth = 0;
            materialTextBoxPath.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            materialTextBoxPath.HideSelection = true;
            materialTextBoxPath.LeadingIcon = null;
            materialTextBoxPath.Location = new Point(112, 398);
            materialTextBoxPath.Margin = new Padding(4, 5, 4, 5);
            materialTextBoxPath.MaxLength = 32767;
            materialTextBoxPath.MouseState = MaterialSkin.MouseState.OUT;
            materialTextBoxPath.Name = "materialTextBoxPath";
            materialTextBoxPath.PasswordChar = '\0';
            materialTextBoxPath.PrefixSuffixText = null;
            materialTextBoxPath.ReadOnly = false;
            materialTextBoxPath.RightToLeft = RightToLeft.No;
            materialTextBoxPath.SelectedText = "";
            materialTextBoxPath.SelectionLength = 0;
            materialTextBoxPath.SelectionStart = 0;
            materialTextBoxPath.ShortcutsEnabled = true;
            materialTextBoxPath.Size = new Size(610, 36);
            materialTextBoxPath.TabIndex = 24;
            materialTextBoxPath.TabStop = false;
            materialTextBoxPath.Text = "materialTextBox21";
            materialTextBoxPath.TextAlign = HorizontalAlignment.Left;
            materialTextBoxPath.TrailingIcon = null;
            materialTextBoxPath.UseSystemPasswordChar = false;
            materialTextBoxPath.UseTallSize = false;
            materialTextBoxPath.TextChanged += materialTextBoxPath_TextChanged;
            // 
            // buttonOnline
            // 
            buttonOnline.AccessibleName = "buttonOnline";
            buttonOnline.AutoSize = false;
            buttonOnline.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOnline.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonOnline.Depth = 0;
            buttonOnline.FlatStyle = FlatStyle.Flat;
            buttonOnline.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonOnline.HighEmphasis = true;
            buttonOnline.Icon = null;
            buttonOnline.Location = new Point(112, 145);
            buttonOnline.Margin = new Padding(6, 10, 6, 10);
            buttonOnline.MouseState = MaterialSkin.MouseState.HOVER;
            buttonOnline.Name = "buttonOnline";
            buttonOnline.NoAccentTextColor = Color.Empty;
            buttonOnline.Size = new Size(282, 77);
            buttonOnline.TabIndex = 25;
            buttonOnline.Text = "Start game ONLINE (no mod)";
            buttonOnline.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonOnline.UseAccentColor = true;
            buttonOnline.UseVisualStyleBackColor = true;
            buttonOnline.Click += materialButton2_Click;
            // 
            // materialButtonBrowse
            // 
            materialButtonBrowse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            materialButtonBrowse.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialButtonBrowse.Depth = 0;
            materialButtonBrowse.HighEmphasis = true;
            materialButtonBrowse.Icon = null;
            materialButtonBrowse.Location = new Point(732, 399);
            materialButtonBrowse.Margin = new Padding(6, 10, 6, 10);
            materialButtonBrowse.MouseState = MaterialSkin.MouseState.HOVER;
            materialButtonBrowse.Name = "materialButtonBrowse";
            materialButtonBrowse.NoAccentTextColor = Color.Empty;
            materialButtonBrowse.Size = new Size(64, 36);
            materialButtonBrowse.TabIndex = 26;
            materialButtonBrowse.Text = "...";
            materialButtonBrowse.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            materialButtonBrowse.UseAccentColor = false;
            materialButtonBrowse.UseVisualStyleBackColor = true;
            materialButtonBrowse.Click += materialButtonBrowse_Click;
            // 
            // radioKb
            // 
            radioKb.AutoSize = true;
            radioKb.Depth = 0;
            radioKb.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            radioKb.Location = new Point(371, 247);
            radioKb.Margin = new Padding(0);
            radioKb.MouseLocation = new Point(-1, -1);
            radioKb.MouseState = MaterialSkin.MouseState.HOVER;
            radioKb.Name = "radioKb";
            radioKb.Ripple = true;
            radioKb.Size = new Size(165, 37);
            radioKb.TabIndex = 33;
            radioKb.TabStop = true;
            radioKb.Text = "Keyboard / mouse";
            radioKb.UseVisualStyleBackColor = true;
            radioKb.CheckedChanged += radioKb_CheckedChanged_1;
            radioKb.Click += radioKb_Click;
            // 
            // radioGamepad
            // 
            radioGamepad.AutoSize = true;
            radioGamepad.Depth = 0;
            radioGamepad.Location = new Point(371, 284);
            radioGamepad.Margin = new Padding(0);
            radioGamepad.MouseLocation = new Point(-1, -1);
            radioGamepad.MouseState = MaterialSkin.MouseState.HOVER;
            radioGamepad.Name = "radioGamepad";
            radioGamepad.Ripple = true;
            radioGamepad.Size = new Size(104, 37);
            radioGamepad.TabIndex = 34;
            radioGamepad.TabStop = true;
            radioGamepad.Text = "Gamepad";
            radioGamepad.UseVisualStyleBackColor = true;
            radioGamepad.CheckedChanged += radioGamepad_CheckedChanged_1;
            radioGamepad.Click += radioGamepad_Click;
            // 
            // materialLabel1
            // 
            materialLabel1.AutoSize = true;
            materialLabel1.Depth = 0;
            materialLabel1.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            materialLabel1.Location = new Point(403, 188);
            materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            materialLabel1.Name = "materialLabel1";
            materialLabel1.Size = new Size(100, 19);
            materialLabel1.TabIndex = 35;
            materialLabel1.Text = "Game Version";
            // 
            // textGameVersion
            // 
            textGameVersion.AnimateReadOnly = false;
            textGameVersion.BorderStyle = BorderStyle.None;
            textGameVersion.Depth = 0;
            textGameVersion.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textGameVersion.LeadingIcon = null;
            textGameVersion.Location = new Point(403, 149);
            textGameVersion.MaxLength = 50;
            textGameVersion.MouseState = MaterialSkin.MouseState.OUT;
            textGameVersion.Multiline = false;
            textGameVersion.Name = "textGameVersion";
            textGameVersion.ReadOnly = true;
            textGameVersion.Size = new Size(102, 36);
            textGameVersion.TabIndex = 36;
            textGameVersion.Text = "";
            textGameVersion.TrailingIcon = null;
            textGameVersion.UseAccent = false;
            textGameVersion.UseTallSize = false;
            // 
            // pictureCheck
            // 
            pictureCheck.BackColor = Color.Transparent;
            pictureCheck.BackgroundImageLayout = ImageLayout.None;
            pictureCheck.Location = new Point(74, 398);
            pictureCheck.Name = "pictureCheck";
            pictureCheck.Size = new Size(31, 37);
            pictureCheck.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureCheck.TabIndex = 39;
            pictureCheck.TabStop = false;
            // 
            // materialLabel2
            // 
            materialLabel2.AutoSize = true;
            materialLabel2.Depth = 0;
            materialLabel2.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            materialLabel2.Location = new Point(112, 480);
            materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            materialLabel2.Name = "materialLabel2";
            materialLabel2.Size = new Size(199, 19);
            materialLabel2.TabIndex = 40;
            materialLabel2.Text = "Correct Mod loader installed";
            // 
            // pictureCheckLoader
            // 
            pictureCheckLoader.BackColor = Color.Transparent;
            pictureCheckLoader.BackgroundImageLayout = ImageLayout.None;
            pictureCheckLoader.Location = new Point(74, 468);
            pictureCheckLoader.Name = "pictureCheckLoader";
            pictureCheckLoader.Size = new Size(31, 37);
            pictureCheckLoader.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureCheckLoader.TabIndex = 41;
            pictureCheckLoader.TabStop = false;
            // 
            // buttonGetGameFolder
            // 
            buttonGetGameFolder.AccessibleName = "buttonGetGameFolder";
            buttonGetGameFolder.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonGetGameFolder.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            buttonGetGameFolder.Depth = 0;
            buttonGetGameFolder.HighEmphasis = true;
            buttonGetGameFolder.Icon = null;
            buttonGetGameFolder.Location = new Point(112, 356);
            buttonGetGameFolder.Margin = new Padding(6, 10, 6, 10);
            buttonGetGameFolder.MouseState = MaterialSkin.MouseState.HOVER;
            buttonGetGameFolder.Name = "buttonGetGameFolder";
            buttonGetGameFolder.NoAccentTextColor = Color.Empty;
            buttonGetGameFolder.Size = new Size(232, 36);
            buttonGetGameFolder.TabIndex = 27;
            buttonGetGameFolder.Text = "Get game path from Steam";
            buttonGetGameFolder.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            buttonGetGameFolder.UseAccentColor = false;
            buttonGetGameFolder.UseVisualStyleBackColor = true;
            buttonGetGameFolder.Click += buttonGetGameFolder_Click_1;
            // 
            // materialButton1
            // 
            materialButton1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            materialButton1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialButton1.Depth = 0;
            materialButton1.Enabled = false;
            materialButton1.HighEmphasis = true;
            materialButton1.Icon = null;
            materialButton1.Location = new Point(726, 238);
            materialButton1.Margin = new Padding(4, 6, 4, 6);
            materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            materialButton1.Name = "materialButton1";
            materialButton1.NoAccentTextColor = Color.Empty;
            materialButton1.Size = new Size(70, 36);
            materialButton1.TabIndex = 37;
            materialButton1.Text = "Theme";
            materialButton1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            materialButton1.UseAccentColor = false;
            materialButton1.UseVisualStyleBackColor = true;
            materialButton1.Visible = false;
            materialButton1.Click += materialButton1_Click_1;
            // 
            // StatusModInstalled
            // 
            StatusModInstalled.AutoSize = true;
            StatusModInstalled.Depth = 0;
            StatusModInstalled.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            StatusModInstalled.Location = new Point(416, 480);
            StatusModInstalled.MouseState = MaterialSkin.MouseState.HOVER;
            StatusModInstalled.Name = "StatusModInstalled";
            StatusModInstalled.Size = new Size(130, 19);
            StatusModInstalled.TabIndex = 42;
            StatusModInstalled.Text = "Hud Mod installed";
            // 
            // pictureModInstalled
            // 
            pictureModInstalled.BackColor = Color.Transparent;
            pictureModInstalled.BackgroundImageLayout = ImageLayout.None;
            pictureModInstalled.Location = new Point(371, 468);
            pictureModInstalled.Name = "pictureModInstalled";
            pictureModInstalled.Size = new Size(31, 37);
            pictureModInstalled.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureModInstalled.TabIndex = 43;
            pictureModInstalled.TabStop = false;
            // 
            // labelVersion
            // 
            labelVersion.AutoSize = true;
            labelVersion.Depth = 0;
            labelVersion.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            labelVersion.Location = new Point(416, 508);
            labelVersion.MouseState = MaterialSkin.MouseState.HOVER;
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(107, 19);
            labelVersion.TabIndex = 44;
            labelVersion.Text = "materialLabel3";
            // 
            // Launcherform
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            ClientSize = new Size(919, 599);
            Controls.Add(labelVersion);
            Controls.Add(pictureModInstalled);
            Controls.Add(StatusModInstalled);
            Controls.Add(pictureCheckLoader);
            Controls.Add(materialLabel2);
            Controls.Add(pictureCheck);
            Controls.Add(materialButton1);
            Controls.Add(textGameVersion);
            Controls.Add(materialLabel1);
            Controls.Add(radioGamepad);
            Controls.Add(radioKb);
            Controls.Add(buttonGetGameFolder);
            Controls.Add(materialButtonBrowse);
            Controls.Add(buttonOnline);
            Controls.Add(buttonOffline);
            Controls.Add(materialTextBoxPath);
            Controls.Add(panel1);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip1);
            DrawerHighlightWithAccent = false;
            DrawerShowIconsWhenHidden = true;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormStyle = FormStyles.ActionBar_48;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            MaximumSize = new Size(919, 599);
            MdiChildrenMinimizedAnchorBottom = false;
            MinimumSize = new Size(919, 599);
            Name = "Launcherform";
            Opacity = 0.97D;
            Padding = new Padding(0, 72, 4, 5);
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "launcherform";
            FormClosing += launcherform_FormClosing;
            Load += Launcherform_Load;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureCheck).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureCheckLoader).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureModInstalled).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public ToolStripStatusLabel statusStripLabel;
        public StatusStrip statusStrip;
        private Panel panel1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripTheme;
        private ToolStripStatusLabel toolStripStatus;
        private ToolStripMenuItem toolStripOptions;
        private ToolStripMenuItem toolstripAutoUpdate;
        private ToolStripMenuItem checkForLauncherUpdateNowToolStripMenuItem;
        private Button buttonGetGameFolder2;
        private Button buttonBrowseGameFolder;
        private MaterialSkin.Controls.MaterialButton buttonOffline;
        private MaterialSkin.Controls.MaterialTextBox2 materialTextBoxPath;
        private MaterialSkin.Controls.MaterialButton buttonOnline;
        private MaterialSkin.Controls.MaterialButton materialButtonBrowse;
        private MaterialSkin.Controls.MaterialRadioButton radioKb;
        private MaterialSkin.Controls.MaterialRadioButton radioGamepad;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialTextBox textGameVersion;
        private ToolStripMenuItem toolstripCheckModUpdate;
        private ToolStripMenuItem toolstripForceModeUpdate;
        private ToolStripSeparator toolStripSeparator1;
        private PictureBox pictureCheck;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private PictureBox pictureCheckLoader;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem checkBoxKeepOpen;
        private ToolStripMenuItem checkBoxStartUpMessage;
        private ToolStripMenuItem checkBoxHideConsole;
        private ToolStripMenuItem buttonAttachLog;
        private ToolStripMenuItem buttonStartupMessage;
        private MaterialSkin.Controls.MaterialButton buttonGetGameFolder;
        private MaterialSkin.Controls.MaterialButton materialButton1;
        private ToolStripMenuItem forceInstallModToolStripMenuItem;
        private MaterialSkin.Controls.MaterialLabel StatusModInstalled;
        private PictureBox pictureModInstalled;
        private MaterialSkin.Controls.MaterialLabel labelVersion;
        //private MaterialSkin.Controls.MaterialDrawer materialDrawer1;
    }
}