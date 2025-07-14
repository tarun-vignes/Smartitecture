using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Smartitecture.Core.Models
{
    public class AgentStateResponse
    {
        [JsonPropertyName("state")]
        public string State { get; set; }
        
        [JsonPropertyName("iteration")]
        public int Iteration { get; set; }
        
        [JsonPropertyName("max_iterations")]
        public int MaxIterations { get; set; }
        
        [JsonPropertyName("available_tools")]
        public List<string> AvailableTools { get; set; } = new List<string>();
        
        [JsonPropertyName("memory")]
        public List<Dictionary<string, object>> Memory { get; set; } = new List<Dictionary<string, object>>();
    }
}
