using DownloadUtils;
using LauncherUtils;
using LEHuDModLauncher.Classlibs;
using LogUtils;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using static LauncherUtils.Utils;

namespace LEHuDModLauncher
{
	public partial class Launcherform : Form
	{
		public static string GameFilename = "Last Epoch.exe";
		public static string VersionFilename = "version.dll";
		public static string VersionBakFilename = "version.dll.bak";

		private Process _gameProcess;
		public Utils Utils = new Utils();
		public FileDownloader fileDownloader = new FileDownloader();

        static string IconPath => Path.Combine(Application.StartupPath, "gooey-daemon_multi2.ico");

        // Tray icon + menu
        private NotifyIcon? trayIcon;
		private ContextMenuStrip? trayMenu;

		public bool userHasInternet = true;

		public AppSettings settings = SettingsManager.Instance.Settings;

		static string Truncate(string s, int len) => s == null ? "" : (s.Length <= len ? s : s.Substring(0, len) + "…");

		public Launcherform()
		{
			try
			{
				InitializeComponent();

				Logger.Global.Debug("Initializing Launcherform");
				//userHasInternet = Utils.UserHasInternet;
				// display a messagebox only once to the user
				if (!SettingsManager.Instance.Settings.ShowedOnce)
				{
					MessageBox.Show("If this is the first time running this app\nand you haven't installed the mod and modloader before,\n" +
					"you may want to click install/reinstall in this launcher app first!!!", "Hello",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
					SettingsManager.Instance.UpdateShowOnce(true);
				}

				// fix occasional bug where form position isn't saved
				if ((SettingsManager.Instance.Settings.MainWindowX < 0) || (SettingsManager.Instance.Settings.MainWindowY < 0))
				{ SettingsManager.Instance.UpdateMainWindowPosition(500, 500); }
				this.StartPosition = FormStartPosition.Manual;
				this.Location = new System.Drawing.Point(
					SettingsManager.Instance.Settings.MainWindowX,
					SettingsManager.Instance.Settings.MainWindowY
				);

				if (SettingsManager.Instance.Settings.DarkMode) ThemeUtils.ApplyDarkTheme(this);
				else ThemeUtils.ApplyLightTheme(this);

				string exeDir = AppDomain.CurrentDomain.BaseDirectory;
				this.Resize += Launcherform_Resize;
				this.Activated += Launcherform_activated;

                var asm = Assembly.GetExecutingAssembly();
                this.Text =
					$"{asm.GetName().Name} - {asm.GetName().Version.Major}.{asm.GetName().Version.Minor} for Last Epoch Hud Mod by Ash, launcher by JP";
				Logger.Global.Info(this.Text);
				Logger.Global.Info($"==========  LE Hud Mod Launcher {asm.GetName().Version.Major}.{asm.GetName().Version.Minor} started ============ ");

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

				if (File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename)))
					ShowGameVersion();
				else textGameVersion.Text = "Unknown";

				ShowStartupMessage(false);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Please Report this error on the LE Hud Mod discord server...\n\n" + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
			}
		}

        public static string? GetGamePath(string steamPath, string gameFolderName)
        {
            string libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
			if (!File.Exists(libraryFile))
				{
				Logger.Global.Error("libraryfolders.vdf not found");
				return null;
				}

            var libraries = new List<string>();


            libraries.Add(steamPath);

            foreach (var line in File.ReadAllLines(libraryFile))
            {
                var match = Regex.Match(line, "\"\\d+\"\\s+\"(.+?)\"");
                if (match.Success)
                {
                    string libPath = match.Groups[1].Value.Replace(@"\\", @"\");
                    libraries.Add(libPath);
                }
            }

            foreach (var lib in libraries)
            {
                string candidate = Path.Combine(lib, "steamapps", "common", gameFolderName);
                if (Directory.Exists(candidate))
                    return candidate;
            }

            return null; // Game not found
        }


        private void Launcherform_activated(object? sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.WindowState = FormWindowState.Normal;
			}

			foreach (Form child in this.MdiChildren)
			{
				child.WindowState = this.WindowState;
			}
		}

		private void Launcherform_Resize(object? sender, EventArgs e)
		{
			foreach (Form child in this.MdiChildren)
			{
				child.WindowState = this.WindowState;
			}
		}

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


		private void CleanupDirs(bool melclean)
		{
			try
			{
				Logger.Global.Info("CleanupDirs started...");

				var melonLoaderPath = Path.Combine(SettingsManager.Instance.Settings.GameDir, "Melonloader");
				var modsPath = Path.Combine(SettingsManager.Instance.Settings.GameDir, "Mods");

				if ((Directory.Exists(melonLoaderPath) && melclean)) Directory.Delete(melonLoaderPath, true);
				else Logger.Global.Debug($"[{nameof(CleanupDirs)}] Melonloader dir not found, skipping delete.");

				if (!Directory.Exists(modsPath))
				{
					Directory.CreateDirectory(modsPath);
					Logger.Global.Info($"[{nameof(CleanupDirs)}] Created Mods directory: {modsPath}");
				}
				else Logger.Global.Debug($"[{nameof(CleanupDirs)}] Mods directory already exists: {modsPath}");
			}
			catch (Exception ex)
			{
				Logger.Global.Error($"CleanupDirs error: {ex}");
				MessageBox.Show($"Failed to clean up directories:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

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
				if (File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename)))
				{
					SetHideConsole(SettingsManager.Instance.Settings.GameDir + @"\\UserData\Loader.cfg");
					ShowGameVersion();
				}
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
			string gamedir = SettingsManager.Instance.Settings.GameDir;
			string gameversion = Path.Combine(gamedir, VersionFilename);
			string gameversionbak = Path.Combine(gamedir, VersionBakFilename);
			try
			{
				if (online)
				{

					if (File.Exists(gameversion))
					{
						if (File.Exists(gameversionbak)) { File.Delete(gameversionbak); }
						File.Move(gameversion, gameversionbak);
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
						// won't properly trigger exit because it looks at steam and not the game process
						
						//process.Exited += GameProcess_Exited;
						process.Start();
						statusStripLabel.Text = "Last Epoch started...";
						//buttonOnline.Enabled = false;
						//buttonOffline.Enabled = false;
					}
					catch (Exception ex)
					{
						Logger.Global.Error($"Failed to start Steam process: {ex}");
						MessageBox.Show($"Failed to start Steam:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				else
				{
					if (!File.Exists(gameversion))
					{
						if (File.Exists(gameversionbak))
						{
							File.Move(gameversionbak, gameversion);
							File.Delete(gameversionbak);
						}
					}
					else if (File.Exists(gameversionbak)) { File.Delete(gameversionbak); }
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
						buttonOnline.Enabled = false;
						buttonOffline.Enabled = false;

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
			buttonOnline.Enabled = true;
			buttonOffline.Enabled = true;
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
			SettingsManager.Instance.Save();
		}

		private void textPath_TextChanged(object sender, EventArgs e)
		{
			SettingsManager.Instance.UpdateGameDir(textGamePath.Text);
			if (File.Exists(Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename)))
			{
				SetHideConsole(SettingsManager.Instance.Settings.GameDir + @"\\UserData\Loader.cfg");
				ShowGameVersion();
			}

		}

		private async void installToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (File.Exists(SettingsManager.ReferenceEquals(SettingsManager.Instance.Settings.GameDir, "") ? "" : Path.Combine(SettingsManager.Instance.Settings.GameDir, GameFilename)) == false)
			{
				MessageBox.Show("Please select a valid Last Epoch game folder first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			SettingsManager.Instance.UpdateErrorCount(0);

			// Pick either keyboard or gamepad version
			if (radioKb.Checked)
			{
				var result = MessageBox.Show("Installing Keyboard version of the mod.\nUse this version if you mainly play the game with your keyboard/mouse.\nProceed?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
				if (result == DialogResult.No) return;
				else Utils.AddDownload(1, "Mod", "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0.0/LastEpoch_Hud.Keyboard.rar", SettingsManager.Instance.Settings.GameDir, SettingsManager.Instance.Settings.GameDir + @"\mods", "LastEpoch_Hud(Keyboard).rar", false, false);
			}
			else
			{
				var result = MessageBox.Show("Installing Gamepad version of the mod.\nUse this version if you mainly play the game with your gamepad.\nProceed?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
				if (result == DialogResult.No) return;
				else Utils.AddDownload(1, "Mod", "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0.0/LastEpoch_Hud.WinGamepad.rar", SettingsManager.Instance.Settings.GameDir, SettingsManager.Instance.Settings.GameDir + @"\mods", "LastEpoch_Hud.WinGamepad.rar", false, false);
			}
			toolStripStatus.Enabled = true;
			toolStripStatus.Enabled = true;
			toolStripStatus.Text = "Installing...";

			var mbresult = CustomMessageBox.Show(
				"Install modloader AND mod or just the mod? You can choose mod only if there has been a new mod release, no need to also update the modloader.",
				"Installation Type",
				"Melonloader and mod",
				"Mod only",
				"Cancel"
			);

			if (mbresult == DialogResult.Yes)
			{
				Utils.AddDownload(2, "Melonloader", "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/Melon.zip", SettingsManager.Instance.Settings.GameDir, SettingsManager.Instance.Settings.GameDir, "Melon.zip", false, false);
				CleanupDirs(true);
			}
			else if (mbresult == DialogResult.No) CleanupDirs(false);

			if (mbresult == DialogResult.Retry) return;


			Utils.StartDownloads();
			Utils.RunExtraction();
			toolStripStatus.Text = "Done!";

			if ((SettingsManager.Instance.Settings.ErrorCount == 0) && (mbresult == DialogResult.Yes))
				MessageBox.Show("All done! You can now click Start game OFFLINE to play the game with the HUD mod enabled.\n\n" +
								"BUT MAKE SURE TO RUN THE GAME ONCE, EXIT WHEN THE GAME IS LOADED.. THIS SHOULD PREVENT MOST COMMON ISSUES."
					, "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			else if ((SettingsManager.Instance.Settings.ErrorCount == 0) && (mbresult == DialogResult.No))
				MessageBox.Show("All done! Just click Start game OFFLINE to play the game!"
					, "Success", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			else if (SettingsManager.Instance.Settings.ErrorCount > 0) MessageBox.Show("Finished with errors. Please check the log.", "Errors occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            var steamPath = GetPathFromRegistry("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath");
            if (!string.IsNullOrEmpty(steamPath))
                SettingsManager.Instance.UpdateSteamPath(steamPath);

            string tempgamedir = GetGamePath(steamPath, "Last Epoch");
            if (tempgamedir != null)
            {
                SettingsManager.Instance.UpdateGameDir(tempgamedir);
                textGamePath.Text = tempgamedir;
            } else MessageBox.Show("Game folder not found. Please select it manually.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

		public void ShowStartupMessage(bool forced = false)

		{
			var rtfPath = AppDomain.CurrentDomain.BaseDirectory;
			string messageFileToLoad = "";
			bool showMessage = false;
			showMessage = forced;
			Logger.Global.Debug($"{settings}  ->>  {AppDomain.CurrentDomain.BaseDirectory}");
			Logger.Global.Debug(rtfPath);
			string dlurl = "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/StartUpMessage.zip";

			try
			{
				if ((File.Exists(rtfPath + @"StartupMessage.zip")) && (!Utils.IsLocalFileUpToDate((rtfPath + @"StartupMessage.zip"), Utils.GetRemoteFileDate(dlurl))))
				{
					if (!Utils.IsLocalFileUpToDate((Path.Combine(rtfPath, "StartupMessage.zip")), Utils.GetRemoteFileDate(dlurl)))
					{
						fileDownloader.DownloadFile(dlurl, AppDomain.CurrentDomain.BaseDirectory, "StartupMessage.zip", 5);
						Utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, false);
						// force display of status message once if newer message found                        
						showMessage = true;
					}
					else if ((File.Exists(Path.Combine(rtfPath, "StartupMessage.zip"))))
					{
						Utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, false);
						showMessage = true;
					}
				}
				else if (!File.Exists(Path.Combine(rtfPath, "StartupMessage.zip")))
				{
					fileDownloader.DownloadFile(dlurl, AppDomain.CurrentDomain.BaseDirectory, "StartupMessage.zip", 5);
					Utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, false);
					showMessage = true;
				}
				else if ((File.Exists(Path.Combine(rtfPath, "StartupMessage.zip"))))
					Utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.BaseDirectory, false);
			}
			catch (Exception ex)
			{
				Logger.Global.Error("Error while loading startup message: " + ex.Message);
			}

			if ((showMessage && File.Exists(Path.Combine(rtfPath, "StartupMessage.rtf")) ||
						(SettingsManager.Instance.Settings.ShowStartupMessage) && (File.Exists(rtfPath + @"StartupMessage.rtf"))))
			{
				messageFileToLoad = Path.Combine(rtfPath, "StartupMessage.rtf");
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
					this.Location.X + this.Width -16,
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
		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged(e);
			SettingsManager.Instance.UpdateMainWindowPosition(this.Location.X, this.Location.Y);
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
			ShowStartupMessage(false);
		}


		private void buttonStartupMessage_Click(object sender, EventArgs e)
		{
			ShowStartupMessage(true);
		}
	}
}