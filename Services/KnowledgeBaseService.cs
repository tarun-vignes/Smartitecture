using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    /// <summary>
    /// Knowledge Base Service that provides actual answers to questions
    /// </summary>
    public class KnowledgeBaseService
    {
        private readonly Dictionary<string, KnowledgeEntry> _knowledgeBase;
        private readonly Dictionary<string, Func<string, string>> _dynamicAnswers;

        public KnowledgeBaseService()
        {
            _knowledgeBase = new Dictionary<string, KnowledgeEntry>();
            _dynamicAnswers = new Dictionary<string, Func<string, string>>();
            InitializeKnowledgeBase();
            InitializeDynamicAnswers();
        }

        private void InitializeKnowledgeBase()
        {
            // Colors
            AddKnowledge("color of grass", "Grass is **green** due to chlorophyll, the pigment that helps plants photosynthesize and convert sunlight into energy.");
            AddKnowledge("color of sky", "The sky appears **blue** because of Rayleigh scattering - shorter blue wavelengths of sunlight are scattered more by air molecules than longer wavelengths.");
            AddKnowledge("color of sun", "The sun appears **yellow** from Earth due to atmospheric scattering, but it's actually **white** when viewed from space.");
            AddKnowledge("color of blood", "Blood is **red** because of hemoglobin, an iron-containing protein that carries oxygen and gives blood its distinctive red color.");

            // Science and Nature
            AddKnowledge("speed of light", "The speed of light in a vacuum is **299,792,458 meters per second** (approximately 300,000 km/s). This is a fundamental constant of physics.");
            AddKnowledge("gravity on earth", "Earth's gravity is **9.81 m/s²** at sea level. This means objects accelerate toward Earth at this rate when falling.");
            AddKnowledge("boiling point of water", "Water boils at **100°C (212°F)** at standard atmospheric pressure (1 atm or 101.325 kPa).");
            AddKnowledge("freezing point of water", "Water freezes at **0°C (32°F)** at standard atmospheric pressure.");

            // Geography
            AddKnowledge("capital of france", "The capital of France is **Paris**, located in the north-central part of the country along the Seine River.");
            AddKnowledge("capital of usa", "The capital of the United States is **Washington, D.C.**, located on the east coast between Maryland and Virginia.");
            AddKnowledge("capital of japan", "The capital of Japan is **Tokyo**, the world's most populous metropolitan area with over 37 million people.");
            AddKnowledge("largest ocean", "The **Pacific Ocean** is the largest ocean, covering about 46% of the world's water surface and 32% of Earth's total surface area.");

            // Technology
            AddKnowledge("who invented computer", "The modern computer was developed by many people, but **Alan Turing** created the theoretical foundation, and **John von Neumann** designed the architecture most computers use today.");
            AddKnowledge("who invented internet", "The internet was invented by **ARPANET researchers** in the late 1960s, with key contributions from **Vint Cerf** and **Bob Kahn** who developed TCP/IP protocols.");
            AddKnowledge("what is ai", "**Artificial Intelligence (AI)** is computer technology that can perform tasks typically requiring human intelligence, like learning, reasoning, problem-solving, and understanding language.");

            // History
            AddKnowledge("when did world war 2 end", "World War II ended on **September 2, 1945**, when Japan formally surrendered aboard the USS Missouri in Tokyo Bay.");
            AddKnowledge("who was first president", "**George Washington** was the first President of the United States, serving from 1789 to 1797.");
            AddKnowledge("when was america founded", "The United States was founded on **July 4, 1776**, when the Declaration of Independence was adopted.");

            // Current Events & People
            AddKnowledge("president of united states", "As of 2024, **Joe Biden** is the President of the United States, serving as the 46th president since January 20, 2021.");
            AddKnowledge("current year", $"The current year is **{DateTime.Now.Year}**.");

            // Math and Numbers
            AddKnowledge("value of pi", "Pi (π) is approximately **3.14159265359**, representing the ratio of a circle's circumference to its diameter.");
            AddKnowledge("square root of 64", "The square root of 64 is **8** (8 × 8 = 64).");
            AddKnowledge("what is fibonacci", "The **Fibonacci sequence** is 0, 1, 1, 2, 3, 5, 8, 13, 21, 34... where each number is the sum of the two preceding ones.");

            // Programming
            AddKnowledge("what is python", "**Python** is a high-level programming language known for its simplicity and readability, widely used for web development, data science, AI, and automation.");
            AddKnowledge("what is javascript", "**JavaScript** is a programming language primarily used for web development to create interactive websites and web applications.");
            AddKnowledge("what is c#", "**C#** is a modern, object-oriented programming language developed by Microsoft, commonly used for desktop applications, web development, and game development.");
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

        private void AddKnowledge(string key, string answer)
        {
            _knowledgeBase[key.ToLower()] = new KnowledgeEntry
            {
                Answer = answer,
                Keywords = key.Split(' ').ToList(),
                Confidence = 1.0
            };
        }

        public string GetAnswer(string question)
        {
            var lowerQuestion = question.ToLower();
            
            // First check dynamic answers
            foreach (var dynamicAnswer in _dynamicAnswers)
            {
                if (lowerQuestion.Contains(dynamicAnswer.Key))
                {
                    return dynamicAnswer.Value(question);
                }
            }

            // Then check knowledge base
            var bestMatch = FindBestMatch(lowerQuestion);
            if (bestMatch != null)
            {
                return bestMatch.Answer;
            }

            // Handle specific question patterns
            if (lowerQuestion.StartsWith("what is the color of") || lowerQuestion.StartsWith("what color is"))
            {
                return HandleColorQuestion(lowerQuestion);
            }

            if (lowerQuestion.StartsWith("who is") || lowerQuestion.StartsWith("who was"))
            {
                return HandlePersonQuestion(lowerQuestion);
            }

            if (lowerQuestion.StartsWith("when") || lowerQuestion.Contains("when did"))
            {
                return HandleTimeQuestion(lowerQuestion);
            }

            if (lowerQuestion.StartsWith("where is") || lowerQuestion.StartsWith("where"))
            {
                return HandleLocationQuestion(lowerQuestion);
            }

            if (lowerQuestion.StartsWith("how") || lowerQuestion.Contains("how to"))
            {
                return HandleHowQuestion(lowerQuestion);
            }

            return null; // No answer found
        }

        private KnowledgeEntry FindBestMatch(string question)
        {
            var bestMatch = _knowledgeBase
                .Where(entry => entry.Key.Split(' ').Any(word => question.Contains(word)))
                .OrderByDescending(entry => CalculateMatchScore(entry.Key, question))
                .FirstOrDefault();

            return bestMatch.Value;
        }

        private double CalculateMatchScore(string knowledgeKey, string question)
        {
            var keyWords = knowledgeKey.Split(' ');
            var matchCount = keyWords.Count(word => question.Contains(word));
            return (double)matchCount / keyWords.Length;
        }

        private string HandleColorQuestion(string question)
        {
            if (question.Contains("grass")) return "Grass is **green** because of chlorophyll.";
            if (question.Contains("sky")) return "The sky is **blue** due to light scattering.";
            if (question.Contains("sun")) return "The sun appears **yellow** from Earth but is actually white.";
            if (question.Contains("ocean") || question.Contains("water")) return "Ocean water appears **blue** due to light absorption and scattering.";
            if (question.Contains("snow")) return "Snow is **white** because it reflects all colors of light equally.";
            
            return "I'd be happy to tell you about colors! Could you be more specific about what you'd like to know the color of?";
        }

        private string HandlePersonQuestion(string question)
        {
            if (question.Contains("president"))
            {
                if (question.Contains("first")) return "**George Washington** was the first President of the United States.";
                if (question.Contains("current")) return "**Joe Biden** is the current President of the United States.";
            }
            
            if (question.Contains("einstein")) return "**Albert Einstein** was a theoretical physicist famous for the theory of relativity and E=mc².";
            if (question.Contains("newton")) return "**Isaac Newton** was an English physicist and mathematician who formulated the laws of motion and universal gravitation.";
            if (question.Contains("shakespeare")) return "**William Shakespeare** was an English playwright and poet, widely regarded as the greatest writer in the English language.";
            
            return "I can provide information about many historical figures, scientists, and leaders. Could you be more specific about who you're asking about?";
        }

        private string HandleTimeQuestion(string question)
        {
            if (question.Contains("world war")) return "World War II ended on **September 2, 1945**.";
            if (question.Contains("america") && question.Contains("founded")) return "America was founded on **July 4, 1776**.";
            if (question.Contains("internet") && question.Contains("invented")) return "The internet was developed in the **late 1960s** with ARPANET.";
            
            return "I can provide historical dates and timelines. What specific event are you asking about?";
        }

        private string HandleLocationQuestion(string question)
        {
            if (question.Contains("paris")) return "**Paris** is the capital of France, located in north-central France.";
            if (question.Contains("tokyo")) return "**Tokyo** is the capital of Japan and the world's largest metropolitan area.";
            if (question.Contains("new york")) return "**New York City** is located in southeastern New York state at the mouth of the Hudson River.";
            
            return "I can help with geographical information. What location are you asking about?";
        }

        private string HandleHowQuestion(string question)
        {
            if (question.Contains("photosynthesis")) return "**Photosynthesis** works by plants using chlorophyll to convert sunlight, carbon dioxide, and water into glucose and oxygen.";
            if (question.Contains("computer")) return "**Computers** work by processing binary data (0s and 1s) through electronic circuits to perform calculations and operations.";
            if (question.Contains("internet")) return "The **internet** works through a network of interconnected computers using standardized protocols like TCP/IP to exchange data.";
            
            return "I can explain how many things work! What specific process or system are you curious about?";
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

    public class KnowledgeEntry
    {
        public string Answer { get; set; } = "";
        public List<string> Keywords { get; set; } = new List<string>();
        public double Confidence { get; set; } = 1.0;
    }
}
