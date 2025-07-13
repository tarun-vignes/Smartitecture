using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace Smartitecture.Core.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly SemaphoreSlim _semaphore = new(100, 100); // 100 concurrent requests

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Check if we can acquire a semaphore
                var canProceed = await _semaphore.WaitAsync(TimeSpan.FromSeconds(1));
                
                if (!canProceed)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Too many requests. Please try again later.",
                        retryAfter = 1
                    });
                    return;
                }

                // Add rate limit headers
                context.Response.Headers.Add("X-RateLimit-Limit", "100");
                context.Response.Headers.Add("X-RateLimit-Remaining", _semaphore.CurrentCount.ToString());
                context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddSeconds(1).ToUnixTimeSeconds().ToString());

                // Call the next middleware
                await _next(context);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
