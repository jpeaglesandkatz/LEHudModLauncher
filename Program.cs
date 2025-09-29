
using LEHuDModLauncher;
using LogUtils;
using SettingsManager;

namespace LEHudModLauncher;

public static class Program
{

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Logger.Configure(folder: Config.Instance.Settings.LogPath, level: Logger.LogLevel.Debug);
        Application.Run(new Launcherform());
    }
}