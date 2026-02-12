using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    /// <summary>
    /// Web Research Engine that searches the internet for real-time information
    /// Provides research capabilities for questions not in the knowledge base
    /// </summary>
    public class WebResearchEngine
    {
        private readonly HttpClient _httpClient;
        private readonly Random _random;

        public WebResearchEngine()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _random = new Random();
        }

        /// <summary>
        /// Research a question using multiple sources and return a comprehensive answer
        /// </summary>
        public async Task<ResearchResult> ResearchQuestionAsync(string question)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Research] Starting research for: '{question}'");

                var researchType = DetermineResearchType(question);
                
                return researchType switch
                {
                    ResearchType.Geography => await ResearchGeography(question),
                    ResearchType.Weather => await ResearchWeather(question),
                    ResearchType.CurrentEvents => await ResearchCurrentEvents(question),
                    ResearchType.Science => await ResearchScience(question),
                    ResearchType.General => await ResearchGeneral(question),
                    _ => await ResearchGeneral(question)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Research] Error: {ex.Message}");
                return new ResearchResult
                {
                    Answer = "I tried to research that for you, but I'm having trouble accessing information right now. Could you try asking something else?",
                    Confidence = 0.1,
                    Sources = new List<string> { "Research engine error" },
                    ResearchType = ResearchType.Error
                };
            }
        }

        private ResearchType DetermineResearchType(string question)
        {
            var lower = question.ToLower();

            // Weather patterns
            if (lower.Contains("weather") || lower.Contains("temperature") || lower.Contains("rain") || 
                lower.Contains("sunny") || lower.Contains("cloudy") || lower.Contains("forecast"))
                return ResearchType.Weather;

            // Geography patterns
            if (lower.Contains("capital") || lower.Contains("country") || lower.Contains("city") || 
                lower.Contains("population") || lower.Contains("located"))
                return ResearchType.Geography;

            // Current events patterns
            if (lower.Contains("president") || lower.Contains("news") || lower.Contains("current") || 
                lower.Contains("latest") || lower.Contains("today") || lower.Contains("recent"))
                return ResearchType.CurrentEvents;

            // Science patterns
            if (lower.Contains("how does") || lower.Contains("why does") || lower.Contains("what causes") ||
                lower.Contains("scientific") || lower.Contains("research") || lower.Contains("study"))
                return ResearchType.Science;

            return ResearchType.General;
        }

        private async Task<ResearchResult> ResearchGeography(string question)
        {
            // For now, use Wikipedia API for geography questions
            try
            {
                var searchTerm = ExtractSearchTerm(question);
                var wikipediaResult = await SearchWikipedia(searchTerm);
                
                if (!string.IsNullOrEmpty(wikipediaResult))
                {
                    return new ResearchResult
                    {
                        Answer = wikipediaResult,
                        Confidence = 0.85,
                        Sources = new List<string> { "Wikipedia" },
                        ResearchType = ResearchType.Geography
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Research] Geography error: {ex.Message}");
            }

            return CreateFallbackResponse(question, ResearchType.Geography);
        }

        private Task<ResearchResult> ResearchWeather(string question)
        {
            // Weather research - for now, provide helpful guidance
            return Task.FromResult(new ResearchResult
            {
                Answer = "I'd love to help with weather information! For real-time weather, I recommend checking a weather app or website like Weather.com. In the future, I'll be able to get your local weather automatically.",
                Confidence = 0.7,
                Sources = new List<string> { "Weather guidance" },
                ResearchType = ResearchType.Weather
            });
        }

        private Task<ResearchResult> ResearchCurrentEvents(string question)
        {
            // Current events research
            return Task.FromResult(new ResearchResult
            {
                Answer = "For the most current news and events, I recommend checking reliable news sources like BBC, Reuters, or AP News. I'm working on getting real-time news capabilities!",
                Confidence = 0.6,
                Sources = new List<string> { "News guidance" },
                ResearchType = ResearchType.CurrentEvents
            });
        }

        private async Task<ResearchResult> ResearchScience(string question)
        {
            // Science research - try Wikipedia for scientific topics
            try
            {
                var searchTerm = ExtractSearchTerm(question);
                var wikipediaResult = await SearchWikipedia(searchTerm);
                
                if (!string.IsNullOrEmpty(wikipediaResult))
                {
                    return new ResearchResult
                    {
                        Answer = wikipediaResult,
                        Confidence = 0.75,
                        Sources = new List<string> { "Wikipedia" },
                        ResearchType = ResearchType.Science
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Research] Science error: {ex.Message}");
            }

            return CreateFallbackResponse(question, ResearchType.Science);
        }

        private async Task<ResearchResult> ResearchGeneral(string question)
        {
            // General research - try Wikipedia first
            try
            {
                var searchTerm = ExtractSearchTerm(question);
                var wikipediaResult = await SearchWikipedia(searchTerm);
                
                if (!string.IsNullOrEmpty(wikipediaResult))
                {
                    return new ResearchResult
                    {
                        Answer = wikipediaResult,
                        Confidence = 0.70,
                        Sources = new List<string> { "Wikipedia" },
                        ResearchType = ResearchType.General
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Research] General error: {ex.Message}");
            }

            return CreateFallbackResponse(question, ResearchType.General);
        }

        private async Task<string?> SearchWikipedia(string searchTerm)
        {
            try
            {
                // Wikipedia API search
                var searchUrl = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(searchTerm)}";
                
                var response = await _httpClient.GetAsync(searchUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);
                    
                    if (json.RootElement.TryGetProperty("extract", out var extract))
                    {
                        var summary = extract.GetString();
                        if (!string.IsNullOrEmpty(summary) && summary.Length > 50)
                        {
                            // Clean up the summary
                            summary = CleanWikipediaSummary(summary);
                            return $"**{searchTerm}**: {summary}\n\n*Source: Wikipedia*";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Research] Wikipedia error: {ex.Message}");
            }

            return null;
        }

        private string CleanWikipediaSummary(string summary)
        {
            // Remove common Wikipedia artifacts
            summary = Regex.Replace(summary, @"\([^)]*\)", ""); // Remove parentheses
            summary = Regex.Replace(summary, @"\s+", " "); // Normalize whitespace
            summary = summary.Trim();
            
            // Limit length
            if (summary.Length > 300)
            {
                var sentences = summary.Split('.', StringSplitOptions.RemoveEmptyEntries);
                summary = string.Join(". ", sentences.Take(2)) + ".";
            }
            
            return summary;
        }

        private string ExtractSearchTerm(string question)
        {
            var lower = question.ToLower();
            
            // Extract key terms from common question patterns
            var patterns = new Dictionary<string, string>
            {
                {@"what is (?:the )?(.+?)(?:\?|$)", "$1"},
                {@"who is (?:the )?(.+?)(?:\?|$)", "$1"},
                {@"where is (?:the )?(.+?)(?:\?|$)", "$1"},
                {@"how (?:does|do) (?:the )?(.+?)(?:\?|$)", "$1"},
                {@"why (?:does|do|is) (?:the )?(.+?)(?:\?|$)", "$1"},
                {@"tell me about (?:the )?(.+?)(?:\?|$)", "$1"}
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(lower, pattern.Key);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            // Fallback: remove common question words
            var cleanedQuestion = Regex.Replace(lower, @"\b(what|who|where|when|why|how|is|are|the|a|an)\b", " ");
            cleanedQuestion = Regex.Replace(cleanedQuestion, @"[^\w\s]", " ");
            cleanedQuestion = Regex.Replace(cleanedQuestion, @"\s+", " ").Trim();
            
            return cleanedQuestion;
        }

        private ResearchResult CreateFallbackResponse(string question, ResearchType type)
        {
            var responses = new[]
            {
                "I'm still learning how to research that topic. Could you try asking about something else?",
                "That's an interesting question! I don't have reliable information about that right now.",
                "I'd love to help with that, but I need to improve my research capabilities for that topic.",
                "Great question! I'm working on getting better at researching topics like that."
            };

            return new ResearchResult
            {
                Answer = responses[_random.Next(responses.Length)],
                Confidence = 0.3,
                Sources = new List<string> { "Fallback response" },
                ResearchType = type
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ResearchResult
    {
        public string Answer { get; set; } = "";
        public double Confidence { get; set; }
        public List<string> Sources { get; set; } = new();
        public ResearchType ResearchType { get; set; }
    }

    public enum ResearchType
    {
        Geography,
        Weather,
        CurrentEvents,
        Science,
        General,
        Error
    }
}
