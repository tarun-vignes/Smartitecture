using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace Smartitecture.Services
{
    public class LanguageService
    {
        private const string LanguageSettingsFile = "language-settings.json";
        private const string DefaultLanguage = "en-US";
        private readonly ILogger<LanguageService> _logger;
        private string _currentLanguage;
        private Dictionary<string, string> _localizedStrings;

        public event EventHandler<string> LanguageChanged;

        public LanguageService(ILogger<LanguageService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentLanguage = DefaultLanguage;
            _localizedStrings = new Dictionary<string, string>();
        }

        public string CurrentLanguage => _currentLanguage;
        public IReadOnlyDictionary<string, string> LocalizedStrings => _localizedStrings;

        public async Task InitializeLanguageAsync()
        {
            try
            {
                if (File.Exists(LanguageSettingsFile))
                {
                    var json = await File.ReadAllTextAsync(LanguageSettingsFile);
                    var settings = JsonSerializer.Deserialize<LanguageSettings>(json);
                    if (settings != null && !string.IsNullOrEmpty(settings.LanguageCode))
                    {
                        await SetLanguageAsync(settings.LanguageCode);
                        return;
                    }
                }
                
                // Set default language if no settings found
                await SetLanguageAsync(DefaultLanguage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize language settings");
                // Fall back to default language
                await SetLanguageAsync(DefaultLanguage);
            }
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                throw new ArgumentException("Language code cannot be null or empty", nameof(languageCode));

            if (string.Equals(_currentLanguage, languageCode, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                // In a real app, you would load the language resources from resource files
                // For this example, we'll use hardcoded strings
                _localizedStrings = LoadLocalizedStrings(languageCode);
                _currentLanguage = languageCode;

                // Update the current thread's culture
                var cultureInfo = new CultureInfo(languageCode);
                System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;

                // Save the preference
                await SaveLanguagePreferenceAsync();

                // Notify listeners that the language has changed
                LanguageChanged?.Invoke(this, languageCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to set language to {languageCode}");
                throw;
            }
        }

        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (_localizedStrings.TryGetValue(key, out var value))
                return value;

            _logger.LogWarning($"Localized string not found for key: {key}");
            return $"[[{key}]]"; // Return the key in double brackets as a fallback
        }

        private async Task SaveLanguagePreferenceAsync()
        {
            try
            {
                var settings = new LanguageSettings { LanguageCode = _currentLanguage };
                var json = JsonSerializer.Serialize(settings);
                await File.WriteAllTextAsync(LanguageSettingsFile, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save language settings");
            }
        }

        private Dictionary<string, string> LoadLocalizedStrings(string languageCode)
        {
            // In a real application, this would load from resource files
            // For now, we'll return some hardcoded strings for demonstration
            var strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (languageCode.StartsWith("es")) // Spanish
            {
                strings["AppTitle"] = "Smartitecture";
                strings["File"] = "Archivo";
                strings["Exit"] = "Salir";
                strings["Settings"] = "Configuración";
                strings["Appearance"] = "Apariencia";
                strings["Language"] = "Idioma";
                strings["Accessibility"] = "Accesibilidad";
                strings["Help"] = "Ayuda";
                strings["About"] = "Acerca de";
            }
            else // Default to English
            {
                strings["AppTitle"] = "Smartitecture";
                strings["File"] = "File";
                strings["Exit"] = "Exit";
                strings["Settings"] = "Settings";
                strings["Appearance"] = "Appearance";
                strings["Language"] = "Language";
                strings["Accessibility"] = "Accessibility";
                strings["Help"] = "Help";
                strings["About"] = "About";
            }

            return strings;
        }

        private class LanguageSettings
        {
            public string LanguageCode { get; set; } = string.Empty;
        }
    }
}
