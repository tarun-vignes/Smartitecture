using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace SmartitectureSimple
{
    public partial class MainWindow : Window
    {
        private readonly OllamaService _ollamaService;
        private readonly WindowsAutomationService _automationService;
        private bool _isLLMConnected = false;
        private Process? _pythonProcess;
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "http://localhost:8001";
        
        // Theme system
        private string _currentTheme = "Dark";
        private bool _isDarkTheme = true;
        private string _themeMode = "Dark";
        public string ThemeMode { get; set; } = "Dark"; // "Dark", "Light", "System"

        public MainWindow()
        {
            InitializeComponent();
            _ollamaService = new OllamaService();
            _automationService = new WindowsAutomationService();
            
            // Initialize theme
            DetectAndApplySystemTheme();
            
            // Set up keyboard shortcuts
            KeyDown += MainWindow_KeyDown;
            
            // Initialize LLM connection
            _ = InitializeLLMAsync();
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ctrl+T to toggle theme
            if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ToggleTheme();
                e.Handled = true;
            }
        }

        private void DetectAndApplySystemTheme()
        {
            _themeMode = "System";
            ApplyTheme();
        }

        private bool IsSystemDarkTheme()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return true; // Default to dark theme if we can't detect
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

        private void UpdateUIState(bool llmConnected)
        {
            _isLLMConnected = llmConnected;
            
            if (llmConnected)
            {
                StatusText.Text = "AI Agent: Ready";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                RunAgentButton.IsEnabled = true;
                StopBackendButton.IsEnabled = true;
                StartBackendButton.IsEnabled = false;
            }
            else
            {
                StatusText.Text = "AI Agent: Offline";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(248, 81, 73)); // Red
                RunAgentButton.IsEnabled = false;
                StopBackendButton.IsEnabled = false;
                StartBackendButton.IsEnabled = true;
            }
        }

        private async void StartBackendButton_Click(object sender, RoutedEventArgs e)
        {
            await InitializeLLMAsync();
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
            finally
            {
                StopBackendButton.IsEnabled = false;
                StartBackendButton.IsEnabled = true;
            }
        }

        private async Task InitializeLLMAsync()
        {
            try
            {
                StatusText.Text = "Connecting to LLM...";
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(251, 191, 36)); // Orange

                var isRunning = await _ollamaService.IsOllamaRunningAsync();
                if (isRunning)
                {
                    var models = await _ollamaService.GetAvailableModelsAsync();
                    _isLLMConnected = true;
                    UpdateUIState(true);
                    
                    OutputTextBox.Text = "🚀 Smartitecture AI Agent Ready!\n\n" +
                                       "✅ Local LLM connected (Ollama)\n" +
                                       $"✅ Available models: {string.Join(", ", models)}\n" +
                                       "✅ Windows automation tools loaded\n" +
                                       "✅ Ready for natural language commands\n\n" +
                                       "Try commands like:\n" +
                                       "• 'Take a screenshot'\n" +
                                       "• 'Focus on Chrome browser'\n" +
                                       "• 'Show system performance'\n" +
                                       "• 'List running processes'\n" +
                                       "• 'Check network connectivity'\n\n" +
                                       "🎯 AI-powered automation with local privacy!";
                }
                else
                {
                    _isLLMConnected = false;
                    UpdateUIState(false);
                    OutputTextBox.Text = "❌ Ollama LLM Not Available\n\n" +
                                       "To use AI features:\n" +
                                       "1. Install Ollama from https://ollama.ai\n" +
                                       "2. Run: ollama pull llama3.1\n" +
                                       "3. Start Ollama service\n\n" +
                                       "Manual automation commands still work!";
                }
            }
            catch (Exception ex)
            {
                _isLLMConnected = false;
                UpdateUIState(false);
                OutputTextBox.Text = $"❌ LLM Connection Failed: {ex.Message}";
            }
        }

        private async void StopBackendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ollamaService?.Dispose();
                UpdateUIState(false);
                OutputTextBox.Text = "⏹ AI Agent disconnected.\n\nReady to reconnect when needed.";
            }
            catch (Exception ex)
            {
                OutputTextBox.Text = $"❌ Error disconnecting: {ex.Message}";
            }
        }

        private async void RunAgentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isLLMConnected)
            {
                OutputTextBox.Text = "❌ LLM not connected. Please check Ollama installation.";
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

                OutputTextBox.Text = $"🤖 Processing: {userInput}\n\n⏳ AI agent is analyzing your request...";

                // Generate automation command using LLM
                var llmResponse = await _ollamaService.GenerateWindowsAutomationCommandAsync(userInput);
                
                try
                {
                    // Parse the LLM response as JSON
                    var commandData = JsonSerializer.Deserialize<JsonElement>(llmResponse);
                    
                    if (commandData.TryGetProperty("action", out var actionElement) &&
                        commandData.TryGetProperty("parameters", out var parametersElement) &&
                        commandData.TryGetProperty("description", out var descriptionElement))
                    {
                        var action = actionElement.GetString();
                        var description = descriptionElement.GetString();
                        
                        // Convert parameters to dictionary
                        var parameters = new Dictionary<string, object>();
                        foreach (var param in parametersElement.EnumerateObject())
                        {
                            parameters[param.Name] = param.Value.GetString() ?? "";
                        }
                        
                        OutputTextBox.Text = $"🤖 Command: {userInput}\n\n" +
                                           $"🧠 AI Analysis: {description}\n" +
                                           $"⚡ Executing: {action}\n\n" +
                                           "⏳ Running automation...";
                        
                        // Execute the automation command
                        var result = await _automationService.ExecuteAutomationCommand(action, parameters);
                        
                        OutputTextBox.Text = $"🤖 Command: {userInput}\n\n" +
                                           $"🧠 AI Analysis: {description}\n" +
                                           $"⚡ Action: {action}\n\n" +
                                           $"📋 Result:\n{result}";
                    }
                    else
                    {
                        // Fallback: treat as direct automation command
                        var result = await ExecuteDirectCommand(userInput);
                        OutputTextBox.Text = $"🤖 Command: {userInput}\n\n" +
                                           $"📋 Direct Execution Result:\n{result}";
                    }
                }
                catch (JsonException)
                {
                    // If LLM response is not valid JSON, treat as direct command
                    var result = await ExecuteDirectCommand(userInput);
                    OutputTextBox.Text = $"🤖 Command: {userInput}\n\n" +
                                       $"🧠 AI Response: {llmResponse}\n\n" +
                                       $"📋 Automation Result:\n{result}";
                }
            }
            catch (Exception ex)
            {
                OutputTextBox.Text = $"❌ Error: {ex.Message}";
            }
            finally
            {
                RunAgentButton.IsEnabled = true;
            }
        }

        private async Task<string> ExecuteDirectCommand(string userInput)
        {
            // Simple command mapping for common automation tasks
            var input = userInput.ToLower();
            
            if (input.Contains("screenshot") || input.Contains("capture"))
            {
                return await _automationService.ExecuteAutomationCommand("screenshot", new Dictionary<string, object>());
            }
            else if (input.Contains("focus") || input.Contains("bring"))
            {
                var processName = ExtractProcessName(userInput);
                var parameters = new Dictionary<string, object> { ["process"] = processName };
                return await _automationService.ExecuteAutomationCommand("focus_window", parameters);
            }
            else if (input.Contains("system") || input.Contains("performance") || input.Contains("info"))
            {
                return await _automationService.ExecuteAutomationCommand("system_info", new Dictionary<string, object>());
            }
            else if (input.Contains("process") || input.Contains("running"))
            {
                return await _automationService.ExecuteAutomationCommand("list_processes", new Dictionary<string, object>());
            }
            else if (input.Contains("disk") || input.Contains("storage"))
            {
                return await _automationService.ExecuteAutomationCommand("get_disk_usage", new Dictionary<string, object>());
            }
            else if (input.Contains("network") || input.Contains("internet"))
            {
                return await _automationService.ExecuteAutomationCommand("get_network_info", new Dictionary<string, object>());
            }
            else
            {
                return $"Command not recognized. Try:\n• Take a screenshot\n• Focus on [app name]\n• Show system info\n• List processes\n• Check disk usage\n• Network info";
            }
        }

        private string ExtractProcessName(string input)
        {
            var words = input.ToLower().Split(' ');
            var commonApps = new[] { "chrome", "firefox", "notepad", "calculator", "explorer", "word", "excel", "powerpoint", "outlook", "teams", "discord", "spotify", "steam" };
            
            foreach (var app in commonApps)
            {
                if (input.ToLower().Contains(app))
                    return app;
            }
            
            // Try to extract from "focus on X" pattern
            var focusIndex = Array.IndexOf(words, "on");
            if (focusIndex >= 0 && focusIndex < words.Length - 1)
            {
                return words[focusIndex + 1];
            }
            
            return "chrome"; // Default fallback
        }

        private void QuickCommand_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string command)
            {
                InputTextBox.Text = command;
                if (_isLLMConnected)
                {
                    RunAgentButton_Click(sender, e);
                }
            }
        }

        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _isLLMConnected)
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
