using System.Collections.Generic;

namespace Smartitecture.Services.Models
{
    /// <summary>
    /// Configuration for AI models and providers
    /// </summary>
    public class AIModelConfiguration
    {
        public string DefaultModel { get; set; } = "gpt-4";
        public Dictionary<string, ModelProvider> Providers { get; set; } = new();
        public ConversationSettings ConversationSettings { get; set; } = new();
    }

    /// <summary>
    /// Configuration for a specific AI model provider
    /// </summary>
    public class ModelProvider
    {
        public string Name { get; set; }
        public string Type { get; set; } // "OpenAI", "Azure", "Anthropic", "Local", etc.
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
        public string ApiVersion { get; set; }
        public Dictionary<string, ModelConfig> Models { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Configuration for a specific model
    /// </summary>
    public class ModelConfig
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int MaxTokens { get; set; } = 4000;
        public double Temperature { get; set; } = 0.7;
        public double TopP { get; set; } = 1.0;
        public int ContextWindow { get; set; } = 8192;
        public bool SupportsStreaming { get; set; } = true;
        public bool SupportsImages { get; set; } = false;
        public bool SupportsCodeExecution { get; set; } = false;
        public List<string> Capabilities { get; set; } = new();
        public Dictionary<string, object> CustomParameters { get; set; } = new();
    }

    /// <summary>
    /// Settings for conversation management
    /// </summary>
    public class ConversationSettings
    {
        public int MaxHistoryLength { get; set; } = 50;
        public int MaxContextTokens { get; set; } = 6000;
        public bool EnableMemory { get; set; } = true;
        public bool EnableAutoSummarization { get; set; } = true;
        public int SummarizationThreshold { get; set; } = 20;
        public string SystemPrompt { get; set; } = "You are Smartitecture, an intelligent assistant for system administration and development tasks.";
    }

    /// <summary>
    /// Azure OpenAI specific configuration
    /// </summary>
    public class AzureOpenAIConfiguration
    {
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
        public string ApiVersion { get; set; } = "2024-02-15-preview";
        public string DeploymentName { get; set; }
        public int MaxTokens { get; set; } = 4000;
        public double Temperature { get; set; } = 0.7;
    }
}
