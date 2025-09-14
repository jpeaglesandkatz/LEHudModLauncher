using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadUtils
{
    public class FileDownloader
    {
        private readonly HttpClient _httpClient;

        public FileDownloader()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Downloads a file asynchronously.
        /// </summary>
        /// <param name="url">The URL of the file.</param>
        /// <param name="destinationPath">Local path to save the file.</param>
        /// <param name="progress">Optional progress callback (0 to 100).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public async Task DownloadFileAsync(
            string url,
            string destinationPath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1 && progress != null;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (canReportProgress)
                {
                    double percent = (double)totalRead / totalBytes * 100;
                    progress!.Report(Math.Round(percent, 2));
                }
            }
        }
    }
}
