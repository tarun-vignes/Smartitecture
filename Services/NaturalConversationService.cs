using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    /// <summary>
    /// Natural, concise conversation service - like modern ChatGPT
    /// Direct, helpful, not verbose
    /// </summary>
    public class NaturalConversationService
    {
        private readonly KnowledgeBaseService _knowledgeBase;
        private readonly Random _random;

        public NaturalConversationService()
        {
            _knowledgeBase = new KnowledgeBaseService();
            _random = new Random();
        }

        public async Task<string> GetResponseAsync(string message, List<ConversationMessage> history = null)
        {
            // Always check knowledge base first for accurate answers
            var knowledgeAnswer = _knowledgeBase.GetAnswer(message);
            if (!string.IsNullOrEmpty(knowledgeAnswer))
            {
                return MakeNatural(knowledgeAnswer, message);
            }

            // Handle different types of messages naturally
            return await GenerateNaturalResponse(message, history);
        }

        private string MakeNatural(string knowledgeAnswer, string originalMessage)
        {
            // Make knowledge base answers more natural and concise
            var lower = originalMessage.ToLower();

            // Math questions - keep it simple
            if (Regex.IsMatch(originalMessage, @"\d+\s*[+\-*/]\s*\d+"))
            {
                return knowledgeAnswer; // Already good: "15 + 27 = 42"
            }

            // Time questions
            if (lower.Contains("time") || lower.Contains("what time"))
            {
                return knowledgeAnswer; // Already formatted well
            }

            // For other knowledge, just return it directly
            return knowledgeAnswer;
        }

        private async Task<string> GenerateNaturalResponse(string message, List<ConversationMessage> history)
        {
            var lower = message.ToLower();

            // Greetings - be friendly but brief
            if (IsGreeting(lower))
            {
                var greetings = new[] { "Hi! How can I help?", "Hello! What can I do for you?", "Hey there! What's up?" };
                return greetings[_random.Next(greetings.Length)];
            }

            // Thanks - acknowledge briefly
            if (lower.Contains("thank") || lower.Contains("thanks"))
            {
                var thanks = new[] { "You're welcome!", "No problem!", "Happy to help!" };
                return thanks[_random.Next(thanks.Length)];
            }

            // How are you - be natural
            if (lower.Contains("how are you") || lower.Contains("how's it going"))
            {
                return "I'm doing well, thanks! What can I help you with?";
            }

            // Commands - be helpful
            if (lower.Contains("open") || lower.Contains("launch") || lower.Contains("start"))
            {
                return "I can open apps like calculator, file explorer, or task manager. What would you like to launch?";
            }

            // Questions we don't know
            if (IsQuestion(lower))
            {
                var unknowns = new[]
                {
                    "I don't have information on that. Anything else I can help with?",
                    "Not sure about that one. What else can I do for you?",
                    "I don't know that, but I can help with math, time, or opening apps."
                };
                return unknowns[_random.Next(unknowns.Length)];
            }

            // Default - be helpful but brief
            return "I can help with calculations, questions, or system tasks. What do you need?";
        }

        private bool IsGreeting(string message)
        {
            var greetings = new[] { "hello", "hi", "hey", "good morning", "good afternoon", "good evening" };
            return greetings.Any(g => message.Contains(g));
        }

        private bool IsQuestion(string message)
        {
            return message.StartsWith("what") || message.StartsWith("how") || 
                   message.StartsWith("why") || message.StartsWith("when") || 
                   message.StartsWith("where") || message.StartsWith("who") ||
                   message.EndsWith("?");
        }
    }
}
