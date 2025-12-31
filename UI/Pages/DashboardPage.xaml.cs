using System;
using System.Windows.Controls;
using Smartitecture.Services;

namespace Smartitecture.UI.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly PreferencesService _prefsService = new PreferencesService();
        private readonly UserPreferences _prefs;

        public DashboardPage()
        {
            InitializeComponent();

            _prefs = _prefsService.Load();
            GreetingText.Text = $"Welcome, {Environment.UserName}";
            SetThemeSelectorFromPrefs();
        }

        private void OpenChat_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppNavigator.Navigate(new ChatPage(AppSession.LlmService));
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var theme = GetSelectedTheme();
                _prefs.Theme = theme.ToString();
                _prefsService.Save(_prefs);
                ThemeManager.ApplyTheme(theme);
            }
            catch
            {
            }
        }

        private void SetThemeSelectorFromPrefs()
        {
            var mode = ThemeManager.Parse(_prefs.Theme);
            ThemeSelector.SelectedIndex = mode == ThemeMode.Dark ? 1 : mode == ThemeMode.Light ? 2 : 0;
        }

        private ThemeMode GetSelectedTheme()
        {
            return ThemeSelector.SelectedIndex == 1 ? ThemeMode.Dark :
                   ThemeSelector.SelectedIndex == 2 ? ThemeMode.Light :
                   ThemeMode.System;
        }

        private void BackToGettingStarted_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppNavigator.Navigate(new StartupPage());
        }
    }
}
