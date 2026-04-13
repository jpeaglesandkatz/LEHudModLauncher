// =========================================================================================================================
// LE HudModLauncher itself and every part of it is released under the MIT license, this does not include the HudMod itself
// =========================================================================================================================

using AssetsTools.NET.Extra;
using ClassUtils;
using DownloadLib;
using LEHuDModLauncher.Classlibs;
using LogUtils;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using SettingsManager;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static DownloadLib.FileDownloader;
using static System.IO.File;
using static System.Windows.Forms.MessageBoxButtons;
using static System.Windows.Forms.MessageBoxIcon;

namespace LEHuDModLauncher;

public partial class Launcherform : MaterialForm
{
    private const string GameFilename = "Last Epoch.exe";
    private const string VersionFilename = "version.dll";
    private const string VersionBakFilename = "version.dll.bak";

    private Process? _gameProcess;
    private readonly Utils _utils = new();
    private readonly FileDownloader _fileDownloader = new();

    private readonly UpdateChecker _updater = new();
    private readonly AssetsManager _assetsManager = new();

    public MaterialSkinManager? _skinManager = MaterialSkinManager.Instance;
    private bool _isDarkTheme;

    private LogViewerForm? _attachedLogForm;
    public bool UserHasInternet = true;
    public DownloadList dllist;


    private readonly Bitmap _imagecheck = Properties.Resources.check_64dp_green;
    private readonly Bitmap _imagecross = Properties.Resources.close_64dp_red;

    public Launcherform()
    {
        InitializeComponent();
        Visible = false;
        materialTextBoxPath.SafeSetText(Config.Instance.Settings.GameDir);
        checkBoxKeepOpen.SafeSetChecked(Config.Instance.Settings.KeepOpen);
        checkBoxHideConsole.SafeSetChecked(Config.Instance.Settings.HideConsole);
        toolstripAutoUpdate.SafeSetChecked(Config.Instance.Settings.AutoUpdate);
        showStartupMessageToolStripMenuItem.SafeSetChecked(Config.Instance.Settings.ShowStartupMessage);
        if (Config.Instance.Settings.KbGamePadSelect == 0) radioKb.SafeSelect(); else radioGamepad.SafeSelect();
        _isDarkTheme = Config.Instance.Settings.DarkMode;
        toolStripStatus.SafeSetEnabled(true);
        _utils.SetHideConsole(Path.Combine(Config.Instance.Settings.GameDir + @"\UserData\Loader.cfg", ""));

        GetGamePath();

        DownloadStuff();

        if (Config.Instance.Settings.AutoUpdate)
        {
            InstallMelonLoader(!IsMelonValid());
            InstallMod(Config.Instance.Settings.AutoUpdate);
        }

        ShowGameVersion();

        ShowStartupMessage(Config.Instance.Settings.ShowStartupMessage);

        if (Config.Instance.Settings.AutoUpdate) _updater.CheckForUpdate(false);

        _ = IsMelonValid();
        _ = IsModInstalled();
    }

    private void Showifvalidgamefolder()

    {
        pictureCheck.Image = Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)) ? _imagecheck : _imagecross;
    }

    private bool IsMelonValid()

    {
        bool shaCheck;
        if (Exists(Path.Combine(Config.Instance.Settings.GameDir + @"\MelonLoader\net6\MelonLoader.dll", "")))
        {
            shaCheck = VerifySha256(Path.Combine(Config.Instance.Settings.GameDir + @"\MelonLoader\net35\MelonLoader.dll", ""),
                "8825deded3c5d882695c01215e57493fb05af8cf5c406753cfa2999f9222c68b");
            // New sha256 for official 0.72 version of Melon loader (29/3/2026)
            //shaCheck = VerifySha256(Path.Combine(Config.Instance.Settings.GameDir + @"\MelonLoader\net35\MelonLoader.dll", ""),
            //    "9DA4175149E7EBA5F67511461A658D15CD062C287E34FC9A03B1EFC8FDC8D21C");

        }
        else
        {
            pictureCheckLoader.Image = Properties.Resources.close_64dp_red;
            return false;
        }

        if (shaCheck)
        {
            pictureCheckLoader.Image = Properties.Resources.check_64dp_green;
            return true;
        }

        pictureCheckLoader.Image = Properties.Resources.close_64dp_red;

        return false;
    }


    private bool DownloadStuff()

    {
        if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename))) return false;
        var gamePath = Config.Instance.Settings.GameDir;
        FileDownloader.DownloadList? dllist;

        if (!Directory.Exists(Path.Combine(gamePath, "modsdl_do_not_delete")))
            Directory.CreateDirectory(Path.Combine(gamePath, "modsdl_do_not_delete"));
        try
        {
            // get download links
            dllist = AddDownloadsFromJson();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Errorr " + ex.Message);
            Logger.Global.Error($"Failed to download dllist {ex.Message}\n{ex.StackTrace}");
            toolStripStatus.SafeSetEnabled(true);
            toolStripStatus.SafeSetText("Error getting download list");
            return false;
        }
        toolStripStatus.SafeSetEnabled(true);
        //toolStripStatus.SafeSetText(dllist?.Files.Count + " files to download");
        for (int i = 0; i < dllist?.Files.Count; i++)
        {
            try
            {
                if (dllist != null && !_utils.IsLocalFileUpToDate(Path.Combine(gamePath, "modsdl_do_not_delete", dllist.Files[i].Filename), dllist.Files[i].Url))
                    _fileDownloader.DownloadFile(dllist.Files[i].Url, Path.Combine(gamePath, "modsdl_do_not_delete"), dllist.Files[i].Filename);
            }
            catch (Exception ex)
            {
                toolStripStatus.SafeSetEnabled(true);
                toolStripStatus.SafeSetText("Error downloading " + dllist.Files[i].Filename);
                Logger.Global.Error($"Error downloading file {dllist.Files[i].Name} {ex.Message}\n{ex.StackTrace}");
            }
        }
        if (Exists(Path.Combine(Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete"), "StartupMessage.rar")))
            _utils.ExtractFileLib("StartupMessage.rar", Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete"),
                Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete"));
        
        
        return true;
    }


    private void CleanupDirs(bool melclean)
    {
        try
        {
            Logger.Global.Info("CleanupDirs started...");

            var melonLoaderPath = Path.Combine(Config.Instance.Settings.GameDir, "MelonLoader");
            var modsPath = Path.Combine(Config.Instance.Settings.GameDir, "Mods");

            if (melclean)
            {
                if (Directory.Exists(melonLoaderPath))
                {
                    Directory.Delete(melonLoaderPath, true);
                    if (File.Exists(Path.Combine(Config.Instance.Settings.GameDir, "version.dll.bak"))) File.Delete(Path.Combine(Config.Instance.Settings.GameDir, "version.dll.bak"));
                    if (File.Exists(Path.Combine(Config.Instance.Settings.GameDir, "version.dll"))) File.Delete(Path.Combine(Config.Instance.Settings.GameDir, "version.dll"));
                    Logger.Global.Info($"Deleted directory: {melonLoaderPath} and deleted version.dll and version.dll.bak");
                }
                else
                {
                    Logger.Global.Debug($"[{nameof(CleanupDirs)}] MelonLoader dir not found, skipping delete.");
                }
            }

            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
                Logger.Global.Info($"Created directory: {modsPath}");
            }
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"CleanupDirs error: {ex.Message}\n{ex.StackTrace}");
        }
    }


    private bool InstallMelonLoader(bool forceinst)

    {
        var gamePath = Config.Instance.Settings.GameDir;
        if (!forceinst) return false;

        try
        {
            var melonZip = Path.Combine(gamePath, "modsdl_do_not_delete", "Melon.zip");
            if (!File.Exists(melonZip))
            {
                Logger.Global.Debug($"InstallMelonLoader: {melonZip} not found");
                return false;
            }

            toolStripStatus.SafeSetEnabled(true);
            toolStripStatus.SafeSetText(@"Installing Melonloader....");
            CleanupDirs(forceinst); // force erase existing melon loader
            Logger.Global.Debug($"Extracting melonloader... to {gamePath}");

            _utils.ExtractFileLib("Melon.zip", Path.GetFullPath(Path.Combine(gamePath, "modsdl_do_not_delete")), Path.Combine(gamePath), false);

            toolStripStatus.SafeSetText(@"Melonloader installation finished. Running the game after install may take a while....");
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"Error installing Melonloader: {ex.Message}\n{ex.StackTrace}");
            toolStripStatus.SafeSetEnabled(false);
            return false;
        }

        // Re-check installation
        return IsMelonValid();
    }


    private bool IsModInstalled()
    {
        pictureModInstalled.Image = Properties.Resources.close_64dp_red;
        if (Exists(Path.Combine(Config.Instance.Settings.GameDir + @"\Mods\LastEpoch_Hud.dll", "")))
        {
            var modfileinfo = new FileInfo(Path.Combine(Config.Instance.Settings.GameDir + @"\Mods\LastEpoch_Hud.dll", ""));
            pictureModInstalled.Image = Properties.Resources.check_64dp_green;
            labelVersion.SafeSetText($"({modfileinfo.LastWriteTime.ToString(CultureInfo.CurrentCulture)})");

            return true;
        }
        return false;

    }

    private bool InstallMod(bool forceinst)
    {
        if (!forceinst && !Config.Instance.Settings.AutoUpdate) return false;
        if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename))) return false;
        if (Config.Instance.Settings.AutoUpdate || forceinst)
        {
            try
            {


                if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, "Mods")))
                    Directory.CreateDirectory(Path.Combine(Config.Instance.Settings.GameDir, "Mods"));
                Console.WriteLine("Installing mod");
                switch (radioKb.Checked)
                {
                    case true when Exists(Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete", "LastEpoch_Hud(Keyboard).rar")):
                        {
                            _utils.ExtractFileLib("LastEpoch_Hud(Keyboard).rar", Path.GetFullPath(Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete")),
                                   Path.GetFullPath(Path.Combine(Config.Instance.Settings.GameDir, "Mods")));
                            toolStripStatus.SafeSetText("Keyboard version of the HUD Mod is now installed");
                            Console.WriteLine("Keyboard version of the HUD Mod is now installed");
                            break;
                        }
                    case false when Exists(Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete", "LastEpoch_Hud(WinGamepad).rar")):
                        {

                            _utils.ExtractFileLib("LastEpoch_Hud(WinGamepad).rar", Path.GetFullPath(Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete")),
                                   Path.GetFullPath(Path.Combine(Config.Instance.Settings.GameDir, "Mods")));
                            toolStripStatus.SafeSetText("Gamepad version of the HUD Mod is now installed");
                            Console.WriteLine("Gamepad version of the HUD Mod is now installed");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Error installing mod {ex}");
                toolStripStatus.SafeSetEnabled(false);
                toolStripStatus.SafeSetText("Error installing mod");
                return false;
            }
            // Copy the Desktop.Robot.dll to userlibs
            try
            {
                if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, "UserLibs", "Desktop.Robot.dll")))
                {
                    Directory.CreateDirectory(Path.Combine(Config.Instance.Settings.GameDir, "UserLibs"));
                    _utils.ExtractFileLib("UserLibs.rar", Path.GetFullPath(Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete")),
                               Path.GetFullPath(Path.Combine(Config.Instance.Settings.GameDir, "UserLibs")));
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Error installing Desktop.Robot.dll {ex}");
                return false;
            }


        }
        return IsModInstalled();
    }

    private void NewApplyTheme()
    {
        _isDarkTheme = Config.Instance.Settings.DarkMode;
        if (_isDarkTheme)
        {
            toolStripOptions.Image = Properties.Resources.settings_white;
            toolStripTheme.Image = Properties.Resources.side_navigation_white;
            buttonAttachLog.Image = Properties.Resources.docs_96dp_white;
            buttonStartupMessage.Image = Properties.Resources.info_96dp_white;
            if (_skinManager == null) return;
            _skinManager.Theme = MaterialSkinManager.Themes.DARK;
            _skinManager.ColorScheme = new ColorScheme(
                Primary.Grey800, Primary.Grey900,
                Primary.Grey500, Accent.DeepOrange700,
                TextShade.WHITE
            );
        }
        else
        {
            toolStripOptions.Image = Properties.Resources.settings_dark;
            toolStripTheme.Image = Properties.Resources.side_navigation_dark;
            buttonAttachLog.Image = Properties.Resources.docs_96dp_black;
            buttonStartupMessage.Image = Properties.Resources.info_96dp_black;
            if (_skinManager == null) return;
            _skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            _skinManager.ColorScheme = new ColorScheme(
                Primary.Cyan900, Primary.Cyan700,
                Primary.Cyan800, Accent.DeepOrange700,
                TextShade.WHITE
            );
        }
    }

    private static string? GetGamePath(string steamPath, string gameFolderName)
    {
        var libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!Exists(libraryFile))
        {
            Logger.Global.Error("libraryfolders.vdf not found");
            return null;
        }

        var libraries = new List<string> { steamPath };


        libraries.AddRange(ReadAllLines(libraryFile)
            .Select(line => MyRegex().Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value.Replace(@"\\", @"\")));

        return libraries.Select(lib => Path.Combine(lib, "steamapps", "common", gameFolderName))
            .FirstOrDefault(Directory.Exists);
    }

    /// <summary>
    /// Search steam installation and libraries for localconfig.vdf files and ensure the
    /// DefaultLaunchOption entry for app id "899770" contains the specified key with value "1".
    /// </summary>
    /// <param name="steamPath">Path to Steam install (root).</param>
    /// <param name="appId">App id to modify (defaults to "899770").</param>
    /// <param name="optionKey">The option key to set (defaults to "96be4408").</param>
    /// <param name="optionValue">The value to enforce (defaults to "1").</param>
    /// <returns>True if at least one file was modified or already contained the expected value; false if none found.</returns>
    public static bool EnsureDefaultLaunchOptionInSteam(string steamPath, string appId = "899770", string optionKey = "96be4408", string optionValue = "1")
    {
        try
        {
            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath)) return false;

            List<string> localConfigPaths;
            try
            {
                // Recursively search for any localconfig.vdf under the steam base path
                localConfigPaths = Directory.EnumerateFiles(steamPath, "localconfig.vdf", SearchOption.AllDirectories)
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Error enumerating files under {steamPath}: {ex.Message}");
                return false;
            }

            if (!localConfigPaths.Any())
            {
                Logger.Global.Debug($"No localconfig.vdf files found under {steamPath}");
                return false;
            }

            var anyChanged = false;
            foreach (var localConfigPath in localConfigPaths)
            {
                try
                {
                    var changed = ModifyLocalConfigFile(localConfigPath, appId, optionKey, optionValue);
                    if (changed) anyChanged = true;
                }
                catch (Exception ex)
                {
                    Logger.Global.Error($"Failed to modify {localConfigPath}: {ex.Message}");
                }
            }

            return anyChanged;
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"EnsureDefaultLaunchOptionInSteam error: {ex.Message}");
            return false;
        }
    }

    private static bool ModifyLocalConfigFile(string filePath, string appId, string optionKey, string optionValue)
    {
        var lines = File.ReadAllLines(filePath).ToList();
        var n = lines.Count;
        for (int i = 0; i < n; i++)
        {
            var trimmed = lines[i].Trim();
            if (!trimmed.StartsWith("\"apps\"")) continue;

            // find opening brace for apps
            int j = i + 1;
            while (j < n && !lines[j].Contains("{")) j++;
            if (j >= n) continue;

            int appsLevel = 1;
            int k = j + 1;
            while (k < n && appsLevel > 0)
            {
                var line = lines[k];
                if (line.Contains("{")) appsLevel++;
                if (line.Contains("}")) appsLevel--;

                // look for app id line when we are directly under apps block
                if (appsLevel == 1 && line.Trim().StartsWith("\"" + appId + "\""))
                {
                    // find app block start
                    int appStart = k;
                    int m = appStart + 1;
                    while (m < n && !lines[m].Contains("{")) m++;
                    if (m >= n) break;

                    int appLevel = 1;
                    int p = m + 1;
                    while (p < n && appLevel > 0)
                    {
                        var al = lines[p];
                        if (al.Contains("{")) appLevel++;
                        if (al.Contains("}")) appLevel--;

                        // look for DefaultLaunchOption key inside this app block
                        if (appLevel == 1 && al.Trim().StartsWith("\"DefaultLaunchOption\""))
                        {
                            // find its block
                            int dloStart = p;
                            int q = dloStart + 1;
                            while (q < n && !lines[q].Contains("{")) q++;
                            if (q >= n) break;

                            int dloLevel = 1;
                            int r = q + 1;
                            while (r < n && dloLevel > 0)
                            {
                                var dl = lines[r];
                                if (dl.Contains("{")) dloLevel++;
                                if (dl.Contains("}")) dloLevel--;

                                // find the option key line
                                var dltrim = dl.Trim();
                                if (dltrim.StartsWith("\"" + optionKey + "\""))
                                {
                                    // replace the line keeping original leading whitespace
                                    var leading = lines[r].Substring(0, lines[r].IndexOf(dltrim, StringComparison.Ordinal));
                                    lines[r] = leading + "\"" + optionKey + "\"\t\t\"" + optionValue + "\"";
                                    File.WriteAllLines(filePath, lines);
                                    Logger.Global.Info($"Updated {optionKey} in {filePath} to {optionValue}");
                                    return true;
                                }

                                r++;
                            }
                        }

                        p++;
                    }
                }

                k++;
            }
        }

        return false;
    }


    private void Launcherform_activated(object? sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }

        foreach (var child in MdiChildren)
        {
            child.WindowState = WindowState;
        }
    }

    private void Launcherform_Resize(object? sender, EventArgs e)
    {
        foreach (var child in MdiChildren)
        {
            child.WindowState = WindowState;
        }
    }

    private void ShowGameVersion()
    {
        var gameversion = NewUnityHelper.ReadGameInfo(_assetsManager, Path.Combine(Config.Instance.Settings.GameDir, "Last Epoch_Data"));
        if (gameversion != null) textGameVersion.SafeSetText(gameversion);
        Showifvalidgamefolder();
    }



    private void StartGame(bool online)
    {
        if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))
        {
            MessageBox.Show("Set a valid game folder first!!!", "Error", OK, Error);
            return;
        }

        var gamedir = Config.Instance.Settings.GameDir;
        var gameversion = Path.Combine(gamedir, VersionFilename);
        var gameversionbak = Path.Combine(gamedir, VersionBakFilename);
        try
        {
            if (online)
            {
                if (Exists(gameversion))
                {
                    if (Exists(gameversionbak))
                    {
                        Delete(gameversionbak);
                    }

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
                    process.Start();
                    statusStripLabel.SafeSetText("Last Epoch started...");
                }
                catch (Exception ex)
                {
                    Logger.Global.Error($"Failed to start Steam process: {ex}");
                    MessageBox.Show($"Failed to start Steam:\n{ex.Message}", "Error", OK, Error);
                }
            }
            else
            {
                if (!Exists(gameversion))
                {
                    if (Exists(gameversionbak))
                    {
                        File.Move(gameversionbak, gameversion);
                        Delete(gameversionbak);
                    }
                }
                else if (Exists(gameversionbak))
                {
                    Delete(gameversionbak);
                }

                Directory.SetCurrentDirectory(gamedir);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = $"\"{gamedir}\\{GameFilename}\"",
                    //FileName = $"steam://rungameid/{899770}",
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
                    statusStripLabel.SafeSetText(" Last Epoch started...");
                    buttonOnline.Enabled = false;
                    buttonOffline.Enabled = false;
                    radioKb.Enabled = false;
                    radioGamepad.Enabled = false;
                    buttonGetGameFolder.Enabled = false;
                    materialTextBoxPath.Enabled = false;
                    materialButtonBrowse.Enabled = false;
                }
                catch (Exception ex)
                {
                    Logger.Global.Error($"Failed to start game process: {ex}");
                    MessageBox.Show($"Failed to start Last Epoch:\n{ex.Message}", "Error", OK, Error);
                }
            }

            if (!Config.Instance.Settings.KeepOpen) Application.Exit();
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"StartGame error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void GameProcess_Exited(object? sender, EventArgs e)
    {
        // Ensure UI work runs on UI thread and return immediately on background thread.
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => GameProcess_Exited(sender, e)));
            return;
        }

        statusStripLabel.SafeSetText("Game Exited!");
        buttonOnline.Enabled = true;
        buttonOffline.Enabled = true;
        radioKb.Enabled = true;
        radioGamepad.Enabled = true;
        buttonGetGameFolder.Enabled = true;
        materialTextBoxPath.Enabled = true;
        materialButtonBrowse.Enabled = true;

        if (_gameProcess != null)
        {
            _gameProcess.Exited -= GameProcess_Exited;
            _gameProcess = null;
        }
    }

    private void launcherform_FormClosing(object sender, FormClosingEventArgs e)
    {
        Config.Instance.Save();
    }

    private FileDownloader.DownloadList AddDownloadsFromJson()
    {
        const string dlurl = "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/dllistnew.json";

        try
        {
            _fileDownloader.DownloadFile(dlurl, Config.Instance.Settings.TmpDownloadFolder, "dllistnew.json");
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"Failed to download dllist {ex.Message}\n{ex.StackTrace}");
            return null;
        }

        var reader = new StreamReader(Path.Combine(Config.Instance.Settings.TmpDownloadFolder, "dllistnew.json"));
        try
        {
            var downloadList = JsonConvert.DeserializeObject<FileDownloader.DownloadList>(reader.ReadToEnd());
            //File.Delete(Path.Combine(Path.GetTempPath(), "dllistnew.json"));
            return downloadList;
        }
        finally
        {
            reader.Dispose();
        }
    }

    private void ShowStartupMessage(bool forced = false)
    {
        var rtfPath = Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete");
        var showMessage = forced;
        var messageFileToLoad = Path.Combine(Config.Instance.Settings.GameDir, "modsdl_do_not_delete", "StartupMessage.rtf");



        if ((!showMessage || !Exists(messageFileToLoad) &&
            (!Config.Instance.Settings.ShowStartupMessage ||
             !Exists(messageFileToLoad)))) return;

        var dlg = new StartupDialog(messageFileToLoad,
            Config.Instance.Settings.ShowStartupMessage);
        try
        {
            _ = dlg.ShowDialog();

            Config.Instance.UpdateShowStartupMessage(dlg.NewShowSetting);
            showStartupMessageToolStripMenuItem.SafeSetChecked(dlg.NewShowSetting);
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"Error showing startup message: {ex.Message}\n{ex.StackTrace}");
            MessageBox.Show($"Error showing startup message:\n{ex.Message}", "Error", OK, Error);
        }
        finally
        {
            dlg.Dispose();
        }
    }

    private void toolStripMenuItem1_Click(object sender, EventArgs e)
    {
        _isDarkTheme = !_isDarkTheme;
        Config.Instance.UpdateDarkMode(_isDarkTheme);
        toolStripTheme.SafeSetText(Config.Instance.Settings.DarkMode ? "Light Theme" : "Dark Theme");
        NewApplyTheme();
    }


    private void AttachedLogForm_Resize(object? sender, EventArgs e)
    {
        if (_attachedLogForm == null) return;
        Config.Instance.UpdateLogWindowWidth(_attachedLogForm.Width);
        Config.Instance.UpdateLogWindowHeight(_attachedLogForm.Height);
    }

    private void PositionLogWindow()
    {
        if (_attachedLogForm is { IsDisposed: false })
        {
            _attachedLogForm.Location = new Point(
                Location.X + Width - 4,
                Location.Y
            );
        }
    }

    private void Launcherform_LocationChanged(object? sender, EventArgs e)
    {
        PositionLogWindow();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_attachedLogForm is { IsDisposed: false })
        {
            _attachedLogForm.Close();
        }

        base.OnFormClosing(e);
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        Config.Instance.UpdateMainWindowPosition(Location.X, Location.Y);
    }



    private void toolstripAutoUpdate_CheckStateChanged(object sender, EventArgs e)
    {
        Config.Instance.UpdateAutoUpdate(toolstripAutoUpdate.Checked);
    }

    private void materialButton1_Click(object sender, EventArgs e)
    {
        toolStripStatus.SafeSetText("");
        StartGame(false);
    }

    private void materialButton2_Click(object sender, EventArgs e)
    {
        StartGame(true);
    }

    private void materialTextBoxPath_TextChanged(object sender, EventArgs e)
    {

        if (Exists(Path.Combine(materialTextBoxPath.Text, GameFilename))) Config.Instance.UpdateGameDir(Path.Combine(materialTextBoxPath.Text, ""));
        else return;
        _utils.SetHideConsole(Path.Combine(Config.Instance.Settings.GameDir + @"\\UserData\Loader.cfg", ""));
        IsMelonValid();
        IsModInstalled();
        Showifvalidgamefolder();
    }

    private void materialButtonBrowse_Click(object sender, EventArgs e)
    {
        toolStripStatus.SafeSetText("");
        try
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Select Last Epoch game folder";
            dialog.SelectedPath = Config.Instance.Settings.GameDir;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                materialTextBoxPath.SafeSetText(dialog.SelectedPath);
                Config.Instance.UpdateGameDir(dialog.SelectedPath);
                ShowGameVersion();
            }

            if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))
            {
                _utils.SetHideConsole(Path.Combine(Config.Instance.Settings.GameDir + @"\\UserData\Loader.cfg", ""));
            }
            else textGameVersion.SafeSetText("Unknown");
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"buttonBrowse_Click error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void GetGamePath()

    {
        var steamPath = Utils.GetPathFromRegistry("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath");
        if (!string.IsNullOrEmpty(steamPath))
            Config.Instance.UpdateSteamPath(steamPath);

        if (steamPath != null)
        {
            var tempgamedir = GetGamePath(steamPath, "Last Epoch");
            if (tempgamedir != null)
            {
                Config.Instance.UpdateGameDir(tempgamedir);
                if (File.Exists(Path.Combine(tempgamedir, GameFilename)) && Config.Instance.Settings.AutoUpdate)
                    // _ = CheckModsForUpdate(true, true, false, false);
                    //textGamePath.Text = tempgamedir;
                    materialTextBoxPath.SafeSetText(tempgamedir);
                ShowGameVersion();
            }
            else
            {
                //MessageBox.Show("Game folder not found. Please select it manually.", "Error", OK, Error);
                Logger.Global.Error("Game folder not found.... ");
            }
        }

        Showifvalidgamefolder();
        _ = IsMelonValid();
        _ = IsModInstalled();


    }

    private void buttonGetGameFolder_Click_1(object sender, EventArgs e)
    {
        GetGamePath();
    }

    private void radioKb_CheckedChanged_1(object sender, EventArgs e)
    {
        if (radioKb.Checked) Config.Instance.UpdateKbGamePadSelect(0);
    }


    public class UpdateChecker
    {
        public void CheckForUpdate(bool reportstatus = false)
        {
            var fileDownloader = new FileDownloader();

            try
            {
                //var exePath = Assembly.GetExecutingAssembly().Location;
                //var exeFolder = Path.GetDirectoryName(exePath);

                const string updateJsonUrl =
                    "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/update.json";

                fileDownloader.DownloadFile(updateJsonUrl, Config.Instance.Settings.TmpDownloadFolder,
                    "update.json");
                var json = ReadAllText(Path.Combine(Config.Instance.Settings.TmpDownloadFolder, "update.json"));

                var updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(json);

                if (updateInfo == null)
                    return;
                //Version currentVersion = new Version(Application.ProductVersion);
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version!;
                var latestVersion = new Version(updateInfo.Version);

                Delete(Path.Combine(Config.Instance.Settings.TmpDownloadFolder, "update.json"));
                if (latestVersion > currentVersion)
                {
                    Logger.Global.Info($"New Launcher version available: {currentVersion.Major}.{currentVersion.Minor} ->\n{latestVersion.Major}.{latestVersion.Minor}");
                    // Show the update prompt on the UI thread and do not block this background task.
                    // If the user accepts, start the download — otherwise do nothing.
                    if (Application.OpenForms.Count > 0)
                    {
                        var owner = Application.OpenForms[0];
                        owner.BeginInvoke(new Action(async () =>
                        {
                            var res = MessageBox.Show(owner,
                                $"A new version ({latestVersion}) of the launcher/installer is available. You are on {currentVersion.Major}.{currentVersion.Minor}.\n\nClose the launcher first and do you want to update now?",
                                "Update Available",
                                YesNo,
                                Information);

                            if (res == DialogResult.Yes)
                            {
                                Logger.Global.Info("Downloading new launcher setup...");
                                await DownloadAndRunInstaller(updateInfo.InstallerUrl);
                            }
                        }));
                    }
                    else
                    {
                        // Fallback: show message box on a new STA thread so we don't block this async task.
                        var t = new System.Threading.Thread(() =>
                        {
                            var res = MessageBox.Show(
                                $"A new version ({latestVersion}) of the launcher/installer is available. You are on {currentVersion.Major}.{currentVersion.Minor}.\n\nClose the launcher and do you want to update now?",
                                "Update Available",
                                YesNo,
                                Information);
                            if (res == DialogResult.Yes)
                            {
                                Logger.Global.Info("Downloading new launcher setup...");
                                // Run the async downloader and block this STA thread until it completes.
                                DownloadAndRunInstaller(updateInfo.InstallerUrl).GetAwaiter().GetResult();
                            }
                        });
                        t.SetApartmentState(System.Threading.ApartmentState.STA);
                        t.IsBackground = true;
                        t.Start();
                    }
                }
                else
                    if (reportstatus)
                    {
                        Logger.Global.Info($"Already on latest launcher: {currentVersion.Major}.{currentVersion.Minor}");
                        MessageBox.Show("You have the latest version of the launcher/installer", "No Update", OK, Information);
                    }
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Launcher Update check failed: {ex.Message}\n{ex.StackTrace}");

            }
        }

        private static async Task DownloadAndRunInstaller(string installerUrl)
        {
            try
            {
                var tempFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(installerUrl));
                var fileDownloader = new FileDownloader();
                fileDownloader.DownloadFile(installerUrl, Path.GetTempPath(),
                    Path.GetFileName(installerUrl));

                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });

                Application.Exit();
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Error! {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    public class UpdateInfo(string version, string installerUrl)
    {
        public string Version { get; } = version;
        public string InstallerUrl { get; } = installerUrl;
    }


    private void toolstripForceModUpdate_Click(object sender, EventArgs e)
    {
        if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))

        {
            InstallMelonLoader(true);
        }
    }

    private void radioKb_Click(object sender, EventArgs e)
    {
        InstallMod(true);
    }

    private void radioGamepad_Click(object sender, EventArgs e)
    {
        InstallMod(true);
    }

    private static bool VerifySha256(string filePath, string expectedHash)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Logger.Global.Debug($"VerifySha256: file not found: {filePath}");
                return false;
            }

            using var stream = File.OpenRead(filePath);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(stream);
            var actual = Convert.ToHexString(hash).ToUpperInvariant();
            return string.Equals(actual, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"VerifySha256 failed for {filePath}: {ex.Message}");
            return false;
        }
    }

    public static void CreateSha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = OpenRead(filePath);
        var hash = sha.ComputeHash(stream);
        var actual = Convert.ToHexString(hash).ToUpperInvariant();
        try
        {
            // Try to set clipboard on current thread (usually UI thread)
            System.Windows.Forms.Clipboard.SetText(actual);
            MessageBox.Show($"SHA256: {actual}\n\nCopied to clipboard.", "File Hash", OK, Information);
        }
        catch (Exception)
        {
            // Fallback: use an STA thread to set the clipboard if current thread is not STA
            var t = new System.Threading.Thread(() =>
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(actual);
                }
                catch
                {
                    // ignore
                }
            });
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
            t.Join();
            MessageBox.Show($"SHA256: {actual}\n\nCopied to clipboard.", "File Hash", OK, Information);
        }
    }


    private void checkBoxKeepOpen_CheckStateChanged(object sender, EventArgs e)
    {
        Config.Instance.UpdateKeepOpen(checkBoxKeepOpen.Checked);
    }



    private void checkBoxHideConsole_CheckStateChanged(object sender, EventArgs e)
    {
        Config.Instance.UpdateHideConsole(checkBoxHideConsole.Checked);
        _utils.SetHideConsole(Path.Combine(Config.Instance.Settings.GameDir + @"\UserData\Loader.cfg", ""));
        Logger.Global.Debug(Path.Combine(Config.Instance.Settings.GameDir + @"\UserData\Loader.cfg", ""));
    }

    private void buttonAttachLog_Click_2(object sender, EventArgs e)
    {
        var logPath = Path.Combine(Config.Instance.Settings.GameDir + @"\MelonLoader\Latest.log", "");
        if (_attachedLogForm == null || _attachedLogForm.IsDisposed)
        {
            _attachedLogForm = new LogViewerForm(logPath);
            _attachedLogForm.StartPosition = FormStartPosition.Manual;
            _attachedLogForm.Size = new Size(
                Config.Instance.Settings.LogWindowWidth,
                Config.Instance.Settings.LogWindowHeight
            );
            _attachedLogForm.Resize += AttachedLogForm_Resize;
            PositionLogWindow();
            _attachedLogForm.Show();
            LocationChanged += Launcherform_LocationChanged;
            SizeChanged += Launcherform_LocationChanged;
        }
        else
        {
            _attachedLogForm.Activate();
        }
    }

    private void buttonStartupMessage_Click_1(object sender, EventArgs e)
    {
        ShowStartupMessage(true);
        showStartupMessageToolStripMenuItem.SafeSetChecked(Config.Instance.Settings.ShowStartupMessage);
    }

    private void forceInstallModToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))

        {
            InstallMod(true);
        }
    }

    private void radioGamepad_CheckedChanged_1(object sender, EventArgs e)
    {
        if (radioGamepad.Checked) Config.Instance.UpdateKbGamePadSelect(1);
    }

    [GeneratedRegex("\"\\d+\"\\s+\"(.+?)\"")]
    private static partial Regex MyRegex();

    private void Launcherform_Load(object sender, EventArgs e)
    {
        var asm = Assembly.GetExecutingAssembly();
        var version = asm.GetName().Version;
        if (version != null)
        {
            this.SafeSetText($"{asm.GetName().Name} - {version.Major}.{version.Minor} for Last Epoch Hud Mod by Ash, launcher by JP");
            Logger.Global.Info(Text);
            Logger.Global.Info(
                $"==========  LE Hud Mod Launcher {version.Major}.{version.Minor} started ============ ");
        }

        if (_skinManager != null)
        {
            _skinManager.EnforceBackcolorOnAllComponents = true;
            _skinManager.AddFormToManage(this);
            NewApplyTheme();
        }
        if (Config.Instance.Settings.MainWindowX < 0 || Config.Instance.Settings.MainWindowY < 0)
        {
            Config.Instance.UpdateMainWindowPosition(500, 500);
        }
        StartPosition = FormStartPosition.Manual;
        Location = new Point(
            Config.Instance.Settings.MainWindowX,
            Config.Instance.Settings.MainWindowY
        );

        Resize += Launcherform_Resize;
        Activated += Launcherform_activated;
        toolStripTheme.SafeSetText(Config.Instance.Settings.DarkMode ? "Light Theme" : "Dark Theme");
        Icon = Properties.Resources.gooey_daemon_multi2;
        IsMelonValid();
        IsModInstalled();
        Showifvalidgamefolder();
    }

    private void changeSteamLaunchToFullOfflineToolStripMenuItem_Click(object sender, EventArgs e)
    {

        var steamPath = Utils.GetPathFromRegistry("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath");
        if (!string.IsNullOrEmpty(steamPath))
        {
            var mbresult = MessageBox.Show("This will change the Last Epoch steam launch option to always launch in offline mode. You can revert this change by changing the launch option back to \"--offline\" or removing it entirely.\n\nMake sure Steam is closed (also from tray) before applying this change.", "Warning", YesNo, Warning);
            if (mbresult == DialogResult.Yes)
            {
                Config.Instance.UpdateSteamPath(steamPath);
                EnsureDefaultLaunchOptionInSteam(steamPath, "899770", "96be4408", "1");
            }
        }
    }

    private void showStartupMessageToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
    {
        Config.Instance.UpdateShowStartupMessage(showStartupMessageToolStripMenuItem.Checked);
    }

    private void checkForLauncherUpdateToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            
            _updater.CheckForUpdate(true);
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"Error checking for updates: {ex.Message}\n{ex.StackTrace}");
        }
    }


}