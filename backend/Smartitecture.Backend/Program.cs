using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("openai", client =>
{
    client.Timeout = TimeSpan.FromSeconds(45);
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                message = "Too many requests. Please wait and try again.",
                code = "rate_limited",
                status = StatusCodes.Status429TooManyRequests
            }
        });
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = BackendHelpers.GetRateLimitKey(context);
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

var app = builder.Build();

app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "smartitecture-backend",
    time = DateTimeOffset.UtcNow
}));

app.MapPost("/v1/chat", async (HttpRequest request, IHttpClientFactory httpFactory) =>
{
    var config = BackendConfig.Load();
    if (string.IsNullOrWhiteSpace(config.OpenAIApiKey))
    {
        return BackendHelpers.ErrorResponse("OPENAI_API_KEY is not configured on the backend.", StatusCodes.Status500InternalServerError, "backend_missing_key");
    }

    if (!string.IsNullOrWhiteSpace(config.ApiKey))
    {
        var authHeader = request.Headers.Authorization.ToString();
        var headerKey = request.Headers["X-API-Key"].ToString();
        var bearerToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        if (!string.Equals(headerKey, config.ApiKey, StringComparison.Ordinal) &&
            !string.Equals(bearerToken, config.ApiKey, StringComparison.Ordinal))
        {
            return BackendHelpers.ErrorResponse("Invalid API key.", StatusCodes.Status401Unauthorized, "invalid_api_key");
        }
    }

    var payload = await JsonSerializer.DeserializeAsync<BackendChatRequest>(request.Body, Json.Options);
    if (payload is null || payload.Messages.Count == 0)
    {
        return BackendHelpers.ErrorResponse("Missing messages.", StatusCodes.Status400BadRequest, "missing_messages");
    }

    var upstream = OpenAIRequest.FromBackend(payload, config);
    var json = JsonSerializer.Serialize(upstream, Json.Options);

    using var client = httpFactory.CreateClient("openai");
    using var message = new HttpRequestMessage(HttpMethod.Post, config.OpenAIEndpoint);
    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.OpenAIApiKey);
    message.Content = new StringContent(json, Encoding.UTF8, "application/json");

    using var response = await client.SendAsync(message);
    var responseJson = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        return BackendHelpers.ErrorResponse($"Upstream error: {responseJson}", (int)response.StatusCode, "upstream_error");
    }

    var backendResponse = OpenAIResponse.ToBackend(responseJson);
    return Results.Ok(backendResponse);
});

app.MapPost("/v1/chat/stream", async (HttpRequest request, HttpResponse response, IHttpClientFactory httpFactory) =>
{
    var config = BackendConfig.Load();
    if (string.IsNullOrWhiteSpace(config.OpenAIApiKey))
    {
        response.StatusCode = StatusCodes.Status500InternalServerError;
        await BackendHelpers.WriteSseErrorAsync(response, "OPENAI_API_KEY is not configured on the backend.", "backend_missing_key");
        return;
    }

    if (!string.IsNullOrWhiteSpace(config.ApiKey))
    {
        var authHeader = request.Headers.Authorization.ToString();
        var headerKey = request.Headers["X-API-Key"].ToString();
        var bearerToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring("Bearer ".Length).Trim()
            : string.Empty;

        if (!string.Equals(headerKey, config.ApiKey, StringComparison.Ordinal) &&
            !string.Equals(bearerToken, config.ApiKey, StringComparison.Ordinal))
        {
            response.StatusCode = StatusCodes.Status401Unauthorized;
            await BackendHelpers.WriteSseErrorAsync(response, "Invalid API key.", "invalid_api_key");
            return;
        }
    }

    var payload = await JsonSerializer.DeserializeAsync<BackendChatRequest>(request.Body, Json.Options);
    if (payload is null || payload.Messages.Count == 0)
    {
        response.StatusCode = StatusCodes.Status400BadRequest;
        await BackendHelpers.WriteSseErrorAsync(response, "Missing messages.", "missing_messages");
        return;
    }

    response.Headers.CacheControl = "no-cache";
    response.Headers.ContentType = "text/event-stream";
    response.Headers.Connection = "keep-alive";

    var upstream = OpenAIRequest.FromBackend(payload, config, stream: true);
    var json = JsonSerializer.Serialize(upstream, Json.Options);

    using var client = httpFactory.CreateClient("openai");
    using var message = new HttpRequestMessage(HttpMethod.Post, config.OpenAIEndpoint);
    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.OpenAIApiKey);
    message.Content = new StringContent(json, Encoding.UTF8, "application/json");

    using var upstreamResponse = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
    if (!upstreamResponse.IsSuccessStatusCode)
    {
        var responseJson = await upstreamResponse.Content.ReadAsStringAsync();
        response.StatusCode = (int)upstreamResponse.StatusCode;
        await BackendHelpers.WriteSseErrorAsync(response, $"Upstream error: {responseJson}", "upstream_error");
        return;
    }

    await using var stream = await upstreamResponse.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(stream);
    var toolCalls = new Dictionary<int, ToolCallAccumulator>();

    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var data = line.Substring("data:".Length).Trim();
        if (data.Equals("[DONE]", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        if (string.IsNullOrWhiteSpace(data))
        {
            continue;
        }

        try
        {
            using var doc = JsonDocument.Parse(data);
            var choice = doc.RootElement.GetProperty("choices")[0];
            if (!choice.TryGetProperty("delta", out var delta))
            {
                continue;
            }

            if (delta.TryGetProperty("content", out var content))
            {
                var token = content.GetString();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    await BackendHelpers.WriteSseAsync(response, new { token });
                }
            }

            if (delta.TryGetProperty("tool_calls", out var toolCallsElement))
            {
                foreach (var tool in toolCallsElement.EnumerateArray())
                {
                    var index = tool.TryGetProperty("index", out var idx) ? idx.GetInt32() : 0;
                    if (!toolCalls.TryGetValue(index, out var accumulator))
                    {
                        accumulator = new ToolCallAccumulator();
                        toolCalls[index] = accumulator;
                    }

                    if (tool.TryGetProperty("id", out var idElement))
                    {
                        accumulator.Id = idElement.GetString();
                    }

                    if (tool.TryGetProperty("function", out var function))
                    {
                        if (function.TryGetProperty("name", out var nameElement))
                        {
                            accumulator.Name ??= nameElement.GetString();
                        }

                        if (function.TryGetProperty("arguments", out var argsElement))
                        {
                            accumulator.Arguments.Append(argsElement.GetString());
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore parse issues
        }
    }

    var finalToolCalls = toolCalls
        .OrderBy(kvp => kvp.Key)
        .Select(kvp => kvp.Value.ToToolCall())
        .Where(tc => tc != null)
        .Cast<BackendToolCall>()
        .ToList();

    await BackendHelpers.WriteSseAsync(response, new
    {
        done = true,
        toolCalls = finalToolCalls
    });
});

app.Run();
