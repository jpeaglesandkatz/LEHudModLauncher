using System.Linq.Expressions;
using System.Reflection;

namespace LEHuDModLauncher.Classlibs

{
    public static class UIThreadHelper
    {
        // ---------- GENERIC INVOKE ----------
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;

            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

        public static void SafeInvoke(this ToolStripItem item, Action action)
        {
            if (item == null) return;

            var owner = item.GetCurrentParent();
            if (owner == null || owner.IsDisposed) return;

            if (owner.InvokeRequired)
                owner.Invoke(action);
            else
                action();
        }

        // ---------- GENERIC PROPERTY SETTER ----------
        public static void SafeSetProperty<TControl, TProperty>(
            this TControl control,
            Expression<Func<TControl, TProperty>> propertyExpression,
            TProperty value)
            where TControl : class
        {
            if (control == null) return;

            if (!(propertyExpression.Body is MemberExpression member) ||
                !(member.Member is PropertyInfo property))
                throw new ArgumentException("Expression must be a property", nameof(propertyExpression));

            if (control is Control winControl)
            {
                winControl.SafeInvoke(() => property.SetValue(control, value));
            }
            else if (control is ToolStripItem item)
            {
                item.SafeInvoke(() => property.SetValue(control, value));
            }
            else
            {
                throw new NotSupportedException("Type must be Control or ToolStripItem");
            }
        }

        // ---------- GENERIC PROPERTY GETTER ----------
        public static TProperty SafeGetProperty<TControl, TProperty>(
            this TControl control,
            Expression<Func<TControl, TProperty>> propertyExpression)
            where TControl : class
        {
            if (control == null) return default;

            if (!(propertyExpression.Body is MemberExpression member) ||
                !(member.Member is PropertyInfo property))
                throw new ArgumentException("Expression must be a property", nameof(propertyExpression));

            TProperty result = default;

            if (control is Control winControl)
            {
                winControl.SafeInvoke(() => result = (TProperty)property.GetValue(control));
            }
            else if (control is ToolStripItem item)
            {
                item.SafeInvoke(() => result = (TProperty)property.GetValue(control));
            }
            else
            {
                throw new NotSupportedException("Type must be Control or ToolStripItem");
            }

            return result;
        }

        // ---------- CONVENIENCE: SET TEXT ----------
        public static void SafeSetText(this Control control, string text)
            => control.SafeSetProperty(c => c.Text, text);

        public static void SafeSetText(this ToolStripItem item, string text)
            => item.SafeSetProperty(i => i.Text, text);

        public static string SafeGetText(this Control control)
            => control.SafeGetProperty(c => c.Text);

        public static string SafeGetText(this ToolStripItem item)
            => item.SafeGetProperty(i => i.Text);

        // ---------- CONVENIENCE: ENABLE / DISABLE ----------
        public static void SafeSetEnabled(this Control control, bool enabled)
            => control.SafeSetProperty(c => c.Enabled, enabled);

        public static void SafeSetEnabled(this ToolStripItem item, bool enabled)
            => item.SafeSetProperty(i => i.Enabled, enabled);

        public static bool SafeGetEnabled(this Control control)
            => control.SafeGetProperty(c => c.Enabled);

        public static bool SafeGetEnabled(this ToolStripItem item)
            => item.SafeGetProperty(i => i.Enabled);

        // ---------- CONVENIENCE: APPEND TEXT ----------
        public static void SafeAppendText(this TextBox textBox, string text)
            => textBox.SafeInvoke(() => textBox.AppendText(text));

        public static void SafeAppendText(this RichTextBox richTextBox, string text)
            => richTextBox.SafeInvoke(() => richTextBox.AppendText(text));

        // ---------- CONVENIENCE: SET VALUE ----------
        public static void SafeSetValue(this ProgressBar progressBar, int value)
            => progressBar.SafeInvoke(() =>
            {
                if (value < progressBar.Minimum) value = progressBar.Minimum;
                if (value > progressBar.Maximum) value = progressBar.Maximum;
                progressBar.Value = value;
            });

        public static void SafeSetValue(this TrackBar trackBar, int value)
            => trackBar.SafeInvoke(() =>
            {
                if (value < trackBar.Minimum) value = trackBar.Minimum;
                if (value > trackBar.Maximum) value = trackBar.Maximum;
                trackBar.Value = value;
            });

        public static int SafeGetValue(this ProgressBar progressBar)
            => progressBar.SafeGetProperty(pb => pb.Value);

        public static int SafeGetValue(this TrackBar trackBar)
            => trackBar.SafeGetProperty(tb => tb.Value);

        // ---------- CONVENIENCE: CHECKED STATUS ----------
        public static void SafeSetChecked(this CheckBox checkBox, bool isChecked)
            => checkBox.SafeSetProperty(cb => cb.Checked, isChecked);

        public static bool SafeGetChecked(this CheckBox checkBox)
            => checkBox.SafeGetProperty(cb => cb.Checked);

        public static void SafeSetChecked(this ToolStripMenuItem item, bool isChecked)
            => item.SafeSetProperty(i => i.Checked, isChecked);

        public static bool SafeGetChecked(this ToolStripMenuItem item)
            => item.SafeGetProperty(i => i.Checked);

        public static void SafeSetChecked(this ToolStripButton item, bool isChecked)
            => item.SafeSetProperty(i => i.Checked, isChecked);

        public static bool SafeGetChecked(this ToolStripButton item)
            => item.SafeGetProperty(i => i.Checked);

        // ---------- CONVENIENCE: RADIO BUTTON SELECTION ----------
        public static void SafeSelect(this RadioButton radioButton)
            => radioButton.SafeSetProperty(rb => rb.Checked, true);

        public static bool SafeIsSelected(this RadioButton radioButton)
            => radioButton.SafeGetProperty(rb => rb.Checked);

        public static void SafeSelect(this ToolStripMenuItem item)
            => item.SafeSetProperty(i => i.Checked, true);

        public static bool SafeIsSelected(this ToolStripMenuItem item)
            => item.SafeGetProperty(i => i.Checked);

        public static void SafeSelect(this ToolStripButton item)
            => item.SafeSetProperty(i => i.Checked, true);

        public static bool SafeIsSelected(this ToolStripButton item)
            => item.SafeGetProperty(i => i.Checked);
    }

}