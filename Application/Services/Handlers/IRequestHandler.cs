using System.Threading.Tasks;

namespace Smartitecture.Services.Handlers
{
    /// <summary>
    /// Interface for request handlers that process specific types of requests.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle</typeparam>
    /// <typeparam name="TResponse">The type of response to return</typeparam>
    public interface IRequestHandler<TRequest, TResponse>
    {
        /// <summary>
        /// Determines whether this handler can handle the specified request.
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>True if this handler can handle the request, false otherwise</returns>
        bool CanHandle(TRequest request);

        /// <summary>
        /// Handles the specified request and returns a response.
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <returns>The response</returns>
        Task<TResponse> HandleAsync(TRequest request);
    }
}
