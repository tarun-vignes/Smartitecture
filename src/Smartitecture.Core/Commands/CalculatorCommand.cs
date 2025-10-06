using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Core.Commands
{
    /// <summary>
    /// Command to launch the Windows Calculator application
    /// </summary>
    public class CalculatorCommand : IAppCommand
    {
        public string CommandName => "Calculator";
        public string Description => "Opens Windows Calculator";
        public bool RequiresElevation => false;

        public async Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "calc.exe",
                    UseShellExecute = true
                });
                
                await Task.Delay(100); // Small delay to ensure process starts
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
