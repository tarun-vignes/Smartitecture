using System.Text.Json;

internal static class BackendHelpers
{
    public static IResult ErrorResponse(string message, int statusCode, string code)
    {
        return Results.Json(new
        {
            error = new
            {
                message,
                code,
                status = statusCode
            }
        }, statusCode: statusCode);
    }

    public static async Task WriteSseAsync(HttpResponse response, object payload)
    {
        var json = JsonSerializer.Serialize(payload, Json.Options);
        await response.WriteAsync($"data: {json}\n\n");
        await response.Body.FlushAsync();
    }

    public static async Task WriteSseErrorAsync(HttpResponse response, string message, string code)
    {
        await WriteSseAsync(response, new
        {
            error = new
            {
                message,
                code
            }
        });
    }

    public static IResult? ValidateApiKey(HttpRequest request, BackendConfig config)
    {
        if (config.RequireApiKey && string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return ErrorResponse(
                "SMARTITECTURE_BACKEND_API_KEY is required when the backend is running in Production.",
                StatusCodes.Status500InternalServerError,
                "backend_api_key_required");
        }

        if (string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return null;
        }

        return RequestHasApiKey(request, config.ApiKey)
            ? null
            : ErrorResponse("Invalid API key.", StatusCodes.Status401Unauthorized, "invalid_api_key");
    }

    public static async Task<bool> ValidateApiKeyForSseAsync(HttpRequest request, HttpResponse response, BackendConfig config)
    {
        if (config.RequireApiKey && string.IsNullOrWhiteSpace(config.ApiKey))
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteSseErrorAsync(
                response,
                "SMARTITECTURE_BACKEND_API_KEY is required when the backend is running in Production.",
                "backend_api_key_required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return true;
        }

        if (RequestHasApiKey(request, config.ApiKey))
        {
            return true;
        }

        response.StatusCode = StatusCodes.Status401Unauthorized;
        await WriteSseErrorAsync(response, "Invalid API key.", "invalid_api_key");
        return false;
    }

    public static IEnumerable<string> TokenizeForStream(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            yield break;
        }

        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < words.Length; i++)
        {
            yield return i == 0 ? words[i] : " " + words[i];
        }
    }

    public static string GetRateLimitKey(HttpContext context)
    {
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return $"key:{apiKey}";
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return $"key:{authHeader.Substring("Bearer ".Length).Trim()}";
        }

        return $"ip:{context.Connection.RemoteIpAddress}";
    }

    private static bool RequestHasApiKey(HttpRequest request, string expectedApiKey)
    {
        var authHeader = request.Headers.Authorization.ToString();
        var headerKey = request.Headers["X-API-Key"].ToString();
        var bearerToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        return string.Equals(headerKey, expectedApiKey, StringComparison.Ordinal) ||
               string.Equals(bearerToken, expectedApiKey, StringComparison.Ordinal);
    }
}
