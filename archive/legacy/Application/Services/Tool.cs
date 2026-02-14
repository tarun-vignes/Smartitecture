using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smartitecture.Application.Services
{
    /// <summary>
    /// Represents a tool that can be executed by the agent.
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the tool.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the function that executes the tool.
        /// </summary>
        public Func<JsonElement, Task<string>> Execute { get; set; }
    }
    
    /// <summary>
    /// Represents a request to the agent.
    /// </summary>
    public class AgentRequestDto
    {
        /// <summary>
        /// Gets or sets the user input.
        /// </summary>
        public string UserInput { get; set; }
    }
    
    /// <summary>
    /// Represents a response from the agent.
    /// </summary>
    public class AgentResponse
    {
        /// <summary>
        /// Gets or sets the message from the agent.
        /// </summary>
        public string Message { get; set; }
    }
    
    /// <summary>
    /// Represents a tool that can be used by the agent.
    /// </summary>
    public class AgentTool
    {
        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the tool.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the function that executes the tool.
        /// </summary>
        public Func<JsonElement, Task<string>> Function { get; set; }
    }
}
