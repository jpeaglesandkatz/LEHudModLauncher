namespace LEHuDModLauncher.Classlibs
{
    partial class ThemePickerForm
    {
        private System.ComponentModel.IContainer components = null;
        private MaterialSkin.Controls.MaterialComboBox cmbPrimary;
        private MaterialSkin.Controls.MaterialComboBox cmbAccent;
        private MaterialSkin.Controls.MaterialComboBox cmbTheme;
        private MaterialSkin.Controls.MaterialButton btnApply;
        private MaterialSkin.Controls.MaterialLabel lblPrimary;
        private MaterialSkin.Controls.MaterialLabel lblAccent;
        private MaterialSkin.Controls.MaterialLabel lblTheme;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.cmbPrimary = new MaterialSkin.Controls.MaterialComboBox();
            this.cmbAccent = new MaterialSkin.Controls.MaterialComboBox();
            this.cmbTheme = new MaterialSkin.Controls.MaterialComboBox();
            this.btnApply = new MaterialSkin.Controls.MaterialButton();
            this.lblPrimary = new MaterialSkin.Controls.MaterialLabel();
            this.lblAccent = new MaterialSkin.Controls.MaterialLabel();
            this.lblTheme = new MaterialSkin.Controls.MaterialLabel();
            this.SuspendLayout();

            // 
            // lblPrimary
            // 
            this.lblPrimary.Text = "Primary Color:";
            this.lblPrimary.Location = new System.Drawing.Point(30, 80);
            this.lblPrimary.AutoSize = true;

            // 
            // cmbPrimary
            // 
            this.cmbPrimary.Location = new System.Drawing.Point(150, 75);
            this.cmbPrimary.Size = new System.Drawing.Size(200, 30);

            // 
            // lblAccent
            // 
            this.lblAccent.Text = "Accent Color:";
            this.lblAccent.Location = new System.Drawing.Point(30, 130);
            this.lblAccent.AutoSize = true;

            // 
            // cmbAccent
            // 
            this.cmbAccent.Location = new System.Drawing.Point(150, 125);
            this.cmbAccent.Size = new System.Drawing.Size(200, 30);

            // 
            // lblTheme
            // 
            this.lblTheme.Text = "Theme:";
            this.lblTheme.Location = new System.Drawing.Point(30, 180);
            this.lblTheme.AutoSize = true;

            // 
            // cmbTheme
            // 
            this.cmbTheme.Location = new System.Drawing.Point(150, 175);
            this.cmbTheme.Size = new System.Drawing.Size(200, 30);

            // 
            // btnApply
            // 
            this.btnApply.Text = "Apply Theme";
            this.btnApply.Location = new System.Drawing.Point(150, 230);
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);

            // 
            // ThemePickerForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.lblPrimary);
            this.Controls.Add(this.cmbPrimary);
            this.Controls.Add(this.lblAccent);
            this.Controls.Add(this.cmbAccent);
            this.Controls.Add(this.lblTheme);
            this.Controls.Add(this.cmbTheme);
            this.Controls.Add(this.btnApply);
            this.Text = "Theme Picker";
            this.ResumeLayout(false);
        }
    }
}
