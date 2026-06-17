using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed record BackendConfig
{
    public string Provider { get; init; } = "gemini";
    public string GeminiApiKey { get; init; } = string.Empty;
    public string GeminiEndpointBase { get; init; } = "https://generativelanguage.googleapis.com/v1beta/models";
    public string OpenAIApiKey { get; init; } = string.Empty;
    public string OpenAIEndpoint { get; init; } = "https://api.openai.com/v1/chat/completions";
    public string DefaultModel { get; init; } = "gemini-3.1-flash-lite";
    public string FastModel { get; init; } = "gemini-3.1-flash-lite";
    public string ApiKey { get; init; } = string.Empty;
    public bool RequireApiKey { get; init; }
    public double Temperature { get; init; } = 0.3;
    public int MaxTokens { get; init; } = 1200;

    public static BackendConfig Load()
    {
        var environment = GetEnvironmentValue("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var requireApiKey = ParseBoolean(GetEnvironmentValue("SMARTITECTURE_REQUIRE_BACKEND_API_KEY"))
                            ?? environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

        var config = new BackendConfig
        {
            Provider = GetEnvironmentValue("SMARTITECTURE_AI_PROVIDER")
                       ?? GetEnvironmentValue("AI_PROVIDER")
                       ?? "gemini",
            GeminiApiKey = GetEnvironmentValue("GEMINI_API_KEY")
                           ?? GetEnvironmentValue("GOOGLE_API_KEY")
                           ?? string.Empty,
            GeminiEndpointBase = GetEnvironmentValue("GEMINI_ENDPOINT_BASE")
                                 ?? "https://generativelanguage.googleapis.com/v1beta/models",
            OpenAIApiKey = GetEnvironmentValue("OPENAI_API_KEY") ?? string.Empty,
            OpenAIEndpoint = GetEnvironmentValue("OPENAI_ENDPOINT")
                             ?? "https://api.openai.com/v1/chat/completions",
            DefaultModel = GetEnvironmentValue("SMARTITECTURE_AI_MODEL")
                           ?? GetEnvironmentValue("GEMINI_MODEL")
                           ?? GetEnvironmentValue("OPENAI_MODEL")
                           ?? "gemini-3.1-flash-lite",
            FastModel = GetEnvironmentValue("SMARTITECTURE_AI_FAST_MODEL")
                        ?? GetEnvironmentValue("GEMINI_FAST_MODEL")
                        ?? GetEnvironmentValue("OPENAI_FAST_MODEL")
                        ?? "gemini-3.1-flash-lite",
            ApiKey = GetEnvironmentValue("SMARTITECTURE_BACKEND_API_KEY")
                     ?? GetEnvironmentValue("SMARTITECTURE_BACKEND_KEY")
                     ?? string.Empty,
            RequireApiKey = requireApiKey
        };

        if (double.TryParse(GetEnvironmentValue("OPENAI_TEMPERATURE"), out var temp))
        {
            config = config with { Temperature = temp };
        }

        if (int.TryParse(GetEnvironmentValue("OPENAI_MAX_TOKENS"), out var tokens))
        {
            config = config with { MaxTokens = tokens };
        }

        return config;
    }

    public bool UsesGemini => Provider.Equals("gemini", StringComparison.OrdinalIgnoreCase);
    public bool UsesOpenAI => Provider.Equals("openai", StringComparison.OrdinalIgnoreCase);

    private static string? GetEnvironmentValue(string name)
    {
        return Environment.GetEnvironmentVariable(name)
               ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
               ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
    }

    private static bool? ParseBoolean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return value.Trim() switch
        {
            "1" => true,
            "0" => false,
            "yes" => true,
            "no" => false,
            "on" => true,
            "off" => false,
            _ => null
        };
    }
}

internal sealed record BackendChatRequest(
    string? Model,
    List<BackendMessage> Messages,
    List<BackendTool>? Tools);

internal sealed record BackendMessage(string Role, string Content);

internal sealed record BackendTool(string Name, string Description, string JsonSchema);

internal sealed record BackendChatResponse(string Content, List<BackendToolCall> ToolCalls);

internal sealed record BackendToolCall(string Name, string ArgumentsJson);

internal sealed record OpenAIRequest(
    string model,
    List<OpenAIMessage> messages,
    double temperature,
    int max_tokens,
    List<OpenAITool>? tools,
    bool stream)
{
    public static OpenAIRequest FromBackend(BackendChatRequest request, BackendConfig config, bool stream = false)
    {
        var model = ResolveModel(request.Model, config);
        var messages = request.Messages
            .Select(m => new OpenAIMessage(m.Role, m.Content))
            .ToList();

        var tools = request.Tools == null || request.Tools.Count == 0
            ? null
            : request.Tools.Select(OpenAITool.FromBackend)
                .Where(t => t != null)
                .Cast<OpenAITool>()
                .ToList();

        return new OpenAIRequest(model, messages, config.Temperature, config.MaxTokens, tools, stream);
    }

    private static string ResolveModel(string? requested, BackendConfig config)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return config.DefaultModel;
        }

        return requested.ToLowerInvariant() switch
        {
            "smartitecture-fast" => config.FastModel,
            "smartitecture" => config.DefaultModel,
            _ => requested
        };
    }
}

internal sealed record OpenAIMessage(string role, string content);

internal sealed record OpenAITool(string type, OpenAIFunction function)
{
    public static OpenAITool? FromBackend(BackendTool tool)
    {
        try
        {
            using var schema = JsonDocument.Parse(tool.JsonSchema);
            var parameters = schema.RootElement.Clone();
            return new OpenAITool("function", new OpenAIFunction(tool.Name, tool.Description, parameters));
        }
        catch
        {
            return null;
        }
    }
}

internal sealed record OpenAIFunction(string name, string description, JsonElement parameters);

internal static class OpenAIResponse
{
    public static BackendChatResponse ToBackend(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var choice = root.GetProperty("choices")[0].GetProperty("message");
        var content = choice.TryGetProperty("content", out var contentElement)
            ? contentElement.GetString() ?? string.Empty
            : string.Empty;

        var toolCalls = new List<BackendToolCall>();
        if (choice.TryGetProperty("tool_calls", out var toolCallsElement))
        {
            foreach (var tool in toolCallsElement.EnumerateArray())
            {
                var func = tool.GetProperty("function");
                var name = func.GetProperty("name").GetString() ?? string.Empty;
                var arguments = func.GetProperty("arguments").GetString() ?? "{}";
                toolCalls.Add(new BackendToolCall(name, arguments));
            }
        }

        return new BackendChatResponse(content, toolCalls);
    }
}

internal sealed record GeminiRequest(
    List<GeminiContent> contents,
    GeminiSystemInstruction? systemInstruction,
    GeminiGenerationConfig generationConfig,
    List<GeminiTool>? tools)
{
    public static GeminiRequest FromBackend(BackendChatRequest request, BackendConfig config)
    {
        var contents = new List<GeminiContent>();
        var systemParts = new List<GeminiPart>();
        var useSearchGrounding = RequiresSearchGrounding(request);

        foreach (var message in request.Messages)
        {
            if (message.Role.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                systemParts.Add(new GeminiPart(message.Content));
                continue;
            }

            contents.Add(new GeminiContent(
                ToGeminiRole(message.Role),
                new List<GeminiPart> { new(message.Content) }));
        }

        systemParts.Add(new GeminiPart(
            $"Current server date: {DateTimeOffset.Now:dddd, MMMM dd, yyyy}. " +
            "For current office holders, recent events, latest information, prices, laws, schedules, or other time-sensitive facts, do not rely on memorized cutoff knowledge. " +
            "Use Google Search grounding when available and state the as-of date when the answer can change."));

        return new GeminiRequest(
            contents,
            new GeminiSystemInstruction(systemParts),
            new GeminiGenerationConfig(config.Temperature, config.MaxTokens),
            useSearchGrounding ? new List<GeminiTool> { GeminiTool.GoogleSearchTool() } : null);
    }

    public static string ResolveModel(string? requested, BackendConfig config)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return config.DefaultModel;
        }

        return requested.ToLowerInvariant() switch
        {
            "smartitecture-fast" => config.FastModel,
            "smartitecture" => config.DefaultModel,
            _ => requested
        };
    }

    private static string ToGeminiRole(string role)
    {
        return role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "model" : "user";
    }

    public static bool RequiresSearchGrounding(BackendChatRequest request)
    {
        var text = LatestUserMessage(request).ToLowerInvariant();
        var searchTriggers = new[]
        {
            "current", "currently", "today", "latest", "recent", "right now", "now",
            "president", "prime minister", "governor", "mayor", "ceo",
            "news", "weather", "price", "stock", "crypto", "schedule",
            "law", "legal", "release date", "version", "update"
        };

        return searchTriggers.Any(text.Contains);
    }

    public static string LatestUserMessage(BackendChatRequest request)
    {
        for (var i = request.Messages.Count - 1; i >= 0; i--)
        {
            if (request.Messages[i].Role.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                return request.Messages[i].Content;
            }
        }

        return request.Messages.Count == 0 ? string.Empty : request.Messages[^1].Content;
    }
}

internal static class CurrentFactAnswers
{
    public static BackendChatResponse? TryAnswer(BackendChatRequest request, DateTimeOffset now)
    {
        var text = GeminiRequest.LatestUserMessage(request).ToLowerInvariant();

        if (text.Contains("president") &&
            (text.Contains("usa") ||
             text.Contains("u.s.") ||
             text.Contains("us ") ||
             text.Contains("united states") ||
             text.Contains("america")))
        {
            var trumpSecondTermStart = new DateTimeOffset(2025, 1, 20, 0, 0, 0, now.Offset);
            var nextScheduledInauguration = new DateTimeOffset(2029, 1, 20, 0, 0, 0, now.Offset);

            if (now >= trumpSecondTermStart && now < nextScheduledInauguration)
            {
                return new BackendChatResponse(
                    $"As of {now:MMMM dd, yyyy}, the president of the United States is Donald J. Trump.",
                    new List<BackendToolCall>());
            }
        }

        return null;
    }
}

internal sealed record GeminiContent(string role, List<GeminiPart> parts);

internal sealed record GeminiPart(string text);

internal sealed record GeminiSystemInstruction(List<GeminiPart> parts);

internal sealed record GeminiGenerationConfig(double temperature, int maxOutputTokens);

internal sealed record GeminiTool
{
    [JsonPropertyName("functionDeclarations")]
    public List<GeminiFunctionDeclaration>? FunctionDeclarations { get; init; }

    [JsonPropertyName("google_search")]
    public object? GoogleSearch { get; init; }

    public static GeminiTool GoogleSearchTool()
    {
        return new GeminiTool { GoogleSearch = new { } };
    }
}

internal sealed record GeminiFunctionDeclaration(string name, string description, JsonElement parameters)
{
    public static GeminiFunctionDeclaration? FromBackend(BackendTool tool)
    {
        try
        {
            using var schema = JsonDocument.Parse(tool.JsonSchema);
            return new GeminiFunctionDeclaration(tool.Name, tool.Description, schema.RootElement.Clone());
        }
        catch
        {
            return null;
        }
    }
}

internal static class GeminiResponse
{
    public static BackendChatResponse ToBackend(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var content = string.Empty;
        var toolCalls = new List<BackendToolCall>();

        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            return new BackendChatResponse(content, toolCalls);
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var candidateContent) ||
            !candidateContent.TryGetProperty("parts", out var parts))
        {
            return new BackendChatResponse(content, toolCalls);
        }

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var text))
            {
                content += text.GetString() ?? string.Empty;
            }

            if (part.TryGetProperty("functionCall", out var functionCall))
            {
                var name = functionCall.TryGetProperty("name", out var nameElement)
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty;
                var args = functionCall.TryGetProperty("args", out var argsElement)
                    ? argsElement.GetRawText()
                    : "{}";

                if (!string.IsNullOrWhiteSpace(name))
                {
                    toolCalls.Add(new BackendToolCall(name, args));
                }
            }
        }

        return new BackendChatResponse(content, toolCalls);
    }
}

internal static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

internal sealed class ToolCallAccumulator
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public StringBuilder Arguments { get; } = new StringBuilder();

    public BackendToolCall? ToToolCall()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return null;
        }

        var args = Arguments.Length == 0 ? "{}" : Arguments.ToString();
        return new BackendToolCall(Name, args);
    }
}
