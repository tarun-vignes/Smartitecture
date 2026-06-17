using System;
using System.IO;

namespace Smartitecture.Services.Safety
{
    public sealed class AuditLogger
    {
        private readonly string _logPath;

        public AuditLogger()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smartitecture", "Logs");
            Directory.CreateDirectory(dir);
            _logPath = Path.Combine(dir, "audit.log");
        }

        public void Log(string message)
        {
            try
            {
                var entry = $"[Audit] {DateTime.UtcNow:o} {message}";
                System.Diagnostics.Debug.WriteLine(entry);
                File.AppendAllText(_logPath, entry + Environment.NewLine);
            }
            catch
            {
                // no-op
            }
        }
    }
}
