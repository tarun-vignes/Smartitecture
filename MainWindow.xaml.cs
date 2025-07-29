using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SmartitectureSimple
{
    public partial class MainWindow : Window
    {
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
                    WorkingDirectory = exeDirectory
                };

                _pythonProcess = Process.Start(startInfo);
                
                if (_pythonProcess == null)
                {
                    throw new Exception("Failed to start Python process");
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
            try
            {
                RunAgentButton.IsEnabled = false;
                OutputTextBox.Text = "Processing request...";

                string input = InputTextBox.Text;
                if (string.IsNullOrWhiteSpace(input))
                {
                    input = "Test request from simplified Smartitecture";
                }

                var requestData = new
                {
                    input = input,
                    max_iterations = 1
                };

                string jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/agent/run", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    string resultText = result.GetProperty("result").GetString() ?? "No result";
                    
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
