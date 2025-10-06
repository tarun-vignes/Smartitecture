# AIPal Handler System Documentation

## Overview

The AIPal Handler System provides a structured approach to processing different types of requests, similar to how web frameworks handle different HTTP methods (GET, POST, PUT, etc.). This system routes incoming requests to specialized handlers based on the request content, allowing for more organized and maintainable code.

## Key Components

### 1. Request Handlers

- **IRequestHandler<TRequest, TResponse>**: The core interface for all handlers
  - `CanHandle(request)`: Determines if this handler can process a given request
  - `HandleAsync(request)`: Processes the request and returns a response

- **AgentRequestHandlerBase**: Base class for agent request handlers
  - Provides common functionality for all agent request handlers
  - Specialized handlers inherit from this base class

### 2. Handler Registry

The `HandlerRegistry<TRequest, TResponse>` manages a collection of handlers:

- **Registration**: Registers handlers with the system
- **Dispatch**: Finds the appropriate handler for a request
- **Execution**: Invokes the handler and returns the response

### 3. Request Processor

The `AgentRequestProcessor` orchestrates the handling process:

- **Initialization**: Sets up the handler registry and registers all handlers
- **Processing**: Routes requests to the appropriate handler
- **Error Handling**: Provides graceful error handling and logging

## Specialized Handlers

The system includes specialized handlers for different types of requests:

- **SystemInfoRequestHandler**: Handles requests for system information
- **FileSearchRequestHandler**: Handles file search requests
- **LaunchAppRequestHandler**: Handles application launch requests
- **WebSearchRequestHandler**: Handles web search requests
- **WeatherRequestHandler**: Handles weather information requests
- **DefaultRequestHandler**: Fallback handler for general requests

## Request Flow

1. **Request Received**: A request comes in through the API controller
2. **Processor Invoked**: The controller passes the request to the processor
3. **Handler Selection**: The processor finds the appropriate handler
4. **Request Handling**: The selected handler processes the request
5. **Response Return**: The handler's response is returned to the client

## Handler Selection Process

Handlers are tried in registration order until one returns `true` from `CanHandle`:

1. Each handler examines the request to determine if it can handle it
2. The first handler that returns `true` from `CanHandle` is selected
3. If no specific handler can handle the request, the default handler is used

## Integration with Agent System

The Handler System works with the Agent System:

- Handlers use the Agent Service to process requests
- Handlers direct the agent to use specific tools based on the request type
- The agent performs the actual work while handlers manage the routing

## Code Example: Creating a Custom Handler

```csharp
public class CustomRequestHandler : AgentRequestHandlerBase
{
    public CustomRequestHandler(IAgentService agentService, ILogger<CustomRequestHandler> logger)
        : base(agentService, logger)
    {
    }

    public override bool CanHandle(AgentRequestDto request)
    {
        // Determine if this handler can handle the request
        var message = request.Message.ToLowerInvariant();
        return message.Contains("custom request") || message.Contains("special task");
    }

    public override async Task<AgentResponse> HandleAsync(AgentRequestDto request)
    {
        _logger.LogInformation("Handling custom request");
        
        // Add any pre-processing logic here
        
        // Process the request through the agent service
        var response = await _agentService.ProcessRequestAsync(request.Message, request.ContextId);
        
        // Add any post-processing logic here
        
        return response;
    }
}
```

## Registering a Custom Handler

Register your custom handler in the `ServiceCollectionExtensions` class:

```csharp
private static void RegisterAgentRequestHandlers(IServiceCollection services)
{
    // Register existing handlers
    services.AddTransient<SystemInfoRequestHandler>();
    services.AddTransient<FileSearchRequestHandler>();
    // ...
    
    // Register your custom handler
    services.AddTransient<CustomRequestHandler>();
    
    // Always register the default handler last
    services.AddTransient<DefaultRequestHandler>();
}
```

## Benefits of the Handler System

1. **Separation of Concerns**: Each handler focuses on a specific type of request
2. **Extensibility**: Easy to add new handlers for new request types
3. **Maintainability**: Changes to one handler don't affect others
4. **Testability**: Handlers can be tested in isolation
5. **Organization**: Code is organized by functionality rather than mixed together

## Best Practices

1. **Handler Order**: Register more specific handlers before general ones
2. **CanHandle Logic**: Keep the logic in `CanHandle` simple and efficient
3. **Default Handler**: Always have a default handler as a fallback
4. **Logging**: Include appropriate logging in handlers for debugging
5. **Error Handling**: Handle exceptions gracefully within handlers

## Future Enhancements

1. **Handler Pipelines**: Chain multiple handlers for complex requests
2. **Handler Priorities**: Assign priorities to handlers for more control
3. **Caching**: Cache handler results for improved performance
4. **Metrics**: Track handler usage and performance metrics
5. **Dynamic Registration**: Register handlers at runtime based on configuration
