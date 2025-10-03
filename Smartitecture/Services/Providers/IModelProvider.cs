using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smartitecture.Services.Models;

namespace Smartitecture.Services.Providers
{
    /// <summary>
    /// Interface for AI model providers (OpenAI, Azure, Anthropic, etc.)
    /// </summary>
    public interface IModelProvider
    {
        /// <summary>
        /// The type/name of this provider
        /// </summary>
        string ProviderType { get; }

        /// <summary>
        /// Whether this provider is available and configured
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Models available from this provider
        /// </summary>
        IEnumerable<string> AvailableModels { get; }

        /// <summary>
        /// Whether this provider supports streaming responses
        /// </summary>
        bool SupportsStreaming { get; }

        /// <summary>
        /// Initialize the provider with configuration
        /// </summary>
        /// <param name="config">AI model configuration</param>
        void Initialize(AIModelConfiguration config);

        /// <summary>
        /// Generate a response using the specified model
        /// </summary>
        /// <param name="modelName">Name of the model to use</param>
        /// <param name="messages">Conversation context</param>
        /// <returns>Generated response</returns>
        Task<string> GenerateResponseAsync(string modelName, List<ConversationMessage> messages);

        /// <summary>
        /// Generate a streaming response using the specified model
        /// </summary>
        /// <param name="modelName">Name of the model to use</param>
        /// <param name="messages">Conversation context</param>
        /// <returns>Async enumerable of response tokens</returns>
        IAsyncEnumerable<string> GenerateStreamingResponseAsync(string modelName, List<ConversationMessage> messages);

        /// <summary>
        /// Test the connection to this provider
        /// </summary>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync();
    }
}
