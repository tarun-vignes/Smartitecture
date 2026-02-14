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
}
