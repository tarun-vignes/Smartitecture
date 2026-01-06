using System;
using System.Windows;
using System.Windows.Controls;
using Smartitecture.Services;

namespace Smartitecture.UI.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly ConfigurationService _configService = new ConfigurationService();

        public DashboardPage()
        {
            InitializeComponent();

            GreetingText.Text = $"Hello, {Environment.UserName}";
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            var openAi = _configService.IsOpenAIConfigured();
            var claude = _configService.IsClaudeConfigured();

            AiStatusText.Text = (openAi || claude)
                ? $"AI providers: {(openAi ? "OpenAI" : string.Empty)}{(openAi && claude ? ", " : string.Empty)}{(claude ? "Claude" : string.Empty)}"
                : "AI providers: not configured (use Settings to add keys).";

            ModelStatusText.Text = $"Model: {AppSession.LlmService.CurrentModel}";
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new ChatPage(AppSession.LlmService));
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new SettingsPage());
        }

        private void OpenSetup_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new StartupPage());
        }
    }
}
