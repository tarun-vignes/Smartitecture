using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;

namespace AIPal.Commands
{
    public class OpenSettingsCommand : ISystemCommand
    {
        public string CommandName => "OpenSettings";
        public string Description => "Opens Windows Settings";
        public bool RequiresElevation => false;

        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
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
