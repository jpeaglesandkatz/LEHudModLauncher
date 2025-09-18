using LauncherUpdater;
using System;
using System.Windows.Forms;

namespace LauncherUpdater
{
    internal static class LauncherMain
    {

        static class Program
        {
            [STAThread]
            static void Main(string[] args)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UpdaterForm(args));
            }
        }
    }
}


