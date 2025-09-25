using System;
using System.Text.Json;
using System.Threading.Tasks;
using AIPal.Application.Security.Tools;
using AIPal.Application.Services;

namespace AIPal.Application.Security.Handlers
{
    /// <summary>
    /// Handler for security and system optimization requests.
    /// </summary>
    public class SecurityHandler : IRequestHandler<AgentRequestDto, AgentResponse>
    {
        /// <summary>
        /// Determines if this handler can process the given request.
        /// </summary>
        public bool CanHandle(AgentRequestDto request)
        {
            string lowercaseRequest = request.UserInput.ToLower();
            
            return lowercaseRequest.Contains("virus") ||
                   lowercaseRequest.Contains("malware") ||
                   lowercaseRequest.Contains("scam") ||
                   lowercaseRequest.Contains("security") ||
                   lowercaseRequest.Contains("optimize") ||
                   lowercaseRequest.Contains("speed up") ||
                   lowercaseRequest.Contains("slow computer") ||
                   lowercaseRequest.Contains("slow laptop") ||
                   lowercaseRequest.Contains("close apps") ||
                   lowercaseRequest.Contains("popup") ||
                   lowercaseRequest.Contains("alert") ||
                   lowercaseRequest.Contains("warning") ||
                   lowercaseRequest.Contains("infected");
        }

        /// <summary>
        /// Handles security and optimization requests.
        /// </summary>
        public async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            string lowercaseRequest = request.UserInput.ToLower();
            
            try
            {
                // Determine which security tool to use based on the request
                if (lowercaseRequest.Contains("virus") || 
                    lowercaseRequest.Contains("malware") || 
                    lowercaseRequest.Contains("infected") ||
                    lowercaseRequest.Contains("security") ||
                    lowercaseRequest.Contains("scam"))
                {
                    // Check for malware with detailed analysis for more comprehensive requests
                    string result = await SecurityTools.CheckForMalware(new JsonElement());
                    return new AgentResponse { Message = result };
                }
                else if (lowercaseRequest.Contains("explain") || 
                         lowercaseRequest.Contains("what is") || 
                         lowercaseRequest.Contains("what's") ||
                         lowercaseRequest.Contains("what are") ||
                         lowercaseRequest.Contains("meaning"))
                {
                    // Extract the security term to explain
                    string term = ExtractSecurityTerm(lowercaseRequest);
                    
                    if (!string.IsNullOrEmpty(term))
                    {
                        var parameters = JsonSerializer.Serialize(new { term });
                        string result = await SecurityTools.ExplainSecurityTerm(JsonDocument.Parse(parameters).RootElement);
                        return new AgentResponse { Message = result };
                    }
                }
                else if (lowercaseRequest.Contains("windows defender") ||
                         lowercaseRequest.Contains("antivirus") ||
                         lowercaseRequest.Contains("protection"))
                {
                    // Check Windows Defender status
                    string result = await SecurityTools.CheckWindowsDefenderStatus(new JsonElement());
                    return new AgentResponse { Message = result };
                }
                else if (lowercaseRequest.Contains("optimize") ||
                         lowercaseRequest.Contains("speed up") ||
                         lowercaseRequest.Contains("slow") ||
                         lowercaseRequest.Contains("improve"))
                {
                    // Suggest security improvements
                    string result = await SecurityTools.SuggestSecurityImprovements(new JsonElement());
                    return new AgentResponse { Message = result };
                }
                
                // Default response for security-related queries
                string defaultResult = await SecurityTools.CheckForMalware(new JsonElement());
                return new AgentResponse { Message = defaultResult };
            }
            catch (Exception ex)
            {
                return new AgentResponse 
                { 
                    Message = "I'm sorry, I encountered an error while trying to check your computer's security. " +
                              "Here are some general security tips:\n\n" +
                              "1. Keep your operating system and software updated\n" +
                              "2. Use a reputable antivirus program\n" +
                              "3. Be careful about clicking on links or opening attachments in emails\n" +
                              "4. Use strong, unique passwords for your accounts\n" +
                              "5. Back up your important files regularly"
                };
            }
        }
        
        /// <summary>
        /// Extracts a security term from the user's request.
        /// </summary>
        private string ExtractSecurityTerm(string request)
        {
            // Common security terms to look for
            string[] securityTerms = new string[] 
            { 
                "malware", "virus", "firewall", "phishing", "vpn", "encryption", 
                "two-factor authentication", "ransomware", "spyware", "trojan", 
                "adware", "backup", "patch", "cookies", "https"
            };
            
            // Check if any security term is in the request
            foreach (var term in securityTerms)
            {
                if (request.Contains(term))
                {
                    return term;
                }
            }
            
            // Try to extract term after common phrases
            string[] phrases = new string[] 
            { 
                "explain ", "what is ", "what's ", "what are ", "meaning of ", 
                "definition of ", "tell me about "
            };
            
            foreach (var phrase in phrases)
            {
                int index = request.IndexOf(phrase);
                if (index >= 0)
                {
                    // Extract everything after the phrase
                    string potentialTerm = request.Substring(index + phrase.Length).Trim();
                    
                    // Remove any trailing punctuation or words
                    int endIndex = potentialTerm.IndexOf(" ");
                    if (endIndex > 0)
                    {
                        potentialTerm = potentialTerm.Substring(0, endIndex);
                    }
                    
                    return potentialTerm;
                }
            }
            
            return string.Empty;
        }
    }
}
