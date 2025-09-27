namespace LEHuDModLauncher.Classlibs
{
    public static class ThemeUtils
    {
        public static void ApplyDarkTheme(Control control)
        {
            var backColor = Color.Black;
            var foreColor = Color.FloralWhite;
            var buttonBack = Color.FromArgb(50, 50, 50);

            SetTheme(control);
            return;

            void SetTheme(Control control)
            {
                control.BackColor = backColor;
                control.ForeColor = foreColor;

                switch (control)
                {
                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = buttonBack;
                        btn.ForeColor = foreColor;
                        btn.FlatAppearance.BorderColor = Color.DimGray;
                        break;
                    case TextBox:
                    case ComboBox:
                    case ProgressBar:
                    case MenuStrip:
                        control.BackColor = Color.FromArgb(40, 40, 40);
                        control.ForeColor = foreColor;
                        break;
                    default:
                    {
                        if (control is ProgressBar pb)
                        {
                            pb.BackColor = backColor;
                            pb.ForeColor = foreColor;
                        }
                        else if (control is RichTextBox richTextbox)
                        {
                            richTextbox.ForeColor = Color.LightGray;
                            richTextbox.BackColor = Color.Black;
                        }

                        break;
                    }
                }

                foreach (Control child in control.Controls)
                    SetTheme(child);
            }
        }

        public static void ApplyLightTheme(Control control)
        {
            var backColor = Color.Black;
            var foreColor = Color.LightGray;
            var buttonBack = Color.LightGray;

            void SetTheme(Control control)
            {
                control.BackColor = foreColor;
                control.ForeColor = backColor;

                switch (control)
                {
                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = Color.DarkGray;
                        btn.ForeColor = backColor;
                        btn.FlatAppearance.BorderColor = Color.DimGray;
                        break;
                    case Panel panel:
                        panel.BackColor = foreColor;
                        panel.ForeColor = backColor;
                        break;
                    case MenuStrip menuStrip:
                        menuStrip.BackColor = foreColor;
                        menuStrip.ForeColor = backColor;
                        break;
                    case TextBox or ComboBox or ProgressBar or MenuStrip:
                        control.BackColor = Color.FromArgb(40, 40, 40);
                        control.ForeColor = foreColor;
                        break;
                    default:
                        switch (control)
                        {
                            case ProgressBar pb:
                                pb.BackColor = backColor;
                                pb.ForeColor = foreColor;
                                break;
                            case RichTextBox richTextbox:
                                richTextbox.ForeColor = Color.Black;
                                richTextbox.BackColor = Color.LightGray;
                                break;
                        }

                        break;
                }

                foreach (Control child in control.Controls)
                    SetTheme(child);
            }
            SetTheme(control);
        }


    }
}
