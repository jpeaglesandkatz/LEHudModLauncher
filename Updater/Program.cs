
namespace LEHuDModLauncher.Updater
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string updateUrl = "https://github.com/jpeaglesandkatz/LEHudModLauncher/releases/download/1.0/update_test.json"; 
            string currentVersion = "1.1";

            var updateInfo = await UpdateChecker.GetUpdateInfoAsync(updateUrl);
            if (updateInfo == null)
            {
                MessageBox.Show("Could not check for updates.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (UpdateChecker.IsUpdateAvailable(currentVersion, updateInfo))
            {
                var updateForm = new UpdateForm();
                updateForm.Show();
                await updateForm.RunUpdateAsync(updateInfo, AppDomain.CurrentDomain.BaseDirectory);
            }
            else
            {
                MessageBox.Show("You already have the latest version.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
