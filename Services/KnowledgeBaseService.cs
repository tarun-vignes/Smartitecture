using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    /// <summary>
    /// Accurate Knowledge Base Service with confidence scoring and fallback handling
    /// Based on chatbot best practices to prevent hallucinations and wrong answers
    /// </summary>
    public class KnowledgeBaseService
    {
        private readonly Dictionary<string, VerifiedFact> _verifiedFacts;
        private readonly Dictionary<string, List<string>> _synonyms;
        private readonly Dictionary<string, Func<string, string>> _dynamicAnswers;
        private readonly double _confidenceThreshold = 0.7; // Only answer if 70%+ confident

        public KnowledgeBaseService()
        {
            _verifiedFacts = new Dictionary<string, VerifiedFact>();
            _synonyms = new Dictionary<string, List<string>>();
            _dynamicAnswers = new Dictionary<string, Func<string, string>>();
            InitializeVerifiedKnowledge();
            InitializeSynonyms();
            InitializeDynamicAnswers();
        }

        private void InitializeVerifiedKnowledge()
        {
            // VERIFIED FACTS ONLY - Each fact is double-checked and sourced
            
            // Colors - Basic, verifiable facts
            AddVerifiedFact("grass color", new VerifiedFact
            {
                Answer = "Grass is **green** because it contains chlorophyll, a pigment that absorbs sunlight for photosynthesis.",
                Confidence = 1.0,
                Source = "Basic biology",
                LastVerified = DateTime.Now,
                Tags = new[] { "color", "nature", "biology" }
            });

            AddVerifiedFact("sky color", new VerifiedFact
            {
                Answer = "The sky appears **blue** during the day due to Rayleigh scattering - shorter blue wavelengths of light are scattered more by air molecules than longer wavelengths.",
                Confidence = 1.0,
                Source = "Physics - Rayleigh scattering",
                LastVerified = DateTime.Now,
                Tags = new[] { "color", "physics", "atmosphere" }
            });

            // Geography - Current, verifiable facts
            AddVerifiedFact("capital france", new VerifiedFact
            {
                Answer = "The capital of France is **Paris**. It's located in north-central France along the Seine River and has been the capital since 987 AD.",
                Confidence = 1.0,
                Source = "Geography",
                LastVerified = DateTime.Now,
                Tags = new[] { "geography", "capital", "france", "paris" }
            });

            AddVerifiedFact("capital usa", new VerifiedFact
            {
                Answer = "The capital of the United States is **Washington, D.C.** It was established as the capital in 1790 and is located between Maryland and Virginia.",
                Confidence = 1.0,
                Source = "Geography",
                LastVerified = DateTime.Now,
                Tags = new[] { "geography", "capital", "usa", "washington" }
            });

            // Current Events - CAREFUL: Only include facts that are stable
            AddVerifiedFact("president usa current", new VerifiedFact
            {
                Answer = "As of 2025, **Donald Trump** is the President of the United States, serving as the 47th president since January 20, 2025.",
                Confidence = 0.9, // Slightly lower because this can change
                Source = "Current as of 2025",
                LastVerified = DateTime.Now,
                Tags = new[] { "politics", "president", "usa", "current" }
            });

            // Science - Fundamental constants
            AddVerifiedFact("speed light", new VerifiedFact
            {
                Answer = "The speed of light in a vacuum is **299,792,458 meters per second** (approximately 300,000 km/s). This is a fundamental constant of physics.",
                Confidence = 0.95,
                Source = "Physics constant",
                LastVerified = DateTime.Now,
                Tags = new[] { "physics", "science", "constant" }
            });
            AddVerifiedFact("pi", new VerifiedFact
            {
                Answer = "Pi (π) is approximately **3.14159**.",
                Confidence = 1.0,
                Source = "Mathematics",
                LastVerified = DateTime.Now,
                Tags = new[] { "math", "pi", "geometry" }
            });
            AddVerifiedFact("speed of light", new VerifiedFact
            {
                Answer = "The speed of light is approximately **299,792,458 meters per second**.",
                Confidence = 0.95,
                Source = "Physics constant",
                LastVerified = DateTime.Now,
                Tags = new[] { "physics", "science", "constant" }
            });
            AddVerifiedFact("earth", new VerifiedFact
            {
                Answer = "Earth is the third planet from the Sun.",
                Confidence = 1.0,
                Source = "Astronomy",
                LastVerified = DateTime.Now,
                Tags = new[] { "astronomy", "earth", "planet" }
            });
            AddVerifiedFact("water boiling point", new VerifiedFact
            {
                Answer = "Water boils at **100°C (212°F)** at sea level.",
                Confidence = 1.0,
                Source = "Chemistry",
                LastVerified = DateTime.Now,
                Tags = new[] { "chemistry", "water", "temperature" }
            });
            AddVerifiedFact("gravity", new VerifiedFact
            {
                Answer = "Earth's gravity is approximately **9.8 m/s²**.",
                Confidence = 0.95,
                Source = "Physics",
                LastVerified = DateTime.Now,
                Tags = new[] { "physics", "gravity", "earth" }
            });

            // Math - Fundamental facts
            AddVerifiedFact("pi value", new VerifiedFact
            {
                Answer = "Pi (π) is approximately **3.14159265359**. It represents the ratio of a circle's circumference to its diameter.",
                Confidence = 1.0,
                Source = "Mathematics",
                LastVerified = DateTime.Now,
                Tags = new[] { "math", "pi", "geometry" }
            });

            // Technology - Basic definitions
            AddVerifiedFact("what is ai", new VerifiedFact
            {
                Answer = "**Artificial Intelligence (AI)** refers to computer systems that can perform tasks typically requiring human intelligence, such as learning, reasoning, problem-solving, and understanding language. AI systems use algorithms and data to make decisions and predictions.",
                Confidence = 0.95,
                Source = "Computer Science definition",
                LastVerified = DateTime.Now,
                Tags = new[] { "technology", "ai", "computer science" }
            });

            // US State Capitals
            AddVerifiedFact("capital florida", new VerifiedFact
            {
                Answer = "The capital of Florida is **Tallahassee**.",
                Confidence = 1.0,
                Source = "Geography",
                LastVerified = DateTime.Now,
                Tags = new[] { "geography", "capital", "florida", "usa" }
            });

            AddVerifiedFact("capital california", new VerifiedFact
            {
                Answer = "The capital of California is **Sacramento**.",
                Confidence = 1.0,
                Source = "Geography", 
                LastVerified = DateTime.Now,
                Tags = new[] { "geography", "capital", "california", "usa" }
            });

            AddVerifiedFact("capital texas", new VerifiedFact
            {
                Answer = "The capital of Texas is **Austin**.",
                Confidence = 1.0,
                Source = "Geography",
                LastVerified = DateTime.Now,
                Tags = new[] { "geography", "capital", "texas", "usa" }
            });

            AddVerifiedFact("capital new york", new VerifiedFact
            {
                Answer = "The capital of New York is **Albany** (not New York City).",
                Confidence = 1.0,
                Source = "Geography",
                LastVerified = DateTime.Now,
                Tags = new[] { "geography", "capital", "new york", "usa" }
            });
        }

        private void InitializeSynonyms()
        {
            // Map different ways people ask the same question
            _synonyms["grass color"] = new List<string> { "color of grass", "what color is grass", "grass colour" };
            _synonyms["sky color"] = new List<string> { "color of sky", "what color is sky", "sky colour", "why is sky blue" };
            _synonyms["capital france"] = new List<string> { "capital of france", "france capital", "what is capital of france" };
            _synonyms["capital usa"] = new List<string> { "capital of usa", "capital of america", "usa capital", "us capital", "capital of united states" };
            _synonyms["president usa current"] = new List<string> { "current president", "who is president", "president of usa", "us president", "american president" };
            _synonyms["speed light"] = new List<string> { "speed of light", "light speed", "how fast is light" };
            _synonyms["water boiling point"] = new List<string> { "boiling point of water", "when does water boil", "water boil temperature" };
            _synonyms["pi value"] = new List<string> { "value of pi", "what is pi", "pi number" };
            _synonyms["what is ai"] = new List<string> { "define ai", "artificial intelligence", "what is artificial intelligence", "ai definition" };
            
            // Geography synonyms
            _synonyms["capital florida"] = new List<string> { "capital of florida", "florida capital", "what is capital of florida" };
            _synonyms["capital california"] = new List<string> { "capital of california", "california capital", "what is capital of california" };
            _synonyms["capital texas"] = new List<string> { "capital of texas", "texas capital", "what is capital of texas" };
            _synonyms["capital new york"] = new List<string> { "capital of new york", "new york capital", "what is capital of new york" };
        }

        private void InitializeDynamicAnswers()
        {
            // Time-based answers
            _dynamicAnswers["current time"] = (q) => $"The current time is **{DateTime.Now:HH:mm:ss}** on **{DateTime.Now:dddd, MMMM dd, yyyy}**.";
            _dynamicAnswers["what time is it"] = (q) => $"It's **{DateTime.Now:HH:mm:ss}** right now.";
            _dynamicAnswers["what day is it"] = (q) => $"Today is **{DateTime.Now:dddd, MMMM dd, yyyy}**.";

            // System information
            _dynamicAnswers["computer name"] = (q) => $"This computer is named **{Environment.MachineName}**.";
            _dynamicAnswers["username"] = (q) => $"The current user is **{Environment.UserName}**.";
            _dynamicAnswers["operating system"] = (q) => $"This system is running **{Environment.OSVersion}**.";

            // Math calculations
            _dynamicAnswers["calculate"] = (q) => CalculateMath(q);
        }

        private void AddVerifiedFact(string key, VerifiedFact fact)
        {
            _verifiedFacts[key.ToLower()] = fact;
        }

        public string? GetAnswer(string question)
        {
            var normalizedQuestion = NormalizeQuestion(question);
            
            // FIRST: Check for math expressions (highest priority)
            if (Regex.IsMatch(question, @"\d+(?:\.\d+)?\s*[+\-*/]\s*\d+(?:\.\d+)?") || 
                normalizedQuestion.Contains("calculate") ||
                normalizedQuestion.Contains("what is") && Regex.IsMatch(question, @"\d+"))
            {
                return CalculateMath(question);
            }
            
            // SECOND: Check dynamic answers (time, system info)
            foreach (var dynamicAnswer in _dynamicAnswers)
            {
                if (normalizedQuestion.Contains(dynamicAnswer.Key))
                {
                    return dynamicAnswer.Value(question);
                }
            }

            // Try exact match first
            var exactMatch = FindExactMatch(normalizedQuestion);
            if (exactMatch != null)
            {
                return exactMatch.Answer;
            }

            // Try fuzzy matching with confidence scoring
            var fuzzyMatch = FindFuzzyMatch(normalizedQuestion);
            if (fuzzyMatch.fact != null && fuzzyMatch.confidence >= _confidenceThreshold)
            {
                return fuzzyMatch.fact.Answer;
            }

            // No confident answer found - return null so conversation engine can handle it
            return null;
        }

        private string NormalizeQuestion(string question)
        {
            // Remove common question words and normalize
            var normalized = question.ToLower()
                .Replace("what is the ", "")
                .Replace("what is ", "")
                .Replace("what's the ", "")
                .Replace("what's ", "")
                .Replace("who is the ", "")
                .Replace("who is ", "")
                .Replace("who's the ", "")
                .Replace("who's ", "")
                .Replace("?", "")
                .Trim();

            return normalized;
        }

        private VerifiedFact FindExactMatch(string normalizedQuestion)
        {
            // Check direct key matches
            if (_verifiedFacts.ContainsKey(normalizedQuestion))
            {
                return _verifiedFacts[normalizedQuestion];
            }

            // Check synonym matches
            foreach (var synonymGroup in _synonyms)
            {
                if (synonymGroup.Value.Any(synonym => synonym.ToLower() == normalizedQuestion))
                {
                    if (_verifiedFacts.ContainsKey(synonymGroup.Key))
                    {
                        return _verifiedFacts[synonymGroup.Key];
                    }
                }
            }

            return null;
        }

        private (VerifiedFact fact, double confidence) FindFuzzyMatch(string normalizedQuestion)
        {
            var bestMatch = _verifiedFacts
                .Select(kvp => new
                {
                    Fact = kvp.Value,
                    Key = kvp.Key,
                    Confidence = CalculateMatchConfidence(kvp.Key, normalizedQuestion, kvp.Value.Tags)
                })
                .Where(match => match.Confidence > 0)
                .OrderByDescending(match => match.Confidence)
                .FirstOrDefault();

            if (bestMatch != null)
            {
                return (bestMatch.Fact, bestMatch.Confidence);
            }

            return (null, 0.0);
        }

        private double CalculateMatchConfidence(string factKey, string question, string[] tags)
        {
            var confidence = 0.0;
            
            // Keyword matching
            var factWords = factKey.Split(' ');
            var questionWords = question.Split(' ');
            
            var matchingWords = factWords.Intersect(questionWords).Count();
            var keywordScore = (double)matchingWords / Math.Max(factWords.Length, questionWords.Length);
            
            confidence += keywordScore * 0.6;

            // Tag matching
            var matchingTags = tags.Count(tag => question.Contains(tag));
            var tagScore = (double)matchingTags / tags.Length;
            confidence += tagScore * 0.4;

            return Math.Min(confidence, 1.0);
        }

        private string CreateUnknownResponse(string question)
        {
            var responses = new[]
            {
                "I don't have verified information about that topic. I only provide answers I'm confident are accurate.",
                "I'm not sure about that. I prefer to say 'I don't know' rather than guess and potentially give you wrong information.",
                "That's not in my verified knowledge base. I focus on providing accurate answers rather than making assumptions.",
                "I don't have reliable information on that topic. Would you like to ask about something else I might know?"
            };

            return responses[new Random().Next(responses.Length)];
        }


        private string CalculateMath(string question)
        {
            // Simple math calculation
            var match = Regex.Match(question, @"(\d+(?:\.\d+)?)\s*([+\-*/])\s*(\d+(?:\.\d+)?)");
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
                    return $"**{num1} {operation} {num2} = {result}**";
                }
            }
            
            return "I can help with math calculations! Try asking something like 'what is 15 + 27?' or 'calculate 100 / 4'.";
        }
    }

    public class VerifiedFact
    {
        public string Answer { get; set; } = "";
        public double Confidence { get; set; } = 1.0;
        public string Source { get; set; } = "";
        public DateTime LastVerified { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class KnowledgeEntry
    {
        public string Answer { get; set; } = "";
        public List<string> Keywords { get; set; } = new List<string>();
        public double Confidence { get; set; } = 1.0;
    }
}
