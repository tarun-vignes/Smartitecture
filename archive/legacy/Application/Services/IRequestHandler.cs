using System.Threading.Tasks;

namespace Smartitecture.Application.Services
{
    /// <summary>
    /// Defines the interface for request handlers in the application.
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        /// Gets a value indicating whether this handler can process the specified request.
        /// </summary>
        /// <param name="request">The request to check.</param>
        /// <returns>True if this handler can process the request; otherwise, false.</returns>
        bool CanHandle(string request);
        
        /// <summary>
        /// Processes the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <returns>The response to the request.</returns>
        Task<string> HandleAsync(string request);
    }
    
    /// <summary>
    /// Defines the interface for typed request handlers in the application.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IRequestHandler<TRequest, TResponse>
    {
        /// <summary>
        /// Gets a value indicating whether this handler can process the specified request.
        /// </summary>
        /// <param name="request">The request to check.</param>
        /// <returns>True if this handler can process the request; otherwise, false.</returns>
        bool CanHandle(TRequest request);
        
        /// <summary>
        /// Processes the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <returns>The response to the request.</returns>
        Task<TResponse> HandleAsync(TRequest request);
    }
}
