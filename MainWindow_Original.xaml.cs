using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartitectureSimple
{
    public partial class MainWindow : Window
    {
        private Process? _pythonProcess;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://127.0.0.1:8001";
        private bool _isDarkTheme = true;
        private string _themeMode = "Dark"; // "Dark", "Light", "System"
        private string _currentTheme = "Dark"; // For settings window compatibility
        private bool _isBackendRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // Initialize UI state
            UpdateUIState(false);
            
            // Add keyboard shortcut for theme toggle
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+T to toggle theme
            if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ToggleTheme();
                e.Handled = true;
            }
        }

        private void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            _currentTheme = _isDarkTheme ? "Dark" : "Light";
            _themeMode = _currentTheme;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            ApplyTheme(_themeMode);
        }

        private void ApplyTheme(string themeMode)
        {
            _themeMode = themeMode;
            var isDark = _themeMode == "Dark" || (_themeMode == "System" && IsSystemDarkTheme());
            _isDarkTheme = isDark;
            _currentTheme = _themeMode;

            if (isDark)
            {
                // Dark theme
                Background = new SolidColorBrush(Color.FromRgb(13, 17, 23));
                ApplyDarkThemeToControls();
            }
            else
            {
                // Light theme
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                ApplyLightThemeToControls();
            }
        }

        private void ApplyDarkThemeToControls()
        {
            foreach (Border border in FindVisualChildren<Border>(this))
            {
                border.Background = new SolidColorBrush(Color.FromRgb(28, 33, 40));
            }
            
            foreach (TextBlock textBlock in FindVisualChildren<TextBlock>(this))
            {
                if (textBlock.Name != "StatusText")
                    textBlock.Foreground = new SolidColorBrush(Colors.White);
            }
            
            foreach (TextBox textBox in FindVisualChildren<TextBox>(this))
            {
                textBox.Background = new SolidColorBrush(Color.FromRgb(22, 27, 34));
                textBox.Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243));
                textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61));
            }
        }

        private void ApplyLightThemeToControls()
        {
            foreach (Border border in FindVisualChildren<Border>(this))
            {
                border.Background = new SolidColorBrush(Colors.White);
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            }
            
            foreach (TextBlock textBlock in FindVisualChildren<TextBlock>(this))
            {
                if (textBlock.Name != "StatusText")
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39));
            }
            
            foreach (TextBox textBox in FindVisualChildren<TextBox>(this))
            {
                textBox.Background = new SolidColorBrush(Color.FromRgb(249, 250, 251));
                textBox.Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39));
                textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));
            }
            
            foreach (Button button in FindVisualChildren<Button>(this))
            {
                if (button.Name != "RunAgentButton" && button.Name != "StartBackendButton" && button.Name != "StopBackendButton")
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(243, 244, 246));
                    button.Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39));
                    button.BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));
                }
            }
        }

        private bool IsSystemDarkTheme()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key?.GetValue("AppsUseLightTheme");
                    return value is int intValue && intValue == 0;
                }
            }
            catch
            {
                return true; // Default to dark if can't detect
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void UpdateUIState(bool backendRunning)
        {
            _isBackendRunning = backendRunning;
            
            if (backendRunning)
            {
                StatusText.Text = "Backend: Online";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                RunAgentButton.IsEnabled = true;
                StopBackendButton.IsEnabled = true;
                StartBackendButton.IsEnabled = false;
            }
            else
            {
                StatusText.Text = "Backend: Offline";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(248, 81, 73)); // Red
                RunAgentButton.IsEnabled = false;
                StopBackendButton.IsEnabled = false;
                StartBackendButton.IsEnabled = true;
            }
        }

        private async void StartBackendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartBackendButton.IsEnabled = false;
                StatusText.Text = "Starting backend...";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(251, 191, 36)); // Orange

                // Get the directory where the executable is located
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string pythonScriptPath = System.IO.Path.Combine(exeDirectory, "minimal_server.py");

                if (!System.IO.File.Exists(pythonScriptPath))
                {
                    throw new Exception($"Python script not found: {pythonScriptPath}");
                }

                // Start Python process (completely hidden using pythonw.exe)
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pythonw",  // Use pythonw.exe instead of python.exe to hide console
                    Arguments = $"\"{pythonScriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                _pythonProcess = Process.Start(startInfo);
                if (_pythonProcess == null)
                {
                    throw new Exception("Failed to start Python process");
                }

                // Wait for backend to start and perform health check
                await Task.Delay(2000);
                
                var healthResponse = await _httpClient.GetAsync($"{BaseUrl}/health");
                if (healthResponse.IsSuccessStatusCode)
                {
                    var healthContent = await healthResponse.Content.ReadAsStringAsync();
                    var healthData = JsonSerializer.Deserialize<JsonElement>(healthContent);
                    
                    UpdateUIState(true);
                    OutputTextBox.Text = "🚀 ReAct Agent Backend Started Successfully!\n\n" +
                                       "✅ Python backend is running\n" +
                                       "✅ ReAct framework initialized\n" +
                                       "✅ All 13 automation tools loaded\n" +
                                       "✅ Ready for natural language commands\n\n" +
                                       "Try commands like:\n" +
                                       "• 'Take a screenshot'\n" +
                                       "• 'Focus on chrome'\n" +
                                       "• 'What's 25 * 4 + 17?'\n" +
                                       "• 'Show running programs'\n" +
                                       "• 'List files in current directory'";
                }
                else
                {
                    throw new Exception($"Backend health check failed: {healthResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                UpdateUIState(false);
                OutputTextBox.Text = $"❌ Backend Failed to Start\n\nError: {ex.Message}\n\n" +
                                   "Troubleshooting:\n" +
                                   "• Ensure Python is installed and in PATH\n" +
                                   "• Check if port 8001 is available\n" +
                                   "• Verify minimal_server.py exists";
                
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    try { _pythonProcess.Kill(); } catch { }
                    _pythonProcess = null;
                }
            }
        }

        private async void StopBackendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill();
                    _pythonProcess = null;
                }
                
                UpdateUIState(false);
                OutputTextBox.Text = "⏹ Backend stopped successfully.\n\nReady to restart when needed.";
            }
            catch (Exception ex)
            {
                OutputTextBox.Text = $"❌ Error stopping backend: {ex.Message}";
            }
        }

        private async void RunAgentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isBackendRunning)
            {
                OutputTextBox.Text = "❌ Backend not running. Please start the backend first.";
                return;
            }

            try
            {
                RunAgentButton.IsEnabled = false;
                var userInput = InputTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(userInput))
                {
                    OutputTextBox.Text = "❌ Please enter a command first.";
                    return;
                }

                OutputTextBox.Text = $"🤖 Processing: {userInput}\n\n⏳ ReAct agent is thinking...";

                var requestData = new
                {
                    input = userInput,
                    max_iterations = 3
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/agent/run", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var agentResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    var result = agentResponse.GetProperty("result").GetString();
                    var scratchpad = agentResponse.GetProperty("scratchpad");
                    var toolsUsed = agentResponse.GetProperty("tools_used");
                    var iterations = agentResponse.GetProperty("iterations").GetInt32();

                    var output = $"🤖 Command: {userInput}\n\n";
                    output += $"📋 ReAct Reasoning Process ({iterations} iterations):\n";
                    
                    foreach (var step in scratchpad.EnumerateArray())
                    {
                        output += $"  {step.GetString()}\n";
                    }
                    
                    output += $"\n🎯 Final Result:\n{result}\n\n";
                    
                    if (toolsUsed.GetArrayLength() > 0)
                    {
                        output += "🛠️ Tools Used:\n";
                        foreach (var tool in toolsUsed.EnumerateArray())
                        {
                            output += $"  • {tool.GetString()}\n";
                        }
                    }

                    OutputTextBox.Text = output;
                }
                else
                {
                    OutputTextBox.Text = $"❌ Agent Error: {responseContent}";
                }
            }
            catch (Exception ex)
            {
                OutputTextBox.Text = $"❌ Exception: {ex.Message}";
            }
            finally
            {
                RunAgentButton.IsEnabled = true;
            }
        }

        private void QuickCommand_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string command)
            {
                InputTextBox.Text = command;
                if (_isBackendRunning)
                {
                    RunAgentButton_Click(sender, e);
                }
            }
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _isBackendRunning)
            {
                RunAgentButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Text = "Output cleared.\n\nReady for new commands.";
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow
            {
                ThemeMode = _currentTheme,
                BackendAutoStart = false, // You can load this from settings
                BackendPort = 8001
            };

            if (settingsWindow.ShowDialog() == true)
            {
                // Apply settings
                _currentTheme = settingsWindow.ThemeMode;
                ApplyTheme(_currentTheme);
                
                // You could also update backend settings here
                OutputTextBox.Text += $"\n✅ Settings updated: Theme = {_currentTheme}";
            }
        }

        private void VoiceInputButton_Click(object sender, RoutedEventArgs e)
        {
            // Voice input functionality (placeholder for future implementation)
            OutputTextBox.Text += "\n🎤 Voice Input: Coming soon! This feature will enable voice-to-text command input.\n" +
                                 "Future capabilities:\n" +
                                 "• Speech recognition for natural language commands\n" +
                                 "• Voice-activated automation workflows\n" +
                                 "• Hands-free system control\n" +
                                 "• Multi-language support\n";
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill();
                }
                _httpClient?.Dispose();
            }
            catch { }
            
            base.OnClosed(e);
        }
    }
}
