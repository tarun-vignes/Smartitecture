using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Smartitecture.Services.Models;

namespace Smartitecture.Services.Providers
{
    /// <summary>
    /// Azure OpenAI provider implementation
    /// </summary>
    public class AzureOpenAIProvider : IModelProvider
    {
        private readonly ILogger<AzureOpenAIProvider> _logger;
        private readonly HttpClient _httpClient;
        private ModelProvider _config;
        private bool _isInitialized;

        public string ProviderType => "Azure";
        public bool IsAvailable => _isInitialized && !string.IsNullOrEmpty(_config?.ApiKey);
        public bool SupportsStreaming => true;

        public IEnumerable<string> AvailableModels
        {
            get
            {
                if (!IsAvailable || _config?.Models == null)
                    return Enumerable.Empty<string>();
                
                return _config.Models.Keys;
            }
        }

        public AzureOpenAIProvider(ILogger<AzureOpenAIProvider> logger, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public void Initialize(AIModelConfiguration config)
        {
            try
            {
                if (config.Providers.TryGetValue("Azure", out var azureConfig))
                {
                    _config = azureConfig;
                    
                    // Set up HTTP client
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
                    _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

                    _isInitialized = true;
                    _logger.LogInformation("Azure OpenAI provider initialized successfully");
                }
                else
                {
                    _logger.LogWarning("Azure configuration not found in AI model configuration");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure OpenAI provider");
                _isInitialized = false;
            }
        }

        public async Task<string> GenerateResponseAsync(string modelName, List<ConversationMessage> messages)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Azure OpenAI provider is not available");

            try
            {
                var modelConfig = _config.Models.GetValueOrDefault(modelName);
                if (modelConfig == null)
                    throw new ArgumentException($"Model {modelName} not configured for Azure provider");

                var requestBody = new
                {
                    messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
                    max_tokens = modelConfig.MaxTokens,
                    temperature = modelConfig.Temperature,
                    top_p = modelConfig.TopP
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_config.Endpoint}/openai/deployments/{modelConfig.Name}/chat/completions?api-version={_config.ApiVersion}";
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (responseData.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var messageContent))
                    {
                        return messageContent.GetString() ?? "No response generated";
                    }
                }

                return "No response generated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating response from Azure OpenAI");
                throw;
            }
        }

        public async IAsyncEnumerable<string> GenerateStreamingResponseAsync(string modelName, List<ConversationMessage> messages)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Azure OpenAI provider is not available");

            // For now, return the full response as a single token
            // In a full implementation, you'd use Server-Sent Events (SSE) for true streaming
            var response = await GenerateResponseAsync(modelName, messages);
            yield return response;
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (!IsAvailable)
                return false;

            try
            {
                // Simple test with a minimal request
                var testMessages = new List<ConversationMessage>
                {
                    new ConversationMessage { Role = "user", Content = "Hello" }
                };

                var firstModel = AvailableModels.FirstOrDefault();
                if (firstModel == null)
                    return false;

                await GenerateResponseAsync(firstModel, testMessages);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for Azure OpenAI provider");
                return false;
            }
        }
    }
}
