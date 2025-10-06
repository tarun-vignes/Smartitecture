using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Smartitecture.Application.Models;

namespace Smartitecture.Application.Services
{
    /// <summary>
    /// Manages the registration of tools that the agent can use.
    /// </summary>
    public class AgentTools
    {
        private readonly ILogger<AgentTools> _logger;
        private readonly IEnumerable<ICommand> _commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTools"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="commands">The collection of available commands.</param>
        public AgentTools(ILogger<AgentTools> logger, IEnumerable<ICommand> commands)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        /// <summary>
        /// Registers all available tools with the agent service.
        /// </summary>
        /// <param name="agentService">The agent service to register tools with.</param>
        public void RegisterTools(IAgentService agentService)
        {
            if (agentService == null)
            {
                throw new ArgumentNullException(nameof(agentService));
            }

            try
            {
                // Register each command as a tool
                foreach (var command in _commands)
                {
                    var tool = CreateToolFromCommand(command);
                    if (tool != null)
                    {
                        agentService.RegisterTool(tool);
                        _logger.LogInformation("Registered tool: {ToolName}", tool.Name);
                    }
                }

                _logger.LogInformation("Successfully registered {Count} tools", ToolCount());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering tools with the agent service");
                throw;
            }
        }

        /// <summary>
        /// Gets the number of available tools.
        /// </summary>
        /// <returns>The number of available tools.</returns>
        public int ToolCount()
        {
            int count = 0;
            foreach (var _ in _commands)
            {
                count++;
            }
            return count;
        }

        private AgentTool CreateToolFromCommand(ICommand command)
        {
            if (command == null)
            {
                return null;
            }

            var tool = new AgentTool
            {
                Name = command.GetType().Name,
                Description = GetCommandDescription(command)
            };

            // Add parameters based on the command type
            var parameters = GetCommandParameters(command);
            if (parameters != null)
            {
                tool.Parameters = parameters;
            }

            return tool;
        }

        private string GetCommandDescription(ICommand command)
        {
            // This is a simplified example. In a real application, you might use attributes
            // or a more sophisticated way to get the command description.
            return $"Executes the {command.GetType().Name} command";
        }

        private List<ToolParameter> GetCommandParameters(ICommand command)
        {
            // This is a simplified example. In a real application, you would inspect
            // the command's properties and create appropriate parameters.
            return new List<ToolParameter>();
        }
    }
}
