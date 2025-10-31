using System;
using System.IO;
using System.Text.Json;

namespace Smartitecture.Services
{
    public class UserPreferences
    {
        public string Theme { get; set; } = "Dark"; // Dark | Light
        public bool NotificationsEnabled { get; set; } = true;
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

