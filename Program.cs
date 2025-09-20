using LEHuDModLauncher;
using LEHuDModLauncher.Classlibs;
using LEHuDModLauncher.LogUtils;
using static LauncherUtils.Utils;

namespace LEHudModLauncher
{
    public static class Program
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


            if (SettingsManager.Instance.Settings.AutoUpdate)
            {
                var updater = new UpdateChecker();
                try
                {
                    updater.CheckForUpdateAsync(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for updates: {ex.Message}", "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            if (File.Exists(@"C:\Users\jp\AppData\Roaming\LEHuDModLauncher\debug.log")) File.Delete(@"C:\Users\jp\AppData\Roaming\LEHuDModLauncher\debug.log");
            Logger.Configure(folder: SettingsManager.Instance.Settings.LogPath ?? null, level: LogLevel.Debug);
            Application.Run(new Launcherform());
        }
    }
}