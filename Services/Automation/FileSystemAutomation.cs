using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Smartitecture.Services.Automation
{
    public class FileSystemAutomation
    {
        public bool CreateFolder(string path)
        {
            try { Directory.CreateDirectory(path); return true; } catch { return false; }
        }

        public bool Move(string source, string dest)
        {
            try
            {
                if (File.Exists(source)) { Directory.CreateDirectory(Path.GetDirectoryName(dest)!); File.Move(source, dest, true); return true; }
                if (Directory.Exists(source)) { Directory.CreateDirectory(Path.GetDirectoryName(dest)!); Directory.Move(source, dest); return true; }
                return false;
            }
            catch { return false; }
        }

        public bool Delete(string path)
        {
            try
            {
                if (File.Exists(path)) { File.Delete(path); return true; }
                if (Directory.Exists(path)) { Directory.Delete(path, true); return true; }
                return false;
            }
            catch { return false; }
        }

        public IEnumerable<string> Search(string root, string pattern)
        {
            if (!Directory.Exists(root)) return Array.Empty<string>();
            try { return Directory.GetFiles(root, pattern, SearchOption.AllDirectories).Take(500); } catch { return Array.Empty<string>(); }
        }
    }
}

