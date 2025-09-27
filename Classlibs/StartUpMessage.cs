using static LEHuDModLauncher.Settings.Config;

namespace LEHuDModLauncher.Classlibs;

internal sealed class StartupDialog : Form
{
    public bool NewShowSetting { get; private set; } // true = show next time, false = don't show

    private readonly RichTextBox _rtb;
    private readonly CheckBox _chkDontShow;
    private readonly Button _btnClose;

    public StartupDialog(string messageFilePath, bool currentShowSetting)
    {
        NewShowSetting = currentShowSetting;
        if (SettingsManager.Instance.Settings.DarkMode) ThemeUtils.ApplyDarkTheme(this);
        else ThemeUtils.ApplyLightTheme(this);

        Text = "Welcome";
        Width = SettingsManager.Instance.Settings.ModInfoWindowX;
        Height = SettingsManager.Instance.Settings.ModInfoWindowY;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(1000, 700);
        MaximizeBox = true;
        MinimizeBox = true;

        _rtb = new RichTextBox
        {
            Dock = DockStyle.Top, ReadOnly = true, Height = ClientSize.Height - 80,
            BorderStyle = BorderStyle.FixedSingle
        };
        _chkDontShow = new CheckBox { Text = "Show this message on startup", Dock = DockStyle.Bottom, Height = 24 };
        _btnClose = new Button
            { Text = "Close", Width = 100, Height = 30, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
        _chkDontShow.Checked = SettingsManager.Instance.Settings.ShowStartupMessage;
        if (SettingsManager.Instance.Settings.DarkMode) ThemeUtils.ApplyDarkTheme(_rtb);
        else ThemeUtils.ApplyLightTheme(_rtb);


        _btnClose.Left = ClientSize.Width - _btnClose.Width - 12;
        _btnClose.Top = ClientSize.Height - _btnClose.Height - 12;
        _btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        Controls.Add(_btnClose);
        Controls.Add(_chkDontShow);
        Controls.Add(_rtb);

        Resize += (s, e) =>
        {
            _rtb.Size = new Size(ClientSize.Width - 5, ClientSize.Height - 80);
            SettingsManager.Instance.UpdateModInfoWindowPosition(ClientSize.Width - 5, ClientSize.Height - 80);
            _rtb.Update();
        };

        LoadMessageFile(messageFilePath);

        _chkDontShow.Checked = SettingsManager.Instance.Settings.ShowStartupMessage;

        _btnClose.Click += (s, e) => Close();
        _chkDontShow.CheckedChanged += (s, e) =>
        {
            NewShowSetting = _chkDontShow.Checked;
            SettingsManager.Instance.UpdateShowStartupMessage(NewShowSetting);
            _chkDontShow.Checked = SettingsManager.Instance.Settings.ShowStartupMessage;
        };

        Shown += (s, e) =>
        {
            // ensure button is above other controls
            _btnClose.BringToFront();
        };
        if (SettingsManager.Instance.Settings.DarkMode) ThemeUtils.ApplyDarkTheme(this);
        else ThemeUtils.ApplyLightTheme(this);
    }

    private void LoadMessageFile(string path)
    {
        try
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            _rtb.Clear();
            // treat as plain text
            _rtb.LoadFile(path, ext == ".rtf" ? RichTextBoxStreamType.RichText : RichTextBoxStreamType.PlainText);
        }
        catch (Exception ex)
        {
            _rtb.SafeSetText($"Failed to load startup message:\n{ex.Message}");
        }
    }

}