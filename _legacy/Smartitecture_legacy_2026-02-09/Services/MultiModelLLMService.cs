using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Smartitecture.Services.Models;
using Smartitecture.Services.Providers;

namespace Smartitecture.Services
{
    /// <summary>
    /// Multi-model LLM service that supports multiple AI providers
    /// </summary>
    public class MultiModelLLMService : ILLMService
    {
        private readonly ILogger<MultiModelLLMService> _logger;
        private readonly AIModelConfiguration _config;
        private readonly Dictionary<string, IModelProvider> _providers;
        private readonly ConversationManager _conversationManager;
        private string _currentModel;

        public event EventHandler<ModelSwitchedEventArgs> ModelSwitched;

        public MultiModelLLMService(
            IOptions<AIModelConfiguration> config,
            ILogger<MultiModelLLMService> logger,
            IEnumerable<IModelProvider> providers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _providers = providers?.ToDictionary(p => p.ProviderType, p => p) ?? new Dictionary<string, IModelProvider>();
            _conversationManager = new ConversationManager(_config.ConversationSettings);
            _currentModel = _config.DefaultModel;

            InitializeProviders();
        }

        public string CurrentModel => _currentModel;

        public IEnumerable<string> AvailableModels
        {
            get
            {
                var models = new List<string>();
                foreach (var provider in _providers.Values.Where(p => p.IsAvailable))
                {
                    models.AddRange(provider.AvailableModels);
                }
                return models;
            }
        }

        public async Task<string> GetResponseAsync(string message, string conversationId = null)
        {
            try
            {
                conversationId ??= "default";
                
                // Add user message to conversation history
                var userMessage = new ConversationMessage
                {
                    Role = "user",
                    Content = message,
                    Model = _currentModel
                };
                
                await _conversationManager.AddMessageAsync(conversationId, userMessage);

                // Get conversation context
                var context = await _conversationManager.GetContextAsync(conversationId);

                // Get the appropriate provider for the current model
                var provider = GetProviderForModel(_currentModel);
                if (provider == null)
                {
                    throw new InvalidOperationException($"No provider available for model: {_currentModel}");
                }

                // Generate response
                var response = await provider.GenerateResponseAsync(_currentModel, context);

                // Add assistant response to conversation history
                var assistantMessage = new ConversationMessage
                {
                    Role = "assistant",
                    Content = response,
                    Model = _currentModel
                };
                
                await _conversationManager.AddMessageAsync(conversationId, assistantMessage);

                _logger.LogInformation("Generated response using model {Model} for conversation {ConversationId}", 
                    _currentModel, conversationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating response with model {Model}", _currentModel);
                return "I apologize, but I encountered an error while processing your request. Please try again.";
            }
        }

        public async Task<string> GetStreamingResponseAsync(string message, Action<string> onTokenReceived, string conversationId = null)
        {
            try
            {
                conversationId ??= "default";
                
                // Add user message to conversation history
                var userMessage = new ConversationMessage
                {
                    Role = "user",
                    Content = message,
                    Model = _currentModel
                };
                
                await _conversationManager.AddMessageAsync(conversationId, userMessage);

                // Get conversation context
                var context = await _conversationManager.GetContextAsync(conversationId);

                // Get the appropriate provider for the current model
                var provider = GetProviderForModel(_currentModel);
                if (provider == null)
                {
                    throw new InvalidOperationException($"No provider available for model: {_currentModel}");
                }

                // Check if provider supports streaming
                if (!provider.SupportsStreaming)
                {
                    var response = await provider.GenerateResponseAsync(_currentModel, context);
                    onTokenReceived?.Invoke(response);
                    return response;
                }

                // Generate streaming response
                var fullResponse = "";
                await foreach (var token in provider.GenerateStreamingResponseAsync(_currentModel, context))
                {
                    fullResponse += token;
                    onTokenReceived?.Invoke(token);
                }

                // Add assistant response to conversation history
                var assistantMessage = new ConversationMessage
                {
                    Role = "assistant",
                    Content = fullResponse,
                    Model = _currentModel
                };
                
                await _conversationManager.AddMessageAsync(conversationId, assistantMessage);

                _logger.LogInformation("Generated streaming response using model {Model} for conversation {ConversationId}", 
                    _currentModel, conversationId);

                return fullResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating streaming response with model {Model}", _currentModel);
                var errorMessage = "I apologize, but I encountered an error while processing your request. Please try again.";
                onTokenReceived?.Invoke(errorMessage);
                return errorMessage;
            }
        }

        public async Task<(string commandName, Dictionary<string, object> parameters)> ParseCommandAsync(string message)
        {
            try
            {
                // Use a specialized prompt for command parsing
                var parsePrompt = $@"Parse the following message and extract any system command. 
If it contains a command, respond with JSON format: {{""command"": ""command_name"", ""parameters"": {{}}}}
If no command is found, respond with: {{""command"": ""Unknown"", ""parameters"": {{}}}}

Message: {message}

Response:";

                var provider = GetProviderForModel(_currentModel);
                if (provider == null)
                {
                    return ("Unknown", new Dictionary<string, object>());
                }

                var response = await provider.GenerateResponseAsync(_currentModel, new List<ConversationMessage>
                {
                    new ConversationMessage { Role = "user", Content = parsePrompt }
                });

                // Parse the JSON response (simplified parsing)
                // In a real implementation, you'd use a proper JSON parser
                if (response.Contains("\"command\""))
                {
                    // Extract command name (simplified)
                    var commandStart = response.IndexOf("\"command\": \"") + 12;
                    var commandEnd = response.IndexOf("\"", commandStart);
                    if (commandEnd > commandStart)
                    {
                        var commandName = response.Substring(commandStart, commandEnd - commandStart);
                        return (commandName, new Dictionary<string, object>());
                    }
                }

                return ("Unknown", new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing command from message");
                return ("Unknown", new Dictionary<string, object>());
            }
        }

        public async Task<bool> SwitchModelAsync(string modelName)
        {
            try
            {
                if (!AvailableModels.Contains(modelName))
                {
                    _logger.LogWarning("Attempted to switch to unavailable model: {ModelName}", modelName);
                    return false;
                }

                var previousModel = _currentModel;
                _currentModel = modelName;

                ModelSwitched?.Invoke(this, new ModelSwitchedEventArgs
                {
                    PreviousModel = previousModel,
                    NewModel = modelName
                });

                _logger.LogInformation("Switched from model {PreviousModel} to {NewModel}", previousModel, modelName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching to model {ModelName}", modelName);
                return false;
            }
        }

        public async Task<List<ConversationMessage>> GetConversationHistoryAsync(string conversationId)
        {
            return await _conversationManager.GetHistoryAsync(conversationId);
        }

        public async Task ClearConversationAsync(string conversationId)
        {
            await _conversationManager.ClearConversationAsync(conversationId);
            _logger.LogInformation("Cleared conversation history for {ConversationId}", conversationId);
        }

        private void InitializeProviders()
        {
            foreach (var provider in _providers.Values)
            {
                try
                {
                    provider.Initialize(_config);
                    _logger.LogInformation("Initialized provider: {ProviderType}", provider.ProviderType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize provider: {ProviderType}", provider.ProviderType);
                }
            }
        }

        private IModelProvider GetProviderForModel(string modelName)
        {
            foreach (var provider in _providers.Values.Where(p => p.IsAvailable))
            {
                if (provider.AvailableModels.Contains(modelName))
                {
                    return provider;
                }
            }
            return null;
        }
    }
}
