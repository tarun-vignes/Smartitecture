using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Smartitecture.Services.Connectors
{
    public class PerformanceConnector
    {
        public async Task<double> GetCpuUsageAsync()
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "wmic";
                process.StartInfo.Arguments = "cpu get loadpercentage /value";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                var match = System.Text.RegularExpressions.Regex.Match(output, @"LoadPercentage=(\d+)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var usage)) return usage;
            }
            catch { }
            return 0.0;
        }

        public async Task<double> GetDiskUsageAsync()
        {
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
                var totalSize = drives.Sum(d => d.TotalSize);
                var usedSize = drives.Sum(d => d.TotalSize - d.AvailableFreeSpace);
                return (double)usedSize / totalSize * 100.0;
            }
            catch { return 0.0; }
        }

        public async Task<double> GetMemoryUsageAsync()
        {
            try
            {
                // Simple heuristic using current process vs. managed heap; replace with proper counters
                var total = GC.GetTotalMemory(false);
                var workingSet = Environment.WorkingSet;
                return (double)workingSet / (total + workingSet) * 100.0;
            }
            catch { return 0.0; }
        }
    }
}

