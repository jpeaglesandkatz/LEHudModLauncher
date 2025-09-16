using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class UnityVersionExtractor
{
	// Keywords that, if present near a version string, increase confidence
	static readonly string[] Keywords = new[]
	{
		"bundleVersion", "bundleversion", "PlayerSettings", "application.version",
		"applicationversion", "productversion", "m_EditorVersion", "m_EngineVersion",
		"gameVersion", "m_GameVersion", "version", "app.info"
	};

	// Regexes for likely version forms
	static readonly Regex UnityEditorPattern = new Regex(@"\b\d{4}\.\d+\.\d+[abfp]\d+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	static readonly Regex SemVer3Pattern = new Regex(@"\b\d+\.\d+\.\d+(?:\.\d+)?\b", RegexOptions.Compiled);
	static readonly Regex ShortVerPattern = new Regex(@"\b\d+\.\d+\b", RegexOptions.Compiled);


	public static List<(string version, string context, int score)> ExtractVersionsFromGlobalGameManagers(string exePath)
	{
		if (exePath == null) throw new ArgumentNullException(nameof(exePath));
		if (!File.Exists(exePath)) throw new FileNotFoundException("Executable not found", exePath);

		string gameDir = Path.GetDirectoryName(exePath);
		string gameName = Path.GetFileNameWithoutExtension(exePath);
		string ggmPath = Path.Combine(gameDir, gameName + "_Data", "globalgamemanagers");

		if (!File.Exists(ggmPath))
			throw new FileNotFoundException("globalgamemanagers not found", ggmPath);

		byte[] data = File.ReadAllBytes(ggmPath);

		// Extract printable ASCII chunks to reduce noise
		var printableChunks = GetPrintableStringsFromBytes(data, minLength: 6);

		var candidates = new Dictionary<string, (string context, int score)>(StringComparer.OrdinalIgnoreCase);

		// Scan each chunk for patterns and keywords
		foreach (var chunk in printableChunks)
		{
			// Boost if chunk contains keyword
			bool hasKeyword = Keywords.Any(k => chunk.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

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
			string allText = Encoding.UTF8.GetString(data);
			foreach (Match m in UnityEditorPattern.Matches(allText)) AddCandidate(candidates, m.Value, "...(from file)...", 120);
			foreach (Match m in SemVer3Pattern.Matches(allText)) AddCandidate(candidates, m.Value, "...(from file)...", 80);
		}

		// Produce ordered result
		var ordered = candidates
			.Select(kv => (version: kv.Key, context: kv.Value.context, score: kv.Value.score))
			.OrderByDescending(x => x.score)
			.ThenBy(x => x.version)
			.ToList();

		return ordered;
	}

	/// <summary>
	/// Convenience wrapper returning the single best guess (or null).
	/// </summary>
	public static string GetBestGuessVersion(string exePath)
	{
		var list = ExtractVersionsFromGlobalGameManagers(exePath);
		return list.FirstOrDefault().version;
	}

	// Helper: add or boost candidate
	static void AddCandidate(Dictionary<string, (string context, int score)> dict, string version, string context, int baseScore)
	{
		if (string.IsNullOrWhiteSpace(version)) return;
		if (dict.TryGetValue(version, out var existing))
		{
			// keep the highest score and a context that contains a keyword if possible
			int newScore = Math.Max(existing.score, baseScore);
			string newCtx = existing.context.Length >= context.Length ? existing.context : context;
			dict[version] = (newCtx, newScore);
		}
		else
		{
			dict[version] = (context.Length > 200 ? context.Substring(0, 200) : context, baseScore);
		}
	}

	// Extract contiguous sequences of printable ASCII characters (reduces binary noise).
	static IEnumerable<string> GetPrintableStringsFromBytes(byte[] data, int minLength = 6)
	{
		if (data == null) yield break;

		var sb = new StringBuilder();
		for (int i = 0; i < data.Length; i++)
		{
			byte b = data[i];
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

