
using LEHuDModLauncher;
using static LEHuDModLauncher.Classlibs.LogUtils.Log;
using static LEHuDModLauncher.Settings.Config;

namespace LEHudModLauncher;

public static class Program
{

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Logger.Configure(folder: SettingsManager.Instance.Settings.LogPath, level: Logger.LogLevel.Debug);
        Application.Run(new Launcherform());
    }
}