using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Smartitecture.Core.Models
{
    public class AgentRunResponse
    {
        [JsonPropertyName("result")]
        public object Result { get; set; }
        
        [JsonPropertyName("state")]
        public string State { get; set; }
        
        [JsonPropertyName("iterations")]
        public int Iterations { get; set; }
        
        [JsonPropertyName("memory")]
        public List<Dictionary<string, object>> Memory { get; set; } = new List<Dictionary<string, object>>();
    }
}
