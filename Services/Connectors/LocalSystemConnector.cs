using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Smartitecture.Services.Connectors
{
    public sealed class LocalSystemConnector
    {
        public Task<string> GetSystemInfoAsync()
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => $"{d.Name} {FormatBytes(d.AvailableFreeSpace)} free of {FormatBytes(d.TotalSize)}")
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Local system info");
            builder.AppendLine($"Machine: {Environment.MachineName}");
            builder.AppendLine($"User: {Environment.UserName}");
            builder.AppendLine($"OS: {Environment.OSVersion}");
            builder.AppendLine($"Architecture: {Environment.Is64BitOperatingSystem switch { true => "64-bit", false => "32-bit" }} OS, {Environment.ProcessorCount} logical processors");
            builder.AppendLine($"Uptime: {FormatDuration(TimeSpan.FromMilliseconds(Environment.TickCount64))}");
            builder.AppendLine($"Process count: {Process.GetProcesses().Length}");

            if (drives.Count > 0)
            {
                builder.AppendLine("Drives:");
                foreach (var drive in drives)
                {
                    builder.AppendLine($"- {drive}");
                }
            }

            return Task.FromResult(builder.ToString().Trim());
        }

        public async Task<string> GetPerformanceSnapshotAsync()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Performance snapshot");
            builder.AppendLine($"CPU load: {await TryGetCpuLoadAsync()}");
            builder.AppendLine($"Memory: {GetSystemMemorySummary()}");
            builder.AppendLine($"Processes: {Process.GetProcesses().Length}");

            var topProcesses = GetProcesses()
                .OrderByDescending(p => p.WorkingSetBytes)
                .Take(5)
                .ToList();

            if (topProcesses.Count > 0)
            {
                builder.AppendLine("Top memory processes:");
                foreach (var process in topProcesses)
                {
                    builder.AppendLine($"- {process.Name} (PID {process.Id}): {FormatBytes(process.WorkingSetBytes)}");
                }
            }

            return builder.ToString().Trim();
        }

        public Task<string> ListProcessesAsync(int count = 12)
        {
            count = Math.Clamp(count, 1, 50);
            var processes = GetProcesses()
                .OrderByDescending(p => p.WorkingSetBytes)
                .Take(count)
                .ToList();

            if (processes.Count == 0)
            {
                return Task.FromResult("No process information is available.");
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Top {processes.Count} processes by memory");
            foreach (var process in processes)
            {
                var status = process.Responding.HasValue
                    ? process.Responding.Value ? "responding" : "not responding"
                    : "status unknown";
                builder.AppendLine($"- {process.Name} | PID {process.Id} | {FormatBytes(process.WorkingSetBytes)} | {status}");
            }

            return Task.FromResult(builder.ToString().Trim());
        }

        public Task<string> GetNetworkAdaptersAsync()
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(a => a.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .OrderByDescending(a => a.OperationalStatus == OperationalStatus.Up)
                .ThenBy(a => a.Name)
                .ToList();

            if (adapters.Count == 0)
            {
                return Task.FromResult("No network adapters were found.");
            }

            var builder = new StringBuilder();
            builder.AppendLine("Network adapters");
            foreach (var adapter in adapters)
            {
                var props = adapter.GetIPProperties();
                var addresses = props.UnicastAddresses
                    .Select(a => a.Address.ToString())
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Take(3);

                builder.AppendLine($"- {adapter.Name}: {adapter.OperationalStatus}, {adapter.NetworkInterfaceType}, {FormatBytes(adapter.Speed / 8)}/s link");
                var addressText = string.Join(", ", addresses);
                if (!string.IsNullOrWhiteSpace(addressText))
                {
                    builder.AppendLine($"  IP: {addressText}");
                }
            }

            return Task.FromResult(builder.ToString().Trim());
        }

        public Task<string> GetBatteryStatusAsync()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT EstimatedChargeRemaining, BatteryStatus, EstimatedRunTime FROM Win32_Battery");

                var batteries = searcher.Get().OfType<ManagementObject>().ToList();
                if (batteries.Count == 0)
                {
                    return Task.FromResult("Battery status\nNo battery was detected. This device may be a desktop PC or Windows did not expose battery data.");
                }

                var builder = new StringBuilder();
                builder.AppendLine("Battery status");
                for (var i = 0; i < batteries.Count; i++)
                {
                    var battery = batteries[i];
                    var charge = Convert.ToInt32(battery["EstimatedChargeRemaining"] ?? -1);
                    var statusCode = Convert.ToInt32(battery["BatteryStatus"] ?? 0);
                    var runtime = Convert.ToInt32(battery["EstimatedRunTime"] ?? -1);

                    builder.AppendLine($"- Battery {i + 1}: {(charge >= 0 ? $"{charge}% charged" : "charge unavailable")}, {FormatBatteryStatus(statusCode)}");
                    if (runtime > 0 && runtime < 71582788)
                    {
                        builder.AppendLine($"  Estimated runtime: {runtime} minutes");
                    }
                }

                return Task.FromResult(builder.ToString().Trim());
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Battery status unavailable: {ex.Message}");
            }
        }

        private static string FormatBatteryStatus(int statusCode)
        {
            return statusCode switch
            {
                1 => "discharging",
                2 => "connected to AC power",
                3 => "fully charged",
                4 => "low",
                5 => "critical",
                6 => "charging",
                7 => "charging and high",
                8 => "charging and low",
                9 => "charging and critical",
                10 => "undefined",
                11 => "partially charged",
                _ => "status unknown"
            };
        }

        public Task<string> KillProcessAsync(int? pid, string? name)
        {
            try
            {
                Process? process = null;
                if (pid.HasValue)
                {
                    process = Process.GetProcessById(pid.Value);
                }
                else if (!string.IsNullOrWhiteSpace(name))
                {
                    process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(name.Trim()))
                        .OrderByDescending(p => p.WorkingSet64)
                        .FirstOrDefault();
                }

                if (process == null)
                {
                    return Task.FromResult("No matching process was found.");
                }

                var processName = process.ProcessName;
                var processId = process.Id;
                process.Kill();
                return Task.FromResult($"Stopped process {processName} (PID {processId}).");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Could not stop the process: {ex.Message}");
            }
        }

        private static List<ProcessSnapshot> GetProcesses()
        {
            var snapshots = new List<ProcessSnapshot>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    snapshots.Add(new ProcessSnapshot
                    {
                        Id = process.Id,
                        Name = process.ProcessName,
                        WorkingSetBytes = process.WorkingSet64,
                        Responding = process.MainWindowHandle == IntPtr.Zero ? null : process.Responding
                    });
                }
                catch
                {
                }
                finally
                {
                    process.Dispose();
                }
            }

            return snapshots;
        }

        private static async Task<string> TryGetCpuLoadAsync()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT LoadPercentage FROM Win32_Processor");

                var values = searcher.Get()
                    .OfType<ManagementObject>()
                    .Select(item => item["LoadPercentage"])
                    .Where(value => value != null)
                    .Select(value => Convert.ToDouble(value))
                    .ToList();

                await Task.CompletedTask;
                return values.Count == 0 ? "unavailable" : $"{values.Average():0.0}%";
            }
            catch
            {
                return "unavailable";
            }
        }

        private static string GetSystemMemorySummary()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");

                var os = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                if (os != null)
                {
                    var totalKb = Convert.ToInt64(os["TotalVisibleMemorySize"] ?? 0);
                    var freeKb = Convert.ToInt64(os["FreePhysicalMemory"] ?? 0);
                    if (totalKb > 0 && freeKb >= 0)
                    {
                        var totalBytes = totalKb * 1024;
                        var freeBytes = freeKb * 1024;
                        var usedBytes = Math.Max(0, totalBytes - freeBytes);
                        var usedPercent = totalBytes == 0 ? 0 : usedBytes * 100.0 / totalBytes;
                        return $"{FormatBytes(usedBytes)} used of {FormatBytes(totalBytes)} ({usedPercent:0.0}% used)";
                    }
                }
            }
            catch
            {
            }

            return "unavailable";
        }

        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            var unit = 0;
            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }

            return $"{value:0.##} {units[unit]}";
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
            }

            return $"{duration.Hours}h {duration.Minutes}m";
        }

        private sealed class ProcessSnapshot
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public long WorkingSetBytes { get; set; }
            public bool? Responding { get; set; }
        }
    }
}
