using System;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

public static class UpdateChecker
{
    private const string UpdateUrl = "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/update.json";

    public static void CheckForUpdates()
    {
        try
        {
            using (var client = new WebClient())
            {
                string json = client.DownloadString(UpdateUrl);
                JObject obj = JObject.Parse(json);

                Version latestVersion = new Version((string)obj["version"]);
                string downloadUrl = (string)obj["url"];
                Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                if (latestVersion > currentVersion)
                {
                    DialogResult dr = MessageBox.Show(
                        $"A new version {latestVersion} is available. Update now?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    if (dr == DialogResult.Yes)
                    {
                        StartUpdater(downloadUrl);
                        Application.Exit();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Update check failed: " + ex.Message);
        }
    }

    private static void StartUpdater(string url)
    {
        string exePath = Application.ExecutablePath;
        string updaterPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

        if (System.IO.File.Exists(updaterPath))
        {
            System.Diagnostics.Process.Start(updaterPath, $"\"{url}\" \"{exePath}\"");
        }
        else
        {
            MessageBox.Show("Updater not found!");
        }
    }
}
