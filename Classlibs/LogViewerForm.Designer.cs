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
            richTextBoxLog.Font = new Font("Consolas", 10F);
            richTextBoxLog.ForeColor = Color.Silver;
            richTextBoxLog.Location = new Point(0, 50);
            richTextBoxLog.Margin = new Padding(4, 3, 4, 3);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.Size = new Size(1000, 513);
            richTextBoxLog.TabIndex = 0;
            richTextBoxLog.Text = "";
            // 
            // buttonCopyLog
            // 
            buttonCopyLog.Dock = DockStyle.Top;
            buttonCopyLog.Location = new Point(0, 0);
            buttonCopyLog.Margin = new Padding(4, 3, 4, 3);
            buttonCopyLog.Name = "buttonCopyLog";
            buttonCopyLog.Size = new Size(1000, 50);
            buttonCopyLog.TabIndex = 1;
            buttonCopyLog.Text = "Copy Log to Clipboard";
            buttonCopyLog.UseVisualStyleBackColor = true;
            buttonCopyLog.Click += buttonCopyLog_Click;
            // 
            // LogViewerForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 563);
            Controls.Add(richTextBoxLog);
            Controls.Add(buttonCopyLog);
            Margin = new Padding(4, 3, 4, 3);
            Name = "LogViewerForm";
            Text = "Log Viewer";
            FormClosing += LogViewerForm_FormClosing;
            ResumeLayout(false);
        }
    }
}