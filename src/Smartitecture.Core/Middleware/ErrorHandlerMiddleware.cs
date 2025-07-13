using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Smartitecture.Core.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";
            var errorCode = "GENERIC_ERROR";
            var details = Array.Empty<string>();

            switch (ex)
            {
                case SmartException smartEx:
                    code = smartEx.StatusCode;
                    message = smartEx.Message;
                    errorCode = smartEx.ErrorCode;
                    details = smartEx.Details ?? Array.Empty<string>();
                    break;

                case ValidationException validationEx:
                    code = HttpStatusCode.BadRequest;
                    message = validationEx.Message;
                    errorCode = "VALIDATION_ERROR";
                    details = validationEx.Details ?? Array.Empty<string>();
                    break;

                case NotFoundException:
                    code = HttpStatusCode.NotFound;
                    message = "The requested resource was not found.";
                    errorCode = "NOT_FOUND";
                    break;

                case UnauthorizedException:
                    code = HttpStatusCode.Unauthorized;
                    message = "Unauthorized access.";
                    errorCode = "UNAUTHORIZED";
                    break;

                case ForbiddenException:
                    code = HttpStatusCode.Forbidden;
                    message = "Forbidden access.";
                    errorCode = "FORBIDDEN";
                    break;
            }

            var errorResponse = new ErrorResponse(
                message,
                errorCode,
                details,
                (int)code,
                context.Request.Path.Value ?? ""
            );

            _logger.LogError(ex, "Error occurred: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }

    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
