using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Smartitecture.Services.Models;

namespace Smartitecture.Services.Providers
{
    /// <summary>
    /// Local fallback provider that provides basic responses when external AI is unavailable
    /// </summary>
    public class LocalFallbackProvider : IModelProvider
    {
        private readonly ILogger<LocalFallbackProvider> _logger;
        private readonly Dictionary<string, string> _responses;

        public string ProviderType => "Local";
        public bool IsAvailable => true; // Always available as fallback
        public bool SupportsStreaming => false;
        public IEnumerable<string> AvailableModels => new[] { "local-fallback" };

        public LocalFallbackProvider(ILogger<LocalFallbackProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _responses = InitializeResponses();
        }

        public void Initialize(AIModelConfiguration config)
        {
            _logger.LogInformation("Local fallback provider initialized");
        }

        public async Task<string> GenerateResponseAsync(string modelName, List<ConversationMessage> messages)
        {
            await Task.Delay(100); // Simulate processing time

            var lastMessage = messages.LastOrDefault(m => m.Role == "user");
            if (lastMessage == null)
                return "I'm here to help! What would you like to know?";

            var userMessage = lastMessage.Content.ToLowerInvariant();

            // Simple keyword-based responses
            foreach (var kvp in _responses)
            {
                if (userMessage.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            // Default response
            return $"I understand you're asking about: '{lastMessage.Content}'. " +
                   "I'm currently running in offline mode with limited capabilities. " +
                   "For full AI functionality, please configure an external AI provider in the settings.";
        }

        public async IAsyncEnumerable<string> GenerateStreamingResponseAsync(string modelName, List<ConversationMessage> messages)
        {
            var response = await GenerateResponseAsync(modelName, messages);
            yield return response;
        }

        public async Task<bool> TestConnectionAsync()
        {
            return await Task.FromResult(true);
        }

        private Dictionary<string, string> InitializeResponses()
        {
            return new Dictionary<string, string>
            {
                ["hello"] = "Hello! I'm Smartitecture, your AI assistant. How can I help you today?",
                ["help"] = "I can help you with various tasks including system administration, file management, and general questions. What would you like to do?",
                ["status"] = "I'm currently running in offline mode. All systems are operational but AI capabilities are limited.",
                ["time"] = $"The current time is {DateTime.Now:HH:mm:ss} on {DateTime.Now:yyyy-MM-dd}.",
                ["date"] = $"Today's date is {DateTime.Now:yyyy-MM-dd}.",
                ["weather"] = "I don't have access to weather information in offline mode. Please check your local weather service.",
                ["file"] = "I can help with file operations. What would you like to do with files?",
                ["system"] = "I can provide system information and help with basic system tasks. What do you need?",
                ["network"] = "I can help with network-related tasks and monitoring. What network information do you need?",
                ["security"] = "I can assist with security monitoring and basic security tasks. How can I help with security?",
                ["screenshot"] = "I can help analyze screenshots for productivity improvements. Please take a screenshot first.",
                ["analyze"] = "I can analyze various types of data and files. What would you like me to analyze?",
                ["command"] = "I can help execute system commands. What command would you like to run?",
                ["settings"] = "You can configure AI providers and other settings in the Settings panel.",
                ["configure"] = "To enable full AI capabilities, please configure an external AI provider (Azure OpenAI, OpenAI, etc.) in the settings.",
                ["offline"] = "I'm currently in offline mode. This means I have limited AI capabilities but can still help with basic tasks.",
                ["online"] = "To go online, please configure an AI provider in the settings panel.",
                ["bye"] = "Goodbye! Feel free to ask if you need any assistance.",
                ["thank"] = "You're welcome! I'm here whenever you need help."
            };
        }
    }
}
