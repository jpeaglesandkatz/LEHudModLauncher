using LEHuDModLauncher.Classlibs;
using System.ComponentModel;
using System.Text.RegularExpressions;
using static LEHuDModLauncher.Settings.Config;
using static System.Environment;

namespace LEHuDModLauncher;

public partial class LogViewerForm : Form
{
    private readonly string _logFilePath;
    private static readonly string[] separatorArray = new[] { "\r\n", "\n" };
    private static readonly string[] separator = new[] { "\r\n", "\n" };
    private readonly System.Windows.Forms.Timer _timer;
    private long _lastFileLength = 0;
    private int _lastLineCount = 0; // Track number of lines processed
    private string _partialLine = string.Empty; // Store incomplete lines between reads

    // Add this field near the top with other private fields
    private Color _defaultLineColor = Color.DarkGray;

    // Regex to match [anything] including timestamps and tags
    private static readonly Regex BracketRegex = new Regex(@"\[[^\]]+\]", RegexOptions.Compiled);

    // Regex to match [hh:mm:ss] or [h:mm:ss] timestamps (whole string)
    private static readonly Regex TimeRegex = new Regex(@"^\[\d{1,2}:\d{2}:\d{2}\]$", RegexOptions.Compiled);

    // Update the timestamp detection regex to be more flexible
    private static readonly Regex TimestampRegex =
        new Regex(
            @"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]|\[\d{2}:\d{2}:\d{2}\.\d{3}\]|\[\d{1,2}:\d{2}:\d{2}\]$",
            RegexOptions.Compiled);

    // Update the tag color dictionary to support wildcards and non-bracketed tags
    private readonly Dictionary<string, Color> _tagColors =
        new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            // Bracketed versions
            { "[ERROR]", Color.Red },
            { "[WARNING]", Color.Orange },
            { "[DEBUG]", Color.LightGray },
            { "[INFO]", Color.DarkGreen },
            { "[LastEpoch_Hud]", Color.Lime },
            { "[Il2CppInterop]", Color.DarkGray },
            { "[Il2CppAssemblyGenerator]", Color.DarkGray },
            { "[UnityExplorer]", Color.DarkGoldenrod },
            { "[Il2CppICallInjector]", Color.DarkGray },

            // Non-bracketed versions
            { "ERROR", Color.Red },
            { "WARNING", Color.Orange },
            { "DEBUG", Color.LightGray },
            { "INFO", Color.DarkGreen },
            { "LastEpoch_Hud", Color.Lime },
            { "Il2CppInterop", Color.DarkGray },
            { "Il2CppAssemblyGenerator", Color.DarkGray },
            { "UnityExplorer", Color.DarkGoldenrod },
            { "Il2CppICallInjector", Color.DarkGray },
            { "--- BEGIN IL2CPP STACK TRACE ---", Color.Red },

            // Wildcard examples
            { "Il2Cpp*", Color.DarkGray },
            { "[Il2Cpp*]", Color.DarkGray },
            { "*BEGIN IL2CPP STACK TRACE*", Color.Red },
            { "*LastEpoch_Hud*", Color.Lime },
            { "*Exception*", Color.Red }
        };

    // Tags that should be removed (not shown) from the displayed log.
    private readonly HashSet<string> _hiddenTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public Color DefaultLineColor
    {
        get => _defaultLineColor;
        set => _defaultLineColor = value;
    }

    public LogViewerForm(string logFilePath)
    {
        InitializeComponent();

        if (SettingsManager.Instance.Settings.LogWindowHeight < 0 ||
            SettingsManager.Instance.Settings.LogWindowWidth < 0)
        {
            SettingsManager.Instance.UpdateLogWindowHeight(500);
            SettingsManager.Instance.UpdateLogWindowWidth(900);

        }

        StartPosition = FormStartPosition.Manual;
        Location = new Point(
            SettingsManager.Instance.Settings.LogWindowWidth,
            SettingsManager.Instance.Settings.LogWindowHeight);
        ShowInTaskbar = false;

        // Don't show lines with the following tags
        AddHiddenTag("[DEBUG]");
        AddHiddenTag("[BS DEBUG]");
        AddHiddenTag("[MelonAssemblyResolver]");


        ThemeUtils.ApplyDarkTheme(this);
        _logFilePath = logFilePath;
        _timer = new System.Windows.Forms.Timer();
        _timer.Interval = 1000; // Check more frequently for better responsiveness
        _timer.Tick += async (s, e) => await UpdateLogAsync();
        _timer.Start();
    }

    // Helper method to check if a bracketed content is a timestamp
    private static bool IsTimestamp(string bracketContent)
    {
        return TimestampRegex.IsMatch(bracketContent);
    }

    // Add this method to support wildcard matching
    private static bool MatchesWildcardTag(string tag, string lineTag)
    {
        // Simple wildcard matching with * and ?
        // * matches any sequence of characters
        // ? matches any single character

        if (string.Equals(tag, lineTag, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!tag.Contains('*') && !tag.Contains('?'))
            return false;

        // Convert wildcard pattern to regex
        var pattern = "^" + Regex.Escape(tag)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".") + "$";

        return Regex.IsMatch(lineTag, pattern, RegexOptions.IgnoreCase);
    }

    public void AddHiddenTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        tag = tag.Trim();

        // Store tags as-is (with or without brackets) to support both formats
        _hiddenTags.Add(tag);

        // Also add the bracketed version if not already bracketed
        if ("[".StartsWith(tag) && tag.EndsWith("]")) return;
        var bracketedTag = "[" + tag.Trim('[', ']') + "]";
        _hiddenTags.Add(bracketedTag);
    }

    private void RemoveHiddenTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        tag = tag.Trim();
        if (!tag.StartsWith("[") || !tag.EndsWith("]"))
            tag = "[" + tag.Trim('[', ']') + "]";
        _hiddenTags.Remove(tag);
    }

    private void SetHiddenTags(IEnumerable<string>? tags)
    {
        _hiddenTags.Clear();
        if (tags == null) return;
        foreach (var t in tags) AddHiddenTag(t);
    }

    public IReadOnlyCollection<string> GetHiddenTags() => new List<string>(_hiddenTags).AsReadOnly();

    private void SetTagColor(string tag, Color color)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        _tagColors[tag.Trim()] = color;
    }

    private async Task UpdateLogAsync()
    {
        try
        {
            if (File.Exists(_logFilePath))
            {
                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length != _lastFileLength)
                {
                    // Read only the new content since last position
                    var newContent = await ReadNewContentAsync();

                    if (!string.IsNullOrEmpty(newContent))
                    {
                        richTextBoxLog.Invoke((Action)(() => ProcessNewContent(newContent)));
                    }

                    _lastFileLength = fileInfo.Length;
                }
            }
            else
            {
                richTextBoxLog.Invoke((Action)(() =>
                {
                    richTextBoxLog.Clear();
                    richTextBoxLog.SelectionColor = Color.Red;
                    richTextBoxLog.AppendText("Log file not found.");
                    _lastFileLength = 0;
                    _partialLine = string.Empty;
                }));
            }
        }
        catch (Exception ex)
        {
            richTextBoxLog.Invoke((Action)(() =>
            {
                richTextBoxLog.Clear();
                richTextBoxLog.SelectionColor = Color.Red;
                richTextBoxLog.AppendText($"Error reading log file:\n{ex.Message}");
                _lastFileLength = 0;
                _partialLine = string.Empty;
            }));
        }
    }

    private async Task<string> ReadNewContentAsync()
    {
        const int maxRetries = 3;
        const int retryDelayMs = 50;

        for (var retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                using var stream =
                    new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                // Seek to the last known position
                if (_lastFileLength > 0 && stream.Length >= _lastFileLength)
                {
                    stream.Seek(_lastFileLength, SeekOrigin.Begin);
                }
                else if (_lastFileLength > stream.Length)
                {
                    // File was truncated, start from beginning
                    _lastFileLength = 0;
                    _partialLine = string.Empty;
                    stream.Seek(0, SeekOrigin.Begin);

                    // If file was truncated, we need to rebuild the entire display
                    using var reader = new StreamReader(stream);
                    var allContent = await reader.ReadToEndAsync();
                    richTextBoxLog.Invoke((Action)(() =>
                    {
                        var lines = allContent.Split(separator, StringSplitOptions.None);
                        DisplayColoredLog(lines);
                    }));
                    return string.Empty; // Already processed
                }

                using var newReader = new StreamReader(stream);
                return await newReader.ReadToEndAsync();
            }
            catch (IOException) when (retry < maxRetries - 1)
            {
                // File might be locked, wait and retry
                await Task.Delay(retryDelayMs);
                continue;
            }
        }

        return string.Empty;
    }

    private void ProcessNewContent(string newContent)
    {
        if (string.IsNullOrEmpty(newContent)) return;

        // Combine any partial line from previous read with new content
        var fullContent = _partialLine + newContent;

        // Split into lines
        var lines = fullContent.Split(separatorArray, StringSplitOptions.None);

        // Check if the last line is complete (ends with newline)
        var lastLineComplete = newContent.EndsWith("\n") || newContent.EndsWith("\r\n");

        richTextBoxLog.SuspendLayout();

        // Process all complete lines
        var linesToProcess = lastLineComplete ? lines.Length : lines.Length - 1;

        for (var i = 0; i < linesToProcess; i++)
        {
            AppendColoredLine(lines[i]);
        }

        // Store the incomplete last line for next iteration
        if (!lastLineComplete && lines.Length > 0)
        {
            _partialLine = lines[lines.Length - 1];
        }
        else
        {
            _partialLine = string.Empty;
        }

        // Keep scrolled to bottom
        richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;
        richTextBoxLog.ScrollToCaret();
        richTextBoxLog.ResumeLayout();
    }

    // New method to append a single colored line
    private void AppendColoredLine(string line)
    {
        // Check if any bracketed content in the line matches a hidden tag
        if (_hiddenTags.Count > 0)
        {
            var matches = BracketRegex.Matches(line);
            var shouldHide = false;

            foreach (Match match in matches)
            {
                // Don't hide based on timestamps, only actual tags
                if (IsTimestamp(match.Value) || !IsTagHidden(match.Value)) continue;
                shouldHide = true;
                break;
            }

            if (shouldHide) return; // skip whole line
        }

        // Determine the color for the line based on tags (including overflow lines)
        var lineColor = GetLineColorFromTags(line);

        var lastIndex = 0;
        foreach (Match match in BracketRegex.Matches(line))
        {
            // Write text before the bracketed content in line color
            if (match.Index > lastIndex)
            {
                richTextBoxLog.SelectionColor = lineColor;
                richTextBoxLog.AppendText(line.Substring(lastIndex, match.Index - lastIndex));
            }

            var bracketContent = match.Value;

            // Check if this is a timestamp
            if (IsTimestamp(bracketContent))
            {
                richTextBoxLog.SelectionColor = Color.Blue; // Timestamp color
                richTextBoxLog.AppendText(bracketContent);
            }
            else
            {
                // For non-timestamp brackets (tags), use the line color
                richTextBoxLog.SelectionColor = lineColor;
                richTextBoxLog.AppendText(bracketContent);
            }

            lastIndex = match.Index + match.Length;
        }

        // Write the rest of the line after the last bracketed content in line color
        if (lastIndex < line.Length)
        {
            richTextBoxLog.SelectionColor = lineColor;
            richTextBoxLog.AppendText(line.Substring(lastIndex));
        }

        richTextBoxLog.AppendText(NewLine);
    }

    // Helper function to check if a tag is hidden, accounting for wildcards
    private bool IsTagHidden(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        var tagWithoutBrackets = tag.Trim('[', ']');

        return _hiddenTags.Any(hiddenTag =>
            MatchesWildcardTag(hiddenTag, tag) || MatchesWildcardTag(hiddenTag, tagWithoutBrackets));
    }

    // Helper function to find a tag's color in the dictionary, accounting for wildcards
    private Color? FindTagColor(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return null;

        foreach (var colorEntry in _tagColors.Where(colorEntry => MatchesWildcardTag(colorEntry.Key, tag)))
        {
            return colorEntry.Value;
        }

        return null;
    }

    // Updated DisplayColoredLog to better handle timestamps in overflow lines (used for initial load and rebuilds)
    private void DisplayColoredLog(string[] lines)
    {
        richTextBoxLog.SuspendLayout();
        richTextBoxLog.Clear();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Only skip the very last line if it's empty (incomplete line)
            if (string.IsNullOrEmpty(line) && i == lines.Length - 1)
                continue;

            AppendColoredLine(line);
        }

        richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;
        richTextBoxLog.ScrollToCaret();
        richTextBoxLog.ResumeLayout();
    }

    // Updated GetLineColorFromTags method to handle overflow lines
    private Color GetLineColorFromTags(string line)
    {
        if (string.IsNullOrEmpty(line)) return _defaultLineColor;

        // Get all bracketed content from the line
        var matches = BracketRegex.Matches(line);

        // Define priority order for tags (higher index = higher priority)
        var tagPriority = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "DEBUG", 1 }, { "[DEBUG]", 1 },
            { "Il2CppInterop", 2 }, { "[Il2CppInterop]", 2 },
            { "Il2CppAssemblyGenerator", 2 }, { "[Il2CppAssemblyGenerator]", 2 },
            { "Il2Cpp*", 2 }, { "[Il2Cpp*]", 2 },
            { "INFO", 3 }, { "[INFO]", 3 },
            { "LastEpoch_Hud", 4 }, { "[LastEpoch_Hud]", 4 },
            { "UnityExplorer", 5 }, { "[UnityExplorer]", 5 },
            { "WARNING", 6 }, { "[WARNING]", 6 },
            { "ERROR", 7 }, { "[ERROR]", 7 }
        };

        var highestPriorityColor = _defaultLineColor;
        var highestPriority = 0;

        // Check all bracketed content in the line, not just those after timestamps
        foreach (Match match in matches)
        {
            var tag = match.Value;

            // Skip only actual timestamps, not all bracketed content
            if (IsTimestamp(tag))
                continue;

            // Find matching color for this tag (including wildcards)
            var matchedColor = FindTagColor(tag);

            if (!matchedColor.HasValue) continue;
            // Find the highest priority among matching patterns
            var tagWithoutBrackets = tag.Trim('[', ']');

            var matchedPriority =
                (from priorityEntry in tagPriority
                    where MatchesWildcardTag(priorityEntry.Key, tag) ||
                          MatchesWildcardTag(priorityEntry.Key, tagWithoutBrackets)
                    select priorityEntry.Value).Prepend(0).Max();

            if (matchedPriority <= highestPriority) continue;
            highestPriority = matchedPriority;
            highestPriorityColor = matchedColor.Value;
        }

        // If no specific tag color was found, also check for keywords in the raw text
        // This handles cases where tags might not be in brackets or are overflow lines
        return highestPriority == 0 ? GetColorForLogLine(line) : highestPriorityColor;
    }

    // Updated GetColorForLogLine to also check for non-bracketed keywords
    private Color GetColorForLogLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return _defaultLineColor;

        // Remove hidden tags before checking for keywords
        var visibleLine = RemoveHiddenTagsFromLine(line);

        // Check for bracketed and non-bracketed keywords
        // Priority: ERROR > WARNING > INFO > DEBUG
        if (visibleLine.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
            visibleLine.Contains("[ERROR]", StringComparison.OrdinalIgnoreCase))
            return Color.Red;

        if (visibleLine.Contains("WARNING", StringComparison.OrdinalIgnoreCase) ||
            visibleLine.Contains("[WARNING]", StringComparison.OrdinalIgnoreCase))
            return Color.Orange;

        if (visibleLine.Contains("INFO", StringComparison.OrdinalIgnoreCase) ||
            visibleLine.Contains("[INFO]", StringComparison.OrdinalIgnoreCase))
            return Color.DarkGreen;

        if (visibleLine.Contains("DEBUG", StringComparison.OrdinalIgnoreCase) ||
            visibleLine.Contains("[DEBUG]", StringComparison.OrdinalIgnoreCase))
            return Color.LightGray;

        // Check for other custom tags that might not be in brackets
        foreach (var colorEntry in from colorEntry in _tagColors
                 let pattern = colorEntry.Key.Trim('[', ']')
                 where MatchesWildcardTag(pattern, visibleLine) ||
                       visibleLine.Contains(pattern, StringComparison.OrdinalIgnoreCase)
                 select colorEntry)
        {
            return colorEntry.Value;
        }

        return _defaultLineColor;
    }

    private string RemoveHiddenTagsFromLine(string line)
    {
        return _hiddenTags.Count == 0
            ? line
            : BracketRegex.Replace(line, m => _hiddenTags.Contains(m.Value) ? string.Empty : m.Value);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        base.OnFormClosing(e);
    }

    private void buttonCopyLog_Click(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(richTextBoxLog.Text))
            {
                Clipboard.SetText(richTextBoxLog.Text);

                MessageBox.Show("Log copied to clipboard", "Copied", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Log is empty.", "Copy Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to copy log: {ex.Message}", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LogViewerForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Don't show lines with the following tags
        RemoveHiddenTag("[DEBUG]");
        RemoveHiddenTag("[MelonAssemblyResolver]");

    }
}