using System;
using System.Text.Json;
using System.Threading.Tasks;
using Smartitecture.Services.AgentTools;

namespace Smartitecture.Services.Handlers
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
                    bool detailed = lowercaseRequest.Contains("detailed") || 
                                   lowercaseRequest.Contains("thorough") ||
                                   lowercaseRequest.Contains("complete");
                    
                    var parameters = new { detailed = detailed };
                    var jsonParams = JsonSerializer.Serialize(parameters);
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SecurityTools.Tools[0].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else if (lowercaseRequest.Contains("optimize") || 
                         lowercaseRequest.Contains("speed up") ||
                         lowercaseRequest.Contains("slow computer") ||
                         lowercaseRequest.Contains("slow laptop") ||
                         lowercaseRequest.Contains("close apps"))
                {
                    // Optimize system by closing unnecessary applications
                    // Use aggressive mode if specifically requested
                    bool aggressive = lowercaseRequest.Contains("aggressive") || 
                                     lowercaseRequest.Contains("all apps") ||
                                     lowercaseRequest.Contains("everything");
                    
                    var parameters = new { aggressive = aggressive };
                    var jsonParams = JsonSerializer.Serialize(parameters);
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SecurityTools.Tools[1].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else if (lowercaseRequest.Contains("popup") || 
                         lowercaseRequest.Contains("alert") ||
                         lowercaseRequest.Contains("warning"))
                {
                    // Extract the alert text from the request
                    // This is a simple extraction - in a real implementation, you might
                    // want to use more sophisticated NLP to extract the alert text
                    string alertText = request.UserInput;
                    
                    // Remove common phrases to isolate the alert text
                    string[] phrasesToRemove = {
                        "what does this mean", "explain this", "is this real",
                        "is this a scam", "what should i do about", "i got a popup saying",
                        "there's an alert saying", "i see a warning that"
                    };
                    
                    foreach (var phrase in phrasesToRemove)
                    {
                        alertText = alertText.Replace(phrase, "", StringComparison.OrdinalIgnoreCase);
                    }
                    
                    var parameters = new { alert_text = alertText.Trim() };
                    var jsonParams = JsonSerializer.Serialize(parameters);
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SecurityTools.Tools[2].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else
                {
                    // Default to malware check if the intent is unclear
                    var parameters = new { detailed = false };
                    var jsonParams = JsonSerializer.Serialize(parameters);
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SecurityTools.Tools[0].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Message = $"An error occurred while processing your security request: {ex.Message}",
                    IsSuccess = false
                };
            }
        }
    }
}
