using System;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace AIPal.Application.Commands
{
    /// <summary>
    /// Command for controlling the system volume.
    /// Supports operations: up, down, mute, and setting specific volume levels.
    /// </summary>
    /// <example>
    /// Usage examples:
    /// - Volume up
    /// - Volume down
    /// - Volume mute
    /// - Volume 50 (sets volume to 50%)
    /// </example>
    public class VolumeCommand : ISystemCommand
    {
        /// <summary>
        /// Gets the name of the command: "Volume"
        /// </summary>
        public string CommandName => "Volume";

        /// <summary>
        /// Gets the description of the command's functionality
        /// </summary>
        public string Description => "Controls system volume";

        /// <summary>
        /// Volume control does not require elevation
        /// </summary>
        public bool RequiresElevation => false;

        /// <summary>
        /// Executes volume control commands based on the provided parameters.
        /// </summary>
        /// <param name="parameters">Array containing the volume command (up/down/mute/number)</param>
        /// <returns>True if the command was executed successfully; otherwise, false.</returns>
        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                // Get the system media controls session
                var session = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                
                if (parameters.Length == 0)
                    return false;

                // Parse the command (up/down/mute/volume level)
                var command = parameters[0].ToLower();
                switch (command)
                {
                    case "up":
                        await session.TryChangeVolumeAsync(0.1, GlobalSystemMediaTransportControlsVolumeChangeRequestedEventArgs.VolumeChangeType.Relative);
                        break;
                    case "down":
                        await session.TryChangeVolumeAsync(-0.1, GlobalSystemMediaTransportControlsVolumeChangeRequestedEventArgs.VolumeChangeType.Relative);
                        break;
                    case "mute":
                        await session.TryToggleMuteAsync();
                        break;
                    default:
                        if (double.TryParse(command, out var level) && level >= 0 && level <= 100)
                        {
                            await session.TryChangeVolumeAsync(level / 100.0, GlobalSystemMediaTransportControlsVolumeChangeRequestedEventArgs.VolumeChangeType.Absolute);
                        }
                        break;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
