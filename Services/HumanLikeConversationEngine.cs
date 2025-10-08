using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    /// <summary>
    /// Human-Like Conversation Engine based on 2024 research
    /// Focuses on emotional intelligence, natural language patterns, and contextual awareness
    /// </summary>
    public class HumanLikeConversationEngine
    {
        private readonly KnowledgeBaseService _knowledgeBase;
        private readonly WebResearchEngine _researchEngine;
        private readonly Random _random;
        private readonly Dictionary<string, ConversationContext> _contexts;

        public HumanLikeConversationEngine()
        {
            _knowledgeBase = new KnowledgeBaseService();
            _researchEngine = new WebResearchEngine();
            _random = new Random();
            _contexts = new Dictionary<string, ConversationContext>();
        }

        public async Task<string> GetResponseAsync(string message, string conversationId = "default", List<ConversationMessage> history = null)
        {
            var context = GetOrCreateContext(conversationId);
            var analysis = AnalyzeMessage(message, context);
            
            // Update context with user message
            context.AddUserMessage(message, analysis);
            
            // Generate human-like response
            var response = await GenerateHumanResponse(message, analysis, context);
            
            // Update context with assistant response
            context.AddAssistantMessage(response);
            
            return response;
        }

        private MessageAnalysis AnalyzeMessage(string message, ConversationContext context)
        {
            var lower = message.ToLower().Trim();
            
            return new MessageAnalysis
            {
                OriginalMessage = message,
                EmotionalTone = DetectEmotionalTone(lower),
                Intent = DetermineIntent(lower, context),
                Urgency = DetectUrgency(lower),
                Politeness = DetectPoliteness(lower),
                IsFollowUp = IsFollowUp(lower, context),
                ContainsMath = ContainsMath(message),
                IsGreeting = IsGreeting(lower),
                IsGoodbye = IsGoodbye(lower),
                IsQuestion = IsQuestion(lower),
                IsCompliment = IsCompliment(lower),
                IsComplaint = IsComplaint(lower)
            };
        }

        private async Task<string> GenerateHumanResponse(string message, MessageAnalysis analysis, ConversationContext context)
        {
            // DEBUG: Log what we're processing
            System.Diagnostics.Debug.WriteLine($"[HumanEngine] Processing: '{message}' | Intent: {analysis.Intent} | IsGreeting: {analysis.IsGreeting}");

            // Handle math first (but make it conversational)
            if (analysis.ContainsMath)
            {
                var mathAnswer = _knowledgeBase.GetAnswer(message);
                if (!string.IsNullOrEmpty(mathAnswer))
                {
                    System.Diagnostics.Debug.WriteLine($"[HumanEngine] Math answer: {mathAnswer}");
                    return MakeMathConversational(mathAnswer, analysis);
                }
            }

            // Handle other knowledge base answers
            var knowledgeAnswer = _knowledgeBase.GetAnswer(message);
            System.Diagnostics.Debug.WriteLine($"[HumanEngine] Knowledge answer: {knowledgeAnswer ?? "NULL"}");
            if (!string.IsNullOrEmpty(knowledgeAnswer))
            {
                return MakeKnowledgeConversational(knowledgeAnswer, analysis, context);
            }

            // If knowledge base doesn't know, try research for questions
            if (analysis.IsQuestion && ShouldResearch(message, analysis))
            {
                System.Diagnostics.Debug.WriteLine($"[HumanEngine] Researching question: {message}");
                var researchResult = await _researchEngine.ResearchQuestionAsync(message);
                if (researchResult.Confidence > 0.5)
                {
                    return MakeResearchConversational(researchResult, analysis, context);
                }
            }

            // Generate contextual human responses
            System.Diagnostics.Debug.WriteLine($"[HumanEngine] Using intent-based response for: {analysis.Intent}");
            return analysis.Intent switch
            {
                ConversationIntent.Greeting => HandleGreeting(analysis, context),
                ConversationIntent.Goodbye => HandleGoodbye(analysis, context),
                ConversationIntent.Question => HandleQuestion(analysis, context),
                ConversationIntent.Compliment => HandleCompliment(analysis, context),
                ConversationIntent.Complaint => HandleComplaint(analysis, context),
                ConversationIntent.Command => HandleCommand(analysis, context),
                ConversationIntent.Casual => HandleCasualChat(analysis, context),
                ConversationIntent.Help => HandleHelpRequest(analysis, context),
                _ => HandleGeneral(analysis, context)
            };
        }

        private string MakeMathConversational(string mathAnswer, MessageAnalysis analysis)
        {
            var responses = analysis.EmotionalTone switch
            {
                EmotionalTone.Excited => new[]
                {
                    $"Ooh, I love math! {mathAnswer} 🧮",
                    $"Easy one! {mathAnswer} ✨",
                    $"Got it! {mathAnswer} 🎯",
                    $"Math time! {mathAnswer} 📊",
                    $"Quick calculation: {mathAnswer} ⚡"
                },
                EmotionalTone.Casual => new[]
                {
                    $"That's {mathAnswer}",
                    $"Sure thing - {mathAnswer}",
                    $"Yep, {mathAnswer}",
                    $"{mathAnswer} 👍",
                    $"There you go: {mathAnswer}"
                },
                EmotionalTone.Polite => new[]
                {
                    $"Of course! {mathAnswer}",
                    $"Happy to help - {mathAnswer}",
                    $"Certainly! {mathAnswer}",
                    $"Absolutely! {mathAnswer}",
                    $"My pleasure: {mathAnswer}"
                },
                _ => new[]
                {
                    mathAnswer,
                    $"That equals {mathAnswer.Split('=').LastOrDefault()?.Trim()}",
                    $"The answer is {mathAnswer.Split('=').LastOrDefault()?.Trim()}",
                    $"Result: {mathAnswer}",
                    $"Here's your answer: {mathAnswer}"
                }
            };

            return responses[_random.Next(responses.Length)];
        }

        private string MakeKnowledgeConversational(string knowledgeAnswer, MessageAnalysis analysis, ConversationContext context)
        {
            // For time questions
            if (knowledgeAnswer.Contains("current time") || knowledgeAnswer.Contains("PM") || knowledgeAnswer.Contains("AM"))
            {
                var timeResponses = new[]
                {
                    $"It's {knowledgeAnswer.Replace("The current time is ", "")}",
                    $"Right now it's {knowledgeAnswer.Replace("The current time is ", "")}",
                    knowledgeAnswer.Replace("The current time is ", "The time is ")
                };
                return timeResponses[_random.Next(timeResponses.Length)];
            }

            // For factual answers, make them more conversational
            if (analysis.EmotionalTone == EmotionalTone.Curious)
            {
                var curiousResponses = new[]
                {
                    $"Great question! {knowledgeAnswer}",
                    $"Interesting you ask - {knowledgeAnswer}",
                    $"I can tell you that {knowledgeAnswer.ToLower()}"
                };
                return curiousResponses[_random.Next(curiousResponses.Length)];
            }

            return knowledgeAnswer;
        }

        private bool ShouldResearch(string message, MessageAnalysis analysis)
        {
            var lower = message.ToLower();
            
            // Research for factual questions
            if (lower.StartsWith("what is") || lower.StartsWith("who is") || lower.StartsWith("where is") ||
                lower.StartsWith("when is") || lower.StartsWith("how does") || lower.StartsWith("why does"))
                return true;
                
            // Research for current events
            if (lower.Contains("president") || lower.Contains("capital") || lower.Contains("weather"))
                return true;
                
            // Don't research for greetings, casual chat, etc.
            if (analysis.Intent == ConversationIntent.Greeting || 
                analysis.Intent == ConversationIntent.Casual ||
                analysis.Intent == ConversationIntent.Compliment)
                return false;
                
            return analysis.IsQuestion;
        }

        private string MakeResearchConversational(ResearchResult research, MessageAnalysis analysis, ConversationContext context)
        {
            var baseAnswer = research.Answer;
            
            // Add conversational flair based on emotional tone
            var intro = analysis.EmotionalTone switch
            {
                EmotionalTone.Excited => new[] { "Great question! Let me research that for you... ", "Ooh, interesting! I found this: ", "I looked that up and here's what I found: " },
                EmotionalTone.Curious => new[] { "That's a fascinating question! ", "I researched that for you: ", "Here's what I discovered: " },
                EmotionalTone.Polite => new[] { "I'd be happy to research that for you. ", "Let me look that up... ", "I found some information about that: " },
                _ => new[] { "I researched that and found: ", "Here's what I found: ", "Let me share what I discovered: " }
            };

            var selectedIntro = intro[_random.Next(intro.Length)];
            
            // Add confidence indicator for research
            var confidenceNote = research.Confidence switch
            {
                > 0.8 => "",
                > 0.6 => " (I'm fairly confident about this information)",
                > 0.4 => " (This information might need verification)",
                _ => " (I found limited information about this)"
            };

            return $"{selectedIntro}{baseAnswer}{confidenceNote}";
        }

        private string HandleGreeting(MessageAnalysis analysis, ConversationContext context)
        {
            var timeOfDay = DateTime.Now.Hour switch
            {
                < 12 => "morning",
                < 17 => "afternoon",
                _ => "evening"
            };

            var greetings = analysis.EmotionalTone switch
            {
                EmotionalTone.Excited => new[]
                {
                    $"Hey there! Great {timeOfDay}! What's up?",
                    $"Hi! You seem energetic today - I love it! How can I help?",
                    $"Hello! What exciting thing can I help you with?",
                    "Hey! Ready to get things done? What do you need?",
                    "Hi there! I'm pumped to help you today!"
                },
                EmotionalTone.Polite => new[]
                {
                    $"Good {timeOfDay}! How may I assist you today?",
                    $"Hello! It's nice to meet you. What can I do for you?",
                    $"Hi there! I'm here to help with whatever you need.",
                    "Good day! How can I be of service?",
                    "Hello! I hope you're having a wonderful day. How can I help?"
                },
                _ => new[]
                {
                    $"Hey! What can I help you with?",
                    $"Hi there! How's it going?",
                    $"Hello! What's on your mind?",
                    "Hey! What brings you here today?",
                    "Hi! Ready to tackle something together?",
                    "Hello! What can we work on?",
                    "Hey there! What's the plan?"
                }
            };

            return greetings[_random.Next(greetings.Length)];
        }

        private string HandleQuestion(MessageAnalysis analysis, ConversationContext context)
        {
            var unknownResponses = analysis.EmotionalTone switch
            {
                EmotionalTone.Frustrated => new[]
                {
                    "I wish I could help with that, but I don't have that info. What else can I try for you?",
                    "Hmm, that's not something I know about. Maybe I can help with something else?",
                    "I don't have the answer to that one, sorry. Is there something else I can do?"
                },
                EmotionalTone.Curious => new[]
                {
                    "That's a really interesting question! I don't have that info, but I'd love to help with something I do know.",
                    "Great question! I don't know that one, but maybe I can help with math, time, or opening apps?",
                    "I love your curiosity! I don't have that answer, but what else can I help with?"
                },
                EmotionalTone.Polite => new[]
                {
                    "I'm sorry, but I don't have information about that. Is there something else I can assist you with?",
                    "I don't have that information, I'm afraid. Perhaps I can help you with something else?",
                    "Unfortunately, I don't know about that topic. What else can I help you with today?"
                },
                _ => new[]
                {
                    "I don't know that one. What else can I help with?",
                    "Not sure about that. Anything else I can do?",
                    "I don't have that info. Try me with something else!"
                }
            };

            return unknownResponses[_random.Next(unknownResponses.Length)];
        }

        private string HandleGoodbye(MessageAnalysis analysis, ConversationContext context)
        {
            var responses = new[]
            {
                "See you later! Feel free to come back anytime.",
                "Goodbye! It was great chatting with you.",
                "Take care! I'll be here when you need me.",
                "Bye! Have a wonderful day!",
                "See you soon! Don't hesitate to ask if you need anything."
            };

            return responses[_random.Next(responses.Length)];
        }

        private string HandleCompliment(MessageAnalysis analysis, ConversationContext context)
        {
            var responses = new[]
            {
                "Aw, thank you! That's really nice to hear. How can I help you today?",
                "Thanks! I appreciate that. What can I do for you?",
                "That's so kind! I'm here whenever you need help.",
                "Thank you! That made my day. What would you like to work on?"
            };

            return responses[_random.Next(responses.Length)];
        }

        private string HandleComplaint(MessageAnalysis analysis, ConversationContext context)
        {
            var responses = new[]
            {
                "I'm sorry you're having trouble. Let me see how I can help make this better.",
                "That sounds frustrating. What can I do to help fix this?",
                "I understand that's annoying. How can I assist you with this?",
                "Sorry about that! Let me try to help you out."
            };

            return responses[_random.Next(responses.Length)];
        }

        private string HandleCommand(MessageAnalysis analysis, ConversationContext context)
        {
            var responses = new[]
            {
                "Sure thing! I can open apps like calculator, file explorer, or task manager. What would you like?",
                "Absolutely! What app would you like me to launch?",
                "I'm on it! Which application should I open for you?",
                "You got it! What do you need me to open?"
            };

            return responses[_random.Next(responses.Length)];
        }

        private string HandleCasualChat(MessageAnalysis analysis, ConversationContext context)
        {
            var responses = new[]
            {
                "I'm doing great, thanks for asking! How are you doing?",
                "All good here! What's going on with you?",
                "I'm well! Thanks for checking in. How's your day going?",
                "Doing fantastic! What can I help you with today?"
            };

            return responses[_random.Next(responses.Length)];
        }

        private string HandleHelpRequest(MessageAnalysis analysis, ConversationContext context)
        {
            var responses = new[]
            {
                "I'm here to help! I can do math, tell you the time, open apps, or answer questions. What do you need?",
                "Happy to help! I'm good with calculations, system tasks, and general questions. What's up?",
                "Of course! I can help with math, time, opening applications, or other questions. What would you like?",
                "Absolutely! I specialize in calculations, system commands, and answering questions. How can I assist?"
            };

            return responses[_random.Next(responses.Length)];
        }

        private string HandleGeneral(MessageAnalysis analysis, ConversationContext context)
        {
            var responses = analysis.EmotionalTone switch
            {
                EmotionalTone.Confused => new[]
                {
                    "I'm not quite sure what you're looking for. Can you tell me more?",
                    "Could you clarify what you need help with?",
                    "I want to help, but I'm not sure what you're asking. Can you explain a bit more?"
                },
                _ => new[]
                {
                    "I'm here to help! What do you need?",
                    "How can I assist you today?",
                    "What can I do for you?"
                }
            };

            return responses[_random.Next(responses.Length)];
        }

        // Analysis helper methods
        private EmotionalTone DetectEmotionalTone(string message)
        {
            if (message.Contains("!") || message.Contains("awesome") || message.Contains("amazing") || message.Contains("love"))
                return EmotionalTone.Excited;
            if (message.Contains("please") || message.Contains("thank") || message.Contains("sorry"))
                return EmotionalTone.Polite;
            if (message.Contains("frustrated") || message.Contains("annoying") || message.Contains("stupid"))
                return EmotionalTone.Frustrated;
            if (message.Contains("?") && (message.Contains("what") || message.Contains("how") || message.Contains("why")))
                return EmotionalTone.Curious;
            if (message.Contains("confused") || message.Contains("don't understand"))
                return EmotionalTone.Confused;
            
            return EmotionalTone.Casual;
        }

        private ConversationIntent DetermineIntent(string message, ConversationContext context)
        {
            if (IsGreeting(message)) return ConversationIntent.Greeting;
            if (IsGoodbye(message)) return ConversationIntent.Goodbye;
            if (IsCompliment(message)) return ConversationIntent.Compliment;
            if (IsComplaint(message)) return ConversationIntent.Complaint;
            if (message.Contains("open") || message.Contains("launch") || message.Contains("start"))
                return ConversationIntent.Command;
            if (message.Contains("help") || message.Contains("what can you do"))
                return ConversationIntent.Help;
            if (IsQuestion(message)) return ConversationIntent.Question;
            if (message.Contains("how are you") || message.Contains("what's up"))
                return ConversationIntent.Casual;
            
            return ConversationIntent.General;
        }

        private bool IsGreeting(string message) =>
            new[] { "hello", "hi", "hey", "good morning", "good afternoon", "good evening" }
            .Any(g => message.Contains(g));

        private bool IsGoodbye(string message) =>
            new[] { "bye", "goodbye", "see you", "talk later", "farewell" }
            .Any(g => message.Contains(g));

        private bool IsQuestion(string message) =>
            message.StartsWith("what") || message.StartsWith("how") || message.StartsWith("why") ||
            message.StartsWith("when") || message.StartsWith("where") || message.EndsWith("?");

        private bool IsCompliment(string message) =>
            new[] { "good job", "well done", "excellent", "perfect", "great work", "amazing", "awesome" }
            .Any(c => message.Contains(c));

        private bool IsComplaint(string message) =>
            new[] { "terrible", "awful", "bad", "wrong", "broken", "doesn't work", "frustrated" }
            .Any(c => message.Contains(c));

        private bool ContainsMath(string message) =>
            Regex.IsMatch(message, @"\d+(?:\.\d+)?\s*[+\-*/]\s*\d+(?:\.\d+)?") ||
            message.ToLower().Contains("calculate");

        private bool DetectUrgency(string message) =>
            new[] { "urgent", "quickly", "asap", "now", "immediately" }.Any(u => message.Contains(u));

        private bool DetectPoliteness(string message) =>
            new[] { "please", "thank you", "thanks", "sorry", "excuse me" }.Any(p => message.Contains(p));

        private bool IsFollowUp(string message, ConversationContext context) =>
            new[] { "also", "and", "what about", "how about", "can you also" }.Any(f => message.Contains(f)) &&
            context.MessageCount > 1;

        private ConversationContext GetOrCreateContext(string conversationId)
        {
            if (!_contexts.ContainsKey(conversationId))
            {
                _contexts[conversationId] = new ConversationContext(conversationId);
            }
            return _contexts[conversationId];
        }
    }

    // Supporting classes
    public class ConversationContext
    {
        public string ConversationId { get; }
        public List<(string role, string message, DateTime timestamp, MessageAnalysis analysis)> Messages { get; }
        public int MessageCount => Messages.Count;
        public DateTime LastInteraction => Messages.LastOrDefault().timestamp;

        public ConversationContext(string conversationId)
        {
            ConversationId = conversationId;
            Messages = new List<(string, string, DateTime, MessageAnalysis)>();
        }

        public void AddUserMessage(string message, MessageAnalysis analysis)
        {
            Messages.Add(("user", message, DateTime.Now, analysis));
        }

        public void AddAssistantMessage(string message)
        {
            Messages.Add(("assistant", message, DateTime.Now, null));
        }
    }

    public class MessageAnalysis
    {
        public string OriginalMessage { get; set; } = "";
        public EmotionalTone EmotionalTone { get; set; }
        public ConversationIntent Intent { get; set; }
        public bool Urgency { get; set; }
        public bool Politeness { get; set; }
        public bool IsFollowUp { get; set; }
        public bool ContainsMath { get; set; }
        public bool IsGreeting { get; set; }
        public bool IsGoodbye { get; set; }
        public bool IsQuestion { get; set; }
        public bool IsCompliment { get; set; }
        public bool IsComplaint { get; set; }
    }

    public enum EmotionalTone
    {
        Casual, Excited, Polite, Frustrated, Curious, Confused
    }

    public enum ConversationIntent
    {
        Greeting, Goodbye, Question, Compliment, Complaint, Command, Casual, Help, General
    }
}
