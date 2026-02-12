using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AIPal.Services.AgentTools
{
    /// <summary>
    /// Provides system diagnostic tools for monitoring and analyzing PC performance.
    /// </summary>
    public class SystemDiagnosticsTools
    {
        // Tool definitions for system diagnostics
        public static readonly List<AgentTool> Tools = new List<AgentTool>
        {
            new AgentTool
            {
                Name = "get_system_temperature",
                Description = "Get the current temperature of CPU and GPU components",
                Parameters = new Dictionary<string, object>
                {
                    ["type"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["enum"] = new[] { "cpu", "gpu", "all" },
                        ["description"] = "The type of component to check temperature for"
                    }
                },
                Handler = GetSystemTemperature
            },
            
            new AgentTool
            {
                Name = "get_system_performance",
                Description = "Get current system performance metrics including CPU usage, memory usage, and disk activity",
                Parameters = new Dictionary<string, object>(),
                Handler = GetSystemPerformance
            },
            
            new AgentTool
            {
                Name = "analyze_startup_programs",
                Description = "Analyze startup programs and their impact on boot time",
                Parameters = new Dictionary<string, object>(),
                Handler = AnalyzeStartupPrograms
            },
            
            new AgentTool
            {
                Name = "check_disk_health",
                Description = "Check the health status of disk drives",
                Parameters = new Dictionary<string, object>
                {
                    ["drive"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Drive letter to check (e.g., 'C'). Leave empty to check all drives."
                    }
                },
                Handler = CheckDiskHealth
            },
            
            new AgentTool
            {
                Name = "get_running_processes",
                Description = "Get information about currently running processes and their resource usage",
                Parameters = new Dictionary<string, object>
                {
                    ["sort_by"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["enum"] = new[] { "cpu", "memory", "name" },
                        ["description"] = "Sort processes by this metric"
                    },
                    ["count"] = new Dictionary<string, object>
                    {
                        ["type"] = "integer",
                        ["description"] = "Number of processes to return (default: 10)"
                    }
                },
                Handler = GetRunningProcesses
            }
        };

        /// <summary>
        /// Gets temperature information for system components.
        /// </summary>
        private static async Task<string> GetSystemTemperature(JsonElement parameters)
        {
            string type = parameters.GetProperty("type").GetString() ?? "all";
            
            try
            {
                var temperatures = new Dictionary<string, double>();
                
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // Convert the value to Celsius (it's in tenths of degrees Kelvin)
                        double temp = Convert.ToDouble(obj["CurrentTemperature"]);
                        temp = (temp / 10) - 273.15;
                        
                        string deviceName = obj["InstanceName"].ToString();
                        temperatures[deviceName] = Math.Round(temp, 1);
                    }
                }
                
                // If no temperature sensors were found through WMI, provide an alternative message
                if (temperatures.Count == 0)
                {
                    return "Unable to access temperature sensors directly. Consider using a third-party tool like HWiNFO, Core Temp, or Open Hardware Monitor for detailed temperature monitoring.";
                }
                
                var result = new StringBuilder();
                result.AppendLine("System Temperature Information:");
                
                foreach (var temp in temperatures)
                {
                    result.AppendLine($"- {temp.Key}: {temp.Value}°C");
                }
                
                // Add recommendations based on temperatures
                result.AppendLine("\nRecommendations:");
                
                bool highTempFound = temperatures.Any(t => t.Value > 80);
                if (highTempFound)
                {
                    result.AppendLine("- WARNING: High temperatures detected! Consider checking your cooling system.");
                    result.AppendLine("- Ensure fans are working properly and heatsinks are clean of dust.");
                    result.AppendLine("- Consider improving airflow in your case or upgrading cooling solutions.");
                }
                else if (temperatures.Any(t => t.Value > 70))
                {
                    result.AppendLine("- Temperatures are elevated but within acceptable range.");
                    result.AppendLine("- Consider cleaning dust from your system if you haven't recently.");
                }
                else
                {
                    result.AppendLine("- Temperatures are within normal operating range.");
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error retrieving temperature information: {ex.Message}\n\nConsider using a third-party tool like HWiNFO, Core Temp, or Open Hardware Monitor for detailed temperature monitoring.";
            }
        }

        /// <summary>
        /// Gets current system performance metrics with adaptive recommendations based on hardware capabilities.
        /// </summary>
        private static async Task<string> GetSystemPerformance(JsonElement parameters)
        {
            try
            {
                // Create performance counters
                using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                using var diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
                using var diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
                
                // First call to initialize
                cpuCounter.NextValue();
                ramCounter.NextValue();
                diskReadCounter.NextValue();
                diskWriteCounter.NextValue();
                
                // Wait a moment to get accurate readings
                await Task.Delay(1000);
                
                // Get values
                float cpuUsage = cpuCounter.NextValue();
                float availableRam = ramCounter.NextValue();
                float diskReadRate = diskReadCounter.NextValue() / 1024 / 1024; // Convert to MB/s
                float diskWriteRate = diskWriteCounter.NextValue() / 1024 / 1024; // Convert to MB/s
                
                // Get system hardware information for context-aware recommendations
                var systemInfo = new Dictionary<string, object>();
                
                // Get CPU information
                string cpuModel = "Unknown";
                int cpuCores = 0;
                int cpuThreads = 0;
                double cpuSpeed = 0;
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        cpuModel = obj["Name"].ToString();
                        cpuCores = int.Parse(obj["NumberOfCores"].ToString());
                        cpuThreads = int.Parse(obj["NumberOfLogicalProcessors"].ToString());
                        cpuSpeed = double.Parse(obj["MaxClockSpeed"].ToString()) / 1000; // Convert to GHz
                        break; // Just get the first CPU
                    }
                }
                
                systemInfo["CpuModel"] = cpuModel;
                systemInfo["CpuCores"] = cpuCores;
                systemInfo["CpuThreads"] = cpuThreads;
                systemInfo["CpuSpeed"] = cpuSpeed;
                
                // Get total physical memory
                long totalRam = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalRam = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                    }
                }
                
                float totalRamGB = totalRam / 1024 / 1024 / 1024;
                float usedRamGB = totalRamGB - (availableRam / 1024);
                float ramUsagePercent = (usedRamGB / totalRamGB) * 100;
                
                systemInfo["TotalRamGB"] = totalRamGB;
                
                // Check if system has SSD
                bool hasSsd = false;
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string mediaType = obj["MediaType"]?.ToString() ?? "";
                        if (mediaType.Contains("SSD") || mediaType.Contains("Solid State"))
                        {
                            hasSsd = true;
                            break;
                        }
                    }
                }
                
                systemInfo["HasSsd"] = hasSsd;
                
                // Get system type (laptop or desktop)
                string chassisType = "Unknown";
                bool isLaptop = false;
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["ChassisTypes"] is uint[] types && types.Length > 0)
                        {
                            // ChassisTypes values 8-14 and 30-31 represent laptops/notebooks
                            uint type = types[0];
                            isLaptop = (type >= 8 && type <= 14) || type == 30 || type == 31;
                            
                            if (isLaptop)
                            {
                                chassisType = "Laptop";
                            }
                            else
                            {
                                chassisType = "Desktop";
                            }
                        }
                        break;
                    }
                }
                
                systemInfo["IsLaptop"] = isLaptop;
                systemInfo["ChassisType"] = chassisType;
                
                // Build the result with adaptive recommendations
                var result = new StringBuilder();
                result.AppendLine("System Performance Metrics:");
                result.AppendLine($"- CPU: {cpuModel} ({cpuCores} cores, {cpuThreads} threads, {cpuSpeed:F1} GHz)");
                result.AppendLine($"- CPU Usage: {cpuUsage:F1}%");
                result.AppendLine($"- Memory: {totalRamGB:F1} GB total");
                result.AppendLine($"- Memory Usage: {usedRamGB:F1} GB / {totalRamGB:F1} GB ({ramUsagePercent:F1}%)");
                result.AppendLine($"- Disk Read: {diskReadRate:F2} MB/s");
                result.AppendLine($"- Disk Write: {diskWriteRate:F2} MB/s");
                result.AppendLine($"- System Type: {chassisType}");
                result.AppendLine($"- Storage: {(hasSsd ? "SSD detected" : "No SSD detected")}");
                
                // Add adaptive analysis and recommendations based on hardware profile
                result.AppendLine("\nAdaptive Analysis & Recommendations:");
                
                // CPU recommendations based on hardware profile
                if (cpuUsage > 90)
                {
                    result.AppendLine("- CPU usage is very high for your system.");
                    
                    if (cpuCores <= 2)
                    {
                        result.AppendLine("  Your CPU has limited cores. Try to run fewer applications simultaneously.");
                        result.AppendLine("  Consider lightweight alternatives to resource-intensive applications.");
                    }
                    else
                    {
                        result.AppendLine("  Use Task Manager to identify which processes are consuming CPU resources.");
                        result.AppendLine("  Check for background processes that might be using excessive resources.");
                    }
                }
                else if (cpuUsage > 70)
                {
                    result.AppendLine("- CPU usage is elevated.");
                    
                    if (cpuCores <= 2)
                    {
                        result.AppendLine("  This is common on systems with fewer CPU cores.");
                        result.AppendLine("  Consider closing applications you're not actively using.");
                    }
                    else
                    {
                        result.AppendLine("  This might indicate a demanding application or background task.");
                    }
                }
                
                // RAM recommendations based on hardware profile
                if (ramUsagePercent > 90)
                {
                    result.AppendLine("- Memory usage is very high for your system.");
                    
                    if (totalRamGB <= 4)
                    {
                        result.AppendLine("  Your system has limited RAM. Consider these optimizations:");
                        result.AppendLine("  * Close unused browser tabs and applications");
                        result.AppendLine("  * Use lightweight applications when possible");
                        result.AppendLine("  * Disable unnecessary startup programs");
                        if (totalRamGB < 4)
                        {
                            result.AppendLine("  * Consider upgrading to at least 8GB RAM if possible for your device");
                        }
                    }
                    else if (totalRamGB <= 8)
                    {
                        result.AppendLine("  For systems with 8GB RAM, high memory usage can occur with multiple applications.");
                        result.AppendLine("  Try closing memory-intensive applications like browsers with many tabs.");
                    }
                    else
                    {
                        result.AppendLine("  Even with your higher RAM capacity, you're using most of it.");
                        result.AppendLine("  Check for memory leaks or unusually demanding applications.");
                    }
                }
                else if (ramUsagePercent > 80)
                {
                    result.AppendLine("- Memory usage is high.");
                    
                    if (totalRamGB <= 4)
                    {
                        result.AppendLine("  This is expected with limited RAM. Consider closing unused applications.");
                    }
                    else
                    {
                        result.AppendLine("  Monitor for potential performance issues if you open more applications.");
                    }
                }
                
                // Disk recommendations based on hardware profile
                if (diskReadRate > 50 || diskWriteRate > 50)
                {
                    result.AppendLine("- Disk activity is high.");
                    
                    if (!hasSsd)
                    {
                        result.AppendLine("  Your system appears to use a traditional hard drive (HDD).");
                        result.AppendLine("  High disk activity on HDDs can significantly impact performance.");
                        result.AppendLine("  Consider upgrading to an SSD for dramatically improved responsiveness.");
                    }
                    else
                    {
                        result.AppendLine("  Even with an SSD, high sustained disk activity can impact performance.");
                        result.AppendLine("  This might indicate file transfers, updates, or indexing.");
                    }
                }
                
                // System-specific recommendations
                if (isLaptop)
                {
                    result.AppendLine("\nLaptop-Specific Recommendations:");
                    result.AppendLine("- Check your power plan settings. Using 'Power Saver' mode can limit performance.");
                    result.AppendLine("- Ensure your laptop has proper ventilation to prevent thermal throttling.");
                    result.AppendLine("- Consider using a cooling pad if you frequently experience high temperatures.");
                    result.AppendLine("- When plugged in, set your power plan to 'High Performance' for maximum speed.");
                }
                
                // General optimization tips based on hardware profile
                result.AppendLine("\nOptimization Tips For Your System:");
                
                if (totalRamGB <= 4)
                {
                    result.AppendLine("- For systems with limited RAM (4GB or less):");
                    result.AppendLine("  * Minimize browser extensions and open tabs");
                    result.AppendLine("  * Use lightweight applications (e.g., Notepad instead of Word for simple tasks)");
                    result.AppendLine("  * Consider increasing virtual memory/page file size");
                    result.AppendLine("  * Disable visual effects (search for 'Adjust the appearance and performance of Windows')");
                }
                
                if (cpuCores <= 2)
                {
                    result.AppendLine("- For systems with fewer CPU cores:");
                    result.AppendLine("  * Limit the number of simultaneously running applications");
                    result.AppendLine("  * Disable unnecessary background services and startup items");
                    result.AppendLine("  * Consider using lightweight alternatives to resource-intensive software");
                }
                
                if (!hasSsd)
                {
                    result.AppendLine("- For systems with traditional hard drives (HDD):");
                    result.AppendLine("  * Regularly defragment your drive (search for 'Defragment and Optimize Drives')");
                    result.AppendLine("  * Keep at least 15-20% of your drive space free for optimal performance");
                    result.AppendLine("  * Consider upgrading to an SSD for the most significant performance improvement");
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error retrieving system performance metrics: {ex.Message}";
            }
        }

        /// <summary>
        /// Analyzes startup programs and their impact on boot time.
        /// </summary>
        private static async Task<string> AnalyzeStartupPrograms(JsonElement parameters)
        {
            try
            {
                var startupItems = new List<(string Name, string Location, string Publisher)>();
                
                // Check HKLM Run key
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            string path = key.GetValue(valueName).ToString();
                            startupItems.Add((valueName, path, GetFilePublisher(path)));
                        }
                    }
                }
                
                // Check HKCU Run key
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            string path = key.GetValue(valueName).ToString();
                            startupItems.Add((valueName, path, GetFilePublisher(path)));
                        }
                    }
                }
                
                // Check Startup folder
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                foreach (var file in Directory.GetFiles(startupFolder))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    startupItems.Add((fileName, file, GetFilePublisher(file)));
                }
                
                var result = new StringBuilder();
                result.AppendLine("Startup Programs Analysis:");
                
                if (startupItems.Count == 0)
                {
                    result.AppendLine("No startup items found.");
                }
                else
                {
                    result.AppendLine($"Found {startupItems.Count} startup items:");
                    
                    foreach (var item in startupItems)
                    {
                        result.AppendLine($"- {item.Name}");
                        result.AppendLine($"  Path: {item.Location}");
                        if (!string.IsNullOrEmpty(item.Publisher))
                        {
                            result.AppendLine($"  Publisher: {item.Publisher}");
                        }
                        result.AppendLine();
                    }
                    
                    result.AppendLine("Recommendations:");
                    result.AppendLine("- Review these startup programs and consider disabling non-essential ones to improve boot time.");
                    result.AppendLine("- You can manage startup items through Task Manager's Startup tab.");
                    result.AppendLine("- Common items that can be safely disabled include updaters for non-critical software,");
                    result.AppendLine("  cloud storage sync tools (if not needed immediately), and hardware utilities that can be run manually.");
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error analyzing startup programs: {ex.Message}";
            }
        }

        /// <summary>
        /// Gets the publisher information for a file.
        /// </summary>
        private static string GetFilePublisher(string filePath)
        {
            try
            {
                // Extract the actual executable path from potential command line arguments
                string actualPath = filePath.Trim('"');
                if (actualPath.Contains(" "))
                {
                    actualPath = actualPath.Substring(0, actualPath.IndexOf(" "));
                }
                
                if (!File.Exists(actualPath))
                {
                    return "Unknown";
                }
                
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(actualPath);
                return !string.IsNullOrEmpty(versionInfo.CompanyName) ? versionInfo.CompanyName : "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Checks the health status of disk drives.
        /// </summary>
        private static async Task<string> CheckDiskHealth(JsonElement parameters)
        {
            string driveLetter = parameters.TryGetProperty("drive", out var driveProperty) 
                ? driveProperty.GetString() 
                : string.Empty;
                
            try
            {
                var result = new StringBuilder();
                result.AppendLine("Disk Health Check:");
                
                // Get all drives or the specified one
                DriveInfo[] allDrives;
                if (string.IsNullOrEmpty(driveLetter))
                {
                    allDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed).ToArray();
                }
                else
                {
                    var drive = new DriveInfo(driveLetter);
                    if (!drive.IsReady)
                    {
                        return $"Drive {driveLetter}: is not ready or does not exist.";
                    }
                    allDrives = new[] { drive };
                }
                
                if (allDrives.Length == 0)
                {
                    return "No valid drives found to check.";
                }
                
                foreach (var drive in allDrives)
                {
                    result.AppendLine($"\nDrive {drive.Name} ({drive.VolumeLabel}):");
                    result.AppendLine($"- Type: {drive.DriveType}");
                    result.AppendLine($"- Format: {drive.DriveFormat}");
                    result.AppendLine($"- Total Size: {FormatSize(drive.TotalSize)}");
                    result.AppendLine($"- Free Space: {FormatSize(drive.AvailableFreeSpace)} ({(double)drive.AvailableFreeSpace / drive.TotalSize:P1})");
                    
                    // Check for low disk space
                    double freeSpacePercent = (double)drive.AvailableFreeSpace / drive.TotalSize;
                    if (freeSpacePercent < 0.1)
                    {
                        result.AppendLine("- WARNING: Very low disk space! This can severely impact system performance.");
                        result.AppendLine("  Consider cleaning up unnecessary files or adding storage.");
                    }
                    else if (freeSpacePercent < 0.2)
                    {
                        result.AppendLine("- Low disk space. Consider cleaning up unnecessary files.");
                    }
                    
                    // Try to get SMART data using WMI
                    try
                    {
                        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE DeviceID LIKE '%" + drive.Name[0] + "%'"))
                        {
                            foreach (ManagementObject wmiDisk in searcher.Get())
                            {
                                string model = wmiDisk["Model"].ToString();
                                string status = wmiDisk["Status"].ToString();
                                
                                result.AppendLine($"- Model: {model}");
                                result.AppendLine($"- Status: {status}");
                                
                                if (status.ToLower() != "ok" && status.ToLower() != "good")
                                {
                                    result.AppendLine("- WARNING: Disk status is not optimal. Consider backing up important data.");
                                }
                            }
                        }
                    }
                    catch
                    {
                        result.AppendLine("- Unable to retrieve detailed disk health information.");
                        result.AppendLine("  For comprehensive disk health analysis, consider using tools like CrystalDiskInfo.");
                    }
                }
                
                result.AppendLine("\nRecommendations:");
                result.AppendLine("- Regularly back up important data to prevent loss from disk failures.");
                result.AppendLine("- For detailed SMART health information, use specialized tools like CrystalDiskInfo.");
                result.AppendLine("- Consider defragmenting HDD drives (but not SSDs) if they're heavily fragmented.");
                result.AppendLine("- For SSDs, ensure TRIM is enabled for optimal performance.");
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error checking disk health: {ex.Message}";
            }
        }

        /// <summary>
        /// Formats a byte size into a human-readable string.
        /// </summary>
        private static string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }

        /// <summary>
        /// Gets information about currently running processes and their resource usage.
        /// </summary>
        private static async Task<string> GetRunningProcesses(JsonElement parameters)
        {
            string sortBy = parameters.TryGetProperty("sort_by", out var sortProperty) 
                ? sortProperty.GetString() 
                : "cpu";
                
            int count = parameters.TryGetProperty("count", out var countProperty) 
                ? countProperty.GetInt32() 
                : 10;
                
            try
            {
                var result = new StringBuilder();
                result.AppendLine("Running Processes Analysis:");
                
                var processes = Process.GetProcesses();
                var processInfoList = new List<(string Name, string Description, double CpuUsage, long MemoryUsage)>();
                
                foreach (var process in processes)
                {
                    try
                    {
                        string description = "Unknown";
                        try
                        {
                            if (!string.IsNullOrEmpty(process.MainModule?.FileName))
                            {
                                var fileInfo = FileVersionInfo.GetVersionInfo(process.MainModule.FileName);
                                description = !string.IsNullOrEmpty(fileInfo.FileDescription) 
                                    ? fileInfo.FileDescription 
                                    : process.ProcessName;
                            }
                        }
                        catch
                        {
                            description = process.ProcessName;
                        }
                        
                        // Get CPU usage (approximate)
                        double cpuUsage = 0;
                        try
                        {
                            using var counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                            counter.NextValue();
                            await Task.Delay(100);
                            cpuUsage = counter.NextValue() / Environment.ProcessorCount;
                        }
                        catch
                        {
                            // Ignore errors in CPU measurement
                        }
                        
                        processInfoList.Add((process.ProcessName, description, cpuUsage, process.WorkingSet64));
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }
                
                // Sort the list based on the specified criteria
                switch (sortBy)
                {
                    case "cpu":
                        processInfoList = processInfoList.OrderByDescending(p => p.CpuUsage).ToList();
                        break;
                    case "memory":
                        processInfoList = processInfoList.OrderByDescending(p => p.MemoryUsage).ToList();
                        break;
                    case "name":
                        processInfoList = processInfoList.OrderBy(p => p.Name).ToList();
                        break;
                }
                
                // Take only the requested number of processes
                processInfoList = processInfoList.Take(count).ToList();
                
                // Format the output
                result.AppendLine($"Top {processInfoList.Count} processes sorted by {sortBy}:");
                
                foreach (var process in processInfoList)
                {
                    result.AppendLine($"- {process.Name} ({process.Description})");
                    result.AppendLine($"  CPU: {process.CpuUsage:F1}%, Memory: {FormatSize(process.MemoryUsage)}");
                }
                
                // Add recommendations based on the process list
                result.AppendLine("\nAnalysis & Recommendations:");
                
                var highCpuProcesses = processInfoList.Where(p => p.CpuUsage > 20).ToList();
                if (highCpuProcesses.Any())
                {
                    result.AppendLine("- High CPU usage detected in these processes:");
                    foreach (var process in highCpuProcesses)
                    {
                        result.AppendLine($"  * {process.Name} ({process.CpuUsage:F1}%)");
                    }
                    result.AppendLine("  Consider investigating if these are expected to use this much CPU.");
                }
                
                var highMemoryProcesses = processInfoList.Where(p => p.MemoryUsage > 500 * 1024 * 1024).ToList();
                if (highMemoryProcesses.Any())
                {
                    result.AppendLine("- High memory usage detected in these processes:");
                    foreach (var process in highMemoryProcesses)
                    {
                        result.AppendLine($"  * {process.Name} ({FormatSize(process.MemoryUsage)})");
                    }
                    result.AppendLine("  Consider closing these if you're experiencing memory pressure.");
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error analyzing running processes: {ex.Message}";
            }
        }
    }
}
