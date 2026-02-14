using System;
using Microsoft.Extensions.Configuration;

namespace Smartitecture.Services.Core
{
    public sealed class ProviderSettings
    {
        public OpenAISettings OpenAI { get; set; } = new();
        public AzureOpenAISettings AzureOpenAI { get; set; } = new();
        public AnthropicSettings Anthropic { get; set; } = new();
        public GeminiSettings Gemini { get; set; } = new();
        public OllamaSettings Ollama { get; set; } = new();
        public BackendSettings Backend { get; set; } = new();

        public static ProviderSettings Load()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var settings = new ProviderSettings();
            config.GetSection("OpenAI").Bind(settings.OpenAI);
            config.GetSection("AzureOpenAI").Bind(settings.AzureOpenAI);
            config.GetSection("Anthropic").Bind(settings.Anthropic);
            config.GetSection("Gemini").Bind(settings.Gemini);
            config.GetSection("Ollama").Bind(settings.Ollama);
            config.GetSection("Backend").Bind(settings.Backend);

            return settings;
        }
    }

    public sealed class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";
    }

    public sealed class AzureOpenAISettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DeploymentName { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = "2024-02-15-preview";
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.2;
    }

    public sealed class AnthropicSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "claude-3-sonnet-20240229";
        public int MaxTokens { get; set; } = 1200;
        public double Temperature { get; set; } = 0.3;
    }

    public sealed class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-1.5-pro";
        public double Temperature { get; set; } = 0.3;
    }

    public sealed class OllamaSettings
    {
        public string Endpoint { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "llama3";
    }

    public sealed class BackendSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
