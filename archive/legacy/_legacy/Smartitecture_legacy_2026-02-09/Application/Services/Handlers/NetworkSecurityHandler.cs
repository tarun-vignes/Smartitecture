using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AIPal.Services.AgentTools;

namespace AIPal.Services.Handlers
{
    /// <summary>
    /// Handler for network security related requests.
    /// Designed to help elderly users understand and manage their Wi-Fi security.
    /// </summary>
    public class NetworkSecurityHandler : IRequestHandler
    {
        /// <summary>
        /// Gets a value indicating whether this handler can process the specified request.
        /// </summary>
        /// <param name="request">The request to check.</param>
        /// <returns>True if this handler can process the request; otherwise, false.</returns>
        public bool CanHandle(string request)
        {
            // Check if the request is related to Wi-Fi or network security
            var wifiPattern = new Regex(@"\b(wifi|wi-fi|wireless|network|internet|connection|router|modem|password|wpa|wep)\b", 
                RegexOptions.IgnoreCase);
                
            var securityPattern = new Regex(@"\b(secure|security|protect|safe|hack|breach|risk|danger|public|open)\b", 
                RegexOptions.IgnoreCase);
                
            var troubleshootPattern = new Regex(@"\b(slow|disconnect|dropping|unstable|weak|signal|strength|problem|issue|trouble|fix|help)\b", 
                RegexOptions.IgnoreCase);
            
            return wifiPattern.IsMatch(request) && 
                  (securityPattern.IsMatch(request) || troubleshootPattern.IsMatch(request));
        }
        
        /// <summary>
        /// Processes the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <returns>The response to the request.</returns>
        public async Task<string> HandleAsync(string request)
        {
            // Determine what type of network security request this is
            if (IsSecurityCheckRequest(request))
            {
                return await HandleSecurityCheckRequest(request);
            }
            else if (IsTermExplanationRequest(request))
            {
                return await HandleTermExplanationRequest(request);
            }
            else if (IsNetworkStatusRequest(request))
            {
                return await HandleNetworkStatusRequest(request);
            }
            else if (IsTroubleshootingRequest(request))
            {
                return await HandleTroubleshootingRequest(request);
            }
            else
            {
                // Default to a security check if we can't determine the specific request type
                return await HandleSecurityCheckRequest(request);
            }
        }
        
        /// <summary>
        /// Determines if the request is asking for a security check.
        /// </summary>
        private bool IsSecurityCheckRequest(string request)
        {
            var pattern = new Regex(@"\b(check|secure|security|safe|protect|how safe|how secure)\b", 
                RegexOptions.IgnoreCase);
                
            return pattern.IsMatch(request);
        }
        
        /// <summary>
        /// Determines if the request is asking for an explanation of terms.
        /// </summary>
        private bool IsTermExplanationRequest(string request)
        {
            var pattern = new Regex(@"\b(what is|what's|explain|mean|definition|understand|wpa|wpa2|wpa3|wep)\b", 
                RegexOptions.IgnoreCase);
                
            return pattern.IsMatch(request);
        }
        
        /// <summary>
        /// Determines if the request is asking for network status.
        /// </summary>
        private bool IsNetworkStatusRequest(string request)
        {
            var pattern = new Regex(@"\b(status|connected|connection|show|display|info|information)\b", 
                RegexOptions.IgnoreCase);
                
            return pattern.IsMatch(request);
        }
        
        /// <summary>
        /// Determines if the request is asking for troubleshooting help.
        /// </summary>
        private bool IsTroubleshootingRequest(string request)
        {
            var pattern = new Regex(@"\b(trouble|problem|issue|slow|disconnect|dropping|unstable|fix|help|not working)\b", 
                RegexOptions.IgnoreCase);
                
            return pattern.IsMatch(request);
        }
        
        /// <summary>
        /// Handles a request to check Wi-Fi security.
        /// </summary>
        private async Task<string> HandleSecurityCheckRequest(string request)
        {
            // In a real implementation, this would call the actual tool
            // For now, we'll return a simulated response
            
            return "I'll check your Wi-Fi security for you. This will help you understand how safe your connection is.\n\n" +
                   "To check your Wi-Fi security, I'll need to:\n" +
                   "1. Look at your current Wi-Fi connection\n" +
                   "2. Check what type of security it's using\n" +
                   "3. Give you recommendations to improve security if needed\n\n" +
                   "Would you like me to proceed with the security check?";
        }
        
        /// <summary>
        /// Handles a request to explain Wi-Fi security terms.
        /// </summary>
        private async Task<string> HandleTermExplanationRequest(string request)
        {
            // Extract the term being asked about
            string term = ExtractSecurityTerm(request);
            
            // In a real implementation, this would call the actual tool
            // For now, we'll return a simulated response
            
            if (!string.IsNullOrEmpty(term))
            {
                return $"I'll explain what '{term}' means in simple terms. Understanding these technical terms can help you make better decisions about your Wi-Fi security.\n\n" +
                       "Would you like me to explain this term for you?";
            }
            else
            {
                return "I can explain common Wi-Fi security terms in simple language. This helps you understand what these technical words mean without the confusing jargon.\n\n" +
                       "Would you like me to explain some common Wi-Fi security terms?";
            }
        }
        
        /// <summary>
        /// Handles a request to check network status.
        /// </summary>
        private async Task<string> HandleNetworkStatusRequest(string request)
        {
            // In a real implementation, this would call the actual tool
            // For now, we'll return a simulated response
            
            return "I'll check your network status for you. This will show you if you're connected to the internet and give details about your connection.\n\n" +
                   "To check your network status, I'll need to:\n" +
                   "1. Check if you're connected to the internet\n" +
                   "2. See what type of connection you're using (Wi-Fi or wired)\n" +
                   "3. Show details about your connection\n\n" +
                   "Would you like me to check your network status?";
        }
        
        /// <summary>
        /// Handles a request to troubleshoot connection issues.
        /// </summary>
        private async Task<string> HandleTroubleshootingRequest(string request)
        {
            // Extract the issue from the request
            string issue = ExtractConnectionIssue(request);
            
            // In a real implementation, this would call the actual tool
            // For now, we'll return a simulated response
            
            if (!string.IsNullOrEmpty(issue))
            {
                return $"I'll help you troubleshoot your '{issue}' issue. I can provide step-by-step instructions to fix common Wi-Fi problems.\n\n" +
                       "To troubleshoot this issue, I'll need to:\n" +
                       "1. Check your current connection status\n" +
                       "2. Identify the most likely causes\n" +
                       "3. Give you simple steps to fix the problem\n\n" +
                       "Would you like me to help troubleshoot this issue?";
            }
            else
            {
                return "I'll help you troubleshoot your Wi-Fi connection. I can provide step-by-step instructions to fix common problems.\n\n" +
                       "To troubleshoot your connection, I'll need to:\n" +
                       "1. Check your current connection status\n" +
                       "2. Identify the most likely causes of your problem\n" +
                       "3. Give you simple steps to fix the issue\n\n" +
                       "Would you like me to help troubleshoot your connection?";
            }
        }
        
        /// <summary>
        /// Extracts a security term from a request.
        /// </summary>
        private string ExtractSecurityTerm(string request)
        {
            // Check for common security terms in the request
            var terms = new[] { "wpa", "wpa2", "wpa3", "wep", "password", "public wifi" };
            
            foreach (var term in terms)
            {
                if (request.ToLower().Contains(term))
                {
                    return term;
                }
            }
            
            // Look for phrases like "what is X" or "explain X"
            var pattern = new Regex(@"(what is|what's|explain|mean|definition)\s+(\w+(\s+\w+)?)", 
                RegexOptions.IgnoreCase);
                
            var match = pattern.Match(request);
            if (match.Success && match.Groups.Count > 2)
            {
                return match.Groups[2].Value.Trim();
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Extracts a connection issue from a request.
        /// </summary>
        private string ExtractConnectionIssue(string request)
        {
            // Check for common connection issues in the request
            var issues = new[] { "slow", "disconnect", "dropping", "unstable", "password", "weak signal" };
            
            foreach (var issue in issues)
            {
                if (request.ToLower().Contains(issue))
                {
                    return issue;
                }
            }
            
            return string.Empty;
        }
    }
}
