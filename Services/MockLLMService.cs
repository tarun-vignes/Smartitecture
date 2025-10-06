using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    /// <summary>
    /// Mock LLM Service for testing and development
    /// </summary>
    public class MockLLMService : ILLMService
    {
        private readonly Dictionary<string, List<ConversationMessage>> _conversations;
        private readonly Random _random;

        public string CurrentModel { get; private set; } = "Mock AI Model";

        public IEnumerable<string> AvailableModels => new[] 
        { 
            "Mock AI Model", 
            "Azure OpenAI", 
            "Local Model", 
            "Fallback" 
        };

        public event EventHandler<ModelSwitchedEventArgs> ModelSwitched;

        public MockLLMService()
        {
            _conversations = new Dictionary<string, List<ConversationMessage>>();
            _random = new Random();
        }

        public async Task<string> GetResponseAsync(string message, string conversationId = null)
        {
            // Simulate processing delay
            await Task.Delay(_random.Next(500, 2000));

            // Store conversation
            await StoreMessageAsync(conversationId, "user", message);

            // Generate mock response
            var response = GenerateMockResponse(message);
            
            // Store AI response
            await StoreMessageAsync(conversationId, "assistant", response);

            return response;
        }

        public async Task<string> GetStreamingResponseAsync(string message, Action<string> onTokenReceived, string conversationId = null)
        {
            // Store user message
            await StoreMessageAsync(conversationId, "user", message);

            // Generate response
            var response = GenerateMockResponse(message);
            
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

        public async Task<(string commandName, Dictionary<string, object> parameters)> ParseCommandAsync(string message)
        {
            await Task.Delay(100); // Simulate processing

            var lowerMessage = message.ToLower();

            // Simple command parsing
            if (lowerMessage.Contains("launch") || lowerMessage.Contains("open") || lowerMessage.Contains("start"))
            {
                var appMatch = Regex.Match(message, @"(?:launch|open|start)\s+(\w+)", RegexOptions.IgnoreCase);
                if (appMatch.Success)
                {
                    return ("launch", new Dictionary<string, object> { ["app"] = appMatch.Groups[1].Value });
                }
                return ("launch", new Dictionary<string, object> { ["app"] = "notepad" });
            }

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

        private string GenerateMockResponse(string message)
        {
            var lowerMessage = message.ToLower();

            // Command-specific responses
            if (lowerMessage.Contains("launch") || lowerMessage.Contains("open") || lowerMessage.Contains("start"))
            {
                return "I'll help you launch that application. Let me execute the launch command for you.";
            }

            if (lowerMessage.Contains("shutdown") || lowerMessage.Contains("shut down"))
            {
                return "I understand you want to shutdown the system. I'll execute the shutdown command with appropriate safety measures.";
            }

            if (lowerMessage.Contains("help") || lowerMessage.Contains("what can you do"))
            {
                return "I'm Smartitecture AI Assistant! I can help you with:\n\n" +
                       "🔧 **System Commands**: Launch applications, shutdown system, manage processes\n" +
                       "🤖 **Automation**: Execute complex workflows and scripts\n" +
                       "💬 **Conversation**: Answer questions and provide assistance\n" +
                       "🔍 **Analysis**: Help analyze system performance and logs\n\n" +
                       "Just tell me what you'd like to do in natural language!";
            }

            if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi") || lowerMessage.Contains("hey"))
            {
                var greetings = new[]
                {
                    "Hello! I'm your Smartitecture AI Assistant. How can I help you today?",
                    "Hi there! Ready to help you with automation and system tasks. What would you like to do?",
                    "Hey! I'm here to assist with your desktop automation needs. What can I help you with?",
                    "Greetings! Your AI assistant is ready. What task would you like me to help you with?"
                };
                return greetings[_random.Next(greetings.Length)];
            }

            // General responses
            var responses = new[]
            {
                "That's an interesting question! As your AI assistant, I'm here to help with automation and system tasks. Could you be more specific about what you'd like me to do?",
                "I understand you're looking for assistance. I can help with launching applications, system commands, and automation tasks. What would you like me to help you with?",
                "Thanks for reaching out! I'm designed to help with desktop automation and system management. Is there a specific task or command you'd like me to execute?",
                "I'm here to help! I can assist with various automation tasks, system commands, and application management. What would you like to accomplish?",
                "Great question! As your Smartitecture assistant, I can help automate tasks and manage your system. What specific action would you like me to take?"
            };

            return responses[_random.Next(responses.Length)];
        }
    }
}
