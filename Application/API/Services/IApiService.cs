using System.Threading.Tasks;
using AIPal.API.Models;

namespace AIPal.API.Services
{
    /// <summary>
    /// Interface for the API service that handles chat interactions.
    /// Provides a higher-level abstraction over the LLM service and command execution.
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// Processes a chat request, handling both conversation and command execution.
        /// </summary>
        /// <param name="request">The chat request containing the message and processing options</param>
        /// <returns>A response with the AI's reply and command execution details if applicable</returns>
        Task<ChatResponseDto> ProcessChatRequestAsync(ChatRequestDto request);
    }
}
