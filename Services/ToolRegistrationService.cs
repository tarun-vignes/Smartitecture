using System;
using System.Collections.Generic;
using AIPal.Services.AgentTools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIPal.Services
{
    /// <summary>
    /// Service for registering and managing tools for the AI agent.
    /// </summary>
    public class ToolRegistrationService
    {
        private readonly ILogger<ToolRegistrationService> _logger;
        private readonly IOptions<AgentOptions> _options;

        /// <summary>
        /// Initializes a new instance of the ToolRegistrationService class.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="options">The agent options</param>
        public ToolRegistrationService(
            ILogger<ToolRegistrationService> logger,
            IOptions<AgentOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Registers all available tools with the agent service.
        /// </summary>
        /// <param name="agentService">The agent service to register tools with</param>
        public void RegisterTools(IAgentService agentService)
        {
            if (agentService == null)
            {
                throw new ArgumentNullException(nameof(agentService));
            }

            _logger.LogInformation("Registering tools with agent service");

            // Register system tools
            RegisterSystemTools(agentService);

            // Register web tools if API keys are available
            RegisterWebTools(agentService);

            _logger.LogInformation("Finished registering tools with agent service");
        }

        /// <summary>
        /// Registers system-related tools with the agent service.
        /// </summary>
        /// <param name="agentService">The agent service to register tools with</param>
        private void RegisterSystemTools(IAgentService agentService)
        {
            _logger.LogInformation("Registering system tools");

            // Register system information tool
            agentService.RegisterTool(SystemTools.CreateSystemInfoTool());
            _logger.LogInformation("Registered system info tool");

            // Register launch application tool
            agentService.RegisterTool(SystemTools.CreateLaunchAppTool(_logger));
            _logger.LogInformation("Registered launch application tool");

            // Register file search tool
            agentService.RegisterTool(SystemTools.CreateFileSearchTool());
            _logger.LogInformation("Registered file search tool");

            // Register date/time tool
            agentService.RegisterTool(SystemTools.CreateDateTimeTool());
            _logger.LogInformation("Registered date/time tool");
        }

        /// <summary>
        /// Registers web-related tools with the agent service.
        /// </summary>
        /// <param name="agentService">The agent service to register tools with</param>
        private void RegisterWebTools(IAgentService agentService)
        {
            _logger.LogInformation("Registering web tools");

            // Register web fetch tool
            agentService.RegisterTool(WebTools.CreateWebFetchTool(_logger));
            _logger.LogInformation("Registered web fetch tool");

            // Register weather tool if API key is available
            if (!string.IsNullOrEmpty(_options.Value.WeatherApiKey))
            {
                agentService.RegisterTool(WebTools.CreateWeatherTool(_options.Value.WeatherApiKey, _logger));
                _logger.LogInformation("Registered weather tool");
            }
            else
            {
                _logger.LogWarning("Weather API key not provided, weather tool not registered");
            }

            // Register web search tool if API keys are available
            if (!string.IsNullOrEmpty(_options.Value.SearchApiKey) && !string.IsNullOrEmpty(_options.Value.SearchEngineId))
            {
                agentService.RegisterTool(WebTools.CreateWebSearchTool(
                    _options.Value.SearchApiKey,
                    _options.Value.SearchEngineId,
                    _logger));
                _logger.LogInformation("Registered web search tool");
            }
            else
            {
                _logger.LogWarning("Search API key or engine ID not provided, web search tool not registered");
            }
        }
    }

    /// <summary>
    /// Options for configuring the agent.
    /// </summary>
    public class AgentOptions
    {
        /// <summary>
        /// Gets or sets the API key for weather services.
        /// </summary>
        public string WeatherApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API key for search services.
        /// </summary>
        public string SearchApiKey { get; set; }

        /// <summary>
        /// Gets or sets the search engine ID for search services.
        /// </summary>
        public string SearchEngineId { get; set; }
    }
}
