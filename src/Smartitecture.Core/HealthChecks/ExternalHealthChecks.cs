using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Smartitecture.Core.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly SmartitectureDbContext _context;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(SmartitectureDbContext context, ILogger<DatabaseHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Test database connection by executing a simple query
                await _context.Database.CanConnectAsync(cancellationToken);
                var count = await _context.Users.CountAsync(cancellationToken);

                _logger.LogInformation("Database health check passed. User count: {UserCount}", count);
                return HealthCheckResult.Healthy($"Database connection successful. User count: {count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }

                // Test database connection
                // var connection = new SqlConnection(connectionString);
                // await connection.OpenAsync(cancellationToken);
                // await connection.CloseAsync();

                return HealthCheckResult.Healthy("Database connection successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }

    public class ExternalServiceHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalServiceHealthCheck> _logger;

        public ExternalServiceHealthCheck(HttpClient httpClient, IConfiguration configuration, ILogger<ExternalServiceHealthCheck> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check OpenAI service
                var openAiConfig = _configuration.GetSection("OpenAI").Get<OpenAIConfig>();
                if (string.IsNullOrEmpty(openAiConfig.ApiKey))
                {
                    return HealthCheckResult.Unhealthy("OpenAI API key not configured");
                }

                var response = await _httpClient.GetAsync(openAiConfig.ApiEndpoint, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Unhealthy($"OpenAI service returned status code: {response.StatusCode}");
                }

                return HealthCheckResult.Healthy("External services are healthy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "External service health check failed");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
