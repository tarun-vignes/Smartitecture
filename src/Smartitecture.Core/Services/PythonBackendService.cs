using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smartitecture.Core.Services.Base;

namespace Smartitecture.Core.Services
{
    public class PythonBackendService : BackgroundService
    {
        private readonly ILogger<PythonBackendService> _logger;
        private readonly PythonApiService _pythonApiService;
        private Process _pythonProcess;
        private readonly string _pythonBackendPath;

        public PythonBackendService(ILogger<PythonBackendService> logger, PythonApiService pythonApiService)
        {
            _logger = logger;
            _pythonApiService = pythonApiService;
            _pythonBackendPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backend-python");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await StartPythonBackend();
                
                // Wait for the Python API to become healthy
                await WaitForPythonApiHealth(stoppingToken);
                
                _logger.LogInformation("Python backend started successfully");
                
                // Keep the service running
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Python backend service is stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Python backend service");
                throw;
            }
        }

        private async Task StartPythonBackend()
        {
            try
            {
                // Check if Python is already running
                if (await _pythonApiService.IsHealthyAsync())
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
                    if (await _pythonApiService.IsHealthyAsync())
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
