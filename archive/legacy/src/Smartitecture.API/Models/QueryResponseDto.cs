using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIPal.API.Models
{
    /// <summary>
    /// Data transfer object for responses to structured queries.
    /// Contains the result of processing a query along with metadata.
    /// </summary>
    public class QueryResponseDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the response.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the ID of the query this response is for.
        /// </summary>
        [JsonPropertyName("queryId")]
        public string QueryId { get; set; }

        /// <summary>
        /// Gets or sets the status of the query processing.
        /// Examples: "success", "failed", "partial"
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the primary textual response to the query.
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets structured data returned by the query.
        /// This could be JSON data, lists, or other structured information.
        /// </summary>
        [JsonPropertyName("data")]
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets information about any actions that were performed.
        /// </summary>
        [JsonPropertyName("actions")]
        public List<QueryAction> Actions { get; set; } = new List<QueryAction>();

        /// <summary>
        /// Gets or sets suggested follow-up queries based on this query and response.
        /// </summary>
        [JsonPropertyName("suggestions")]
        public List<string> Suggestions { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the context identifier for maintaining conversation context.
        /// </summary>
        [JsonPropertyName("contextId")]
        public string ContextId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the response was created.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents an action that was performed as part of processing a query.
    /// </summary>
    public class QueryAction
    {
        /// <summary>
        /// Gets or sets the type of action performed.
        /// Examples: "command", "search", "calculation"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the action performed.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether the action was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets details about the action result.
        /// </summary>
        [JsonPropertyName("result")]
        public string Result { get; set; }
    }
}
