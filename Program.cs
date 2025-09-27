
using LEHuDModLauncher;
using LEHuDModLauncher.Classlibs.LogUtils;
using static LEHuDModLauncher.Classlibs.LogUtils.Log;
using static LEHuDModLauncher.Settings.Config;

namespace LEHudModLauncher
{
    public static class Program
    {

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Logger.Configure(folder: SettingsManager.Instance.Settings.LogPath ?? null, level: Logger.LogLevel.Debug);
            Application.Run(new Launcherform());
        }
    }
}