using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace UpdaterReplacer
{
    internal static class Program
    {
        /// <summary>
        /// Args:
        ///   0 - path to old updater.exe (to delete)
        ///   1 - path to new updater.exe (already downloaded)
        ///   2 - optional args to pass when starting the new updater
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: UpdaterReplacer <oldPath> <newPath> [args]");
                return;
            }

            string oldPath = args[0];
            string newPath = args[1];
            string arguments = args.Length > 2 ? args[2] : "";

            try
            {
                // Wait until the old updater process is gone
                Thread.Sleep(1500);
                for (int i = 0; i < 10; i++)
                {
                    if (!IsFileLocked(oldPath)) break;
                    Thread.Sleep(500);
                }

                // Delete old updater
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }

                // Move new updater into place
                string finalPath = oldPath;
                File.Move(newPath, finalPath, overwrite: true);

                // Start the new updater
                var psi = new ProcessStartInfo
                {
                    FileName = finalPath,
                    Arguments = arguments,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdaterReplacer failed: {ex.Message}");
            }
        }

        private static bool IsFileLocked(string path)
        {
            try
            {
                using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
