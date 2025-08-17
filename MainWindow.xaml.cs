using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
// using SmartitectureSimple.Plugins; // Temporarily disabled
using System.Windows.Markup;
using System.Runtime.InteropServices;

namespace SmartitectureSimple
{
    public class CommandHistoryItem
    {
        public string Input { get; set; } = "";
        public string Output { get; set; } = "";
        public DateTime Timestamp { get; set; }
        
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Input}";
        }
    }

    public partial class MainWindow : Window
    {
        // Windows API for hiding console windows
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        
        private HttpClientService _httpClient;
        private BackendHealthMonitor _healthMonitor;
        private LoggingService _logger;
        // private PluginRegistry _pluginRegistry; // Temporarily disabled
        private Process _pythonProcess;
        private List<CommandHistoryItem> _commandHistory = new List<CommandHistoryItem>();
        private const string BaseUrl = "http://127.0.0.1:8001";

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize advanced error handling services
            _httpClient = new HttpClientService(maxRetries: 3, timeoutSeconds: 30);
            _healthMonitor = new BackendHealthMonitor(BaseUrl);
            _logger = LoggingService.Instance;
            // _pluginRegistry = new PluginRegistry(_logger); // Temporarily disabled
            
            // Set up health monitoring events
            _healthMonitor.StatusChanged += OnBackendStatusChanged;
            _healthMonitor.RecoveryAttempted += OnBackendRecoveryAttempted;
            
            _logger.Info("Smartitecture application starting up");
            
            // Initialize plugin system
            // InitializePluginSystemAsync(); // Temporarily disabled
            
            // Initialize theme on startup
            LoadSettings();
            ApplyTheme();
        }

        private async void StartBackendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartBackendButton.IsEnabled = false;
                StatusText.Text = "Starting Python backend...";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;

                // Get the directory where the executable is located
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string pythonScriptPath = System.IO.Path.Combine(exeDirectory, "minimal_server.py");

                if (!System.IO.File.Exists(pythonScriptPath))
                {
                    throw new Exception($"Python script not found: {pythonScriptPath}");
                }

                // Start Python process (completely invisible - no windows at all)
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pythonw.exe",  // Use pythonw.exe instead of python.exe to hide console
                    Arguments = $"\"{pythonScriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = false,  // No redirection to prevent any console
                    RedirectStandardError = false,   // No redirection to prevent any console
                    RedirectStandardInput = false,   // No input redirection
                    WorkingDirectory = exeDirectory,
                    Verb = "",  // No special verb
                    LoadUserProfile = false  // Don't load user profile to minimize footprint
                };
                
                // Multiple layers of window hiding
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;

                _pythonProcess = Process.Start(startInfo);
                
                if (_pythonProcess == null)
                {
                    throw new Exception("Failed to start Python process");
                }
                
                // Additional step to ensure complete invisibility
                if (!_pythonProcess.HasExited)
                {
                    try
                    {
                        // Wait a moment for process to initialize
                        await Task.Delay(500);
                        
                        // Force hide any window that might have appeared
                        if (_pythonProcess.MainWindowHandle != IntPtr.Zero)
                        {
                            ShowWindow(_pythonProcess.MainWindowHandle, 0); // SW_HIDE = 0
                        }
                        
                        // Also hide any child windows
                        foreach (ProcessThread thread in _pythonProcess.Threads)
                        {
                            // Additional hiding measures for any spawned windows
                        }
                    }
                    catch
                    {
                        // Ignore errors in window hiding - process is still running
                    }
                }

                // Wait a moment for the server to start
                await Task.Delay(2000);

                // Test the connection
                bool isHealthy = await TestBackendHealth();
                
                if (isHealthy)
                {
                    StatusText.Text = "✅ Python Backend: Running on port 8001";
                    StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                    RunAgentButton.IsEnabled = true;
                    StopBackendButton.IsEnabled = true;
                    OutputTextBox.Text = "Backend started successfully! Ready to process requests.";
                }
                else
                {
                    throw new Exception("Backend started but health check failed");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "❌ Backend Failed to Start";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                OutputTextBox.Text = $"Error starting backend: {ex.Message}";
                StartBackendButton.IsEnabled = true;
                
                // Clean up process if it exists
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill();
                    _pythonProcess = null;
                }
            }
        }

        private async void RunAgentButton_Click(object sender, RoutedEventArgs e)
        {
            // Declare variables outside try block for catch/finally block access
            bool isMultiStep = false;
            string input = "";
            
            try
            {
                RunAgentButton.IsEnabled = false;
                OutputTextBox.Text = "Processing request...";

                input = InputTextBox.Text;
                if (string.IsNullOrWhiteSpace(input))
                {
                    input = "Test request from simplified Smartitecture";
                }
                
                // Command will be added to history with output after processing

                // Check if this might be a multi-step task
                isMultiStep = ContainsMultiStepKeywords(input);
                if (isMultiStep)
                {
                    ShowProgressIndicator("Analyzing multi-step task...", 0);
                }

                var requestData = new
                {
                    input = input,
                    max_iterations = 3
                };

                string jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                if (isMultiStep)
                {
                    UpdateProgressIndicator("Executing steps...", 50);
                }

                var response = await _httpClient.PostAsync($"{BaseUrl}/agent/run", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (isMultiStep)
                {
                    UpdateProgressIndicator("Completing task...", 90);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    string resultText = result.GetProperty("result").GetString() ?? "No result";
                    
                    // Check if it's a multi-step result
                    if (result.TryGetProperty("framework", out var framework) && 
                        framework.GetString() == "ReAct Multi-Step")
                    {
                        if (result.TryGetProperty("total_steps", out var totalSteps) && 
                            result.TryGetProperty("completed_steps", out var completedSteps))
                        {
                            UpdateProgressIndicator($"Completed {completedSteps.GetInt32()}/{totalSteps.GetInt32()} steps", 100);
                        }
                    }
                    
                    string outputText = $"✅ Success!\n\nInput: {input}\n\nResult: {resultText}\n\nResponse: {responseContent}";
                    OutputTextBox.Text = outputText;
                    
                    // Add to command history with output
                    AddToCommandHistory(input, outputText);
                }
                else
                {
                    string errorText = $"❌ Error: {response.StatusCode}\n\nResponse: {responseContent}";
                    OutputTextBox.Text = errorText;
                    
                    // Add to command history with error output
                    AddToCommandHistory(input, errorText);
                }
            }
            catch (Exception ex)
            {
                string exceptionText = $"❌ Exception: {ex.Message}";
                OutputTextBox.Text = exceptionText;
                
                // Add to command history with exception output
                AddToCommandHistory(input, exceptionText);
            }
            finally
            {
                RunAgentButton.IsEnabled = true;
                
                // Hide progress indicator after a brief delay to show completion
                if (isMultiStep)
                {
                    await Task.Delay(2000); // Show completion for 2 seconds
                    HideProgressIndicator();
                }
            }
        }

        private void StopBackendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill();
                    _pythonProcess.WaitForExit(5000);
                    _pythonProcess = null;
                }

                StatusText.Text = "Python Backend: Stopped";
                StatusText.Foreground = System.Windows.Media.Brushes.Gray;
                RunAgentButton.IsEnabled = false;
                StopBackendButton.IsEnabled = false;
                StartBackendButton.IsEnabled = true;
                OutputTextBox.Text = "Backend stopped successfully.";
            }
            catch (Exception ex)
            {
                OutputTextBox.Text = $"Error stopping backend: {ex.Message}";
            }
        }

        private async Task<bool> TestBackendHealth()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private bool ContainsMultiStepKeywords(string input)
        {
            string[] multiStepKeywords = { "then", "and then", "after that", "followed by" };
            string lowerInput = input.ToLower();
            return Array.Exists(multiStepKeywords, keyword => lowerInput.Contains(keyword));
        }

        private void ShowProgressIndicator(string message, double progress)
        {
            ProgressPanel.Visibility = Visibility.Visible;
            ProgressText.Text = message;
            ProgressBar.Value = progress;
        }

        private void UpdateProgressIndicator(string message, double progress)
        {
            ProgressText.Text = message;
            ProgressBar.Value = progress;
        }

        private void HideProgressIndicator()
        {
            ProgressPanel.Visibility = Visibility.Collapsed;
        }

        // Settings Panel Event Handlers
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            ShowSettingsPanel();
        }

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            HideSettingsPanel();
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            HideSettingsPanel();
        }

        private void CancelSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            HideSettingsPanel();
        }

        private void ShowSettingsPanel()
        {
            SettingsOverlay.Visibility = Visibility.Visible;
        }

        private void HideSettingsPanel()
        {
            SettingsOverlay.Visibility = Visibility.Collapsed;
        }

        private void LoadSettings()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SmartitectureSimple"))
                {
                    var theme = key.GetValue("Theme", "System").ToString();
                    SystemThemeRadio.IsChecked = theme == "System";
                    DarkThemeRadio.IsChecked = theme == "Dark";
                    LightThemeRadio.IsChecked = theme == "Light";
                    BlueThemeRadio.IsChecked = theme == "Blue";
                    BackendPortTextBox.Text = key.GetValue("BackendPort", "8001").ToString();
                    BackendTimeoutTextBox.Text = key.GetValue("BackendTimeout", "30").ToString();
                    AutoStartBackendCheckBox.IsChecked = bool.Parse(key.GetValue("AutoStartBackend", "True").ToString());
                    ShowProgressIndicatorsCheckBox.IsChecked = bool.Parse(key.GetValue("ShowProgressIndicators", "True").ToString());
                    ShowCommandHistoryCheckBox.IsChecked = bool.Parse(key.GetValue("ShowCommandHistory", "True").ToString());
                    MinimizeToTrayCheckBox.IsChecked = bool.Parse(key.GetValue("MinimizeToTray", "False").ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SmartitectureSimple"))
                {
                    string theme = "System";
                    if (DarkThemeRadio.IsChecked == true) theme = "Dark";
                    else if (LightThemeRadio.IsChecked == true) theme = "Light";
                    else if (BlueThemeRadio.IsChecked == true) theme = "Blue";
                    
                    key.SetValue("Theme", theme);
                    key.SetValue("BackendPort", BackendPortTextBox.Text);
                    key.SetValue("BackendTimeout", BackendTimeoutTextBox.Text);
                    key.SetValue("AutoStartBackend", (AutoStartBackendCheckBox.IsChecked ?? false).ToString());
                    key.SetValue("ShowProgressIndicators", (ShowProgressIndicatorsCheckBox.IsChecked ?? true).ToString());
                    key.SetValue("ShowCommandHistory", (ShowCommandHistoryCheckBox.IsChecked ?? true).ToString());
                    key.SetValue("MinimizeToTray", (MinimizeToTrayCheckBox.IsChecked ?? false).ToString());
                }
                ApplyTheme();
                MessageBox.Show("Settings saved successfully! ✅", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyTheme()
        {
            string theme = "Dark";
            if (LightThemeRadio.IsChecked == true) theme = "Light";
            else if (BlueThemeRadio.IsChecked == true) theme = "Blue";
            else if (SystemThemeRadio?.IsChecked == true) theme = GetSystemTheme();

            ApplyThemeColors(theme);
        }

        private string GetSystemTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key?.GetValue("AppsUseLightTheme") is int lightTheme)
                    {
                        return lightTheme == 0 ? "Dark" : "Light";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting system theme: {ex.Message}");
            }
            return "Dark"; // Default fallback
        }

        private void ApplyThemeColors(string theme)
        {
            switch (theme)
            {
                case "Light":
                    // Light theme with light grey containers
                    this.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); // Pure white background
                    UpdateTextColors(Color.FromRgb(0, 0, 0), Color.FromRgb(0, 0, 0)); // Pure black text
                    UpdateTextBoxColors(Color.FromRgb(255, 255, 255), Color.FromRgb(0, 0, 0), Color.FromRgb(200, 200, 200));
                    UpdateContainerColors(Color.FromRgb(240, 240, 240)); // Light grey container
                    break;
                case "Blue":
                    // Professional blue theme
                    this.Background = new SolidColorBrush(Color.FromRgb(13, 110, 253));
                    UpdateTextColors(Colors.White, Color.FromRgb(255, 255, 255));
                    UpdateTextBoxColors(Color.FromRgb(255, 255, 255), Colors.Black, Color.FromRgb(108, 117, 125));
                    UpdateContainerColors(Color.FromRgb(0, 102, 204)); // #0066CC - Professional blue container
                    break;
                default: // Dark
                    // Dark theme
                    this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    UpdateTextColors(Colors.White, Color.FromRgb(255, 255, 255));
                    UpdateTextBoxColors(Color.FromRgb(52, 58, 64), Colors.White, Color.FromRgb(108, 117, 125));
                    UpdateContainerColors(Color.FromRgb(45, 45, 45)); // Dark grey container
                    break;
            }
        }

        private void UpdateTextColors(Color primaryText, Color secondaryText)
        {
            try
            {
                // Update main text elements
                var titleBlock = this.FindName("TitleBlock") as TextBlock;
                if (titleBlock != null) titleBlock.Foreground = new SolidColorBrush(primaryText);
                
                var statusText = this.FindName("StatusText") as TextBlock;
                if (statusText != null && statusText.Foreground != System.Windows.Media.Brushes.Orange && 
                    statusText.Foreground != System.Windows.Media.Brushes.LightGreen && 
                    statusText.Foreground != System.Windows.Media.Brushes.Red)
                {
                    statusText.Foreground = new SolidColorBrush(secondaryText);
                }
                
                // Update all labels and text elements
                UpdateAllTextElements(primaryText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating text colors: {ex.Message}");
            }
        }
        
        private void UpdateAllTextElements(Color textColor)
        {
            try
            {
                // Find and update all TextBlock and Label elements
                var textBrush = new SolidColorBrush(textColor);
                
                // Update Input/Output labels
                foreach (var child in LogicalTreeHelper.GetChildren(this))
                {
                    UpdateTextElementsRecursive(child, textBrush);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating all text elements: {ex.Message}");
            }
        }
        
        private void UpdateTextElementsRecursive(object element, SolidColorBrush textBrush)
        {
            try
            {
                if (element is TextBlock textBlock && textBlock.Name != "StatusText")
                {
                    textBlock.Foreground = textBrush;
                }
                else if (element is Label label)
                {
                    label.Foreground = textBrush;
                }
                else if (element is DependencyObject depObj)
                {
                    foreach (var child in LogicalTreeHelper.GetChildren(depObj))
                    {
                        UpdateTextElementsRecursive(child, textBrush);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in recursive text update: {ex.Message}");
            }
        }

        private void UpdateTextBoxColors(Color backgroundColor, Color textColor, Color borderColor)
        {
            try
            {
                // Update Input TextBox
                var inputTextBox = this.FindName("InputTextBox") as TextBox;
                if (inputTextBox != null)
                {
                    inputTextBox.Background = new SolidColorBrush(backgroundColor);
                    inputTextBox.Foreground = new SolidColorBrush(textColor);
                    inputTextBox.BorderBrush = new SolidColorBrush(borderColor);
                }
                
                // Update Output TextBox
                var outputTextBox = this.FindName("OutputTextBox") as TextBox;
                if (outputTextBox != null)
                {
                    outputTextBox.Background = new SolidColorBrush(backgroundColor);
                    outputTextBox.Foreground = new SolidColorBrush(textColor);
                    outputTextBox.BorderBrush = new SolidColorBrush(borderColor);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating textbox colors: {ex.Message}");
            }
        }
        
        private void UpdateContainerColors(Color containerColor)
        {
            try
            {
                // Directly target the named InputOutputContainer Border
                if (InputOutputContainer != null)
                {
                    InputOutputContainer.Background = new SolidColorBrush(containerColor);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating container colors: {ex.Message}");
            }
        }
        
        private void CommandHistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CommandHistoryListBox.SelectedItem is CommandHistoryItem selectedItem)
                {
                    // Show the full input/output conversation
                    InputTextBox.Text = selectedItem.Input;
                    OutputTextBox.Text = selectedItem.Output;
                    CommandHistoryListBox.SelectedItem = null; // Deselect after clicking
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command history selection: {ex.Message}");
            }
        }
        
        private void AddToCommandHistory(string command, string output = "")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    var historyItem = new CommandHistoryItem
                    {
                        Input = command,
                        Output = output,
                        Timestamp = DateTime.Now
                    };
                    
                    // Add to the beginning of the list
                    CommandHistoryListBox.Items.Insert(0, historyItem);
                    
                    // Keep only the last 10 commands
                    while (CommandHistoryListBox.Items.Count > 10)
                    {
                        CommandHistoryListBox.Items.RemoveAt(CommandHistoryListBox.Items.Count - 1);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to command history: {ex.Message}");
            }
        }
        
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to clear all command history?\n\nThis action cannot be undone.",
                    "Clear Command History",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);
                    
                if (result == MessageBoxResult.Yes)
                {
                    CommandHistoryListBox.Items.Clear();
                    MessageBox.Show("Command history cleared! 🗑️", "History Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DeleteSingleCommand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is CommandHistoryItem historyItem)
                {
                    var result = MessageBox.Show(
                        $"Delete this command?\n\n[{historyItem.Timestamp:HH:mm:ss}] {historyItem.Input}\n\nThis action cannot be undone.",
                        "Delete Command",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);
                        
                    if (result == MessageBoxResult.Yes)
                    {
                        CommandHistoryListBox.Items.Remove(historyItem);
                        MessageBox.Show("Command deleted! ❌", "Command Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CommandHistoryItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is TextBlock textBlock && textBlock.DataContext is CommandHistoryItem historyItem)
                {
                    // Load the command input and output
                    InputTextBox.Text = historyItem.Input;
                    OutputTextBox.Text = historyItem.Output;
                    
                    // Show feedback to user
                    MessageBox.Show($"Command loaded!\n\nInput: {historyItem.Input}\nOutput: {(historyItem.Output.Length > 100 ? historyItem.Output.Substring(0, 100) + "..." : historyItem.Output)}", 
                                  "Command Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var aboutWindow = new AboutWindow();
                aboutWindow.Owner = this;
                aboutWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to open About dialog", ex);
                MessageBox.Show($"Failed to open About dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Plugin system temporarily disabled
        // private async void InitializePluginSystemAsync()
        // {
        //     try
        //     {
        //         _logger.LogInfo("Initializing plugin system...");
        //         await _pluginRegistry.InitializeAsync();
        //         _logger.LogInfo($"Plugin system initialized with {_pluginRegistry.ToolCount} tools");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError("Failed to initialize plugin system", ex);
        //     }
        // }

        private void PluginManagerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Plugin system temporarily disabled
                MessageBox.Show("Plugin Manager is currently under development and will be available in the next update.", "Plugin Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                // var pluginManagerWindow = new PluginManagerWindow(_pluginRegistry, _logger);
                // pluginManagerWindow.Owner = this;
                // pluginManagerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to open Plugin Manager", ex);
                MessageBox.Show($"Failed to open Plugin Manager: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WorkflowDesignerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var workflowDesignerWindow = new WorkflowDesignerWindow(_logger);
                workflowDesignerWindow.Owner = this;
                workflowDesignerWindow.ShowDialog();
                _logger.Info("Workflow Designer window closed");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to open Workflow Designer", ex);
                MessageBox.Show($"Failed to open Workflow Designer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OnBackendStatusChanged(object sender, BackendStatusEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e.IsHealthy)
                {
                    StatusText.Text = "Backend: Healthy ✅";
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                    RunAgentButton.IsEnabled = true;
                    _logger.Info("Backend status: Healthy");
                }
                else
                {
                    StatusText.Text = $"Backend: Unhealthy ❌ (Failures: {e.ConsecutiveFailures})";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    RunAgentButton.IsEnabled = false;
                    _logger.Warning($"Backend status: Unhealthy - {e.Error}");
                }
            });
        }
        
        private void OnBackendRecoveryAttempted(object sender, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"🔄 {message}";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;
                _logger.Info($"Recovery attempt: {message}");
            });
        }
        
        public async void AttemptBackendRecovery()
        {
            try
            {
                _logger.Info("Starting backend recovery process");
                
                // Stop existing backend if running
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _logger.Info("Stopping existing backend process");
                    _pythonProcess.Kill();
                    _pythonProcess.Dispose();
                    _pythonProcess = null;
                    await Task.Delay(2000); // Wait for cleanup
                }
                
                // Restart backend
                _logger.Info("Attempting to restart backend");
                await StartBackendProcess();
                
                // Wait for backend to become healthy
                var isHealthy = await _healthMonitor.WaitForHealthyAsync(TimeSpan.FromSeconds(30));
                
                if (isHealthy)
                {
                    _logger.Info("Backend recovery successful");
                    StatusText.Text = "Backend recovered successfully! ✅";
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    _logger.Error("Backend recovery failed - backend did not become healthy");
                    StatusText.Text = "Backend recovery failed ❌";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Backend recovery process failed", ex);
                StatusText.Text = "Recovery failed ❌";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        
        private async Task StartBackendProcess()
        {
            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string pythonScriptPath = System.IO.Path.Combine(exeDirectory, "minimal_server.py");
                
                if (!System.IO.File.Exists(pythonScriptPath))
                {
                    throw new FileNotFoundException($"Python script not found: {pythonScriptPath}");
                }
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pythonw.exe",
                    Arguments = $"\"{pythonScriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                
                _pythonProcess = Process.Start(startInfo);
                
                if (_pythonProcess != null)
                {
                    _logger.Info($"Backend process started with PID: {_pythonProcess.Id}");
                    
                    // Wait a moment for the process to initialize
                    await Task.Delay(3000);
                    
                    if (_pythonProcess.HasExited)
                    {
                        throw new Exception($"Backend process exited immediately with code: {_pythonProcess.ExitCode}");
                    }
                }
                else
                {
                    throw new Exception("Failed to start backend process");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to start backend process", ex);
                throw;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _logger?.Info("Smartitecture application shutting down");
                
                // Stop health monitoring
                _healthMonitor?.StopMonitoring();
                
                // Clean up resources
                _httpClient?.Dispose();
                _healthMonitor?.Dispose();
                
                // Stop Python backend if running
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    try
                    {
                        _logger?.Info("Stopping Python backend process");
                        _pythonProcess.Kill();
                        _pythonProcess.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error("Error stopping Python process", ex);
                    }
                }
                
                // Clean old log files
                _logger?.CleanOldLogs();
                _logger?.Info("Application shutdown complete");
            }
            catch (Exception ex)
            {
                // Fallback logging if logger fails
                Console.WriteLine($"Error during shutdown: {ex.Message}");
            }
            
            base.OnClosed(e);
        }
    }
}
