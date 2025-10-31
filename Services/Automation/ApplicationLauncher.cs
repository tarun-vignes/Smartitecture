using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Services.Automation
{
    public class ApplicationLauncher
    {
        public async Task<bool> LaunchAsync(string fileName, string? arguments = null)
        {
            try
            {
                var psi = new ProcessStartInfo { FileName = fileName, Arguments = arguments ?? string.Empty };
                Process.Start(psi);
                return true;
            }
            catch { return false; }
        }
    }
}

