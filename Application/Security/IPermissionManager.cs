using System.Threading.Tasks;

namespace AIPal.Application.Security
{
    /// <summary>
    /// Interface for the permission manager to allow for dependency injection and mocking in tests
    /// </summary>
    public interface IPermissionManager
    {
        /// <summary>
        /// Checks if the current process is running with administrative privileges
        /// </summary>
        /// <returns>True if the process is elevated, false otherwise</returns>
        bool IsElevated();

        /// <summary>
        /// Requests a specific Windows capability permission from the user
        /// </summary>
        /// <param name="capability">The capability identifier to request access for</param>
        /// <returns>True if permission was granted, false otherwise</returns>
        Task<bool> RequestPermissionAsync(string capability);

        /// <summary>
        /// Validates a command string for security concerns and proper formatting
        /// </summary>
        /// <param name="command">The command string to validate</param>
        /// <returns>True if the command is valid and safe, false otherwise</returns>
        bool ValidateCommand(string command);
    }
}
