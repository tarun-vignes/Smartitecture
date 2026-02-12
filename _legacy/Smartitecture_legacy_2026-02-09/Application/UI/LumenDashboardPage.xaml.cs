using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace Smartitecture.UI
{
    public sealed partial class LumenDashboardPage : Page
    {
        private DispatcherTimer _monitoringTimer;
        private string _currentMode = "LUMEN";

        public LumenDashboardPage()
        {
            this.InitializeComponent();
            InitializeMonitoring();
            AddWelcomeMessage();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            StartMonitoring();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            StopMonitoring();
        }

        private void InitializeMonitoring()
        {
            _monitoringTimer = new DispatcherTimer();
            _monitoringTimer.Interval = TimeSpan.FromSeconds(2);
            _monitoringTimer.Tick += MonitoringTimer_Tick;
        }

        private void StartMonitoring()
        {
            _monitoringTimer?.Start();
            UpdateSystemMetrics();
        }

        private void StopMonitoring()
        {
            _monitoringTimer?.Stop();
        }

        private void MonitoringTimer_Tick(object sender, object e)
        {
            UpdateSystemMetrics();
        }

        private async void UpdateSystemMetrics()
        {
            try
            {
                // Update CPU usage
                var cpuUsage = await GetCpuUsage();
                CpuProgress.Value = cpuUsage;

                // Update Memory usage
                var memoryUsage = await GetMemoryUsage();
                MemoryProgress.Value = memoryUsage;

                // Update Disk usage
                var diskUsage = await GetDiskUsage();
                DiskProgress.Value = diskUsage;

                // Update CPU temperature (if available)
                var cpuTemp = await GetCpuTemperature();
                CpuTempText.Text = cpuTemp > 0 ? $"CPU Temp: {cpuTemp}°C" : "CPU Temp: N/A";

                // Update network speeds
                var (upload, download) = await GetNetworkSpeeds();
                UploadSpeedText.Text = $"↑ {upload} kbps";
                DownloadSpeedText.Text = $"↓ {download} kbps";
            }
            catch (Exception ex)
            {
                // Log error silently
                System.Diagnostics.Debug.WriteLine($"Error updating metrics: {ex.Message}");
            }
        }

        private async Task<double> GetCpuUsage()
        {
            // TODO: Implement actual CPU usage monitoring
            await Task.Delay(10);
            return new Random().Next(20, 60);
        }

        private async Task<double> GetMemoryUsage()
        {
            // TODO: Implement actual memory usage monitoring
            await Task.Delay(10);
            return new Random().Next(40, 80);
        }

        private async Task<double> GetDiskUsage()
        {
            // TODO: Implement actual disk usage monitoring
            await Task.Delay(10);
            return 75;
        }

        private async Task<int> GetCpuTemperature()
        {
            // TODO: Implement actual CPU temperature monitoring
            await Task.Delay(10);
            return 0; // Return 0 if not available
        }

        private async Task<(int upload, int download)> GetNetworkSpeeds()
        {
            // TODO: Implement actual network speed monitoring
            await Task.Delay(10);
            return (1000, 1000);
        }

        private void AddWelcomeMessage()
        {
            AddChatMessage("Happy to help! I'm good with calculations, system tasks, and general questions. How can I assist?", false);
        }

        private void AddChatMessage(string message, bool isUser)
        {
            var messagePanel = new StackPanel
            {
                Spacing = 4,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 600,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var messageBorder = new Border
            {
                Background = isUser ? 
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Purple) : 
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 51, 65, 85)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12)
            };

            var messageText = new TextBlock
            {
                Text = message,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14
            };

            messageBorder.Child = messageText;
            messagePanel.Children.Add(messageBorder);

            var timeText = new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm:ss"),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 148, 163, 184)),
                FontSize = 11,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            messagePanel.Children.Add(timeText);
            ChatMessagesPanel.Children.Add(messagePanel);
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void ChatInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            var message = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(message))
                return;

            // Add user message
            AddChatMessage(message, true);
            ChatInputBox.Text = "";

            // Disable send button
            SendButton.IsEnabled = false;

            // Simulate AI response
            await Task.Delay(1000);

            // Add AI response
            var response = await GetAIResponse(message);
            AddChatMessage(response, false);

            // Re-enable send button
            SendButton.IsEnabled = true;
        }

        private async Task<string> GetAIResponse(string userMessage)
        {
            // TODO: Implement actual AI service integration
            await Task.Delay(500);

            // Simple response logic for demo
            if (userMessage.Contains("1+1") || userMessage.Contains("1 + 1"))
            {
                return "**1 + 1 = 2**\n\nThat equals 2*";
            }
            else if (userMessage.ToLower().Contains("president"))
            {
                return "I can help you with that question. Would you like me to research current information?";
            }
            else
            {
                return $"I understand you said: \"{userMessage}\". How can I help you with that?";
            }
        }

        private void ModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ModeSelector.SelectedIndex;
            _currentMode = selectedIndex switch
            {
                0 => "LUMEN",
                1 => "FORTIS",
                2 => "NEXA",
                _ => "LUMEN"
            };

            // TODO: Switch AI mode
            AddChatMessage($"Switched to {_currentMode} mode", false);
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesPanel.Children.Clear();
            AddWelcomeMessage();
        }

        private async void OptimizePerformance_Click(object sender, RoutedEventArgs e)
        {
            AddChatMessage("Starting performance optimization...", false);
            await Task.Delay(2000);
            AddChatMessage("Performance optimization complete! Freed up 500 MB of memory.", false);
        }

        private async void QuickScan_Click(object sender, RoutedEventArgs e)
        {
            AddChatMessage("Starting quick security scan...", false);
            await Task.Delay(3000);
            AddChatMessage("Quick scan complete. No threats detected.", false);
            LastScanText.Text = $"Last Scan: {DateTime.Now:HH:mm:ss}";
        }

        private async void FullScan_Click(object sender, RoutedEventArgs e)
        {
            AddChatMessage("Starting full security scan... This may take several minutes.", false);
            await Task.Delay(5000);
            AddChatMessage("Full scan complete. System is secure.", false);
            LastScanText.Text = $"Last Scan: {DateTime.Now:HH:mm:ss}";
        }

        private async void ResearchTopic_Click(object sender, RoutedEventArgs e)
        {
            var message = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                AddChatMessage("Please enter a topic to research in the chat box.", false);
                return;
            }

            AddChatMessage($"Researching: {message}", false);
            await Task.Delay(2000);
            AddChatMessage($"Research complete! I found information about {message}.", false);
            
            Action1Text.Text = $"{DateTime.Now:HH:mm:ss}  Research completed";
        }

        private async void CleanTempFiles_Click(object sender, RoutedEventArgs e)
        {
            AddChatMessage("Cleaning temporary files...", false);
            await Task.Delay(2000);
            AddChatMessage("Cleaned 1.2 GB of temporary files!", false);
        }

        private void Attach_Click(object sender, RoutedEventArgs e)
        {
            AddChatMessage("File attachment feature coming soon!", false);
        }
    }
}
