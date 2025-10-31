using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Smartitecture.Services.Connectors;
using Smartitecture.Services;

namespace Smartitecture
{
    public partial class StartupWindow : Window
    {
        private readonly WindowsDefenderConnector _defender = new WindowsDefenderConnector();
        private readonly FirewallConnector _firewall = new FirewallConnector();
        private readonly PreferencesService _prefsService = new PreferencesService();
        private UserPreferences _prefs = new UserPreferences();
        private int _wizardStep = 1;

        public StartupWindow()
        {
            InitializeComponent();
            Loaded += StartupWindow_Loaded;
        }

        private async void StartupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load preferences and bind simple controls
            _prefs = _prefsService.Load();
            try
            {
                if (_prefs.Theme.Equals("Light", StringComparison.OrdinalIgnoreCase))
                {
                    ThemeSelector.SelectedIndex = 1;
                }
                else
                {
                    ThemeSelector.SelectedIndex = 0;
                }
                NotifyToggle.IsChecked = _prefs.NotificationsEnabled;
            }
            catch { }
            await RefreshReadinessAsync();
        }

        private async Task RefreshReadinessAsync()
        {
            var hasOpenAi = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            var hasClaude = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));
            var configOk = hasOpenAi || hasClaude;

            StatusConfig.Text = configOk ? "Config OK" : "Config Missing";
            StatusConfig.Style = (Style)FindResource(configOk ? "Text.Success" : "Text.Warning");

            bool defenderAvailable = _defender.IsAvailable();
            DefenderText.Text = defenderAvailable ? "Defender: Available" : "Defender: Not Found";
            StatusDefender.Text = defenderAvailable ? "Defender OK" : "Defender Missing";
            StatusDefender.Style = (Style)FindResource(defenderAvailable ? "Text.Success" : "Text.Warning");

            string fw = await _firewall.GetStatusAsync();
            bool fwOn = fw.IndexOf(@"State\s*ON", StringComparison.OrdinalIgnoreCase) >= 0 || fw.IndexOf("ON", StringComparison.OrdinalIgnoreCase) >= 0;
            FirewallText.Text = string.IsNullOrWhiteSpace(fw) ? "Firewall: Unknown" : "Firewall: Checked";
            StatusFirewall.Text = fwOn ? "Firewall ON" : "Firewall Unknown";
            StatusFirewall.Style = (Style)FindResource(fwOn ? "Text.Success" : "Text.Warning");

            bool ready = configOk && defenderAvailable; // minimal readiness
            StatusReady.Text = ready ? "Ready" : "Not Ready";
            StatusReady.Style = (Style)FindResource(ready ? "Text.Success" : "Text.Error");
        }

        private async void HealthCheck_Click(object sender, RoutedEventArgs e)
        {
            await RefreshReadinessAsync();
            MessageBox.Show(this, "Health check completed.", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var readme = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");
                if (System.IO.File.Exists(readme))
                    Process.Start(new ProcessStartInfo { FileName = readme, UseShellExecute = true });
                else
                    Process.Start(new ProcessStartInfo { FileName = "https://github.com/", UseShellExecute = true });
            }
            catch { }
        }

        private async void SaveKeys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openai = OpenAiKeyBox.Password?.Trim();
                var claude = ClaudeKeyBox.Password?.Trim();
                if (!string.IsNullOrWhiteSpace(openai)) Environment.SetEnvironmentVariable("OPENAI_API_KEY", openai, EnvironmentVariableTarget.User);
                if (!string.IsNullOrWhiteSpace(claude)) Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", claude, EnvironmentVariableTarget.User);
                await RefreshReadinessAsync();
                MessageBox.Show(this, "API keys saved to your user environment.", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to save keys: {ex.Message}", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyPrefs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _prefs.Theme = (ThemeSelector.SelectedIndex == 1) ? "Light" : "Dark";
                _prefs.NotificationsEnabled = NotifyToggle.IsChecked == true;
                _prefsService.Save(_prefs);
                MessageBox.Show(this, "Preferences saved.", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to save preferences: {ex.Message}", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartWizard_Click(object sender, RoutedEventArgs e)
        {
            _wizardStep = 1;
            WizardOverlay.Visibility = Visibility.Visible;
            UpdateWizardStep();
        }

        private void LaunchDashboard_Click(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Close();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new SmartitectureSimple.SettingsWindow();
                win.Owner = this;
                win.ShowDialog();
            }
            catch { }
        }

        private void WizardCancel_Click(object sender, RoutedEventArgs e)
        {
            WizardOverlay.Visibility = Visibility.Collapsed;
        }

        private void WizardBack_Click(object sender, RoutedEventArgs e)
        {
            if (_wizardStep > 1)
            {
                _wizardStep--;
                UpdateWizardStep();
            }
        }

        private async void WizardNext_Click(object sender, RoutedEventArgs e)
        {
            if (_wizardStep < 4)
            {
                _wizardStep++;
                UpdateWizardStep();
                if (_wizardStep == 2)
                {
                    await RefreshReadinessAsync();
                }
            }
            else
            {
                WizardOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateWizardStep()
        {
            try
            {
                WizardContent.Children.Clear();
                var panel = new System.Windows.Controls.StackPanel();
                panel.Margin = new Thickness(0, 4, 0, 0);
                switch (_wizardStep)
                {
                    case 1:
                        WizardStepText.Text = "Step 1 of 4 — API Keys";
                        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Enter your API keys to enable AI features.", Style = (Style)FindResource("Text.Body") });
                        var openLabel = new System.Windows.Controls.TextBlock { Text = "OpenAI API Key", Style = (Style)FindResource("Text.Caption"), Margin = new Thickness(0,8,0,0) };
                        var openBox = new System.Windows.Controls.PasswordBox { Name = "WizardOpenAI", Margin = new Thickness(0,4,0,0) };
                        var claudeLabel = new System.Windows.Controls.TextBlock { Text = "Anthropic (Claude) API Key", Style = (Style)FindResource("Text.Caption"), Margin = new Thickness(0,8,0,0) };
                        var claudeBox = new System.Windows.Controls.PasswordBox { Name = "WizardClaude", Margin = new Thickness(0,4,0,0) };
                        panel.Children.Add(openLabel);
                        panel.Children.Add(openBox);
                        panel.Children.Add(claudeLabel);
                        panel.Children.Add(claudeBox);
                        // wire save on next
                        WizardNext.Click -= SaveWizardKeysThenNext;
                        WizardNext.Click -= WizardNext_Click;
                        WizardNext.Click += SaveWizardKeysThenNext;
                        break;
                    case 2:
                        WizardStepText.Text = "Step 2 of 4 — Permissions";
                        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = "We will verify Windows Defender and Firewall.", Style = (Style)FindResource("Text.Body") });
                        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = DefenderText.Text, Style = (Style)FindResource("Text.Caption"), Margin = new Thickness(0,8,0,0) });
                        panel.Children.Add(new System.Windows.Controls.TextBlock { Text = FirewallText.Text, Style = (Style)FindResource("Text.Caption"), Margin = new Thickness(0,4,0,0) });
                        ResetNextHandlers();
                        break;
                    case 3:
                        WizardStepText.Text = "Step 3 of 4 — Preferences";
                        var desc = new System.Windows.Controls.TextBlock { Text = "Choose your theme and notification preferences.", Style = (Style)FindResource("Text.Body") };
                        panel.Children.Add(desc);
                        var themeRow = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new Thickness(0,8,0,0) };
                        themeRow.Children.Add(new System.Windows.Controls.TextBlock { Text = "Theme:", VerticalAlignment = VerticalAlignment.Center, Style = (Style)FindResource("Text.Caption") });
                        var themeBox = new System.Windows.Controls.ComboBox { Name = "WizardTheme", Width = 150, Margin = new Thickness(8,0,0,0) };
                        themeBox.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Dark" });
                        themeBox.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Light" });
                        themeBox.SelectedIndex = _prefs.Theme.Equals("Light", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                        themeRow.Children.Add(themeBox);
                        var notify = new System.Windows.Controls.CheckBox { Name = "WizardNotify", Content = "Enable Notifications", Margin = new Thickness(0,8,0,0), IsChecked = _prefs.NotificationsEnabled };
                        panel.Children.Add(themeRow);
                        panel.Children.Add(notify);
                        // Save on Next
                        WizardNext.Click -= SaveWizardPrefsThenNext;
                        WizardNext.Click -= WizardNext_Click;
                        WizardNext.Click += SaveWizardPrefsThenNext;
                        break;
                    case 4:
                        WizardStepText.Text = "Step 4 of 4 — Finish";
                        var doneText = new System.Windows.Controls.TextBlock { Text = "All set! You can now launch the dashboard.", Style = (Style)FindResource("Text.Body") };
                        var launch = new System.Windows.Controls.Button { Content = "Launch Dashboard >", Margin = new Thickness(0,10,0,0), Style = (Style)FindResource("Button.Primary") };
                        launch.Click += (s, e) => { LaunchDashboard_Click(s, e); };
                        panel.Children.Add(doneText);
                        panel.Children.Add(launch);
                        ResetNextHandlers();
                        WizardNext.Content = "Done";
                        break;
                }
                WizardContent.Children.Add(panel);
                WizardBack.IsEnabled = _wizardStep > 1;
                if (_wizardStep < 4 && WizardNext.Content?.ToString() == "Done") WizardNext.Content = "Next >";
            }
            catch { }
        }

        private void ResetNextHandlers()
        {
            WizardNext.Click -= SaveWizardKeysThenNext;
            WizardNext.Click -= SaveWizardPrefsThenNext;
            WizardNext.Click -= WizardNext_Click;
            WizardNext.Click += WizardNext_Click;
        }

        private void SaveWizardKeysThenNext(object? sender, RoutedEventArgs e)
        {
            try
            {
                // find children by index (we know order) and save
                if (WizardContent.Children.Count > 0 && WizardContent.Children[0] is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 4)
                {
                    var open = sp.Children[1] as System.Windows.Controls.PasswordBox;
                    var claude = sp.Children[3] as System.Windows.Controls.PasswordBox;
                    var openai = open?.Password?.Trim();
                    var claudeKey = claude?.Password?.Trim();
                    if (!string.IsNullOrWhiteSpace(openai)) Environment.SetEnvironmentVariable("OPENAI_API_KEY", openai, EnvironmentVariableTarget.User);
                    if (!string.IsNullOrWhiteSpace(claudeKey)) Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", claudeKey, EnvironmentVariableTarget.User);
                }
            }
            catch { }
            finally
            {
                ResetNextHandlers();
                WizardNext_Click(sender!, e);
            }
        }

        private void SaveWizardPrefsThenNext(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (WizardContent.Children.Count > 0 && WizardContent.Children[0] is System.Windows.Controls.StackPanel sp && sp.Children.Count >= 3)
                {
                    var themeRow = sp.Children[1] as System.Windows.Controls.StackPanel;
                    var themeBox = themeRow?.Children.Count >= 2 ? themeRow.Children[1] as System.Windows.Controls.ComboBox : null;
                    var notify = sp.Children[2] as System.Windows.Controls.CheckBox;
                    _prefs.Theme = (themeBox?.SelectedIndex == 1) ? "Light" : "Dark";
                    _prefs.NotificationsEnabled = notify?.IsChecked == true;
                    _prefsService.Save(_prefs);
                }
            }
            catch { }
            finally
            {
                ResetNextHandlers();
                WizardNext_Click(sender!, e);
            }
        }
    }
}
