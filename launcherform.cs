using LauncherUtils;
using LEHudModLauncher;
using LEHuDModLauncher.Classlibs;
using LogUtils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using static LauncherUtils.Utils;

namespace LEHuDModLauncher
{
    public partial class Launcherform : Form
    {
        public static string GameFilename = "Last Epoch.exe";
        public static string VersionFilename = @"version.dll";
        public static string VersionBackFilename = @"version.dll.back";

        private Process _gameProcess;
        public Logger Dlog = new Logger();
        public Utils Utils = new Utils();

        // Tray icon + menu
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;

        public AppSettings settings = SettingsManager.Instance.Settings;

        static string Truncate(string s, int len) => s == null ? "" : (s.Length <= len ? s : s.Substring(0, len) + "…");

        public Launcherform()
        {
            try
            {
                InitializeComponent();
                Logger.Global.Debug("Initializing Launcherform");
                if (SettingsManager.Instance.Settings.DarkMode) ThemeUtils.ApplyDarkTheme(this);
                else ThemeUtils.ApplyLightTheme(this);
                
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string iconPath = Path.Combine(exeDir, "Resources", "gooey-daemon.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else
                {
                    var asm = Assembly.GetExecutingAssembly();
                    using var stream = asm.GetManifestResourceStream("LEHuDModLauncher.Resources.gooey-daemon.ico");
                    if (stream != null) this.Icon = new Icon(stream);
                }
                //InitializeTrayIcon();
                //this.Resize += Launcherform_Resize;

                var assembly = Assembly.GetExecutingAssembly();
                this.Text =
                    $"{assembly.GetName().Name} - {assembly.GetName().Version} for Last Epoch Hud Mod by Ash, launcher by JP";
                Logger.Global.Info(this.Text);
                Logger.Global.Info("==========  LE Hud Mod Launcher started ============ ");

                SettingsManager.Instance.Load();

                var gameDir = GetPathFromRegistry("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 899770", "Installlocation");
                if (SettingsManager.Instance.Settings.GameDir == null) SettingsManager.Instance.UpdateGameDir(gameDir);

                var steamPath = GetPathFromRegistry("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath");
                if (!string.IsNullOrEmpty(steamPath))
                    SettingsManager.Instance.UpdateSteamPath(steamPath);

                SettingsManager.Instance.UpdateTmpDownloadFolder(Path.GetTempPath() + @"LEModtempDir");

                //if (!string.IsNullOrEmpty(SettingsManager.Instance.Settings.Steampath))
                //{
                //    SettingsManager.Instance.UpdateSteamPath(SettingsManager.Instance.Settings.Steampath);
                //    Logger.Global.Debug($"GameDir: {SettingsManager.Instance.Settings.GameDir}");
                //    Logger.Global.Debug($"Steampath: {SettingsManager.Instance.Settings.Steampath}");
                //}

                textGamePath.Text = SettingsManager.Instance.Settings.GameDir;
                checkBoxKeepOpen.Checked = SettingsManager.Instance.Settings.KeepOpen;
                checkBoxStartUpMessage.Checked = SettingsManager.Instance.Settings.ShowStartupMessage;
                checkBoxHideConsole.Checked = SettingsManager.Instance.Settings.HideConsole;
                if (SettingsManager.Instance.Settings.KbGamePadSelect == 0) radioKb.Checked = true;
                else radioGamepad.Checked = true;
                textGameVersion.Text = "Unknown";
                toolStripProgress.Enabled = true;
                toolStripProgress.Visible = false;
                toolStripStatus.Text = "";
                toolStripTheme.Text = SettingsManager.Instance.Settings.DarkMode ? "Light theme" : "Dark theme";
                SetHideConsole(SettingsManager.Instance.Settings.GameDir + @"\\UserData\Loader.cfg");

                // Extract embedded resources to temp folder
                ExtractAllResourcesToTempFolder();

                if (File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename)))
                    ShowGameVersion();
                else textGameVersion.Text = "Unknown";

                ShowStartupMessage();

                // Set window position from settings
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new System.Drawing.Point(
                    SettingsManager.Instance.Settings.MainWindowX,
                    SettingsManager.Instance.Settings.MainWindowY
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please Report this error on the LE Hud Mod discord server...\n\n" + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
            }
        }

        // Initialize tray icon + context menu
        //private void InitializeTrayIcon()
        //{
        //    // Create context menu
        //    trayMenu = new ContextMenuStrip();
        //    var restoreItem = new ToolStripMenuItem("Restore");
        //    restoreItem.Click += (s, e) => RestoreFromTray();
        //    var exitItem = new ToolStripMenuItem("Exit");
        //    exitItem.Click += (s, e) => Application.Exit();
        //    trayMenu.Items.AddRange(new ToolStripItem[] { restoreItem, exitItem });

        //    // Create the NotifyIcon
        //    trayIcon = new NotifyIcon
        //    {
        //        Icon = this.Icon ?? SystemIcons.Application,
        //        ContextMenuStrip = trayMenu,
        //        Text = $"{Assembly.GetExecutingAssembly().GetName().Name} - LE Hud Mod Launcher",
        //        Visible = true
        //    };

        //    trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        //}

        //// Restore window from tray
        //private void RestoreFromTray()
        //{
        //    if (InvokeRequired)
        //    {
        //        BeginInvoke(new Action(RestoreFromTray));
        //        return;
        //    }

        //    Show();
        //    WindowState = FormWindowState.Normal;
        //    ShowInTaskbar = true;
        //    Activate();
        //    trayIcon!.Visible = true;
        //}

        //// Minimize to tray behavior
        //private void Launcherform_Resize(object? sender, EventArgs e)
        //{
        //    if (WindowState == FormWindowState.Minimized)
        //    {
        //        Hide();
        //        ShowInTaskbar = false;
        //        if (trayIcon != null) trayIcon.Visible = true;
        //    }
        //}

        public void ShowGameVersion()
        {
            var candidates = UnityVersionExtractor.ExtractVersionsFromGlobalGameManagers(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename));
            if (candidates.Count == 0) textGameVersion.Text = "Unknown";
            else
            {
                var best = candidates[1];
                textGameVersion.Text = best.version;
            }
        }

        //public void Showgameversion()
        //{
        //    try
        //    {
        //        string exe = Path.Combine(SettingsManager.Instance.Settings.GameDir, "Last Epoch.exe");
        //        var candidates = UnityVersionExtractor.ExtractVersionsFromGlobalGameManagers(exe);

        //        if (candidates.Count == 0)
        //        {
        //            Logger.Global.Error("No version-like strings found in globalgamemanagers.\nTry checking app.info, version.txt, or Steam appmanifest as a fallback.");
        //            return;
        //        }

        //        Logger.Global.Info("Version candidates (best first):");
        //        foreach (var c in candidates)
        //        {
        //            Logger.Global.Info($" - {c.version}  (score={c.score})\ncontext: {Truncate(c.context, 140)}");
        //        }

        //        Logger.Global.Info("Best guess: " + UnityVersionExtractor.GetBestGuessVersion(exe));
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Global.Info("Error: " + ex.Message);
        //    }
        //}

        private void CleanupDirs()
        {
            try
            {
                Logger.Global.Info("CleanupDirs started...");

                var melonLoaderPath = Path.Combine(SettingsManager.Instance.Settings.GameDir, "Melonloader");
                var modsPath = Path.Combine(SettingsManager.Instance.Settings.GameDir, "Mods");

                if (Directory.Exists(melonLoaderPath))
                    Directory.Delete(melonLoaderPath, true);
                else Logger.Global.Info($"[{nameof(CleanupDirs)}] Melonloader dir not found, skipping delete.");

                if (!Directory.Exists(modsPath))
                {
                    Directory.CreateDirectory(modsPath);
                    Logger.Global.Info($"[{nameof(CleanupDirs)}] Created Mods directory: {modsPath}");
                }
                else Logger.Global.Info($"[{nameof(CleanupDirs)}] Mods directory already exists: {modsPath}");
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"CleanupDirs error: {ex}");
                MessageBox.Show($"Failed to clean up directories:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Extract all embedded resources to the temporary folder
        public void ExtractAllResourcesToTempFolder()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourcePrefix = "LEHuDModLauncher.Resources.";

                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    string fileName = resourceName.StartsWith(resourcePrefix)
                        ? resourceName.Substring(resourcePrefix.Length)
                        : resourceName;

                    var outputPath = SettingsManager.Instance.Settings.TmpDownloadFolder;

                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null) continue;
                    using var fileStream = new FileStream(Path.Combine(outputPath, fileName), FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"ExtractAllResourcesToTempFolder error: {ex}");
                MessageBox.Show($"Failed to extract resources:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // select Game folder
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new FolderBrowserDialog();
                dialog.Description = "Select Last Epoch game folder";
                dialog.SelectedPath = SettingsManager.Instance.Settings.GameDir;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textGamePath.Text = dialog.SelectedPath;
                    SettingsManager.Instance.UpdateGameDir(dialog.SelectedPath);
                }
                if (File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename))) ShowGameVersion();
                else textGameVersion.Text = "Unknown";
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"buttonBrowse_Click error: {ex}");
                MessageBox.Show($"Failed to select game folder:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void StartGame(bool online)
        {
            if (!File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename)))
            {
                MessageBox.Show("Set a valid game folder first!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                if (online)
                {
                    string gamedir = SettingsManager.Instance.Settings.GameDir;
                    if (File.Exists(gamedir + VersionFilename))
                    {
                        if (File.Exists(gamedir + VersionBackFilename)) { File.Delete(gamedir + VersionFilename); }
                        else { File.Move(gamedir + VersionFilename, gamedir + VersionBackFilename); }
                    }

                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = $"steam://rungameid/{899770}",
                        UseShellExecute = true,
                    };
                    using var process = new Process();
                    process.StartInfo = processStartInfo;
                    Logger.Global.Info($"Starting game with steam://rungameid/{899770}");
                    try
                    {
                        process.Exited += GameProcess_Exited;
                        process.Start();
                        statusStripLabel.Text = "Last Epoch started...";
                    }
                    catch (Exception ex)
                    {
                        Logger.Global.Error($"Failed to start Steam process: {ex}");
                        MessageBox.Show($"Failed to start Steam:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    string gamedir = SettingsManager.Instance.Settings.GameDir;
                    if (!File.Exists(gamedir + VersionFilename))
                    {
                        if (File.Exists(gamedir + VersionBackFilename))
                        {
                            File.Move(gamedir + VersionBackFilename, gamedir + VersionFilename);
                        }
                    }
                    else if (File.Exists(gamedir + VersionBackFilename)) { File.Delete(gamedir + VersionBackFilename); }
                    Directory.SetCurrentDirectory(gamedir);
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = $"\"{gamedir}\\{GameFilename}\"",
                        Arguments = "--offline",
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WorkingDirectory = $"\"{gamedir}\""
                    };
                    Logger.Global.Debug("Launching game: " + $"\"{gamedir}\\{GameFilename}\"\n" + $"\"{gamedir}\"");
                    try
                    {
                        _gameProcess = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };
                        _gameProcess.Exited += GameProcess_Exited;
                        _gameProcess.Start();
                        statusStripLabel.Text = "Last Epoch started...";

                    }
                    catch (Exception ex)
                    {
                        Logger.Global.Error($"Failed to start game process: {ex}");
                        MessageBox.Show($"Failed to start Last Epoch:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (!SettingsManager.Instance.Settings.KeepOpen) Application.Exit();
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"StartGame error: {ex}");
                MessageBox.Show($"Unexpected error launching game:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GameProcess_Exited(object? sender, EventArgs e)
        {
            // This event is raised on a background thread → marshal back to UI
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => GameProcess_Exited(sender, e)));
                statusStripLabel.Text = "Game Exited!";
                _gameProcess.Exited -= GameProcess_Exited;
                return;
            }
            statusStripLabel.Text = "Game Exited!";
        }
        private void buttonOnline_Click(object sender, EventArgs e)
        {
            StartGame(true);
        }
        private void buttonOffline_Click(object sender, EventArgs e)
        {
            toolStripStatus.Text = "";
            StartGame(false);
        }

        private void launcherform_FormClosing(object sender, FormClosingEventArgs e)
        {

            // Dispose tray icon and menu to free native resources
            try
            {
                trayIcon?.Dispose();
                trayMenu?.Dispose();
            }
            catch { /* swallow */ }
        }

        private void textPath_TextChanged(object sender, EventArgs e)
        {
            SettingsManager.Instance.UpdateGameDir(textGamePath.Text);
            if (File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename))) ShowGameVersion();

        }

        private void installToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(SettingsManager.ReferenceEquals(SettingsManager.Instance.Settings.GameDir, "") ? "" : Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename)) == false)
            {
                MessageBox.Show("Please select a valid Last Epoch game folder first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            CleanupDirs();

            SettingsManager.Instance.UpdateErrorCount(0);

            Utils.AddDownload(2, "Melonloader", "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/Melon.zip", SettingsManager.Instance.Settings.GameDir, SettingsManager.Instance.Settings.GameDir, "Melon.zip", false, false);

            // Pick either keyboard or gamepad version
            if (radioKb.Checked)
            {
                var result = MessageBox.Show("Installing Keyboard version of the mod.\nUse this version if you mainly play the game with your keyboard/mouse.\nProceed?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.No) return;
                else Utils.AddDownload(1, "Mod", "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/LastEpoch_Hud.Keyboard.rar", SettingsManager.Instance.Settings.GameDir, SettingsManager.Instance.Settings.GameDir + @"\mods", "LastEpoch_Hud(Keyboard).rar", true, false);
            }
            else
            {
                var result = MessageBox.Show("Installing Gamepad version of the mod.\nUse this version if you mainly play the game with your gamepad.\nProceed?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.No) return;
                else Utils.AddDownload(1, "Mod", "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/LastEpoch_Hud.WinGamepad.rar", SettingsManager.Instance.Settings.GameDir, SettingsManager.Instance.Settings.GameDir + @"\mods", "LastEpoch_Hud.WinGamepad.rar", true, false);
            }
            toolStripStatus.Enabled = true;
            toolStripStatus.Enabled = true;
            toolStripStatus.Text = "Installing...";


            if (Directory.Exists(SettingsManager.Instance.Settings.GameDir + @"\Melonloader")) Directory.Delete(SettingsManager.Instance.Settings.GameDir + @"\Melonloader", true);

            Utils.StartDownloads();
            Utils.RunExtraction();
            toolStripStatus.Text = "Done!";

            if (SettingsManager.Instance.Settings.ErrorCount == 0) MessageBox.Show("All done! You can now start Last Epoch with the HUD mod installed.\n\n" +
                "But make sure to run the game at least once and exit. This should fix most common issues."
                , "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else MessageBox.Show("Finished with errors. Please check the log.", "Errors occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            toolStripStatus.Text = "Installation finished... Run the game once and exit the game!";
        }

        private void buttonBrowseSteamFolder_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Select steam folder";
            dialog.SelectedPath = SettingsManager.Instance.Settings.Steampath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textGamePath.Text = dialog.SelectedPath;
                SettingsManager.Instance.UpdateGameDir(dialog.SelectedPath);
                if (File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename))) ShowGameVersion();
                else textGameVersion.Text = "Unknown";
            }
        }

        private void buttonGetGameFolder_Click(object sender, EventArgs e)
        {
            SettingsManager.Instance.UpdateGameDir(GetPathFromRegistry("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 899770", "Installlocation"));
            textGamePath.Text = SettingsManager.Instance.Settings.GameDir;
        }

        private void buttonGetSteamFolder_Click(object sender, EventArgs e)
        {
            if (SettingsManager.Instance.Settings.Steampath == "") SettingsManager.Instance.UpdateSteamPath(Utils.GetPathFromRegistry("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath"));

            SettingsManager.Instance.UpdateSteamPath(Path.GetFullPath(SettingsManager.Instance.Settings.Steampath));
        }

        private void checkBoxKeepOpen_CheckedChanged(object sender, EventArgs e)
        {
            SettingsManager.Instance.UpdateKeepOpen(checkBoxKeepOpen.Checked);
        }

        public void ShowStartupMessage()

        {
            var rtfPath = AppDomain.CurrentDomain.BaseDirectory;
            string messageFileToLoad = "";
            bool showMessage = false;
            Logger.Global.Debug($"{settings}  ->>  {AppDomain.CurrentDomain.BaseDirectory}");
            Logger.Global.Debug(rtfPath);
            string dlurl = "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/StartUpMessage.zip";
            try
            {
                if ((File.Exists(rtfPath + @"StartupMessage.zip")) && (!Utils.IsLocalFileUpToDate((rtfPath + @"StartupMessage.zip"), Utils.GetRemoteFileDate(dlurl))))
                {
                    if (!Utils.IsLocalFileUpToDate((rtfPath + @"StartupMessage.zip"), Utils.GetRemoteFileDate(dlurl)))
                    {
                        Utils.RunCurl(dlurl, false, AppDomain.CurrentDomain.BaseDirectory, "StartupMessage.zip");
                        Utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, false);
                        // force display of status message once if newer message found                        
                        showMessage = true;
                    }
                    else if ((File.Exists(rtfPath + @"StartupMessage.zip")))
                    {
                        Utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, false);
                        showMessage = true;
                    }
                }
                else if (!File.Exists(rtfPath + @"StartupMessage.zip"))
                {
                    Utils.RunCurl(dlurl, false, AppDomain.CurrentDomain.BaseDirectory, "StartupMessage.zip");
                    Utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, false);
                    showMessage = true;
                }

                if ((showMessage && File.Exists(rtfPath + @"StartupMessage.rtf")) ||
                    (SettingsManager.Instance.Settings.ShowStartupMessage) && (File.Exists(rtfPath + @"StartupMessage.rtf")))
                {
                    messageFileToLoad = rtfPath + @"StartupMessage.rtf";
                    if (messageFileToLoad != null)
                    {
                        using (var dlg = new StartupDialog(messageFileToLoad,
                                   SettingsManager.Instance.Settings.ShowStartupMessage))
                        {
                            var result = dlg.ShowDialog();

                            SettingsManager.Instance.UpdateShowStartupMessage(dlg.NewShowSetting);
                            checkBoxStartUpMessage.Checked = dlg.NewShowSetting;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Error("Error while loading startup message: " + ex.Message);
            }
        }


        private void checkBoxStartUpMessage_CheckedChanged(object sender, EventArgs e)
        {

            SettingsManager.Instance.UpdateShowStartupMessage(checkBoxStartUpMessage.Checked);


        }

        private void radioKb_CheckedChanged(object sender, EventArgs e)
        {
            if (radioKb.Checked) SettingsManager.Instance.UpdateKbGamePadSelect(0);
        }

        private void radioGamepad_CheckedChanged(object sender, EventArgs e)
        {
            if (radioGamepad.Checked) SettingsManager.Instance.UpdateKbGamePadSelect(1);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (SettingsManager.Instance.Settings.DarkMode)
            {
                ThemeUtils.ApplyLightTheme(this);
                SettingsManager.Instance.UpdateDarkMode(false);
            }
            else { ThemeUtils.ApplyDarkTheme(this); SettingsManager.Instance.UpdateDarkMode(true); }
            toolStripTheme.Text = SettingsManager.Instance.Settings.DarkMode ? "Light theme" : "Dark theme";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var logForm = new LogViewerForm(@"C:\Program Files (x86)\Steam\steamapps\common\Last Epoch\MelonLoader\Latest.log");
            logForm.Show();
        }

        private LogViewerForm? _attachedLogForm;

        private void buttonAttachLog_Click(object sender, EventArgs e)
        {
            string logPath = SettingsManager.Instance.Settings.GameDir + @"\MelonLoader\Latest.log";
            if (_attachedLogForm == null || _attachedLogForm.IsDisposed)
            {
                _attachedLogForm = new LogViewerForm(logPath);
                _attachedLogForm.StartPosition = FormStartPosition.Manual;
                _attachedLogForm.Size = new System.Drawing.Size(
                    SettingsManager.Instance.Settings.LogWindowWidth,
                    SettingsManager.Instance.Settings.LogWindowHeight
                );
                _attachedLogForm.Resize += AttachedLogForm_Resize;
                PositionLogWindow();
                _attachedLogForm.Show();
                this.LocationChanged += Launcherform_LocationChanged;
                this.SizeChanged += Launcherform_LocationChanged;
            }
            else
            {
                _attachedLogForm.Activate();
            }
        }

        private void AttachedLogForm_Resize(object? sender, EventArgs e)
        {
            if (_attachedLogForm != null)
            {
                SettingsManager.Instance.UpdateLogWindowWidth(_attachedLogForm.Width);
                SettingsManager.Instance.UpdateLogWindowHeight(_attachedLogForm.Height);
            }
        }

        private void PositionLogWindow()
        {
            if (_attachedLogForm != null && !_attachedLogForm.IsDisposed)
            {
                _attachedLogForm.Location = new System.Drawing.Point(
                    this.Location.X + this.Width,
                    this.Location.Y
                );
                // Do NOT set _attachedLogForm.Height here; let it keep its own dimensions.
            }
        }

        private void Launcherform_LocationChanged(object? sender, EventArgs e)
        {
            PositionLogWindow();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_attachedLogForm != null && !_attachedLogForm.IsDisposed)
            {
                _attachedLogForm.Close();
            }
            base.OnFormClosing(e);
        }

        // Save position when moved
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            SettingsManager.Instance.UpdateMainWindowPosition(this.Location.X, this.Location.Y);
        }

        private void Launcherform_Load(object sender, EventArgs e)
        {

        }

        private void checkBoxHideConsole_CheckedChanged(object sender, EventArgs e)
        {
            SettingsManager.Instance.UpdateHideConsole(checkBoxHideConsole.Checked);
            SetHideConsole(SettingsManager.Instance.Settings.GameDir + @"\UserData\Loader.cfg");
            Logger.Global.Debug(SettingsManager.Instance.Settings.GameDir + @"\UserData\Loader.cfg");
            Logger.Global.Debug("SHOW CONSOLE" + $"(" + SettingsManager.Instance.Settings.GameDir + @"\UserData\Loader.cfg" + $"---- {SettingsManager.Instance.Settings.HideConsole} / {checkBoxHideConsole.Checked}");
        }

        private void checkBoxStartUpMessage_Click(object sender, EventArgs e)
        {
            SettingsManager.Instance.UpdateShowStartupMessage(checkBoxStartUpMessage.Checked);
            ShowStartupMessage();
        }
    }
}