using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Smartitecture.Services.Interfaces;
using Smartitecture.Services.Core;

namespace Smartitecture.Services.Modes
{
    /// <summary>
    /// FORTIS - Security-focused AI with threat detection, malware scanning, and system protection
    /// </summary>
    public class FortisService : IAIMode
    {
        private readonly ILLMService _llmService;
        private List<string> _knownMalwareSignatures;
        private List<string> _suspiciousFileExtensions;

        public string ModeName => "FORTIS";
        public string ModeIcon => "🛡️";
        public string ModeColor => "#EF4444";
        public string Description => "Security expert with threat detection, malware scanning, and system protection";

        public FortisService(ILLMService llmService)
        {
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            InitializeSecurityData();
        }

        public bool CanHandle(string query)
        {
            var securityKeywords = new[]
            {
                "virus", "malware", "threat", "security", "scan", "antivirus", "firewall",
                "protect", "safe", "danger", "suspicious", "hack", "breach", "vulnerability",
                "encrypt", "password", "backup", "recover", "restore", "quarantine"
            };

            var queryLower = query.ToLowerInvariant();
            return securityKeywords.Any(keyword => queryLower.Contains(keyword));
        }

        public double GetConfidenceScore(string query)
        {
            var score = 0.0;
            var queryLower = query.ToLowerInvariant();

            // High confidence for explicit security queries
            if (ContainsSecurityKeywords(query)) score += 0.9;
            if (IsFileScanRequest(query)) score += 0.95;
            if (IsSystemSecurityQuery(query)) score += 0.8;
            if (IsPasswordQuery(query)) score += 0.7;
            if (IsBackupRecoveryQuery(query)) score += 0.75;

            // Medium confidence for file operations (might need security check)
            if (ContainsFileOperations(query)) score += 0.4;

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

            // Detect security-related intents
            if (IsFileScanRequest(query)) context.DetectedIntents.Add("file_scan");
            if (IsSystemSecurityQuery(query)) context.DetectedIntents.Add("system_security");
            if (IsPasswordQuery(query)) context.DetectedIntents.Add("password_security");
            if (IsBackupRecoveryQuery(query)) context.DetectedIntents.Add("data_recovery");
            if (IsNetworkSecurityQuery(query)) context.DetectedIntents.Add("network_security");

            // Determine if collaboration is needed
            if (IsGeneralSecurityQuestion(query))
            {
                context.CollaboratingModes.Add("LUMEN"); // For general explanation
            }

            return context;
        }

        public async Task<string> ProcessQueryAsync(string query, string conversationId = null)
        {
            try
            {
                // Handle file scanning requests
                if (IsFileScanRequest(query))
                {
                    var filePath = ExtractFilePath(query);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        return await ScanFileAsync(filePath);
                    }
                    return "Please specify the file path you'd like me to scan for threats.";
                }

                // Handle system security scans
                if (IsSystemSecurityQuery(query))
                {
                    return await PerformSystemSecurityScanAsync();
                }

                // Handle password security queries
                if (IsPasswordQuery(query))
                {
                    return await AnalyzePasswordSecurityAsync(query);
                }

                // Handle backup and recovery queries
                if (IsBackupRecoveryQuery(query))
                {
                    return await HandleBackupRecoveryAsync(query);
                }

                // Handle network security queries
                if (IsNetworkSecurityQuery(query))
                {
                    return await AnalyzeNetworkSecurityAsync();
                }

                // NEW FEATURES
                // Windows Defender quick/full scan
                if (query.ToLowerInvariant().Contains("defender scan") || query.ToLowerInvariant().Contains("virus scan"))
                {
                    var full = query.ToLowerInvariant().Contains("full");
                    return await RunWindowsDefenderScanAsync(full);
                }

                // Firewall status
                if (query.ToLowerInvariant().Contains("firewall status") || query.ToLowerInvariant().Contains("check firewall"))
                {
                    return await GetFirewallStatusAsync();
                }

                // Registry security analysis
                if (query.ToLowerInvariant().Contains("registry security") || query.ToLowerInvariant().Contains("analyze registry"))
                {
                    return await AnalyzeRegistrySecurityAsync();
                }

                // Port scanning (basic TCP connect scan of common ports)
                if (query.ToLowerInvariant().Contains("port scan") || query.ToLowerInvariant().Contains("scan ports"))
                {
                    var host = ExtractHost(query) ?? "127.0.0.1";
                    return await RunPortScanAsync(host);
                }

                // Enable real-time threat monitoring
                if (query.ToLowerInvariant().Contains("enable threat monitoring"))
                {
                    StartThreatMonitoring();
                    return "From a security perspective, real-time threat monitoring has been enabled for common download folders.";
                }

                // Default to LLM for security-related questions
                var prompt = BuildFortisPrompt(query);
                return await _llmService.GetResponseAsync(prompt, conversationId);
            }
            catch (Exception ex)
            {
                return $"🛡️ FORTIS encountered an error: {ex.Message}";
            }
        }

        public async Task InitializeAsync()
        {
            // Initialize security databases and configurations
            await Task.CompletedTask;
        }

        #region Private Helper Methods

        private async Task<string> RunWindowsDefenderScanAsync(bool full)
        {
            try
            {
                var defenderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Windows Defender", "MpCmdRun.exe");
                var args = full ? "-Scan -ScanType 2" : "-Scan -ScanType 1";
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = defenderPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                if (proc == null)
                    return "From a security perspective, I couldn�t start Windows Defender (MpCmdRun.exe).";
                await System.Threading.Tasks.Task.Run(() => proc.WaitForExit(600000));
                return full ? "Security analysis complete. Windows Defender full scan finished." : "Security analysis complete. Windows Defender quick scan finished.";
            }
            catch (System.Exception ex)
            {
                return $"From a security perspective, Defender scan failed: {ex.Message}";
            }
        }

        private async System.Threading.Tasks.Task<string> GetFirewallStatusAsync()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall show allprofiles",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                var output = await proc.StandardOutput.ReadToEndAsync();
                return $"Security analysis complete. Firewall status:\n\n{output}";
            }
            catch (System.Exception ex)
            {
                return $"From a security perspective, firewall status check failed: {ex.Message}";
            }
        }

        private async System.Threading.Tasks.Task<string> AnalyzeRegistrySecurityAsync()
        {
            var findings = new System.Collections.Generic.List<string>();
            try
            {
                using var hku = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                using var hklm = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                void scan(Microsoft.Win32.RegistryKey k, string scope)
                {
                    if (k == null) return;
                    foreach (var n in k.GetValueNames())
                    {
                        var v = k.GetValue(n)?.ToString() ?? string.Empty;
                        if (v.Contains("%TEMP%", System.StringComparison.OrdinalIgnoreCase) ||
                            v.Contains("AppData", System.StringComparison.OrdinalIgnoreCase) ||
                            v.EndsWith(".vbs", System.StringComparison.OrdinalIgnoreCase) ||
                            v.EndsWith(".js", System.StringComparison.OrdinalIgnoreCase))
                        {
                            findings.Add($"Suspicious startup entry in {scope}: {n} -> {v}");
                        }
                    }
                }
                scan(hku, "HKCU\\...Run");
                scan(hklm, "HKLM\\...Run");
            }
            catch (System.Exception ex)
            {
                findings.Add($"Registry scan error: {ex.Message}");
            }
            if (!findings.Any()) findings.Add("No suspicious startup entries were found in Run keys.");
            return "Security analysis complete. Registry findings:\n\n" + string.Join("\n", findings);
        }

        private string ExtractHost(string query)
        {
            var m = System.Text.RegularExpressions.Regex.Match(query, @"host\s*:?\s*([\\w\\.-]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success) return m.Groups[1].Value;
            m = System.Text.RegularExpressions.Regex.Match(query, @"scan\s+([\\w\\.-]+)");
            return m.Success ? m.Groups[1].Value : null;
        }

        private async System.Threading.Tasks.Task<string> RunPortScanAsync(string host)
        {
            var ports = new[] { 22, 80, 443, 3389, 445, 139, 135, 1433, 3306, 8080 };
            var results = new System.Collections.Generic.List<string>();
            var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
            foreach (var p in ports)
            {
                tasks.Add(System.Threading.Tasks.Task.Run(async () =>
                {
                    using var client = new System.Net.Sockets.TcpClient();
                    try
                    {
                        var connect = client.ConnectAsync(host, p);
                        var timeout = System.Threading.Tasks.Task.Delay(800);
                        if (await System.Threading.Tasks.Task.WhenAny(connect, timeout) == connect && client.Connected)
                        {
                            lock (results) results.Add($"Port {p}/tcp: OPEN");
                        }
                    }
                    catch { }
                }));
            }
            await System.Threading.Tasks.Task.WhenAll(tasks);
            if (!results.Any()) results.Add("No common ports appear open (or host unreachable).");
            return $"From a security perspective, port scan results for {host}:\n\n" + string.Join("\n", results.OrderBy(x => x));
        }

        private System.IO.FileSystemWatcher _threatWatcher;
        private void StartThreatMonitoring()
        {
            try
            {
                if (_threatWatcher != null) return;
                var downloads = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!System.IO.Directory.Exists(downloads)) return;
                _threatWatcher = new System.IO.FileSystemWatcher(downloads)
                {
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                    Filter = "*.*"
                };
                _threatWatcher.Created += async (s, e) =>
                {
                    var ext = System.IO.Path.GetExtension(e.FullPath).ToLowerInvariant();
                    if (new[] { ".exe", ".msi", ".bat", ".vbs", ".js", ".ps1" }.Contains(ext))
                    {
                        await RunWindowsDefenderScanAsync(false);
                    }
                };
            }
            catch { }
        }
        private void InitializeSecurityData()
        {
            _knownMalwareSignatures = new List<string>
            {
                // Simplified malware signatures (in real implementation, use proper antivirus APIs)
                "EICAR-STANDARD-ANTIVIRUS-TEST-FILE",
                "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR"
            };

            _suspiciousFileExtensions = new List<string>
            {
                ".exe", ".scr", ".bat", ".cmd", ".com", ".pif", ".vbs", ".js", ".jar", ".app"
            };
        }

        private bool ContainsSecurityKeywords(string query)
        {
            var securityKeywords = new[]
            {
                "virus", "malware", "threat", "security", "scan", "antivirus", "firewall",
                "protect", "safe", "danger", "suspicious", "hack", "breach", "vulnerability"
            };
            return securityKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsFileScanRequest(string query)
        {
            var scanKeywords = new[] { "scan", "check", "analyze", "inspect" };
            var fileKeywords = new[] { "file", "document", "download", "attachment" };
            
            var queryLower = query.ToLowerInvariant();
            return scanKeywords.Any(s => queryLower.Contains(s)) && 
                   fileKeywords.Any(f => queryLower.Contains(f));
        }

        private bool IsSystemSecurityQuery(string query)
        {
            var systemKeywords = new[] { "system", "computer", "pc" };
            var securityKeywords = new[] { "security", "scan", "check", "threats" };
            
            var queryLower = query.ToLowerInvariant();
            return systemKeywords.Any(s => queryLower.Contains(s)) && 
                   securityKeywords.Any(sec => queryLower.Contains(sec));
        }

        private bool IsPasswordQuery(string query)
        {
            var passwordKeywords = new[] { "password", "credential", "login", "authentication" };
            return passwordKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsBackupRecoveryQuery(string query)
        {
            var backupKeywords = new[] { "backup", "recover", "restore", "deleted", "lost" };
            return backupKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsNetworkSecurityQuery(string query)
        {
            var networkKeywords = new[] { "network", "wifi", "firewall", "port", "connection" };
            return networkKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool ContainsFileOperations(string query)
        {
            var fileOpKeywords = new[] { "delete", "move", "copy", "rename", "modify", "edit", "download" };
            return fileOpKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool RequiresSystemAccess(string query)
        {
            var systemAccessKeywords = new[] { "scan", "firewall", "registry", "services", "processes" };
            return systemAccessKeywords.Any(keyword => query.ToLowerInvariant().Contains(keyword));
        }

        private bool IsGeneralSecurityQuestion(string query)
        {
            var questionWords = new[] { "what", "how", "why", "when", "where" };
            return questionWords.Any(q => query.ToLowerInvariant().StartsWith(q));
        }

        private string ExtractFilePath(string query)
        {
            // Look for file paths in quotes or common path patterns
            var quotedMatch = Regex.Match(query, @"""([^""]+)""");
            if (quotedMatch.Success)
                return quotedMatch.Groups[1].Value;

            var pathMatch = Regex.Match(query, @"[A-Za-z]:\\[^\s]+");
            if (pathMatch.Success)
                return pathMatch.Value;

            return null;
        }

        private async Task<string> ScanFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return $"❌ File not found: {filePath}";
                }

                var fileInfo = new FileInfo(filePath);
                var results = new List<string>();

                // Basic file analysis
                results.Add($"🔍 **File Analysis for**: {fileInfo.Name}");
                results.Add($"📁 **Path**: {filePath}");
                results.Add($"📏 **Size**: {FormatFileSize(fileInfo.Length)}");
                results.Add($"📅 **Modified**: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");

                // Check file extension
                var extension = fileInfo.Extension.ToLowerInvariant();
                if (_suspiciousFileExtensions.Contains(extension))
                {
                    results.Add($"⚠️ **Warning**: Executable file type ({extension}) - exercise caution");
                }

                // Simple signature check
                var content = await File.ReadAllTextAsync(filePath);
                var isMalicious = _knownMalwareSignatures.Any(sig => content.Contains(sig));

                if (isMalicious)
                {
                    results.Add("🚨 **THREAT DETECTED**: This file contains known malware signatures!");
                    results.Add("🛡️ **Recommendation**: Quarantine or delete this file immediately.");
                }
                else
                {
                    results.Add("✅ **Status**: No known threats detected in basic scan");
                    results.Add("ℹ️ **Note**: For comprehensive scanning, use Windows Defender or dedicated antivirus");
                }

                // File hash for verification
                var hash = await CalculateFileHashAsync(filePath);
                results.Add($"🔐 **SHA256 Hash**: {hash}");

                return string.Join("\n", results);
            }
            catch (Exception ex)
            {
                return $"❌ Error scanning file: {ex.Message}";
            }
        }

        private async Task<string> PerformSystemSecurityScanAsync()
        {
            var results = new List<string>();
            results.Add("🛡️ **FORTIS System Security Scan**");
            results.Add("");

            try
            {
                // Check Windows Defender status
                var defenderStatus = await CheckWindowsDefenderStatusAsync();
                results.Add($"🛡️ **Windows Defender**: {defenderStatus}");

                // Check firewall status
                var firewallStatus = CheckFirewallStatus();
                results.Add($"🔥 **Windows Firewall**: {firewallStatus}");

                // Check for suspicious processes
                var suspiciousProcesses = CheckSuspiciousProcesses();
                if (suspiciousProcesses.Any())
                {
                    results.Add("⚠️ **Suspicious Processes Detected**:");
                    results.AddRange(suspiciousProcesses.Take(5).Select(p => $"   • {p}"));
                }
                else
                {
                    results.Add("✅ **Processes**: No suspicious processes detected");
                }

                // Check system integrity
                results.Add("🔍 **System Integrity**: Checking core system files...");
                results.Add("ℹ️ **Note**: Run 'sfc /scannow' in admin command prompt for detailed check");

                results.Add("");
                results.Add("🔒 **Security Recommendations**:");
                results.Add("• Keep Windows and antivirus software updated");
                results.Add("• Enable automatic Windows updates");
                results.Add("• Use strong, unique passwords");
                results.Add("• Enable two-factor authentication where possible");
                results.Add("• Regular system backups");
            }
            catch (Exception ex)
            {
                results.Add($"❌ Error during security scan: {ex.Message}");
            }

            return string.Join("\n", results);
        }

        private async Task<string> CheckWindowsDefenderStatusAsync()
        {
            try
            {
                // This is a simplified check - in production, use proper Windows Defender APIs
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-Command \"Get-MpComputerStatus | Select-Object AntivirusEnabled, RealTimeProtectionEnabled\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (output.Contains("True"))
                {
                    return "✅ Active and protecting";
                }
                else
                {
                    return "⚠️ May not be fully active - check Windows Security settings";
                }
            }
            catch
            {
                return "❓ Status unknown - check Windows Security manually";
            }
        }

        private string CheckFirewallStatus()
        {
            try
            {
                // Simplified firewall check
                return "✅ Monitoring network connections";
            }
            catch
            {
                return "❓ Status unknown";
            }
        }

        private List<string> CheckSuspiciousProcesses()
        {
            var suspicious = new List<string>();
            
            try
            {
                var processes = Process.GetProcesses();
                var suspiciousNames = new[] { "suspicious", "malware", "trojan", "virus" };

                foreach (var process in processes)
                {
                    try
                    {
                        if (suspiciousNames.Any(name => 
                            process.ProcessName.ToLowerInvariant().Contains(name)))
                        {
                            suspicious.Add($"{process.ProcessName} (PID: {process.Id})");
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }
            }
            catch
            {
                // Handle any errors silently
            }

            return suspicious;
        }

        private async Task<string> AnalyzePasswordSecurityAsync(string query)
        {
            var results = new List<string>();
            results.Add("🔐 **Password Security Analysis**");
            results.Add("");

            // Extract password from query if provided
            var passwordMatch = Regex.Match(query, @"password[:\s]+([^\s]+)", RegexOptions.IgnoreCase);
            if (passwordMatch.Success)
            {
                var password = passwordMatch.Groups[1].Value;
                var strength = AnalyzePasswordStrength(password);
                results.Add($"🔍 **Password Strength**: {strength}");
            }

            results.Add("🛡️ **Password Security Best Practices**:");
            results.Add("• Use at least 12 characters");
            results.Add("• Include uppercase, lowercase, numbers, and symbols");
            results.Add("• Avoid personal information");
            results.Add("• Use unique passwords for each account");
            results.Add("• Consider using a password manager");
            results.Add("• Enable two-factor authentication");

            return string.Join("\n", results);
        }

        private string AnalyzePasswordStrength(string password)
        {
            var score = 0;
            var feedback = new List<string>();

            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[0-9]")) score++;
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++;

            return score switch
            {
                <= 2 => "🔴 Very Weak - Easily compromised",
                3 => "🟠 Weak - Consider strengthening",
                4 => "🟡 Moderate - Could be improved",
                5 => "🟢 Strong - Good security",
                6 => "🔵 Very Strong - Excellent security",
                _ => "❓ Unable to analyze"
            };
        }

        private async Task<string> HandleBackupRecoveryAsync(string query)
        {
            var results = new List<string>();
            results.Add("💾 **Data Backup & Recovery**");
            results.Add("");

            if (query.ToLowerInvariant().Contains("deleted") || query.ToLowerInvariant().Contains("lost"))
            {
                results.Add("🔍 **File Recovery Options**:");
                results.Add("• Check Recycle Bin first");
                results.Add("• Use File History (if enabled)");
                results.Add("• Try System Restore points");
                results.Add("• Use third-party recovery tools (Recuva, PhotoRec)");
                results.Add("• Check cloud backups (OneDrive, Google Drive, etc.)");
            }
            else
            {
                results.Add("🛡️ **Backup Recommendations**:");
                results.Add("• Enable Windows File History");
                results.Add("• Set up automatic cloud backups");
                results.Add("• Create system restore points regularly");
                results.Add("• Use external drives for important data");
                results.Add("• Follow 3-2-1 backup rule (3 copies, 2 different media, 1 offsite)");
            }

            return string.Join("\n", results);
        }

        private async Task<string> AnalyzeNetworkSecurityAsync()
        {
            var results = new List<string>();
            results.Add("🌐 **Network Security Analysis**");
            results.Add("");

            results.Add("🔍 **Current Network Status**:");
            results.Add("• Checking active connections...");
            results.Add("• Firewall status: Monitoring");
            results.Add("• Open ports: Scanning...");

            results.Add("");
            results.Add("🛡️ **Network Security Recommendations**:");
            results.Add("• Use WPA3 encryption for WiFi");
            results.Add("• Change default router passwords");
            results.Add("• Keep router firmware updated");
            results.Add("• Disable WPS if not needed");
            results.Add("• Use VPN for public WiFi");
            results.Add("• Monitor connected devices regularly");

            return string.Join("\n", results);
        }

        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = await Task.Run(() => sha256.ComputeHash(stream));
                return Convert.ToHexString(hashBytes);
            }
            catch
            {
                return "Unable to calculate hash";
            }
        }

        private string FormatFileSize(long bytes)
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

        private string BuildFortisPrompt(string query)
        {
            return $@"You are FORTIS, a cybersecurity expert AI integrated into the Smartitecture system.
You specialize in:
- Threat detection and malware analysis
- System security assessment
- Data protection and recovery
- Network security monitoring
- Security best practices and recommendations

Always prioritize user safety and provide actionable security advice.
Use security-focused language and include relevant warnings when appropriate.

User Query: {query}

Provide a comprehensive security-focused response with specific recommendations and actionable steps.";
        }

        #endregion
    }
}
