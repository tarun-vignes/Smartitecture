using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIPal.API.Models
{
    /// <summary>
    /// Data transfer object for responses from the AI agent.
    /// Contains the result of processing a request along with any actions taken.
    /// </summary>
    public class AgentResponseDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the response.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the text response from the agent.
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets the list of actions taken by the agent during processing.
        /// </summary>
        [JsonPropertyName("actions")]
        public List<AgentActionDto> Actions { get; set; } = new List<AgentActionDto>();

        /// <summary>
        /// Gets or sets a value indicating whether the agent successfully completed the task.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets any error message if the agent encountered an issue.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the response was created.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents an action taken by an agent using a tool.
    /// </summary>
    public class AgentActionDto
    {
        /// <summary>
        /// Gets or sets the name of the tool used.
        /// </summary>
        [JsonPropertyName("toolName")]
        public string ToolName { get; set; }

        /// <summary>
        /// Gets or sets the input provided to the tool.
        /// </summary>
        [JsonPropertyName("input")]
        public Dictionary<string, object> Input { get; set; }

        /// <summary>
        /// Gets or sets the output received from the tool.
        /// </summary>
        [JsonPropertyName("output")]
        public object Output { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tool execution was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }

    /// <summary>
    /// Provides information about a tool that an agent can use.
    /// </summary>
    public class AgentToolInfoDto
    {
        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of what the tool does.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parameters that the tool accepts.
        /// </summary>
        [JsonPropertyName("parameters")]
        public Dictionary<string, ToolParameter> Parameters { get; set; }
    }
}
