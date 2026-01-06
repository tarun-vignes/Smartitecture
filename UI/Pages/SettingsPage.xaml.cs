using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Smartitecture.Services;

namespace Smartitecture.UI.Pages
{
    public partial class SettingsPage : Page
    {
        private readonly PreferencesService _prefsService = new PreferencesService();
        private readonly ConfigurationService _configService = new ConfigurationService();
        private UserPreferences _prefs = new UserPreferences();
        private bool _isLoading;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoading = true;

            _prefs = _prefsService.Load();

            var theme = ThemeManager.Parse(_prefs.Theme);
            ThemeSelector.SelectedIndex = theme == ThemeMode.Dark ? 1 : theme == ThemeMode.Light ? 2 : 0;

            NotificationsToggle.IsChecked = _prefs.NotificationsEnabled;
            AutoStartToggle.IsChecked = AutoStartService.IsEnabled();

            RefreshProviderStatus();
            RefreshModels();

            _isLoading = false;
        }

private void RefreshModels()
        {
            CurrentModelText.Text = AppSession.LlmService.CurrentModel;

            ModelSelector.Items.Clear();
            foreach (var model in AppSession.LlmService.AvailableModels)
            {
                ModelSelector.Items.Add(model);
            }

            var current = AppSession.LlmService.CurrentModel;
            if (ModelSelector.Items.Cast<string>().Contains(current))
            {
                ModelSelector.SelectedItem = current;
            }
            else if (ModelSelector.Items.Count > 0)
            {
                ModelSelector.SelectedIndex = 0;
            }
        }

        private void RefreshProviderStatus()
        {
            var openAiConfigured = _configService.IsOpenAIConfigured();
            OpenAiStatusText.Text = openAiConfigured ? "Configured" : "Not configured";
            OpenAiStatusText.Style = (Style)FindResource(openAiConfigured ? "Text.Success" : "Text.Warning");

            var claudeConfigured = _configService.IsClaudeConfigured();
            ClaudeStatusText.Text = claudeConfigured ? "Configured" : "Not configured";
            ClaudeStatusText.Style = (Style)FindResource(claudeConfigured ? "Text.Success" : "Text.Warning");
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;

            var theme = ThemeSelector.SelectedIndex == 1 ? ThemeMode.Dark :
                        ThemeSelector.SelectedIndex == 2 ? ThemeMode.Light :
                        ThemeMode.System;

            _prefs.Theme = theme.ToString();
            _prefsService.Save(_prefs);
            ThemeManager.ApplyTheme(theme);
            SaveStatusText.Text = "Theme updated.";
        }

        private void NotificationsToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            _prefs.NotificationsEnabled = NotificationsToggle.IsChecked == true;
            _prefsService.Save(_prefs);
            SaveStatusText.Text = "Preferences saved.";
        }

        private async void ModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;

            if (ModelSelector.SelectedItem is not string modelName || string.IsNullOrWhiteSpace(modelName))
            {
                return;
            }

            await SwitchModelAsync(modelName);
        }

        private async Task SwitchModelAsync(string modelName)
        {
            try
            {
                var ok = await AppSession.LlmService.SwitchModelAsync(modelName);
                if (!ok)
                {
                    SaveStatusText.Text = "Model switch not available.";
                    return;
                }

                var config = _configService.GetConfiguration();
                config.UI.DefaultModel = modelName;
                await _configService.SaveConfigurationAsync(config);

                CurrentModelText.Text = AppSession.LlmService.CurrentModel;
                SaveStatusText.Text = $"Switched to {modelName}.";
            }
            catch (Exception ex)
            {
                SaveStatusText.Text = $"Failed to switch model: {ex.Message}";
            }
        }

        private async void SaveKeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openai = OpenAiKeyBox.Password?.Trim();
                var claude = ClaudeKeyBox.Password?.Trim();

                if (!string.IsNullOrWhiteSpace(openai))
                {
                    await _configService.SetOpenAIApiKeyAsync(openai);
                    Environment.SetEnvironmentVariable("OPENAI_API_KEY", openai, EnvironmentVariableTarget.Process);
                    Environment.SetEnvironmentVariable("OPENAI_API_KEY", openai, EnvironmentVariableTarget.User);
                }

                if (!string.IsNullOrWhiteSpace(claude))
                {
                    await _configService.SetClaudeApiKeyAsync(claude);
                    Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", claude, EnvironmentVariableTarget.Process);
                    Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", claude, EnvironmentVariableTarget.User);
                }

                if (AppSession.LlmService is MultiModelAIService multi)
                {
                    multi.ReloadProviders();
                }

                OpenAiKeyBox.Password = string.Empty;
                ClaudeKeyBox.Password = string.Empty;

                RefreshProviderStatus();
                RefreshModels();

                SaveStatusText.Text = "Keys saved. Providers refreshed.";
            }
            catch (Exception ex)
            {
                SaveStatusText.Text = $"Failed to save keys: {ex.Message}";
            }
        }

        private async void ClearKeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = _configService.GetConfiguration();
                config.OpenAI.ApiKey = string.Empty;
                config.Claude.ApiKey = string.Empty;
                await _configService.SaveConfigurationAsync(config);

                Environment.SetEnvironmentVariable("OPENAI_API_KEY", null, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("OPENAI_API_KEY", null, EnvironmentVariableTarget.User);
                Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", null, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", null, EnvironmentVariableTarget.User);

                if (AppSession.LlmService is MultiModelAIService multi)
                {
                    multi.ReloadProviders();
                }

                RefreshProviderStatus();
                RefreshModels();

                SaveStatusText.Text = "Stored keys cleared.";
            }
            catch (Exception ex)
            {
                SaveStatusText.Text = $"Failed to clear keys: {ex.Message}";
            }
        }

        private void AutoStartToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            var enable = AutoStartToggle.IsChecked == true;
            var ok = AutoStartService.SetEnabled(enable);
            if (!ok)
            {
                _isLoading = true;
                AutoStartToggle.IsChecked = !enable;
                _isLoading = false;

                SaveStatusText.Text = "Failed to update startup setting.";
                return;
            }

            SaveStatusText.Text = enable ? "Will start with Windows." : "Startup disabled.";
        }
    }
}
