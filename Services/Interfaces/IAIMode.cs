using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Smartitecture.Services.Interfaces
{
    /// <summary>
    /// Base interface for all Smartitecture AI modes (LUMEN, FORTIS, NEXA)
    /// </summary>
    public interface IAIMode
    {
        /// <summary>
        /// The name of the AI mode (LUMEN, FORTIS, or NEXA)
        /// </summary>
        string ModeName { get; }

        /// <summary>
        /// The display icon for the mode
        /// </summary>
        string ModeIcon { get; }

        /// <summary>
        /// The primary color associated with this mode
        /// </summary>
        string ModeColor { get; }

        /// <summary>
        /// Description of the mode's capabilities
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Determines if this mode can handle the given query
        /// </summary>
        /// <param name="query">The user's query</param>
        /// <returns>True if this mode should handle the query</returns>
        bool CanHandle(string query);

        /// <summary>
        /// Processes a query and returns a response
        /// </summary>
        /// <param name="query">The user's query</param>
        /// <param name="conversationId">Optional conversation ID for context</param>
        /// <returns>The AI response</returns>
        Task<string> ProcessQueryAsync(string query, string conversationId = null);

        /// <summary>
        /// Initializes the AI mode with necessary services and configurations
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Gets the confidence level for handling a specific query (0.0 to 1.0)
        /// </summary>
        /// <param name="query">The user's query</param>
        /// <returns>Confidence score</returns>
        double GetConfidenceScore(string query);

        /// <summary>
        /// Provides context analysis for collaboration with other modes
        /// </summary>
        /// <param name="query">The user's query</param>
        /// <returns>Context information for other modes</returns>
        Task<AIContext> AnalyzeContextAsync(string query);
    }

    /// <summary>
    /// Context information shared between AI modes
    /// </summary>
    public class AIContext
    {
        public string Query { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<string> DetectedIntents { get; set; } = new();
        public bool RequiresSecurityAnalysis { get; set; }
        public bool RequiresPerformanceAnalysis { get; set; }
        public bool RequiresSystemAccess { get; set; }
        public double ConfidenceScore { get; set; }
        public string PrimaryMode { get; set; }
        public List<string> CollaboratingModes { get; set; } = new();
    }

    /// <summary>
    /// Response from an AI mode that may include collaboration data
    /// </summary>
    public class AIResponse
    {
        public string Content { get; set; }
        public string SourceMode { get; set; }
        public List<string> CollaboratingModes { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool RequiresUserConfirmation { get; set; }
        public string ConfirmationMessage { get; set; }
    }
}
