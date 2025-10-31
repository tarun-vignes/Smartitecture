using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Smartitecture.Services.Connectors
{
    public class ProcessConnector
    {
        public async Task<IReadOnlyList<string>> ListProcessesAsync()
        {
            try
            {
                var processes = Process.GetProcesses();
                return processes.OrderByDescending(p => p.ProcessName)
                    .Select(p => $"{p.ProcessName} (PID: {p.Id})")
                    .ToList();
            }
            catch { return Array.Empty<string>(); }
        }

        public async Task<bool> KillProcessAsync(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                p.Kill(true);
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> StartProcessAsync(string fileName, string arguments = null)
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

