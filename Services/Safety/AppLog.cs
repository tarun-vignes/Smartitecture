using System;
using System.IO;

namespace Smartitecture.Services.Safety
{
    public static class AppLog
    {
        private static readonly object SyncRoot = new();

        public static string LogDirectory { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Smartitecture",
            "Logs");

        public static string LogPath { get; } = Path.Combine(LogDirectory, "app.log");

        public static void Info(string message)
        {
            Write("Info", message);
        }

        public static void Error(string message, Exception exception)
        {
            Write("Error", $"{message}{Environment.NewLine}{exception}");
        }

        private static void Write(string level, string message)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                var entry = $"[{level}] {DateTimeOffset.UtcNow:o} {message}";
                System.Diagnostics.Debug.WriteLine(entry);

                lock (SyncRoot)
                {
                    File.AppendAllText(LogPath, entry + Environment.NewLine);
                }
            }
            catch
            {
                // Logging must never block app startup or shutdown.
            }
        }
    }
}
