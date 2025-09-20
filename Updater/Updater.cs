using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using LEHuDModLauncher.DownloadUtils;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace LEHuDModLauncher.Updater
{

    public class Updater
    {
        private static readonly HttpClient Http = new HttpClient();

        private static readonly FileDownloader FileDownloader = new FileDownloader();

        public static async Task<bool> CheckForUpdatesAsync(
            string updateUrl, string currentVersion, string tempDir)
        {
            Console.WriteLine("Checking for updates...");

            // Fetch update.json
            
            
            string json = await Http.GetStringAsync(updateUrl);
            var info = JsonSerializer.Deserialize<UpdateInfo>(json);
            if (info == null) throw new Exception("Invalid update.json");

            if (info.Version == currentVersion)
            {
                MessageBox.Show("You already have the latest version.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            MessageBox.Show("A new version is available. Downloading...", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            tempDir = Path.GetTempPath();
            foreach (var archive in info.Archives)
            {
                Console.WriteLine($"Processing {archive.Name}...");

                string fileName = Path.GetFileName(new Uri(archive.Url).AbsolutePath);
                string archivePath = Path.Combine(tempDir, fileName);

                try
                {
                    await FileDownloader.DownloadFileAsync(archive.Url, tempDir, fileName, 20);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"There was a problem downloading the update!\n{e.Message}");
                    return false;
                }
                

                // Verify SHA256
                if (!VerifySha256(archivePath, archive.Sha256))
                    throw new Exception($"{archive.Name}: Archive SHA256 mismatch!");

                Console.WriteLine($"{archive.Name}: Archive hash OK.");

                // Extract based on extension
                string extractDir = Path.Combine(tempDir, archive.Name);
                if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
                Directory.CreateDirectory(extractDir);

                if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    ZipFile.ExtractToDirectory(archivePath, extractDir);
                }
                else if (archivePath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
                {
                    ExtractRar(archivePath, extractDir);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported archive type: {archivePath}");
                }

                // Verify per-file hashes
                if (archive.Files != null)
                {
                    foreach (var kv in archive.Files)
                    {
                        string relPath = kv.Key.Replace('/', Path.DirectorySeparatorChar);
                        string filePath = Path.Combine(extractDir, relPath);

                        if (!File.Exists(filePath))
                            throw new Exception($"Missing file {relPath} in {archive.Name}");

                        if (!VerifySha256(filePath, kv.Value))
                            throw new Exception($"Hash mismatch for {relPath} in {archive.Name}");
                    }

                    Console.WriteLine($"{archive.Name}: All per-file hashes OK.");
                }
            }

            return true;
        }

        private static void ExtractRar(string rarPath, string outputDir)
        {
            using var archive = ArchiveFactory.Open(rarPath);
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    entry.WriteToDirectory(outputDir, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        private static bool VerifySha256(string filePath, string expectedHash)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha.ComputeHash(stream);
            string actual = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            return string.Equals(actual, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}