using System;
using System.Text.Json;
using System.Threading.Tasks;
using AIPal.Services.AgentTools;

namespace AIPal.Services.Handlers
{
    /// <summary>
    /// Handler for system diagnostics and performance monitoring requests.
    /// </summary>
    public class SystemDiagnosticsHandler : IRequestHandler<AgentRequestDto, AgentResponse>
    {
        /// <summary>
        /// Determines if this handler can process the given request.
        /// </summary>
        public bool CanHandle(AgentRequestDto request)
        {
            string lowercaseRequest = request.UserInput.ToLower();
            
            return lowercaseRequest.Contains("temperature") ||
                   lowercaseRequest.Contains("performance") ||
                   lowercaseRequest.Contains("cpu usage") ||
                   lowercaseRequest.Contains("memory usage") ||
                   lowercaseRequest.Contains("disk space") ||
                   lowercaseRequest.Contains("startup") ||
                   lowercaseRequest.Contains("boot time") ||
                   lowercaseRequest.Contains("processes") ||
                   lowercaseRequest.Contains("system health") ||
                   lowercaseRequest.Contains("computer is slow") ||
                   lowercaseRequest.Contains("pc is slow") ||
                   lowercaseRequest.Contains("laptop is slow") ||
                   lowercaseRequest.Contains("optimize") ||
                   lowercaseRequest.Contains("speed up") ||
                   lowercaseRequest.Contains("computer hot") ||
                   lowercaseRequest.Contains("pc hot") ||
                   lowercaseRequest.Contains("laptop hot") ||
                   lowercaseRequest.Contains("overheating") ||
                   lowercaseRequest.Contains("fan noise");
        }

        /// <summary>
        /// Handles system diagnostics requests by determining the appropriate diagnostic tool to use.
        /// </summary>
        public async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            string lowercaseRequest = request.UserInput.ToLower();
            
            try
            {
                // Determine which diagnostic tool to use based on the request
                if (lowercaseRequest.Contains("temperature") || 
                    lowercaseRequest.Contains("hot") || 
                    lowercaseRequest.Contains("overheating") || 
                    lowercaseRequest.Contains("fan"))
                {
                    // Create parameters for temperature check
                    var parameters = new { type = "all" };
                    var jsonParams = JsonSerializer.Serialize(parameters);
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SystemDiagnosticsTools.Tools[0].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else if (lowercaseRequest.Contains("performance") || 
                         lowercaseRequest.Contains("cpu usage") || 
                         lowercaseRequest.Contains("memory usage") ||
                         lowercaseRequest.Contains("slow"))
                {
                    var jsonParams = "{}";
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SystemDiagnosticsTools.Tools[1].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else if (lowercaseRequest.Contains("startup") || 
                         lowercaseRequest.Contains("boot time"))
                {
                    var jsonParams = "{}";
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SystemDiagnosticsTools.Tools[2].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else if (lowercaseRequest.Contains("disk") || 
                         lowercaseRequest.Contains("drive") || 
                         lowercaseRequest.Contains("storage"))
                {
                    // Create parameters for disk check
                    var parameters = new { drive = "" };  // Check all drives by default
                    var jsonParams = JsonSerializer.Serialize(parameters);
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SystemDiagnosticsTools.Tools[3].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else if (lowercaseRequest.Contains("processes") || 
                         lowercaseRequest.Contains("running"))
                {
                    // Create parameters for process check
                    var parameters = new { sort_by = "cpu", count = 10 };
                    var jsonParams = JsonSerializer.Serialize(parameters);
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    
                    string result = await SystemDiagnosticsTools.Tools[4].Handler(jsonElement);
                    return new AgentResponse
                    {
                        Message = result,
                        IsSuccess = true
                    };
                }
                else
                {
                    // If the request is more general, provide a comprehensive system check
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("# Comprehensive System Health Check\n");
                    
                    // Get performance metrics
                    var jsonParams = "{}";
                    var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                    sb.AppendLine(await SystemDiagnosticsTools.Tools[1].Handler(jsonElement));
                    sb.AppendLine("\n---\n");
                    
                    // Check disk health
                    var diskParams = new { drive = "" };
                    var diskJsonParams = JsonSerializer.Serialize(diskParams);
                    var diskJsonElement = JsonDocument.Parse(diskJsonParams).RootElement;
                    sb.AppendLine(await SystemDiagnosticsTools.Tools[3].Handler(diskJsonElement));
                    
                    return new AgentResponse
                    {
                        Message = sb.ToString(),
                        IsSuccess = true
                    };
                }
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Message = $"An error occurred while analyzing your system: {ex.Message}",
                    IsSuccess = false
                };
            }
        }
    }
}
