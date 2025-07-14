using System.Text.Json.Serialization;

namespace Smartitecture.Core.Models
{
    public class AgentRunRequest
    {
        [JsonPropertyName("input")]
        public string Input { get; set; }
        
        [JsonPropertyName("max_iterations")]
        public int? MaxIterations { get; set; }
    }
}
