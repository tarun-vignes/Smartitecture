using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Serilog;
using Smartitecture.Core.Exceptions;
using Smartitecture.Core.Models;
using Smartitecture.Core.Options;
using Smartitecture.Core.Services;
using Smartitecture.Core.Services.Base;

namespace Smartitecture.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures the core services required by the Smartitecture application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddSmartitectureCore(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
                
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Configure options
            services.Configure<PythonApiOptions>(
                configuration.GetSection(PythonApiOptions.SectionName));

            // Register the Python backend service
            services.AddSingleton<IPythonBackendService, PythonBackendService>();
            services.AddHostedService(provider => (PythonBackendService)provider.GetRequiredService<IPythonBackendService>());
            
            // Configure HTTP client with retry and circuit breaker policies
            services.AddHttpClient<IPythonApiService, PythonApiService>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PythonApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                
                // Add any additional headers from configuration
                if (options.Headers != null)
                {
                    foreach (var header in options.Headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            })
            .AddPolicyHandler((provider, request) => GetRetryPolicy(provider))
            .AddPolicyHandler((provider, request) => GetCircuitBreakerPolicy(provider))
            .AddHttpMessageHandler(provider => new LoggingHttpMessageHandler(
                provider.GetRequiredService<ILogger<LoggingHttpMessageHandler>>()));
            
            // Register other core services with proper lifetime scoping
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(dispose: true);
            });
            
            return services;
        }

        /// <summary>
        /// Creates a retry policy that will wait and retry on transient failures.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve services.</param>
        /// <returns>An <see cref="IAsyncPolicy{HttpResponseMessage}"/> that defines the retry policy.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<PythonApiService>>();
            var options = serviceProvider.GetRequiredService<IOptions<PythonApiOptions>>().Value;
            
            // Create a jittered backoff to prevent thundering herd
            var delay = Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromMilliseconds(200),
                retryCount: options.RetryCount);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    delay,
                    (outcome, timeSpan, retryAttempt, context) =>
                    {
                        logger?.LogWarning(
                            "Retry {RetryAttempt} after {TotalMilliseconds}ms due to: {Exception}",
                            retryAttempt,
                            timeSpan.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    });
        }

        /// <summary>
        /// Creates a circuit breaker policy that will break the circuit after a number of failures.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve services.</param>
        /// <returns>An <see cref="IAsyncPolicy{HttpResponseMessage}"/> that defines the circuit breaker policy.</returns>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<PythonApiService>>();
            
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        logger?.LogError(
                            "Circuit broken! Will remain open for {BreakDelay}ms due to: {Exception}",
                            breakDelay.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    },
                    onReset: () => logger?.LogInformation("Circuit breaker reset"),
                    onHalfOpen: () => logger?.LogInformation("Circuit breaker half-open"));
        }
    }
}
