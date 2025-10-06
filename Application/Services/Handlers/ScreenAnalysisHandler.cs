using System;
using System.Text.Json;
using System.Threading.Tasks;
using Smartitecture.Services.AgentTools;

namespace Smartitecture.Services.Handlers
{
    /// <summary>
    /// Handler for screen capture and analysis requests.
    /// </summary>
    public class ScreenAnalysisHandler : IRequestHandler<AgentRequestDto, AgentResponse>
    {
        /// <summary>
        /// Determines if this handler can process the given request.
        /// </summary>
        public bool CanHandle(AgentRequestDto request)
        {
            string lowercaseRequest = request.UserInput.ToLower();
            
            return lowercaseRequest.Contains("analyze screen") ||
                   lowercaseRequest.Contains("capture screen") ||
                   lowercaseRequest.Contains("check my screen") ||
                   lowercaseRequest.Contains("look at my screen") ||
                   lowercaseRequest.Contains("what's on my screen") ||
                   lowercaseRequest.Contains("what is on my screen") ||
                   lowercaseRequest.Contains("scan my screen") ||
                   lowercaseRequest.Contains("read my screen") ||
                   lowercaseRequest.Contains("analyze this window") ||
                   lowercaseRequest.Contains("check this window") ||
                   lowercaseRequest.Contains("look at this window") ||
                   lowercaseRequest.Contains("what's in this window") ||
                   lowercaseRequest.Contains("scan this window") ||
                   lowercaseRequest.Contains("read this window") ||
                   lowercaseRequest.Contains("screenshot") ||
                   lowercaseRequest.Contains("screen shot");
        }

        /// <summary>
        /// Handles screen analysis requests.
        /// </summary>
        public async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            string lowercaseRequest = request.UserInput.ToLower();
            
            try
            {
                // Determine the type of analysis to perform
                string analysisType = "general";
                
                if (lowercaseRequest.Contains("security") || 
                    lowercaseRequest.Contains("virus") || 
                    lowercaseRequest.Contains("malware") || 
                    lowercaseRequest.Contains("scam") || 
                    lowercaseRequest.Contains("alert") || 
                    lowercaseRequest.Contains("warning") || 
                    lowercaseRequest.Contains("popup"))
                {
                    analysisType = "security";
                }
                else if (lowercaseRequest.Contains("text") || 
                         lowercaseRequest.Contains("read") || 
                         lowercaseRequest.Contains("ocr") || 
                         lowercaseRequest.Contains("extract text"))
                {
                    analysisType = "text";
                }
                else if (lowercaseRequest.Contains("ui") || 
                         lowercaseRequest.Contains("interface") || 
                         lowercaseRequest.Contains("button") || 
                         lowercaseRequest.Contains("control") || 
                         lowercaseRequest.Contains("element") || 
                         lowercaseRequest.Contains("navigate"))
                {
                    analysisType = "ui";
                }
                
                // Determine whether to capture the entire screen or just the active window
                bool captureActiveWindowOnly = lowercaseRequest.Contains("window") || 
                                             lowercaseRequest.Contains("active") || 
                                             lowercaseRequest.Contains("current window") || 
                                             lowercaseRequest.Contains("this window");
                
                // Create parameters for the screen analysis
                var parameters = new { analysis_type = analysisType };
                var jsonParams = JsonSerializer.Serialize(parameters);
                var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                
                string result;
                
                if (captureActiveWindowOnly)
                {
                    // Capture and analyze only the active window
                    result = await ScreenAnalysisTools.Tools[1].Handler(jsonElement);
                }
                else
                {
                    // Capture and analyze the entire screen
                    result = await ScreenAnalysisTools.Tools[0].Handler(jsonElement);
                }
                
                return new AgentResponse
                {
                    Message = result,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Message = $"An error occurred while analyzing the screen: {ex.Message}",
                    IsSuccess = false
                };
            }
        }
    }
}
