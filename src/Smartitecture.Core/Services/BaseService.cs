using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.Core.Services
{
    public abstract class BaseService : IBaseService
    {
        protected readonly ILogger Logger;
        protected readonly IServiceProvider ServiceProvider;

        protected BaseService(ILogger logger, IServiceProvider serviceProvider)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
        }

        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        public virtual async Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        public async Task<TResponse> ExecuteAsync<TResponse>(
            Func<Task<TResponse>> action,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing service action");
                throw;
            }
        }
    }
}
