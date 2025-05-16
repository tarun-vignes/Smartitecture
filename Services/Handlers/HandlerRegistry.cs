using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIPal.Services.Handlers
{
    /// <summary>
    /// Registry for request handlers that manages handler registration and dispatch.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle</typeparam>
    /// <typeparam name="TResponse">The type of response to return</typeparam>
    public class HandlerRegistry<TRequest, TResponse>
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<Type> _handlerTypes = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the HandlerRegistry class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving handlers</param>
        /// <param name="logger">The logger</param>
        public HandlerRegistry(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a handler type with the registry.
        /// </summary>
        /// <typeparam name="THandler">The type of handler to register</typeparam>
        public void RegisterHandler<THandler>() where THandler : IRequestHandler<TRequest, TResponse>
        {
            var handlerType = typeof(THandler);
            
            if (!_handlerTypes.Contains(handlerType))
            {
                _handlerTypes.Add(handlerType);
                _logger.LogInformation("Registered handler: {HandlerType}", handlerType.Name);
            }
        }

        /// <summary>
        /// Handles a request by finding an appropriate handler and dispatching to it.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response from the handler</returns>
        public async Task<TResponse> HandleRequestAsync(TRequest request)
        {
            // Get all registered handlers from the service provider
            var handlers = _handlerTypes
                .Select(type => (IRequestHandler<TRequest, TResponse>)_serviceProvider.GetService(type))
                .Where(handler => handler != null)
                .ToList();

            // Find the first handler that can handle this request
            var handler = handlers.FirstOrDefault(h => h.CanHandle(request));

            if (handler == null)
            {
                _logger.LogWarning("No handler found for request: {RequestType}", request.GetType().Name);
                throw new InvalidOperationException($"No handler found for request: {request.GetType().Name}");
            }

            _logger.LogInformation("Handling request with: {HandlerType}", handler.GetType().Name);
            return await handler.HandleAsync(request);
        }

        /// <summary>
        /// Gets all registered handler types.
        /// </summary>
        /// <returns>The list of registered handler types</returns>
        public IReadOnlyList<Type> GetRegisteredHandlerTypes()
        {
            return _handlerTypes.AsReadOnly();
        }
    }
}
