using LEHuDModLauncher;
using LogUtils;
using static LauncherUtils.Utils;

namespace LEHudModLauncher
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Logger.Configure(folder: SettingsManager.Instance.Settings.LogPath ?? null, level: LogLevel.Info);
            Application.Run(new Launcherform());
        }
    }
}