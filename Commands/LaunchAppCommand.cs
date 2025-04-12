using System;
using System.Threading.Tasks;
using Windows.System;

namespace AIPal.Commands
{
    public class LaunchAppCommand : ISystemCommand
    {
        public string CommandName => "LaunchApp";
        public string Description => "Launches a Windows application";
        public bool RequiresElevation => false;

        public async Task<bool> ExecuteAsync(string[] parameters)
        {
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
