using System;
using System.Collections.Generic;
using System.Windows;

namespace Smartitecture.Services
{
    public enum AppColorTheme
    {
        Dark,
        Light,
        System
    }

    public static class ThemeManager
    {
        public static void Apply(AppColorTheme theme)
        {
            var resolved = theme == AppColorTheme.System ? ResolveSystemTheme() : theme;
            var target = resolved == AppColorTheme.Dark ? "Dark.xaml" : "Light.xaml";
            var app = Application.Current;
            if (app == null) return;
            var merged = app.Resources.MergedDictionaries;

            // Remove any previous theme dictionaries
            var toRemove = new List<ResourceDictionary>();
            foreach (var md in merged)
            {
                if (md.Source != null && md.Source.OriginalString.Contains("Resources/Themes/"))
                    toRemove.Add(md);
            }
            foreach (var md in toRemove) merged.Remove(md);

            // Add the selected theme
            merged.Add(new ResourceDictionary
            {
                Source = new Uri($"Resources/Themes/{target}", UriKind.Relative)
            });
        }

        public static AppColorTheme ResolveSystemTheme()
        {
            try
            {
                // Windows registry: 1=light, 0=dark
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                if (value is int i) return i == 1 ? AppColorTheme.Light : AppColorTheme.Dark;
            }
            catch { }
            return AppColorTheme.Dark;
        }
    }
}
