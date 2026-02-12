using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smartitecture.Services
{
    /// <summary>
    /// Interface for Language Model services supporting multiple AI providers
    /// </summary>
    public interface ILLMService
    {
        /// <summary>
        /// Gets a conversational response from the AI model
        /// </summary>
        /// <param name="message">The user's message</param>
        /// <param name="conversationId">Optional conversation ID for context</param>
        /// <returns>The AI's response</returns>
        Task<string> GetResponseAsync(string message, string? conversationId = null);

        /// <summary>
        /// Gets a streaming response from the AI model
        /// </summary>
        /// <param name="message">The user's message</param>
        /// <param name="onTokenReceived">Callback for each token received</param>
        /// <param name="conversationId">Optional conversation ID for context</param>
        /// <returns>The complete response</returns>
        Task<string> GetStreamingResponseAsync(string message, Action<string> onTokenReceived, string? conversationId = null);

        /// <summary>
        /// Parses a message to extract command information
        /// </summary>
        /// <param name="message">The message to parse</param>
        /// <returns>Tuple of command name and parameters</returns>
        Task<(string? commandName, Dictionary<string, object>? parameters)> ParseCommandAsync(string message);

        /// <summary>
        /// Gets the current AI model being used
        /// </summary>
        string CurrentModel { get; }

        /// <summary>
        /// Gets available AI models
        /// </summary>
        IEnumerable<string> AvailableModels { get; }

        /// <summary>
        /// Switches to a different AI model
        /// </summary>
        /// <param name="modelName">The name of the model to switch to</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SwitchModelAsync(string modelName);

        /// <summary>
        /// Gets conversation history for a specific conversation
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        /// <returns>List of messages in the conversation</returns>
        Task<List<ConversationMessage>> GetConversationHistoryAsync(string conversationId);

        /// <summary>
        /// Clears conversation history for a specific conversation
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        Task ClearConversationAsync(string conversationId);

        /// <summary>
        /// Event fired when the AI model is switched
        /// </summary>
        event EventHandler<ModelSwitchedEventArgs>? ModelSwitched;
    }

    /// <summary>
    /// Represents a message in a conversation
    /// </summary>
    public class ConversationMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Role { get; set; } = ""; // "user" or "assistant"
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Model { get; set; } = "";
    }

    /// <summary>
    /// Event arguments for model switching
    /// </summary>
    public class ModelSwitchedEventArgs : EventArgs
    {
        public string PreviousModel { get; set; } = "";
        public string NewModel { get; set; } = "";
        public DateTime SwitchTime { get; set; } = DateTime.UtcNow;
    }
}
