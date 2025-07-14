using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AIPal.Services.AgentTools
{
    /// <summary>
    /// Provides security and malware detection tools with simple explanations for users.
    /// </summary>
    public class SecurityTools
    {
        // Tool definitions for security and malware detection
        public static readonly List<AgentTool> Tools = new List<AgentTool>
        {
            new AgentTool
            {
                Name = "check_for_malware",
                Description = "Check the system for signs of malware and provide simple explanations",
                Parameters = new Dictionary<string, object>
                {
                    ["detailed"] = new Dictionary<string, object>
                    {
                        ["type"] = "boolean",
                        ["description"] = "Whether to perform a more detailed check"
                    }
                },
                Handler = CheckForMalware
            },
            
            new AgentTool
            {
                Name = "optimize_system",
                Description = "Close unnecessary applications to improve system performance",
                Parameters = new Dictionary<string, object>
                {
                    ["aggressive"] = new Dictionary<string, object>
                    {
                        ["type"] = "boolean",
                        ["description"] = "Whether to be more aggressive in closing applications"
                    }
                },
                Handler = OptimizeSystem
            },
            
            new AgentTool
            {
                Name = "explain_security_alert",
                Description = "Explain a security alert or popup in simple terms",
                Parameters = new Dictionary<string, object>
                {
                    ["alert_text"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "The text of the security alert or popup"
                    }
                },
                Handler = ExplainSecurityAlert
            }
        };

        /// <summary>
        /// Checks the system for signs of malware and provides simple explanations.
        /// </summary>
        private static async Task<string> CheckForMalware(JsonElement parameters)
        {
            bool detailed = parameters.TryGetProperty("detailed", out var detailedProperty) && detailedProperty.GetBoolean();
            
            try
            {
                var result = new StringBuilder();
                result.AppendLine("# Security Check Results\n");
                result.AppendLine("I'm checking your computer for common signs of security problems. This is NOT a complete virus scan, but it can help identify obvious issues.\n");
                
                // Track suspicious findings
                var suspiciousFindings = new List<string>();
                
                // Check Windows Defender status
                result.AppendLine("## Antivirus Protection\n");
                bool defenderEnabled = false;
                bool realtimeProtectionEnabled = false;
                
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender"))
                    {
                        if (key != null)
                        {
                            defenderEnabled = (int)key.GetValue("ProductStatus", 0) > 0;
                        }
                    }
                    
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection"))
                    {
                        if (key != null)
                        {
                            realtimeProtectionEnabled = (int)key.GetValue("DisableRealtimeMonitoring", 1) == 0;
                        }
                    }
                    
                    if (defenderEnabled && realtimeProtectionEnabled)
                    {
                        result.AppendLine("✅ Windows Defender is active and protecting your computer.");
                    }
                    else
                    {
                        result.AppendLine("⚠️ Windows Defender may not be fully protecting your computer.");
                        suspiciousFindings.Add("Windows Defender protection is not fully enabled");
                    }
                }
                catch
                {
                    result.AppendLine("❓ Could not check Windows Defender status.");
                }
                
                // Check for suspicious startup items
                result.AppendLine("\n## Startup Programs\n");
                var suspiciousStartupItems = new List<string>();
                
                try
                {
                    // Common startup locations
                    var startupLocations = new Dictionary<string, string>
                    {
                        { "HKLM Run", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run" },
                        { "HKCU Run", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run" },
                        { "HKLM RunOnce", @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" },
                        { "HKCU RunOnce", @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" }
                    };
                    
                    foreach (var location in startupLocations)
                    {
                        RegistryKey key = null;
                        
                        try
                        {
                            if (location.Key.StartsWith("HKLM"))
                            {
                                key = Registry.LocalMachine.OpenSubKey(location.Value);
                            }
                            else
                            {
                                key = Registry.CurrentUser.OpenSubKey(location.Value);
                            }
                            
                            if (key != null)
                            {
                                foreach (var valueName in key.GetValueNames())
                                {
                                    string path = key.GetValue(valueName).ToString();
                                    
                                    // Check for suspicious characteristics
                                    if (IsSuspiciousPath(path))
                                    {
                                        suspiciousStartupItems.Add($"{valueName} ({path})");
                                    }
                                }
                            }
                        }
                        finally
                        {
                            key?.Dispose();
                        }
                    }
                    
                    // Check Startup folder
                    string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                    foreach (var file in Directory.GetFiles(startupFolder))
                    {
                        if (IsSuspiciousPath(file))
                        {
                            suspiciousStartupItems.Add(file);
                        }
                    }
                    
                    if (suspiciousStartupItems.Count > 0)
                    {
                        result.AppendLine("⚠️ Found potentially suspicious startup items:");
                        foreach (var item in suspiciousStartupItems)
                        {
                            result.AppendLine($"  - {item}");
                        }
                        suspiciousFindings.Add("Suspicious startup items detected");
                    }
                    else
                    {
                        result.AppendLine("✅ No obviously suspicious startup programs found.");
                    }
                }
                catch (Exception ex)
                {
                    result.AppendLine($"❓ Could not check startup items: {ex.Message}");
                }
                
                // Check for suspicious processes
                result.AppendLine("\n## Running Programs\n");
                var suspiciousProcesses = new List<string>();
                
                try
                {
                    var processes = Process.GetProcesses();
                    
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (process.MainModule != null)
                            {
                                string path = process.MainModule.FileName;
                                
                                if (IsSuspiciousPath(path))
                                {
                                    suspiciousProcesses.Add($"{process.ProcessName} ({path})");
                                }
                            }
                        }
                        catch
                        {
                            // Skip processes we can't access
                        }
                    }
                    
                    if (suspiciousProcesses.Count > 0)
                    {
                        result.AppendLine("⚠️ Found potentially suspicious running programs:");
                        foreach (var proc in suspiciousProcesses.Take(5)) // Limit to 5 to avoid overwhelming
                        {
                            result.AppendLine($"  - {proc}");
                        }
                        
                        if (suspiciousProcesses.Count > 5)
                        {
                            result.AppendLine($"  - ... and {suspiciousProcesses.Count - 5} more");
                        }
                        
                        suspiciousFindings.Add("Suspicious processes detected");
                    }
                    else
                    {
                        result.AppendLine("✅ No obviously suspicious programs currently running.");
                    }
                }
                catch (Exception ex)
                {
                    result.AppendLine($"❓ Could not check running programs: {ex.Message}");
                }
                
                // Check browser extensions if detailed check requested
                if (detailed)
                {
                    result.AppendLine("\n## Browser Extensions\n");
                    result.AppendLine("For a complete check, you should manually review your browser extensions:");
                    result.AppendLine("1. In Chrome: type 'chrome://extensions' in the address bar");
                    result.AppendLine("2. In Edge: type 'edge://extensions' in the address bar");
                    result.AppendLine("3. In Firefox: click the menu button, then 'Add-ons and themes'");
                    result.AppendLine("\nRemove any extensions you don't recognize or no longer use.");
                }
                
                // Provide a summary and recommendations
                result.AppendLine("\n## Summary and Recommendations\n");
                
                if (suspiciousFindings.Count > 0)
                {
                    result.AppendLine("⚠️ **Potential security concerns were found:**");
                    foreach (var finding in suspiciousFindings)
                    {
                        result.AppendLine($"  - {finding}");
                    }
                    
                    result.AppendLine("\n**What to do next:**");
                    result.AppendLine("1. Run a full scan with Windows Defender or your antivirus program");
                    result.AppendLine("2. If you're concerned about a scam or virus, DO NOT call phone numbers from popups");
                    result.AppendLine("3. Never give remote access to your computer to someone who contacted you first");
                    result.AppendLine("4. If you need help, ask a trusted family member or local computer repair shop");
                }
                else
                {
                    result.AppendLine("✅ **Good news! No obvious security issues were found.**");
                    result.AppendLine("\nRemember:");
                    result.AppendLine("- Keep Windows and your programs updated");
                    result.AppendLine("- Never share passwords or personal information with strangers");
                    result.AppendLine("- Be cautious of unexpected popups saying your computer has a virus");
                    result.AppendLine("- Legitimate tech companies won't call you about viruses on your computer");
                }
                
                result.AppendLine("\n## Important Safety Tips\n");
                result.AppendLine("**Common Scam Warning Signs:**");
                result.AppendLine("- Popups saying your computer is infected and to call a number");
                result.AppendLine("- Someone calling you claiming to be from Microsoft, Windows, or Apple");
                result.AppendLine("- Anyone asking for payment in gift cards or cryptocurrency");
                result.AppendLine("- Anyone asking for remote access to your computer");
                result.AppendLine("- Threats that your computer will be damaged if you don't act immediately");
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error checking for malware: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Checks if a file path has suspicious characteristics.
        /// </summary>
        private static bool IsSuspiciousPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
                
            path = path.ToLower();
            
            // Check for suspicious locations
            if (path.Contains("temp\\") || 
                path.Contains("temporary internet files") ||
                path.Contains("appdata\\local\\temp"))
            {
                return true;
            }
            
            // Check for suspicious file characteristics
            if (path.Contains("random") ||
                path.Contains("temp") && path.EndsWith(".exe") ||
                path.Contains("update") && !path.Contains("windows") && path.EndsWith(".exe") ||
                path.Contains("\\%") ||
                path.Contains("\\~"))
            {
                return true;
            }
            
            // Check for unusual or obfuscated names
            if (Path.GetFileName(path).Length > 20 && Path.GetFileName(path).Contains("_") ||
                Guid.TryParse(Path.GetFileNameWithoutExtension(path), out _))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Closes unnecessary applications to improve system performance.
        /// </summary>
        private static async Task<string> OptimizeSystem(JsonElement parameters)
        {
            bool aggressive = parameters.TryGetProperty("aggressive", out var aggressiveProperty) && aggressiveProperty.GetBoolean();
            
            try
            {
                var result = new StringBuilder();
                result.AppendLine("# System Optimization\n");
                
                // Get all running processes
                var allProcesses = Process.GetProcesses();
                
                // Get processes sorted by memory usage
                var processesByMemory = allProcesses
                    .Where(p => !IsSystemCriticalProcess(p.ProcessName))
                    .Select(p => new 
                    {
                        Process = p,
                        Name = p.ProcessName,
                        MemoryMB = p.WorkingSet64 / 1024 / 1024,
                        IsBackground = IsLikelyBackgroundProcess(p.ProcessName)
                    })
                    .Where(p => p.MemoryMB > 50) // Only consider processes using significant memory
                    .OrderByDescending(p => p.MemoryMB)
                    .ToList();
                
                // Identify processes that can be safely closed
                var processesToClose = new List<(Process Process, string Name, long MemoryMB)>();
                var backgroundProcesses = new List<(Process Process, string Name, long MemoryMB)>();
                
                foreach (var proc in processesByMemory)
                {
                    if (proc.IsBackground)
                    {
                        // Background processes can be closed more aggressively
                        backgroundProcesses.Add((proc.Process, proc.Name, proc.MemoryMB));
                    }
                    else if (IsSafeToClose(proc.Name) || (aggressive && IsLikelySafeToClose(proc.Name)))
                    {
                        // User-facing processes that are safe to close
                        processesToClose.Add((proc.Process, proc.Name, proc.MemoryMB));
                    }
                }
                
                // Report on memory usage before optimization
                long totalMemoryBefore = allProcesses.Sum(p => p.WorkingSet64);
                result.AppendLine($"Current memory usage: {totalMemoryBefore / 1024 / 1024} MB\n");
                
                // Close background processes first
                result.AppendLine("## Closing Background Processes\n");
                
                if (backgroundProcesses.Count > 0)
                {
                    long memoryFreed = 0;
                    int closedCount = 0;
                    
                    foreach (var proc in backgroundProcesses)
                    {
                        try
                        {
                            proc.Process.CloseMainWindow();
                            await Task.Delay(100); // Give it a moment to close gracefully
                            
                            if (!proc.Process.HasExited)
                            {
                                proc.Process.Kill();
                            }
                            
                            memoryFreed += proc.MemoryMB;
                            closedCount++;
                            
                            result.AppendLine($"✅ Closed: {proc.Name} (freed {proc.MemoryMB} MB)");
                        }
                        catch (Exception ex)
                        {
                            result.AppendLine($"❌ Could not close {proc.Name}: {ex.Message}");
                        }
                    }
                    
                    result.AppendLine($"\nClosed {closedCount} background processes, freeing approximately {memoryFreed} MB of memory.");
                }
                else
                {
                    result.AppendLine("No background processes found that are safe to close.");
                }
                
                // Close user-facing applications if requested
                if (aggressive)
                {
                    result.AppendLine("\n## Closing Non-Essential Applications\n");
                    
                    if (processesToClose.Count > 0)
                    {
                        long memoryFreed = 0;
                        int closedCount = 0;
                        
                        foreach (var proc in processesToClose)
                        {
                            try
                            {
                                proc.Process.CloseMainWindow();
                                await Task.Delay(500); // Give it more time to close gracefully
                                
                                if (!proc.Process.HasExited)
                                {
                                    // Don't force-kill user applications
                                    result.AppendLine($"⚠️ {proc.Name} didn't close immediately (waiting for it to save data)");
                                    continue;
                                }
                                
                                memoryFreed += proc.MemoryMB;
                                closedCount++;
                                
                                result.AppendLine($"✅ Closed: {proc.Name} (freed {proc.MemoryMB} MB)");
                            }
                            catch (Exception ex)
                            {
                                result.AppendLine($"❌ Could not close {proc.Name}: {ex.Message}");
                            }
                        }
                        
                        result.AppendLine($"\nClosed {closedCount} applications, freeing approximately {memoryFreed} MB of memory.");
                    }
                    else
                    {
                        result.AppendLine("No non-essential applications found that are safe to close.");
                    }
                }
                
                // Run garbage collection to free memory
                result.AppendLine("\n## Cleaning Up System Memory\n");
                
                try
                {
                    // Run garbage collection
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    
                    result.AppendLine("✅ Cleaned up system memory");
                    
                    // Get memory usage after optimization
                    var processesAfter = Process.GetProcesses();
                    long totalMemoryAfter = processesAfter.Sum(p => p.WorkingSet64);
                    long memorySaved = totalMemoryBefore - totalMemoryAfter;
                    
                    result.AppendLine($"\nMemory usage before: {totalMemoryBefore / 1024 / 1024} MB");
                    result.AppendLine($"Memory usage after: {totalMemoryAfter / 1024 / 1024} MB");
                    result.AppendLine($"Memory saved: {memorySaved / 1024 / 1024} MB");
                }
                catch (Exception ex)
                {
                    result.AppendLine($"❌ Error cleaning up memory: {ex.Message}");
                }
                
                // Additional recommendations
                result.AppendLine("\n## Additional Recommendations\n");
                result.AppendLine("To further improve performance:");
                result.AppendLine("1. Restart your computer regularly (at least once a week)");
                result.AppendLine("2. Uninstall programs you don't use (Control Panel > Programs > Uninstall a program)");
                result.AppendLine("3. Clear browser cache and temporary files regularly");
                result.AppendLine("4. Consider upgrading your RAM if your computer is consistently slow");
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error optimizing system: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Determines if a process is a critical system process that should never be closed.
        /// </summary>
        private static bool IsSystemCriticalProcess(string processName)
        {
            string[] criticalProcesses = 
            {
                "system", "smss", "csrss", "wininit", "services", "lsass", "winlogon", 
                "explorer", "dwm", "taskmgr", "svchost", "spoolsv", "ntoskrnl", "aipal"
            };
            
            return criticalProcesses.Any(p => processName.ToLower().Contains(p));
        }
        
        /// <summary>
        /// Determines if a process is likely a background process that can be closed more aggressively.
        /// </summary>
        private static bool IsLikelyBackgroundProcess(string processName)
        {
            string[] backgroundKeywords = 
            {
                "updater", "helper", "agent", "service", "tray", "background", "daemon", 
                "sync", "scheduler", "monitor", "notif", "assistant", "launcher"
            };
            
            string lowerName = processName.ToLower();
            
            return backgroundKeywords.Any(k => lowerName.Contains(k)) ||
                   lowerName.EndsWith("bg") ||
                   lowerName.EndsWith("svc");
        }
        
        /// <summary>
        /// Determines if a process is definitely safe to close.
        /// </summary>
        private static bool IsSafeToClose(string processName)
        {
            string[] safeToCloseProcesses = 
            {
                "calculator", "notepad", "paint", "wordpad", "snippingtool",
                "mspaint", "stickynot", "photos", "reader", "acrobat", 
                "vlc", "mediaplayer", "groove", "zune"
            };
            
            return safeToCloseProcesses.Any(p => processName.ToLower().Contains(p));
        }
        
        /// <summary>
        /// Determines if a process is likely safe to close (used in aggressive mode).
        /// </summary>
        private static bool IsLikelySafeToClose(string processName)
        {
            string lowerName = processName.ToLower();
            
            // Common applications that are generally safe to close
            if (lowerName.Contains("game") || 
                lowerName.Contains("player") ||
                lowerName.Contains("media") ||
                lowerName.Contains("music") ||
                lowerName.Contains("photo") ||
                lowerName.Contains("viewer") ||
                lowerName.Contains("editor"))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Explains a security alert or popup in simple terms.
        /// </summary>
        private static async Task<string> ExplainSecurityAlert(JsonElement parameters)
        {
            string alertText = parameters.GetProperty("alert_text").GetString() ?? string.Empty;
            
            try
            {
                var result = new StringBuilder();
                result.AppendLine("# Security Alert Explanation\n");
                
                // Common scam alert patterns
                bool isLikelyScam = false;
                
                if (alertText.Contains("call") && 
                    (alertText.Contains("technician") || 
                     alertText.Contains("support") || 
                     alertText.Contains("microsoft") || 
                     alertText.Contains("apple") || 
                     alertText.Contains("windows")))
                {
                    isLikelyScam = true;
                }
                
                if (alertText.Contains("virus") || 
                    alertText.Contains("infected") || 
                    alertText.Contains("malware") || 
                    alertText.Contains("trojan") || 
                    alertText.Contains("hacked"))
                {
                    if (alertText.Contains("call") || 
                        alertText.Contains("phone") || 
                        alertText.Contains("support") || 
                        alertText.Contains("technician"))
                    {
                        isLikelyScam = true;
                    }
                }
                
                if (isLikelyScam)
                {
                    result.AppendLine("## ⚠️ THIS IS LIKELY A SCAM! ⚠️\n");
                    result.AppendLine("This appears to be a fake security alert designed to trick you into:");
                    result.AppendLine("- Calling a fake support number");
                    result.AppendLine("- Paying money to fix a non-existent problem");
                    result.AppendLine("- Giving someone remote access to your computer");
                    
                    result.AppendLine("\n**What to do:**");
                    result.AppendLine("1. DO NOT call any phone numbers shown in the alert");
                    result.AppendLine("2. DO NOT click any buttons in the popup");
                    result.AppendLine("3. Try to close the window or browser tab");
                    result.AppendLine("4. If you can't close it, restart your computer");
                    result.AppendLine("5. Run a scan with Windows Defender or your antivirus after restarting");
                    
                    result.AppendLine("\n**Remember:**");
                    result.AppendLine("- Microsoft, Apple, or other tech companies will NEVER show popups asking you to call them");
                    result.AppendLine("- Real virus alerts don't have phone numbers to call");
                    result.AppendLine("- Never give remote access to your computer to someone who contacted you first");
                }
                else if (alertText.Contains("windows defender") || 
                         alertText.Contains("virus") || 
                         alertText.Contains("threat") || 
                         alertText.Contains("protection"))
                {
                    result.AppendLine("## Windows Security Alert\n");
                    result.AppendLine("This appears to be a notification from Windows Defender or your antivirus software.");
                    
                    result.AppendLine("\n**What to do:**");
                    result.AppendLine("1. Read the alert carefully - what file or program is it warning about?");
                    result.AppendLine("2. If it's a file you just downloaded or a program you just installed:");
                    result.AppendLine("   - If you don't recognize it or didn't intend to download it, let Windows remove it");
                    result.AppendLine("   - If it's something you meant to download from a trusted source, it might be a false alarm");
                    result.AppendLine("3. You can usually click on the notification to see more details");
                    
                    result.AppendLine("\n**Is it real or fake?**");
                    result.AppendLine("Real Windows security alerts:");
                    result.AppendLine("- Usually appear in the bottom right corner as notifications");
                    result.AppendLine("- Don't have phone numbers to call");
                    result.AppendLine("- Don't use scary language or countdown timers");
                    result.AppendLine("- Allow you to see details about the threat");
                }
                else if (alertText.Contains("update") || 
                         alertText.Contains("upgrade") || 
                         alertText.Contains("install"))
                {
                    result.AppendLine("## Software Update Alert\n");
                    result.AppendLine("This appears to be a notification about updating software.");
                    
                    result.AppendLine("\n**What to do:**");
                    result.AppendLine("1. Check which program is asking for the update");
                    result.AppendLine("2. Updates for Windows and legitimate programs are generally good to install");
                    result.AppendLine("3. If you don't recognize the program, be cautious");
                    
                    result.AppendLine("\n**Is it real or fake?**");
                    result.AppendLine("Real update notifications:");
                    result.AppendLine("- Come from programs you recognize");
                    result.AppendLine("- Don't use alarming language");
                    result.AppendLine("- Don't redirect you to websites to download updates");
                    result.AppendLine("- Windows updates come through Settings or Windows Update");
                }
                else
                {
                    result.AppendLine("## Alert Analysis\n");
                    result.AppendLine("I'm not sure exactly what type of alert this is based on the text provided.");
                    
                    result.AppendLine("\n**General Safety Advice:**");
                    result.AppendLine("1. Be skeptical of any alert that creates a sense of urgency or fear");
                    result.AppendLine("2. Never call phone numbers from popups or alerts");
                    result.AppendLine("3. Don't click on buttons in popups from websites you don't trust");
                    result.AppendLine("4. If unsure, restart your computer and run a scan with Windows Defender");
                    
                    result.AppendLine("\n**Common Scam Warning Signs:**");
                    result.AppendLine("- Alerts saying your computer is infected and to call a number");
                    result.AppendLine("- Countdown timers creating urgency");
                    result.AppendLine("- Claims that your personal data or files are at risk");
                    result.AppendLine("- Poor grammar or spelling errors");
                    result.AppendLine("- Requests for payment to fix problems");
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Error explaining security alert: {ex.Message}";
            }
        }
    }
}
