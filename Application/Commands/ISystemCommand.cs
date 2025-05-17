using System.Threading.Tasks;

namespace AIPal.Application.Commands
{
    /// <summary>
    /// Defines the contract for all system commands in AIPal.
    /// Each command represents a specific system operation that can be executed.
    /// </summary>
    public interface ISystemCommand
    {
        /// <summary>
        /// Gets the unique name of the command used for identification and invocation.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets whether the command requires administrative privileges to execute.
        /// </summary>
        bool RequiresElevation { get; }

        /// <summary>
        /// Executes the command with the provided parameters.
        /// </summary>
        /// <param name="parameters">Array of string parameters for the command execution.</param>
        /// <returns>True if the command executed successfully; otherwise, false.</returns>
        Task<bool> ExecuteAsync(string[] parameters);
    }
}
