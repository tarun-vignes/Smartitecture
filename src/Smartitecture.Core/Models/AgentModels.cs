using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Smartitecture.Core.Models
{
    // Request model for running the agent
    public class AgentRunRequest
    {
        [JsonPropertyName("input")]
        public string Input { get; set; }

        [JsonPropertyName("max_iterations")]
        public int? MaxIterations { get; set; }
    }

    // Response model from the agent
    public class AgentRunResponse
    {
        [JsonPropertyName("result")]
        public object Result { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("iterations")]
        public int Iterations { get; set; }

        [JsonPropertyName("memory")]
        public List<MemoryItem> Memory { get; set; }
    }

    // Represents a single step in the agent's memory/thought process
    public class MemoryItem
    {
        [JsonPropertyName("thought")]
        public string Thought { get; set; }

        [JsonPropertyName("action")]
        public AgentAction Action { get; set; }

        [JsonPropertyName("observation")]
        public AgentObservation Observation { get; set; }
    }

    // Represents the action the agent decides to take
    public class AgentAction
    {
        [JsonPropertyName("tool_name")]
        public string ToolName { get; set; }

        [JsonPropertyName("tool_parameters")]
        public Dictionary<string, object> ToolParameters { get; set; }
    }

    // Represents the observation/result from an action
    public class AgentObservation
    {
        [JsonPropertyName("result")]
        public object Result { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
