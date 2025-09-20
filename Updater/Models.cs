
namespace LEHuDModLauncher.Updater
{
    public class UpdateInfo
    {
        public string Version { get; set; } = "";
        public List<ArchiveInfo> Archives { get; set; } = new();
    }

    public class ArchiveInfo
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public Dictionary<string, string>? Files { get; set; }
    }
}
