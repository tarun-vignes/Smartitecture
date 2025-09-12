using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smartitecture.Core.Exceptions;
using Smartitecture.Core.Models;
using Smartitecture.Core.Options;

namespace Smartitecture.Core.Services
{
    /// <summary>
    /// Service responsible for managing the lifecycle of the Python backend process.
    /// </summary>
    public class PythonBackendService : BackgroundService, IPythonBackendService
    {
        private readonly ILogger<PythonBackendService> _logger;
        private readonly PythonApiOptions _options;
        private readonly IPythonApiService _pythonApiService;
        private Process _pythonProcess;
        private readonly string _pythonBackendPath;
        private readonly string _pythonExecutable;
        private readonly string _apiScriptPath;
        private readonly object _processLock = new object();
        private readonly ConcurrentQueue<string> _outputBuffer = new ConcurrentQueue<string>();
        private bool _isDisposed;

        public PythonBackendService(
            ILogger<PythonBackendService> logger,
            IOptions<PythonApiOptions> options,
            IPythonApiService pythonApiService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _pythonApiService = pythonApiService ?? throw new ArgumentNullException(nameof(pythonApiService));
            
            // Determine the path to the Python backend
            _pythonBackendPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "backend-python");
                
            _pythonBackendPath = Path.GetFullPath(_pythonBackendPath);
            
            // Determine the Python executable to use
            _pythonExecutable = GetPythonExecutable();
            _apiScriptPath = Path.Combine(_pythonBackendPath, "minimal_server.py");
            
            _logger.LogInformation("Python backend service initialized");
            _logger.LogInformation($"Python executable: {_pythonExecutable}");
            _logger.LogInformation($"API script path: {_apiScriptPath}");
        }

        private string GetPythonExecutable()
        {
            // Try to find Python in common locations
            var possiblePaths = new List<string>();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                possiblePaths.Add("python");
                possiblePaths.Add("python3");
                possiblePaths.Add(Path.Combine("C:\\Python39\\python.exe"));
                possiblePaths.Add(Path.Combine("C:\\Python310\\python.exe"));
                possiblePaths.Add(Path.Combine("C:\\Python311\\python.exe"));
                possiblePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python39\\python.exe"));
                possiblePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python310\\python.exe"));
                possiblePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python311\\python.exe"));
                possiblePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Python39-32\\python.exe"));
                possiblePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Python310-32\\python.exe"));
            }
            else
            {
                possiblePaths.Add("python3");
                possiblePaths.Add("python");
                possiblePaths.Add("/usr/bin/python3");
                possiblePaths.Add("/usr/local/bin/python3");
                possiblePaths.Add("/opt/homebrew/bin/python3");
            }
            
            // Try to find an existing Python executable
            foreach (var path in possiblePaths)
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(startInfo);
                    if (process != null && process.WaitForExit(5000))
                    {
                        if (process.ExitCode == 0)
                        {
                            _logger.LogInformation($"Found Python at: {path}");
                            return path;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, $"Failed to find Python at: {path}");
                }
            }
            
            // Default to 'python' and let the OS resolve it
            return "python";
        }
        
        private async Task StartPythonBackendAsync(CancellationToken cancellationToken)
        {
            if (IsRunning)
            {
                _logger.LogInformation("Python backend is already running");
                return;
            }

            try
            {
                _logger.LogInformation("Starting Python backend...");
                
                if (!File.Exists(_pythonExecutable))
                {
                    throw new FileNotFoundException($"Python executable not found at: {_pythonExecutable}");
                }
                
                if (!File.Exists(_apiScriptPath))
                {
                    throw new FileNotFoundException($"API script not found at: {_apiScriptPath}");
                }

                lock (_processLock)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = _pythonExecutable,
                        Arguments = $"\"{_apiScriptPath}\" --port {_options.Port}",
                        WorkingDirectory = _pythonBackendPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };

                    // Set environment variables
                    startInfo.Environment["PYTHONUNBUFFERED"] = "1";
                    startInfo.Environment["PYTHONIOENCODING"] = "utf-8";
                    startInfo.Environment["PYTHONPATH"] = _pythonBackendPath;
                    
                    // Add any additional environment variables from options
                    if (_options.EnvironmentVariables != null)
                    {
                        foreach (var envVar in _options.EnvironmentVariables)
                        {
                            startInfo.Environment[envVar.Key] = envVar.Value;
                        }
                    }

                    _pythonProcess = new Process { StartInfo = startInfo };
                    
                    // Capture output and error streams
                    _pythonProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            _logger.LogInformation($"[Python] {e.Data}");
                            _outputBuffer.Enqueue(e.Data);
                            OnOutputReceived(e.Data);
                        }
                    };
                    
                    _pythonProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            _logger.LogError($"[Python Error] {e.Data}");
                            _outputBuffer.Enqueue($"[ERROR] {e.Data}");
                            OnErrorReceived(e.Data);
                        }
                    };
                    
                    _pythonProcess.Exited += (sender, e) =>
                    {
                        var exitCode = _pythonProcess?.ExitCode ?? -1;
                        _logger.LogWarning($"Python process exited with code: {exitCode}");
                    };

                    _logger.LogInformation($"Starting Python process: {_pythonExecutable} {_apiScriptPath}");
                    
                    if (!_pythonProcess.Start())
                    {
                        throw new InvalidOperationException("Failed to start Python process");
                    }
                    
                    _pythonProcess.BeginOutputReadLine();
                    _pythonProcess.BeginErrorReadLine();
                    
                    _logger.LogInformation($"Python process started with ID: {_pythonProcess.Id}");
                }
                
                // Wait for the API to be ready
                var isHealthy = await WaitForPythonApiHealthAsync(cancellationToken);
                
                if (!isHealthy)
                {
                    throw new PythonBackendException("Failed to start Python backend: Health check failed");
                }
                
                _logger.LogInformation("Python backend is ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Python backend");
                StopPythonBackend();
                throw new PythonBackendException("Failed to start Python backend", ex);
            }
        }
        
        private void StopPythonBackend()
        {
            try
            {
                if (_pythonProcess != null)
                {
                    if (!_pythonProcess.HasExited)
                    {
                        _logger.LogInformation("Stopping Python process...");
                        
                        // Try to terminate gracefully first
                        _pythonProcess.CloseMainWindow();
                        
                        if (!_pythonProcess.WaitForExit(5000))
                        {
                            _logger.LogWarning("Python process did not exit gracefully, forcing termination...");
                            _pythonProcess.Kill(entireProcessTree: true);
                        }
                    }
                    
                    _pythonProcess.Dispose();
                    _pythonProcess = null;
                    _logger.LogInformation("Python process stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Python process");
            }
        }
        
        private async Task<bool> WaitForPythonApiHealthAsync(CancellationToken cancellationToken, int maxAttempts = 10, int delayMs = 1000)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            
            var healthCheckUrl = new Uri(new Uri(_options.BaseUrl), "health");
            
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                
                try
                {
                    var response = await httpClient.GetAsync(healthCheckUrl, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Python API is healthy");
                        return true;
                    }
                    
                    _logger.LogWarning($"Health check attempt {attempt}/{maxAttempts} failed with status: {response.StatusCode}");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogDebug(ex, $"Health check attempt {attempt}/{maxAttempts} failed");
                }
                
                if (attempt < maxAttempts)
                {
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
            
            _logger.LogError($"Python API did not become healthy after {maxAttempts} attempts");
            return false;
        }
        
        public bool IsRunning => _pythonProcess != null && !_pythonProcess.HasExited;
        
        public int? ProcessId => _pythonProcess?.Id;

        public event EventHandler<ProcessOutputEventArgs> OutputReceived;
        public event EventHandler<ProcessOutputEventArgs> ErrorReceived;
        
        protected virtual void OnOutputReceived(string data) => 
            OutputReceived?.Invoke(this, new ProcessOutputEventArgs(data));
            
        protected virtual void OnErrorReceived(string data) => 
            ErrorReceived?.Invoke(this, new ProcessOutputEventArgs(data));
        
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
            {
                _logger.LogInformation("Python backend is already running");
                return;
            }
            
            await StartPythonBackendAsync(cancellationToken);
        }
        
        public async Task RestartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Restarting Python backend...");
            
            if (IsRunning)
            {
                StopPythonBackend();
                // Small delay to ensure process is fully terminated
                await Task.Delay(1000, cancellationToken);
            }
            
            await StartPythonBackendAsync(cancellationToken);
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Python backend service");
            
            try
            {
                await StartPythonBackendAsync(stoppingToken);
                
                // Keep the service running until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                    
                    // Check if the Python process is still running
                    if (_pythonProcess == null || _pythonProcess.HasExited)
                    {
                        _logger.LogWarning("Python process is not running. Attempting to restart...");
                        await StartPythonBackendAsync(stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // This is expected when the service is being stopped
                _logger.LogInformation("Python backend service is stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the Python backend service");
                throw;
            }
            finally
            {
                // Ensure the Python process is terminated when the service stops
                StopPythonBackend();
            }
        }

        private async Task StartPythonBackend()
        {
            try
            {
                // Check if Python is already running
                if (await _pythonApiService.CheckHealthAsync())
                {
                    _logger.LogInformation("Python backend is already running");
                    return;
                }

                var startScript = Path.Combine(_pythonBackendPath, "start_api.py");
                
                if (!File.Exists(startScript))
                {
                    throw new FileNotFoundException($"Python backend script not found: {startScript}");
                }

                _logger.LogInformation("Starting Python backend...");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = startScript,
                    WorkingDirectory = _pythonBackendPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _pythonProcess = new Process { StartInfo = startInfo };
                
                _pythonProcess.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        _logger.LogInformation("Python: {Output}", args.Data);
                };
                
                _pythonProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        _logger.LogError("Python Error: {Error}", args.Data);
                };

                _pythonProcess.Start();
                _pythonProcess.BeginOutputReadLine();
                _pythonProcess.BeginErrorReadLine();

                _logger.LogInformation("Python backend process started with PID: {ProcessId}", _pythonProcess.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Python backend");
                throw;
            }
        }

        private async Task WaitForPythonApiHealth(CancellationToken cancellationToken)
        {
            const int maxRetries = 30;
            const int retryDelayMs = 1000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (await _pythonApiService.CheckHealthAsync())
                    {
                        _logger.LogInformation("Python API is healthy");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Python API health check failed, retrying... ({Retry}/{MaxRetries})", i + 1, maxRetries);
                }

                await Task.Delay(retryDelayMs, cancellationToken);
            }

            throw new TimeoutException("Python API failed to become healthy within the timeout period");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Python backend service...");

            if (_pythonProcess != null && !_pythonProcess.HasExited)
            {
                try
                {
                    _pythonProcess.Kill();
                    await _pythonProcess.WaitForExitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping Python process");
                }
                finally
                {
                    _pythonProcess?.Dispose();
                }
            }

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _pythonProcess?.Dispose();
            base.Dispose();
        }
    }
}
