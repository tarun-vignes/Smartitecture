using System;
using System.Linq;
using System.Threading.Tasks;
using AIPal.API.Models;
using Microsoft.Extensions.Logging;

namespace AIPal.Services.Handlers
{
    /// <summary>
    /// Base class for agent request handlers.
    /// </summary>
    public abstract class AgentRequestHandlerBase : IRequestHandler<AgentRequestDto, AgentResponse>
    {
        protected readonly ILogger _logger;
        protected readonly IAgentService _agentService;

        /// <summary>
        /// Initializes a new instance of the AgentRequestHandlerBase class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        /// <param name="logger">The logger</param>
        protected AgentRequestHandlerBase(IAgentService agentService, ILogger logger)
        {
            _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        public abstract bool CanHandle(AgentRequestDto request);

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        public abstract Task<AgentResponse> HandleAsync(AgentRequestDto request);
    }

    /// <summary>
    /// Handler for system information requests.
    /// </summary>
    public class SystemInfoRequestHandler : AgentRequestHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the SystemInfoRequestHandler class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        /// <param name="logger">The logger</param>
        public SystemInfoRequestHandler(IAgentService agentService, ILogger<SystemInfoRequestHandler> logger)
            : base(agentService, logger)
        {
        }

        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        public override bool CanHandle(AgentRequestDto request)
        {
            // Check if the request is about system information
            var message = request.Message.ToLowerInvariant();
            return message.Contains("system info") || 
                   message.Contains("system information") || 
                   message.Contains("computer info") ||
                   message.Contains("pc info") ||
                   message.Contains("about my computer") ||
                   message.Contains("about my system") ||
                   message.Contains("system specs") ||
                   message.Contains("hardware info");
        }

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        public override async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            _logger.LogInformation("Handling system info request");
            
            // Process the request through the agent service
            // The agent will use the GetSystemInfo tool
            return await _agentService.ProcessRequestAsync(request.Message, request.ContextId);
        }
    }

    /// <summary>
    /// Handler for file search requests.
    /// </summary>
    public class FileSearchRequestHandler : AgentRequestHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the FileSearchRequestHandler class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        /// <param name="logger">The logger</param>
        public FileSearchRequestHandler(IAgentService agentService, ILogger<FileSearchRequestHandler> logger)
            : base(agentService, logger)
        {
        }

        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        public override bool CanHandle(AgentRequestDto request)
        {
            // Check if the request is about searching files
            var message = request.Message.ToLowerInvariant();
            return message.Contains("find file") || 
                   message.Contains("search file") || 
                   message.Contains("locate file") ||
                   message.Contains("find files") ||
                   message.Contains("search files") ||
                   message.Contains("find documents") ||
                   message.Contains("search for files") ||
                   message.Contains("look for files");
        }

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        public override async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            _logger.LogInformation("Handling file search request");
            
            // Process the request through the agent service
            // The agent will use the SearchFiles tool
            return await _agentService.ProcessRequestAsync(request.Message, request.ContextId);
        }
    }

    /// <summary>
    /// Handler for application launch requests.
    /// </summary>
    public class LaunchAppRequestHandler : AgentRequestHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the LaunchAppRequestHandler class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        /// <param name="logger">The logger</param>
        public LaunchAppRequestHandler(IAgentService agentService, ILogger<LaunchAppRequestHandler> logger)
            : base(agentService, logger)
        {
        }

        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        public override bool CanHandle(AgentRequestDto request)
        {
            // Check if the request is about launching an application
            var message = request.Message.ToLowerInvariant();
            return message.Contains("launch ") || 
                   message.Contains("open ") || 
                   message.Contains("start ") ||
                   message.Contains("run ") ||
                   message.StartsWith("execute ");
        }

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        public override async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            _logger.LogInformation("Handling application launch request");
            
            // Process the request through the agent service
            // The agent will use the LaunchApplication tool
            return await _agentService.ProcessRequestAsync(request.Message, request.ContextId);
        }
    }

    /// <summary>
    /// Handler for web search requests.
    /// </summary>
    public class WebSearchRequestHandler : AgentRequestHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the WebSearchRequestHandler class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        /// <param name="logger">The logger</param>
        public WebSearchRequestHandler(IAgentService agentService, ILogger<WebSearchRequestHandler> logger)
            : base(agentService, logger)
        {
        }

        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        public override bool CanHandle(AgentRequestDto request)
        {
            // Check if the request is about searching the web
            var message = request.Message.ToLowerInvariant();
            return message.Contains("search web") || 
                   message.Contains("search the web") || 
                   message.Contains("search online") ||
                   message.Contains("web search") ||
                   message.Contains("find online") ||
                   message.Contains("look up online") ||
                   message.Contains("google ") ||
                   message.Contains("search for information");
        }

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        public override async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            _logger.LogInformation("Handling web search request");
            
            // Process the request through the agent service
            // The agent will use the SearchWeb tool
            return await _agentService.ProcessRequestAsync(request.Message, request.ContextId);
        }
    }

    /// <summary>
    /// Handler for weather information requests.
    /// </summary>
    public class WeatherRequestHandler : AgentRequestHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the WeatherRequestHandler class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        /// <param name="logger">The logger</param>
        public WeatherRequestHandler(IAgentService agentService, ILogger<WeatherRequestHandler> logger)
            : base(agentService, logger)
        {
        }

        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        public override bool CanHandle(AgentRequestDto request)
        {
            // Check if the request is about weather information
            var message = request.Message.ToLowerInvariant();
            return message.Contains("weather") || 
                   message.Contains("temperature") || 
                   message.Contains("forecast") ||
                   message.Contains("is it raining") ||
                   message.Contains("is it sunny") ||
                   message.Contains("how hot") ||
                   message.Contains("how cold") ||
                   message.Contains("what's the temperature");
        }

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        public override async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            _logger.LogInformation("Handling weather request");
            
            // Process the request through the agent service
            // The agent will use the GetWeatherInfo tool
            return await _agentService.ProcessRequestAsync(request.Message, request.ContextId);
        }
    }

    /// <summary>
    /// Default handler for general requests that don't match other handlers.
    /// </summary>
    public class DefaultRequestHandler : AgentRequestHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the DefaultRequestHandler class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        /// <param name="logger">The logger</param>
        public DefaultRequestHandler(IAgentService agentService, ILogger<DefaultRequestHandler> logger)
            : base(agentService, logger)
        {
        }

        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        public override bool CanHandle(AgentRequestDto request)
        {
            // This is the fallback handler, so it can handle any request
            return true;
        }

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        public override async Task<AgentResponse> HandleAsync(AgentRequestDto request)
        {
            _logger.LogInformation("Handling general request with default handler");
            
            // Process the request through the agent service
            // The agent will determine the best approach
            return await _agentService.ProcessRequestAsync(request.Message, request.ContextId);
        }
    }
}
