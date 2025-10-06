using System.Text.Json.Serialization;

namespace Smartitecture.API.Models
{
    /// <summary>
    /// Data transfer object for chat responses sent to external applications.
    /// </summary>
    public class ChatResponseDto
    {
        /// <summary>
        /// Gets or sets the AI assistant's response message.
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a command was executed.
        /// </summary>
        [JsonPropertyName("commandExecuted")]
        public bool CommandExecuted { get; set; }

        /// <summary>
        /// Gets or sets the name of the command that was executed, if any.
        /// </summary>
        [JsonPropertyName("commandName")]
        public string CommandName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command execution was successful.
        /// </summary>
        [JsonPropertyName("commandSuccess")]
        public bool CommandSuccess { get; set; }
    }
}
