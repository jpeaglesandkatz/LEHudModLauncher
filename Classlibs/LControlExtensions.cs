
namespace LEHuDModLauncher.Classlibs
{
    public static class LControlExtensions
    {
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }

        // For ToolStripItems (like ToolStripStatusLabel)
        public static void SafeInvoke(this ToolStripItem item, Action action)
        {
            if (item?.Owner == null)
            {
                action();
                return;
            }

            if (item.Owner.InvokeRequired)
                item.Owner.Invoke(action);
            else
                action();
        }
    }
}