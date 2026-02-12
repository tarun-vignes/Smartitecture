using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Services.Connectors
{
    public class FirewallConnector
    {
        public async Task<string> GetStatusAsync()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall show allprofiles",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "Failed to query firewall";
                var output = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync();
                return output;
            }
            catch (Exception ex)
            {
                return $"Firewall status error: {ex.Message}";
            }
        }
    }
}

