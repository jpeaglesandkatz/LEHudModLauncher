using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LauncherUpdater
{
    public partial class UpdaterForm : Form
    {
        private readonly string downloadUrl;
        private readonly string appPath;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public UpdaterForm(string[] args)
        {
            InitializeComponent();

            if (args.Length < 2)
            {
                MessageBox.Show(
                    "Usage: Updater.exe <url> <appPath>",
                    "Updater",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Environment.Exit(1);
            }

            downloadUrl = args[0];
            appPath = args[1];
        }

        private async void UpdaterForm_Load(object sender, EventArgs e)
        {
            await RunUpdateAsync(cts.Token);
        }

        private async Task RunUpdateAsync(CancellationToken token)
        {
            try
            {
                string zipPath = Path.Combine(Path.GetTempPath(), "update.zip");
                string appDir = Path.GetDirectoryName(appPath);

                lblStatus.Text = "Waiting for app to close...";
                await Task.Delay(2000, token);

                lblStatus.Text = "Downloading update...";

                using (var client = new HttpClient())
                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, token))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    using (var inputStream = await response.Content.ReadAsStreamAsync(token))
                    using (var outputStream = File.Create(zipPath))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;
                        while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                        {
                            await outputStream.WriteAsync(buffer, 0, bytesRead, token);
                            totalRead += bytesRead;

                            if (canReportProgress)
                            {
                                int progress = (int)((totalRead * 100) / totalBytes);
                                progressBar1.Value = progress;
                                lblStatus.Text = $"Downloading... {progress}%";
                            }
                        }
                    }
                }

                lblStatus.Text = "Extracting update...";
                ZipFile.ExtractToDirectory(zipPath, appDir, true);
                File.Delete(zipPath);

                lblStatus.Text = "Restarting application...";
                Process.Start(appPath);

                Application.Exit();
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Update cancelled.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update failed: " + ex.Message, "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Update failed.";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cts.Cancel();
            btnCancel.Enabled = false;
            lblStatus.Text = "Cancelling...";
        }
    }
}
