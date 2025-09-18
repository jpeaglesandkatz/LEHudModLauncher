using Newtonsoft.Json.Linq;
using System;
using System.IO;


class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: UpdateJsonGenerator.exe <version> <zipPath> <baseUrl>");
            return;
        }

        string version = args[0];
        string zipPath = args[1];
        string baseUrl = args[2].TrimEnd('/');

        if (!File.Exists(zipPath))
        {
            Console.WriteLine("ZIP file not found: " + zipPath);
            return;
        }

        string fileName = Path.GetFileName(zipPath);
        string url = $"{baseUrl}/{fileName}";

        var json = new JObject
        {
            ["version"] = version,
            ["url"] = url
        };

        string outputPath = Path.Combine(Path.GetDirectoryName(zipPath), "update.json");
        File.WriteAllText(outputPath, json.ToString());

        Console.WriteLine("update.json generated at: " + outputPath);
    }
}
