using System.Text;
using System.Text.RegularExpressions;

namespace LEHuDModLauncher.Classlibs;

internal class UnityVersionExtractor
{
    // Keywords that, if present near a version string, increase confidence
    private static readonly string[] Keywords =
    [
        "bundleVersion", "bundleversion", "PlayerSettings", "application.version",
        "applicationversion", "productversion", "m_EditorVersion", "m_EngineVersion",
        "gameVersion", "m_GameVersion", "version", "app.info"
    ];

    // Regexes for likely version forms
    private static readonly Regex UnityEditorPattern = new Regex(@"\b\d{4}\.\d+\.\d+[abfp]\d+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SemVer3Pattern = new Regex(@"\b\d+\.\d+\.\d+(?:\.\d+)?\b", RegexOptions.Compiled);
    private static readonly Regex ShortVerPattern = new Regex(@"\b\d+\.\d+\b", RegexOptions.Compiled);


    private static List<(string version, string context, int score)> ExtractVersionsFromGlobalGameManagers(string exePath)
    {
        ArgumentNullException.ThrowIfNull(exePath);
        if (!File.Exists(exePath)) throw new FileNotFoundException("Executable not found", exePath);

        var gameDir = Path.GetDirectoryName(exePath);
        Path.GetFileNameWithoutExtension(exePath);
        var ggmPath = gameDir + @"\Last Epoch_Data\globalgamemanagers";

        if (!File.Exists(ggmPath))
            throw new FileNotFoundException("globalgamemanagers not found", ggmPath);

        var data = File.ReadAllBytes(ggmPath);

        // Extract printable ASCII chunks to reduce noise
        var printableChunks = GetPrintableStringsFromBytes(data, minLength: 6);

        var candidates = new Dictionary<string, (string context, int score)>(StringComparer.OrdinalIgnoreCase);

        // Scan each chunk for patterns and keywords
        foreach (var chunk in printableChunks)
        {
            // Boost if chunk contains keyword
            var hasKeyword = Keywords.Any(k => chunk.Contains(k, StringComparison.OrdinalIgnoreCase));

            // 1) Editor-style (2021.3.15f1)
            foreach (Match m in UnityEditorPattern.Matches(chunk))
            {
                AddCandidate(candidates, m.Value, chunk, baseScore: 150 + (hasKeyword ? 40 : 0));
            }

            // 2) Full semantic version (1.2.3 or 1.2.3.456)
            foreach (Match m in SemVer3Pattern.Matches(chunk))
            {
                AddCandidate(candidates, m.Value, chunk, baseScore: 100 + (hasKeyword ? 30 : 0));
            }

            // 3) Fallback short versions (1.2) - lower score (avoid noise)
            foreach (Match m in ShortVerPattern.Matches(chunk))
            {
                // avoid counting something already captured by semver (e.g. "1.2.3" contains "1.2")
                if (!SemVer3Pattern.IsMatch(m.Value))
                    AddCandidate(candidates, m.Value, chunk, baseScore: 40 + (hasKeyword ? 10 : 0));
            }
        }

        // If nothing found in printable chunks, scan entire file text as fallback
        if (candidates.Count == 0)
        {
            var allText = Encoding.UTF8.GetString(data);
            foreach (Match m in UnityEditorPattern.Matches(allText)) AddCandidate(candidates, m.Value, "...(from file)...", 120);
            foreach (Match m in SemVer3Pattern.Matches(allText)) AddCandidate(candidates, m.Value, "...(from file)...", 80);
        }

        // Produce ordered result
        var ordered = candidates
            .Select(kv => (version: kv.Key, kv.Value.context, kv.Value.score))
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.version)
            .ToList();

        return ordered;
    }

    private static void AddCandidate(Dictionary<string, (string context, int score)> dict, string version, string context, int baseScore)
    {
        if (string.IsNullOrWhiteSpace(version)) return;
        if (dict.TryGetValue(version, out var existing))
        {
            // keep the highest score and a context that contains a keyword if possible
            var newScore = Math.Max(existing.score, baseScore);
            var newCtx = existing.context.Length >= context.Length ? existing.context : context;
            dict[version] = (newCtx, newScore);
        }
        else
        {
            dict[version] = (context.Length > 200 ? context[..200] : context, baseScore);
        }
    }

    // Extract contiguous sequences of printable ASCII characters (reduces binary noise).
    static IEnumerable<string> GetPrintableStringsFromBytes(byte[]? data, int minLength = 6)
    {
        if (data == null) yield break;

        var sb = new StringBuilder();
        foreach (var b in data)
        {
            if (b >= 32 && b <= 126) // printable ASCII
            {
                sb.Append((char)b);
            }
            else
            {
                if (sb.Length >= minLength)
                {
                    yield return sb.ToString();
                }
                sb.Clear();
            }
        }

        if (sb.Length >= minLength) yield return sb.ToString();
    }
}