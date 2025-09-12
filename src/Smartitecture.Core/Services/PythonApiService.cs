using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Smartitecture.Core.Exceptions;
using Smartitecture.Core.Models;
using Smartitecture.Core.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smartitecture.Core.Services
{
    public class PythonApiService : IPythonApiService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PythonApiService> _logger;
        private readonly PythonApiOptions _options;
        private readonly string _baseUrl;
        private readonly Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        private bool _disposed;

        public PythonApiService(
            HttpClient httpClient,
            IOptions<PythonApiOptions> options,
            ILogger<PythonApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = _options.BaseUrl ?? "http://127.0.0.1:8001";

            // Configure retry policy
            _retryPolicy = Polly.Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: _options.RetryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, delay, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} after {Delay}ms due to: {Exception}",
                            retryCount,
                            delay.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    });

            // Configure circuit breaker policy
            _circuitBreakerPolicy = Polly.Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        _logger.LogError(
                            "Circuit broken! Will remain open for {BreakDelay}ms due to: {Exception}",
                            breakDelay.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    },
                    onReset: () => _logger.LogInformation("Circuit breaker reset"),
                    onHalfOpen: () => _logger.LogInformation("Circuit breaker half-open"));
        }

        public async Task<bool> CheckHealthAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            try
            {
                var response = await ExecuteWithResilienceAsync(
                    () => _httpClient.GetAsync("health", cts.Token),
                    cts.Token);
                
                var isHealthy = response.IsSuccessStatusCode;
                _logger.LogDebug("Health check {Status} with status code {StatusCode}", 
                    isHealthy ? "succeeded" : "failed", response.StatusCode);
                    
                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return false;
            }
        }

        public async Task<AgentStateResponse> GetAgentStateAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            
            try
            {
                var response = await ExecuteWithResilienceAsync(
                    () => _httpClient.GetAsync("agent/state", cts.Token),
                    cts.Token);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                var result = JsonSerializer.Deserialize<AgentStateResponse>(
                    content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                _logger.LogDebug("Retrieved agent state: {State}", result?.State);
                return result;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Circuit is open. Service is unavailable.");
                throw new ServiceUnavailableException("Service is currently unavailable. Please try again later.", ex);
            }
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                _logger.LogError(ex, "HTTP error {StatusCode} while getting agent state", ex.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting agent state");
                throw new PythonApiException("An error occurred while getting agent state", ex);
            }
        }

        public async Task<AgentRunResponse> RunAgentAsync(string input, int? maxIterations = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or whitespace", nameof(input));
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            
            try
            {
                var request = new AgentRunRequest
                {
                    Input = input,
                    MaxIterations = maxIterations
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await ExecuteWithResilienceAsync(
                    () => _httpClient.PostAsync("agent/run", content, cts.Token),
                    cts.Token);

                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AgentRunResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                _logger.LogInformation("Agent run completed in {Iterations} iterations", result?.Iterations);
                return result;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Circuit is open. Service is unavailable.");
                throw new ServiceUnavailableException("Service is currently unavailable. Please try again later.", ex);
            }
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                _logger.LogError(ex, "HTTP error {StatusCode} while running agent", ex.StatusCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error running agent with input: {Input}", input);
                throw new PythonApiException("An error occurred while running the agent", ex);
            }
        }

        private async Task<HttpResponseMessage> ExecuteWithResilienceAsync(
            Func<Task<HttpResponseMessage>> action,
            CancellationToken cancellationToken)
        {
            // Combine retry and circuit breaker policies
            var policy = Policy.WrapAsync((Polly.IAsyncPolicy)_retryPolicy, (Polly.IAsyncPolicy)_circuitBreakerPolicy);
            
            return await policy.ExecuteAsync(async (ct) =>
            {
                var response = await action();
                
                // Log rate limiting headers if present
                if (response.Headers.TryGetValues("X-RateLimit-Limit", out var limitValues) &&
                    response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues))
                {
                    _logger.LogDebug("Rate limit: {Remaining}/{Limit} requests remaining",
                        string.Join(",", remainingValues),
                        string.Join(",", limitValues));
                }
                
                return response;
            }, cancellationToken);
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
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        public async Task<string> GetConfigurationAsync(string key)
        {
            var response = await GetAsync<dynamic>($"/api/config/{key}");
            return response?.value?.ToString();
        }

        public async Task SetConfigurationAsync(string key, string value)
        {
            var data = new { key, value };
            await PostAsync<dynamic>("/api/config", data);
        }

        public async Task<string> ProcessTextAsync(string inputText, string model = "gpt-4")
        {
            var data = new { input_text = inputText, model };
            var response = await PostAsync<dynamic>("/api/process", data);
            return response?.output_text?.ToString();
        }

        public async Task<string> ReadFileAsync(string path)
        {
            var data = new { path };
            var response = await PostAsync<dynamic>("/api/file/read", data);
            return response?.content?.ToString();
        }

        public async Task WriteFileAsync(string path, string content)
        {
            var data = new { path, content };
            await PostAsync<dynamic>("/api/file/write", data);
        }

        public async Task<bool> DeleteFileAsync(string path)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/file/{path}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {Path}", path);
                return false;
            }
        }

        private async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to GET from Python API: {Endpoint}", endpoint);
                throw;
            }
        }

        private async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to POST to Python API: {Endpoint}", endpoint);
                throw;
            }
        }

        protected async Task<bool> ExecuteAsync()
        {
            // Check if Python API is running
            return await CheckHealthAsync();
        }
    }
}
