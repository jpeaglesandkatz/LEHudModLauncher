using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LauncherUtils.Utils;

namespace LEHuDModLauncher.Classlibs
{
	class StartupDialog : Form
	{
		public AppSettings settings = SettingsManager.Instance.Settings;
		public bool NewShowSetting { get; private set; } // true = show next time, false = don't show

		readonly RichTextBox rtb;
		readonly CheckBox chkDontShow;
		readonly Button btnClose;
		readonly string filePath;

		public StartupDialog(string messageFilePath, bool currentShowSetting)
		{
			this.filePath = messageFilePath;
			this.NewShowSetting = currentShowSetting;
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

			rtb = new RichTextBox() { Dock = DockStyle.Top, ReadOnly = true, Height = ClientSize.Height - 80, BorderStyle = BorderStyle.FixedSingle };
			chkDontShow = new CheckBox() { Text = "Show this message on startup", Dock = DockStyle.Bottom, Height = 24 };
			btnClose = new Button() { Text = "Close", Width = 100, Height = 30, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
			chkDontShow.Checked = SettingsManager.Instance.Settings.ShowStartupMessage;
			if (SettingsManager.Instance.Settings.DarkMode) ThemeUtils.ApplyDarkTheme(rtb);
			else ThemeUtils.ApplyLightTheme(rtb);


			btnClose.Left = ClientSize.Width - btnClose.Width - 12;
			btnClose.Top = ClientSize.Height - btnClose.Height - 12;
			btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			Controls.Add(btnClose);
			Controls.Add(chkDontShow);
			Controls.Add(rtb);

			this.Resize += (s, e) =>
			{
				rtb.Size = new Size(ClientSize.Width - 5, ClientSize.Height - 80);
				SettingsManager.Instance.UpdateModInfoWindowPosition(ClientSize.Width - 5, ClientSize.Height - 80);
				rtb.Update();
			};

			LoadMessageFile(messageFilePath);

			chkDontShow.Checked = SettingsManager.Instance.Settings.ShowStartupMessage;

			btnClose.Click += (s, e) => Close();
			chkDontShow.CheckedChanged += (s, e) =>
			{
				NewShowSetting = chkDontShow.Checked;
				SettingsManager.Instance.UpdateShowStartupMessage(NewShowSetting);
				chkDontShow.Checked = SettingsManager.Instance.Settings.ShowStartupMessage;
			};

			Shown += (s, e) =>
			{
				// ensure button is above other controls
				btnClose.BringToFront();
			};
			if (SettingsManager.Instance.Settings.DarkMode) ThemeUtils.ApplyDarkTheme(this);
			else ThemeUtils.ApplyLightTheme(this);
		}

		void LoadMessageFile(string path)
		{
			try
			{
				string ext = Path.GetExtension(path).ToLowerInvariant();
				if (ext == ".rtf")
				{
					rtb.Clear();
					rtb.LoadFile(path, RichTextBoxStreamType.RichText);
				}
				else // treat as plain text
				{
					rtb.Clear();
					rtb.LoadFile(path, RichTextBoxStreamType.PlainText);
				}
			}
			catch (Exception ex)
			{
				rtb.Text = $"Failed to load startup message:\n{ex.Message}";
			}
		}

	}
}