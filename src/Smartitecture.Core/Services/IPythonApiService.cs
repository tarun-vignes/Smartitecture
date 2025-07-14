using System.Threading.Tasks;
using Smartitecture.Core.Models;

namespace Smartitecture.Core.Services
{
    public interface IPythonApiService
    {
        /// <summary>
        /// Gets the current state of the agent
        /// </summary>
        Task<AgentStateResponse> GetAgentStateAsync();

        /// <summary>
        /// Runs the agent with the given input
        /// </summary>
        /// <param name="input">The input to process</param>
        /// <param name="maxIterations">Maximum number of iterations (optional)</param>
        Task<AgentRunResponse> RunAgentAsync(string input, int? maxIterations = null);
        
        /// <summary>
        /// Checks if the Python API is healthy
        /// </summary>
        Task<bool> CheckHealthAsync();
    }
}
