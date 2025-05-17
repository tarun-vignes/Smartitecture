using System;
using System.Threading.Tasks;
using AIPal.Application.Network.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIPal.Application.Commands;
using AIPal.Application.Network.Services;
using AIPal.Application.Security.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIPal.Application.Services
{
    /// <summary>
    /// Central service for coordinating application components and processing user requests.
    /// </summary>
    public class ApplicationService
    {
        private readonly HandlerRegistry _handlerRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Func<Task<string>>> _commandMap;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationService"/> class.
        /// </summary>
        /// <param name="handlerRegistry">The handler registry.</param>
        /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
        /// <param name="networkMonitorService">The network monitor service.</param>
        /// <param name="securityMonitorService">The security monitor service.</param>
        public ApplicationService(
            HandlerRegistry handlerRegistry,
            IServiceProvider serviceProvider,
            NetworkMonitorService networkMonitorService,
            SecurityMonitorService securityMonitorService)
        {
            _handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            
            // Initialize command map
            _commandMap = new Dictionary<string, Func<Task<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "check wifi security", () => ExecuteCommandAsync<CheckWiFiSecurityCommand, string>() },
                { "check system security", () => ExecuteCommandAsync<CheckSystemSecurityCommand, string>() }
            };
            
            // Subscribe to network status changes
            networkMonitorService.NetworkStatusChanged += OnNetworkStatusChanged;
            
            // Subscribe to security status changes
            securityMonitorService.SecurityStatusChanged += OnSecurityStatusChanged;
        }
        
        /// <summary>
        /// Event that is raised when a notification needs to be shown to the user.
        /// </summary>
        public event EventHandler<NotificationEventArgs> NotificationReceived;
        
        /// <summary>
        /// Processes a user request and returns a response.
        /// </summary>
        /// <param name="request">The user request.</param>
        /// <returns>The response to the user request.</returns>
        public async Task<string> ProcessRequestAsync(string request)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return "Please provide a request.";
            }
            
            // Check if we have a direct command match
            foreach (var command in _commandMap.Keys)
            {
                if (request.Contains(command, StringComparison.OrdinalIgnoreCase))
                {
                    return await _commandMap[command]();
                }
            }
            
            // Find a handler for the request
            var handler = _handlerRegistry.GetHandlerForRequest(request);
            
            if (handler != null)
            {
                // Process the request with the handler
                return await handler.HandleAsync(request);
            }
            
            // No handler found
            return "I'm sorry, I don't know how to handle that request. You can ask me to check your WiFi security or system security.";
        }
        
        /// <summary>
        /// Processes a typed request and returns a typed response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request to process.</param>
        /// <returns>The response to the request.</returns>
        public async Task<TResponse> ProcessRequestAsync<TRequest, TResponse>(TRequest request)
            where TResponse : new()
        {
            try
            {
                // Process the request using the handler registry
                return await _handlerRegistry.ProcessRequestAsync<TRequest, TResponse>(request);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error processing request: {ex}");
                
                // Return a default response
                return new TResponse();
            }
        }
        
        /// <summary>
        /// Checks the security of the current Wi-Fi connection.
        /// </summary>
        /// <returns>A report on the Wi-Fi security status.</returns>
        public async Task<string> CheckWiFiSecurityAsync()
        {
            try
            {
                // Use the NetworkSecurityTools to check Wi-Fi security
                return "Wi-Fi security check functionality will be implemented here";
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error checking Wi-Fi security: {ex}");
                
                // Return a friendly error message
                return "I'm sorry, I encountered an error while checking your Wi-Fi security. Please try again.";
            }
        }
        
        /// <summary>
        /// Checks the system for malware and security issues.
        /// </summary>
        /// <returns>A report on the system security status.</returns>
        public async Task<string> CheckSystemSecurityAsync()
        {
            try
            {
                // Use the SecurityTools to check system security
                return "System security check functionality will be implemented here";
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error checking system security: {ex}");
                
                // Return a friendly error message
                return "I'm sorry, I encountered an error while checking your system security. Please try again.";
            }
        }
    }
}
