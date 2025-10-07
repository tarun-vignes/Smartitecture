using System;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using System.Collections.Generic;
using System.Linq;

namespace Smartitecture.Services
{
    /// <summary>
    /// Real OpenAI API integration service
    /// </summary>
    public class OpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly string _model;
        private readonly KnowledgeBaseService _knowledgeBase;

        public OpenAIService(string apiKey, string model = "gpt-4")
        {
            _client = new OpenAIClient(apiKey);
            _model = model;
            _knowledgeBase = new KnowledgeBaseService();
        }

        public async Task<string> GetResponseAsync(string message, List<ConversationMessage> conversationHistory = null)
        {
            try
            {
                // First check our local knowledge base for accurate answers
                var knowledgeAnswer = _knowledgeBase.GetAnswer(message);
                if (!string.IsNullOrEmpty(knowledgeAnswer))
                {
                    return knowledgeAnswer;
                }

                // Build conversation context
                var messages = new List<ChatMessage>();
                
                // System prompt for accuracy and helpfulness
                messages.Add(new SystemChatMessage(
                    "You are Smartitecture AI Assistant, a helpful and accurate AI that can help with system automation, " +
                    "programming, and general questions. Be concise, accurate, and helpful. If you're not sure about " +
                    "something, say so rather than guessing."
                ));

                // Add conversation history
                if (conversationHistory != null && conversationHistory.Any())
                {
                    foreach (var msg in conversationHistory.TakeLast(10)) // Limit context to last 10 messages
                    {
                        if (msg.Role == "user")
                        {
                            messages.Add(new UserChatMessage(msg.Content));
                        }
                        else if (msg.Role == "assistant")
                        {
                            messages.Add(new AssistantChatMessage(msg.Content));
                        }
                    }
                }

                // Add current message
                messages.Add(new UserChatMessage(message));

                // Call OpenAI API
                var completion = await _client.GetChatClient(_model).CompleteChatAsync(messages);
                
                return completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                // Fallback to knowledge base or error message
                return $"I'm having trouble connecting to OpenAI right now. Error: {ex.Message}\n\n" +
                       "Please check your API key configuration or try again later.";
            }
        }

        public async Task<string> GetStreamingResponseAsync(string message, Action<string> onTokenReceived, List<ConversationMessage> conversationHistory = null)
        {
            try
            {
                // First check our local knowledge base
                var knowledgeAnswer = _knowledgeBase.GetAnswer(message);
                if (!string.IsNullOrEmpty(knowledgeAnswer))
                {
                    // Simulate streaming for knowledge base answers
                    var words = knowledgeAnswer.Split(' ');
                    var knowledgeResponse = "";
                    
                    foreach (var word in words)
                    {
                        await Task.Delay(50); // Simulate typing speed
                        var token = (knowledgeResponse.Length == 0 ? "" : " ") + word;
                        knowledgeResponse += token;
                        onTokenReceived?.Invoke(token);
                    }
                    
                    return knowledgeResponse;
                }

                // Build messages for OpenAI
                var messages = new List<ChatMessage>();
                
                messages.Add(new SystemChatMessage(
                    "You are Smartitecture AI Assistant. Be helpful, accurate, and concise."
                ));

                if (conversationHistory != null && conversationHistory.Any())
                {
                    foreach (var msg in conversationHistory.TakeLast(10))
                    {
                        if (msg.Role == "user")
                        {
                            messages.Add(new UserChatMessage(msg.Content));
                        }
                        else if (msg.Role == "assistant")
                        {
                            messages.Add(new AssistantChatMessage(msg.Content));
                        }
                    }
                }

                messages.Add(new UserChatMessage(message));

                // Stream from OpenAI
                var fullResponse = "";
                await foreach (var update in _client.GetChatClient(_model).CompleteChatStreamingAsync(messages))
                {
                    if (update.ContentUpdate.Count > 0)
                    {
                        var token = update.ContentUpdate[0].Text;
                        fullResponse += token;
                        onTokenReceived?.Invoke(token);
                    }
                }

                return fullResponse;
            }
            catch (Exception ex)
            {
                var errorMessage = $"OpenAI connection error: {ex.Message}";
                onTokenReceived?.Invoke(errorMessage);
                return errorMessage;
            }
        }

        public bool IsConfigured()
        {
            return _client != null;
        }

        public string GetModelName()
        {
            return _model;
        }
    }

    /// <summary>
    /// Configuration for OpenAI API
    /// </summary>
    public class OpenAIConfig
    {
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-4";
        public string OrganizationId { get; set; } = "";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 2000;
    }
}
