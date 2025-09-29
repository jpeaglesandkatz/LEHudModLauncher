
using SettingsManager;

namespace LEHuDModLauncher.Classlibs;

public partial class CustomMessageBox : Form
{
    public DialogResult CustomResult { get; private set; }

    public CustomMessageBox(string message, string title, string button1Text, string button2Text, string button3Text = null)
    {
        InitializeComponent();

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        // Apply theme
        if (Config.Instance.Settings.DarkMode)
            ThemeUtils.ApplyDarkTheme(this);
        else
            ThemeUtils.ApplyLightTheme(this);

        // Create message label with proper sizing
        var lblMessage = new Label
        {
            Text = message,
            Location = new Point(40, 40), // Increased margins
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false
        };

        // Measure the text properly with larger dimensions
        using (var g = CreateGraphics())
        {
            var maxWidth = 800; // Increased maximum message width
            var messageSize = g.MeasureString(message, lblMessage.Font, maxWidth);
            lblMessage.Size = new Size((int)Math.Ceiling(messageSize.Width), (int)Math.Ceiling(messageSize.Height));
        }

        // Create buttons with proper sizing
        var btn1 = new Button
        {
            Text = button1Text,
            UseVisualStyleBackColor = true,
            AutoSize = false
        };

        var btn2 = new Button
        {
            Text = button2Text,
            UseVisualStyleBackColor = true,
            AutoSize = false
        };

        Button btn3 = null;
        if (!string.IsNullOrEmpty(button3Text))
        {
            btn3 = new Button
            {
                Text = button3Text,
                UseVisualStyleBackColor = true,
                AutoSize = false
            };
        }

        // Measure button text and set appropriate sizes - making buttons larger
        using (var g = CreateGraphics())
        {
            var btn1TextSize = g.MeasureString(button1Text, btn1.Font);
            var btn2TextSize = g.MeasureString(button2Text, btn2.Font);

            // Increased padding and minimum sizes for buttons
            const int padding = 40; // Increased padding
            const int minWidth = 150; // Increased minimum width
            const int minHeight = 45; // Increased minimum height

            btn1.Size = new Size(
                Math.Max((int)Math.Ceiling(btn1TextSize.Width) + padding, minWidth),
                Math.Max((int)Math.Ceiling(btn1TextSize.Height) + 20, minHeight)
            );

            btn2.Size = new Size(
                Math.Max((int)Math.Ceiling(btn2TextSize.Width) + padding, minWidth),
                Math.Max((int)Math.Ceiling(btn2TextSize.Height) + 20, minHeight)
            );

            if (btn3 != null)
            {
                var btn3TextSize = g.MeasureString(button3Text, btn3.Font);
                btn3.Size = new Size(
                    Math.Max((int)Math.Ceiling(btn3TextSize.Width) + padding, minWidth),
                    Math.Max((int)Math.Ceiling(btn3TextSize.Height) + 20, minHeight)
                );
            }
        }

        // Calculate form dimensions with increased margins and spacing
        const int margin = 40; // Increased margin
        const int buttonSpacing = 25; // Increased button spacing
        var buttonAreaHeight = Math.Max(btn1.Height, btn2.Height);
        if (btn3 != null)
            buttonAreaHeight = Math.Max(buttonAreaHeight, btn3.Height);
        buttonAreaHeight += margin * 2;

        var totalButtonWidth = btn1.Width + btn2.Width + buttonSpacing;
        if (btn3 != null)
            totalButtonWidth += btn3.Width + buttonSpacing;

        // Form width should accommodate either the message or the buttons (whichever is wider)
        var formWidth = Math.Max(lblMessage.Width + margin * 2, totalButtonWidth + margin * 2);
        formWidth = Math.Max(formWidth, 600); // Increased minimum form width

        var formHeight = lblMessage.Top + lblMessage.Height + buttonAreaHeight + margin;
        formHeight = Math.Max(formHeight, 250); // Increased minimum form height

        ClientSize = new Size(formWidth, formHeight);

        // Center the message label horizontally if form is wider than the message
        if (formWidth > lblMessage.Width + margin * 2)
        {
            lblMessage.Location = new Point((formWidth - lblMessage.Width) / 2, lblMessage.Top);
        }

        // Position buttons with increased spacing
        var buttonY = lblMessage.Bottom + margin;
        var buttonsStartX = (formWidth - totalButtonWidth) / 2;

        btn1.Location = new Point(buttonsStartX, buttonY);
        btn2.Location = new Point(buttonsStartX + btn1.Width + buttonSpacing, buttonY);

        if (btn3 != null)
        {
            btn3.Location = new Point(buttonsStartX + btn1.Width + btn2.Width + buttonSpacing * 2, buttonY);
        }

        // Set up button events and dialog results
        btn1.Click += (s, e) =>
        {
            CustomResult = DialogResult.Yes;
            DialogResult = DialogResult.Yes;
            Close();
        };

        btn2.Click += (s, e) =>
        {
            CustomResult = DialogResult.No;
            DialogResult = DialogResult.No;
            Close();
        };

        if (btn3 != null)
        {
            btn3.Click += (s, e) =>
            {
                CustomResult = DialogResult.Retry; // Using Retry for the third button
                DialogResult = DialogResult.Retry;
                Close();
            };
        }

        // Add controls to form
        Controls.Add(lblMessage);
        Controls.Add(btn1);
        Controls.Add(btn2);
        if (btn3 != null)
            Controls.Add(btn3);

        // Set button properties
        AcceptButton = btn1;
        CancelButton = btn2;
        btn1.TabIndex = 0;
        btn2.TabIndex = 1;
        if (btn3 != null)
            btn3.TabIndex = 2;

        // Apply theme after adding controls
        if (Config.Instance.Settings.DarkMode)
            ThemeUtils.ApplyDarkTheme(this);
        else
            ThemeUtils.ApplyLightTheme(this);
    }

    // Two button version
    public static DialogResult Show(string message, string title, string button1Text, string button2Text)
    {
        using (var dialog = new CustomMessageBox(message, title, button1Text, button2Text))
        {
            var result = dialog.ShowDialog();
            return dialog.CustomResult != DialogResult.None ? dialog.CustomResult : result;
        }
    }

    // Three button version
    public static DialogResult Show(string message, string title, string button1Text, string button2Text, string button3Text)
    {
        using (var dialog = new CustomMessageBox(message, title, button1Text, button2Text, button3Text))
        {
            var result = dialog.ShowDialog();
            return dialog.CustomResult != DialogResult.None ? dialog.CustomResult : result;
        }
    }

    // Two button version with owner
    public static DialogResult Show(IWin32Window owner, string message, string title, string button1Text, string button2Text)
    {
        using (var dialog = new CustomMessageBox(message, title, button1Text, button2Text))
        {
            var result = dialog.ShowDialog(owner);
            return dialog.CustomResult != DialogResult.None ? dialog.CustomResult : result;
        }
    }

    // Three button version with owner
    public static DialogResult Show(IWin32Window owner, string message, string title, string button1Text, string button2Text, string button3Text)
    {
        using (var dialog = new CustomMessageBox(message, title, button1Text, button2Text, button3Text))
        {
            var result = dialog.ShowDialog(owner);
            return dialog.CustomResult != DialogResult.None ? dialog.CustomResult : result;
        }
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        ResumeLayout(false);
    }
}