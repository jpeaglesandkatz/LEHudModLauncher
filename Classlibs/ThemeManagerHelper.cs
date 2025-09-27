using MaterialSkin;
using Newtonsoft.Json;

namespace LEHuDModLauncher.Classlibs;

public static class ThemeManagerHelper
{
    private static readonly string SettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.json");

    public static void SaveTheme(Primary primary, Accent accent, MaterialSkinManager.Themes theme)
    {
        var settings = new ThemeSettings
        {
            Primary = primary.ToString(),
            Accent = accent.ToString(),
            Theme = theme.ToString()
        };

        File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
    }

    public static void LoadTheme(MaterialSkinManager skinManager)
    {
        if (!File.Exists(SettingsFile))
            return;

        try
        {
            var json = File.ReadAllText(SettingsFile);
            var settings = JsonConvert.DeserializeObject<ThemeSettings>(json);

            if (settings == null) return;
            var primary = Enum.Parse<Primary>(settings.Primary);
            var accent = Enum.Parse<Accent>(settings.Accent);
            var theme = Enum.Parse<MaterialSkinManager.Themes>(settings.Theme);

            skinManager.Theme = theme;
            skinManager.ColorScheme = new ColorScheme(primary, primary, primary, accent, TextShade.WHITE);
        }
        catch
        {
            // Ignore errors, fallback to default
        }
    }
}