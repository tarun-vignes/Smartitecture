using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace Smartitecture.Services
{
    /// <summary>
    /// Enhanced Mock LLM Service with intelligent responses and better command recognition
    /// </summary>
    public class EnhancedMockLLMService : ILLMService
    {
        private readonly Dictionary<string, List<ConversationMessage>> _conversations;
        private readonly Random _random;

        public string CurrentModel { get; private set; } = "Enhanced AI Model";

        public IEnumerable<string> AvailableModels => new[] 
        { 
            "Enhanced AI Model", 
            "Azure OpenAI", 
            "Local Model", 
            "Fallback" 
        };

        public event EventHandler<ModelSwitchedEventArgs> ModelSwitched;

        public EnhancedMockLLMService()
        {
            _conversations = new Dictionary<string, List<ConversationMessage>>();
            _random = new Random();
        }

        public async Task<string> GetResponseAsync(string message, string conversationId = null)
        {
            await Task.Delay(_random.Next(300, 1000)); // Simulate processing time
            
            return GenerateIntelligentResponse(message);
        }

        public async Task<string> GetStreamingResponseAsync(string message, Action<string> onTokenReceived, string conversationId = null)
        {
            // Store user message
            await StoreMessageAsync(conversationId, "user", message);

            // Generate response
            var response = GenerateIntelligentResponse(message);
            
            // Simulate streaming by sending tokens
            var words = response.Split(' ');
            var fullResponse = "";

            foreach (var word in words)
            {
                await Task.Delay(_random.Next(50, 200)); // Simulate network delay
                var token = (fullResponse.Length == 0 ? "" : " ") + word;
                fullResponse += token;
                onTokenReceived?.Invoke(token);
            }

            // Store AI response
            await StoreMessageAsync(conversationId, "assistant", fullResponse);

            return fullResponse;
        }

        private string GenerateIntelligentResponse(string message)
        {
            var lowerMessage = message.ToLower();
            
            // Math calculations
            if (lowerMessage.Contains("what is") && (lowerMessage.Contains("+") || lowerMessage.Contains("-") || lowerMessage.Contains("*") || lowerMessage.Contains("/")))
            {
                return HandleMathQuestion(message);
            }
            
            // Time-related queries
            if (lowerMessage.Contains("time") || lowerMessage.Contains("date"))
            {
                return $"The current time is {DateTime.Now:HH:mm:ss} and today's date is {DateTime.Now:MMMM dd, yyyy}.";
            }
            
            // System information
            if (lowerMessage.Contains("system") || lowerMessage.Contains("computer") || lowerMessage.Contains("pc"))
            {
                return GetSystemInfo();
            }
            
            // File operations
            if (lowerMessage.Contains("file") || lowerMessage.Contains("folder") || lowerMessage.Contains("directory"))
            {
                return "I can help you with file operations! Try commands like:\n• 'list files in Documents'\n• 'create folder called MyProject'\n• 'open file explorer'";
            }
            
            // Application launching
            if (lowerMessage.Contains("open") || lowerMessage.Contains("launch") || lowerMessage.Contains("start"))
            {
                return HandleAppLaunchRequest(message);
            }
            
            // Automation help
            if (lowerMessage.Contains("automate") || lowerMessage.Contains("automation"))
            {
                return "I can help you automate various tasks:\n• System commands (shutdown, restart, volume)\n• File operations (copy, move, organize)\n• Application launching\n• Network diagnostics\n• Scheduled tasks\n\nWhat would you like to automate?";
            }
            
            // Help requests
            if (lowerMessage.Contains("help") || lowerMessage.Contains("what can you do"))
            {
                return GetHelpResponse();
            }
            
            // Greeting responses
            if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi") || lowerMessage.Contains("hey"))
            {
                return GetGreetingResponse();
            }
            
            // Settings and configuration
            if (lowerMessage.Contains("setting") || lowerMessage.Contains("config") || lowerMessage.Contains("preference"))
            {
                return "I can help you configure various settings:\n• Display settings\n• Network configuration\n• System preferences\n• Application settings\n\nWhat would you like to configure?";
            }
            
            // Default intelligent responses based on context
            return GetContextualResponse(message);
        }

        private string HandleMathQuestion(string message)
        {
            try
            {
                // Simple math parsing for basic operations
                var match = Regex.Match(message, @"(\d+(?:\.\d+)?)\s*([+\-*/])\s*(\d+(?:\.\d+)?)");
                if (match.Success)
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
                        return $"The answer is {result}. Is there anything else you'd like me to calculate?";
                    }
                }
            }
            catch
            {
                // Fall through to default response
            }
            
            return "I can help with basic math! Try asking something like 'What is 15 + 27?' or 'Calculate 100 / 4'.";
        }

        private string GetSystemInfo()
        {
            try
            {
                var osVersion = Environment.OSVersion.ToString();
                var machineName = Environment.MachineName;
                var userName = Environment.UserName;
                var processorCount = Environment.ProcessorCount;
                
                return $"Here's your system information:\n" +
                       $"• Computer: {machineName}\n" +
                       $"• User: {userName}\n" +
                       $"• OS: {osVersion}\n" +
                       $"• Processors: {processorCount} cores\n" +
                       $"• .NET Version: {Environment.Version}\n\n" +
                       $"Would you like more detailed system information?";
            }
            catch
            {
                return "I can provide system information, but I'm having trouble accessing it right now. Try asking about specific system details!";
            }
        }

        private string HandleAppLaunchRequest(string message)
        {
            var lowerMessage = message.ToLower();
            
            if (lowerMessage.Contains("notepad"))
                return "I can launch Notepad for you! Use the command 'launch notepad' and I'll open it right away.";
            
            if (lowerMessage.Contains("calculator"))
                return "I can open Calculator! Try the command 'launch calc' or 'open calculator'.";
            
            if (lowerMessage.Contains("browser") || lowerMessage.Contains("chrome") || lowerMessage.Contains("edge"))
                return "I can open your web browser! Use commands like 'launch chrome' or 'open browser'.";
            
            if (lowerMessage.Contains("explorer") || lowerMessage.Contains("file manager"))
                return "I can open File Explorer! Try 'launch explorer' or 'open file manager'.";
            
            return "I can help you launch applications! Some examples:\n• 'launch notepad'\n• 'open calculator'\n• 'start chrome'\n• 'open file explorer'\n\nWhat application would you like to open?";
        }

        private string GetHelpResponse()
        {
            return "🤖 I'm your Smartitecture AI Assistant! Here's what I can help you with:\n\n" +
                   "📋 **Commands & Automation:**\n" +
                   "• Launch applications (notepad, calculator, browser)\n" +
                   "• System operations (shutdown, restart, volume control)\n" +
                   "• File operations (create, move, organize files)\n\n" +
                   "💬 **Conversations:**\n" +
                   "• Answer questions and provide information\n" +
                   "• Perform calculations\n" +
                   "• System information and diagnostics\n\n" +
                   "⚙️ **Configuration:**\n" +
                   "• Help with system settings\n" +
                   "• Network configuration\n" +
                   "• Application preferences\n\n" +
                   "Just ask me naturally, like 'open notepad' or 'what's 25 + 17?' or 'help me automate my workflow'!";
        }

        private string GetGreetingResponse()
        {
            var greetings = new[]
            {
                "Hello! I'm your Smartitecture AI Assistant. How can I help you today?",
                "Hi there! Ready to help you with automation and system tasks. What do you need?",
                "Hey! I'm here to assist with your computer tasks and answer questions. What's on your mind?",
                "Greetings! Your AI assistant is ready. What would you like to accomplish today?",
                "Hello! I can help you automate tasks, launch applications, and answer questions. How can I assist?"
            };
            
            return greetings[_random.Next(greetings.Length)];
        }

        private string GetContextualResponse(string message)
        {
            var responses = new[]
            {
                "That's an interesting request! Could you provide more details about what you'd like to accomplish?",
                "I understand what you're asking. Let me help you with that step by step.",
                "Great question! Based on what you've said, I can suggest a few approaches.",
                "I'm here to help! Could you be more specific about what you need assistance with?",
                "That sounds like something I can definitely help you with. What's your goal?",
                "Interesting! I can assist with that. Would you like me to break it down into steps?",
                "I can help you with that task. What's the specific outcome you're looking for?",
                "That's within my capabilities! Let me know if you need me to execute any commands.",
                "I understand your request. Would you like me to provide options or take action directly?",
                "Perfect! I can guide you through that process. What's your preferred approach?"
            };
            
            return responses[_random.Next(responses.Length)];
        }

        public async Task<(string commandName, Dictionary<string, object> parameters)> ParseCommandAsync(string message)
        {
            await Task.Delay(100); // Simulate processing

            var lowerMessage = message.ToLower();

            // Calculator commands
            if (lowerMessage.Contains("calculator") || lowerMessage.Contains("calc"))
            {
                return ("calculator", new Dictionary<string, object>());
            }

            // File Explorer commands
            if (lowerMessage.Contains("explorer") || lowerMessage.Contains("file manager") || lowerMessage.Contains("files"))
            {
                return ("explorer", new Dictionary<string, object>());
            }

            // Task Manager commands
            if (lowerMessage.Contains("task manager") || lowerMessage.Contains("taskmgr") || lowerMessage.Contains("processes"))
            {
                return ("taskmanager", new Dictionary<string, object>());
            }

            // Enhanced launch command parsing
            if (lowerMessage.Contains("launch") || lowerMessage.Contains("open") || lowerMessage.Contains("start"))
            {
                // Check for specific applications
                if (lowerMessage.Contains("notepad"))
                    return ("launch", new Dictionary<string, object> { ["app"] = "notepad" });
                if (lowerMessage.Contains("calculator") || lowerMessage.Contains("calc"))
                    return ("calculator", new Dictionary<string, object>());
                if (lowerMessage.Contains("explorer") || lowerMessage.Contains("files"))
                    return ("explorer", new Dictionary<string, object>());
                if (lowerMessage.Contains("task manager") || lowerMessage.Contains("taskmgr"))
                    return ("taskmanager", new Dictionary<string, object>());

                // Generic app launch
                var appMatch = Regex.Match(message, @"(?:launch|open|start)\s+(\w+)", RegexOptions.IgnoreCase);
                if (appMatch.Success)
                {
                    var app = appMatch.Groups[1].Value.ToLower();
                    return (app switch
                    {
                        "calc" or "calculator" => "calculator",
                        "explorer" or "files" => "explorer",
                        "taskmgr" or "taskmanager" => "taskmanager",
                        _ => "launch"
                    }, new Dictionary<string, object> { ["app"] = app });
                }
                return ("launch", new Dictionary<string, object> { ["app"] = "notepad" });
            }

            // Shutdown commands
            if (lowerMessage.Contains("shutdown") || lowerMessage.Contains("shut down"))
            {
                var timeMatch = Regex.Match(message, @"(\d+)\s*(?:second|minute|hour)", RegexOptions.IgnoreCase);
                if (timeMatch.Success)
                {
                    var time = int.Parse(timeMatch.Groups[1].Value);
                    if (lowerMessage.Contains("minute")) time *= 60;
                    if (lowerMessage.Contains("hour")) time *= 3600;
                    return ("shutdown", new Dictionary<string, object> { ["delay"] = time.ToString() });
                }
                return ("shutdown", new Dictionary<string, object>());
            }

            return (null, null);
        }

        public async Task<List<ConversationMessage>> GetConversationHistoryAsync(string conversationId)
        {
            await Task.Delay(50);
            
            if (string.IsNullOrEmpty(conversationId) || !_conversations.ContainsKey(conversationId))
            {
                return new List<ConversationMessage>();
            }

            return new List<ConversationMessage>(_conversations[conversationId]);
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
            await Task.Delay(500); // Simulate model switching delay

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

        private async Task StoreMessageAsync(string conversationId, string role, string content)
        {
            if (string.IsNullOrEmpty(conversationId))
                return;

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
    }
}
