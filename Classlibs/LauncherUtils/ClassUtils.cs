using LogUtils;
using SettingsManager;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Net.NetworkInformation;
using static Microsoft.Win32.Registry;

namespace ClassUtils;

public class Utils()

{
    public static void CopyFolder(string sourceDir, string destinationDir, bool overwrite = true)
    {
        // Make sure source exists
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source folder not found: {sourceDir}");

        // Create destination if missing
        Directory.CreateDirectory(destinationDir);

        // Copy files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite);
        }

        // Copy subdirectories
        foreach (var subdir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destinationDir, Path.GetFileName(subdir));
            CopyFolder(subdir, destSubDir, overwrite); // recursion
        }
    }

    public bool CheckUserHasInternet()

    {
        try
        {
            using var ping = new Ping();
            var reply = ping.Send("1.1.1.1", 2000); // 3 second timeout
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    internal static string? GetPathFromRegistry(string regpath, string valuename)
    {

        if (string.IsNullOrEmpty(regpath) || string.IsNullOrEmpty(valuename))
            return null;

        var parts = regpath.Split('\\', 2);
        if (parts.Length != 2)
            return null;

        var baseKey = parts[0].ToUpper() switch
        {
            "HKEY_LOCAL_MACHINE" => LocalMachine,
            "HKEY_CURRENT_USER" => CurrentUser,
            _ => null
        };


        using var subKey = baseKey?.OpenSubKey(parts[1]);

        return subKey?.GetValue(valuename) as string;
    }


    public static Task ExtractFileLib(string filename, string sourcepath, string destinationpath, bool deloriginal = false)
    {
        var archive = ArchiveFactory.Open(Path.Combine(sourcepath, filename));
        try
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    //Logger.Global.Info(entry.Key);
                    entry.WriteToDirectory(destinationpath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                        PreserveFileTime = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"Extraction failed: {ex.Message}\n{ex.StackTrace}");

        }
        finally
        {
            archive.Dispose();
        }
        if (deloriginal)
        {
            File.Delete(Path.Combine(sourcepath, filename));
        }

        return Task.CompletedTask;
    }

    public async Task ExtractFile(string filename, string? sourcepath, string? destinationpath, bool deloriginal = false)
    {
        try
        {
            if (sourcepath != null)
            {
                Logger.Global.Info(
                    $"Extracting {Path.Combine(sourcepath, filename)} to\n{Path.Combine(destinationpath ?? throw new ArgumentNullException(nameof(destinationpath)))} \n");
                //if (File.Exists(Path.Combine(destinationpath, filename))) File.Delete(Path.Combine(destinationpath, filename));

                await ExtractFileLib(filename, sourcepath, destinationpath, deloriginal);
            }
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"Extraction failed: {ex.Message}");
        }
        Logger.Global.Info($"Extracted {filename}");
    }

    public bool IsLocalFileUpToDate(string localFilePath, string remoteFilePath)
    {
        if (!File.Exists(localFilePath))
            return false;
        var (cremoteFileDate, cremoteFileSize) = GetRemoteFileInfo(remoteFilePath);
        var localFileInfo = new FileInfo(localFilePath);

        // If server provided a valid Last-Modified date, prefer using it.
        if (cremoteFileDate != DateTime.MinValue)
        {
            try
            {
                if (localFileInfo.LastWriteTimeUtc >= cremoteFileDate.ToUniversalTime())
                {
                    Logger.Global.Debug($"Local file '{localFilePath}' is up to date by LastWriteTimeUtc ({localFileInfo.LastWriteTimeUtc} >= {cremoteFileDate.ToUniversalTime()}).");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Warning($"Timestamp comparison failed for '{localFilePath}': {ex.Message}");
            }
        }

        // Fall back to size comparison if server provided Content-Length.
        if (cremoteFileSize < 0) return false;
        {
            try
            {
                if (localFileInfo.Length == cremoteFileSize)
                {
                    Logger.Global.Debug($"Local file '{localFilePath}' is up to date by size ({localFileInfo.Length} == {cremoteFileSize}).");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Global.Error($"Size comparison failed for '{localFilePath}': {ex.Message}");
            }
        }

        // Not up to date by either metric.
        return false;
    }

    public bool IsLocalFileUpToDateLocal(string localFilePath, string remoteFilePath)
    {
        if (!File.Exists(localFilePath) || !File.Exists(remoteFilePath))
            return false;

        try
        {
            var localFileInfo = new FileInfo(localFilePath);
            var remoteFileInfo = new FileInfo(remoteFilePath);
            if (localFileInfo.LastWriteTimeUtc >= remoteFileInfo.LastWriteTimeUtc)
            {
                Logger.Global.Debug($"Local file '{localFilePath}' is up to date by LastWriteTimeUtc ({localFileInfo.LastWriteTimeUtc} >= {remoteFileInfo.LastWriteTimeUtc}).");
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Global.Warning($"Timestamp comparison failed for '{localFilePath}': {ex.Message}");
        }

        return false;
    }

    public static DateTime GetRemoteFileDate(string url)
    {
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        var response = client.Send(request);
        return response.Content.Headers.LastModified?.UtcDateTime ?? DateTime.MinValue;
    }

    public (DateTime LastModified, long ContentLength) GetRemoteFileInfo(string url)
    {
        try
        {
            if (!url.StartsWith("http"))
            {
                var fileInfo = new FileInfo(url);
                var lastmodified = fileInfo.LastWriteTimeUtc;
                var filesize = fileInfo.Length;
                return (lastmodified, filesize);
            }
            else
            {

                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = client.Send(request);

                var lastModified = DateTime.MinValue;
                long contentLength = -1;

                if (response.Content.Headers.LastModified.HasValue)
                    lastModified = response.Content.Headers.LastModified.Value.UtcDateTime;

                if (response.Content.Headers.ContentLength.HasValue)
                    contentLength = response.Content.Headers.ContentLength.Value;

                return (lastModified, contentLength);
            }
        }
        catch (Exception ex)
        {
            Logger.Global.Error($"GetRemoteFileInfo failed for {url}: {ex.Message}");
            return (DateTime.MinValue, -1);
        }
    }

    public void SetHideConsole(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            Logger.Global.Error($"Config file {configFilePath} does not exist.");
            return;
        }

        var lines = File.ReadAllLines(configFilePath);
        var changed = false;
        var enabledconsole = Config.Instance.Settings.HideConsole;

        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Trim().StartsWith("hide_console", StringComparison.OrdinalIgnoreCase)) continue;
            if (!lines[i].Trim().Contains("hide_console = false", StringComparison.OrdinalIgnoreCase) &&
                !lines[i].Trim().Contains("hide_console = true", StringComparison.OrdinalIgnoreCase)) continue;
            if (enabledconsole) lines[i] = "hide_console = true"; else lines[i] = "hide_console = false";
            changed = true;
        }

        if (changed)
            File.WriteAllLines(configFilePath, lines);
    }
}