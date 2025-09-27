using System.Net;
using static LEHuDModLauncher.Classlibs.LogUtils.Log;

namespace LEHuDModLauncher.Classlibs.FileDownloaderLib;

public class DownloadLib
{
    public class FileDownloader
    {
        private readonly HttpClient _httpClient;

        public class DownloadEntry
        {
            public required string Id { get; set; }
            public required string Name { get; set; }
            public required string Url { get; set; }
            public required string Destination { get; set; }
            public required string Filename { get; set; }
        }

        public class DownloadList
        {
            public DownloadList(List<DownloadEntry> files, string version)
            {
                Files = files;
                Version = version;
            }

            public string Version { get; set; }
            public required List<DownloadEntry> Files { get; init; }
        }

        public FileDownloader()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false // we handle redirects manually
            };

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FileDownloader/1.0");
        }

        public async Task DownloadFileAsync(string url, string destinationPath, string filename, int maxRedirects = 5)
        {
            try
            {
                Logger.Global.Info($"Downloading file... {url} ---> {destinationPath}");
                var currentUrl = url;
                var redirectCount = 0;

                while (true)
                {
                    using var response =
                        await _httpClient.GetAsync(currentUrl, HttpCompletionOption.ResponseHeadersRead);
                    // ✅ Handle redirects manually
                    if (IsRedirect(response.StatusCode))
                    {
                        if (redirectCount >= maxRedirects)
                            throw new Exception("Too many redirects.");

                        if (response.Headers.Location == null)
                            throw new Exception("Redirect response missing Location header.");

                        var newUri = response.Headers.Location.IsAbsoluteUri
                            ? response.Headers.Location
                            : new Uri(new Uri(currentUrl), response.Headers.Location);

                        currentUrl = newUri.ToString();
                        redirectCount++;
                        continue;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine(
                            $"ERROR {response.StatusCode} Downloading file...{url} ---> {destinationPath}");
                        Logger.Global.Error(
                            $"ERROR {response.StatusCode} Downloading file...{url} ---> {destinationPath}");
                        throw new Exception($"Server returned status code {response.StatusCode}");
                    }

                    var totalBytes = response.Content.Headers.ContentLength;
                    long receivedBytes = 0;

                    await using (var stream = await response.Content.ReadAsStreamAsync())
                    await using (var fileStream = new FileStream(Path.Combine(destinationPath, filename),
                                     FileMode.Create,
                                     FileAccess.Write,
                                     FileShare.None,
                                     bufferSize: 8192,
                                     useAsync: true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                        {
                            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                            receivedBytes += bytesRead;

                            // Optional: progress logging
                            if (totalBytes is > 0)
                            {
                                var percent = receivedBytes / (double)totalBytes.Value * 100;
                                //Logger.Global.Debug($"Downloading... {percent:0.0}%");
                            }
                        }
                    }

                    Console.WriteLine($"\nDownload completed successfully! __ {url}");
                    Logger.Global.Debug($"Download completed successfully!  --> {url}");
                    break; // exit loop
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Debug($"\nError downloading file: {ex.Message}");
                Console.WriteLine($"\nError downloading file: {ex.Message}");
                throw;
            }
        }

        private static bool IsRedirect(HttpStatusCode code)
        {
            return code == HttpStatusCode.Moved ||
                   code == HttpStatusCode.Redirect ||
                   code == HttpStatusCode.RedirectMethod ||
                   code == HttpStatusCode.TemporaryRedirect ||
                   (int)code == 308; // PermanentRedirect (not in enum)

        }
    }
}