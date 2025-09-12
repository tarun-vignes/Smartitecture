using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Smartitecture.Core.Models
{
    /// <summary>
    /// Placeholder types for Polly policies until proper Polly package is configured
    /// </summary>
    public class AsyncRetryPolicy<T>
    {
        public static AsyncRetryPolicy<T> Handle<TException>() where TException : Exception
        {
            return new AsyncRetryPolicy<T>();
        }

        public AsyncRetryPolicy<T> OrResult(Func<T, bool> predicate)
        {
            return this;
        }

        public AsyncRetryPolicy<T> WaitAndRetryAsync(int retryCount, Func<int, TimeSpan> sleepDurationProvider, Action<DelegateResult<T>, TimeSpan, int, Context> onRetry = null)
        {
            return this;
        }

        public async Task<T> ExecuteAsync(Func<Task<T>> action)
        {
            return await action();
        }
    }

    public class AsyncCircuitBreakerPolicy<T>
    {
        public static AsyncCircuitBreakerPolicy<T> Handle<TException>() where TException : Exception
        {
            return new AsyncCircuitBreakerPolicy<T>();
        }

        public AsyncCircuitBreakerPolicy<T> OrResult(Func<T, bool> predicate)
        {
            return this;
        }

        public AsyncCircuitBreakerPolicy<T> CircuitBreakerAsync(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak, Action<DelegateResult<T>, TimeSpan> onBreak = null, Action onReset = null, Action onHalfOpen = null)
        {
            return this;
        }

        public async Task<T> ExecuteAsync(Func<Task<T>> action)
        {
            return await action();
        }
    }

    public class DelegateResult<T>
    {
        public Exception Exception { get; set; }
        public T Result { get; set; }
    }

    public class Context
    {
    }

    public static class Policy<T>
    {
        public static AsyncRetryPolicy<T> Handle<TException>() where TException : Exception
        {
            return AsyncRetryPolicy<T>.Handle<TException>();
        }
    }
}
