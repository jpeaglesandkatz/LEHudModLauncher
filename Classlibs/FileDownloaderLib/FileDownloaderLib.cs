using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LogUtils;

namespace DownloadUtils
{
    public class FileDownloader
    {
        //Logger logger = new Logger();

        public void DownloadFile(string url, string destinationPath, string filename, int maxRedirects = 5)
        {
            
            try
            {
                //Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                Logger.Global.Info($"Downloading file...{url} --- > {destinationPath}");
                string currentUrl = url;
                int redirectCount = 0;

                while (true)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(currentUrl);
                    request.Method = "GET";
                    request.UserAgent = "FileDownloader/1.0";
                    request.Timeout = 30000;
                    request.ReadWriteTimeout = 30000;
                    request.AllowAutoRedirect = false; // we'll handle manually

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        // ✅ Handle redirects manually
                        if (IsRedirect(response.StatusCode))
                        {
                            if (redirectCount >= maxRedirects)
                                throw new Exception("Too many redirects.");

                            string? redirectUrl = response.Headers["Location"];
                            if (string.IsNullOrEmpty(redirectUrl))
                                throw new Exception("Redirect response missing Location header.");

                            if (!Uri.TryCreate(new Uri(currentUrl), redirectUrl, out Uri? newUri))
                                throw new Exception($"Invalid redirect URL: {redirectUrl}");

                            currentUrl = newUri.ToString();
                            redirectCount++;
                            continue; // try again with the new URL
                        }

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Logger.Global.Error($"ERROR {response.StatusCode} Downloading file...{url} --- > {destinationPath}");
                            throw new Exception($"Server returned status code {response.StatusCode}");
                        }
                        

                        long totalBytes = response.ContentLength;
                        long receivedBytes = 0;

                        using (Stream responseStream = response.GetResponseStream())
                        using (FileStream fileStream = new FileStream(Path.Combine(destinationPath, filename), FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                                receivedBytes += bytesRead;

                                // Optional: show progress if known
                                if (totalBytes > 0)
                                {
                                    double percent = (receivedBytes / (double)totalBytes) * 100;
                                    Logger.Global.Debug($"\rDownloading... {percent:0.0}%");
                                }
                            }
                        }

                        Console.WriteLine("\nDownload completed successfully!");
                        Logger.Global.Debug($"Download completed successfully!");
                        break; // exit redirect loop
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
