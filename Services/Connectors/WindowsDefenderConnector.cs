using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Services.Connectors
{
    public class WindowsDefenderConnector
    {
        public bool IsAvailable()
        {
            try
            {
                var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Windows Defender", "MpCmdRun.exe");
                return System.IO.File.Exists(path);
            }
            catch { return false; }
        }

        public async Task<string> RunScanAsync(bool full)
        {
            try
            {
                var defenderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Windows Defender", "MpCmdRun.exe");
                if (!System.IO.File.Exists(defenderPath)) return "Windows Defender not found";

                var args = full ? "-Scan -ScanType 2" : "-Scan -ScanType 1";
                var psi = new ProcessStartInfo
                {
                    FileName = defenderPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "Failed to start Defender scan";
                var output = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync();
                return string.IsNullOrWhiteSpace(output) ? (full ? "Full scan completed" : "Quick scan completed") : output;
            }
            catch (Exception ex)
            {
                return $"Defender scan error: {ex.Message}";
            }
        }

        public async Task<string> GetStatusAsync()
        {
            await Task.CompletedTask;
            return "Windows Defender status: (placeholder)";
        }
    }
}

