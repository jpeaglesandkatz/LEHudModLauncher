using System.ComponentModel;
using System.Text.Json;
using static LEHuDModLauncher.Classlibs.LogUtils.Log;

namespace LEHuDModLauncher.Settings;

public class Config
{
    public class AppSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _gameDir = string.Empty;
        public string GameDir
        {
            get => _gameDir;
            set
            {
                if (_gameDir == value) return;
                _gameDir = value;
                OnPropertyChanged(nameof(GameDir));
            }
        }

        private int _errorCount;
        public int ErrorCount
        {
            get => _errorCount;
            set
            {
                if (_errorCount == value) return;
                _errorCount = value;
                OnPropertyChanged(nameof(ErrorCount));
            }
        }

        private string _logPath = string.Empty;
        public string LogPath
        {
            get => _logPath;
            init
            {
                if (_logPath == value) return;
                _logPath = value;
                OnPropertyChanged(nameof(LogPath));
            }
        }

        private string _steampath = string.Empty;
        public string Steampath
        {
            get => _steampath;
            set
            {
                if (_steampath == value) return;
                _steampath = value;
                OnPropertyChanged(nameof(Steampath));
            }
        }

        private bool _hideConsole = true;
        public bool HideConsole
        {
            get => _hideConsole;
            set
            {
                if (_hideConsole == value) return;
                _hideConsole = value;
                OnPropertyChanged(nameof(HideConsole));
            }
        }

        private bool _keepOpen = true;
        public bool KeepOpen
        {
            get => _keepOpen;
            set
            {
                if (_keepOpen == value) return;
                _keepOpen = value;
                OnPropertyChanged(nameof(KeepOpen));
            }
        }

        private bool _showStartupMessage = true;
        public bool ShowStartupMessage
        {
            get => _showStartupMessage;
            set
            {
                if (_showStartupMessage == value) return;
                _showStartupMessage = value;
                OnPropertyChanged(nameof(ShowStartupMessage));
            }
        }

        private int _kbGamePadSelect = 0;
        public int KbGamePadSelect
        {
            get => _kbGamePadSelect;
            set
            {
                if (_kbGamePadSelect == value) return;
                _kbGamePadSelect = value;
                OnPropertyChanged(nameof(KbGamePadSelect));
            }
        }

        private bool _darkMode = false;
        public bool DarkMode
        {
            get => _darkMode;
            set
            {
                if (_darkMode == value) return;
                _darkMode = value;
                OnPropertyChanged(nameof(DarkMode));
            }
        }

        private string _tmpDownloadFolder = string.Empty;
        public string TmpDownloadFolder
        {
            get => _tmpDownloadFolder;
            set
            {
                if (_tmpDownloadFolder == value) return;
                _tmpDownloadFolder = value;
                OnPropertyChanged(nameof(TmpDownloadFolder));
            }
        }

        private bool _showedOnce = false;
        public bool ShowedOnce
        {
            get => _showedOnce;
            set
            {
                if (_showedOnce == value) return;
                _showedOnce = value;
                OnPropertyChanged(nameof(ShowedOnce));
            }
        }

        private int _logWindowWidth = 800;
        public int LogWindowWidth
        {
            get => _logWindowWidth;
            set
            {
                if (_logWindowWidth == value) return;
                _logWindowWidth = value;
                OnPropertyChanged(nameof(LogWindowWidth));
            }
        }

        private int _logWindowHeight = 450;
        public int LogWindowHeight
        {
            get => _logWindowHeight;
            set
            {
                if (_logWindowHeight == value) return;
                _logWindowHeight = value;
                OnPropertyChanged(nameof(LogWindowHeight));
            }
        }

        private int _mainWindowX = 400;
        public int MainWindowX
        {
            get => _mainWindowX;
            set
            {
                if (_mainWindowX == value) return;
                _mainWindowX = value;
                OnPropertyChanged(nameof(MainWindowX));
            }
        }

        private int _mainWindowY = 400;
        public int MainWindowY
        {
            get => _mainWindowY;
            set
            {
                if (_mainWindowY == value) return;
                _mainWindowY = value;
                OnPropertyChanged(nameof(MainWindowY));
            }
        }

        private int _modinfoWindowX = 1600;
        public int ModInfoWindowX
        {
            get => _modinfoWindowX;
            set
            {
                if (_modinfoWindowX == value) return;
                _modinfoWindowX = value;
                OnPropertyChanged(nameof(ModInfoWindowX));
            }
        }

        private int _modinfoWindowY = 800;
        public int ModInfoWindowY
        {
            get => _modinfoWindowY;
            set
            {
                if (_modinfoWindowY == value) return;
                _modinfoWindowY = value;
                OnPropertyChanged(nameof(ModInfoWindowY));
            }
        }

        private bool _autoupdate = true;
        public bool AutoUpdate
        {
            get => _autoupdate;
            set
            {
                if (_autoupdate == value) return;
                _autoupdate = value;
                OnPropertyChanged(nameof(AutoUpdate));
            }
        }

        private bool _autoCheckModVersion = true;
        public bool AutoCheckModVersion
        {
            get => _autoCheckModVersion;
            set
            {
                if (_autoCheckModVersion == value) return;
                _autoCheckModVersion = value;
                OnPropertyChanged(nameof(AutoCheckModVersion));
            }
        }

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

    public class SettingsManager
    {
        private static readonly Lazy<SettingsManager> _instance = new(() => new SettingsManager());

        public static SettingsManager Instance => _instance.Value;

        //public AppSettings Settings { get; private set; }

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

        private void Load()
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
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();

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
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                try
                {
                    if (!Directory.Exists(SettingsPath))
                        Directory.CreateDirectory(SettingsPath);

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

        public void UpdateAutoUpdate(bool autoupdate)
        {
            _settings.AutoUpdate = autoupdate;
            Save();
        }

        public void UpdateCheckModVersion(bool autocheckmodversion)
        {
            _settings.AutoCheckModVersion = autocheckmodversion;
            Save();
        }


    }
}