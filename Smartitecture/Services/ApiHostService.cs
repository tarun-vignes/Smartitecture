using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smartitecture.API;
using Smartitecture.Commands;

namespace Smartitecture.Services
{
    public class ApiHostService : IHostedService, IDisposable
    {
        private readonly ILLMService _llmService;
        private readonly CommandMapper _commandMapper;
        private readonly ILogger<ApiHostService> _logger;
        private ApiHost _apiHost;
        private bool _disposed = false;

        public ApiHostService(
            ILLMService llmService,
            CommandMapper commandMapper,
            ILogger<ApiHostService> logger)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _commandMapper = commandMapper ?? throw new ArgumentNullException(nameof(commandMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting API host...");
                
                _apiHost = new ApiHost(_llmService, _commandMapper);
                await _apiHost.StartAsync();
                
                _logger.LogInformation("API host started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start API host");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_apiHost != null)
                {
                    _logger.LogInformation("Stopping API host...");
                    await _apiHost.StopAsync();
                    _logger.LogInformation("API host stopped successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping API host");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _apiHost?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
