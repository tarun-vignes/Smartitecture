using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartitectureSimple
{
    public class WindowsAutomationService
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        public async Task<string> ExecuteAutomationCommand(string action, Dictionary<string, object> parameters)
        {
            try
            {
                return action.ToLower() switch
                {
                    "screenshot" => await TakeScreenshotAsync(),
                    "focus_window" => await FocusWindowAsync(parameters),
                    "system_info" => await GetSystemInfoAsync(),
                    "list_processes" => await ListProcessesAsync(),
                    "start_process" => await StartProcessAsync(parameters),
                    "close_process" => await CloseProcessAsync(parameters),
                    "get_disk_usage" => await GetDiskUsageAsync(),
                    "get_network_info" => await GetNetworkInfoAsync(),
                    _ => $"Unknown automation command: {action}"
                };
            }
            catch (Exception ex)
            {
                return $"Error executing {action}: {ex.Message}";
            }
        }

        private async Task<string> TakeScreenshotAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var bounds = Screen.PrimaryScreen.Bounds;
                    using var bitmap = new Bitmap(bounds.Width, bounds.Height);
                    using var graphics = Graphics.FromImage(bitmap);
                    
                    graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    
                    var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                    
                    bitmap.Save(filePath, ImageFormat.Png);
                    
                    // Open the screenshot
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    
                    return $"✅ Screenshot saved to: {filePath}";
                }
                catch (Exception ex)
                {
                    return $"❌ Screenshot failed: {ex.Message}";
                }
            });
        }

        private async Task<string> FocusWindowAsync(Dictionary<string, object> parameters)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!parameters.TryGetValue("process", out var processName))
                        return "❌ Process name not specified";

                    var processes = Process.GetProcessesByName(processName.ToString());
                    if (processes.Length == 0)
                        return $"❌ No process found with name: {processName}";

                    var process = processes[0];
                    var handle = process.MainWindowHandle;
                    
                    if (handle == IntPtr.Zero)
                        return $"❌ No window found for process: {processName}";

                    ShowWindow(handle, SW_RESTORE);
                    SetForegroundWindow(handle);
                    
                    return $"✅ Focused window: {process.ProcessName} - {process.MainWindowTitle}";
                }
                catch (Exception ex)
                {
                    return $"❌ Focus window failed: {ex.Message}";
                }
            });
        }

        private async Task<string> GetSystemInfoAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var info = new StringBuilder();
                    info.AppendLine("🖥️ System Information:");
                    info.AppendLine($"OS: {Environment.OSVersion}");
                    info.AppendLine($"Machine: {Environment.MachineName}");
                    info.AppendLine($"User: {Environment.UserName}");
                    info.AppendLine($"Processors: {Environment.ProcessorCount}");
                    info.AppendLine($"Working Set: {Environment.WorkingSet / (1024 * 1024)} MB");
                    
                    // Get CPU usage
                    using var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    pc.NextValue();
                    System.Threading.Thread.Sleep(100);
                    var cpuUsage = pc.NextValue();
                    info.AppendLine($"CPU Usage: {cpuUsage:F1}%");
                    
                    // Get memory info
                    var totalMemory = GC.GetTotalMemory(false) / (1024 * 1024);
                    info.AppendLine($"GC Memory: {totalMemory} MB");
                    
                    return info.ToString();
                }
                catch (Exception ex)
                {
                    return $"❌ System info failed: {ex.Message}";
                }
            });
        }

        private async Task<string> ListProcessesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var processes = Process.GetProcesses()
                        .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                        .OrderBy(p => p.ProcessName)
                        .Take(20);
                    
                    var info = new StringBuilder();
                    info.AppendLine("📋 Running Processes (Top 20):");
                    
                    foreach (var process in processes)
                    {
                        try
                        {
                            var memory = process.WorkingSet64 / (1024 * 1024);
                            info.AppendLine($"• {process.ProcessName} (PID: {process.Id}, Memory: {memory} MB)");
                        }
                        catch
                        {
                            info.AppendLine($"• {process.ProcessName} (PID: {process.Id})");
                        }
                    }
                    
                    return info.ToString();
                }
                catch (Exception ex)
                {
                    return $"❌ List processes failed: {ex.Message}";
                }
            });
        }

        private async Task<string> StartProcessAsync(Dictionary<string, object> parameters)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!parameters.TryGetValue("name", out var processName))
                        return "❌ Process name not specified";

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = processName.ToString(),
                        UseShellExecute = true
                    };

                    if (parameters.TryGetValue("arguments", out var args))
                        startInfo.Arguments = args.ToString();

                    var process = Process.Start(startInfo);
                    return $"✅ Started process: {processName} (PID: {process?.Id})";
                }
                catch (Exception ex)
                {
                    return $"❌ Start process failed: {ex.Message}";
                }
            });
        }

        private async Task<string> CloseProcessAsync(Dictionary<string, object> parameters)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!parameters.TryGetValue("name", out var processName))
                        return "❌ Process name not specified";

                    var processes = Process.GetProcessesByName(processName.ToString());
                    if (processes.Length == 0)
                        return $"❌ No process found with name: {processName}";

                    var closedCount = 0;
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.CloseMainWindow();
                            if (!process.WaitForExit(5000))
                                process.Kill();
                            closedCount++;
                        }
                        catch { }
                    }

                    return $"✅ Closed {closedCount} instance(s) of {processName}";
                }
                catch (Exception ex)
                {
                    return $"❌ Close process failed: {ex.Message}";
                }
            });
        }

        private async Task<string> GetDiskUsageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var info = new StringBuilder();
                    info.AppendLine("💾 Disk Usage:");
                    
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (drive.IsReady)
                        {
                            var totalGB = drive.TotalSize / (1024 * 1024 * 1024);
                            var freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                            var usedGB = totalGB - freeGB;
                            var usagePercent = (double)usedGB / totalGB * 100;
                            
                            info.AppendLine($"Drive {drive.Name}: {usedGB}GB / {totalGB}GB ({usagePercent:F1}% used)");
                        }
                    }
                    
                    return info.ToString();
                }
                catch (Exception ex)
                {
                    return $"❌ Disk usage failed: {ex.Message}";
                }
            });
        }

        private async Task<string> GetNetworkInfoAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var info = new StringBuilder();
                    info.AppendLine("🌐 Network Information:");
                    info.AppendLine($"Machine Name: {Environment.MachineName}");
                    info.AppendLine($"Domain: {Environment.UserDomainName}");
                    
                    // Test internet connectivity
                    try
                    {
                        using var ping = new System.Net.NetworkInformation.Ping();
                        var reply = ping.Send("8.8.8.8", 3000);
                        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        {
                            info.AppendLine($"Internet: ✅ Connected (Ping: {reply.RoundtripTime}ms)");
                        }
                        else
                        {
                            info.AppendLine("Internet: ❌ Not connected");
                        }
                    }
                    catch
                    {
                        info.AppendLine("Internet: ❓ Unable to test");
                    }
                    
                    return info.ToString();
                }
                catch (Exception ex)
                {
                    return $"❌ Network info failed: {ex.Message}";
                }
            });
        }
    }
}
