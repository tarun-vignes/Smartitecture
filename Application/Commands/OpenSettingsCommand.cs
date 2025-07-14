using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;

namespace AIPal.Application.Commands
{
    /// <summary>
    /// Command to open the Windows Settings application.
    /// Uses the Windows System Launcher to open the settings URI.
    /// </summary>
    /// <example>
    /// Usage:
    /// - Open settings
    /// - Show system settings
    /// - Launch Windows settings
    /// </example>
    public class OpenSettingsCommand : ISystemCommand
    {
        /// <summary>
        /// Gets the name of the command: "OpenSettings"
        /// </summary>
        public string CommandName => "OpenSettings";

        /// <summary>
        /// Gets the description of what the command does
        /// </summary>
        public string Description => "Opens Windows Settings";

        /// <summary>
        /// Opening settings does not require elevation
        /// </summary>
        public bool RequiresElevation => false;

        /// <summary>
        /// Executes the command to open Windows Settings.
        /// </summary>
        /// <param name="parameters">Not used for this command</param>
        /// <returns>True if Windows Settings was opened successfully; otherwise, false.</returns>
        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                // Launch the Windows Settings app using the ms-settings: protocol
                await Launcher.LaunchUriAsync(new Uri("ms-settings:"));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
