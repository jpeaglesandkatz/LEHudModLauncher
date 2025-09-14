using System;
using System.Drawing;
using System.Windows.Forms;

namespace LEHuDModLauncher
{
    public partial class CustomMessageBox : Form
    {
        public DialogResult CustomResult { get; private set; }
        
        public CustomMessageBox(string message, string title, string button1Text, string button2Text)
        {
            InitializeComponent();
            
            this.Text = title;
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Message label
            var lblMessage = new Label
            {
                Text = message,
                Location = new Point(20, 20),
                Size = new Size(340, 80),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };
            
            // Button 1 (Melonloader and mod)
            var btn1 = new Button
            {
                Text = button1Text,
                Location = new Point(80, 120),
                Size = new Size(120, 30),
                DialogResult = DialogResult.Yes
            };
            btn1.Click += (s, e) => 
            {
                CustomResult = DialogResult.Yes;
                this.Close();
            };
            
            // Button 2 (Mod only)
            var btn2 = new Button
            {
                Text = button2Text,
                Location = new Point(220, 120),
                Size = new Size(120, 30),
                DialogResult = DialogResult.No
            };
            btn2.Click += (s, e) => 
            {
                CustomResult = DialogResult.No;
                this.Close();
            };
            
            // Add controls to form
            this.Controls.Add(lblMessage);
            this.Controls.Add(btn1);
            this.Controls.Add(btn2);
            
            // Set default button
            this.AcceptButton = btn1;
        }
        
        public static DialogResult Show(string message, string title, string button1Text, string button2Text)
        {
            using (var dialog = new CustomMessageBox(message, title, button1Text, button2Text))
            {
                dialog.ShowDialog();
                return dialog.CustomResult;
            }
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}
