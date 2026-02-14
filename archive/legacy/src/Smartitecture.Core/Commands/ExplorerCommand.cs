using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Core.Commands
{
    /// <summary>
    /// Command to launch Windows File Explorer
    /// </summary>
    public class ExplorerCommand : IAppCommand
    {
        public string CommandName => "Explorer";
        public string Description => "Opens Windows File Explorer";
        public bool RequiresElevation => false;

        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                string path = parameters.Length > 0 ? parameters[0] : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = path,
                    UseShellExecute = true
                });
                
                await Task.Delay(100);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
