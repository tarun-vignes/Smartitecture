using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Core.Commands
{
    /// <summary>
    /// Command implementation for launching Windows applications.
    /// Supports both Microsoft Store apps and traditional desktop applications.
    /// </summary>
    public class LaunchAppCommand : IAppCommand
    {
        /// <summary>Gets the name of the command</summary>
        public string CommandName => "LaunchApp";

        /// <summary>Gets the description of what the command does</summary>
        public string Description => "Launches a Windows application";

        /// <summary>Indicates whether the command requires administrative elevation</summary>
        public bool RequiresElevation => false;

        /// <summary>
        /// Attempts to launch a Windows application using the provided application name
        /// </summary>
        /// <param name="parameters">Array where first parameter is the application name/identifier</param>
        /// <returns>True if the application was launched successfully, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            // Validate that an application name was provided
            if (parameters.Length == 0)
                return false;

            try
            {
                var appName = parameters[0];
                
                // Launch the application using Process.Start
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = appName,
                        UseShellExecute = true
                    });
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
