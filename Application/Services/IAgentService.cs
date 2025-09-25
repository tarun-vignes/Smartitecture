// MODIFIED: Rebranded from AIPal to Smartitecture
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smartitecture.Services
{
    /// <summary>
    /// Interface for AI Agent services that extend basic LLM capabilities.
    /// Provides methods for tool use, multi-step reasoning, and autonomous task completion.
    /// </summary>
    public interface IAgentService
    {
        /// <summary>
        /// Processes a user request through an agent that can use tools and perform multi-step reasoning.
        /// </summary>
        /// <param name="userInput">The user's message or query</param>
        /// <param name="contextId">Optional context ID for maintaining conversation history</param>
        /// <returns>The agent's response including any actions taken</returns>
        Task<AgentResponse> ProcessRequestAsync(string userInput, string contextId = null);

        /// <summary>
        /// Registers a tool that the agent can use to perform actions.
        /// </summary>
        /// <param name="tool">The tool to register</param>
        void RegisterTool(AgentTool tool);

        /// <summary>
        /// Gets all available tools that the agent can use.
        /// </summary>
        /// <returns>A list of available tools</returns>
        IReadOnlyList<AgentTool> GetAvailableTools();
    }

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
        /// Gets or sets any error message if the agent encountered an issue.
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents an action taken by an agent using a tool.
    /// </summary>
    public class AgentAction
    {
        /// <summary>
        /// Gets or sets the name of the tool used.
        /// </summary>
        public string ToolName { get; set; }

        /// <summary>
        /// Gets or sets the input provided to the tool.
        /// </summary>
        public Dictionary<string, object> Input { get; set; }

        /// <summary>
        /// Gets or sets the output received from the tool.
        /// </summary>
        public object Output { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tool execution was successful.
        /// </summary>
        public bool Success { get; set; }
    }

    /// <summary>
    /// Represents a tool that an agent can use to perform actions.
    /// </summary>
    public class AgentTool
    {
        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of what the tool does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parameters that the tool accepts.
        /// </summary>
        public Dictionary<string, ToolParameter> Parameters { get; set; } = new Dictionary<string, ToolParameter>();

        /// <summary>
        /// Gets or sets the function that executes the tool's action.
        /// </summary>
        public Func<Dictionary<string, object>, Task<object>> Execute { get; set; }
    }

    /// <summary>
    /// Represents a parameter for an agent tool.
    /// </summary>
    public class ToolParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter (e.g., "string", "number", "boolean").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is required.
        /// </summary>
        public bool Required { get; set; }
    }
}
