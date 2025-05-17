using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace AIPal.Application.Security.Services
{
    /// <summary>
    /// Service for monitoring system security and providing security information.
    /// </summary>
    public class SecurityMonitorService
    {
        private Timer _monitorTimer;
        private bool _isWindowsDefenderEnabled;
        private bool _isRealtimeProtectionEnabled;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityMonitorService"/> class.
        /// </summary>
        public SecurityMonitorService()
        {
            // Initialize security status
            _isWindowsDefenderEnabled = IsWindowsDefenderEnabled();
            _isRealtimeProtectionEnabled = IsRealtimeProtectionEnabled();
            
            // Start monitoring security status
            _monitorTimer = new Timer(CheckSecurityStatus, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }
        
        /// <summary>
        /// Event that is raised when the security status changes.
        /// </summary>
        public event EventHandler<SecurityStatusChangedEventArgs> SecurityStatusChanged;
        
        /// <summary>
        /// Gets a value indicating whether Windows Defender is enabled.
        /// </summary>
        public bool IsWindowsDefenderEnabled => _isWindowsDefenderEnabled;
        
        /// <summary>
        /// Gets a value indicating whether real-time protection is enabled.
        /// </summary>
        public bool IsRealtimeProtectionEnabled => _isRealtimeProtectionEnabled;
        
        /// <summary>
        /// Gets the security status of the system.
        /// </summary>
        /// <returns>A <see cref="SecurityStatus"/> object containing information about the system's security status.</returns>
        public SecurityStatus GetSecurityStatus()
        {
            var status = new SecurityStatus
            {
                IsWindowsDefenderEnabled = IsWindowsDefenderEnabled(),
                IsRealtimeProtectionEnabled = IsRealtimeProtectionEnabled(),
                LastDefinitionUpdateTime = GetDefinitionUpdateTime(),
                LastScanTime = GetLastMalwareScanTime(),
                IsWindowsUpdateEnabled = IsWindowsUpdateEnabled(),
                IsAdminAccount = IsAdminAccount()
            };
            
            // Calculate overall security rating
            CalculateSecurityRating(status);
            
            return status;
        }
        
        /// <summary>
        /// Checks for suspicious processes on the system.
        /// </summary>
        /// <returns>A list of <see cref="SuspiciousProcess"/> objects representing suspicious processes.</returns>
        public List<SuspiciousProcess> CheckForSuspiciousProcesses()
        {
            var suspiciousProcesses = new List<SuspiciousProcess>();
            
            try
            {
                Process[] allProcesses = Process.GetProcesses();
                
                // Get processes with high CPU or memory usage
                foreach (var process in allProcesses)
                {
                    try
                    {
                        // This is a simple heuristic - in a real application, you'd want more sophisticated detection
                        if (process.WorkingSet64 > 500 * 1024 * 1024) // More than 500MB memory
                        {
                            suspiciousProcesses.Add(new SuspiciousProcess
                            {
                                ProcessName = process.ProcessName,
                                ProcessId = process.Id,
                                MemoryUsage = process.WorkingSet64,
                                Reason = "High memory usage"
                            });
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access
                        continue;
                    }
                }
                
                // Limit to top 5 most resource-intensive
                return suspiciousProcesses.OrderByDescending(p => p.MemoryUsage).Take(5).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for suspicious processes: {ex.Message}");
                return new List<SuspiciousProcess>();
            }
        }
        
        /// <summary>
        /// Gets security recommendations based on the current security status.
        /// </summary>
        /// <returns>A list of security recommendations.</returns>
        public List<SecurityRecommendation> GetSecurityRecommendations()
        {
            var recommendations = new List<SecurityRecommendation>();
            var status = GetSecurityStatus();
            
            // Check Windows Defender status
            if (!status.IsWindowsDefenderEnabled)
            {
                recommendations.Add(new SecurityRecommendation
                {
                    Title = "Enable Windows Defender",
                    Description = "Windows Defender is your computer's built-in protection against viruses and other threats. Since it's turned off, your computer might be vulnerable.",
                    Priority = RecommendationPriority.High,
                    Steps = new List<string>
                    {
                        "Click the Start button and type 'Windows Security'",
                        "Click on 'Virus & threat protection'",
                        "Make sure all the protection settings are turned on"
                    }
                });
            }
            
            // Check real-time protection
            if (status.IsWindowsDefenderEnabled && !status.IsRealtimeProtectionEnabled)
            {
                recommendations.Add(new SecurityRecommendation
                {
                    Title = "Enable Real-time Protection",
                    Description = "Real-time protection helps protect your computer by scanning for malware as files are downloaded or opened.",
                    Priority = RecommendationPriority.High,
                    Steps = new List<string>
                    {
                        "Click the Start button and type 'Windows Security'",
                        "Click on 'Virus & threat protection'",
                        "Click on 'Manage settings' under 'Virus & threat protection settings'",
                        "Turn on 'Real-time protection'"
                    }
                });
            }
            
            // Check definition updates
            if (status.LastDefinitionUpdateTime.HasValue)
            {
                TimeSpan timeSinceUpdate = DateTime.Now - status.LastDefinitionUpdateTime.Value;
                if (timeSinceUpdate.TotalDays > 7)
                {
                    recommendations.Add(new SecurityRecommendation
                    {
                        Title = "Update Virus Definitions",
                        Description = "Your virus definitions might be outdated. These are the files that help Windows Defender recognize new viruses.",
                        Priority = RecommendationPriority.Medium,
                        Steps = new List<string>
                        {
                            "Click the Start button and type 'Windows Security'",
                            "Click on 'Virus & threat protection'",
                            "Click on 'Check for updates' under 'Virus & threat protection updates'"
                        }
                    });
                }
            }
            
            // Check last scan time
            if (status.LastScanTime.HasValue)
            {
                TimeSpan timeSinceScan = DateTime.Now - status.LastScanTime.Value;
                if (timeSinceScan.TotalDays > 7)
                {
                    recommendations.Add(new SecurityRecommendation
                    {
                        Title = "Run a Virus Scan",
                        Description = "It's been more than a week since your last virus scan. Regular scans help keep your computer safe.",
                        Priority = RecommendationPriority.Medium,
                        Steps = new List<string>
                        {
                            "Click the Start button and type 'Windows Security'",
                            "Click on 'Virus & threat protection'",
                            "Click on 'Quick scan' under 'Current threats'"
                        }
                    });
                }
            }
            
            // Check Windows Update
            if (!status.IsWindowsUpdateEnabled)
            {
                recommendations.Add(new SecurityRecommendation
                {
                    Title = "Enable Windows Update",
                    Description = "Windows updates include important security patches that help protect your computer.",
                    Priority = RecommendationPriority.Medium,
                    Steps = new List<string>
                    {
                        "Click the Start button and type 'Windows Update'",
                        "Click on 'Windows Update settings'",
                        "Make sure automatic updates are turned on"
                    }
                });
            }
            
            // Check if using admin account
            if (status.IsAdminAccount)
            {
                recommendations.Add(new SecurityRecommendation
                {
                    Title = "Use a Standard User Account",
                    Description = "Using an administrator account for everyday tasks can be risky. Consider creating a standard user account for daily use.",
                    Priority = RecommendationPriority.Low,
                    Steps = new List<string>
                    {
                        "Click the Start button and type 'User accounts'",
                        "Click on 'Add, edit, or remove other users'",
                        "Click on 'Add someone else to this PC'",
                        "Follow the steps to create a standard user account"
                    }
                });
            }
            
            // Add general recommendations
            recommendations.Add(new SecurityRecommendation
            {
                Title = "Use Strong, Unique Passwords",
                Description = "Strong passwords help protect your accounts from unauthorized access.",
                Priority = RecommendationPriority.Medium,
                Steps = new List<string>
                {
                    "Make passwords at least 12 characters long",
                    "Use a mix of letters, numbers, and symbols",
                    "Don't use the same password for multiple accounts",
                    "Consider using a password manager to help remember them"
                }
            });
            
            recommendations.Add(new SecurityRecommendation
            {
                Title = "Be Careful with Email and Websites",
                Description = "Many security threats come from email attachments, links, and malicious websites.",
                Priority = RecommendationPriority.Medium,
                Steps = new List<string>
                {
                    "Don't open attachments or click links from unknown senders",
                    "Be suspicious of emails asking for personal information",
                    "Look for 'https' and a padlock icon when shopping or banking online"
                }
            });
            
            recommendations.Add(new SecurityRecommendation
            {
                Title = "Back Up Your Important Files",
                Description = "Regular backups help protect your data in case of a security incident or hardware failure.",
                Priority = RecommendationPriority.Medium,
                Steps = new List<string>
                {
                    "Keep copies of important photos and documents on an external drive",
                    "Consider using a cloud backup service like OneDrive or Google Drive"
                }
            });
            
            return recommendations;
        }
        
        /// <summary>
        /// Disposes the security monitor service.
        /// </summary>
        public void Dispose()
        {
            _monitorTimer?.Dispose();
        }
        
        private void CheckSecurityStatus(object state)
        {
            bool isDefenderEnabled = IsWindowsDefenderEnabled();
            bool isRealtimeEnabled = IsRealtimeProtectionEnabled();
            
            bool statusChanged = false;
            
            if (isDefenderEnabled != _isWindowsDefenderEnabled)
            {
                _isWindowsDefenderEnabled = isDefenderEnabled;
                statusChanged = true;
            }
            
            if (isRealtimeEnabled != _isRealtimeProtectionEnabled)
            {
                _isRealtimeProtectionEnabled = isRealtimeEnabled;
                statusChanged = true;
            }
            
            if (statusChanged)
            {
                OnSecurityStatusChanged(new SecurityStatusChangedEventArgs(isDefenderEnabled, isRealtimeEnabled));
            }
        }
        
        private void OnSecurityStatusChanged(SecurityStatusChangedEventArgs e)
        {
            SecurityStatusChanged?.Invoke(this, e);
        }
        
        private void CalculateSecurityRating(SecurityStatus status)
        {
            int score = 0;
            int maxScore = 5;
            
            // Windows Defender
            if (status.IsWindowsDefenderEnabled)
            {
                score++;
            }
            
            // Real-time protection
            if (status.IsRealtimeProtectionEnabled)
            {
                score++;
            }
            
            // Definition updates
            if (status.LastDefinitionUpdateTime.HasValue)
            {
                TimeSpan timeSinceUpdate = DateTime.Now - status.LastDefinitionUpdateTime.Value;
                if (timeSinceUpdate.TotalDays <= 7)
                {
                    score++;
                }
            }
            
            // Recent scan
            if (status.LastScanTime.HasValue)
            {
                TimeSpan timeSinceScan = DateTime.Now - status.LastScanTime.Value;
                if (timeSinceScan.TotalDays <= 7)
                {
                    score++;
                }
            }
            
            // Windows Update
            if (status.IsWindowsUpdateEnabled)
            {
                score++;
            }
            
            // Calculate rating
            double percentage = (double)score / maxScore;
            
            if (percentage >= 0.8)
            {
                status.SecurityRating = SecurityRating.Good;
            }
            else if (percentage >= 0.6)
            {
                status.SecurityRating = SecurityRating.Fair;
            }
            else
            {
                status.SecurityRating = SecurityRating.Poor;
            }
        }
        
        #region Helper Methods
        
        private static bool IsWindowsDefenderEnabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("DisableAntiSpyware");
                        return value == null || (int)value != 1;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool IsRealtimeProtectionEnabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("DisableRealtimeMonitoring");
                        return value == null || (int)value != 1;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        private static DateTime? GetDefinitionUpdateTime()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Signature Updates"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("SignatureLastUpdated");
                        if (value != null)
                        {
                            // The value is stored as a FileTime (64-bit value representing the number of 100-nanosecond intervals since January 1, 1601)
                            long fileTime = Convert.ToInt64(value);
                            return DateTime.FromFileTime(fileTime);
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        private static DateTime? GetLastMalwareScanTime()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Scan"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("LastScanRun");
                        if (value != null)
                        {
                            // The value is stored as a FileTime
                            long fileTime = Convert.ToInt64(value);
                            return DateTime.FromFileTime(fileTime);
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        private static bool IsWindowsUpdateEnabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AUOptions");
                        // 2, 3, or 4 means some form of automatic updates is enabled
                        return value != null && ((int)value == 2 || (int)value == 3 || (int)value == 4);
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        private static bool IsAdminAccount()
        {
            try
            {
                // Check if the current user is in the administrators group
                bool isAdmin = false;
                using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    var principal = new System.Security.Principal.WindowsPrincipal(identity);
                    isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
                return isAdmin;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Event arguments for security status changes.
    /// </summary>
    public class SecurityStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityStatusChangedEventArgs"/> class.
        /// </summary>
        /// <param name="isWindowsDefenderEnabled">Whether Windows Defender is enabled.</param>
        /// <param name="isRealtimeProtectionEnabled">Whether real-time protection is enabled.</param>
        public SecurityStatusChangedEventArgs(bool isWindowsDefenderEnabled, bool isRealtimeProtectionEnabled)
        {
            IsWindowsDefenderEnabled = isWindowsDefenderEnabled;
            IsRealtimeProtectionEnabled = isRealtimeProtectionEnabled;
        }
        
        /// <summary>
        /// Gets a value indicating whether Windows Defender is enabled.
        /// </summary>
        public bool IsWindowsDefenderEnabled { get; }
        
        /// <summary>
        /// Gets a value indicating whether real-time protection is enabled.
        /// </summary>
        public bool IsRealtimeProtectionEnabled { get; }
    }
    
    /// <summary>
    /// Security status of the system.
    /// </summary>
    public class SecurityStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether Windows Defender is enabled.
        /// </summary>
        public bool IsWindowsDefenderEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether real-time protection is enabled.
        /// </summary>
        public bool IsRealtimeProtectionEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the time of the last definition update.
        /// </summary>
        public DateTime? LastDefinitionUpdateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the time of the last malware scan.
        /// </summary>
        public DateTime? LastScanTime { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether Windows Update is enabled.
        /// </summary>
        public bool IsWindowsUpdateEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the current user is an administrator.
        /// </summary>
        public bool IsAdminAccount { get; set; }
        
        /// <summary>
        /// Gets or sets the security rating of the system.
        /// </summary>
        public SecurityRating SecurityRating { get; set; }
    }
    
    /// <summary>
    /// Security rating of the system.
    /// </summary>
    public enum SecurityRating
    {
        /// <summary>
        /// Poor security rating.
        /// </summary>
        Poor,
        
        /// <summary>
        /// Fair security rating.
        /// </summary>
        Fair,
        
        /// <summary>
        /// Good security rating.
        /// </summary>
        Good
    }
    
    /// <summary>
    /// Priority of a security recommendation.
    /// </summary>
    public enum RecommendationPriority
    {
        /// <summary>
        /// Low priority recommendation.
        /// </summary>
        Low,
        
        /// <summary>
        /// Medium priority recommendation.
        /// </summary>
        Medium,
        
        /// <summary>
        /// High priority recommendation.
        /// </summary>
        High
    }
    
    /// <summary>
    /// Information about a suspicious process.
    /// </summary>
    public class SuspiciousProcess
    {
        /// <summary>
        /// Gets or sets the name of the process.
        /// </summary>
        public string ProcessName { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the process.
        /// </summary>
        public int ProcessId { get; set; }
        
        /// <summary>
        /// Gets or sets the memory usage of the process in bytes.
        /// </summary>
        public long MemoryUsage { get; set; }
        
        /// <summary>
        /// Gets or sets the reason why the process is considered suspicious.
        /// </summary>
        public string Reason { get; set; }
    }
    
    /// <summary>
    /// Security recommendation for the user.
    /// </summary>
    public class SecurityRecommendation
    {
        /// <summary>
        /// Gets or sets the title of the recommendation.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the recommendation.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the priority of the recommendation.
        /// </summary>
        public RecommendationPriority Priority { get; set; }
        
        /// <summary>
        /// Gets or sets the steps to implement the recommendation.
        /// </summary>
        public List<string> Steps { get; set; }
    }
}
