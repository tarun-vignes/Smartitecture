using System;
using System.Threading.Tasks;
using AIPal.API.Models;
using Microsoft.Extensions.Logging;

namespace AIPal.Services.Handlers
{
    /// <summary>
    /// Service for processing agent requests using the appropriate handlers.
    /// </summary>
    public class AgentRequestProcessor
    {
        private readonly HandlerRegistry<AgentRequestDto, AgentResponse> _handlerRegistry;
        private readonly ILogger<AgentRequestProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the AgentRequestProcessor class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving handlers</param>
        /// <param name="logger">The logger</param>
        public AgentRequestProcessor(IServiceProvider serviceProvider, ILogger<AgentRequestProcessor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlerRegistry = new HandlerRegistry<AgentRequestDto, AgentResponse>(serviceProvider, logger);
            
            // Register all handlers
            RegisterHandlers();
        }

        /// <summary>
        /// Processes an agent request by finding and using the appropriate handler.
        /// </summary>
        /// <param name="request">The request to process</param>
        /// <returns>The response from the handler</returns>
        public async Task<AgentResponse> ProcessRequestAsync(AgentRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                _logger.LogInformation("Processing agent request: {Message}", request.Message);
                return await _handlerRegistry.HandleRequestAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing agent request");
                
                return new AgentResponse
                {
                    Success = false,
                    ErrorMessage = $"Error processing request: {ex.Message}",
                    Response = "I'm sorry, but I encountered an error while processing your request. Please try again."
                };
            }
        }

        /// <summary>
        /// Registers all available handlers with the registry.
        /// </summary>
        private void RegisterHandlers()
        {
            // Register specific handlers first (order matters for matching)
            _handlerRegistry.RegisterHandler<SystemInfoRequestHandler>();
            _handlerRegistry.RegisterHandler<FileSearchRequestHandler>();
            _handlerRegistry.RegisterHandler<LaunchAppRequestHandler>();
            _handlerRegistry.RegisterHandler<WebSearchRequestHandler>();
            _handlerRegistry.RegisterHandler<WeatherRequestHandler>();
            
            // Register default handler last (as fallback)
            _handlerRegistry.RegisterHandler<DefaultRequestHandler>();
            
            _logger.LogInformation("Registered {Count} handlers", _handlerRegistry.GetRegisteredHandlerTypes().Count);
        }
    }
}
