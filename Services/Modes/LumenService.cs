using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;
using Smartitecture.Services.Interfaces;
using Smartitecture.Services.Core;

namespace Smartitecture.Services.Modes
{
    /// <summary>
    /// LUMEN - General AI assistant with OS integration, file search, and web research
    /// </summary>
    public class LumenService : IAIMode
    {
        private readonly WebResearchEngine _webResearch;
        private readonly KnowledgeBaseService _knowledgeBase;
        private readonly ILLMService _llmService;

        public string ModeName => "LUMEN";
        public string ModeIcon => "💡";
        public string ModeColor => "#3B82F6";
        public string Description => "General assistant with file search, web research, and OS integration";

        public LumenService(WebResearchEngine webResearch, KnowledgeBaseService knowledgeBase, ILLMService llmService)
        {
            _webResearch = webResearch ?? throw new ArgumentNullException(nameof(webResearch));
            _knowledgeBase = knowledgeBase ?? throw new ArgumentNullException(nameof(knowledgeBase));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
        }

        public bool CanHandle(string query)
        {
            // LUMEN can handle general queries, file operations, and web research
            var generalKeywords = new[]
            {
                "find", "search", "help", "what", "how", "where", "when", "explain",
                "file", "document", "folder", "open", "create", "web", "google", "research"
            };

            var queryLower = query.ToLowerInvariant();
            return generalKeywords.Any(keyword => queryLower.Contains(keyword)) || 
                   IsFileSearchQuery(query) || 
                   IsWebSearchQuery(query);
        }

        public double GetConfidenceScore(string query)
        {
            var score = 0.0;
            var queryLower = query.ToLowerInvariant();

            // Base confidence for general queries
            if (IsGeneralQuery(query)) score += 0.6;
            if (IsFileSearchQuery(query)) score += 0.8;
            if (IsWebSearchQuery(query)) score += 0.7;
            if (IsSystemInfoQuery(query)) score += 0.5;

            // Reduce confidence if it's clearly security or performance related
            if (IsSecurityQuery(query)) score -= 0.3;
            if (IsPerformanceQuery(query)) score -= 0.3;

            return Math.Max(0.0, Math.Min(1.0, score));
        }

        public async Task<AIContext> AnalyzeContextAsync(string query)
        {
            var context = new AIContext
            {
                Query = query,
                PrimaryMode = ModeName,
                ConfidenceScore = GetConfidenceScore(query)
            };

            // Detect if security analysis is needed
            if (IsSecurityQuery(query) || ContainsFileOperations(query))
            {
                context.RequiresSecurityAnalysis = true;
                context.CollaboratingModes.Add("FORTIS");
            }

            // Detect if performance analysis is needed
            if (IsPerformanceQuery(query) || IsSystemInfoQuery(query))
            {
                context.RequiresPerformanceAnalysis = true;
                context.CollaboratingModes.Add("NEXA");
            }

            // Add detected intents
            if (IsFileSearchQuery(query)) context.DetectedIntents.Add("file_search");
            if (IsWebSearchQuery(query)) context.DetectedIntents.Add("web_search");
            if (IsSystemInfoQuery(query)) context.DetectedIntents.Add("system_info");

            return context;
        }

        public async Task<string> ProcessQueryAsync(string query, string conversationId = null)
        {
            try
            {
                // Handle file search queries
                if (IsFileSearchQuery(query))
                {
                    var fileResults = await SearchFilesAsync(query);
                    if (fileResults.Any())
                    {
                        var fileList = string.Join("\n", fileResults.Take(10).Select(f => $"📄 {f}"));
                        return $"I found these files matching your search:\n\n{fileList}";
                    }
                    return "I couldn't find any files matching your search criteria.";
                }

                // Handle web search queries
                if (IsWebSearchQuery(query))
                {
                    var research = await _webResearch.ResearchQuestionAsync(query);
                    var webResults = research?.Answer ?? "";
                    return $"Here's what I found on the web:\n\n{webResults}";
                }

                                // NEW: File system automation
                if (query.ToLowerInvariant().Contains("create folder") || query.ToLowerInvariant().Contains("make folder"))
                {
                    var name = ExtractSearchTerm(query);
                    var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), name);
                    System.IO.Directory.CreateDirectory(path);
                    return $"I'd be happy to help you with that. I created a folder at: {path}";
                }
                if (query.ToLowerInvariant().Contains("move file"))
                {
                    var parts = query.Split('"'); // expect: move file "src" to "dest"
                    if (parts.Length >= 4)
                    {
                        var src = parts[1]; var dst = parts[3];
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dst) ?? dst);
                        System.IO.File.Move(src, dst, true);
                        return $"Here's what I found: moved the file to {dst}. Would you like me to organize more files?";
                    }
                }

                // NEW: Simple email integration (mailto)
                if (query.ToLowerInvariant().Contains("send email"))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "mailto:",
                            UseShellExecute = true
                        });
                        return "That's a great question! I opened your default mail app. Would you like me to draft a subject and body next?";
                    }
                    catch (System.Exception ex)
                    {
                        return $"I tried to help with email, but encountered: {ex.Message}";
                    }
                }

                // NEW: Calendar/productivity stub
                if (query.ToLowerInvariant().Contains("add to calendar") || query.ToLowerInvariant().Contains("create reminder"))
                {
                    return "I'd be happy to help you with that. Calendar integration is coming soon. For now, tell me the title, date, and time, and I can prepare a reminder draft.";
                }// Handle system information queries
                if (IsSystemInfoQuery(query))
                {
                    var systemInfo = GetBasicSystemInfo();
                    return $"Here's your system information:\n\n{systemInfo}";
                }

                // Handle application launching
                if (IsAppLaunchQuery(query))
                {
                    var appName = ExtractAppName(query);
                    var launchResult = await LaunchApplicationAsync(appName);
                    return launchResult;
                }

                // Default to LLM for general conversation
                var prompt = BuildLumenPrompt(query);
                return await _llmService.GetResponseAsync(prompt, conversationId);
            }
            catch (Exception ex)
            {
                return $"I encountered an error while processing your request: {ex.Message}";
            }
        }

        public async Task InitializeAsync()
        {
            // Initialize any required services or configurations
            await Task.CompletedTask;
        }

        #region Private Helper Methods

        private bool IsGeneralQuery(string query)
        {
            var generalPatterns = new[]
            {
                @"\b(what|how|why|when|where|who)\b",
                @"\b(help|assist|explain|tell me)\b",
                @"\b(can you|could you|please)\b"
            };

            return generalPatterns.Any(pattern => Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase));
        }

        private bool IsFileSearchQuery(string query)
        {
            var fileKeywords = new[] { "find", "search", "locate", "file", "document", "folder", "directory" };
            var fileExtensions = new[] { ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
            
            var queryLower = query.ToLowerInvariant();
            return fileKeywords.Any(keyword => queryLower.Contains(keyword)) ||
                   fileExtensions.Any(ext => queryLower.Contains(ext));
        }

        private bool IsWebSearchQuery(string query)
        {
            var webKeywords = new[] { "google", "search web", "look up", "research", "online", "internet" };
            return webKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsSystemInfoQuery(string query)
        {
            var systemKeywords = new[] { "system", "computer", "pc", "specs", "information", "details" };
            return systemKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsSecurityQuery(string query)
        {
            var securityKeywords = new[] { "virus", "malware", "security", "threat", "scan", "safe" };
            return securityKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsPerformanceQuery(string query)
        {
            var performanceKeywords = new[] { "slow", "fast", "performance", "optimize", "speed", "lag" };
            return performanceKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool ContainsFileOperations(string query)
        {
            var fileOpKeywords = new[] { "delete", "move", "copy", "rename", "modify", "edit" };
            return fileOpKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsAppLaunchQuery(string query)
        {
            var launchKeywords = new[] { "open", "launch", "start", "run", "execute" };
            return launchKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private async Task<List<string>> SearchFilesAsync(string query)
        {
            var results = new List<string>();
            var searchTerm = ExtractSearchTerm(query);
            
            if (string.IsNullOrEmpty(searchTerm)) return results;

            try
            {
                // Search in common directories
                var searchPaths = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                foreach (var path in searchPaths.Where(Directory.Exists))
                {
                    var files = Directory.GetFiles(path, $"*{searchTerm}*", SearchOption.TopDirectoryOnly);
                    results.AddRange(files.Take(5)); // Limit results per directory
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                results.Add($"Error searching files: {ex.Message}");
            }

            return results;
        }

        private string ExtractSearchTerm(string query)
        {
            // Simple extraction - look for quoted terms or last word
            var quotedMatch = Regex.Match(query, @"""([^""]+)""");
            if (quotedMatch.Success)
                return quotedMatch.Groups[1].Value;

            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.LastOrDefault() ?? "";
        }

        private string ExtractAppName(string query)
        {
            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // Look for the word after "open", "launch", etc.
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (new[] { "open", "launch", "start", "run" }.Contains(words[i].ToLowerInvariant()))
                {
                    return words[i + 1];
                }
            }
            return words.LastOrDefault() ?? "";
        }

        private async Task<string> LaunchApplicationAsync(string appName)
        {
            try
            {
                // Try to launch the application
                Process.Start(new ProcessStartInfo
                {
                    FileName = appName,
                    UseShellExecute = true
                });
                return $"✅ Launched {appName} successfully.";
            }
            catch (Exception ex)
            {
                return $"❌ Could not launch {appName}: {ex.Message}";
            }
        }

        private string GetBasicSystemInfo()
        {
            try
            {
                var info = new List<string>
                {
                    $"💻 Computer: {Environment.MachineName}",
                    $"👤 User: {Environment.UserName}",
                    $"🖥️ OS: {Environment.OSVersion}",
                    $"⚙️ Processors: {Environment.ProcessorCount}",
                    $"📁 Current Directory: {Environment.CurrentDirectory}",
                    $"🕒 System Uptime: {TimeSpan.FromMilliseconds(Environment.TickCount):dd\\.hh\\:mm\\:ss}"
                };

                return string.Join("\n", info);
            }
            catch (Exception ex)
            {
                return $"Error retrieving system information: {ex.Message}";
            }
        }

        private string BuildLumenPrompt(string query)
        {
            return $@"You are LUMEN, a helpful AI assistant integrated into the Smartitecture system. 
You specialize in:
- General knowledge and conversation
- File management and search assistance
- Web research and information gathering
- Basic system information and guidance

User Query: {query}

Provide a helpful, friendly, and informative response. If the query involves security concerns, mention that FORTIS can provide specialized security analysis. If it involves system performance, mention that NEXA can provide detailed performance optimization.";
        }

        #endregion
    }
}


