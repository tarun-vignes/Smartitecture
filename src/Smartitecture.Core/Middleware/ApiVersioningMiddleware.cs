using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Smartitecture.Core.Middleware
{
    public class ApiVersioningMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiVersioningMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public ApiVersioningMiddleware(RequestDelegate next, ILogger<ApiVersioningMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get API version from configuration
            var apiVersion = _configuration.GetValue<string>("Api:Version") ?? "v1";

            // Check for version in URL
            var path = context.Request.Path.Value;
            if (!path.StartsWith($"/api/{apiVersion}", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "API version not found",
                    supportedVersions = new[] { apiVersion }
                });
                return;
            }

            // Add version headers
            context.Response.Headers.Add("X-API-Version", apiVersion);
            context.Response.Headers.Add("X-API-Supported-Versions", apiVersion);

            await _next(context);
        }
    }

    public static class ApiVersioningMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiVersioning(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiVersioningMiddleware>();
        }
    }
}
