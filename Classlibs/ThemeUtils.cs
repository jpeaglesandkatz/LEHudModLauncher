using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace LEHuDModLauncher.Classlibs
{
	public static class ThemeUtils
	{
		public static void ApplyDarkTheme(Control control)
		{
			Color backColor = Color.Black;
			Color foreColor = Color.FloralWhite;
			Color buttonBack = Color.FromArgb(50, 50, 50);

			void SetTheme(Control control)
			{
				control.BackColor = backColor;
				control.ForeColor = foreColor;

				if (control is Button btn)
				{
					btn.FlatStyle = FlatStyle.Flat;
					btn.BackColor = buttonBack;
					btn.ForeColor = foreColor;
					btn.FlatAppearance.BorderColor = Color.DimGray;
				}
				else if (control is TextBox || control is ComboBox || control is ProgressBar || control is MenuStrip)
				{
					control.BackColor = Color.FromArgb(40, 40, 40);
					control.ForeColor = foreColor;
				}
				else if (control is ProgressBar pb)
				{
					pb.BackColor = backColor;
					pb.ForeColor = foreColor;
				}
				else if (control is RichTextBox richTextbox)
				{
					richTextbox.ForeColor = Color.LightGray;
					richTextbox.BackColor = Color.Black;
				}

				foreach (Control child in control.Controls)
					SetTheme(child);
			}
			SetTheme(control);
		}

		public static void ApplyLightTheme(Control control)
		{
			Color backColor = Color.Black;
			Color foreColor = Color.LightGray;
			Color buttonBack = Color.LightGray;

			void SetTheme(Control control)
			{
				control.BackColor = foreColor;
				control.ForeColor = backColor;

				if (control is Button btn)
				{
					btn.FlatStyle = FlatStyle.Flat;
					btn.BackColor = Color.DarkGray;
					btn.ForeColor = backColor;
					btn.FlatAppearance.BorderColor = Color.DimGray;
				}
				else if (control is Panel panel)
				{
					panel.BackColor = foreColor;
					panel.ForeColor = backColor;

				}
				else if (control is MenuStrip menuStrip)
				{
					menuStrip.BackColor = foreColor;
					menuStrip.ForeColor = backColor;
				}

				else if (control is TextBox || control is ComboBox || control is ProgressBar || control is MenuStrip)
				{
					control.BackColor = Color.FromArgb(40, 40, 40);
					control.ForeColor = foreColor;
				}
				else if (control is ProgressBar pb)
				{
					pb.BackColor = backColor;
					pb.ForeColor = foreColor;
				}
				else if (control is RichTextBox richTextbox)
				{
					richTextbox.ForeColor = Color.Black;
					richTextbox.BackColor = Color.LightGray;
				}

				foreach (Control child in control.Controls)
					SetTheme(child);
			}
			SetTheme(control);
		}


	}
}
