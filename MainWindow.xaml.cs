using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Net.NetworkInformation;
using Smartitecture.Services;
using Smartitecture.Services.Core;
using Smartitecture.Services.Modes;
using Smartitecture.Services.Connectors;
using Smartitecture.Services.Security;
using Smartitecture.Services.Hardware;

namespace Smartitecture
{
    public partial class MainWindow : Window
    {
        private readonly ILLMService _llmService;
        private readonly string _conversationId;
        private readonly DispatcherTimer _typingTimer;
        private readonly DispatcherTimer _dashboardTimer;
        private bool _isProcessing = false;
        private Border _currentStreamingMessage;
        private TextBlock _currentStreamingTextBlock;
        private string _currentMode = "LUMEN";
        private readonly AIModeRouter _modeRouter;
        private readonly PerformanceConnector _perf = new PerformanceConnector();
        private readonly FirewallConnector _fw = new FirewallConnector();
        private readonly WindowsDefenderConnector _defender = new WindowsDefenderConnector();
        private readonly NetworkMonitor _net = new NetworkMonitor();
        private readonly SensorMonitor _sensor = new SensorMonitor();
        private FileSystemWatcher _fsw;
        private bool _typingActive = false;
        private DateTime _lastNetSample = DateTime.MinValue;
        private long _lastBytesSent = 0;
        private long _lastBytesReceived = 0;
        private readonly List<string> _recentActions = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            _llmService = new MultiModelAIService();
            _llmService.ModelSwitched += OnModelSwitched;
            _conversationId = Guid.NewGuid().ToString();

            // Initialize AI Mode Router and register modes
            _modeRouter = new AIModeRouter();
            var research = new WebResearchEngine();
            var kb = new KnowledgeBaseService();
            _modeRouter.RegisterMode(ChatMode.Lumen, new LumenService(research, kb, _llmService));
            _modeRouter.RegisterMode(ChatMode.Fortis, new FortisService(_llmService));
            _modeRouter.RegisterMode(ChatMode.Nexa, new NexaService(_llmService));
            _modeRouter.ModeChanged += ModeRouter_ModeChanged;

            _typingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _typingTimer.Tick += TypingTimer_Tick;

            SetupPlaceholderText();
            UpdateModelStatus();
            InitializeDashboard();
            _dashboardTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _dashboardTimer.Tick += DashboardTimer_Tick;
            _dashboardTimer.Start();
        }

        private void OnModelSwitched(object sender, ModelSwitchedEventArgs e)
        {
            Dispatcher.Invoke(UpdateModelStatus);
        }

        private void UpdateModelStatus()
        {
            if (ModelStatusText != null)
            {
                var isAuto = (AutoDetectToggle?.IsChecked ?? false);
                ModelStatusText.Text = isAuto ? "(Auto)" : $"({_currentMode})";
            }
        }
        
        private async void ModeRouter_ModeChanged(object sender, ModeChangedEventArgs e)
        {
            _currentMode = e.NewMode.GetDisplayName();
            var best = ChooseBestModelForMode(_currentMode);
            if (!string.IsNullOrEmpty(best))
            {
                try { await _llmService.SwitchModelAsync(best); } catch { }
            }
            Dispatcher.Invoke(UpdateModelStatus);
            Dispatcher.Invoke(UpdateDashboardVisibility);
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var icon = _currentMode == "FORTIS" ? "🛡️" : _currentMode == "NEXA" ? "⚡" : "💡";
                    if (HeaderModeIcon != null) HeaderModeIcon.Text = icon;
                    if (ChatHeaderModeIcon != null) ChatHeaderModeIcon.Text = icon;
                    if (AssistantModeIcon != null) AssistantModeIcon.Text = icon;
                }
                catch { }
            });
        }

        private void SetupPlaceholderText()
        {
            var placeholderText = "Type your message or command here...";
            MessageInput.Text = placeholderText;
            MessageInput.Foreground = Brushes.Gray;

            MessageInput.GotFocus += (s, e) =>
            {
                if (MessageInput.Text == placeholderText)
                {
                    MessageInput.Text = string.Empty;
                    MessageInput.Foreground = Brushes.White;
                }
            };

            MessageInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(MessageInput.Text))
                {
                    MessageInput.Text = placeholderText;
                    MessageInput.Foreground = Brushes.Gray;
                }
            };
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            var placeholderText = "Type your message or command here...";
            if (_isProcessing || string.IsNullOrWhiteSpace(MessageInput.Text) || MessageInput.Text == placeholderText)
                return;

            var userMessage = MessageInput.Text.Trim();
            MessageInput.Text = string.Empty;
            MessageInput.Foreground = Brushes.White;
            _isProcessing = true;
            SendButton.IsEnabled = false;

            AddMessageToChat(userMessage, "user");
            ShowTypingIndicator();

            try
            {
                var isAuto = AutoDetectToggle?.IsChecked ?? false;
                if (isAuto)
                {
                    _modeRouter.AutoDetectionEnabled = true;
                    var ai = await _modeRouter.ProcessQueryAsync(userMessage, _conversationId);
                    _currentMode = ai.SourceMode ?? _currentMode;
                    UpdateModelStatus();
                    await StreamTextAsync(ai.Content ?? string.Empty);
                }
                else
                {
                    // Manual mode: route via AIModeRouter honoring ManualMode and mode-specific prompts
                    _modeRouter.AutoDetectionEnabled = false;
                    var ai = await _modeRouter.ProcessQueryAsync(userMessage, _conversationId);
                    _currentMode = ai.SourceMode ?? _currentMode;
                    UpdateModelStatus();
                    await StreamTextAsync(ai.Content ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"Error: {ex.Message}", "system");
            }
            finally
            {
                HideTypingIndicator();
                _isProcessing = false;
                SendButton.IsEnabled = true;
                MessageInput.Focus();
            }
        }

        private void OnTokenReceived(string token)
        {
            Dispatcher.Invoke(() =>
            {
                if (TypingIndicator != null && TypingIndicator.Visibility == Visibility.Visible)
                {
                    HideTypingIndicator();
                }
                if (_currentStreamingTextBlock == null)
                {
                    _currentStreamingTextBlock = new TextBlock
                    {
                        Text = string.Empty,
                        Foreground = Brushes.White,
                        TextWrapping = TextWrapping.Wrap
                    };
                    _currentStreamingMessage = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(8, 4, 8, 4),
                        Margin = new Thickness(20, 5, 20, 5),
                        Child = _currentStreamingTextBlock
                    };
                    ChatMessagesPanel.Children.Add(_currentStreamingMessage);
                    ChatScrollViewer.ScrollToEnd();
                }

                _currentStreamingTextBlock.Text += token;
                ChatScrollViewer.ScrollToEnd();
            });
        }

        private void InitializeDashboard()
        {
            try
            {
                var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (Directory.Exists(docs))
                {
                    _fsw = new FileSystemWatcher(docs)
                    {
                        IncludeSubdirectories = false,
                        EnableRaisingEvents = true
                    };
                    _fsw.Created += (s, e) => AppendFileActivity($"Created: {e.Name}");
                    _fsw.Changed += (s, e) => AppendFileActivity($"Changed: {e.Name}");
                    _fsw.Renamed += (s, e) => AppendFileActivity($"Renamed: {((RenamedEventArgs)e).OldName} -> {e.Name}");
                    _fsw.Deleted += (s, e) => AppendFileActivity($"Deleted: {e.Name}");
                }
                _ = _net.StartMonitoringAsync();
            }
            catch { }
        }

        private void DashboardTimer_Tick(object sender, EventArgs e)
        {
            _ = UpdateDashboardAsync();
        }

        private async Task UpdateDashboardAsync()
        {
            try
            {
                var cpu = await _perf.GetCpuUsageAsync();
                var mem = await _perf.GetMemoryUsageAsync();
                var disk = await _perf.GetDiskUsageAsync();
                // Disk space
                double diskPercent = 0; string diskLabel = "";
                try
                {
                    var sys = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
                    var di = new DriveInfo(sys);
                    if (di.IsReady)
                    {
                        diskPercent = (double)(di.TotalSize - di.AvailableFreeSpace) / di.TotalSize * 100.0;
                        diskLabel = $"{Bytes(di.TotalSize - di.AvailableFreeSpace)} used / {Bytes(di.TotalSize)}";
                    }
                }
                catch { }
                Dispatcher.Invoke(() =>
                {
                    if (CpuBar != null)
                    {
                        CpuBar.Value = Math.Max(0, Math.Min(100, cpu));
                        CpuBar.Foreground = new SolidColorBrush(cpu >= 85 ? Colors.OrangeRed : cpu >= 70 ? Colors.Goldenrod : Colors.LimeGreen);
                    }
                    if (MemBar != null)
                    {
                        MemBar.Value = Math.Max(0, Math.Min(100, mem));
                        MemBar.Foreground = new SolidColorBrush(mem >= 85 ? Colors.OrangeRed : mem >= 70 ? Colors.Goldenrod : Color.FromRgb(33,150,243));
                    }
                    if (DiskBar != null)
                    {
                        DiskBar.Value = Math.Max(0, Math.Min(100, disk));
                        DiskBar.Foreground = new SolidColorBrush(disk >= 90 ? Colors.OrangeRed : disk >= 75 ? Colors.Goldenrod : Colors.Yellow);
                    }
                    if (DiskUsageBar != null) DiskUsageBar.Value = Math.Max(0, Math.Min(100, diskPercent));
                    if (DiskUsageText != null) DiskUsageText.Text = diskLabel;
                    if (DiskUsageBar != null) DiskUsageBar.ToolTip = diskLabel;
                });

                var fwStatus = await _fw.GetStatusAsync();
                var defenderAvailable = _defender.IsAvailable();
                var cpuTemp = await _sensor.GetCpuTemperatureAsync();
                Dispatcher.Invoke(() =>
                {
                    if (FirewallStatusText != null) FirewallStatusText.Text = string.IsNullOrWhiteSpace(fwStatus) ? "Firewall: Unknown" : "Firewall: Checked";
                    if (FirewallDot != null) FirewallDot.Fill = new SolidColorBrush(Colors.LimeGreen);
                    if (DefenderStatusText != null) DefenderStatusText.Text = defenderAvailable ? "Defender: Available" : "Defender: Not Found";
                    if (DefenderDot != null) DefenderDot.Fill = defenderAvailable ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.OrangeRed);
                    if (CpuTempText != null) CpuTempText.Text = cpuTemp.HasValue ? $"{cpuTemp.Value:0} °C" : "N/A";
                });

                try
                {
                    var now = DateTime.UtcNow;
                    long sent = 0, recv = 0;
                    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (ni.OperationalStatus != OperationalStatus.Up) continue;
                        if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                        var stats = ni.GetIPv4Statistics();
                        sent += stats.BytesSent;
                        recv += stats.BytesReceived;
                    }
                    double upKbps = 0, downKbps = 0;
                    if (_lastNetSample != DateTime.MinValue)
                    {
                        var dt = (now - _lastNetSample).TotalSeconds;
                        if (dt > 0)
                        {
                            upKbps = ((_lastBytesSent > 0 ? sent - _lastBytesSent : 0) / 1024.0) / dt * 8.0;
                            downKbps = ((_lastBytesReceived > 0 ? recv - _lastBytesReceived : 0) / 1024.0) / dt * 8.0;
                        }
                    }
                    _lastNetSample = now;
                    _lastBytesSent = sent;
                    _lastBytesReceived = recv;

                Dispatcher.Invoke(() =>
                {
                    if (NetworkStatusText != null) NetworkStatusText.Text = NetworkInterface.GetIsNetworkAvailable() ? "Network: Connected" : "Network: Disconnected";
                    if (UpSpeedText != null) UpSpeedText.Text = $"Up: {upKbps:0} kbps";
                    if (DownSpeedText != null) DownSpeedText.Text = $"Down: {downKbps:0} kbps";
                });
                }
                catch { }

                Dispatcher.Invoke(UpdateDashboardVisibility);
            }
            catch { }
        }

        private void UpdateDashboardVisibility()
        {
            if (OptimizeButton != null) OptimizeButton.IsEnabled = string.Equals(_currentMode, "NEXA", StringComparison.OrdinalIgnoreCase);
            var fortisActive = string.Equals(_currentMode, "FORTIS", StringComparison.OrdinalIgnoreCase);
            if (QuickScanButton != null) QuickScanButton.IsEnabled = fortisActive;
            if (FullScanButton != null) FullScanButton.IsEnabled = fortisActive;
            if (ResearchButton != null) ResearchButton.IsEnabled = string.Equals(_currentMode, "LUMEN", StringComparison.OrdinalIgnoreCase);
        }

        private void AppendFileActivity(string text)
        {
            Dispatcher.Invoke(() =>
            {
                if (FileActivityList == null) return;
                FileActivityList.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} {text}");
                while (FileActivityList.Items.Count > 50) FileActivityList.Items.RemoveAt(FileActivityList.Items.Count - 1);
            });
        }

        private void AddMessageToChat(string content, string role)
        {
            var isUser = role == "user";
            var bubble = new Border
            {
                Background = isUser ? new SolidColorBrush(Color.FromRgb(0, 120, 212)) : new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                CornerRadius = isUser ? new CornerRadius(15, 15, 5, 15) : new CornerRadius(15, 15, 15, 5),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(10, 5, 50, 5),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = content,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                }
            };
            ChatMessagesPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToEnd();
        }

        private void ShowTypingIndicator()
        {
            if (TypingIndicator != null)
            {
                try
                {
                    if (TypingIndicator.Child is StackPanel sp && sp.Children.Count > 0 && sp.Children[0] is TextBlock lbl)
                    {
                        lbl.Text = ($"{_currentMode} is typing");
                    }
                }
                catch { }
                TypingIndicator.Visibility = Visibility.Visible;
                _typingActive = true;
                _typingTimer.Start();
            }
        }

        private void HideTypingIndicator()
        {
            TypingIndicator.Visibility = Visibility.Collapsed;
            _typingTimer.Stop();
            _currentStreamingMessage = null;
            _currentStreamingTextBlock = null;
            _typingActive = false;
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            TypingDots.Text = TypingDots.Text.Length >= 6 ? "." : TypingDots.Text + ".";
        }

        private void OpenChatButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Opening AI Chat Assistant...";
            ShowChatView();
            StatusText.Text = "AI Chat Assistant ready";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMainView();
            StatusText.Text = "Ready";
        }

        private void ShowChatView()
        {
            MainView.Visibility = Visibility.Collapsed;
            ChatView.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            HeaderTitle.Text = "AI Chat Assistant";
            MessageInput.Focus();
        }

        private void ShowMainView()
        {
            ChatView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Collapsed;
            HeaderTitle.Text = "Smartitecture - AI Desktop Assistant";
        }

        private async void ModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_llmService == null || ModelSelector.SelectedItem == null) return;
            var selected = ((ComboBoxItem)ModelSelector.SelectedItem).Content.ToString();
            if (selected.StartsWith("LUMEN")) _currentMode = "LUMEN";
            else if (selected.StartsWith("FORTIS")) _currentMode = "FORTIS";
            else if (selected.StartsWith("NEXA")) _currentMode = "NEXA";

            var best = ChooseBestModelForMode(_currentMode);
            var ok = await _llmService.SwitchModelAsync(best);
            _modeRouter.AutoDetectionEnabled = false;
            _modeRouter.ManualMode = _currentMode == "FORTIS" ? ChatMode.Fortis : _currentMode == "NEXA" ? ChatMode.Nexa : ChatMode.Lumen;
            AddMessageToChat(ok ? $"Switched to {_currentMode}" : $"Failed to switch mode: {_currentMode}", "system");
            UpdateModelStatus();
        }

        private async Task StreamTextAsync(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                var token = (i == 0 ? string.Empty : " ") + words[i];
                OnTokenReceived(token);
                await Task.Delay(35);
            }
        }

        // Deprecated: manual LLM selection is no longer exposed
        private async void ManualModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private string ChooseBestModelForMode(string mode)
        {
            var available = _llmService.AvailableModels?.ToList() ?? new List<string>();
            string[] priority = new[]
            {
                "Anthropic Claude 3.5 Sonnet",
                "OpenAI GPT-4",
                "Azure OpenAI GPT-4",
                "OpenAI GPT-3.5-Turbo",
                "Advanced AI Assistant",
                "Local Ollama Model",
                "Google Gemini",
                "System Expert Mode"
            };
            foreach (var p in priority) if (available.Contains(p)) return p;
            return _llmService.CurrentModel ?? "Advanced AI Assistant";
        }
        private void LaunchAppButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Launch App demo is disabled in this build";
        }

        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Shutdown demo is disabled in this build";
        }

        private async void RunScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecurityScanProgress != null) SecurityScanProgress.Value = 5;
            var result = await _defender.RunScanAsync(full: true);
            if (SecurityScanProgress != null) SecurityScanProgress.Value = 100;
            AddMessageToChat("[Security Analysis]\n" + (string.IsNullOrWhiteSpace(result) ? "Full scan completed." : result), "assistant");
            AddAction("Full system scan completed");
        }

        private async void QuickScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (ThreatStatusText != null) ThreatStatusText.Text = "Quick scan running...";
            if (SecurityScanProgress != null) SecurityScanProgress.Value = 15;
            var result = await _defender.RunScanAsync(full: false);
            if (SecurityScanProgress != null) SecurityScanProgress.Value = 100;
            if (LastScanText != null) LastScanText.Text = $"Last scan: {DateTime.Now:yyyy-MM-dd HH:mm}";
            if (ThreatStatusText != null) ThreatStatusText.Text = "Quick scan completed";
            AddMessageToChat("[Security Analysis]\n" + (string.IsNullOrWhiteSpace(result) ? "Quick scan completed." : result), "assistant");
            AddAction("Quick scan completed");
        }

        private async void FullScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (ThreatStatusText != null) ThreatStatusText.Text = "Full scan running...";
            if (SecurityScanProgress != null) SecurityScanProgress.Value = 10;
            var result = await _defender.RunScanAsync(full: true);
            if (SecurityScanProgress != null) SecurityScanProgress.Value = 100;
            if (LastScanText != null) LastScanText.Text = $"Last scan: {DateTime.Now:yyyy-MM-dd HH:mm}";
            if (ThreatStatusText != null) ThreatStatusText.Text = "Full scan completed";
            AddMessageToChat("[Security Analysis]\n" + (string.IsNullOrWhiteSpace(result) ? "Full scan completed." : result), "assistant");
            AddAction("Full scan completed");
        }

        private async void OptimizeButton_Click(object sender, RoutedEventArgs e)
        {
            AddMessageToChat("Optimizing performance...", "system");
            var ai = await _modeRouter.ProcessQueryAsync("optimize performance", _conversationId);
            await StreamTextAsync(ai.Content ?? "Optimization suggestions ready.");
            AddAction("Optimization analysis completed");
        }

        private async void ResearchButton_Click(object sender, RoutedEventArgs e)
        {
            var text = MessageInput?.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text)) text = "research this topic";
            if (ResearchProgress != null) ResearchProgress.Value = 25;
            if (ResearchStatusText != null) ResearchStatusText.Text = "Research in progress...";
            var ai = await _modeRouter.ProcessQueryAsync(text, _conversationId);
            if (ResearchProgress != null) ResearchProgress.Value = 100;
            await StreamTextAsync(ai.Content ?? string.Empty);
            if (ResearchStatusText != null) ResearchStatusText.Text = "Research complete";
            AddAction("Research completed");
        }

        private async void CleanTempButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                long bytes = 0; int files = 0;
                void clean(string path)
                {
                    try
                    {
                        if (!Directory.Exists(path)) return;
                        foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                        {
                            try { var fi = new FileInfo(f); bytes += fi.Length; fi.IsReadOnly = false; File.Delete(f); files++; }
                            catch { }
                        }
                    }
                    catch { }
                }
                var temp = Path.GetTempPath();
                var winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                clean(temp);
                clean(winTemp);
                AppendFileActivity($"Cleaned {files} files (~{Bytes(bytes)}) from temp folders");
                AddMessageToChat($"Cleanup complete: removed {files} files (~{Bytes(bytes)}).", "system");
            }
            catch (Exception ex)
            {
                AddMessageToChat($"Cleanup error: {ex.Message}", "system");
            }
        }

        private static string Bytes(long b)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = b; int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) { order++; len /= 1024; }
            return $"{len:0.##} {sizes[order]}";
        }

        private void AutoDetectToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (ModelSelector != null) ModelSelector.Visibility = Visibility.Collapsed;
            _modeRouter.AutoDetectionEnabled = true;
            AddMessageToChat("Auto mode enabled", "system");
            UpdateModelStatus();
        }

        private void AutoDetectToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ModelSelector != null) ModelSelector.Visibility = Visibility.Visible;
            _modeRouter.AutoDetectionEnabled = false;
            AddMessageToChat("Manual mode enabled", "system");
            UpdateModelStatus();
        }

        private void ModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatMessagesPanel == null || !IsLoaded) return;
            AddMessageToChat("Mode selector coming soon!", "system");
        }

        private async void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesPanel.Children.Clear();
            var welcome = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(20, 5, 20, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "Welcome to Smartitecture AI Assistant!",
                    Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            ChatMessagesPanel.Children.Add(welcome);
            if (_llmService != null)
            {
                await _llmService.ClearConversationAsync(_conversationId);
            }
            // Clear recent actions
            _recentActions.Clear();
            if (RecentActionsList != null) RecentActionsList.Items.Clear();
        }

        private void AddAction(string text)
        {
            var entry = $"{DateTime.Now:HH:mm:ss}  {text}";
            _recentActions.Insert(0, entry);
            while (_recentActions.Count > 5) _recentActions.RemoveAt(_recentActions.Count - 1);
            if (RecentActionsList != null)
            {
                RecentActionsList.Items.Clear();
                foreach (var a in _recentActions) RecentActionsList.Items.Add(a);
            }
        }

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("File attachment feature coming soon!", "Feature Preview", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}


