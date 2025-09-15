using LEHuDModLauncher.Classlibs;
using System.Text.RegularExpressions;
using static LauncherUtils.Utils;
using System.ComponentModel;

namespace LEHuDModLauncher
{
    public partial class LogViewerForm : Form
    {
        private readonly string _logFilePath;
        private readonly System.Windows.Forms.Timer _timer;
        private long _lastFileLength = 0;
        private int _lastLineCount = 0; // Track number of lines processed
        private string _partialLine = string.Empty; // Store incomplete lines between reads
        public AppSettings settings = SettingsManager.Instance.Settings;

        // Add this field near the top with other private fields
        private Color _defaultLineColor = Color.DarkGray;

        // Regex to match [anything] including timestamps and tags
        private static readonly Regex BracketRegex = new Regex(@"\[[^\]]+\]", RegexOptions.Compiled);
        // Regex to match [hh:mm:ss] or [h:mm:ss] timestamps (whole string)
        private static readonly Regex TimeRegex = new Regex(@"^\[\d{1,2}:\d{2}:\d{2}\]$", RegexOptions.Compiled);

        // Update the timestamp detection regex to be more flexible
        private static readonly Regex TimestampRegex = new Regex(@"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]|\[\d{2}:\d{2}:\d{2}\.\d{3}\]|\[\d{1,2}:\d{2}:\d{2}\]$", RegexOptions.Compiled);

        // Update the tag color dictionary to support wildcards and non-bracketed tags
        private readonly Dictionary<string, Color> _tagColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            // Bracketed versions
            { "[ERROR]", Color.Red },
            { "[WARNING]", Color.Orange },
            { "[DEBUG]", Color.LightGray },
            { "[INFO]", Color.DarkGreen },
            { "[LastEpoch_Hud]", Color.Lime },
            { "[Il2CppInterop]", Color.DarkGray},
            { "[Il2CppAssemblyGenerator]", Color.DarkGray },
            { "[UnityExplorer]", Color.DarkGoldenrod },
            { "[Il2CppICallInjector]", Color.DarkGray },
            
            // Non-bracketed versions
            { "ERROR", Color.Red },
            { "WARNING", Color.Orange },
            { "DEBUG", Color.LightGray },
            { "INFO", Color.DarkGreen },
            { "LastEpoch_Hud", Color.Lime },
            { "Il2CppInterop", Color.DarkGray},
            { "Il2CppAssemblyGenerator", Color.DarkGray },
            { "UnityExplorer", Color.DarkGoldenrod },
            { "Il2CppICallInjector", Color.DarkGray },
            { "--- BEGIN IL2CPP STACK TRACE ---", Color.Red },
            
            // Wildcard examples
            { "Il2Cpp*", Color.DarkGray },
            { "[Il2Cpp*]", Color.DarkGray },
            { "*BEGIN IL2CPP STACK TRACE*", Color.Red},
            { "*LastEpoch_Hud*", Color.Lime}
        };

        // Tags that should be removed (not shown) from the displayed log.
        // Use AddHiddenTag/RemoveHiddenTag/SetHiddenTags to manage this set.
        private readonly HashSet<string> _hiddenTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add this property
        /// <summary>
        /// Gets or sets the default color for log lines that don't match any specific tags.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        // Add this property
        /// <summary>
        /// Gets or sets the default color for log lines that don't match any specific tags.
        /// </summary>
        public Color DefaultLineColor
        {
            get => _defaultLineColor;
            set => _defaultLineColor = value;
        }

        public LogViewerForm(string logFilePath)
        {
            InitializeComponent();

            if ((SettingsManager.Instance.Settings.LogWindowHeight< 0) || (SettingsManager.Instance.Settings.LogWindowWidth < 0))
            { 
                SettingsManager.Instance.UpdateLogWindowHeight(500);
                SettingsManager.Instance.UpdateLogWindowWidth(900);
               
            }
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(
                SettingsManager.Instance.Settings.LogWindowWidth,
                SettingsManager.Instance.Settings.LogWindowHeight);

            // Don't show lines with the following tags
            AddHiddenTag("[DEBUG]");
            AddHiddenTag("[BS DEBUG]");
            AddHiddenTag("[MelonAssemblyResolver]");


            ThemeUtils.ApplyDarkTheme(this);
            _logFilePath = logFilePath;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1500; // Check more frequently for better responsiveness
            _timer.Tick += async (s, e) => await UpdateLogAsync();
            _timer.Start();
        }

        // Helper method to check if a bracketed content is a timestamp
        private bool IsTimestamp(string bracketContent)
        {
            return TimestampRegex.IsMatch(bracketContent);
        }

        // Add this method to support wildcard matching
        private bool MatchesWildcardTag(string tag, string lineTag)
        {
            // Simple wildcard matching with * and ?
            // * matches any sequence of characters
            // ? matches any single character
            
            if (string.Equals(tag, lineTag, StringComparison.OrdinalIgnoreCase))
                return true;
                
            if (!tag.Contains('*') && !tag.Contains('?'))
                return false;
                
            // Convert wildcard pattern to regex
            string pattern = "^" + Regex.Escape(tag)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$";
                
            return Regex.IsMatch(lineTag, pattern, RegexOptions.IgnoreCase);
        }

        // Modify the existing methods to handle tags without brackets and wildcards

        /// <summary>
        /// Add a hidden tag. Supports wildcards (* and ?) and accepts tags with or without brackets.
        /// Examples: "DEBUG", "[DEBUG]", "Il2Cpp*", "[Il2Cpp*]"
        /// </summary>
        public void AddHiddenTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            tag = tag.Trim();
            
            // Store tags as-is (with or without brackets) to support both formats
            _hiddenTags.Add(tag);
            
            // Also add the bracketed version if not already bracketed
            if (!tag.StartsWith("[") || !tag.EndsWith("]"))
            {
                string bracketedTag = "[" + tag.Trim('[', ']') + "]";
                _hiddenTags.Add(bracketedTag);
            }
        }

        /// <summary>
        /// Remove a previously hidden tag. Accepts either "TAG" or "[TAG]".
        /// </summary>
        public void RemoveHiddenTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            tag = tag.Trim();
            if (!tag.StartsWith("[") || !tag.EndsWith("]"))
                tag = "[" + tag.Trim('[', ']') + "]";
            _hiddenTags.Remove(tag);
        }

        /// <summary>
        /// Replace hidden tags with the provided collection. Accepts tags with or without brackets.
        /// </summary>
        public void SetHiddenTags(IEnumerable<string> tags)
        {
            _hiddenTags.Clear();
            if (tags == null) return;
            foreach (var t in tags) AddHiddenTag(t);
        }

        /// <summary>
        /// Returns a snapshot of hidden tags.
        /// </summary>
        public IReadOnlyCollection<string> GetHiddenTags() => new List<string>(_hiddenTags).AsReadOnly();

        // Add method to add custom tag colors with wildcard support
        /// <summary>
        /// Add or update a tag color. Supports wildcards and tags with or without brackets.
        /// </summary>
        public void SetTagColor(string tag, Color color)
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
                        string newContent = await ReadNewContentAsync();
                        
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

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    using var stream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    
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
                            var lines = allContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
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
            string fullContent = _partialLine + newContent;
            
            // Split into lines
            string[] lines = fullContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            // Check if the last line is complete (ends with newline)
            bool lastLineComplete = newContent.EndsWith("\n") || newContent.EndsWith("\r\n");
            
            richTextBoxLog.SuspendLayout();
            
            // Process all complete lines
            int linesToProcess = lastLineComplete ? lines.Length : lines.Length - 1;
            
            for (int i = 0; i < linesToProcess; i++)
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
                bool shouldHide = false;
                
                foreach (Match match in matches)
                {
                    // Don't hide based on timestamps, only actual tags
                    if (!IsTimestamp(match.Value) && IsTagHidden(match.Value))
                    {
                        shouldHide = true;
                        break;
                    }
                }
                
                if (shouldHide) return; // skip whole line
            }

            // Determine the color for the line based on tags (including overflow lines)
            Color lineColor = GetLineColorFromTags(line);

            int lastIndex = 0;
            foreach (Match match in BracketRegex.Matches(line))
            {
                // Write text before the bracketed content in line color
                if (match.Index > lastIndex)
                {
                    richTextBoxLog.SelectionColor = lineColor;
                    richTextBoxLog.AppendText(line.Substring(lastIndex, match.Index - lastIndex));
                }

                string bracketContent = match.Value;
                
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
            
            richTextBoxLog.AppendText(Environment.NewLine);
        }

        // Helper function to check if a tag is hidden, accounting for wildcards
        private bool IsTagHidden(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            string tagWithoutBrackets = tag.Trim('[', ']');
            
            foreach (string hiddenTag in _hiddenTags)
            {
                if (MatchesWildcardTag(hiddenTag, tag) || MatchesWildcardTag(hiddenTag, tagWithoutBrackets))
                    return true;
            }
            
            return false;
        }

        // Helper function to find a tag's color in the dictionary, accounting for wildcards
        private Color? FindTagColor(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return null;
            
            foreach (var colorEntry in _tagColors)
            {
                if (MatchesWildcardTag(colorEntry.Key, tag))
                    return colorEntry.Value;
            }
            
            return null;
        }

        // Updated DisplayColoredLog to better handle timestamps in overflow lines (used for initial load and rebuilds)
        private void DisplayColoredLog(string[] lines)
        {
            richTextBoxLog.SuspendLayout();
            richTextBoxLog.Clear();
            
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                
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

            Color highestPriorityColor = _defaultLineColor;
            int highestPriority = 0;

            // Check all bracketed content in the line, not just those after timestamps
            foreach (Match match in matches)
            {
                string tag = match.Value;
                
                // Skip only actual timestamps, not all bracketed content
                if (IsTimestamp(tag))
                    continue;

                // Find matching color for this tag (including wildcards)
                Color? matchedColor = FindTagColor(tag);
                
                if (matchedColor.HasValue)
                {
                    // Find the highest priority among matching patterns
                    int matchedPriority = 0;
                    string tagWithoutBrackets = tag.Trim('[', ']');
                    
                    foreach (var priorityEntry in tagPriority)
                    {
                        if (MatchesWildcardTag(priorityEntry.Key, tag) || 
                            MatchesWildcardTag(priorityEntry.Key, tagWithoutBrackets))
                        {
                            if (priorityEntry.Value > matchedPriority)
                                matchedPriority = priorityEntry.Value;
                        }
                    }
                    
                    if (matchedPriority > highestPriority)
                    {
                        highestPriority = matchedPriority;
                        highestPriorityColor = matchedColor.Value;
                    }
                }
            }

            // If no specific tag color was found, also check for keywords in the raw text
            // This handles cases where tags might not be in brackets or are overflow lines
            if (highestPriority == 0)
            {
                return GetColorForLogLine(line);
            }

            return highestPriorityColor;
        }

        // Updated GetColorForLogLine to also check for non-bracketed keywords
        private Color GetColorForLogLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return _defaultLineColor;

            // Remove hidden tags before checking for keywords
            string visibleLine = RemoveHiddenTagsFromLine(line);

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
            foreach (var colorEntry in _tagColors)
            {
                string pattern = colorEntry.Key.Trim('[', ']'); // Remove brackets for comparison
                if (MatchesWildcardTag(pattern, visibleLine) || 
                    visibleLine.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return colorEntry.Value;
                }
            }
    
            return _defaultLineColor;
        }

        /// <summary>
        /// Utility that returns the given line with any configured hidden bracket-tags removed.
        /// </summary>
        private string RemoveHiddenTagsFromLine(string line)
        {
            if (_hiddenTags.Count == 0) return line;
            return BracketRegex.Replace(line, m => _hiddenTags.Contains(m.Value) ? string.Empty : m.Value);
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
                    
                    MessageBox.Show("Log copied to clipboard", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Log is empty.", "Copy Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy log: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LogViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Don't show lines with the following tags
            RemoveHiddenTag("[DEBUG]");
            RemoveHiddenTag("[MelonAssemblyResolver]");
            
        }
    }
}