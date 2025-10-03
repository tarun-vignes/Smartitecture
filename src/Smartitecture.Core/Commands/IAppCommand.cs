using System.Threading.Tasks;

namespace Smartitecture.Core.Commands
{
    /// <summary>
    /// Defines a command that can be executed asynchronously.
    /// </summary>
    public interface IAppCommand
    {
        /// <summary>Gets the name of the command</summary>
        string CommandName { get; }

        /// <summary>Gets the description of what the command does</summary>
        string Description { get; }

        /// <summary>Indicates whether the command requires administrative elevation</summary>
        bool RequiresElevation { get; }

        /// <summary>
        /// Executes the command with the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the command.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the command executed successfully.</returns>
        Task<bool> ExecuteAsync(string[] parameters);
    }
}
