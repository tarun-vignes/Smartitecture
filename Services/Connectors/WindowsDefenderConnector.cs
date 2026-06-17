using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

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
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return "Failed to start Defender scan";
                await Task.CompletedTask;
                return full
                    ? $"Started a full Microsoft Defender scan in the background (PID {proc.Id}). This can take a while."
                    : $"Started a quick Microsoft Defender scan in the background (PID {proc.Id}). You can keep using the app while it runs.";
            }
            catch (Exception ex)
            {
                return $"Defender scan error: {ex.Message}";
            }
        }

        public async Task<string> GetScanStatusAsync()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Microsoft Defender scan status");

            var runningScans = Process.GetProcessesByName("MpCmdRun").ToList();
            if (runningScans.Count > 0)
            {
                builder.AppendLine("A Defender command-line scan appears to be running:");
                foreach (var process in runningScans)
                {
                    builder.AppendLine($"- MpCmdRun PID {process.Id}");
                    process.Dispose();
                }

                await Task.CompletedTask;
                return builder.ToString().Trim();
            }

            foreach (var process in runningScans)
            {
                process.Dispose();
            }

            try
            {
                const string logName = "Microsoft-Windows-Windows Defender/Operational";
                var query = new EventLogQuery(logName, PathType.LogName, "*[System[(EventID=1000 or EventID=1001 or EventID=1002 or EventID=1116 or EventID=1117)]]")
                {
                    ReverseDirection = true
                };

                using var reader = new EventLogReader(query);
                EventRecord? lastFinishedScan = null;
                EventRecord? lastStartedScan = null;
                for (var i = 0; i < 20; i++)
                {
                    var record = reader.ReadEvent();
                    if (record == null)
                    {
                        break;
                    }

                    var message = SafeFormatEvent(record);
                    if (record.Id == 1001)
                    {
                        lastFinishedScan = record;
                        continue;
                    }

                    if (record.Id == 1000)
                    {
                        lastStartedScan = record;
                        if (lastFinishedScan != null)
                        {
                            break;
                        }

                        record.Dispose();
                        continue;
                    }

                    if (record.Id == 1002)
                    {
                        builder.AppendLine($"Last scan was stopped before finishing at {record.TimeCreated:MMM d, yyyy h:mm tt}.");
                        builder.AppendLine(FirstLine(message));
                        record.Dispose();
                        await Task.CompletedTask;
                        return builder.ToString().Trim();
                    }

                    if (record.Id is 1116 or 1117)
                    {
                        builder.AppendLine($"Recent threat event at {record.TimeCreated:MMM d, yyyy h:mm tt}.");
                        builder.AppendLine(FirstLine(message));
                        record.Dispose();
                        await Task.CompletedTask;
                        return builder.ToString().Trim();
                    }

                    record.Dispose();
                }

                if (lastFinishedScan != null)
                {
                    var message = SafeFormatEvent(lastFinishedScan);
                    builder.AppendLine($"Last scan finished at {lastFinishedScan.TimeCreated:MMM d, yyyy h:mm tt}.");
                    builder.AppendLine(ContainsAny(message, "no threats", "no new threats", "found 0")
                        ? "Result: no threats were found."
                        : "Result: the scan finished, and Defender did not report any threats.");
                    lastFinishedScan.Dispose();
                    lastStartedScan?.Dispose();
                    await Task.CompletedTask;
                    return builder.ToString().Trim();
                }

                builder.AppendLine("No recent Defender scan result was found in the Windows Defender operational log.");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"Could not read Defender scan events: {ex.Message}");
            }

            await Task.CompletedTask;
            return builder.ToString().Trim();
        }

        public async Task<string> GetStatusAsync()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Windows security status");
            builder.AppendLine(IsAvailable()
                ? "Microsoft Defender command-line scanner: available"
                : "Microsoft Defender command-line scanner: not found");

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\SecurityCenter2",
                    "SELECT displayName, productState FROM AntiVirusProduct");

                foreach (ManagementObject item in searcher.Get())
                {
                    var name = item["displayName"]?.ToString();
                    var state = item["productState"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        builder.AppendLine($"Antivirus product: {name} (state {state ?? "unknown"})");
                    }
                }
            }
            catch (Exception ex)
            {
                builder.AppendLine($"Security Center query unavailable: {ex.Message}");
            }

            await Task.CompletedTask;
            return builder.ToString().Trim();
        }

        private static string SafeFormatEvent(EventRecord record)
        {
            try
            {
                return record.FormatDescription() ?? string.Empty;
            }
            catch
            {
                return $"Defender event {record.Id}";
            }
        }

        private static string FirstLine(string value)
        {
            return value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
                ?? value;
        }

        private static bool ContainsAny(string value, params string[] patterns)
        {
            return patterns.Any(pattern => value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }
}
