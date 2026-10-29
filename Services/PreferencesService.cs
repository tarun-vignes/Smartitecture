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
        public bool AutoUpdate { get; set; } = false;
        public string UpdateChannel { get; set; } = "Stable";
        public bool ShareDiagnostics { get; set; } = false;
        public int ApiPort { get; set; } = 8080;
        public string BackendBaseUrl { get; set; } = "";
        public string BackendApiKey { get; set; } = "";
    }

    public class PreferencesService
    {
        private readonly string _prefsPath;

        public string? LoadWarning { get; private set; }

        public PreferencesService() : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smartitecture", "prefs.json"))
        {
        }

        public PreferencesService(string preferencesPath)
        {
            _prefsPath = Path.GetFullPath(preferencesPath);
        }

        public UserPreferences Load()
        {
            LoadWarning = null;
            foreach (var path in new[] { _prefsPath, _prefsPath + ".bak" })
            {
                if (!File.Exists(path)) continue;
                try
                {
                    var prefs = JsonSerializer.Deserialize<UserPreferences>(File.ReadAllText(path))
                        ?? throw new JsonException("Settings were empty.");
                    var storedKey = prefs.BackendApiKey ?? string.Empty;
                    try
                    {
                        prefs.BackendApiKey = WindowsCredentialProtection.Unprotect(storedKey);
                    }
                    catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException or FormatException)
                    {
                        prefs.BackendApiKey = string.Empty;
                        LoadWarning = "Your saved connection could not be unlocked. Local tools still work; reconnect online AI in advanced settings.";
                    }
                    if (path != _prefsPath)
                        LoadWarning = "Settings were recovered from a backup. Review them and save to keep using this copy.";
                    // Migrate old plaintext credentials without keeping a plaintext backup.
                    if (!string.IsNullOrEmpty(storedKey) && !WindowsCredentialProtection.IsProtected(storedKey))
                    {
                        try { Save(prefs, keepBackup: false); }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.Cryptography.CryptographicException)
                        {
                            LoadWarning = "Your settings were loaded, but the saved connection could not be secured. Please save settings again.";
                        }
                    }
                    return prefs;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or System.Security.Cryptography.CryptographicException)
                {
                    LoadWarning = "Saved settings could not be loaded. Default settings are in use; local tools are still available.";
                }
            }
            return new UserPreferences();
        }

        public void Save(UserPreferences prefs) => Save(prefs, keepBackup: true);

        private void Save(UserPreferences prefs, bool keepBackup)
        {
            ArgumentNullException.ThrowIfNull(prefs);
            Directory.CreateDirectory(Path.GetDirectoryName(_prefsPath)!);
            // Clone so encryption never changes the caller's in-memory credential.
            var stored = JsonSerializer.Deserialize<UserPreferences>(JsonSerializer.Serialize(prefs))!;
            stored.BackendApiKey = WindowsCredentialProtection.Protect(prefs.BackendApiKey ?? string.Empty);
            var json = JsonSerializer.Serialize(stored, new JsonSerializerOptions { WriteIndented = true });
            var temporaryPath = _prefsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush(flushToDisk: true);
                }
                if (File.Exists(_prefsPath))
                {
                    // A backup must never reintroduce a legacy plaintext credential.
                    UserPreferences? previous = null;
                    try { previous = JsonSerializer.Deserialize<UserPreferences>(File.ReadAllText(_prefsPath)); }
                    catch (JsonException) { }
                    var safeBackup = keepBackup && previous != null && (string.IsNullOrEmpty(previous?.BackendApiKey)
                        || WindowsCredentialProtection.IsProtected(previous.BackendApiKey));
                    File.Replace(temporaryPath, _prefsPath, safeBackup ? _prefsPath + ".bak" : null);
                }
                else File.Move(temporaryPath, _prefsPath);
                if (!keepBackup && File.Exists(_prefsPath + ".bak")) File.Delete(_prefsPath + ".bak");
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }
    }
}
