using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Smartitecture.Services.Automation;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;
using Smartitecture.Services.Modes;
using Smartitecture.Services.Providers;

namespace Smartitecture.Services
{
    /// <summary>
    /// Orchestrates chat modes, providers, streaming, and command parsing.
    /// </summary>
    public sealed class MultiModelAIService : ILLMService
    {
        private sealed class ModelDefinition
        {
            public string Name { get; set; } = string.Empty;
            public string ProviderKey { get; set; } = string.Empty;
            public string ModelId { get; set; } = string.Empty;
            public AIModeType Mode { get; set; }
        }

        private readonly Dictionary<string, List<ConversationMessage>> _conversations = new();
        private readonly Dictionary<string, ModelDefinition> _modelCatalog;
        private readonly Dictionary<string, ILLMProvider> _providers;
        private readonly AIModeRouter _modeRouter;
        private readonly ToolExecutionService _toolExecutor = new ToolExecutionService();
        private AIModeType _currentMode;
        private bool _modeFollowsModel = true;

        public string CurrentModel { get; private set; } = "Advanced AI Assistant";
        public AIModeType CurrentMode => _currentMode;

        public IEnumerable<string> AvailableModels => _modelCatalog.Values
            .Where(IsModelAvailable)
            .Select(m => m.Name)
            .ToList();

        public IEnumerable<AIModeType> AvailableModes => new[]
        {
            AIModeType.Lumen,
            AIModeType.Fortis,
            AIModeType.Nexa
        };

        public event EventHandler<ModelSwitchedEventArgs>? ModelSwitched;
        public event EventHandler<ModeSwitchedEventArgs>? ModeSwitched;

        public MultiModelAIService()
        {
            _providers = new Dictionary<string, ILLMProvider>(StringComparer.OrdinalIgnoreCase)
            {
                ["backend"] = new BackendProvider(),
                ["local"] = new LocalFallbackProvider(),
                ["openai"] = new OpenAIProvider(),
                ["anthropic"] = new AnthropicProvider(),
                ["ollama"] = new OllamaProvider(),
                ["gemini"] = new GeminiProvider(),
                ["azure"] = new AzureOpenAIProvider()
            };

            _modeRouter = new AIModeRouter(new IAIMode[]
            {
                new LumenService(),
                new FortisService(),
                new NexaService()
            });

            _modelCatalog = BuildModelCatalog();

            if (!IsModelAvailable(GetModelDefinition(CurrentModel)))
            {
                var firstAvailable = _modelCatalog.Values.FirstOrDefault(IsModelAvailable);
                if (firstAvailable != null)
                {
                    CurrentModel = firstAvailable.Name;
                }
            }

            _currentMode = GetModelDefinition(CurrentModel).Mode;

        }

        public async Task<string> GetResponseAsync(string message, string? conversationId = null)
        {
            var request = BuildRequest(message, conversationId);
            var provider = GetProvider(request);

            await StoreMessageAsync(conversationId, "user", message);
            var response = await provider.GetResponseAsync(request, CancellationToken.None);
            var content = response.Content;
            var toolCalls = response.ToolCalls.ToList();

            if (ToolCallParser.TryExtractToolCalls(content, out var cleaned, out var parsedCalls))
            {
                content = cleaned;
                toolCalls.AddRange(parsedCalls);
            }

            if (toolCalls.Count > 0)
            {
                var toolSummary = await ExecuteToolCallsAsync(toolCalls);
                if (!string.IsNullOrWhiteSpace(toolSummary))
                {
                    content = string.IsNullOrWhiteSpace(content)
                        ? toolSummary
                        : $"{content}\n\n{toolSummary}".Trim();
                }
            }

            await StoreMessageAsync(conversationId, "assistant", content);
            return content;
        }

        public async Task<string> GetStreamingResponseAsync(string message, Action<string> onTokenReceived, string? conversationId = null)
        {
            var request = BuildRequest(message, conversationId);
            var provider = GetProvider(request);

            await StoreMessageAsync(conversationId, "user", message);

            var fullResponse = string.Empty;
            await foreach (var token in provider.StreamResponseAsync(request, CancellationToken.None))
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                fullResponse += token;
                onTokenReceived?.Invoke(token);
            }

            if (ToolCallParser.TryExtractToolCalls(fullResponse, out var cleaned, out var parsedCalls))
            {
                fullResponse = cleaned;
                var toolSummary = await ExecuteToolCallsAsync(parsedCalls);
                if (!string.IsNullOrWhiteSpace(toolSummary))
                {
                    fullResponse = string.IsNullOrWhiteSpace(fullResponse)
                        ? toolSummary
                        : $"{fullResponse}\n\n{toolSummary}".Trim();
                }
            }

            await StoreMessageAsync(conversationId, "assistant", fullResponse);
            return fullResponse;
        }

        public async Task<(string? commandName, Dictionary<string, object>? parameters)> ParseCommandAsync(string message)
        {
            await Task.Delay(50);
            var lowerMessage = message.ToLowerInvariant();

            var launchMatch = Regex.Match(lowerMessage, @"\b(open|launch|start)\s+(?<target>.+)");
            if (launchMatch.Success)
            {
                var target = launchMatch.Groups["target"].Value.Trim();
                if (!string.IsNullOrWhiteSpace(target))
                {
                    return ("launch", new Dictionary<string, object> { ["target"] = target });
                }
            }

            var commandPatterns = new Dictionary<string, string[]>
            {
                ["calculator"] = new[] { "calculator", "calc", "calculate", "math" },
                ["explorer"] = new[] { "explorer", "files", "file manager", "browse files" },
                ["taskmgr"] = new[] { "task manager", "taskmgr", "processes", "performance" },
                ["shutdown"] = new[] { "shutdown", "shut down", "power off", "turn off" },
                ["launch"] = new[] { "launch", "open", "start", "run" }
            };

            foreach (var (command, patterns) in commandPatterns)
            {
                if (patterns.Any(pattern => lowerMessage.Contains(pattern)))
                {
                    return (command, new Dictionary<string, object>());
                }
            }

            return (null, null);
        }

        public async Task<List<ConversationMessage>> GetConversationHistoryAsync(string conversationId)
        {
            await Task.Delay(10);
            return string.IsNullOrEmpty(conversationId) || !_conversations.ContainsKey(conversationId)
                ? new List<ConversationMessage>()
                : new List<ConversationMessage>(_conversations[conversationId]);
        }

        public async Task ClearConversationAsync(string conversationId)
        {
            await Task.Delay(10);
            if (!string.IsNullOrEmpty(conversationId) && _conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId].Clear();
            }
        }

        public async Task<bool> SwitchModelAsync(string modelName)
        {
            await Task.Delay(150);
            if (!_modelCatalog.ContainsKey(modelName))
            {
                return false;
            }

            var previous = CurrentModel;
            CurrentModel = modelName;
            ModelSwitched?.Invoke(this, new ModelSwitchedEventArgs
            {
                PreviousModel = previous,
                NewModel = modelName
            });

            if (_modeFollowsModel)
            {
                var modelMode = GetModelDefinition(modelName).Mode;
                if (_currentMode != modelMode)
                {
                    var previousMode = _currentMode;
                    _currentMode = modelMode;
                    ModeSwitched?.Invoke(this, new ModeSwitchedEventArgs
                    {
                        PreviousMode = previousMode,
                        NewMode = _currentMode
                    });
                }
            }

            return true;
        }

        public async Task<bool> SwitchModeAsync(AIModeType mode)
        {
            await Task.Delay(80);
            if (!AvailableModes.Contains(mode))
            {
                return false;
            }

            if (_currentMode == mode)
            {
                return true;
            }

            var previous = _currentMode;
            _currentMode = mode;
            _modeFollowsModel = false;
            ModeSwitched?.Invoke(this, new ModeSwitchedEventArgs
            {
                PreviousMode = previous,
                NewMode = _currentMode
            });

            return true;
        }

        private LLMRequest BuildRequest(string message, string? conversationId)
        {
            var history = GetConversationContext(conversationId);
            var def = GetModelDefinition(CurrentModel);
            var mode = _modeRouter.GetMode(_currentMode);

            return new LLMRequest
            {
                UserMessage = message,
                History = history,
                SystemPrompt = mode.SystemPrompt,
                Model = def.ModelId,
                Tools = mode.Tools
            };
        }

        private ILLMProvider GetProvider(LLMRequest request)
        {
            var def = GetModelDefinition(CurrentModel);
            if (_providers.TryGetValue(def.ProviderKey, out var provider) && provider.IsConfigured)
            {
                return provider;
            }

            // Fallback to local if desired provider is not configured
            return _providers["local"];
        }

        private bool IsModelAvailable(ModelDefinition definition)
        {
            if (definition.ProviderKey.Equals("local", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (_providers.TryGetValue(definition.ProviderKey, out var provider))
            {
                return provider.IsConfigured;
            }

            return false;
        }

        private ModelDefinition GetModelDefinition(string modelName)
        {
            if (_modelCatalog.TryGetValue(modelName, out var def))
            {
                return def;
            }

            return _modelCatalog["Advanced AI Assistant"];
        }

        private List<LLMMessage> GetConversationContext(string? conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return new List<LLMMessage>();
            }

            if (!_conversations.TryGetValue(conversationId, out var messages))
            {
                return new List<LLMMessage>();
            }

            return messages
                .Select(m => new LLMMessage { Role = m.Role, Content = m.Content })
                .ToList();
        }

        private async Task StoreMessageAsync(string? conversationId, string role, string content)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return;
            }

            if (!_conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId] = new List<ConversationMessage>();
            }

            _conversations[conversationId].Add(new ConversationMessage
            {
                Role = role,
                Content = content,
                Model = CurrentModel,
                Timestamp = DateTime.UtcNow
            });

            await Task.CompletedTask;
        }

        private async Task<string> ExecuteToolCallsAsync(IReadOnlyList<ToolCall> toolCalls)
        {
            var summaries = new List<string>();
            foreach (var call in toolCalls)
            {
                var result = await _toolExecutor.ExecuteToolAsync(call.Name, call.ArgumentsJson);
                if (result.RequiresConfirmation)
                {
                    summaries.Add(result.Message);
                    continue;
                }

                summaries.Add(result.Message);
            }

            return string.Join("\n", summaries);
        }

        private static Dictionary<string, ModelDefinition> BuildModelCatalog()
        {
            return new Dictionary<string, ModelDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["Smartitecture Cloud"] = new ModelDefinition
                {
                    Name = "Smartitecture Cloud",
                    ProviderKey = "backend",
                    ModelId = "smartitecture",
                    Mode = AIModeType.Lumen
                },
                ["Smartitecture Cloud (Fast)"] = new ModelDefinition
                {
                    Name = "Smartitecture Cloud (Fast)",
                    ProviderKey = "backend",
                    ModelId = "smartitecture-fast",
                    Mode = AIModeType.Lumen
                },
                ["Advanced AI Assistant"] = new ModelDefinition
                {
                    Name = "Advanced AI Assistant",
                    ProviderKey = "local",
                    ModelId = "local-fallback",
                    Mode = AIModeType.Lumen
                },
                ["OpenAI GPT-4"] = new ModelDefinition
                {
                    Name = "OpenAI GPT-4",
                    ProviderKey = "openai",
                    ModelId = "gpt-4",
                    Mode = AIModeType.Lumen
                },
                ["OpenAI GPT-3.5-Turbo"] = new ModelDefinition
                {
                    Name = "OpenAI GPT-3.5-Turbo",
                    ProviderKey = "openai",
                    ModelId = "gpt-3.5-turbo",
                    Mode = AIModeType.Lumen
                },
                ["Azure OpenAI GPT-4"] = new ModelDefinition
                {
                    Name = "Azure OpenAI GPT-4",
                    ProviderKey = "azure",
                    ModelId = "gpt-4",
                    Mode = AIModeType.Lumen
                },
                ["Anthropic Claude"] = new ModelDefinition
                {
                    Name = "Anthropic Claude",
                    ProviderKey = "anthropic",
                    ModelId = "claude-3-sonnet",
                    Mode = AIModeType.Lumen
                },
                ["Google Gemini"] = new ModelDefinition
                {
                    Name = "Google Gemini",
                    ProviderKey = "gemini",
                    ModelId = "gemini-1.5-pro",
                    Mode = AIModeType.Lumen
                },
                ["Local Ollama Model"] = new ModelDefinition
                {
                    Name = "Local Ollama Model",
                    ProviderKey = "ollama",
                    ModelId = "llama3",
                    Mode = AIModeType.Lumen
                },
                ["System Expert Mode"] = new ModelDefinition
                {
                    Name = "System Expert Mode",
                    ProviderKey = "local",
                    ModelId = "local-fallback",
                    Mode = AIModeType.Fortis
                }
            };
        }
    }
}
