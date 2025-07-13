using Polly;
using Polly.Retry;
using System.Net;

namespace Smartitecture.Core.Services
{
    public interface IRetryService
    {
        T ExecuteWithRetry<T>(Func<T> action, string operationName);
        T ExecuteWithRetry<T>(Func<T> action, string operationName, int maxRetries);
        void ExecuteWithRetry(Action action, string operationName);
        void ExecuteWithRetry(Action action, string operationName, int maxRetries);
    }

    public class RetryService : IRetryService
    {
        private readonly ILogger<RetryService> _logger;
        private readonly IAsyncPolicy _retryPolicy;

        public RetryService(ILogger<RetryService> logger)
        {
            _logger = logger;
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<SmartException>(ex => ex.StatusCode == HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retrying {OperationName} attempt {RetryCount} after {TimeSpan}ms due to {ExceptionType}: {Message}",
                            context.OperationKey,
                            retryCount,
                            timeSpan.TotalMilliseconds,
                            exception.GetType().Name,
                            exception.Message
                        );
                    }
                );
        }

        public T ExecuteWithRetry<T>(Func<T> action, string operationName)
        {
            return _retryPolicy.Execute(() => action());
        }

        public T ExecuteWithRetry<T>(Func<T> action, string operationName, int maxRetries)
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetry(
                    maxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retrying {OperationName} attempt {RetryCount} after {TimeSpan}ms due to {ExceptionType}: {Message}",
                            context.OperationKey,
                            retryCount,
                            timeSpan.TotalMilliseconds,
                            exception.GetType().Name,
                            exception.Message
                        );
                    }
                );

            return policy.Execute(() => action());
        }

        public void ExecuteWithRetry(Action action, string operationName)
        {
            _retryPolicy.Execute(() => action());
        }

        public void ExecuteWithRetry(Action action, string operationName, int maxRetries)
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetry(
                    maxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retrying {OperationName} attempt {RetryCount} after {TimeSpan}ms due to {ExceptionType}: {Message}",
                            context.OperationKey,
                            retryCount,
                            timeSpan.TotalMilliseconds,
                            exception.GetType().Name,
                            exception.Message
                        );
                    }
                );

            policy.Execute(() => action());
        }
    }
}
