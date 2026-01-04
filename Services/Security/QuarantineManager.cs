using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Smartitecture.Services.Security
{
    public class QuarantineManager
    {
        private readonly string _quarantineDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smartitecture", "Quarantine");

        public QuarantineManager()
        {
            Directory.CreateDirectory(_quarantineDir);
        }

        public async Task<bool> QuarantineFileAsync(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                var dest = Path.Combine(_quarantineDir, Path.GetFileName(path) + ".quarantined");
                File.Move(path, dest, true);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> RestoreFileAsync(string quarantinedPath, string restoreTo)
        {
            try
            {
                if (!File.Exists(quarantinedPath)) return false;
                File.Move(quarantinedPath, restoreTo, true);
                return true;
            }
            catch { return false; }
        }

        public IReadOnlyList<string> ListQuarantined()
        {
            if (!Directory.Exists(_quarantineDir)) return Array.Empty<string>();
            return Directory.GetFiles(_quarantineDir);
        }
    }
}

