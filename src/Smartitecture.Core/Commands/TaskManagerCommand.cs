using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Core.Commands
{
    /// <summary>
    /// Command to launch Windows Task Manager
    /// </summary>
    public class TaskManagerCommand : IAppCommand
    {
        public string CommandName => "TaskManager";
        public string Description => "Opens Windows Task Manager";
        public bool RequiresElevation => false;

        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskmgr.exe",
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
