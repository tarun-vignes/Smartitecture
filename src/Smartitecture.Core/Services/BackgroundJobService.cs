using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Smartitecture.Core.Services
{
    public interface IBackgroundJobService
    {
        Task EnqueueJobAsync(Func<Task> job);
        Task EnqueueJobAsync(Func<CancellationToken, Task> job);
    }

    public class BackgroundJobService : BackgroundService, IBackgroundJobService
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IRetryService _retryService;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _jobs = new();

        public BackgroundJobService(ILogger<BackgroundJobService> logger, IRetryService retryService)
        {
            _logger = logger;
            _retryService = retryService;
        }

        public async Task EnqueueJobAsync(Func<Task> job)
        {
            await EnqueueJobAsync(async (ct) => await job());
        }

        public async Task EnqueueJobAsync(Func<CancellationToken, Task> job)
        {
            await _semaphore.WaitAsync();
            try
            {
                _jobs.Enqueue(job);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting background job service");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_jobs.TryDequeue(out var job))
                {
                    try
                    {
                        await _retryService.ExecuteWithRetry(
                            () => job(stoppingToken),
                            "BackgroundJob"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing background job");
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }

            _logger.LogInformation("Background job service stopped");
        }
    }
}
