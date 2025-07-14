using System.Threading;
using System.Threading.Tasks;

namespace Smartitecture.Core.Services
{
    /// <summary>
    /// Defines a service for managing the Python backend process.
    /// </summary>
    public interface IPythonBackendService
    {
        /// <summary>
        /// Starts the Python backend process if it's not already running.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Stops the Python backend process if it's running.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task StopAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Restarts the Python backend process.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RestartAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a value indicating whether the Python backend process is running.
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Gets the process ID of the Python backend if it's running; otherwise, null.
        /// </summary>
        int? ProcessId { get; }
    }
}
