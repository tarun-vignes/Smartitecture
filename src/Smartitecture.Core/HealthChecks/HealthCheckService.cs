using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Smartitecture.Core.HealthChecks
{
    public class HealthCheckService : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IConfiguration _configuration;

        public HealthCheckService(HttpClient httpClient, ILogger<HealthCheckService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check dependencies
                var healthChecks = new List<(string Name, string Status)>();

                // Check OpenAI service
                var openAiConfig = _configuration.GetSection("OpenAI").Get<OpenAIConfig>();
                if (openAiConfig != null)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(openAiConfig.ApiEndpoint, cancellationToken);
                        healthChecks.Add(("OpenAI", response.IsSuccessStatusCode ? "Healthy" : "Unhealthy"));
                    }
                    catch
                    {
                        healthChecks.Add(("OpenAI", "Unhealthy"));
                    }
                }

                // Add more health checks here

                var unhealthyChecks = healthChecks.Where(h => h.Status == "Unhealthy").ToList();
                if (unhealthyChecks.Any())
                {
                    var details = unhealthyChecks.ToDictionary(
                        k => k.Name,
                        v => v.Status
                    );

                    return HealthCheckResult.Unhealthy("Some services are unhealthy", exception: null, details);
                }

                return HealthCheckResult.Healthy("All services are healthy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
