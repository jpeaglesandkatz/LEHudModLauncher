using System.Net;
using System.Net.Http;
using LEHuDModLauncher.LogUtils;

namespace LEHuDModLauncher.DownloadUtils
{
    public class FileDownloader
    {
        private readonly HttpClient _httpClient;

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
                string currentUrl = url;
                int redirectCount = 0;

                while (true)
                {
                    using (HttpResponseMessage response = await _httpClient.GetAsync(currentUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        // ✅ Handle redirects manually
                        if (IsRedirect(response.StatusCode))
                        {
                            if (redirectCount >= maxRedirects)
                                throw new Exception("Too many redirects.");

                            if (response.Headers.Location == null)
                                throw new Exception("Redirect response missing Location header.");

                            Uri newUri = response.Headers.Location.IsAbsoluteUri
                                ? response.Headers.Location
                                : new Uri(new Uri(currentUrl), response.Headers.Location);

                            currentUrl = newUri.ToString();
                            redirectCount++;
                            continue;
                        }

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Logger.Global.Error($"ERROR {response.StatusCode} Downloading file...{url} ---> {destinationPath}");
                            throw new Exception($"Server returned status code {response.StatusCode}");
                        }

                        long? totalBytes = response.Content.Headers.ContentLength;
                        long receivedBytes = 0;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(Path.Combine(destinationPath, filename),
                                                              FileMode.Create,
                                                              FileAccess.Write,
                                                              FileShare.None,
                                                              bufferSize: 8192,
                                                              useAsync: true))
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                receivedBytes += bytesRead;

                                // Optional: progress logging
                                if (totalBytes.HasValue && totalBytes.Value > 0)
                                {
                                    double percent = (receivedBytes / (double)totalBytes.Value) * 100;
                                    Logger.Global.Debug($"Downloading... {percent:0.0}%");
                                }
                            }
                        }

                        Console.WriteLine("\nDownload completed successfully!");
                        Logger.Global.Debug("Download completed successfully!");
                        break; // exit loop
                    }
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
