using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIPal.Application.Network.Handlers;
using AIPal.Application.Security.Handlers;

namespace AIPal.Application.Services
{
    /// <summary>
    /// Registry for all request handlers in the application.
    /// </summary>
    public class HandlerRegistry
    {
        private readonly List<IRequestHandler> _handlers = new List<IRequestHandler>();
        private readonly List<object> _typedHandlers = new List<object>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerRegistry"/> class.
        /// </summary>
        public HandlerRegistry()
        {
            // Register all handlers
            RegisterHandlers();
        }
        
        /// <summary>
        /// Registers all handlers with the registry.
        /// </summary>
        private void RegisterHandlers()
        {
            // Register network security handlers
            _handlers.Add(new NetworkSecurityHandler());
            
            // Register security handlers
            // Note: This is commented out as it requires the AgentRequestDto and AgentResponse types
            // _typedHandlers.Add(new SecurityHandler());
            
            // Register other handlers as needed
        }
        
        /// <summary>
        /// Gets a handler that can process the specified request.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <returns>A handler that can process the request, or null if no handler can process it.</returns>
        public IRequestHandler GetHandler(string request)
        {
            return _handlers.FirstOrDefault(h => h.CanHandle(request));
        }
        
        /// <summary>
        /// Gets a typed handler that can process the specified request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to process.</param>
        /// <returns>A handler that can process the request, or null if no handler can process it.</returns>
        public IRequestHandler<TRequest, TResponse> GetHandler<TRequest, TResponse>(TRequest request)
        {
            return _typedHandlers
                .OfType<IRequestHandler<TRequest, TResponse>>()
                .FirstOrDefault(h => h.CanHandle(request));
        }
        
        /// <summary>
        /// Processes the specified request using the appropriate handler.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <returns>The response from the handler, or a default response if no handler can process the request.</returns>
        public async Task<string> ProcessRequestAsync(string request)
        {
            var handler = GetHandler(request);
            
            if (handler != null)
            {
                return await handler.HandleAsync(request);
            }
            
            // Default response if no handler can process the request
            return "I'm sorry, I don't understand that request. Could you please rephrase it?";
        }
        
        /// <summary>
        /// Processes the specified typed request using the appropriate handler.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to process.</param>
        /// <returns>The response from the handler, or a default response if no handler can process the request.</returns>
        public async Task<TResponse> ProcessRequestAsync<TRequest, TResponse>(TRequest request)
            where TResponse : new()
        {
            var handler = GetHandler<TRequest, TResponse>(request);
            
            if (handler != null)
            {
                return await handler.HandleAsync(request);
            }
            
            // Default response if no handler can process the request
            return default;
        }
    }
}
