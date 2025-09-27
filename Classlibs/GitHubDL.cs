using System.Text.Json;

namespace LEHuDModLauncher.Classlibs

{
    public class GitHubReleaseDownloader
    {

        public class GitHubContentItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string Type { get; set; }  // "file" or "dir"
            public string Download_Url { get; set; }
        }

        public class GitHubFolderDownloader
        {
            private static readonly HttpClient httpClient = new HttpClient();

            public async Task<List<GitHubContentItem>> FetchFolderFilesAsync(string user, string repo, string branch, string folder)
            {
                string apiUrl = $"https://api.github.com/repos/{user}/{repo}/contents/{folder}";

                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("dllatest-Updater/1.0");

                string json = await httpClient.GetStringAsync(apiUrl);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<GitHubContentItem>>(json, options);
            }

            public async Task DownloadFilesAsync(List<GitHubContentItem> files, string destinationFolder)
            {
                Directory.CreateDirectory(destinationFolder);

                foreach (var file in files.Where(f => f.Type == "file"))
                {
                    string filePath = Path.Combine(destinationFolder, file.Name);
                    Console.WriteLine($"⬇️ Downloading {file.Name}...");

                    using var response = await httpClient.GetAsync(file.Download_Url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(fs);

                    Console.WriteLine($"✅ Saved to {filePath}");
                }
            }
        }
    }
}




