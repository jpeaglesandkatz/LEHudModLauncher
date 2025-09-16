using LogUtils;
using Microsoft.Win32;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using static Microsoft.Win32.Registry;
using DownloadUtils;


namespace LauncherUtils
{
    public class Utils
    {
        public AppSettings settings = SettingsManager.Instance.Settings;

        private List<DownloadTask> QueDownloads { get; set; } = new List<DownloadTask>();
        private List<ExtractItem> QueExtracts { get; set; } = new List<ExtractItem>();

        public FileDownloader fileDownloader = new FileDownloader();

        public class DownloadTask
        {
            public int Id { get; set; }
            public int Priority { get; set; }
            public string? Type { get; set; }
            public string? Url { get; set; }
            public string? DownloadPath { get; set; }
            public string? DestinationPath { get; set; }
            public string? Filename { get; set; }
            public bool DelOriginal { get; set; }
            public bool SkipIfExist { get; set; }
        }

        public class ExtractItem
        {
            public int Priority { get; set; }
            public string? Type { get; set; }
            public string? SourcePath { get; set; }
            public string? DestinationPath { get; set; }
            public string? FileName { get; set; }
            public bool DelOriginal { get; set; }
        }

        public class AppSettings : INotifyPropertyChanged
        {
            private string _gameDir = string.Empty;
            public string GameDir
            {
                get => _gameDir;
                set
                {
                    if (_gameDir != value)
                    {
                        _gameDir = value;
                        OnPropertyChanged(nameof(GameDir));
                    }
                }
            }

            private int _errorCount;
            public int ErrorCount
            {
                get => _errorCount;
                set
                {
                    if (_errorCount != value)
                    {
                        _errorCount = value;
                        OnPropertyChanged(nameof(ErrorCount));
                    }
                }
            }

            private string _logPath = string.Empty;
            public string LogPath
            {
                get => _logPath;
                set
                {
                    if (_logPath != value)
                    {
                        _logPath = value;
                        OnPropertyChanged(nameof(LogPath));
                    }
                }
            }

            private string _steampath = string.Empty;
            public string Steampath
            {
                get => _steampath;
                set
                {
                    if (_steampath != value)
                    {
                        _steampath = value;
                        OnPropertyChanged(nameof(Steampath));
                    }
                }
            }

            private bool _hideConsole = true;
            public bool HideConsole
            {
                get => _hideConsole;
                set
                {
                    if (_hideConsole != value)
                    {
                        _hideConsole = value;
                        OnPropertyChanged(nameof(HideConsole));
                    }
                }
            }

            private bool _keepOpen = true;
            public bool KeepOpen
            {
                get => _keepOpen;
                set
                {
                    if (_keepOpen != value)
                    {
                        _keepOpen = value;
                        OnPropertyChanged(nameof(KeepOpen));
                    }
                }
            }

            private bool _showStartupMessage = true;
            public bool ShowStartupMessage
            {
                get => _showStartupMessage;
                set
                {
                    if (_showStartupMessage != value)
                    {
                        _showStartupMessage = value;
                        OnPropertyChanged(nameof(ShowStartupMessage));
                    }
                }
            }

            private int _kbGamePadSelect = 0;
            public int KbGamePadSelect
            {
                get => _kbGamePadSelect;
                set
                {
                    if (_kbGamePadSelect != value)
                    {
                        _kbGamePadSelect = value;
                        OnPropertyChanged(nameof(KbGamePadSelect));
                    }
                }
            }

            private bool _darkMode = true;
            public bool DarkMode
            {
                get => _darkMode;
                set
                {
                    if (_darkMode != value)
                    {
                        _darkMode = value;
                        OnPropertyChanged(nameof(DarkMode));
                    }
                }
            }

            private string _tmpDownloadFolder = string.Empty;
            public string TmpDownloadFolder
            {
                get => _tmpDownloadFolder;
                set
                {
                    if (_tmpDownloadFolder != value)
                    {
                        _tmpDownloadFolder = value;
                        OnPropertyChanged(nameof(TmpDownloadFolder));
                    }
                }
            }

            private bool _ShowedOnce = false;
            public bool ShowedOnce
            {
                get => _ShowedOnce;
                set
                {
                    if (_ShowedOnce != value)
                    {
                        _ShowedOnce = value;
                        OnPropertyChanged(nameof(ShowedOnce));
                    }
                }
            }

            private int _logWindowWidth = 800;
            public int LogWindowWidth
            {
                get => _logWindowWidth;
                set
                {
                    if (_logWindowWidth != value)
                    {
                        _logWindowWidth = value;
                        OnPropertyChanged(nameof(LogWindowWidth));
                    }
                }
            }

            private int _logWindowHeight = 450;
            public int LogWindowHeight
            {
                get => _logWindowHeight;
                set
                {
                    if (_logWindowHeight != value)
                    {
                        _logWindowHeight = value;
                        OnPropertyChanged(nameof(LogWindowHeight));
                    }
                }
            }

            private int _mainWindowX = 400;
            public int MainWindowX
            {
                get => _mainWindowX;
                set
                {
                    if (_mainWindowX != value)
                    {
                        _mainWindowX = value;
                        OnPropertyChanged(nameof(MainWindowX));
                    }
                }
            }

            private int _mainWindowY = 400;
            public int MainWindowY
            {
                get => _mainWindowY;
                set
                {
                    if (_mainWindowY != value)
                    {
                        _mainWindowY = value;
                        OnPropertyChanged(nameof(MainWindowY));
                    }
                }
            }

            private int _modinfoWindowX = 1600;
            public int ModInfoWindowX
            {
                get => _modinfoWindowX;
                set
                {
                    if (_modinfoWindowX != value)
                    {
                        _modinfoWindowX = value;
                        OnPropertyChanged(nameof(ModInfoWindowX));
                    }
                }
            }

            private int _modinfoWindowY = 800;
            public int ModInfoWindowY
            {
                get => _modinfoWindowY;
                set
                {
                    if (_modinfoWindowY != value)
                    {
                        _modinfoWindowY = value;
                        OnPropertyChanged(nameof(ModInfoWindowY));
                    }
                }
            }




            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                try
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    //Logger.Global.Debug($"AppSettings: property changed: {propertyName}");
                }
                catch
                {
                    // best-effort
                }
            }

            public AppSettings()
            {
                var dd = Path.GetTempPath();
                TmpDownloadFolder = Path.Combine(dd, "LEModtempDir");
                try
                {
                    if (!Directory.Exists(TmpDownloadFolder)) Directory.CreateDirectory(TmpDownloadFolder);
                }
                catch (Exception ex)
                {
                    Logger.Global.Warning($"Failed to create TmpDownloadFolder: {ex.Message}");
                }
            }
        }

        public sealed class SettingsManager
        {
            private static readonly Lazy<SettingsManager> Lazy =
                new(() => new SettingsManager());

            public static SettingsManager Instance => Lazy.Value;

            private static readonly string SettingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEHuDModLauncher");
            private static readonly string SettingsFilePath = Path.Combine(SettingsPath, "Settings.json");

            private static readonly object SaveLock = new object();

            private AppSettings _settings = new AppSettings();
            public AppSettings Settings => _settings;

            private SettingsManager()
            {
                Load();
                // Note: Load attaches PropertyChanged handler to the loaded _settings.
            }

            private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                // Persist settings whenever a property changes.
                try
                {
                    Save();
                }
                catch (Exception ex)
                {
                    Logger.Global.Error($"Settings_PropertyChanged -> Save failed: {ex.Message}");
                }
            }

            public void Load()
            {
                try
                {
                    if (!Directory.Exists(SettingsPath))
                        Directory.CreateDirectory(SettingsPath);

                    // Unsubscribe old instance if present to avoid duplicate handlers
                    try
                    {
                        _settings.PropertyChanged -= Settings_PropertyChanged;
                    }
                    catch { /* ignore if not subscribed */ }

                    if (File.Exists(SettingsFilePath))
                    {
                        string json = File.ReadAllText(SettingsFilePath);
                        var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                        _settings = loaded ?? new AppSettings();
                    }
                    else
                    {
                        _settings = new AppSettings();
                        // create defaults on disk
                        Save();
                    }

                    // Subscribe to property changes on the currently loaded settings instance
                    _settings.PropertyChanged += Settings_PropertyChanged;
                }
                catch (Exception ex)
                {
                    Logger.Global.Error($"SettingsManager.Load failed: {ex.Message}");
                    _settings = new AppSettings();
                    _settings.PropertyChanged += Settings_PropertyChanged;
                }
            }

            public void Save()
            {
                lock (SaveLock)
                {
                    try
                    {
                        if (!Directory.Exists(SettingsPath))
                            Directory.CreateDirectory(SettingsPath);

                        string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                        //Logger.Global.Debug($"Saving settings -> {SettingsFilePath}");
                        File.WriteAllText(SettingsFilePath, json);
                    }
                    catch (Exception ex)
                    {
                        Logger.Global.Error($"SettingsManager.Save failed: {ex.Message}");
                    }
                }
            }

            // Centralized update methods
            public void UpdateErrorCount(int errorcount)
            {
                _settings.ErrorCount = errorcount;
                Save();
            }

            public void UpdateTmpDownloadFolder(string tmpdownloadfolder)
            {
                _settings.TmpDownloadFolder = tmpdownloadfolder;
                Save();
            }

            public void UpdateGameDir(string gameDir)
            {
                _settings.GameDir = gameDir;
                Save();
            }

            public void UpdateSteamPath(string steamPath)
            {
                _settings.Steampath = steamPath;
                Save();
            }

            public void UpdateKeepOpen(bool keepOpen)
            {
                _settings.KeepOpen = keepOpen;
                Save();
            }

            public void UpdateShowStartupMessage(bool show)
            {
                _settings.ShowStartupMessage = show;
                Save();
            }

            public void UpdateDarkMode(bool darkMode)
            {
                _settings.DarkMode = darkMode;
                Save();
            }

            public void UpdateShowOnce(bool showOnce)
            {
                _settings.ShowedOnce = showOnce;
                Save();
            }


            public void UpdateKbGamePadSelect(int value)
            {
                _settings.KbGamePadSelect = value;
                Save();
            }

            public void UpdateLogWindowWidth(int width)
            {
                _settings.LogWindowWidth = width;
                Save();
            }

            public void UpdateLogWindowHeight(int height)
            {
                _settings.LogWindowHeight = height;
                Save();
            }

            public void UpdateMainWindowPosition(int x, int y)
            {
                _settings.MainWindowX = x;
                _settings.MainWindowY = y;
                Save();
            }

            public void UpdateModInfoWindowPosition(int x, int y)
            {
                _settings.ModInfoWindowX = x;
                _settings.ModInfoWindowY = y;
            }

            public void UpdateHideConsole(bool hideconsole)
            {
                _settings.HideConsole = hideconsole;
                Save();
            }
        }


        public Utils()

        {
            QueDownloads.Clear();
            QueExtracts.Clear();


        }

        internal static string? GetPathFromRegistry(string regpath, string valuename)
        {

            if (string.IsNullOrEmpty(regpath) || string.IsNullOrEmpty(valuename))
                return null;

            string[] parts = regpath.Split('\\', 2);
            if (parts.Length != 2)
                return null;

            RegistryKey? baseKey = parts[0].ToUpper() switch
            {
                "HKEY_LOCAL_MACHINE" => LocalMachine,
                "HKEY_CURRENT_USER" => CurrentUser,
                _ => null
            };
            if (baseKey == null)
                return null;


            using var subKey = baseKey.OpenSubKey(parts[1]);

            if (subKey == null)
                return null;

            return subKey.GetValue(valuename) as string;
        }


        public void Deletefiles(string? path)
        {
            {
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException($"Directory not found: {path}");

                // Delete all files
                foreach (string file in Directory.GetFiles(path))
                {
                    File.SetAttributes(file, FileAttributes.Normal); // remove read-only if set
                    File.Delete(file);
                }

                // Delete all subdirectories
                foreach (string dir in Directory.GetDirectories(path))
                {
                    Directory.Delete(dir, true); // true = recursive delete
                }
            }
        }

        public void RunExtraction()
        {
            if (QueExtracts.Count > 0)
            {
                Logger.Global.Debug($"Extracting {QueExtracts.Count} files");
                foreach (var em in QueExtracts)
                { Logger.Global.Info($"{em.SourcePath}\\{em.FileName} ---> {em.DestinationPath} {em.DelOriginal}"); }

                QueExtracts = QueExtracts.OrderBy(p => p.Priority).ToList();
                foreach (var extractitem in QueExtracts)
                {
                    ExtractFileLib(extractitem.FileName, extractitem.SourcePath, extractitem.DestinationPath, extractitem.DelOriginal);
                }
                QueExtracts.Clear();
            }
        }


        public void ExtractFileLib(string filename, string? sourcepath, string? destinationpath, bool deloriginal = false)
        {
            var archive = ArchiveFactory.Open(Path.Combine(sourcepath, filename)); //@"C:\Code\sharpcompress\TestArchives\sharpcompress.zip");
            try
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        Logger.Global.Info(entry.Key);
                        entry.WriteToDirectory(destinationpath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Extraction failed: {ex.Message}");
                SettingsManager.Instance.Settings.ErrorCount++;
            }
            finally
            {
                archive.Dispose();
            }
            if (deloriginal)
            {
                File.Delete(Path.Combine(sourcepath, filename));
            }
        }

        public void ExtractFile(string filename, string? sourcepath, string? destinationpath, bool deloriginal = false)
        {
            try
            {
                if (sourcepath != null)
                {
                    Logger.Global.Info(
                        $"Extracting {Path.Combine(sourcepath, filename)} to\n{Path.Combine(destinationpath ?? throw new ArgumentNullException(nameof(destinationpath)))} \n");
                    //if (File.Exists(Path.Combine(destinationpath, filename))) File.Delete(Path.Combine(destinationpath, filename));
                    if (settings?.TmpDownloadFolder != null)
                    {
                        ExtractFileLib(filename, sourcepath, destinationpath, deloriginal);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Extraction failed: {ex.Message}");
            }
            Logger.Global.Info($"Extracted {filename}");
        }

        public void AddDownload(int priority, string type, string url, string? downloadpath, string? destination, string filename, bool deloriginal, bool skipifexist)
        {

            var task = new DownloadTask
            {
                Url = url,
                DownloadPath = downloadpath,
                DestinationPath = destination,
                Priority = priority,
                Type = type,
                Filename = filename,
                DelOriginal = deloriginal,
                SkipIfExist = skipifexist

            };
            QueDownloads.Add(task);
            Logger.Global.Debug($"Added download: {url} to {downloadpath} as {filename} skipifexist: {skipifexist}");
        }

        public void StartDownloads()
        {
            int tprogress = 1;
            SettingsManager.Instance.Settings.ErrorCount = 0;
            QueDownloads = QueDownloads.OrderBy(p => p.Priority).ToList();
            foreach (var item in QueDownloads)
            {
                Logger.Global.Debug($"Download que: {item.Url} to {item.DownloadPath}\\{item.Filename} skipifexist: {item.SkipIfExist}");
            }
            // sort list by priority
            foreach (var item in QueDownloads)
            {
                try
                {
                    string localPath = item.DownloadPath;

                    // Get remote file date and size from server (HTTP HEAD)
                    string quefilepath = Path.Combine(item.DownloadPath, item.Filename);
                    if (File.Exists(quefilepath))
                    {
                        var (remoteFileDate, remoteFileSize) = GetRemoteFileInfo(item.Url);
                        Logger.Global.Debug($"Remote file date: {remoteFileDate}, size: {remoteFileSize}");
                        if (!IsLocalFileUpToDate(quefilepath, remoteFileDate, remoteFileSize))
                        {
                            Logger.Global.Debug($"Local file: {quefilepath} is not up to date. Downloading {item.Url} to {item.DownloadPath} as {item.Filename} (download count: {QueDownloads.Count})");
                            fileDownloader.DownloadFile(item.Url, item.DownloadPath, item.Filename, 5);
                        }
                        else Logger.Global.Debug($"Local file: {quefilepath} is up to date. Skipping download of {item.Url} (download count: {QueDownloads.Count})");
                    }
                    else fileDownloader.DownloadFile(item.Url, item.DownloadPath, item.Filename, 5);
                    // Add downloads to extaction que

                }
                catch (Exception ex)
                {
                    if (settings != null) SettingsManager.Instance.Settings.ErrorCount++;
                }
                if (File.Exists(Path.Combine(item.DownloadPath, item.Filename))) QueExtracts.Add(new ExtractItem
                {
                    FileName = item.Filename,
                    SourcePath = item.DownloadPath,
                    DestinationPath = item.DestinationPath,
                    DelOriginal = item.DelOriginal
                });

            }
            QueDownloads.Clear();
        }

        public bool IsLocalFileUpToDate(string localFilePath, DateTime remoteFileDate, long remoteFileSize = -1)
        {
            if (!File.Exists(localFilePath))
                return false;

            var localFileInfo = new FileInfo(localFilePath);

            // If server provided a valid Last-Modified date, prefer using it.
            if (remoteFileDate != DateTime.MinValue)
            {
                try
                {
                    if (localFileInfo.LastWriteTimeUtc >= remoteFileDate.ToUniversalTime())
                    {
                        Logger.Global.Debug($"Local file '{localFilePath}' is up to date by LastWriteTimeUtc ({localFileInfo.LastWriteTimeUtc} >= {remoteFileDate.ToUniversalTime()}).");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Global.Warning($"Timestamp comparison failed for '{localFilePath}': {ex.Message}");
                }
            }

            // Fall back to size comparison if server provided Content-Length.
            if (remoteFileSize >= 0)
            {
                try
                {
                    if (localFileInfo.Length == remoteFileSize)
                    {
                        Logger.Global.Debug($"Local file '{localFilePath}' is up to date by size ({localFileInfo.Length} == {remoteFileSize}).");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Global.Error($"Size comparison failed for '{localFilePath}': {ex.Message}");
                }
            }

            // Not up to date by either metric.
            return false;
        }

        // Helper to get remote file date (kept for compatibility)
        public DateTime GetRemoteFileDate(string url)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = client.Send(request);
            if (response.Content.Headers.LastModified.HasValue)
                return response.Content.Headers.LastModified.Value.UtcDateTime;
            return DateTime.MinValue;
        }

        // New helper: get remote Last-Modified and Content-Length via HTTP HEAD.
        public (DateTime LastModified, long ContentLength) GetRemoteFileInfo(string url)
        {
            try
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = client.Send(request);

                DateTime lastModified = DateTime.MinValue;
                long contentLength = -1;

                if (response.Content.Headers.LastModified.HasValue)
                    lastModified = response.Content.Headers.LastModified.Value.UtcDateTime;

                if (response.Content.Headers.ContentLength.HasValue)
                    contentLength = response.Content.Headers.ContentLength.Value;

                return (lastModified, contentLength);
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"GetRemoteFileInfo failed for {url}: {ex.Message}");
                return (DateTime.MinValue, -1);
            }
        }

        public static void SetHideConsole(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                Logger.Global.Error($"Config file {configFilePath} does not exist.");
                return;
            }

            var lines = File.ReadAllLines(configFilePath);
            bool changed = false;
            bool enabledconsole = SettingsManager.Instance.Settings.HideConsole;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("hide_console", StringComparison.OrdinalIgnoreCase))
                {
                    if (lines[i].Trim().Contains("hide_console = false", StringComparison.OrdinalIgnoreCase) || lines[i].Trim().Contains("hide_console = true", StringComparison.OrdinalIgnoreCase))
                    {
                        if (enabledconsole) lines[i] = "hide_console = true"; else lines[i] = "hide_console = false";
                        changed = true;
                    }
                }
            }

            if (changed)
                File.WriteAllLines(configFilePath, lines);
        }
    }
}