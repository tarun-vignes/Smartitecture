using System.Threading.Tasks;
using AIPal.API.Models;

namespace AIPal.API.Services
{
    /// <summary>
    /// Interface for the query service that handles structured queries to the AI assistant.
    /// Provides methods for processing different types of queries and maintaining context.
    /// </summary>
    public interface IQueryService
    {
        /// <summary>
        /// Processes a structured query and returns a response.
        /// </summary>
        /// <param name="query">The structured query to process</param>
        /// <returns>A response containing the result of processing the query</returns>
        Task<QueryResponseDto> ProcessQueryAsync(QueryDto query);

        /// <summary>
        /// Converts a natural language text input into a structured query.
        /// </summary>
        /// <param name="text">The natural language input</param>
        /// <param name="contextId">Optional context ID for maintaining conversation context</param>
        /// <returns>A structured query parsed from the natural language input</returns>
        Task<QueryDto> ParseTextToQueryAsync(string text, string contextId = null);

        /// <summary>
        /// Gets suggested queries based on the current context.
        /// </summary>
        /// <param name="contextId">The context ID to get suggestions for</param>
        /// <param name="count">The number of suggestions to return</param>
        /// <returns>A list of suggested queries</returns>
        Task<string[]> GetQuerySuggestionsAsync(string contextId, int count = 3);
    }
}
