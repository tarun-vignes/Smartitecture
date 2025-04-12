using System.Threading.Tasks;

namespace AIPal.Services
{
    /// <summary>
    /// Interface for Language Learning Model (LLM) services.
    /// Provides methods for natural language processing and command parsing.
    /// Implementations can use different LLM providers (e.g., Azure OpenAI, OpenAI, etc.)
    /// </summary>
    public interface ILLMService
    {
        /// <summary>
        /// Gets a conversational response from the LLM based on user input.
        /// Used for general conversation and responses when no specific command is detected.
        /// </summary>
        /// <param name="userInput">The user's message or query</param>
        /// <returns>The LLM's response as a string</returns>
        Task<string> GetResponseAsync(string userInput);

        /// <summary>
        /// Attempts to parse the user input into a system command and parameters.
        /// Uses the LLM to understand natural language and map it to specific commands.
        /// </summary>
        /// <param name="userInput">The user's message or command</param>
        /// <returns>
        /// A tuple containing:
        /// - commandName: The name of the detected command, or "Unknown" if no command is detected
        /// - parameters: Array of parameters for the command, or empty array if no parameters
        /// </returns>
        Task<(string commandName, string[] parameters)> ParseCommandAsync(string userInput);
    }
}
