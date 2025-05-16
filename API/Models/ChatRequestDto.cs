using System.Text.Json.Serialization;

namespace AIPal.API.Models
{
    /// <summary>
    /// Data transfer object for chat requests coming from external applications.
    /// </summary>
    public class ChatRequestDto
    {
        /// <summary>
        /// Gets or sets the user's message or query.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to attempt command parsing.
        /// If true, the system will try to interpret the message as a command.
        /// If false, the message will only be treated as a conversation.
        /// </summary>
        [JsonPropertyName("parseAsCommand")]
        public bool ParseAsCommand { get; set; } = true;
    }
}
