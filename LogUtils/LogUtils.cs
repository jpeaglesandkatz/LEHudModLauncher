using System;
using System.IO;

namespace LogUtils
{
    public enum LogLevel
    {
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }

    public sealed class Logger
    {
        private static readonly object _fileLock = new();
        private static readonly string DefaultFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEHuDModLauncher");
        private static string _logFolder = DefaultFolder;
        private static string _logFileName = "debug.log";
        private static long _maxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

#if DEBUG
        public static LogLevel CurrentLevel { get; set; } = LogLevel.Debug;
#else
        public static LogLevel CurrentLevel { get; set; } = LogLevel.Info;
#endif

        // Global shared logger instance (convenience; keeps a single writer for the app)
        private static readonly Lazy<Logger> _global = new(() => new Logger());
        public static Logger Global => _global.Value;

        // Keep constructor public for compatibility with existing code that does 'new Logger()'
        public Logger()
        {
            try
            {
                EnsureLogFolder();
            }
            catch
            {
                // Best-effort only — do not throw from ctor
            }
        }

        /// <summary>
        /// Configure log folder, filename and rotating size (bytes). Call early in application start if you want custom location/size.
        /// </summary>
        public static void Configure(string? folder = null, string? fileName = null, long? maxFileSizeBytes = null, LogLevel? level = null)
        {
            if (!string.IsNullOrWhiteSpace(folder)) _logFolder = folder!;
            if (!string.IsNullOrWhiteSpace(fileName)) _logFileName = fileName!;
            if (maxFileSizeBytes.HasValue && maxFileSizeBytes.Value > 0) _maxFileSizeBytes = maxFileSizeBytes.Value;
            if (level.HasValue) CurrentLevel = level.Value;

            try
            {
                Directory.CreateDirectory(_logFolder);
            }
            catch
            {
                // ignore
            }
        }

        private static string LogFilePath => Path.Combine(_logFolder, _logFileName);

        private static void EnsureLogFolder()
        {
            if (!Directory.Exists(_logFolder))
                Directory.CreateDirectory(_logFolder);
        }

        private static void RotateIfNeeded()
        {
            try
            {
                var path = LogFilePath;
                if (!File.Exists(path)) return;

                var fi = new FileInfo(path);
                if (fi.Length <= _maxFileSizeBytes) return;

                string archiveName = Path.Combine(_logFolder,
                    Path.GetFileNameWithoutExtension(_logFileName)
                    + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss")
                    + Path.GetExtension(_logFileName));

                // Move the current file to archive name
                File.Move(path, archiveName);
            }
            catch
            {
                // Swallow rotation errors; logging must remain best-effort
            }
        }

        public void Log(LogLevel level, string message)
        {
            try
            {
                if (level < CurrentLevel) return; // filter by configured level

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";

                // Console (colored) -- keep this non-failing
                try
                {
                    WriteColored(level, logEntry);
                }
                catch { /* ignore console errors */ }

                // File (thread-safe, with simple rotation)
                lock (_fileLock)
                {
                    try
                    {
                        EnsureLogFolder();
                        RotateIfNeeded();
                        // Append with sharing so other readers (LogViewer) can open file
                        using var stream = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                        using var writer = new StreamWriter(stream);
                        writer.WriteLine(logEntry);
                        writer.Flush();
                    }
                    catch
                    {
                        // ignore file IO errors to avoid crashing callers
                    }
                }
            }
            catch
            {
                // swallow everything - logging should not throw
            }
        }

        // Instance helpers (existing code uses instance methods; keep them)
        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message) => Log(LogLevel.Error, message);

        // Static convenience helpers — call from anywhere: Logger.DebugStatic(...), or use Logger.Global.Debug(...)
        public static void DebugStatic(string message) => Global.Debug(message);
        public static void InfoStatic(string message) => Global.Info(message);
        public static void WarningStatic(string message) => Global.Warning(message);
        public static void ErrorStatic(string message) => Global.Error(message);

        private static void WriteColored(LogLevel level, string message)
        {
            // Only try to write colored output if a console is available
            try
            {
                if (!Environment.UserInteractive) return;

                ConsoleColor original = Console.ForegroundColor;

                switch (level)
                {
                    case LogLevel.Debug: Console.ForegroundColor = ConsoleColor.Gray; break;
                    case LogLevel.Info: Console.ForegroundColor = ConsoleColor.Green; break;
                    case LogLevel.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case LogLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                }

                Console.WriteLine(message);
                Console.ForegroundColor = original;
            }
            catch
            {
                // ignore console errors
            }
        }
    }
}