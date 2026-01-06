using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Smartitecture.Services;
using Smartitecture.Services.Connectors;

namespace Smartitecture.UI.Pages
{
    public partial class StartupPage : Page
    {
        private readonly WindowsDefenderConnector _defender = new WindowsDefenderConnector();
        private readonly FirewallConnector _firewall = new FirewallConnector();
        private readonly ConfigurationService _configService = new ConfigurationService();

        public StartupPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await RefreshReadinessAsync();
        }

        private async Task RefreshReadinessAsync()
        {
            var openAi = _configService.IsOpenAIConfigured();
            var claude = _configService.IsClaudeConfigured();
            var configOk = openAi || claude;

            ConfigStatusText.Text = configOk
                ? $"Configured ({(openAi ? "OpenAI" : string.Empty)}{(openAi && claude ? ", " : string.Empty)}{(claude ? "Claude" : string.Empty)})"
                : "Not configured";
            ConfigStatusText.Style = (Style)FindResource(configOk ? "Text.Success" : "Text.Warning");

            var defenderAvailable = _defender.IsAvailable();
            DefenderStatusText.Text = defenderAvailable ? "Available" : "Not detected";
            DefenderStatusText.Style = (Style)FindResource(defenderAvailable ? "Text.Success" : "Text.Warning");

            var firewallRaw = await _firewall.GetStatusAsync();
            var firewallOk = firewallRaw.IndexOf(@"State\s*ON", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             firewallRaw.IndexOf("ON", StringComparison.OrdinalIgnoreCase) >= 0;
            FirewallStatusText.Text = string.IsNullOrWhiteSpace(firewallRaw) ? "Unknown" : (firewallOk ? "On" : "Unknown");
            FirewallStatusText.Style = (Style)FindResource(firewallOk ? "Text.Success" : "Text.Warning");

            var ready = configOk && defenderAvailable;
            OverallStatusText.Text = ready ? "Ready" : "Needs attention";
            OverallStatusText.Style = (Style)FindResource(ready ? "Text.Success" : "Text.Warning");
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await RefreshReadinessAsync();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new SettingsPage());
        }

        private void GoDashboard_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new DashboardPage());
        }

        private void GoChat_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new ChatPage(AppSession.LlmService));
        }
    }
}
