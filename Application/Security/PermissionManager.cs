using System;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Authorization.AppCapabilityAccess;

namespace Smartitecture.Security
{
    /// <summary>
    /// Manages security permissions and access control for the Smartitecture application.
    /// Handles UAC elevation checks, capability requests, and command validation.
    /// </summary>
    public class PermissionManager : IPermissionManager
    {
        /// <summary>
        /// Checks if the current process is running with administrative privileges
        /// </summary>
        /// <returns>True if the process is elevated, false otherwise</returns>
        public bool IsElevated()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Requests a specific Windows capability permission from the user
        /// </summary>
        /// <param name="capability">The capability identifier to request access for</param>
        /// <returns>True if permission was granted, false otherwise</returns>
        public async Task<bool> RequestPermissionAsync(string capability)
        {
            try
            {
                var status = await AppCapability.Create(capability).RequestAccessAsync();
                return status == AppCapabilityAccessStatus.Allowed;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a command string for security concerns and proper formatting
        /// </summary>
        /// <param name="command">The command string to validate</param>
        /// <returns>True if the command is valid and safe, false otherwise</returns>
        public bool ValidateCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            // Check for potentially dangerous patterns
            string[] dangerousPatterns = new[]
            {
                @"rm -rf",                // Recursive delete
                @"format[\s]+[a-zA-Z]:", // Format drive
                @"del[\s]+/[a-zA-Z]",    // Delete with switches
                @"rd[\s]+/[a-zA-Z]",     // Remove directory with switches
                @"shutdown",              // System shutdown (handled by ShutdownCommand)
                @"taskkill",             // Kill processes
                @"net user",              // User management
                @"net localgroup",        // Group management
                @"reg delete",            // Registry deletion
                @"reg add",               // Registry addition
                @"powershell -e",         // Encoded PowerShell commands
                @"cmd.exe",               // Direct cmd execution
                @"wscript",               // Windows Script Host
                @"cscript"                // Windows Script Host
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(command, pattern, RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }

            // Validate command length
            if (command.Length > 500)
            {
                return false;
            }

            // Check for injection attempts
            if (command.Contains(";") || command.Contains("|") || command.Contains("&"))
            {
                return false;
            }

            return true;
        }
    }
}
