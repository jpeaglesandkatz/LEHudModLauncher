using System.Text.Json;


namespace LEHuDModLauncher.Updater
{
    public static class UpdateChecker
    {

        public static async Task<UpdateInfo?> GetUpdateInfoAsync(string updateUrl)
        {
            using var http = new HttpClient();
            try
            {
                string json = await http.GetStringAsync(updateUrl);
                return JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update check failed: {ex.Message}");
                return null;
            }
        }


        public static bool IsUpdateAvailable(string currentVersion, UpdateInfo updateInfo)
        {
            if (string.IsNullOrWhiteSpace(updateInfo.Version))
                return false;

            if (Version.TryParse(updateInfo.Version, out var latest) &&
                Version.TryParse(currentVersion, out var current))
            {
                return latest > current;
            }

            // If version parsing fails, fall back to string compare
            return !string.Equals(updateInfo.Version, currentVersion, StringComparison.OrdinalIgnoreCase);
        }
    }
}
