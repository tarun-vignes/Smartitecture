using System;
using System.Threading.Tasks;
using Windows.System;
using Smartitecture.Commands;

namespace Smartitecture.Application.Commands
{
    /// <summary>
    /// Command implementation for launching Windows applications.
    /// Supports both Microsoft Store apps and traditional desktop applications.
    /// </summary>
    public class LaunchAppCommand : ICommand<string[], bool>
    {
        /// <summary>Gets the name of the command</summary>
        public static string CommandName => "LaunchApp";

        /// <summary>Gets the description of what the command does</summary>
        public static string Description => "Launches a Windows application";

        /// <summary>Indicates whether the command requires administrative elevation</summary>
        public static bool RequiresElevation => false;

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
                
                // Try to launch as a store app first
                try
                {
                    var options = new LauncherOptions
                    {
                        TargetApplicationPackageFamilyName = appName
                    };
                    await Launcher.LaunchUriAsync(new Uri("ms-store:"), options);
                    return true;
                }
                catch
                {
                    // If store app launch fails, try as a desktop app
                    var options = new LauncherOptions
                    {
                        TreatAsUntrusted = false
                    };
                    await Launcher.LaunchUriAsync(new Uri($"shell:AppsFolder\\{appName}"), options);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
