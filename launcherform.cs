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

    private readonly MaterialSkinManager? _skinManager;
    private bool _isDarkTheme;

    private LogViewerForm? _attachedLogForm;
    public bool UserHasInternet = true;

    private readonly Bitmap _imagecheck = Properties.Resources.check_64dp_green;
    private readonly Bitmap _imagecross = Properties.Resources.close_64dp_red;

    //static string Truncate(string s, int len) => s == null ? "" : (s.Length <= len ? s : s.Substring(0, len) + "…");

    public Launcherform()
    {
        Visible = false;
        InitializeComponent();
        Visible = false;

        try
        {
            // Check for launcher update
            Logger.Global.Info("Launcher Update Check....");
            if (Config.Instance.Settings.AutoUpdate)
            {
                try
                {
                    _ = _updater.CheckForUpdateAsync(false);
                }
                catch (Exception ex)
                {
                    Logger.Global.Error("Launcher update check failed!");
                }
            }

            var asm = Assembly.GetExecutingAssembly();
            var version = asm.GetName().Version;
            if (version != null)
            {
                this.SafeSetText($"{asm.GetName().Name} - {version.Major}.{version.Minor} for Last Epoch Hud Mod by Ash, launcher by JP");
                Logger.Global.Info(Text);
                Logger.Global.Info(
                    $"==========  LE Hud Mod Launcher {version.Major}.{version.Minor} started ============ ");
            }
            _skinManager = MaterialSkinManager.Instance;
            _skinManager.EnforceBackcolorOnAllComponents = true;
            _skinManager.AddFormToManage(this);
            Icon = Properties.Resources.gooey_daemon_multi2;
            _isDarkTheme = Config.Instance.Settings.DarkMode;
            NewApplyTheme();

            Resize += Launcherform_Resize;
            Activated += Launcherform_activated;
            ShowStartupMessage();
            toolStripStatus.SafeSetText("");
            Visible = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Please Report this error on the LE Hud Mod discord server...\n\n" + ex.Message + "\n" +
                            ex.Source + "\n" + ex.StackTrace);
        }
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            await Task.Run(Initapp);
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"Fatal error {ex.Message}\n{ex.StackTrace}");
        }
        this.Visible = true;
    }


    private void Initapp()
    {
        Logger.Global.Debug(" === Initializing Launcher background init thread ==== ");

        materialTextBoxPath.SafeSetText(Config.Instance.Settings.GameDir);
        checkBoxKeepOpen.SafeSetChecked(Config.Instance.Settings.KeepOpen);
        checkBoxStartUpMessage.SafeSetChecked(Config.Instance.Settings.ShowStartupMessage);
        checkBoxHideConsole.SafeSetChecked(Config.Instance.Settings.HideConsole);
        toolstripAutoUpdate.SafeSetChecked(Config.Instance.Settings.AutoUpdate);
        toolstripCheckModUpdate.SafeSetChecked(Config.Instance.Settings.AutoCheckModVersion);
        Config.Instance.UpdateAutoUpdate(toolstripAutoUpdate.Checked);
        labelVersion.SafeSetText("");
        if (Config.Instance.Settings.KbGamePadSelect == 0) radioKb.SafeSelect();
        else radioGamepad.SafeSelect();
        textGameVersion.SafeSetText("Unknown");
        if (Config.Instance.Settings.MainWindowX < 0 || Config.Instance.Settings.MainWindowY < 0)
        {
            Config.Instance.UpdateMainWindowPosition(500, 500);
        }

        StartPosition = FormStartPosition.Manual;
        Location = new Point(
            Config.Instance.Settings.MainWindowX,
            Config.Instance.Settings.MainWindowY
        );

        toolStripTheme.SafeSetText(Config.Instance.Settings.DarkMode ? "Light Theme" : "Dark Theme");

        if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))
        {
            ShowGameVersion();
        }

        _utils.SetHideConsole(Config.Instance.Settings.GameDir + @"\\UserData\Loader.cfg");
        if (Config.Instance.Settings.AutoUpdate) CheckLauncherUpdate(false);
        if (Config.Instance.Settings.AutoCheckModVersion)
        {
            _ = CheckModsForUpdate(true, true, false, false);
        }

        IsMelonValid();
        IsModInstalled();
        Showifvalidgamefolder();
    }


    public sealed override string Text
    {
        get => base.Text;
        set => base.Text = value;
    }

    private static void CheckLauncherUpdate(bool rep)

    {
        var updater = new UpdateChecker();
        try
        {
            _ = updater.CheckForUpdateAsync(rep);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error checking for updates: {ex.Message}", "Update", OK, Warning);
        }
    }

    private void Showifvalidgamefolder()

    {
        pictureCheck.Image = Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)) ? _imagecheck : _imagecross;
    }

    private bool IsMelonValid()

    {
        bool shaCheck;
        if (Exists(Config.Instance.Settings.GameDir + @"\MelonLoader\net6\MelonLoader.dll"))
        {
            shaCheck = VerifySha256(Config.Instance.Settings.GameDir + @"\MelonLoader\net35\MelonLoader.dll",
                "8825deded3c5d882695c01215e57493fb05af8cf5c406753cfa2999f9222c68b");
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

    private async Task<bool> CheckModsForUpdate(bool autoupdate, bool showmessage, bool forcemelon, bool forcemod)
    {
        IsModInstalled();
        _ = IsMelonValid();
        Showifvalidgamefolder();
        Logger.Global.Debug($"Install Melon/mods: {autoupdate} - {showmessage} - {forcemelon} - {forcemod}");
        if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename))) return false;
        var skipmelondl = false;
        var gamePath = Config.Instance.Settings.GameDir;
        FileDownloader.DownloadList? dllist;

        if (!Directory.Exists(gamePath + @"\modsdl_do_not_delete"))
            Directory.CreateDirectory(gamePath + @"\modsdl_do_not_delete");
        try
        {
            // get download links
            dllist = await AddDownloadsFromJson();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to download dllist {ex.Message}\n{ex.Message}", "Update", OK, Warning);
            return false;
        }
        Logger.Global.Debug($"Install Melonloader, autoupdate: {autoupdate} - Forcemelon {forcemelon}");
        if (autoupdate || forcemelon)
        {
            // Check if Existing Melon.zip is uptodate
            toolStripStatus.SafeSetText("Checking Melonloader...");
            if (Exists(gamePath + @"\modsdl_do_not_delete\Melon.zip"))
            {
                toolStripStatus.SafeSetEnabled(true);
                toolStripStatus.SafeSetEnabled(true);

                // Check if the correct Melonloader was already downloaded so skip if yes
                // var (remoteFileDate, remoteFileSize) = _utils.GetRemoteFileInfo(dllist.Files[0].Url);

                if (dllist != null && _utils.IsLocalFileUpToDate(gamePath + @"\modsdl_do_not_delete\Melon.zip", dllist.Files[0].Url))
                {
                    skipmelondl = true; // up-to-date so skip dl
                }
                Logger.Global.Debug($"SKIPmelondl = {skipmelondl}");
            }

            // download melon.zip 
            Logger.Global.Debug($"skipmelondl: {skipmelondl}");
            if (!skipmelondl)
            {
                pictureCheckLoader.Image = Properties.Resources.close_64dp_red;

                try
                {
                    if (!skipmelondl)
                    {
                        //CleanupDirs(true);
                        Logger.Global.Debug("Download melonloader...");
                        if (dllist != null)
                            await _fileDownloader.DownloadFileAsync(dllist.Files[0].Url,
                                gamePath + @"\modsdl_do_not_delete", "Melon.zip");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Global.Debug($"Error downloading Melonloader {ex.Message}\n{ex.StackTrace}");
                }
            }
            if (!IsMelonValid() || forcemelon) // if no valid version there or if forced to install
            {
                CleanupDirs(true);
                Logger.Global.Debug($"Extracting melonloader... to {gamePath}");
                await _utils.ExtractFile("Melon.zip", Path.GetFullPath(gamePath + @"\modsdl_do_not_delete"), gamePath);
                //if (showmessage) MessageBox.Show("Melonloader installed!", );
                toolStripStatus.SafeSetText(
                    @"Melonloader Installation finished. Running the game after install may take a while....");
            }
            else toolStripStatus.SafeSetText("");

            if (!IsMelonValid())
                Logger.Global.Debug("Something went wrong during melonloader install");
        }

        // Install mod
        Logger.Global.Debug($"Install MOD, autoupdate: {autoupdate} - Forcemod {forcemod}");
        if (autoupdate || forcemod) // always update with autoupdate or forcemod (install)
        {
            // var (remoteFileDate, remoteFileSize) = _utils.GetRemoteFileInfo(dllist.Files[1].Url);
            // var (remoteFileDate2, remoteFileSize2) = _utils.GetRemoteFileInfo(dllist.Files[2].Url);
            bool skipmoddl;
            if (dllist != null && Exists(gamePath + @"\modsdl_do_not_delete\LastEpoch_Hud(Keyboard).rar") &
                _utils.IsLocalFileUpToDate(gamePath + @"\modsdl_do_not_delete\LastEpoch_Hud(Keyboard).rar",
                    dllist.Files[1].Url) &
                Exists(gamePath + @"\modsdl_do_not_delete\LastEpoch_Hud(WinGamepad).rar") &
                _utils.IsLocalFileUpToDate(gamePath + @"\modsdl_do_not_delete\LastEpoch_Hud(WinGamepad).rar",
                    dllist.Files[2].Url))
                skipmoddl = true;
            else
                skipmoddl = false;

            Logger.Global.Debug($"Install MOD, skipmoddl {skipmoddl}");
            if (!skipmoddl)
            {
                try
                {
                    var folderDownloader = new GitHubReleaseDownloader.GitHubFolderDownloader();
                    var files = await folderDownloader.FetchFolderFilesAsync("RCInet", "LastEpoch_Mods", "master", "Latest");
                    await folderDownloader.DownloadFilesAsync(files, gamePath + @"\modsdl_do_not_delete");
                }
                catch (Exception ex)
                {
                    Logger.Global.Error("Error downloading/extracting modfiles");
                    return false;
                }
            }

            // Extract the downloaded files from the \Lastest folder
            ExtractAllRars(gamePath + @"\modsdl_do_not_delete", gamePath + @"\modsdl_do_not_delete\");
            InstallMod(!autoupdate);
        }

        IsModInstalled();
        _ = IsMelonValid();
        Showifvalidgamefolder();
        return false;
    }

    private void IsModInstalled()
    {
        if (Exists(Config.Instance.Settings.GameDir + @"\Mods\LastEpoch_Hud.dll"))
        {
            var modfileinfo = new FileInfo(Config.Instance.Settings.GameDir + @"\Mods\LastEpoch_Hud.dll");
            pictureModInstalled.Image = Properties.Resources.check_64dp_green;
            labelVersion.SafeSetText($"({modfileinfo.LastWriteTime.ToString(CultureInfo.CurrentCulture)})");
            return;
        }
        labelVersion.SafeSetText("");
        pictureModInstalled.Image = Properties.Resources.close_64dp_red;
    }

    private void InstallMod(bool showmessage)
    {
        if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename))) return;
        if (!Exists(Config.Instance.Settings.GameDir + @"\Mods"))
            Directory.CreateDirectory(Config.Instance.Settings.GameDir + @"\Mods");
        Console.WriteLine("Installing mod");
        switch (radioKb.Checked)
        {
            case true when Exists(Config.Instance.Settings.GameDir + @"\modsdl_do_not_delete\LastEpoch_Hud(Keyboard).rar"):
            {
                Utils.CopyFolder(Config.Instance.Settings.GameDir + @"\modsdl_do_not_delete\LastEpoch_Hud(Keyboard)",
                    Config.Instance.Settings.GameDir + @"\Mods");
                if (showmessage) MessageBox.Show("Keyboard version of the HUD Mod is now installed");
                Console.WriteLine("Keyboard version of the HUD Mod is now installed");
                break;
            }
            case false when Exists(Config.Instance.Settings.GameDir + @"\modsdl_do_not_delete\LastEpoch_Hud(WinGamepad).rar"):
            {
                Utils.CopyFolder(Config.Instance.Settings.GameDir + @"\modsdl_do_not_delete\LastEpoch_Hud(WinGamepad)",
                    Config.Instance.Settings.GameDir + @"\Mods");
                if (showmessage) MessageBox.Show("Gamepad version of the HUD Mod is now installed");
                Console.WriteLine("Gamepad version of the HUD Mod is now installed");
                break;
            }
        }

        IsModInstalled();
    }


    private static void ExtractAllRars(string sourceFolder, string destinationFolder)
    {
        try
        {
            var rarFiles = Directory.GetFiles(sourceFolder, "*.rar");

            foreach (var rarFile in rarFiles)
            {
                Console.WriteLine($"Extracting {Path.GetFileName(rarFile)}...");

                using var archive = RarArchive.Open(rarFile);
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    Directory.CreateDirectory(destinationFolder + $"\\{Path.GetFileNameWithoutExtension(rarFile)}");
                    entry.WriteToDirectory(
                        destinationFolder + $"\\{Path.GetFileNameWithoutExtension(rarFile)}",
                        new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true,
                            PreserveFileTime = true
                        }
                    );
                }
            }
        }
        catch (Exception ex)

        {
            Logger.Global.Error($"Error downloading latest mods\n{ex.Message}\n{ex.StackTrace}");
            MessageBox.Show($"Error downloading latest mods\n{ex.Message}");
        }
    }

    private void NewApplyTheme()
    {
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
                Primary.Grey500, Accent.Cyan700,
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
                Primary.Cyan900, Primary.Cyan800,
                Primary.Cyan800, Accent.Orange700,
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
        var gameversion = NewUnityHelper.ReadGameInfo(_assetsManager, Config.Instance.Settings.GameDir + @"\Last Epoch_Data");
        if (gameversion != null) textGameVersion.SafeSetText(gameversion);
        Showifvalidgamefolder();
    }

    private void CleanupDirs(bool melclean)
    {
        try
        {
            Logger.Global.Info("CleanupDirs started...");

            var melonLoaderPath = Path.Combine(Config.Instance.Settings.GameDir, "Melonloader");
            var modsPath = Path.Combine(Config.Instance.Settings.GameDir, "Mods");

            if (Directory.Exists(melonLoaderPath) && melclean) Directory.Delete(melonLoaderPath, true);
            else Logger.Global.Debug($"[{nameof(CleanupDirs)}] Melonloader dir not found, skipping delete.");

            if (!Directory.Exists(modsPath)) Directory.CreateDirectory(modsPath);
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"CleanupDirs error: {ex}");
        }
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
            Logger.Global.Error($"StartGame error: {ex}");
            MessageBox.Show($"Unexpected error launching game:\n{ex.Message}", "Error", OK, Error);
        }
    }

    private void GameProcess_Exited(object? sender, EventArgs e)
    {
        // This event is raised on a background thread → marshal back to UI
        if (InvokeRequired)
        {
            BeginInvoke(() => GameProcess_Exited(sender, e));
            statusStripLabel.SafeSetText("Game Exited!");
            if (_gameProcess != null) _gameProcess.Exited -= GameProcess_Exited;

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
    }

    private void launcherform_FormClosing(object sender, FormClosingEventArgs e)
    {
        Config.Instance.Save();
    }

    private async Task<FileDownloader.DownloadList?> AddDownloadsFromJson()
    {
        //FileDownloader fileDownloader = new FileDownloader();

        const string dlurl = "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/dllistnew.json";

        try
        {
            await _fileDownloader.DownloadFileAsync(dlurl, Config.Instance.Settings.TmpDownloadFolder, "dllistnew.json");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to download dllist {ex.Message}\n{ex.StackTrace}", "Error addownloadfromjson", OK,
                Warning);
            return null;
        }

        //var jsonContents = await ReadAllTextAsync(Path.Combine(Instance.Settings.TmpDownloadFolder, "dllistnew.json"));

        var reader = new StreamReader(Path.Combine(Config.Instance.Settings.TmpDownloadFolder, "dllistnew.json"));
        try
        {
            var downloadList = JsonConvert.DeserializeObject<FileDownloader.DownloadList>(await reader.ReadToEndAsync());
            //File.Delete(Path.Combine(Path.GetTempPath(), "dllistnew.json"));
            return downloadList;
        }
        finally
        {
            reader.Dispose();
        }
    }

    private async void ShowStartupMessage(bool forced = false)
    {
        var rtfPath = AppDomain.CurrentDomain.BaseDirectory;
        var showMessage = forced;

        Logger.Global.Debug(rtfPath);
        const string dlurl =
            "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/StartUpMessage.zip";

        try
        {
            if (Exists(new StringBuilder().Append(rtfPath).Append(@"StartupMessage.zip").ToString()) &&
                !_utils.IsLocalFileUpToDate(
                    new StringBuilder().Append(rtfPath).Append(@"StartupMessage.zip").ToString(), dlurl))
            {
                if (!_utils.IsLocalFileUpToDate(Path.Combine(rtfPath, "StartupMessage.zip"), dlurl))
                {
                    Delete(new StringBuilder().Append(rtfPath).Append(@"StartupMessage.zip").ToString());
                    if (Exists(Path.Combine(rtfPath, "StartupMessage.rtf")))
                        Delete(Path.Combine(rtfPath, "StartupMessage.rtf"));
                    await _fileDownloader.DownloadFileAsync(dlurl, AppDomain.CurrentDomain.BaseDirectory,
                        "StartupMessage.zip");
                    await _utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory,
                        AppDomain.CurrentDomain.BaseDirectory);
                    // force display of status message once if newer message found                        
                    showMessage = true;
                }
                else if (Exists(Path.Combine(rtfPath, "StartupMessage.zip")))
                {
                    await _utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory,
                        AppDomain.CurrentDomain.BaseDirectory);
                    showMessage = true;
                }
            }
            else if (!Exists(Path.Combine(rtfPath, "StartupMessage.zip")))
            {
                await _fileDownloader.DownloadFileAsync(dlurl, AppDomain.CurrentDomain.BaseDirectory,
                    "StartupMessage.zip");
                await _utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory,
                    AppDomain.CurrentDomain.BaseDirectory);
                showMessage = true;
            }
            else if (Exists(Path.Combine(rtfPath, "StartupMessage.zip")))
                await _utils.ExtractFile("StartupMessage.zip", AppDomain.CurrentDomain.BaseDirectory,
                    AppDomain.CurrentDomain.BaseDirectory);
        }
        catch (Exception ex)
        {
            Logger.Global.Error(new StringBuilder().Append("Error while loading startup message: ")
                .Append(ex.Message)
                .ToString());
        }

        if ((!showMessage || !Exists(Path.Combine(rtfPath, "StartupMessage.rtf"))) &&
            (!Config.Instance.Settings.ShowStartupMessage ||
             !Exists(new StringBuilder().Append(rtfPath).Append(@"StartupMessage.rtf").ToString()))) return;
        var messageFileToLoad = Path.Combine(rtfPath, "StartupMessage.rtf");
        var dlg = new StartupDialog(messageFileToLoad,
            Config.Instance.Settings.ShowStartupMessage);
        try
        {
            _ = dlg.ShowDialog();

            Config.Instance.UpdateShowStartupMessage(dlg.NewShowSetting);
            checkBoxStartUpMessage.SafeSetChecked(dlg.NewShowSetting);
        }
        finally
        {
            dlg.Dispose();
        }
    }

    private void toolStripMenuItem1_Click(object sender, EventArgs e)
    {
        if (_isDarkTheme)
        {
            _isDarkTheme = false;
            NewApplyTheme();
            Config.Instance.UpdateDarkMode(false);
        }
        else
        {
            _isDarkTheme = true;
            NewApplyTheme();
            Config.Instance.UpdateDarkMode(true);
        }

        _isDarkTheme = Config.Instance.Settings.DarkMode;
        toolStripTheme.SafeSetText(Config.Instance.Settings.DarkMode ? "Light Theme" : "Dark Theme");
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
            // Do NOT set _attachedLogForm.Height here; let it keep its own dimensions.
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

    private async void checkForLauncherUpdateNowToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            var updater = new UpdateChecker();
            await updater.CheckForUpdateAsync(true);
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"Error checking for updates: {ex.Message}", "Update", OK, Warning);
        }
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
        Config.Instance.UpdateGameDir(materialTextBoxPath.Text);
        if (!Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename))) return;
        _utils.SetHideConsole(Config.Instance.Settings.GameDir + @"\\UserData\Loader.cfg");
        ShowGameVersion();
        IsModInstalled();
        _ = IsMelonValid();
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
                if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)) && Config.Instance.Settings.AutoUpdate)
                    _ = CheckModsForUpdate(true, true, false, false);
            }

            if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))
            {
                _utils.SetHideConsole(Config.Instance.Settings.GameDir + @"\\UserData\Loader.cfg");
            }
            else textGameVersion.SafeSetText("Unknown");
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"buttonBrowse_Click error: {ex}");
            _ = IsMelonValid();
            MessageBox.Show($"Failed to select game folder:\n{ex.Message}", "Error", OK, Error);
        }

        IsModInstalled();
        Showifvalidgamefolder();
        _ = IsMelonValid();
    }

    private void buttonGetGameFolder_Click_1(object sender, EventArgs e)
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
                if (Exists(Path.Combine(tempgamedir, GameFilename)) && Config.Instance.Settings.AutoUpdate)
                    _ = CheckModsForUpdate(true, true, false, false);
                //textGamePath.Text = tempgamedir;
                materialTextBoxPath.SafeSetText(tempgamedir);
                ShowGameVersion();
            }
            else MessageBox.Show("Game folder not found. Please select it manually.", "Error", OK, Error);
        }

        Showifvalidgamefolder();
        _ = IsMelonValid();
    }

    private void radioKb_CheckedChanged_1(object sender, EventArgs e)
    {
        if (radioKb.Checked) Config.Instance.UpdateKbGamePadSelect(0);
    }

    private void materialButton1_Click_1(object sender, EventArgs e)
    {
        var themePickerForm = new ThemePickerForm
        {
            Site = null,
            AccessibleDefaultActionDescription = null,
            AccessibleDescription = null,
            AccessibleName = null,
            AccessibleRole = AccessibleRole.None,
            AllowDrop = false,
            Anchor = AnchorStyles.None,
            AutoScrollOffset = default,
            DataContext = null,
            BackgroundImage = null,
            BackgroundImageLayout = ImageLayout.None,
            Bounds = default,
            Capture = false,
            CausesValidation = false,
            ContextMenuStrip = null,
            Cursor = null,
            Dock = DockStyle.None,
            Enabled = false,
            Font = null,
            ForeColor = default,
            Height = 0,
            IsAccessible = false,
            Left = 0,
            Name = null,
            Parent = null,
            Region = null,
            RightToLeft = RightToLeft.No,
            Tag = null,
            Top = 0,
            UseWaitCursor = false,
            Visible = false,
            Width = 0,
            WindowTarget = null,
            Padding = default,
            ImeMode = ImeMode.NoControl,
            AutoScrollMargin = default,
            AutoScrollPosition = default,
            AutoScrollMinSize = default,
            BindingContext = null,
            AutoScaleDimensions = default,
            AutoScaleMode = AutoScaleMode.None,
            ActiveControl = null,
            AutoSize = false,
            BackColor = default,
            ClientSize = default,
            Location = default,
            Margin = default,
            MaximumSize = default,
            MinimumSize = default,
            Size = default,
            TabIndex = 0,
            TabStop = false,
            AutoScroll = false,
            AutoValidate = AutoValidate.Disable,
            AcceptButton = null,
            AllowTransparency = false,
            AutoScaleBaseSize = default,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            CancelButton = null,
            ControlBox = false,
            DesktopBounds = default,
            DesktopLocation = default,
            DialogResult = DialogResult.None,
            HelpButton = false,
            Icon = null,
            IsMdiContainer = false,
            KeyPreview = false,
            MainMenuStrip = null,
            MaximizeBox = false,
            MdiChildrenMinimizedAnchorBottom = false,
            MdiParent = null,
            MinimizeBox = false,
            Opacity = 0,
            Owner = null,
            RightToLeftLayout = false,
            ShowInTaskbar = false,
            ShowIcon = false,
            SizeGripStyle = SizeGripStyle.Auto,
            StartPosition = FormStartPosition.Manual,
            TopLevel = false,
            TopMost = false,
            TransparencyKey = default,
            Text = null,
            FormBorderStyle = FormBorderStyle.None,
            WindowState = FormWindowState.Normal,
            Depth = 0,
            MouseState = MouseState.HOVER,
            Sizable = false,
            FormStyle = FormStyles.StatusAndActionBar_None,
            DrawerShowIconsWhenHidden = false,
            DrawerWidth = 0,
            DrawerAutoHide = false,
            DrawerAutoShow = false,
            DrawerIndicatorWidth = 0,
            DrawerIsOpen = false,
            DrawerUseColors = false,
            DrawerHighlightWithAccent = false,
            DrawerBackgroundWithAccent = false,
            DrawerTabControl = null
        };
        themePickerForm.Show();
    }

    public class UpdateChecker
    {
        public async Task CheckForUpdateAsync(bool reportstatus = false)
        {
            var fileDownloader = new FileDownloader();

            try
            {
                //var exePath = Assembly.GetExecutingAssembly().Location;
                //var exeFolder = Path.GetDirectoryName(exePath);

                const string updateJsonUrl =
                    "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/update.json";

                await fileDownloader.DownloadFileAsync(updateJsonUrl, Config.Instance.Settings.TmpDownloadFolder,
                    "update.json");
                var json = await ReadAllTextAsync(Path.Combine(Config.Instance.Settings.TmpDownloadFolder, "update.json"));

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
                    var result = MessageBox.Show(
                        $"A new version ({latestVersion}) of the launcher/installer is available. You are on {currentVersion.Major}.{currentVersion.Minor}.\n\nDo you want to update now?",
                        "Update Available",
                        YesNo,
                        Information);

                    if (result == DialogResult.Yes)
                        Logger.Global.Info("Downloading new launcher setup...");
                    await DownloadAndRunInstaller(updateInfo.InstallerUrl);
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
                await fileDownloader.DownloadFileAsync(installerUrl, Path.GetTempPath(),
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
                MessageBox.Show($"Error! {ex.Message}\n{ex.StackTrace}", "Error", OK, Error);
            }
        }
    }

    public class UpdateInfo(string version, string installerUrl)
    {
        public string Version { get; } = version;
        public string InstallerUrl { get; } = installerUrl;
    }

    private void toolstripCheckModUpdate_CheckedChanged(object sender, EventArgs e)
    {
        Config.Instance.UpdateCheckModVersion(toolstripCheckModUpdate.Checked);
    }

    private void toolstripForceModeUpdate_Click(object sender, EventArgs e)
    {
        if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))

        {
            _ = CheckModsForUpdate(false, true, true, false);
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
        using var sha = SHA256.Create();
        using var stream = OpenRead(filePath);
        var hash = sha.ComputeHash(stream);
        var actual = Convert.ToHexString(hash).ToUpperInvariant();
        return string.Equals(actual, expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    private void checkBoxKeepOpen_CheckStateChanged(object sender, EventArgs e)
    {
        Config.Instance.UpdateKeepOpen(checkBoxKeepOpen.Checked);
    }

    private void checkBoxStartUpMessage_CheckedChanged_2(object sender, EventArgs e)
    {
        Config.Instance.UpdateShowStartupMessage(checkBoxStartUpMessage.Checked);
    }

    private void checkBoxHideConsole_CheckStateChanged(object sender, EventArgs e)
    {
        Config.Instance.UpdateHideConsole(checkBoxHideConsole.Checked);
        _utils.SetHideConsole(Config.Instance.Settings.GameDir + @"\UserData\Loader.cfg");
        Logger.Global.Debug(Config.Instance.Settings.GameDir + @"\UserData\Loader.cfg");
    }

    private void buttonAttachLog_Click_2(object sender, EventArgs e)
    {
        var logPath = Config.Instance.Settings.GameDir + @"\MelonLoader\Latest.log";
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
    }

    private void forceInstallModToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (Exists(Path.Combine(Config.Instance.Settings.GameDir, GameFilename)))

        {
            _ = CheckModsForUpdate(false, true, false, true);
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
    }
}