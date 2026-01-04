using System;
using System.Diagnostics;
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
        private readonly PreferencesService _prefsService = new PreferencesService();
        private UserPreferences _prefs = new UserPreferences();
        private int _wizardStep = 1;

        public StartupPage()
        {
            InitializeComponent();
            Loaded += StartupPage_Loaded;
        }

        private Window OwnerWindow => Window.GetWindow(this) ?? Application.Current?.MainWindow!;

        private async void StartupPage_Loaded(object sender, RoutedEventArgs e)
        {
            _prefs = _prefsService.Load();
            try
            {
                var mode = ThemeManager.Parse(_prefs.Theme);
                ThemeSelector.SelectedIndex = mode == ThemeMode.Dark ? 1 : mode == ThemeMode.Light ? 2 : 0;
                NotifyToggle.IsChecked = _prefs.NotificationsEnabled;
            }
            catch
            {
            }

            await RefreshReadinessAsync();
        }

        private async Task RefreshReadinessAsync()
        {
            var hasOpenAi = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            var hasClaude = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));
            var configOk = hasOpenAi || hasClaude;

            StatusConfig.Text = configOk ? "Config OK" : "Config Missing";
            StatusConfig.Style = (Style)FindResource(configOk ? "Text.Success" : "Text.Warning");

            var defenderAvailable = _defender.IsAvailable();
            DefenderText.Text = defenderAvailable ? "Defender: Available" : "Defender: Not Found";
            StatusDefender.Text = defenderAvailable ? "Defender OK" : "Defender Missing";
            StatusDefender.Style = (Style)FindResource(defenderAvailable ? "Text.Success" : "Text.Warning");

            var fw = await _firewall.GetStatusAsync();
            var fwOn = fw.IndexOf(@"State\s*ON", StringComparison.OrdinalIgnoreCase) >= 0 ||
                       fw.IndexOf("ON", StringComparison.OrdinalIgnoreCase) >= 0;
            FirewallText.Text = string.IsNullOrWhiteSpace(fw) ? "Firewall: Unknown" : "Firewall: Checked";
            StatusFirewall.Text = fwOn ? "Firewall ON" : "Firewall Unknown";
            StatusFirewall.Style = (Style)FindResource(fwOn ? "Text.Success" : "Text.Warning");

            var ready = configOk && defenderAvailable;
            StatusReady.Text = ready ? "Ready" : "Not Ready";
            StatusReady.Style = (Style)FindResource(ready ? "Text.Success" : "Text.Error");
        }

        private async void HealthCheck_Click(object sender, RoutedEventArgs e)
        {
            await RefreshReadinessAsync();
            MessageBox.Show(OwnerWindow, "Health check completed.", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var readme = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");
                Process.Start(new ProcessStartInfo
                {
                    FileName = System.IO.File.Exists(readme) ? readme : "https://github.com/",
                    UseShellExecute = true
                });
            }
            catch
            {
            }
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
                MessageBox.Show(OwnerWindow, "API keys saved to your user environment.", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(OwnerWindow, $"Failed to save keys: {ex.Message}", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyPrefs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _prefs.Theme = GetSelectedTheme().ToString();
                _prefs.NotificationsEnabled = NotifyToggle.IsChecked == true;
                _prefsService.Save(_prefs);
                ThemeManager.ApplyTheme(ThemeManager.Parse(_prefs.Theme));
                MessageBox.Show(OwnerWindow, "Preferences saved.", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(OwnerWindow, $"Failed to save preferences: {ex.Message}", "Smartitecture", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ThemeMode GetSelectedTheme()
        {
            return ThemeSelector.SelectedIndex == 1 ? ThemeMode.Dark :
                   ThemeSelector.SelectedIndex == 2 ? ThemeMode.Light :
                   ThemeMode.System;
        }

        private void StartWizard_Click(object sender, RoutedEventArgs e)
        {
            _wizardStep = 1;
            WizardOverlay.Visibility = Visibility.Visible;
            UpdateWizardStep();
        }

        private void LaunchDashboard_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new DashboardPage());
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new SmartitectureSimple.SettingsWindow { Owner = OwnerWindow };
                win.ShowDialog();
            }
            catch
            {
            }
        }

        private void BackToWelcome_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new WelcomePage());
        }

        private void WizardCancel_Click(object sender, RoutedEventArgs e)
        {
            WizardOverlay.Visibility = Visibility.Collapsed;
        }

        private void WizardBack_Click(object sender, RoutedEventArgs e)
        {
            if (_wizardStep <= 1) return;
            _wizardStep--;
            UpdateWizardStep();
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
            WizardContent.Children.Clear();

            var panel = new StackPanel();
            switch (_wizardStep)
            {
                case 1:
                    WizardStepText.Text = "Step 1 of 4 - API Keys";
                    panel.Children.Add(new TextBlock { Text = "Optionally add your API keys to enable AI features.", Style = (Style)FindResource("Text.Body") });
                    panel.Children.Add(new TextBlock { Text = "OpenAI API Key", Style = (Style)FindResource("Text.Caption"), Margin = new Thickness(0, 10, 0, 0) });
                    panel.Children.Add(new PasswordBox { Name = "WizardOpenAi", Margin = new Thickness(0, 4, 0, 0) });
                    panel.Children.Add(new TextBlock { Text = "Anthropic (Claude) API Key", Style = (Style)FindResource("Text.Caption"), Margin = new Thickness(0, 10, 0, 0) });
                    panel.Children.Add(new PasswordBox { Name = "WizardClaude", Margin = new Thickness(0, 4, 0, 0) });
                    WizardNext.Click -= WizardNext_Click;
                    WizardNext.Click -= SaveWizardKeysThenNext;
                    WizardNext.Click += SaveWizardKeysThenNext;
                    break;

                case 2:
                    WizardStepText.Text = "Step 2 of 4 - Health Check";
                    panel.Children.Add(new TextBlock { Text = "Run a quick readiness check.", Style = (Style)FindResource("Text.Body") });
                    var run = new Button
                    {
                        Content = "Run Health Check",
                        Margin = new Thickness(0, 10, 0, 0),
                        Style = (Style)FindResource("Button.Glass")
                    };
                    run.Click += async (_, __) => { await RefreshReadinessAsync(); };
                    panel.Children.Add(run);
                    ResetNextHandlers();
                    break;

                case 3:
                    WizardStepText.Text = "Step 3 of 4 - Preferences";
                    panel.Children.Add(new TextBlock { Text = "Choose your theme and notification preference.", Style = (Style)FindResource("Text.Body") });

                    var themeRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                    themeRow.Children.Add(new TextBlock { Text = "Theme:", VerticalAlignment = VerticalAlignment.Center, Style = (Style)FindResource("Text.Caption") });
                    var themeBox = new ComboBox { Name = "WizardTheme", Width = 160, Margin = new Thickness(8, 0, 0, 0) };
                    themeBox.Items.Add(new ComboBoxItem { Content = "System" });
                    themeBox.Items.Add(new ComboBoxItem { Content = "Dark" });
                    themeBox.Items.Add(new ComboBoxItem { Content = "Light" });
                    themeBox.SelectedIndex = ThemeManager.Parse(_prefs.Theme) == ThemeMode.Dark ? 1 : ThemeManager.Parse(_prefs.Theme) == ThemeMode.Light ? 2 : 0;
                    themeRow.Children.Add(themeBox);

                    var notify = new CheckBox { Name = "WizardNotify", Content = "Enable Notifications", Margin = new Thickness(0, 10, 0, 0), IsChecked = _prefs.NotificationsEnabled };
                    panel.Children.Add(themeRow);
                    panel.Children.Add(notify);

                    WizardNext.Click -= WizardNext_Click;
                    WizardNext.Click -= SaveWizardPrefsThenNext;
                    WizardNext.Click += SaveWizardPrefsThenNext;
                    break;

                case 4:
                    WizardStepText.Text = "Step 4 of 4 - Finish";
                    panel.Children.Add(new TextBlock { Text = "All set! You can now launch the dashboard.", Style = (Style)FindResource("Text.Body") });
                    var launch = new Button
                    {
                        Content = "Launch Dashboard \u2192",
                        Margin = new Thickness(0, 10, 0, 0),
                        Style = (Style)FindResource("Button.Primary")
                    };
                    launch.Click += LaunchDashboard_Click;
                    panel.Children.Add(launch);
                    ResetNextHandlers();
                    break;
            }

            WizardContent.Children.Add(panel);
            WizardBack.IsEnabled = _wizardStep > 1;
            WizardNext.Content = _wizardStep == 4 ? "Done" : "Next \u2192";
        }

        private void ResetNextHandlers()
        {
            WizardNext.Click -= SaveWizardKeysThenNext;
            WizardNext.Click -= SaveWizardPrefsThenNext;
            WizardNext.Click -= WizardNext_Click;
            WizardNext.Click += WizardNext_Click;
        }

        private async void SaveWizardKeysThenNext(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WizardContent.Children.Count == 0) return;
                var sp = WizardContent.Children[0] as StackPanel;
                if (sp == null) return;

                PasswordBox? openAi = null;
                PasswordBox? claude = null;
                foreach (var child in sp.Children)
                {
                    if (child is PasswordBox pb && pb.Name == "WizardOpenAi") openAi = pb;
                    if (child is PasswordBox pb2 && pb2.Name == "WizardClaude") claude = pb2;
                }

                var openai = openAi?.Password?.Trim();
                var claudeKey = claude?.Password?.Trim();
                if (!string.IsNullOrWhiteSpace(openai)) Environment.SetEnvironmentVariable("OPENAI_API_KEY", openai, EnvironmentVariableTarget.User);
                if (!string.IsNullOrWhiteSpace(claudeKey)) Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", claudeKey, EnvironmentVariableTarget.User);
                await RefreshReadinessAsync();
            }
            catch
            {
            }
            finally
            {
                ResetNextHandlers();
                WizardNext_Click(sender, e);
            }
        }

        private void SaveWizardPrefsThenNext(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WizardContent.Children.Count == 0) return;
                var sp = WizardContent.Children[0] as StackPanel;
                if (sp == null) return;

                ComboBox? themeBox = null;
                CheckBox? notify = null;
                foreach (var child in sp.Children)
                {
                    if (child is StackPanel row)
                    {
                        foreach (var item in row.Children)
                        {
                            if (item is ComboBox cb && cb.Name == "WizardTheme") themeBox = cb;
                        }
                    }
                    if (child is CheckBox c && c.Name == "WizardNotify") notify = c;
                }

                var sel = themeBox?.SelectedIndex ?? 0;
                _prefs.Theme = sel == 1 ? ThemeMode.Dark.ToString() : sel == 2 ? ThemeMode.Light.ToString() : ThemeMode.System.ToString();
                _prefs.NotificationsEnabled = notify?.IsChecked == true;
                _prefsService.Save(_prefs);
                ThemeManager.ApplyTheme(ThemeManager.Parse(_prefs.Theme));
            }
            catch
            {
            }
            finally
            {
                ResetNextHandlers();
                WizardNext_Click(sender, e);
            }
        }
    }
}
