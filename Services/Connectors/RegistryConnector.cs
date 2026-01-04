using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smartitecture.Services.Connectors
{
    public class RegistryConnector
    {
        public object GetValue(RegistryHive hive, string subKeyPath, string name)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKeyPath);
            return key?.GetValue(name);
        }

        public bool SetValue(RegistryHive hive, string subKeyPath, string name, object value)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.CreateSubKey(subKeyPath, true);
                key?.SetValue(name, value);
                return true;
            }
            catch { return false; }
        }

        public bool DeleteValue(RegistryHive hive, string subKeyPath, string name)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.OpenSubKey(subKeyPath, true);
                if (key == null) return false;
                key.DeleteValue(name, false);
                return true;
            }
            catch { return false; }
        }

        public IEnumerable<string> EnumerateValues(RegistryHive hive, string subKeyPath)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(subKeyPath);
            return key?.GetValueNames() ?? Array.Empty<string>();
        }
    }
}

