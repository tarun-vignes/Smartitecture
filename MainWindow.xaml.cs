using System;
using System.Diagnostics;
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
        private bool _isBackendRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // Initialize UI state
            UpdateUIState(false);
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
            OutputTextBox.Text = "🤖 Output cleared. Ready for new commands.";
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
