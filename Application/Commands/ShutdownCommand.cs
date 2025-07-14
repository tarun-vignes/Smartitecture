using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AIPal.Application.Commands
{
    /// <summary>
    /// Command implementation for shutting down the Windows system.
    /// Requires administrative privileges to execute.
    /// </summary>
        /// <summary>Gets the name of the command</summary>
        public string CommandName => "Shutdown";

        /// <summary>Gets the description of what the command does</summary>
        public string Description => "Shuts down the computer";

        /// <summary>Indicates whether the command requires administrative elevation</summary>
        public bool RequiresElevation => true;

        /// <summary>
        /// Executes the shutdown command with the specified parameters
        /// </summary>
        /// <param name="parameters">Optional parameters array where first parameter is timeout in seconds (defaults to 60)</param>
        /// <returns>True if shutdown command was initiated successfully, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                // Get timeout parameter or use default 60 seconds
                var timeout = parameters.Length > 0 ? parameters[0] : "60";
                // Configure the shutdown process with administrative privileges
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = $"/s /t {timeout}",
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };

                // Initiate the shutdown command
                process.Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
