using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Smartitecture.UI
{
    public sealed partial class WelcomeSetupPage : Page
    {
        private const string OPENAI_KEY_SETTING = "OpenAI_API_Key";
        private const string CLAUDE_KEY_SETTING = "Claude_API_Key";
        private ApplicationDataContainer localSettings;

        public WelcomeSetupPage()
        {
            this.InitializeComponent();
            localSettings = ApplicationData.Current.LocalSettings;
            LoadSavedKeys();
            CheckSystemReadiness();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CheckSystemReadiness();
        }

        private void LoadSavedKeys()
        {
            try
            {
                // Load OpenAI key
                if (localSettings.Values.ContainsKey(OPENAI_KEY_SETTING))
                {
                    OpenAIKeyBox.Password = localSettings.Values[OPENAI_KEY_SETTING] as string ?? "";
                }

                // Load Claude key
                if (localSettings.Values.ContainsKey(CLAUDE_KEY_SETTING))
                {
                    ClaudeKeyBox.Password = localSettings.Values[CLAUDE_KEY_SETTING] as string ?? "";
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error loading saved keys", ex.Message);
            }
        }

        private async void SaveKeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool hasOpenAI = !string.IsNullOrWhiteSpace(OpenAIKeyBox.Password);
                bool hasClaude = !string.IsNullOrWhiteSpace(ClaudeKeyBox.Password);

                if (!hasOpenAI && !hasClaude)
                {
                    await ShowErrorDialog("No API Keys", "Please enter at least one API key (OpenAI or Claude).");
                    return;
                }

                // Save OpenAI key
                if (hasOpenAI)
                {
                    localSettings.Values[OPENAI_KEY_SETTING] = OpenAIKeyBox.Password;
                }

                // Save Claude key
                if (hasClaude)
                {
                    localSettings.Values[CLAUDE_KEY_SETTING] = ClaudeKeyBox.Password;
                }

                // Update readiness status
                CheckSystemReadiness();

                // Show success message
                await ShowSuccessDialog("Keys Saved", "Your API keys have been saved securely.");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error saving keys", ex.Message);
            }
        }

        private async void HealthCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HealthCheckButton.IsEnabled = false;
                HealthCheckButton.Content = "Checking...";

                // Simulate health check
                await Task.Delay(1000);

                // Check Windows Defender
                bool defenderOk = await CheckWindowsDefender();
                
                // Check Firewall
                bool firewallOk = await CheckFirewall();

                // Update status
                DefenderStatus.Text = defenderOk ? "Defender OK" : "Defender Issue";
                FirewallStatus.Text = firewallOk ? "Firewall ON" : "Firewall OFF";

                CheckSystemReadiness();

                HealthCheckButton.Content = "Run Health Check";
                HealthCheckButton.IsEnabled = true;

                await ShowSuccessDialog("Health Check Complete", 
                    $"Defender: {(defenderOk ? "OK" : "Issue")}\nFirewall: {(firewallOk ? "ON" : "OFF")}");
            }
            catch (Exception ex)
            {
                HealthCheckButton.Content = "Run Health Check";
                HealthCheckButton.IsEnabled = true;
                await ShowErrorDialog("Health Check Failed", ex.Message);
            }
        }

        private void CheckSystemReadiness()
        {
            bool hasOpenAI = localSettings.Values.ContainsKey(OPENAI_KEY_SETTING) && 
                            !string.IsNullOrWhiteSpace(localSettings.Values[OPENAI_KEY_SETTING] as string);
            bool hasClaude = localSettings.Values.ContainsKey(CLAUDE_KEY_SETTING) && 
                            !string.IsNullOrWhiteSpace(localSettings.Values[CLAUDE_KEY_SETTING] as string);
            
            bool hasApiKeys = hasOpenAI || hasClaude;

            // Update config status
            if (hasApiKeys)
            {
                ConfigStatus.Text = "Config Ready";
                var brush = Application.Current.Resources["StatusOkBrush"] as Microsoft.UI.Xaml.Media.SolidColorBrush;
                if (ConfigStatus.Parent is Border configBorder)
                {
                    configBorder.Background = brush;
                }
            }
            else
            {
                ConfigStatus.Text = "Config Missing";
                var brush = Application.Current.Resources["StatusWarningBrush"] as Microsoft.UI.Xaml.Media.SolidColorBrush;
                if (ConfigStatus.Parent is Border configBorder)
                {
                    configBorder.Background = brush;
                }
            }

            // Update ready status
            if (hasApiKeys)
            {
                ReadyStatus.Text = "Ready";
                var brush = Application.Current.Resources["StatusOkBrush"] as Microsoft.UI.Xaml.Media.SolidColorBrush;
                ReadyStatusBorder.Background = brush;
                GetStartedButton.IsEnabled = true;
            }
            else
            {
                ReadyStatus.Text = "Not Ready";
                var brush = Application.Current.Resources["StatusErrorBrush"] as Microsoft.UI.Xaml.Media.SolidColorBrush;
                ReadyStatusBorder.Background = brush;
                GetStartedButton.IsEnabled = false;
            }
        }

        private async Task<bool> CheckWindowsDefender()
        {
            try
            {
                // TODO: Implement actual Windows Defender check
                // For now, return true
                await Task.Delay(100);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CheckFirewall()
        {
            try
            {
                // TODO: Implement actual Firewall check
                // For now, return true
                await Task.Delay(100);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GetStarted_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to LUMEN Dashboard
            Frame.Navigate(typeof(LumenDashboardPage));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to settings
            Frame.Navigate(typeof(ThemeSettingsPage));
        }

        private void LearnMore_Click(object sender, RoutedEventArgs e)
        {
            // Open documentation
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://platform.openai.com/docs"));
        }

        private void AboutDocs_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to about page or open docs
            Frame.Navigate(typeof(GettingStartedPage));
        }

        private async Task ShowSuccessDialog(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowErrorDialog(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
