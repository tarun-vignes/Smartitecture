using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Management;

namespace Smartitecture.Services
{
    /// <summary>
    /// Advanced Multi-Model AI Service with real LLM integration and sophisticated capabilities
    /// </summary>
    public class MultiModelAIService : ILLMService
    {
        private readonly Dictionary<string, List<ConversationMessage>> _conversations;
        private readonly HttpClient _httpClient;
        private readonly Random _random;
        private Dictionary<string, AIModelConfig> _modelConfigs;
        private readonly IntelligentTrainingService _trainingService;
        private readonly KnowledgeBaseService _knowledgeBase;
        private readonly ConfigurationService _configService;
        private readonly HumanLikeConversationEngine _humanConversation;
        private OpenAIService _openAIService;
        private ClaudeService _claudeService;

        public string CurrentModel { get; private set; } = "Advanced AI Assistant";

        public IEnumerable<string> AvailableModels
        {
            get
            {
                var list = new List<string> { "Advanced AI Assistant" };

                // OpenAI availability
                if (_openAIService != null && _configService.IsOpenAIConfigured())
                {
                    list.Add("OpenAI GPT-4");
                    list.Add("OpenAI GPT-3.5-Turbo");
                    // Optional: Azure path only if treated as configured (not implemented here)
                }

                // Claude availability
                if (_claudeService != null && _claudeService.IsConfigured())
                {
                    list.Add("Anthropic Claude 3.5 Sonnet");
                    list.Add("Anthropic Claude 3 Haiku");
                }

                // Keep these hidden unless implemented/configured
                // list.Add("Azure OpenAI GPT-4");
                // list.Add("Local Ollama Model");
                // list.Add("Google Gemini");
                // list.Add("System Expert Mode");

                return list;
            }
        }

        public event EventHandler<ModelSwitchedEventArgs> ModelSwitched;

        public MultiModelAIService()
        {
            _conversations = new Dictionary<string, List<ConversationMessage>>();
            _httpClient = new HttpClient();
            _random = new Random();
            _trainingService = new IntelligentTrainingService();
            _knowledgeBase = new KnowledgeBaseService();
            _configService = new ConfigurationService();
            _humanConversation = new HumanLikeConversationEngine();
            InitializeModelConfigs();
            InitializeOpenAI();
            InitializeClaude();
        }

        public void ReloadProviders()
        {
            InitializeOpenAI();
            InitializeClaude();
        }

        private void InitializeOpenAI()
        {
            try
            {
                if (_configService.IsOpenAIConfigured())
                {
                    var apiKey = _configService.GetOpenAIApiKey();
                    _openAIService = new OpenAIService(apiKey);
                }
            }
            catch (Exception)
            {
                // OpenAI initialization failed - will fall back to mock responses
                _openAIService = null;
            }
        }

        private void InitializeClaude()
        {
            try
            {
                if (_configService.IsClaudeConfigured())
                {
                    _claudeService = new ClaudeService();
                }
            }
            catch
            {
                _claudeService = null;
            }
        }

        private void InitializeModelConfigs()
        {
            _modelConfigs = new Dictionary<string, AIModelConfig>
            {
                ["Advanced AI Assistant"] = new AIModelConfig
                {
                    Name = "Advanced AI Assistant",
                    Type = "Enhanced Mock",
                    SystemPrompt = "You are an advanced AI assistant specialized in system automation, programming, and intelligent task execution."
                },
                ["OpenAI GPT-4"] = new AIModelConfig
                {
                    Name = "OpenAI GPT-4",
                    Type = "OpenAI",
                    SystemPrompt = "You are a highly capable AI assistant with access to system automation tools."
                },
                ["OpenAI GPT-3.5-Turbo"] = new AIModelConfig
                {
                    Name = "OpenAI GPT-3.5-Turbo",
                    Type = "OpenAI",
                    SystemPrompt = "You are a helpful AI assistant focused on providing accurate and concise responses."
                },
                ["Azure OpenAI GPT-4"] = new AIModelConfig
                {
                    Name = "Azure OpenAI GPT-4",
                    Type = "Azure OpenAI",
                    SystemPrompt = "You are a highly capable AI assistant with access to system automation tools."
                },
                ["System Expert Mode"] = new AIModelConfig
                {
                    Name = "System Expert Mode",
                    Type = "System Expert",
                    SystemPrompt = "You are a system administration expert with deep knowledge of Windows, automation, and technical troubleshooting."
                }
            };
        }

        public async Task<string> GetResponseAsync(string message, string conversationId = null)
        {
            await Task.Delay(_random.Next(200, 800));
            return await GenerateAdvancedResponse(message, conversationId);
        }

        public async Task<string> GetStreamingResponseAsync(string message, Action<string> onTokenReceived, string conversationId = null)
        {
            await StoreMessageAsync(conversationId, "user", message);
            
                        // Claude streaming (simulated token stream)
            if ((CurrentModel == "Anthropic Claude 3.5 Sonnet" || CurrentModel == "Anthropic Claude 3 Haiku") &&
                _claudeService != null && _claudeService.IsConfigured())
            {
                try
                {
                    var context = await GetConversationContext(conversationId);
                    var claudeModel = CurrentModel.Contains("Sonnet") ? "claude-3-5-sonnet-20240620" : "claude-3-haiku-20240307";
                    var full = await _claudeService.GetStreamingResponseAsync(message, onTokenReceived, context, claudeModel);
                    await StoreMessageAsync(conversationId, "assistant", full);
                    return full;
                }
                catch { /* fall through to default */ }
            }
// Check if we should use real OpenAI streaming
            if ((CurrentModel == "OpenAI GPT-4" || CurrentModel == "OpenAI GPT-3.5-Turbo") && 
                _openAIService != null && _openAIService.IsConfigured())
            {
                try
                {
                    var context = await GetConversationContext(conversationId);
                    var openAIResponse = await _openAIService.GetStreamingResponseAsync(message, onTokenReceived, context);
                    await StoreMessageAsync(conversationId, "assistant", openAIResponse);
                    return openAIResponse;
                }
                catch (Exception)
                {
                    // Fall back to mock streaming if OpenAI fails
                }
            }
            
            // Use mock streaming for other models or if OpenAI fails
            var response = await GenerateAdvancedResponse(message, conversationId);
            
            // Advanced streaming simulation
            var sentences = response.Split(new[] { ". ", "! ", "? " }, StringSplitOptions.RemoveEmptyEntries);
            var fullResponse = "";

            foreach (var sentence in sentences)
            {
                var words = sentence.Split(' ');
                foreach (var word in words)
                {
                    await Task.Delay(_random.Next(30, 120));
                    var token = (fullResponse.Length == 0 ? "" : " ") + word;
                    fullResponse += token;
                    onTokenReceived?.Invoke(token);
                }
                
                if (!sentence.EndsWith(".") && !sentence.EndsWith("!") && !sentence.EndsWith("?"))
                {
                    fullResponse += ". ";
                    onTokenReceived?.Invoke(". ");
                }
            }

            await StoreMessageAsync(conversationId, "assistant", fullResponse);
            return fullResponse;
        }

        private async Task<string> GenerateAdvancedResponse(string message, string conversationId)
        {
            var lowerMessage = message.ToLower();
            
            // DEBUG: Log which model we're using
            System.Diagnostics.Debug.WriteLine($"[MultiModel] Current Model: '{CurrentModel}' | Message: '{message}'");
            
            // Get conversation context
            var context = await GetConversationContext(conversationId);
            
            switch (CurrentModel)
            {
                case "OpenAI GPT-4":
                case "OpenAI GPT-3.5-Turbo":
                    System.Diagnostics.Debug.WriteLine("[MultiModel] Using OpenAI path");
                    return await GenerateOpenAIResponse(message, context);
                case "System Expert Mode":
                    System.Diagnostics.Debug.WriteLine("[MultiModel] Using System Expert path");
                    return await GenerateSystemExpertResponse(message, context);
                case "Azure OpenAI GPT-4":
                    System.Diagnostics.Debug.WriteLine("[MultiModel] Using Azure OpenAI path");
                    return await GenerateAzureOpenAIResponse(message, context);
                case "Anthropic Claude 3.5 Sonnet":
                case "Anthropic Claude 3 Haiku":
                    System.Diagnostics.Debug.WriteLine("[MultiModel] Using Anthropic Claude path");
                    return await GenerateClaudeResponse(message, context);
                default:
                    System.Diagnostics.Debug.WriteLine("[MultiModel] Using DEFAULT path (Advanced AI)");
                    return await GenerateAdvancedAIResponse(message, context);
            }
        }

        private async Task<string> GenerateOpenAIResponse(string message, List<ConversationMessage> context)
        {
            try
            {
                if (_openAIService != null && _openAIService.IsConfigured())
                {
                    // Use real OpenAI API - it handles knowledge base internally
                    return await _openAIService.GetResponseAsync(message, context);
                }
                else
                {
                    // No OpenAI configured - use our human-like conversation engine
                    return await _humanConversation.GetResponseAsync(message, "openai-fallback", context);
                }
            }
            catch (Exception ex)
            {
                // Error with OpenAI - fallback to human-like conversation
                return await _humanConversation.GetResponseAsync(message, "openai-error", context);
            }
        }

        private async Task<string> GenerateSystemExpertResponse(string message, List<ConversationMessage> context)
        {
            var lowerMessage = message.ToLower();
            
            // System diagnostics
            if (lowerMessage.Contains("system") || lowerMessage.Contains("performance") || lowerMessage.Contains("diagnostic"))
            {
                return await GetSystemDiagnostics();
            }
            
            // Process management
            if (lowerMessage.Contains("process") || lowerMessage.Contains("task") || lowerMessage.Contains("memory"))
            {
                return await GetProcessInformation();
            }
            
            // Network diagnostics
            if (lowerMessage.Contains("network") || lowerMessage.Contains("internet") || lowerMessage.Contains("connection"))
            {
                return "🌐 **Network Diagnostics:**\n\n" +
                       "I can help you with network troubleshooting:\n" +
                       "• Check network connectivity\n" +
                       "• Diagnose DNS issues\n" +
                       "• Monitor network usage\n" +
                       "• Configure network settings\n\n" +
                       "Would you like me to run a specific network diagnostic?";
            }
            
            return "🔧 **System Expert Mode Active**\n\n" +
                   "I'm operating in advanced system administration mode. I can help with:\n" +
                   "• System performance analysis\n" +
                   "• Process and memory management\n" +
                   "• Network diagnostics\n" +
                   "• Security assessments\n" +
                   "• Automation scripting\n\n" +
                   "What system task would you like assistance with?";
        }

        private async Task<string> GetSystemDiagnostics()
        {
            try
            {
                var diagnostics = new StringBuilder();
                diagnostics.AppendLine("🖥️ **System Diagnostics Report:**\n");
                
                // Basic system info
                diagnostics.AppendLine($"• **Computer:** {Environment.MachineName}");
                diagnostics.AppendLine($"• **OS:** {Environment.OSVersion}");
                diagnostics.AppendLine($"• **Processors:** {Environment.ProcessorCount} cores");
                diagnostics.AppendLine($"• **User:** {Environment.UserName}");
                diagnostics.AppendLine($"• **.NET Version:** {Environment.Version}\n");
                
                // Memory info
                var totalMemory = GC.GetTotalMemory(false);
                diagnostics.AppendLine($"• **Current Memory Usage:** {totalMemory / 1024 / 1024:N0} MB");
                
                // Uptime
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                diagnostics.AppendLine($"• **System Uptime:** {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m\n");
                
                diagnostics.AppendLine("Would you like more detailed diagnostics for any specific component?");
                
                return diagnostics.ToString();
            }
            catch (Exception ex)
            {
                return $"⚠️ Error gathering system diagnostics: {ex.Message}";
            }
        }

        private async Task<string> GetProcessInformation()
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcesses()
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(10)
                    .ToList();
                
                var info = new StringBuilder();
                info.AppendLine("📊 **Top 10 Processes by Memory Usage:**\n");
                
                foreach (var process in processes)
                {
                    try
                    {
                        var memoryMB = process.WorkingSet64 / 1024 / 1024;
                        info.AppendLine($"• **{process.ProcessName}** - {memoryMB:N0} MB");
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }
                
                info.AppendLine("\nWould you like me to help manage any specific processes?");
                return info.ToString();
            }
            catch (Exception ex)
            {
                return $"⚠️ Error gathering process information: {ex.Message}";
            }
        }

        private async Task<string> GenerateAzureOpenAIResponse(string message, List<ConversationMessage> context)
        {
            // Simulate Azure OpenAI response (in real implementation, this would call Azure OpenAI API)
            await Task.Delay(500);
            
            return "🧠 **Azure OpenAI GPT-4 Response:**\n\n" +
                   "I'm simulating an Azure OpenAI GPT-4 response. In a real implementation, this would:\n" +
                   "• Connect to Azure OpenAI Service\n" +
                   "• Use your API key and endpoint\n" +
                   "• Provide state-of-the-art AI responses\n" +
                   "• Support advanced reasoning and code generation\n\n" +
                   "To enable real Azure OpenAI integration, configure your API credentials in the settings.";
        }

        private async Task<string> GenerateAdvancedAIResponse(string message, List<ConversationMessage> context)
        {
            // DEBUG: Log that we're using the human conversation engine
            System.Diagnostics.Debug.WriteLine($"[MultiModel] Using Human-Like Conversation Engine for: '{message}'");
            
            // Use human-like conversation engine with emotional intelligence
            var response = await _humanConversation.GetResponseAsync(message, "default", context);
            
            System.Diagnostics.Debug.WriteLine($"[MultiModel] Human engine returned: '{response}'");
            return response;
        }

        private string HandleAdvancedMath(string message)
        {
            try
            {
                // Enhanced math parsing
                var patterns = new[]
                {
                    @"(\d+(?:\.\d+)?)\s*([+\-*/])\s*(\d+(?:\.\d+)?)",
                    @"sqrt\((\d+(?:\.\d+)?)\)",
                    @"(\d+(?:\.\d+)?)\s*\^\s*(\d+(?:\.\d+)?)"
                };
                
                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(message, pattern);
                    if (match.Success)
                    {
                        if (pattern.Contains("sqrt"))
                        {
                            var num = double.Parse(match.Groups[1].Value);
                            var result = Math.Sqrt(num);
                            return $"🧮 **Advanced Calculator:**\n\n√{num} = {result:F4}\n\nNeed help with more complex calculations?";
                        }
                        else if (pattern.Contains("^"))
                        {
                            var baseNum = double.Parse(match.Groups[1].Value);
                            var exponent = double.Parse(match.Groups[2].Value);
                            var result = Math.Pow(baseNum, exponent);
                            return $"🧮 **Advanced Calculator:**\n\n{baseNum}^{exponent} = {result:F4}\n\nI can handle complex mathematical operations!";
                        }
                        else
                        {
                            var num1 = double.Parse(match.Groups[1].Value);
                            var operation = match.Groups[2].Value;
                            var num2 = double.Parse(match.Groups[3].Value);
                            
                            double result = operation switch
                            {
                                "+" => num1 + num2,
                                "-" => num1 - num2,
                                "*" => num1 * num2,
                                "/" => num2 != 0 ? num1 / num2 : double.NaN,
                                _ => double.NaN
                            };
                            
                            if (!double.IsNaN(result))
                            {
                                return $"🧮 **Advanced Calculator:**\n\n{num1} {operation} {num2} = {result}\n\nI can also handle square roots, powers, and more complex equations!";
                            }
                        }
                    }
                }
            }
            catch { }
            
            return "🧮 **Advanced Calculator Ready!**\n\nI can help with:\n• Basic arithmetic (15 + 27, 100 / 4)\n• Square roots (sqrt(25))\n• Powers (2^8)\n• Complex equations\n\nWhat would you like to calculate?";
        }

        private string GetCodingAssistance(string message)
        {
            return "💻 **Programming Assistant:**\n\n" +
                   "I can help you with:\n" +
                   "• **Code Generation** - Write functions, classes, scripts\n" +
                   "• **Debugging** - Find and fix code issues\n" +
                   "• **Code Review** - Optimize and improve code\n" +
                   "• **Architecture** - Design patterns and best practices\n" +
                   "• **Multiple Languages** - C#, Python, JavaScript, PowerShell\n\n" +
                   "What programming task can I help you with?";
        }

        private string GetAutomationHelp(string message)
        {
            return "🤖 **Automation Specialist:**\n\n" +
                   "I can help you automate:\n" +
                   "• **File Operations** - Batch rename, organize, backup\n" +
                   "• **System Tasks** - Scheduled operations, maintenance\n" +
                   "• **Application Workflows** - Launch sequences, data processing\n" +
                   "• **Network Operations** - Monitoring, diagnostics\n" +
                   "• **Custom Scripts** - PowerShell, batch files\n\n" +
                   "What workflow would you like to automate?";
        }

        private string GetFileOperationHelp(string message)
        {
            return "📁 **File System Expert:**\n\n" +
                   "I can assist with:\n" +
                   "• **File Management** - Copy, move, rename, delete\n" +
                   "• **Directory Operations** - Create, organize, clean up\n" +
                   "• **Search & Filter** - Find files by criteria\n" +
                   "• **Batch Operations** - Process multiple files\n" +
                   "• **Backup & Sync** - Data protection strategies\n\n" +
                   "What file operation do you need help with?";
        }

        private string GetTimeAndSchedulingInfo(string message)
        {
            var now = DateTime.Now;
            return $"⏰ **Time & Scheduling Assistant:**\n\n" +
                   $"**Current Time:** {now:HH:mm:ss}\n" +
                   $"**Date:** {now:dddd, MMMM dd, yyyy}\n" +
                   $"**Time Zone:** {TimeZoneInfo.Local.DisplayName}\n\n" +
                   "I can help with:\n" +
                   "• Task scheduling and reminders\n" +
                   "• Time zone conversions\n" +
                   "• Calendar management\n" +
                   "• Automated time-based tasks\n\n" +
                   "What time-related task can I assist with?";
        }

        private string GetContextAwareResponse(string message, List<ConversationMessage> context)
        {
            var responses = new[]
            {
                "🎯 **Advanced AI Assistant Ready!**\n\nI'm equipped with sophisticated capabilities to help you with complex tasks. What would you like to accomplish?",
                "🚀 **Multi-Model AI at Your Service!**\n\nI can switch between different AI models and expertise modes. How can I assist you today?",
                "🧠 **Intelligent Assistant Active!**\n\nI understand context, remember our conversation, and can execute complex workflows. What's your goal?",
                "⚡ **Advanced Automation Ready!**\n\nI can help with system tasks, programming, calculations, and intelligent automation. What do you need?",
                "🔧 **Expert System Online!**\n\nI have access to system diagnostics, file operations, and advanced AI capabilities. How can I help?"
            };
            
            return responses[_random.Next(responses.Length)];
        }

        // Additional methods for conversation management, command parsing, etc.
        public async Task<(string commandName, Dictionary<string, object> parameters)> ParseCommandAsync(string message)
        {
            await Task.Delay(100);
            var lowerMessage = message.ToLower();

            // Enhanced command parsing with AI-powered intent recognition
            var commandPatterns = new Dictionary<string, string[]>
            {
                ["calculator"] = new[] { "calculator", "calc", "calculate", "math" },
                ["explorer"] = new[] { "explorer", "files", "file manager", "browse files" },
                ["taskmanager"] = new[] { "task manager", "taskmgr", "processes", "performance" },
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
            await Task.Delay(50);
            return string.IsNullOrEmpty(conversationId) || !_conversations.ContainsKey(conversationId) 
                ? new List<ConversationMessage>() 
                : new List<ConversationMessage>(_conversations[conversationId]);
        }

        private async Task<List<ConversationMessage>> GetConversationContext(string conversationId)
        {
            return await GetConversationHistoryAsync(conversationId);
        }

        public async Task ClearConversationAsync(string conversationId)
        {
            await Task.Delay(50);
            if (!string.IsNullOrEmpty(conversationId) && _conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId].Clear();
            }
        }

        public async Task<bool> SwitchModelAsync(string modelName)
        {
            await Task.Delay(300);
            if (AvailableModels.Contains(modelName))
            {
                var previousModel = CurrentModel;
                CurrentModel = modelName;
                
                ModelSwitched?.Invoke(this, new ModelSwitchedEventArgs
                {
                    PreviousModel = previousModel,
                    NewModel = modelName
                });

                return true;
            }
            return false;
        }

                private async Task<string> GenerateClaudeResponse(string message, List<ConversationMessage> context)
        {
            try
            {
                if (_claudeService != null && _claudeService.IsConfigured())
                {
                    var model = CurrentModel.Contains("Sonnet") ? "claude-3-5-sonnet-20240620" : "claude-3-haiku-20240307";
                    return await _claudeService.GetResponseAsync(message, context, model);
                }
                else
                {
                    return await _humanConversation.GetResponseAsync(message, "claude-fallback", context);
                }
            }
            catch
            {
                return await _humanConversation.GetResponseAsync(message, "claude-error", context);
            }
        }
private async Task StoreMessageAsync(string conversationId, string role, string content)
        {
            if (string.IsNullOrEmpty(conversationId)) return;

            if (!_conversations.ContainsKey(conversationId))
                _conversations[conversationId] = new List<ConversationMessage>();

            _conversations[conversationId].Add(new ConversationMessage
            {
                Role = role,
                Content = content,
                Model = CurrentModel,
                Timestamp = DateTime.UtcNow
            });

            await Task.CompletedTask;
        }
    }

    public class AIModelConfig
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string SystemPrompt { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string Endpoint { get; set; } = "";
    }
}







