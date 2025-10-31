using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Smartitecture.Services.Safety
{
    public class AuditLogger
    {
        private readonly string _logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smartitecture", "audit.log");

        public AuditLogger()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
        }

        public async Task LogOperationAsync(string actor, string action, string details)
        {
            var line = $"{DateTime.UtcNow:O}\t{actor}\t{action}\t{details}" + Environment.NewLine;
            await File.AppendAllTextAsync(_logFile, line);
        }

        public async Task<IReadOnlyList<string>> GetLogsAsync(int max = 500)
        {
            if (!File.Exists(_logFile)) return Array.Empty<string>();
            var lines = await File.ReadAllLinesAsync(_logFile);
            if (lines.Length <= max) return lines;
            var slice = new string[max];
            Array.Copy(lines, Math.Max(0, lines.Length - max), slice, 0, max);
            return slice;
        }
    }
}

