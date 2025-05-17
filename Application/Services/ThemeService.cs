using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace AIPal.Services
{
    /// <summary>
    /// Theme options for the application.
    /// </summary>
    public enum AppTheme
    {
        /// <summary>
        /// Light theme.
        /// </summary>
        Light,
        
        /// <summary>
        /// Dark theme.
        /// </summary>
        Dark,
        
        /// <summary>
        /// Use the system theme.
        /// </summary>
        System
    }
    
    /// <summary>
    /// Service for managing the application theme.
    /// </summary>
    public class ThemeService
    {
        private const string ThemeSettingKey = "AppTheme";
        private readonly ApplicationDataContainer _localSettings;
        
        /// <summary>
        /// Event raised when the theme changes.
        /// </summary>
        public event EventHandler<AppTheme> ThemeChanged;
        
        /// <summary>
        /// Initializes a new instance of the ThemeService class.
        /// </summary>
        public ThemeService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
        }
        
        /// <summary>
        /// Gets the current theme.
        /// </summary>
        public AppTheme CurrentTheme
        {
            get
            {
                if (_localSettings.Values.TryGetValue(ThemeSettingKey, out var value) && 
                    value is string themeString && 
                    Enum.TryParse<AppTheme>(themeString, out var theme))
                {
                    return theme;
                }
                
                return AppTheme.System;
            }
        }
        
        /// <summary>
        /// Sets the application theme.
        /// </summary>
        /// <param name="theme">The theme to set</param>
        public async Task SetThemeAsync(AppTheme theme)
        {
            // Save the theme setting
            _localSettings.Values[ThemeSettingKey] = theme.ToString();
            
            // Apply the theme
            await ApplyThemeAsync(theme);
            
            // Raise the theme changed event
            ThemeChanged?.Invoke(this, theme);
        }
        
        /// <summary>
        /// Initializes the theme based on saved settings.
        /// </summary>
        public async Task InitializeThemeAsync()
        {
            await ApplyThemeAsync(CurrentTheme);
        }
        
        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme to apply</param>
        private async Task ApplyThemeAsync(AppTheme theme)
        {
            // Get the root element
            var rootElement = (Application.Current as App)?.MainWindow?.Content as FrameworkElement;
            if (rootElement == null)
            {
                return;
            }
            
            ElementTheme elementTheme;
            
            switch (theme)
            {
                case AppTheme.Light:
                    elementTheme = ElementTheme.Light;
                    break;
                    
                case AppTheme.Dark:
                    elementTheme = ElementTheme.Dark;
                    break;
                    
                case AppTheme.System:
                default:
                    elementTheme = ElementTheme.Default;
                    break;
            }
            
            // Apply the theme to the root element
            rootElement.RequestedTheme = elementTheme;
            
            // Wait for the UI thread to process the theme change
            await Task.Delay(10);
        }
        
        /// <summary>
        /// Toggles between light and dark themes.
        /// </summary>
        public async Task ToggleThemeAsync()
        {
            AppTheme newTheme;
            
            switch (CurrentTheme)
            {
                case AppTheme.Light:
                    newTheme = AppTheme.Dark;
                    break;
                    
                case AppTheme.Dark:
                    newTheme = AppTheme.Light;
                    break;
                    
                case AppTheme.System:
                default:
                    // If using system theme, switch to light or dark based on current system theme
                    var rootElement = (Application.Current as App)?.MainWindow?.Content as FrameworkElement;
                    if (rootElement?.ActualTheme == ElementTheme.Dark)
                    {
                        newTheme = AppTheme.Light;
                    }
                    else
                    {
                        newTheme = AppTheme.Dark;
                    }
                    break;
            }
            
            await SetThemeAsync(newTheme);
        }
    }
}
