using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Logging;

namespace Smartitecture.Services
{
    public class ThemeService
    {
        private const string ThemeSettingsFile = "theme-settings.json";
        private readonly ILogger<ThemeService> _logger;
        private bool _isDarkTheme;
        private string _currentTheme = "Default";

        public event EventHandler<string> ThemeChanged;

        public ThemeService(ILogger<ThemeService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsDarkTheme => _currentTheme == "Dark" || (_currentTheme == "Default" && IsSystemUsingDarkTheme());

        public string CurrentTheme => _currentTheme;

        private bool IsSystemUsingDarkTheme()
        {
            // Check if the system is using dark theme
            // This is a simple implementation - you might need to adjust based on your requirements
            var defaultBackground = SystemParameters.WindowGlassBrush.ToString();
            var isDark = defaultBackground.Contains("#FF1E1E1E"); // Dark theme background color
            return isDark;
        }

        public async Task InitializeThemeAsync()
        {
            try
            {
                if (File.Exists(ThemeSettingsFile))
                {
                    var json = await File.ReadAllTextAsync(ThemeSettingsFile);
                    var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
                    if (settings != null && !string.IsNullOrEmpty(settings.Theme))
                    {
                        SetTheme(settings.Theme);
                        return;
                    }
                }
                
                // Default to system theme if no saved preference
                SetTheme("Default");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load theme settings");
                // Fall back to system theme
                SetTheme("Default");
            }
        }
        
        public void SetTheme(string theme)
        {
            if (string.IsNullOrEmpty(theme) || (theme != "Light" && theme != "Dark" && theme != "Default"))
            {
                theme = "Default";
            }
            
            if (_currentTheme != theme)
            {
                _currentTheme = theme;
                _isDarkTheme = IsDarkTheme; // Update the dark theme flag
                ApplyTheme();
                ThemeChanged?.Invoke(this, theme);
                _ = SaveThemePreferenceAsync();
            }
        }

        private async Task SaveThemePreferenceAsync()
        {
            try
            {
                var settings = new ThemeSettings { Theme = _currentTheme };
                var json = JsonSerializer.Serialize(settings);
                await File.WriteAllTextAsync(ThemeSettingsFile, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save theme settings");
            }
        }

        public void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme();
            ThemeChanged?.Invoke(this, _isDarkTheme);
        }

        private void ApplyTheme()
        {
            // Apply theme to the application
            var app = Application.Current;
            if (app == null) return;

            try
            {
                // Clear existing resources
                app.Resources.MergedDictionaries.Clear();

                // Add base theme resources
                var baseTheme = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/Smartitecture;component/Themes/Generic.xaml", UriKind.Absolute)
                };
                app.Resources.MergedDictionaries.Add(baseTheme);

                // Add theme-specific resources
                var themeDictionary = new ResourceDictionary();
                
                if (IsDarkTheme)
                {
                    // Dark theme colors
                    themeDictionary["BackgroundColor"] = (Color)ColorConverter.ConvertFromString("#FF1E1E1E");
                    themeDictionary["ForegroundColor"] = Colors.White;
                    themeDictionary["AccentColor"] = (Color)ColorConverter.ConvertFromString("#FF0078D7");
                    themeDictionary["ControlBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#FF2D2D30");
                    themeDictionary["ControlForegroundColor"] = Colors.White;
                }
                else
                {
                    // Light theme colors
                    themeDictionary["BackgroundColor"] = Colors.White;
                    themeDictionary["ForegroundColor"] = (Color)ColorConverter.ConvertFromString("#FF000000");
                    themeDictionary["AccentColor"] = (Color)ColorConverter.ConvertFromString("#FF0078D7");
                    themeDictionary["ControlBackgroundColor"] = Colors.White;
                    themeDictionary["ControlForegroundColor"] = (Color)ColorConverter.ConvertFromString("#FF000000");
                }

                app.Resources.MergedDictionaries.Add(themeDictionary);

                // Update system colors
                SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying theme");
                // Fall back to default theme on error
                _currentTheme = "Default";
            }
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast) ||
                e.PropertyName == nameof(SystemParameters.UsesLightTheme))
            {
                // Only re-apply if we're using the system theme
                if (_currentTheme == "Default")
                {
                    _isDarkTheme = IsSystemUsingDarkTheme();
                    ApplyTheme();
                }
            }
        }

        private class ThemeSettings
        {
            public string Theme { get; set; } = "Default";
        }
    }
}
