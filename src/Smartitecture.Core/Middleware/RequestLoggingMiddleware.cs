using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smartitecture.Core.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = true
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log request details
            var request = context.Request;
            var requestLog = new
            {
                Timestamp = DateTime.UtcNow,
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            };

            _logger.LogInformation("Request: {@Request}", requestLog);

            // Log response details
            var originalBodyStream = context.Response.Body;
            using var newBodyStream = new MemoryStream();
            context.Response.Body = newBodyStream;

            await _next(context);

            newBodyStream.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(newBodyStream).ReadToEndAsync();
            newBodyStream.Seek(0, SeekOrigin.Begin);
            await newBodyStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            var responseLog = new
            {
                Timestamp = DateTime.UtcNow,
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType,
                Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                ContentLength = context.Response.ContentLength,
                Content = responseText
            };

            _logger.LogInformation("Response: {@Response}", responseLog);
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
