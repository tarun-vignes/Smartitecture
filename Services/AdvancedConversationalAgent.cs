using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    /// <summary>
    /// Advanced Conversational AI Agent with personality, reasoning, and context awareness
    /// </summary>
    public class AdvancedConversationalAgent
    {
        private readonly KnowledgeBaseService _knowledgeBase;
        private readonly Dictionary<string, ConversationContext> _conversations;
        private readonly Random _random;

        public AdvancedConversationalAgent()
        {
            _knowledgeBase = new KnowledgeBaseService();
            _conversations = new Dictionary<string, ConversationContext>();
            _random = new Random();
        }

        public async Task<string> GenerateAdvancedResponseAsync(string message, string conversationId, List<ConversationMessage> history)
        {
            var context = GetOrCreateContext(conversationId);
            context.AddMessage("user", message);

            // Analyze the message for intent and complexity
            var analysis = AnalyzeMessage(message, context);

            // Generate contextual, conversational response
            var response = await GenerateContextualResponse(message, analysis, context, history);
            
            context.AddMessage("assistant", response);
            return response;
        }

        private MessageAnalysis AnalyzeMessage(string message, ConversationContext context)
        {
            var analysis = new MessageAnalysis
            {
                OriginalMessage = message,
                Intent = DetermineIntent(message),
                Complexity = AssessComplexity(message),
                RequiresReasoning = RequiresReasoning(message),
                IsFollowUp = IsFollowUpQuestion(message, context),
                EmotionalTone = DetectEmotionalTone(message),
                TopicCategory = CategorizeMessage(message)
            };

            return analysis;
        }

        private async Task<string> GenerateContextualResponse(string message, MessageAnalysis analysis, ConversationContext context, List<ConversationMessage> history)
        {
            // First check for exact knowledge (math, facts)
            var knowledgeAnswer = _knowledgeBase.GetAnswer(message);
            if (!string.IsNullOrEmpty(knowledgeAnswer))
            {
                return EnhanceKnowledgeResponse(knowledgeAnswer, analysis, context);
            }

            // Generate conversational response based on intent and context
            return analysis.Intent switch
            {
                MessageIntent.Mathematical => await HandleMathematicalQuery(message, analysis, context),
                MessageIntent.Question => await HandleQuestionQuery(message, analysis, context),
                MessageIntent.Command => await HandleCommandQuery(message, analysis, context),
                MessageIntent.Conversational => await HandleConversationalQuery(message, analysis, context),
                MessageIntent.Creative => await HandleCreativeQuery(message, analysis, context),
                MessageIntent.Problem => await HandleProblemSolving(message, analysis, context),
                _ => await HandleGeneralQuery(message, analysis, context)
            };
        }

        private string EnhanceKnowledgeResponse(string knowledgeAnswer, MessageAnalysis analysis, ConversationContext context)
        {
            // Make knowledge responses more conversational
            var enhancers = new[]
            {
                $"Great question! {knowledgeAnswer}",
                $"I can help with that. {knowledgeAnswer}",
                $"Here's what I know: {knowledgeAnswer}",
                $"Let me calculate that for you. {knowledgeAnswer}",
                $"That's an interesting question. {knowledgeAnswer}"
            };

            var baseResponse = enhancers[_random.Next(enhancers.Length)];

            // Add contextual follow-up based on the type of question
            if (analysis.Intent == MessageIntent.Mathematical)
            {
                var followUps = new[]
                {
                    " Would you like me to show you how I calculated that?",
                    " Need help with any other calculations?",
                    " Is there anything else you'd like to compute?",
                    " Would you like me to break down the steps?"
                };
                baseResponse += followUps[_random.Next(followUps.Length)];
            }

            return baseResponse;
        }

        private async Task<string> HandleMathematicalQuery(string message, MessageAnalysis analysis, ConversationContext context)
        {
            // Enhanced mathematical reasoning
            var mathMatch = Regex.Match(message, @"(\d+(?:\.\d+)?)\s*([+\-*/])\s*(\d+(?:\.\d+)?)");
            if (mathMatch.Success)
            {
                var num1 = double.Parse(mathMatch.Groups[1].Value);
                var operation = mathMatch.Groups[2].Value;
                var num2 = double.Parse(mathMatch.Groups[3].Value);

                var result = operation switch
                {
                    "+" => num1 + num2,
                    "-" => num1 - num2,
                    "*" => num1 * num2,
                    "/" => num2 != 0 ? num1 / num2 : double.NaN,
                    _ => double.NaN
                };

                if (!double.IsNaN(result))
                {
                    var responses = new[]
                    {
                        $"Let me solve that for you: {num1} {operation} {num2} = **{result}**. That's a straightforward calculation!",
                        $"I can calculate that easily! {num1} {operation} {num2} equals **{result}**. Anything else you'd like to compute?",
                        $"Here's your answer: **{result}**. I computed {num1} {operation} {num2} for you. Need help with more math?",
                        $"Quick calculation: {num1} {operation} {num2} = **{result}**. Mathematics is one of my strong suits!"
                    };
                    return responses[_random.Next(responses.Length)];
                }
            }

            return "I'd be happy to help with mathematical calculations! Could you provide the numbers and operation you'd like me to compute?";
        }

        private async Task<string> HandleQuestionQuery(string message, MessageAnalysis analysis, ConversationContext context)
        {
            var questionStarters = new[]
            {
                "That's a thoughtful question!",
                "I'm glad you asked about that.",
                "Interesting question!",
                "Let me think about that for you.",
                "That's something I can help explain."
            };

            var starter = questionStarters[_random.Next(questionStarters.Length)];
            
            // Try to provide a helpful response even without specific knowledge
            if (analysis.TopicCategory == TopicCategory.Science)
            {
                return $"{starter} While I don't have specific information about that scientific topic in my verified knowledge base, I'd recommend checking reliable scientific sources. Is there something else I can help you with?";
            }
            else if (analysis.TopicCategory == TopicCategory.Technology)
            {
                return $"{starter} That's a technology-related question. I focus on providing accurate information, so I'd suggest consulting current tech documentation. Can I help you with something else?";
            }

            return $"{starter} I don't have verified information about that specific topic. I prefer to be honest rather than guess. Is there something else I can assist you with?";
        }

        private async Task<string> HandleConversationalQuery(string message, MessageAnalysis analysis, ConversationContext context)
        {
            var greetings = new[] { "hello", "hi", "hey", "good morning", "good afternoon", "good evening" };
            var lowerMessage = message.ToLower();

            if (greetings.Any(g => lowerMessage.Contains(g)))
            {
                var responses = new[]
                {
                    "Hello! I'm your AI assistant, ready to help with calculations, questions, and system tasks. What can I do for you today?",
                    "Hi there! I'm here to assist you with math, information, and computer tasks. How can I help?",
                    "Hey! Great to chat with you. I can help with calculations, answer questions, and manage system tasks. What would you like to do?",
                    "Good to see you! I'm your intelligent assistant for math, information, and system automation. What's on your mind?"
                };
                return responses[_random.Next(responses.Length)];
            }

            if (lowerMessage.Contains("how are you") || lowerMessage.Contains("how's it going"))
            {
                return "I'm doing great, thank you for asking! I'm here and ready to help you with whatever you need. How are you doing today?";
            }

            if (lowerMessage.Contains("thank you") || lowerMessage.Contains("thanks"))
            {
                return "You're very welcome! I'm always happy to help. Is there anything else you'd like assistance with?";
            }

            return "I'm here to help! Feel free to ask me questions, request calculations, or let me know if you need help with system tasks.";
        }

        private async Task<string> HandleCreativeQuery(string message, MessageAnalysis analysis, ConversationContext context)
        {
            return "I appreciate your creative request! While I focus on providing accurate, factual information and system assistance, I'd be happy to help you with calculations, questions, or computer tasks instead. What would you like to work on?";
        }

        private async Task<string> HandleCommandQuery(string message, MessageAnalysis analysis, ConversationContext context)
        {
            return "I can help you execute system commands! I can open applications like calculator, file explorer, task manager, and more. What would you like me to launch or manage?";
        }

        private async Task<string> HandleProblemSolving(string message, MessageAnalysis analysis, ConversationContext context)
        {
            return "I'd be happy to help you solve problems! I'm particularly good with mathematical calculations, system tasks, and providing factual information. What specific problem can I assist you with?";
        }

        private async Task<string> HandleGeneralQuery(string message, MessageAnalysis analysis, ConversationContext context)
        {
            return "I'm here to help! I specialize in accurate calculations, factual information, and system tasks. Feel free to ask me questions or let me know what you'd like assistance with.";
        }

        // Helper methods for analysis
        private MessageIntent DetermineIntent(string message)
        {
            var lower = message.ToLower();
            
            if (Regex.IsMatch(message, @"\d+\s*[+\-*/]\s*\d+") || lower.Contains("calculate") || lower.Contains("math"))
                return MessageIntent.Mathematical;
            
            if (lower.StartsWith("what") || lower.StartsWith("how") || lower.StartsWith("why") || lower.StartsWith("when") || lower.StartsWith("where"))
                return MessageIntent.Question;
            
            if (lower.Contains("open") || lower.Contains("launch") || lower.Contains("start") || lower.Contains("run"))
                return MessageIntent.Command;
            
            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("thanks") || lower.Contains("how are you"))
                return MessageIntent.Conversational;
            
            if (lower.Contains("write") || lower.Contains("create") || lower.Contains("generate") || lower.Contains("make"))
                return MessageIntent.Creative;
            
            if (lower.Contains("help") || lower.Contains("problem") || lower.Contains("issue") || lower.Contains("solve"))
                return MessageIntent.Problem;
            
            return MessageIntent.General;
        }

        private ComplexityLevel AssessComplexity(string message)
        {
            if (message.Length < 20) return ComplexityLevel.Simple;
            if (message.Length < 100) return ComplexityLevel.Medium;
            return ComplexityLevel.Complex;
        }

        private bool RequiresReasoning(string message)
        {
            var reasoningKeywords = new[] { "why", "how", "explain", "because", "reason", "analyze" };
            return reasoningKeywords.Any(k => message.ToLower().Contains(k));
        }

        private bool IsFollowUpQuestion(string message, ConversationContext context)
        {
            var followUpWords = new[] { "also", "and", "what about", "how about", "can you also" };
            return followUpWords.Any(f => message.ToLower().Contains(f)) && context.MessageCount > 1;
        }

        private EmotionalTone DetectEmotionalTone(string message)
        {
            var lower = message.ToLower();
            if (lower.Contains("!") || lower.Contains("awesome") || lower.Contains("great")) return EmotionalTone.Excited;
            if (lower.Contains("please") || lower.Contains("thank")) return EmotionalTone.Polite;
            if (lower.Contains("urgent") || lower.Contains("quickly")) return EmotionalTone.Urgent;
            return EmotionalTone.Neutral;
        }

        private TopicCategory CategorizeMessage(string message)
        {
            var lower = message.ToLower();
            if (lower.Contains("science") || lower.Contains("physics") || lower.Contains("chemistry")) return TopicCategory.Science;
            if (lower.Contains("computer") || lower.Contains("software") || lower.Contains("technology")) return TopicCategory.Technology;
            if (Regex.IsMatch(message, @"\d+\s*[+\-*/]\s*\d+")) return TopicCategory.Mathematics;
            return TopicCategory.General;
        }

        private ConversationContext GetOrCreateContext(string conversationId)
        {
            if (!_conversations.ContainsKey(conversationId))
            {
                _conversations[conversationId] = new ConversationContext(conversationId);
            }
            return _conversations[conversationId];
        }
    }

    // Supporting classes
    public class ConversationContext
    {
        public string ConversationId { get; }
        public List<(string role, string message, DateTime timestamp)> Messages { get; }
        public Dictionary<string, object> Context { get; }
        public int MessageCount => Messages.Count;

        public ConversationContext(string conversationId)
        {
            ConversationId = conversationId;
            Messages = new List<(string, string, DateTime)>();
            Context = new Dictionary<string, object>();
        }

        public void AddMessage(string role, string message)
        {
            Messages.Add((role, message, DateTime.Now));
        }
    }

    public class MessageAnalysis
    {
        public string OriginalMessage { get; set; } = "";
        public MessageIntent Intent { get; set; }
        public ComplexityLevel Complexity { get; set; }
        public bool RequiresReasoning { get; set; }
        public bool IsFollowUp { get; set; }
        public EmotionalTone EmotionalTone { get; set; }
        public TopicCategory TopicCategory { get; set; }
    }

    public enum MessageIntent
    {
        Mathematical, Question, Command, Conversational, Creative, Problem, General
    }

    public enum ComplexityLevel
    {
        Simple, Medium, Complex
    }

    public enum EmotionalTone
    {
        Neutral, Excited, Polite, Urgent
    }

    public enum TopicCategory
    {
        Mathematics, Science, Technology, General
    }
}
