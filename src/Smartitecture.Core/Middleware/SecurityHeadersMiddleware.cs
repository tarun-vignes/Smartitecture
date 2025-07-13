using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Smartitecture.Core.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            
            // Add CSP (Content Security Policy)
            var csp = "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                     "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                     "img-src 'self' data: https:; " +
                     "connect-src 'self' https://api.openai.com; " +
                     "font-src 'self' https://fonts.gstatic.com; " +
                     "object-src 'none'; " +
                     "media-src 'self' https:; " +
                     "frame-src 'none'; " +
                     "child-src 'none'; " +
                     "form-action 'self'; " +
                     "base-uri 'self'; " +
                     "manifest-src 'self'; " +
                     "worker-src 'self' blob:; " +
                     "frame-ancestors 'none'; " +
                     "plugin-types application/pdf; " +
                     "report-uri /csp-report-endpoint/; " +
                     "upgrade-insecure-requests; " +
                     "block-all-mixed-content; " +
                     "require-trusted-types-for 'script'; " +
                     "trusted-types default; " +
                     "policy-uri /csp-policy.json";

            context.Response.Headers.Add("Content-Security-Policy", csp);

            await _next(context);
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
