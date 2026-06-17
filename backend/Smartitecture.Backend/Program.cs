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
    if (BackendHelpers.ValidateApiKey(request, config) is { } apiKeyError)
    {
        return apiKeyError;
    }

    if (config.UsesGemini && string.IsNullOrWhiteSpace(config.GeminiApiKey))
    {
        return BackendHelpers.ErrorResponse("GEMINI_API_KEY is not configured on the backend.", StatusCodes.Status500InternalServerError, "backend_missing_key");
    }

    if (config.UsesOpenAI && string.IsNullOrWhiteSpace(config.OpenAIApiKey))
    {
        return BackendHelpers.ErrorResponse("OPENAI_API_KEY is not configured on the backend.", StatusCodes.Status500InternalServerError, "backend_missing_key");
    }

    var payload = await JsonSerializer.DeserializeAsync<BackendChatRequest>(request.Body, Json.Options);
    if (payload is null || payload.Messages.Count == 0)
    {
        return BackendHelpers.ErrorResponse("Missing messages.", StatusCodes.Status400BadRequest, "missing_messages");
    }

    if (config.UsesGemini && CurrentFactAnswers.TryAnswer(payload, DateTimeOffset.Now) is { } currentFactAnswer)
    {
        return Results.Ok(currentFactAnswer);
    }

    using var client = httpFactory.CreateClient("openai");

    if (config.UsesGemini)
    {
        var geminiModel = GeminiRequest.ResolveModel(payload.Model, config);
        var geminiRequest = GeminiRequest.FromBackend(payload, config);
        var geminiJson = JsonSerializer.Serialize(geminiRequest, Json.Options);
        var geminiUrl = $"{config.GeminiEndpointBase.TrimEnd('/')}/{geminiModel}:generateContent";

        using var geminiMessage = new HttpRequestMessage(HttpMethod.Post, geminiUrl);
        geminiMessage.Headers.Add("x-goog-api-key", config.GeminiApiKey);
        geminiMessage.Content = new StringContent(geminiJson, Encoding.UTF8, "application/json");

        using var geminiResponse = await client.SendAsync(geminiMessage);
        var geminiResponseJson = await geminiResponse.Content.ReadAsStringAsync();

        if (!geminiResponse.IsSuccessStatusCode)
        {
            if ((int)geminiResponse.StatusCode == StatusCodes.Status429TooManyRequests &&
                GeminiRequest.RequiresSearchGrounding(payload))
            {
                return Results.Ok(new BackendChatResponse(
                    "I need live web grounding to answer that reliably, but the Gemini search/quota limit is exhausted for this API key right now. Try again later or enable billing/quota for grounded search.",
                    new List<BackendToolCall>()));
            }

            return BackendHelpers.ErrorResponse($"Gemini upstream error: {geminiResponseJson}", (int)geminiResponse.StatusCode, "upstream_error");
        }

        var geminiBackendResponse = GeminiResponse.ToBackend(geminiResponseJson);
        return Results.Ok(geminiBackendResponse);
    }

    var openAiRequest = OpenAIRequest.FromBackend(payload, config);
    var openAiJson = JsonSerializer.Serialize(openAiRequest, Json.Options);

    using var openAiMessage = new HttpRequestMessage(HttpMethod.Post, config.OpenAIEndpoint);
    openAiMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.OpenAIApiKey);
    openAiMessage.Content = new StringContent(openAiJson, Encoding.UTF8, "application/json");

    using var openAiResponse = await client.SendAsync(openAiMessage);
    var openAiResponseJson = await openAiResponse.Content.ReadAsStringAsync();

    if (!openAiResponse.IsSuccessStatusCode)
    {
        return BackendHelpers.ErrorResponse($"Upstream error: {openAiResponseJson}", (int)openAiResponse.StatusCode, "upstream_error");
    }

    var openAiBackendResponse = OpenAIResponse.ToBackend(openAiResponseJson);
    return Results.Ok(openAiBackendResponse);
});

app.MapPost("/v1/chat/stream", async (HttpRequest request, HttpResponse response, IHttpClientFactory httpFactory) =>
{
    var config = BackendConfig.Load();
    if (!await BackendHelpers.ValidateApiKeyForSseAsync(request, response, config))
    {
        return;
    }

    if (config.UsesGemini && string.IsNullOrWhiteSpace(config.GeminiApiKey))
    {
        response.StatusCode = StatusCodes.Status500InternalServerError;
        await BackendHelpers.WriteSseErrorAsync(response, "GEMINI_API_KEY is not configured on the backend.", "backend_missing_key");
        return;
    }

    if (config.UsesOpenAI && string.IsNullOrWhiteSpace(config.OpenAIApiKey))
    {
        response.StatusCode = StatusCodes.Status500InternalServerError;
        await BackendHelpers.WriteSseErrorAsync(response, "OPENAI_API_KEY is not configured on the backend.", "backend_missing_key");
        return;
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

    if (config.UsesGemini && CurrentFactAnswers.TryAnswer(payload, DateTimeOffset.Now) is { } currentFactAnswer)
    {
        foreach (var token in BackendHelpers.TokenizeForStream(currentFactAnswer.Content))
        {
            await BackendHelpers.WriteSseAsync(response, new { token });
            await Task.Delay(8);
        }

        await BackendHelpers.WriteSseAsync(response, new
        {
            done = true,
            toolCalls = currentFactAnswer.ToolCalls
        });
        return;
    }

    using var client = httpFactory.CreateClient("openai");

    if (config.UsesGemini)
    {
        var geminiModel = GeminiRequest.ResolveModel(payload.Model, config);
        var geminiRequest = GeminiRequest.FromBackend(payload, config);
        var geminiJson = JsonSerializer.Serialize(geminiRequest, Json.Options);
        var geminiUrl = $"{config.GeminiEndpointBase.TrimEnd('/')}/{geminiModel}:generateContent";

        using var geminiMessage = new HttpRequestMessage(HttpMethod.Post, geminiUrl);
        geminiMessage.Headers.Add("x-goog-api-key", config.GeminiApiKey);
        geminiMessage.Content = new StringContent(geminiJson, Encoding.UTF8, "application/json");

        using var geminiResponse = await client.SendAsync(geminiMessage);
        var geminiResponseJson = await geminiResponse.Content.ReadAsStringAsync();
        if (!geminiResponse.IsSuccessStatusCode)
        {
            if ((int)geminiResponse.StatusCode == StatusCodes.Status429TooManyRequests &&
                GeminiRequest.RequiresSearchGrounding(payload))
            {
                var quotaMessage = "I need live web grounding to answer that reliably, but the Gemini search/quota limit is exhausted for this API key right now. Try again later or enable billing/quota for grounded search.";
                foreach (var token in BackendHelpers.TokenizeForStream(quotaMessage))
                {
                    await BackendHelpers.WriteSseAsync(response, new { token });
                    await Task.Delay(8);
                }

                await BackendHelpers.WriteSseAsync(response, new
                {
                    done = true,
                    toolCalls = Array.Empty<BackendToolCall>()
                });
                return;
            }

            response.StatusCode = (int)geminiResponse.StatusCode;
            await BackendHelpers.WriteSseErrorAsync(response, $"Gemini upstream error: {geminiResponseJson}", "upstream_error");
            return;
        }

        var geminiBackendResponse = GeminiResponse.ToBackend(geminiResponseJson);
        foreach (var token in BackendHelpers.TokenizeForStream(geminiBackendResponse.Content))
        {
            await BackendHelpers.WriteSseAsync(response, new { token });
            await Task.Delay(8);
        }

        await BackendHelpers.WriteSseAsync(response, new
        {
            done = true,
            toolCalls = geminiBackendResponse.ToolCalls
        });
        return;
    }

    var openAiRequest = OpenAIRequest.FromBackend(payload, config, stream: true);
    var openAiJson = JsonSerializer.Serialize(openAiRequest, Json.Options);

    using var openAiMessage = new HttpRequestMessage(HttpMethod.Post, config.OpenAIEndpoint);
    openAiMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.OpenAIApiKey);
    openAiMessage.Content = new StringContent(openAiJson, Encoding.UTF8, "application/json");

    using var openAiResponse = await client.SendAsync(openAiMessage, HttpCompletionOption.ResponseHeadersRead);
    if (!openAiResponse.IsSuccessStatusCode)
    {
        var openAiErrorJson = await openAiResponse.Content.ReadAsStringAsync();
        response.StatusCode = (int)openAiResponse.StatusCode;
        await BackendHelpers.WriteSseErrorAsync(response, $"Upstream error: {openAiErrorJson}", "upstream_error");
        return;
    }

    await using var stream = await openAiResponse.Content.ReadAsStreamAsync();
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
