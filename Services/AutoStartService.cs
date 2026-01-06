using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Smartitecture.Services
{
    public static class AutoStartService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppValueName = "Smartitecture";

        public static bool IsEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
                var value = key?.GetValue(AppValueName) as string;
                return !string.IsNullOrWhiteSpace(value);
            }
            catch
            {
                return false;
            }
        }

        public static bool SetEnabled(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true) ??
                                Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

                if (key == null)
                {
                    return false;
                }

                if (!enable)
                {
                    key.DeleteValue(AppValueName, throwOnMissingValue: false);
                    return true;
                }

                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    return false;
                }

                key.SetValue(AppValueName, $"\"{exePath}\"", RegistryValueKind.String);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
