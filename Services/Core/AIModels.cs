using System.Collections.Generic;

namespace Smartitecture.Services.Core
{
    public enum AIModeType
    {
        Lumen,
        Fortis,
        Nexa
    }

    public sealed class LLMMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    public sealed class ToolDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string JsonSchema { get; set; } = string.Empty;
    }

    public sealed class ToolCall
    {
        public string Name { get; set; } = string.Empty;
        public string ArgumentsJson { get; set; } = "{}";
    }

    public sealed class LLMRequest
    {
        public string UserMessage { get; set; } = string.Empty;
        public IReadOnlyList<LLMMessage> History { get; set; } = new List<LLMMessage>();
        public string? SystemPrompt { get; set; }
        public string? Model { get; set; }
        public IReadOnlyList<ToolDefinition> Tools { get; set; } = new List<ToolDefinition>();
    }

    public sealed class LLMResponse
    {
        public string Content { get; set; } = string.Empty;
        public IReadOnlyList<ToolCall> ToolCalls { get; set; } = new List<ToolCall>();
    }
}
