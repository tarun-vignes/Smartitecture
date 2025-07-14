using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIPal.API.Models
{
    /// <summary>
    /// Data transfer object for structured queries sent to the AI assistant.
    /// Provides a more structured approach compared to free-form chat messages.
    /// </summary>
    public class QueryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the query.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the type of query being performed.
        /// Examples: "command", "question", "search", etc.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the specific action or intent of the query.
        /// Examples: "open", "find", "calculate", etc.
        /// </summary>
        [JsonPropertyName("action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the natural language text of the query.
        /// This is the original text input by the user.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the parameters associated with the query.
        /// These are key-value pairs that provide additional context or configuration.
        /// </summary>
        [JsonPropertyName("parameters")]
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the context identifier for maintaining conversation context.
        /// Allows for follow-up queries that reference previous queries.
        /// </summary>
        [JsonPropertyName("contextId")]
        public string ContextId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the query was created.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
