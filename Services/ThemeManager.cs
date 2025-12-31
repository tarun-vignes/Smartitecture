using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace Smartitecture.Services
{
    public static class ThemeManager
    {
        private static readonly Uri DarkThemeSource = new Uri("Themes/Theme.Dark.xaml", UriKind.Relative);
        private static readonly Uri LightThemeSource = new Uri("Themes/Theme.Light.xaml", UriKind.Relative);

        public static ThemeMode Parse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return ThemeMode.System;
            if (Enum.TryParse<ThemeMode>(value.Trim(), ignoreCase: true, out var mode)) return mode;
            return ThemeMode.System;
        }

        public static ThemeMode GetEffectiveTheme(ThemeMode requested)
        {
            return requested == ThemeMode.System ? GetSystemTheme() : requested;
        }

        public static ThemeMode GetSystemTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var appsUseLight = key?.GetValue("AppsUseLightTheme");
                if (appsUseLight is int i)
                {
                    return i == 0 ? ThemeMode.Dark : ThemeMode.Light;
                }
            }
            catch
            {
            }

            return ThemeMode.Light;
        }

        public static void ApplyTheme(ThemeMode requested)
        {
            var effective = GetEffectiveTheme(requested);
            var source = effective == ThemeMode.Dark ? DarkThemeSource : LightThemeSource;

            var app = Application.Current;
            if (app == null) return;

            var merged = app.Resources.MergedDictionaries;

            var existing = merged.FirstOrDefault(IsThemeDictionary);
            if (existing != null)
            {
                if (existing.Source != null && UriEquals(existing.Source, source)) return;
                merged.Remove(existing);
            }

            merged.Add(new ResourceDictionary { Source = source });
        }

        private static bool IsThemeDictionary(ResourceDictionary dictionary)
        {
            var src = dictionary.Source?.ToString() ?? string.Empty;
            return src.IndexOf("Theme.Dark.xaml", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   src.IndexOf("Theme.Light.xaml", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool UriEquals(Uri left, Uri right)
        {
            return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
