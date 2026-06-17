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
        private const AIModeType UnifiedMode = AIModeType.Lumen;

        public string CurrentModel { get; private set; } = "Advanced AI Assistant";
        public AIModeType CurrentMode => UnifiedMode;

        public IEnumerable<string> AvailableModels => _modelCatalog.Values
            .Where(IsModelAvailable)
            .Select(m => m.Name)
            .ToList();

        public IEnumerable<AIModeType> AvailableModes => new[] { UnifiedMode };

        public event EventHandler<ModelSwitchedEventArgs>? ModelSwitched;
        public event EventHandler<ModeSwitchedEventArgs>? ModeSwitched
        {
            add { }
            remove { }
        }

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

            _modelCatalog = BuildModelCatalog(ProviderSettings.Load());

            if (!IsModelAvailable(GetModelDefinition(CurrentModel)))
            {
                var firstAvailable = _modelCatalog.Values.FirstOrDefault(IsModelAvailable);
                if (firstAvailable != null)
                {
                    CurrentModel = firstAvailable.Name;
                }
            }

        }

        public async Task<string> GetResponseAsync(string message, string? conversationId = null)
        {
            var request = BuildRequest(message, conversationId);
            var provider = GetProvider(request);

            await StoreMessageAsync(conversationId, "user", message);
            var response = await GetResponseWithFallbackAsync(provider, request);
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
            await foreach (var token in StreamWithFallbackAsync(provider, request, CancellationToken.None))
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

            if (IsSystemInfoRequest(lowerMessage))
            {
                return ("system_info", new Dictionary<string, object>());
            }

            if (IsBatteryStatusRequest(lowerMessage))
            {
                return ("battery_status", new Dictionary<string, object>());
            }

            if (IsDefenderScanStatusRequest(lowerMessage))
            {
                return ("defender_scan_status", new Dictionary<string, object>());
            }

            if (IsDefenderScanRequest(lowerMessage))
            {
                var full = lowerMessage.Contains("full") || lowerMessage.Contains("complete");
                return ("defender_scan", new Dictionary<string, object> { ["full"] = full });
            }

            if (IsDefenderStatusRequest(lowerMessage))
            {
                return ("defender_status", new Dictionary<string, object>());
            }

            if (IsPerformanceRequest(lowerMessage))
            {
                return ("performance_snapshot", new Dictionary<string, object>());
            }

            if (IsProcessListRequest(lowerMessage))
            {
                return ("list_processes", new Dictionary<string, object> { ["count"] = 12 });
            }

            var killMatch = Regex.Match(lowerMessage, @"\b(kill|stop|end|terminate)\s+(process\s+)?(?<target>[\w\-.]+)");
            if (killMatch.Success)
            {
                var target = killMatch.Groups["target"].Value.Trim();
                if (int.TryParse(target, out var pid))
                {
                    return ("kill_process", new Dictionary<string, object> { ["pid"] = pid });
                }

                return ("kill_process", new Dictionary<string, object> { ["name"] = target });
            }

            if (IsNetworkRequest(lowerMessage))
            {
                return ("network_adapters", new Dictionary<string, object>());
            }

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
                ["calculator"] = new[] { "calculator", "calc" },
                ["explorer"] = new[] { "explorer", "files", "file manager", "browse files" },
                ["taskmgr"] = new[] { "task manager", "taskmgr", "processes", "performance" },
                ["shutdown"] = new[] { "shutdown", "shut down", "power off", "turn off" },
                ["launch"] = new[] { "launch app", "open app", "start app", "run app" }
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

        private static bool IsSystemInfoRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "system info",
                "system information",
                "computer info",
                "computer information",
                "machine info",
                "about this pc",
                "information of my pc",
                "info of my pc",
                "my pc information",
                "my pc info",
                "information about my pc",
                "information on my pc",
                "check my pc",
                "check this pc",
                "pc health",
                "computer health",
                "health check",
                "device info",
                "device information",
                "computer specs",
                "pc specs",
                "device specs");
        }

        private static bool IsPerformanceRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "performance",
                "cpu usage",
                "memory usage",
                "ram usage",
                "system usage",
                "resource usage",
                "check performance",
                "performance check",
                "diagnose performance",
                "diagnose my pc",
                "diagnose my computer",
                "what is slowing",
                "what's slowing",
                "how is my pc running",
                "system status",
                "why is my pc slow",
                "why is my computer slow",
                "pc slow",
                "computer slow",
                "running slow",
                "laggy",
                "lagging",
                "why is my pc hot",
                "why is my computer hot",
                "pc hot",
                "computer hot",
                "overheating",
                "fan loud",
                "fan noise",
                "temperature");
        }

        private static bool IsBatteryStatusRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "battery",
                "battery percentage",
                "battery percent",
                "battery level",
                "battery status",
                "how much battery",
                "charge remaining",
                "charge percentage",
                "power level");
        }

        private static bool IsProcessListRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "list processes",
                "show processes",
                "running processes",
                "what processes",
                "top processes",
                "process list",
                "what is using ram",
                "what's using ram",
                "what is using my ram",
                "what's using my ram",
                "what is using memory",
                "what's using memory",
                "what is using my memory",
                "what's using my memory",
                "what is using cpu",
                "what's using cpu",
                "what is using my cpu",
                "what's using my cpu",
                "highest memory",
                "highest cpu");
        }

        private static bool IsNetworkRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "network adapter",
                "network adapters",
                "wifi adapter",
                "wi-fi adapter",
                "ip address",
                "show my ip",
                "what is my ip",
                "what's my ip",
                "wifi status",
                "wi-fi status",
                "network status");
        }

        private static bool IsDefenderStatusRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "defender status",
                "windows defender status",
                "antivirus status",
                "security status",
                "malicious files",
                "malware files",
                "virus files",
                "any files that are malicious",
                "files are malicious",
                "file malicious",
                "malware",
                "virus",
                "is my pc safe",
                "is my computer safe",
                "am i protected",
                "am i secure");
        }

        private static bool IsDefenderScanStatusRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "scan status",
                "scan result",
                "scan results",
                "defender scan status",
                "defender scan result",
                "defender scan results",
                "is the scan done",
                "is scan done",
                "did the scan finish",
                "did the scan find anything",
                "did my scan find anything",
                "did scan find anything",
                "did it find anything",
                "did defender find anything",
                "any threats found",
                "were threats found",
                "what did the scan find");
        }

        private static bool IsDefenderScanRequest(string lowerMessage)
        {
            return ContainsAny(lowerMessage,
                "defender scan",
                "windows defender scan",
                "antivirus scan",
                "security scan",
                "scan my pc",
                "scan computer");
        }

        private static bool ContainsAny(string value, params string[] patterns)
        {
            return patterns.Any(value.Contains);
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

            return true;
        }

        public async Task<bool> SwitchModeAsync(AIModeType mode)
        {
            await Task.Delay(80);
            return mode == UnifiedMode;
        }

        private LLMRequest BuildRequest(string message, string? conversationId)
        {
            var history = GetConversationContext(conversationId);
            var def = GetModelDefinition(CurrentModel);
            var mode = _modeRouter.GetMode(UnifiedMode);

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
            if (def.ProviderKey.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                if (_providers.TryGetValue("backend", out var backend) && backend.IsConfigured)
                {
                    return backend;
                }

                return _providers["local"];
            }

            if (_providers.TryGetValue(def.ProviderKey, out var provider) && provider.IsConfigured)
            {
                return provider;
            }

            // Fallback to local if desired provider is not configured
            return _providers["local"];
        }

        private async Task<LLMResponse> GetResponseWithFallbackAsync(ILLMProvider provider, LLMRequest request)
        {
            try
            {
                return await provider.GetResponseAsync(request, CancellationToken.None);
            }
            catch (Exception ex) when (!IsLocalProvider(provider))
            {
                var local = _providers["local"];
                var fallback = await local.GetResponseAsync(request, CancellationToken.None);
                fallback.Content = BuildServerFallbackMessage(ex, fallback.Content);
                return fallback;
            }
        }

        private async IAsyncEnumerable<string> StreamWithFallbackAsync(
            ILLMProvider provider,
            LLMRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            LLMResponse response;
            try
            {
                response = await provider.GetResponseAsync(request, cancellationToken);
            }
            catch (Exception ex) when (!IsLocalProvider(provider))
            {
                response = await _providers["local"].GetResponseAsync(request, cancellationToken);
                response.Content = BuildServerFallbackMessage(ex, response.Content);
            }

            foreach (var token in SplitForStreaming(response.Content))
            {
                yield return token;
            }
        }

        private static IEnumerable<string> SplitForStreaming(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                yield break;
            }

            var words = content.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                yield return i == 0 ? words[i] : " " + words[i];
            }
        }

        private static string BuildServerFallbackMessage(Exception ex, string localResponse)
        {
            var detail = ex.Message;
            if (detail.Length > 160)
            {
                detail = detail.Substring(0, 160).TrimEnd() + "...";
            }

            return $"I couldn't reach the Smartitecture server, so I used the local assistant instead.\n\nReason: {detail}\n\n{localResponse}";
        }

        private static bool IsLocalProvider(ILLMProvider provider)
        {
            return provider.Name.Contains("local", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsModelAvailable(ModelDefinition definition)
        {
            if (definition.ProviderKey.Equals("local", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (definition.ProviderKey.Equals("auto", StringComparison.OrdinalIgnoreCase))
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

        private static Dictionary<string, ModelDefinition> BuildModelCatalog(ProviderSettings settings)
        {
            var ollamaModel = string.IsNullOrWhiteSpace(settings.Ollama.Model)
                ? "llama3"
                : settings.Ollama.Model;
            var ollamaSmallModel = string.IsNullOrWhiteSpace(settings.Ollama.SmallModel)
                ? string.Empty
                : settings.Ollama.SmallModel;

            var catalog = new Dictionary<string, ModelDefinition>(StringComparer.OrdinalIgnoreCase)
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
                    ProviderKey = "auto",
                    ModelId = "smartitecture",
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
                    ModelId = ollamaModel,
                    Mode = AIModeType.Lumen
                }
            };

            if (!string.IsNullOrWhiteSpace(ollamaSmallModel))
            {
                catalog["Local CPU (Small)"] = new ModelDefinition
                {
                    Name = "Local CPU (Small)",
                    ProviderKey = "ollama",
                    ModelId = ollamaSmallModel,
                    Mode = AIModeType.Lumen
                };
            }

            return catalog;
        }
    }
}
