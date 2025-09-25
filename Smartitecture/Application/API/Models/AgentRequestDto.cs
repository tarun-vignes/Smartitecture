using System.Text.Json.Serialization;

namespace AIPal.API.Models
{
    /// <summary>
    /// Data transfer object for agent requests sent to the AI assistant.
    /// </summary>
    public class AgentRequestDto
    {
        /// <summary>
        /// Gets or sets the user's message or query.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the context identifier for maintaining conversation context.
        /// Allows for follow-up requests that reference previous interactions.
        /// </summary>
        [JsonPropertyName("contextId")]
        public string ContextId { get; set; }
    }
}
