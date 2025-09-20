using LEHuDModLauncher.DownloadUtils;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO.Compression;
using System.Security.Cryptography;

namespace LEHuDModLauncher.Updater
{
    public partial class UpdateForm : Form
    {
        private CancellationTokenSource _cts = new();

        private static FileDownloader fileDownloader = new();

        public UpdateForm()
        {
            InitializeComponent();
        }

        public async Task RunUpdateAsync(UpdateInfo updateInfo, string installPath)
        {
            var progress = new Progress<int>(percent =>
            {
                progressBar.Value = Math.Min(progressBar.Maximum, Math.Max(progressBar.Minimum, percent));
                lblStatus.Text = $"Updating... {percent}%";
            });

            try
            {
                lblStatus.Text = "Starting update...";
                progressBar.Value = 0;

                await DownloadAndInstall(updateInfo, installPath, progress, _cts.Token);

                lblStatus.Text = "Update completed successfully.";
                progressBar.Value = 100;
                await Task.Delay(1000);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Update canceled.";
                DialogResult = DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
                MessageBox.Show(ex.ToString(), "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Abort;
            }
        }

        private async Task DownloadAndInstall(UpdateInfo updateInfo, string installPath, IProgress<int> progress, CancellationToken ct)
        {
            using var http = new HttpClient();

            int totalSteps = updateInfo.Archives.Count * 2; // download + extract
            int currentStep = 0;

            foreach (var archive in updateInfo.Archives)
            {
                ct.ThrowIfCancellationRequested();

                string tempFilePath = Path.GetFullPath(Path.GetTempPath());
                string tempFile = Path.Combine(tempFilePath, archive.Name);

                // Download archive
                lblStatus.Text = $"Downloading {archive.Name}...";

                try
                {
                    if (File.Exists(tempFile)) File.Delete(tempFile);
                    Task task = fileDownloader.DownloadFileAsync(archive.Url, tempFilePath, archive.Name, 20);
                    await task;
                    if (task.IsFaulted) { MessageBox.Show($"TASK FAILED: {task.Exception}\n{task.Exception.Message}"); throw task.Exception; }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Download of update failed!\n{ex.Message}");
                    return;
                }
                                

                //await fileDownloader.DownloadFileAsync(archive.Url, tempFilePath, archive.Name, 5);


                //using (var response = await http.GetAsync(archive.Url, HttpCompletionOption.ResponseHeadersRead, ct))
                //{
                //    response.EnsureSuccessStatusCode();
                //    using var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);
                //    await response.Content.CopyToAsync(fs, ct);
                //}

                // Verify checksum
                string hash = ComputeSha256(tempFile);
                if (!hash.Equals(archive.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Checksum mismatch for {archive.Name}. Expected {archive.Sha256}, got {hash}");
                }

                currentStep++;
                progress.Report((int)((double)currentStep / totalSteps * 100));

                // Extract archive
                lblStatus.Text = $"Extracting {archive.Name}...";
                if (tempFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    using var zip = ZipFile.OpenRead(tempFile);
                    foreach (var entry in zip.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name)) continue; // skip directories
                        string destFile = Path.Combine(installPath, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                        entry.ExtractToFile(destFile, true);
                    }
                }
                else if (tempFile.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                {
                    using var rar = ArchiveFactory.Open(tempFile);
                    foreach (var entry in rar.Entries)
                    {
                        if (entry.IsDirectory) continue;
                        string destFile = Path.Combine(installPath, entry.Key);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                        entry.WriteToFile(destFile, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                    }
                }
                else
                {
                    throw new Exception($"Unsupported archive type: {tempFile}");
                }

                currentStep++;
                progress.Report((int)((double)currentStep / totalSteps * 100));
            }
        }

        private string ComputeSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;
            lblStatus.Text = "Canceling...";
            _cts.Cancel();
        }
    }
}
