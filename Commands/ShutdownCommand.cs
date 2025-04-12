using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AIPal.Commands
{
    public class ShutdownCommand : ISystemCommand
    {
        public string CommandName => "Shutdown";
        public string Description => "Shuts down the computer";
        public bool RequiresElevation => true;

        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                var timeout = parameters.Length > 0 ? parameters[0] : "60";
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
