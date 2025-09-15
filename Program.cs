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
            if (File.Exists(@"C:\Users\jp\AppData\Roaming\LEHuDModLauncher\debug.log")) File.Delete(@"C:\Users\jp\AppData\Roaming\LEHuDModLauncher\debug.log");
            Logger.Configure(folder: SettingsManager.Instance.Settings.LogPath ?? null, level: LogLevel.Debug);
            Application.Run(new Launcherform());
        }
    }
}