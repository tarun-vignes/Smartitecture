using System;
using System.IO;
using System.Text.Json;

namespace Smartitecture.Services
{
    public class UserPreferences
    {
        public string Theme { get; set; } = "Dark"; // Dark | Light
        public bool NotificationsEnabled { get; set; } = true;
        public string Language { get; set; } = "en-US";
        public string Region { get; set; } = "US";
        public string TimeFormat { get; set; } = "12h";
        public string Units { get; set; } = "Metric";
        public string TextSize { get; set; } = "Default";
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimized { get; set; } = false;
        public bool ReduceMotion { get; set; } = false;
        public bool AutoUpdate { get; set; } = true;
        public string UpdateChannel { get; set; } = "Stable";
        public bool ShareDiagnostics { get; set; } = false;
        public int ApiPort { get; set; } = 8080;
    }

    public class PreferencesService
    {
        private readonly string _prefsPath;

        public PreferencesService()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smartitecture");
            Directory.CreateDirectory(dir);
            _prefsPath = Path.Combine(dir, "prefs.json");
        }

        public UserPreferences Load()
        {
            try
            {
                if (File.Exists(_prefsPath))
                {
                    var json = File.ReadAllText(_prefsPath);
                    var prefs = JsonSerializer.Deserialize<UserPreferences>(json);
                    if (prefs != null) return prefs;
                }
            }
            catch { }
            return new UserPreferences();
        }

        public void Save(UserPreferences prefs)
        {
            try
            {
                var json = JsonSerializer.Serialize(prefs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_prefsPath, json);
            }
            catch { }
        }
    }
}
