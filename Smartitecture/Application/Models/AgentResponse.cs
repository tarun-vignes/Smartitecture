using System.Collections.Generic;

namespace Smartitecture.Application.Models
{
    /// <summary>
    /// Represents a response from an AI agent, including any actions taken.
    /// </summary>
    public class AgentResponse
    {
        /// <summary>
        /// Gets or sets the text response from the agent.
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets the list of actions taken by the agent during processing.
        /// </summary>
        public List<AgentAction> Actions { get; set; } = new List<AgentAction>();

        /// <summary>
        /// Gets or sets a value indicating whether the agent successfully completed the task.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets any error message if the agent encountered an error.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the context ID for tracking the conversation.
        /// </summary>
        public string ContextId { get; set; }
    }
}
