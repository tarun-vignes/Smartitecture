using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartitecture.Services.Models;

namespace Smartitecture.Services
{
    /// <summary>
    /// Manages conversation history and context for AI interactions
    /// </summary>
    public class ConversationManager
    {
        private readonly ConversationSettings _settings;
        private readonly Dictionary<string, List<ConversationMessage>> _conversations;

        public ConversationManager(ConversationSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _conversations = new Dictionary<string, List<ConversationMessage>>();
        }

        /// <summary>
        /// Add a message to a conversation
        /// </summary>
        public async Task AddMessageAsync(string conversationId, ConversationMessage message)
        {
            if (!_conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId] = new List<ConversationMessage>();
            }

            var conversation = _conversations[conversationId];
            conversation.Add(message);

            // Trim conversation if it exceeds max length
            if (conversation.Count > _settings.MaxHistoryLength)
            {
                var messagesToRemove = conversation.Count - _settings.MaxHistoryLength;
                conversation.RemoveRange(0, messagesToRemove);
            }

            // Auto-summarize if enabled and threshold is reached
            if (_settings.EnableAutoSummarization && 
                conversation.Count >= _settings.SummarizationThreshold)
            {
                await SummarizeConversationAsync(conversationId);
            }
        }

        /// <summary>
        /// Get conversation context for AI model
        /// </summary>
        public async Task<List<ConversationMessage>> GetContextAsync(string conversationId)
        {
            if (!_conversations.ContainsKey(conversationId))
            {
                // Return system prompt as first message
                return new List<ConversationMessage>
                {
                    new ConversationMessage
                    {
                        Role = "system",
                        Content = _settings.SystemPrompt
                    }
                };
            }

            var messages = new List<ConversationMessage>
            {
                new ConversationMessage
                {
                    Role = "system",
                    Content = _settings.SystemPrompt
                }
            };

            // Add conversation history
            var conversation = _conversations[conversationId];
            var contextMessages = GetRecentMessages(conversation, _settings.MaxContextTokens);
            messages.AddRange(contextMessages);

            return messages;
        }

        /// <summary>
        /// Get full conversation history
        /// </summary>
        public async Task<List<ConversationMessage>> GetHistoryAsync(string conversationId)
        {
            if (!_conversations.ContainsKey(conversationId))
            {
                return new List<ConversationMessage>();
            }

            return new List<ConversationMessage>(_conversations[conversationId]);
        }

        /// <summary>
        /// Clear a conversation
        /// </summary>
        public async Task ClearConversationAsync(string conversationId)
        {
            if (_conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId].Clear();
            }
        }

        /// <summary>
        /// Get all conversation IDs
        /// </summary>
        public IEnumerable<string> GetConversationIds()
        {
            return _conversations.Keys;
        }

        /// <summary>
        /// Get recent messages that fit within token limit
        /// </summary>
        private List<ConversationMessage> GetRecentMessages(List<ConversationMessage> messages, int maxTokens)
        {
            // Simple token estimation: ~4 characters per token
            var result = new List<ConversationMessage>();
            var currentTokens = 0;

            // Add messages from most recent backwards
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                var message = messages[i];
                var estimatedTokens = message.Content.Length / 4;

                if (currentTokens + estimatedTokens > maxTokens)
                {
                    break;
                }

                result.Insert(0, message);
                currentTokens += estimatedTokens;
            }

            return result;
        }

        /// <summary>
        /// Summarize old conversation messages to save context space
        /// </summary>
        private async Task SummarizeConversationAsync(string conversationId)
        {
            // This would use the AI model to summarize old messages
            // For now, we'll just remove older messages
            var conversation = _conversations[conversationId];
            if (conversation.Count > _settings.SummarizationThreshold)
            {
                var messagesToSummarize = conversation.Take(_settings.SummarizationThreshold / 2).ToList();
                var summary = $"[Previous conversation summary: {messagesToSummarize.Count} messages exchanged about various topics]";
                
                // Remove old messages and add summary
                conversation.RemoveRange(0, messagesToSummarize.Count);
                conversation.Insert(0, new ConversationMessage
                {
                    Role = "system",
                    Content = summary,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
