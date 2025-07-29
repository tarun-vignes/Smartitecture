using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SmartitectureSimple
{
    public partial class MainWindow : Window
    {
        // Windows API for hiding console windows
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        private Process? _pythonProcess;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://127.0.0.1:8001";

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
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
            // Declare isMultiStep outside try block for finally block access
            bool isMultiStep = false;
            
            try
            {
                RunAgentButton.IsEnabled = false;
                OutputTextBox.Text = "Processing request...";

                string input = InputTextBox.Text;
                if (string.IsNullOrWhiteSpace(input))
                {
                    input = "Test request from simplified Smartitecture";
                }

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
                    
                    OutputTextBox.Text = $"✅ Success!\n\nInput: {input}\n\nResult: {resultText}\n\nResponse: {responseContent}";
                }
                else
                {
                    OutputTextBox.Text = $"❌ Error: {response.StatusCode}\n\nResponse: {responseContent}";
                }
            }
            catch (Exception ex)
            {
                OutputTextBox.Text = $"❌ Exception: {ex.Message}";
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

        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources
            if (_pythonProcess != null && !_pythonProcess.HasExited)
            {
                _pythonProcess.Kill();
                _pythonProcess = null;
            }
            
            _httpClient?.Dispose();
            base.OnClosed(e);
        }
    }
}
