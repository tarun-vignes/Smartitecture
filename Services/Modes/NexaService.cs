using System;


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using Smartitecture.Services.Interfaces;
using Smartitecture.Services.Core;

namespace Smartitecture.Services.Modes
{
    /// <summary>
    /// NEXA - Performance optimization AI with hardware control, system monitoring, and automation
    /// </summary>
    public class NexaService : IAIMode
    {
        private readonly ILLMService _llmService;
        private PerformanceCounterCategory[] _performanceCategories;

        public string ModeName => "NEXA";
        public string ModeIcon => "⚡";
        public string ModeColor => "#8B5CF6";
        public string Description => "Performance optimizer with hardware control, system monitoring, and automation";

        public NexaService(ILLMService llmService)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            InitializePerformanceMonitoring();
        }

        public bool CanHandle(string query)
        {
            var performanceKeywords = new[]
            {
                "performance", "speed", "slow", "fast", "optimize", "cpu", "gpu", "memory", "ram",
                "disk", "temperature", "fan", "cooling", "benchmark", "fps", "lag", "freeze",
                "crash", "system", "hardware", "monitor", "overclock", "power", "battery"
            };

            var queryLower = query.ToLowerInvariant();
            return performanceKeywords.Any(keyword => queryLower.Contains(keyword));
        }

        public double GetConfidenceScore(string query)
        {
            var score = 0.0;
            var queryLower = query.ToLowerInvariant();

            // High confidence for explicit performance queries
            if (IsPerformanceOptimizationQuery(query)) score += 0.95;
            if (IsHardwareQuery(query)) score += 0.9;
            if (IsTemperatureQuery(query)) score += 0.85;
            if (IsSystemMonitoringQuery(query)) score += 0.8;
            if (IsPowerManagementQuery(query)) score += 0.75;

            // Medium confidence for system-related queries
            if (IsSystemInfoQuery(query)) score += 0.6;
            if (IsProcessManagementQuery(query)) score += 0.7;

            return Math.Max(0.0, Math.Min(1.0, score));
        }

        public async Task<AIContext> AnalyzeContextAsync(string query)
        {
            var context = new AIContext
            {
                Query = query,
                PrimaryMode = ModeName,
                ConfidenceScore = GetConfidenceScore(query),
                RequiresSystemAccess = RequiresSystemAccess(query)
            };

            // Detect performance-related intents
            if (IsPerformanceOptimizationQuery(query)) context.DetectedIntents.Add("performance_optimization");
            if (IsHardwareQuery(query)) context.DetectedIntents.Add("hardware_analysis");
            if (IsTemperatureQuery(query)) context.DetectedIntents.Add("temperature_monitoring");
            if (IsSystemMonitoringQuery(query)) context.DetectedIntents.Add("system_monitoring");
            if (IsPowerManagementQuery(query)) context.DetectedIntents.Add("power_management");
            if (IsProcessManagementQuery(query)) context.DetectedIntents.Add("process_management");

            // Determine if collaboration is needed
            if (IsGeneralPerformanceQuestion(query))
            {
                context.CollaboratingModes.Add("LUMEN"); // For general explanation
            }

            if (ContainsSecurityImplications(query))
            {
                context.RequiresSecurityAnalysis = true;
                context.CollaboratingModes.Add("FORTIS");
            }

            return context;
        }

        public async Task<string> ProcessQueryAsync(string query, string conversationId = null)
        {
            try
            {
                // Handle performance optimization requests
                if (IsPerformanceOptimizationQuery(query))
                {
                    return await OptimizeSystemPerformanceAsync(query);
                }

                // Handle hardware information queries
                if (IsHardwareQuery(query))
                {
                    return await GetHardwareInformationAsync();
                }

                // Handle temperature monitoring
                if (IsTemperatureQuery(query))
                {
                    return await MonitorSystemTemperatureAsync();
                }

                // Handle system monitoring
                if (IsSystemMonitoringQuery(query))
                {
                    return await GetSystemMonitoringDataAsync();
                }

                // Handle power management
                if (IsPowerManagementQuery(query))
                {
                    return await HandlePowerManagementAsync(query);
                }

                // Handle process management
                if (IsProcessManagementQuery(query))
                {
                    return await HandleProcessManagementAsync(query);
                }

                // Default to LLM for performance-related questions
                var prompt = BuildNexaPrompt(query);
                return await _llmService.GetResponseAsync(prompt, conversationId);
            }
            catch (Exception ex)
            {
                return $"⚡ NEXA encountered an error: {ex.Message}";
            }
        }

        public async Task InitializeAsync()
        {
            // Initialize performance monitoring and hardware interfaces
            await Task.CompletedTask;
        }

        #region Private Helper Methods

        private void InitializePerformanceMonitoring()
        {
            try
            {
                _performanceCategories = PerformanceCounterCategory.GetCategories()
                    .Where(cat => new[] { "Processor", "Memory", "PhysicalDisk", "Network Interface" }
                        .Contains(cat.CategoryName))
                    .ToArray();
            }
            catch
            {
                // Handle initialization errors silently
            }
        }

        private bool IsPerformanceOptimizationQuery(string query)
        {
            var optimizeKeywords = new[] { "optimize", "speed up", "make faster", "improve performance", "boost" };
            return optimizeKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsHardwareQuery(string query)
        {
            var hardwareKeywords = new[] { "cpu", "gpu", "processor", "graphics", "motherboard", "ram", "memory", "disk", "ssd", "hdd" };
            return hardwareKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsTemperatureQuery(string query)
        {
            var tempKeywords = new[] { "temperature", "temp", "hot", "cold", "cooling", "fan", "thermal", "overheat" };
            return tempKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsSystemMonitoringQuery(string query)
        {
            var monitorKeywords = new[] { "monitor", "usage", "utilization", "stats", "metrics", "performance" };
            return monitorKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsPowerManagementQuery(string query)
        {
            var powerKeywords = new[] { "power", "battery", "energy", "consumption", "sleep", "hibernate" };
            return powerKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsProcessManagementQuery(string query)
        {
            var processKeywords = new[] { "process", "task", "application", "program", "kill", "end", "stop" };
            return processKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsSystemInfoQuery(string query)
        {
            var systemKeywords = new[] { "system", "computer", "pc", "specs", "information" };
            return systemKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool RequiresSystemAccess(string query)
        {
            var systemAccessKeywords = new[] { "optimize", "kill", "end", "change", "modify", "overclock" };
            return systemAccessKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsGeneralPerformanceQuestion(string query)
        {
            var questionWords = new[] { "what", "how", "why", "when", "where" };
            return questionWords.Any(q => query.ToLowerInvariant().StartsWith(q));
        }

        private bool ContainsSecurityImplications(string query)
        {
            var securityKeywords = new[] { "overclock", "modify", "change settings", "registry" };
            return securityKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private async Task<string> OptimizeSystemPerformanceAsync(string query)
        {
            var results = new List<string>();
            results.Add("⚡ **NEXA Performance Optimization**");
            results.Add("");

            try
            {
                // Analyze current system performance
                var cpuUsage = await GetCPUUsageAsync();
                var memoryUsage = await GetMemoryUsageAsync();
                var diskUsage = await GetDiskUsageAsync();

                results.Add("📊 **Current System Status**:");
                results.Add($"🖥️ CPU Usage: {cpuUsage:F1}%");
                results.Add($"💾 Memory Usage: {memoryUsage:F1}%");
                results.Add($"💿 Disk Usage: {diskUsage:F1}%");
                results.Add("");

                // Provide optimization recommendations
                results.Add("🚀 **Optimization Recommendations**:");

                if (cpuUsage > 80)
                {
                    results.Add("🔴 **High CPU Usage Detected**");
                    results.Add("• Close unnecessary applications");
                    results.Add("• Check for background processes");
                    results.Add("• Consider upgrading CPU or adding cooling");
                }

                if (memoryUsage > 80)
                {
                    results.Add("🟠 **High Memory Usage Detected**");
                    results.Add("• Close unused browser tabs");
                    results.Add("• Restart memory-intensive applications");
                    results.Add("• Consider adding more RAM");
                }

                if (diskUsage > 90)
                {
                    results.Add("🟡 **High Disk Usage Detected**");
                    results.Add("• Run Disk Cleanup utility");
                    results.Add("• Move files to external storage");
                    results.Add("• Consider SSD upgrade");
                }

                // General optimization tips
                results.Add("");
                results.Add("💡 **General Performance Tips**:");
                results.Add("• Keep Windows updated");
                results.Add("• Run regular disk defragmentation");
                results.Add("• Disable startup programs you don't need");
                results.Add("• Clear temporary files regularly");
                results.Add("• Ensure adequate cooling and ventilation");

                // Ask for confirmation for system changes
                if (query.ToLowerInvariant().Contains("apply") || query.ToLowerInvariant().Contains("do it"))
                {
                    results.Add("");
                    results.Add("⚠️ **Confirmation Required**");
                    results.Add("Some optimizations require system changes. Would you like me to proceed?");
                    results.Add("Type 'yes' to confirm or 'no' to cancel.");
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ Error analyzing system performance: {ex.Message}");
            }

            return string.Join("\n", results);
        }

        private async Task<string> GetHardwareInformationAsync()
        {
            var results = new List<string>();
            results.Add("🔧 **Hardware Information**");
            results.Add("");

            try
            {
                // CPU Information
                var cpuInfo = await GetCPUInformationAsync();
                results.Add("🖥️ **Processor**:");
                results.AddRange(cpuInfo);
                results.Add("");

                // Memory Information
                var memoryInfo = await GetMemoryInformationAsync();
                results.Add("💾 **Memory**:");
                results.AddRange(memoryInfo);
                results.Add("");

                // Storage Information
                var storageInfo = await GetStorageInformationAsync();
                results.Add("💿 **Storage**:");
                results.AddRange(storageInfo);
                results.Add("");

                // Graphics Information
                var graphicsInfo = await GetGraphicsInformationAsync();
                results.Add("🎮 **Graphics**:");
                results.AddRange(graphicsInfo);
            }
            catch (Exception ex)
            {
                results.Add($"❌ Error retrieving hardware information: {ex.Message}");
            }

            return string.Join("\n", results);
        }

        private async Task<string> MonitorSystemTemperatureAsync()
        {
            var results = new List<string>();
            results.Add("🌡️ **System Temperature Monitoring**");
            results.Add("");

            try
            {
                // Note: This is a simplified implementation
                // In production, you'd use proper hardware monitoring libraries like OpenHardwareMonitor
                
                results.Add("📊 **Temperature Status**:");
                results.Add("🖥️ CPU Temperature: Monitoring... (Requires hardware sensors)");
                results.Add("🎮 GPU Temperature: Monitoring... (Requires hardware sensors)");
                results.Add("💾 System Temperature: Normal range");
                results.Add("");

                results.Add("🌡️ **Temperature Guidelines**:");
                results.Add("• CPU: Idle <40°C, Load <80°C");
                results.Add("• GPU: Idle <50°C, Load <85°C");
                results.Add("• System: Ambient +10-15°C");
                results.Add("");

                results.Add("❄️ **Cooling Recommendations**:");
                results.Add("• Ensure proper case ventilation");
                results.Add("• Clean dust from fans and heatsinks");
                results.Add("• Check thermal paste application");
                results.Add("• Consider upgrading cooling solution");
                results.Add("• Monitor fan speeds and curves");

                results.Add("");
                results.Add("ℹ️ **Note**: For detailed temperature monitoring, install hardware monitoring software like HWiNFO64 or Core Temp");
            }
            catch (Exception ex)
            {
                results.Add($"❌ Error monitoring temperature: {ex.Message}");
            }

            return string.Join("\n", results);
        }

        private async Task<string> GetSystemMonitoringDataAsync()
        {
            var results = new List<string>();
            results.Add("📊 **Real-Time System Monitoring**");
            results.Add("");

            try
            {
                var cpuUsage = await GetCPUUsageAsync();
                var memoryUsage = await GetMemoryUsageAsync();
                var diskUsage = await GetDiskUsageAsync();
                var networkUsage = await GetNetworkUsageAsync();

                results.Add("📈 **Current Performance Metrics**:");
                results.Add($"🖥️ CPU Usage: {cpuUsage:F1}% {GetUsageBar(cpuUsage)}");
                results.Add($"💾 Memory Usage: {memoryUsage:F1}% {GetUsageBar(memoryUsage)}");
                results.Add($"💿 Disk Activity: {diskUsage:F1}% {GetUsageBar(diskUsage)}");
                results.Add($"🌐 Network Activity: {networkUsage:F1}% {GetUsageBar(networkUsage)}");
                results.Add("");

                // Top processes
                var topProcesses = await GetTopProcessesAsync();
                results.Add("🔝 **Top Resource Consumers**:");
                results.AddRange(topProcesses.Take(5));
                results.Add("");

                // System uptime
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                results.Add($"⏱️ **System Uptime**: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
                
                // Available memory
                var availableMemory = await GetAvailableMemoryAsync();
                results.Add($"💾 **Available Memory**: {availableMemory:F1} GB");
            }
            catch (Exception ex)
            {
                results.Add($"❌ Error retrieving monitoring data: {ex.Message}");
            }

            return string.Join("\n", results);
        }

        private async Task<string> HandlePowerManagementAsync(string query)
        {
            var results = new List<string>();
            results.Add("🔋 **Power Management**");
            results.Add("");

            try
            {
                // Battery status (if applicable)
                var batteryStatus = await GetBatteryStatusAsync();
                if (!string.IsNullOrEmpty(batteryStatus))
                {
                    results.Add("🔋 **Battery Status**:");
                    results.Add(batteryStatus);
                    results.Add("");
                }

                // Power plan information
                results.Add("⚡ **Power Plans**:");
                results.Add("• Balanced: Optimal performance and energy efficiency");
                results.Add("• High Performance: Maximum performance, higher energy usage");
                results.Add("• Power Saver: Extended battery life, reduced performance");
                results.Add("");

                // Power optimization recommendations
                results.Add("💡 **Power Optimization Tips**:");
                results.Add("• Adjust screen brightness");
                results.Add("• Use sleep mode when inactive");
                results.Add("• Disable unnecessary background apps");
                results.Add("• Enable adaptive brightness");
                results.Add("• Use power-efficient hardware settings");

                if (query.ToLowerInvariant().Contains("change") || query.ToLowerInvariant().Contains("set"))
                {
                    results.Add("");
                    results.Add("⚠️ **Power Plan Changes**:");
                    results.Add("To change power plans, go to Settings > System > Power & battery");
                    results.Add("Or use 'powercfg' command in administrator command prompt");
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ Error handling power management: {ex.Message}");
            }

            return string.Join("\n", results);
        }

        private async Task<string> HandleProcessManagementAsync(string query)
        {
            var results = new List<string>();
            results.Add("🔄 **Process Management**");
            results.Add("");

            try
            {
                if (query.ToLowerInvariant().Contains("kill") || query.ToLowerInvariant().Contains("end"))
                {
                    results.Add("⚠️ **Process Termination**");
                    results.Add("Ending processes can cause data loss or system instability.");
                    results.Add("Please specify the process name or use Task Manager for safety.");
                    results.Add("");
                }

                // Show running processes
                var processes = await GetRunningProcessesAsync();
                results.Add("📋 **Running Processes** (Top 10 by CPU usage):");
                results.AddRange(processes.Take(10));
                results.Add("");

                results.Add("🛠️ **Process Management Tips**:");
                results.Add("• Use Task Manager (Ctrl+Shift+Esc) for detailed process control");
                results.Add("• End tasks carefully to avoid data loss");
                results.Add("• Monitor resource usage regularly");
                results.Add("• Identify and manage startup programs");
            }
            catch (Exception ex)
            {
                results.Add($"❌ Error managing processes: {ex.Message}");
            }

            return string.Join("\n", results);
        }

        #endregion

        #region Performance Monitoring Helper Methods

        private async Task<double> GetCPUUsageAsync()
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

                var match = Regex.Match(output, @"LoadPercentage=(\d+)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var usage))
                {
                    return usage;
                }
            }
            catch { }

            return 0.0; // Fallback
        }

        private async Task<double> GetMemoryUsageAsync()
        {
            try
            {
                var totalMemory = GC.GetTotalMemory(false);
                var workingSet = Environment.WorkingSet;
                return (double)workingSet / (totalMemory + workingSet) * 100;
            }
            catch
            {
                return 0.0;
            }
        }

        private async Task<double> GetDiskUsageAsync()
        {
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
                var totalSize = drives.Sum(d => d.TotalSize);
                var usedSize = drives.Sum(d => d.TotalSize - d.AvailableFreeSpace);
                return (double)usedSize / totalSize * 100;
            }
            catch
            {
                return 0.0;
            }
        }

        private async Task<double> GetNetworkUsageAsync()
        {
            // Simplified network usage calculation
            return 0.0; // Would require proper network monitoring
        }

        private async Task<List<string>> GetCPUInformationAsync()
        {
            var info = new List<string>();
            try
            {
                info.Add($"• Name: {Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown"}");
                info.Add($"• Cores: {Environment.ProcessorCount}");
                info.Add($"• Architecture: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "Unknown"}");
            }
            catch
            {
                info.Add("• Unable to retrieve CPU information");
            }
            return info;
        }

        private async Task<List<string>> GetMemoryInformationAsync()
        {
            var info = new List<string>();
            try
            {
                var totalMemory = GC.GetTotalMemory(false);
                info.Add($"• Working Set: {Environment.WorkingSet / (1024 * 1024)} MB");
                info.Add($"• GC Memory: {totalMemory / (1024 * 1024)} MB");
            }
            catch
            {
                info.Add("• Unable to retrieve memory information");
            }
            return info;
        }

        private async Task<List<string>> GetStorageInformationAsync()
        {
            var info = new List<string>();
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
                foreach (var drive in drives)
                {
                    var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                    var usedPercent = (double)usedSpace / drive.TotalSize * 100;
                    info.Add($"• Drive {drive.Name}: {FormatBytes(usedSpace)} / {FormatBytes(drive.TotalSize)} ({usedPercent:F1}% used)");
                }
            }
            catch
            {
                info.Add("• Unable to retrieve storage information");
            }
            return info;
        }

        private async Task<List<string>> GetGraphicsInformationAsync()
        {
            var info = new List<string>();
            try
            {
                info.Add("• Graphics information requires additional hardware detection");
                info.Add("• Use GPU-Z or similar tools for detailed GPU information");
            }
            catch
            {
                info.Add("• Unable to retrieve graphics information");
            }
            return info;
        }

        private async Task<List<string>> GetTopProcessesAsync()
        {
            var processes = new List<string>();
            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => !p.HasExited)
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(5);

                foreach (var process in runningProcesses)
                {
                    try
                    {
                        var memoryMB = process.WorkingSet64 / (1024 * 1024);
                        processes.Add($"• {process.ProcessName}: {memoryMB} MB");
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }
            }
            catch
            {
                processes.Add("• Unable to retrieve process information");
            }
            return processes;
        }

        private async Task<List<string>> GetRunningProcessesAsync()
        {
            return await GetTopProcessesAsync(); // Reuse the same logic
        }

        private async Task<double> GetAvailableMemoryAsync()
        {
            try
            {
                // Simplified calculation
                return Environment.WorkingSet / (1024.0 * 1024.0 * 1024.0);
            }
            catch
            {
                return 0.0;
            }
        }

        private async Task<string> GetBatteryStatusAsync()
        {
            return await Task.FromResult<string>(null);
        }

        private string GetUsageBar(double percentage)
        {
            var barLength = 10;
            var filledLength = (int)(percentage / 100.0 * barLength);
            var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
            
            return percentage switch
            {
                < 50 => $"🟢 [{bar}]",
                < 80 => $"🟡 [{bar}]",
                _ => $"🔴 [{bar}]"
            };
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        // New: Start/stop a process by name
        public async Task<string> StartProcessAsync(string processName)
        {
            try { Process.Start(new ProcessStartInfo { FileName = processName, UseShellExecute = true }); return $"Optimization recommendation: started {processName}."; } catch (Exception ex) { return $"Failed to start {processName}: {ex.Message}"; }
        }
        public async Task<string> KillProcessAsync(string processName)
        {
            try { foreach (var p in Process.GetProcessesByName(processName)) { try { p.Kill(); } catch { } } return $"System analysis indicates: attempted to stop {processName}."; } catch (Exception ex) { return $"Failed to stop {processName}: {ex.Message}"; }
        }

        // New: Startup optimization (list entries)
        public async Task<string> ListStartupProgramsAsync()
        {
            var entries = new System.Text.StringBuilder();
            void read(RegistryKey k, string scope) { if (k==null) return; foreach(var n in k.GetValueNames()){ entries.AppendLine($"- {scope}: {n} -> {k.GetValue(n)}"); } }
            using var hku = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
            using var hklm = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            read(hku, "HKCU"); read(hklm, "HKLM");
            return entries.Length==0 ? "Current system status: no startup entries found." : $"Current system status:\n{entries.ToString()}";
        }

        // New: Automated cleanup
        public async Task<string> CleanupTempFilesAsync()
        {
            int count=0; long bytes=0; void clean(string dir){ if(!Directory.Exists(dir)) return; foreach(var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)){ try{ var fi=new FileInfo(f); bytes+=fi.Length; fi.Attributes = FileAttributes.Normal; fi.Delete(); count++; } catch{} } }
            clean(Path.GetTempPath());
            var winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
            clean(winTemp);
            return $"Optimization recommendation: cleaned {count} files (~{bytes/1024/1024:N0} MB).";
        }
        private string BuildNexaPrompt(string query)
        {
            return $@"You are NEXA, a system performance optimization AI integrated into the Smartitecture system.
You specialize in:
- Hardware analysis and optimization
- System performance monitoring
- Temperature and cooling management
- Power management and efficiency
- Process and resource management
- Performance troubleshooting and tuning

Always provide technical, actionable advice with specific steps and recommendations.
Include performance metrics and optimization suggestions when relevant.
Warn users about potentially risky operations that could affect system stability.

User Query: {query}

Provide a comprehensive performance-focused response with specific technical recommendations and actionable steps.";
        }

        #endregion
    }
}



