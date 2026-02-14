using System.Text;
using System.Text.Json;

internal sealed record BackendConfig
{
    public string OpenAIApiKey { get; init; } = string.Empty;
    public string OpenAIEndpoint { get; init; } = "https://api.openai.com/v1/chat/completions";
    public string DefaultModel { get; init; } = "gpt-4o-mini";
    public string FastModel { get; init; } = "gpt-4o-mini";
    public string ApiKey { get; init; } = string.Empty;
    public double Temperature { get; init; } = 0.3;
    public int MaxTokens { get; init; } = 1200;

    public static BackendConfig Load()
    {
        var config = new BackendConfig
        {
            OpenAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty,
            OpenAIEndpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT")
                             ?? "https://api.openai.com/v1/chat/completions",
            DefaultModel = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini",
            FastModel = Environment.GetEnvironmentVariable("OPENAI_FAST_MODEL") ?? "gpt-4o-mini",
            ApiKey = Environment.GetEnvironmentVariable("SMARTITECTURE_BACKEND_KEY") ?? string.Empty
        };

        if (double.TryParse(Environment.GetEnvironmentVariable("OPENAI_TEMPERATURE"), out var temp))
        {
            config = config with { Temperature = temp };
        }

        if (int.TryParse(Environment.GetEnvironmentVariable("OPENAI_MAX_TOKENS"), out var tokens))
        {
            config = config with { MaxTokens = tokens };
        }

        return config;
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

internal static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
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
