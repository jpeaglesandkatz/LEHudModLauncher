namespace LEHuDModLauncher
{
    partial class LogViewerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.Button buttonCopyLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            richTextBoxLog = new RichTextBox();
            buttonCopyLog = new Button();
            SuspendLayout();
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
            richTextBoxLog.ForeColor = Color.Silver;
            richTextBoxLog.Location = new Point(0, 30);
            richTextBoxLog.Margin = new Padding(3, 2, 3, 2);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.Size = new Size(700, 308);
            richTextBoxLog.TabIndex = 0;
            richTextBoxLog.Text = "";
            // 
            // buttonCopyLog
            // 
            buttonCopyLog.Dock = DockStyle.Top;
            buttonCopyLog.Location = new Point(0, 0);
            buttonCopyLog.Margin = new Padding(3, 2, 3, 2);
            buttonCopyLog.Name = "buttonCopyLog";
            buttonCopyLog.Size = new Size(700, 30);
            buttonCopyLog.TabIndex = 1;
            buttonCopyLog.Text = "Copy Log to Clipboard";
            buttonCopyLog.UseVisualStyleBackColor = true;
            buttonCopyLog.Click += buttonCopyLog_Click;
            // 
            // LogViewerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 338);
            Controls.Add(richTextBoxLog);
            Controls.Add(buttonCopyLog);
            Margin = new Padding(3, 2, 3, 2);
            Name = "LogViewerForm";
            Text = "Log Viewer";
            FormClosing += LogViewerForm_FormClosing;
            ResumeLayout(false);
        }
    }
}