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
using AIPal.Services.AgentTools;

namespace AIPal.Application.Security.Tools
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
                Name = "CheckForMalware",
                Description = "Checks if the system has any signs of malware or viruses",
                Function = CheckForMalware
            },
            new AgentTool
            {
                Name = "ExplainSecurityTerm",
                Description = "Explains security terms in simple language for elderly users",
                Function = ExplainSecurityTerm
            },
            new AgentTool
            {
                Name = "CheckWindowsDefenderStatus",
                Description = "Checks if Windows Defender is active and up to date",
                Function = CheckWindowsDefenderStatus
            },
            new AgentTool
            {
                Name = "SuggestSecurityImprovements",
                Description = "Suggests ways to improve system security",
                Function = SuggestSecurityImprovements
            }
        };

        /// <summary>
        /// Checks the system for signs of malware or viruses.
        /// </summary>
        private static async Task<string> CheckForMalware(JsonElement parameters)
        {
            StringBuilder result = new StringBuilder();
            
            try
            {
                // Check Windows Defender status
                bool defenderEnabled = IsWindowsDefenderEnabled();
                bool realtimeProtectionEnabled = IsRealtimeProtectionEnabled();
                
                result.AppendLine("I've checked your computer's security status:");
                
                if (defenderEnabled)
                {
                    result.AppendLine("✅ Windows Defender is active on your computer.");
                }
                else
                {
                    result.AppendLine("⚠️ Windows Defender appears to be turned off. This is your main protection against viruses.");
                }
                
                if (realtimeProtectionEnabled)
                {
                    result.AppendLine("✅ Real-time protection is enabled, which helps catch viruses immediately.");
                }
                else
                {
                    result.AppendLine("⚠️ Real-time protection is disabled. This means your computer isn't actively checking for threats.");
                }
                
                // Check for suspicious processes
                var suspiciousProcesses = FindSuspiciousProcesses();
                if (suspiciousProcesses.Count > 0)
                {
                    result.AppendLine("\nI've noticed some processes that might need attention:");
                    foreach (var process in suspiciousProcesses)
                    {
                        result.AppendLine($"⚠️ {process.ProcessName} - This program is using a lot of resources or has unusual behavior.");
                    }
                    result.AppendLine("\nHaving these processes isn't always a problem, but if you don't recognize them, it might be worth investigating.");
                }
                else
                {
                    result.AppendLine("\n✅ I don't see any suspicious processes running right now.");
                }
                
                // Check for recent scans
                DateTime? lastScanTime = GetLastMalwareScanTime();
                if (lastScanTime.HasValue)
                {
                    TimeSpan timeSinceLastScan = DateTime.Now - lastScanTime.Value;
                    if (timeSinceLastScan.TotalDays > 7)
                    {
                        result.AppendLine($"\n⚠️ Your last virus scan was {(int)timeSinceLastScan.TotalDays} days ago. It's good to scan weekly.");
                    }
                    else
                    {
                        result.AppendLine($"\n✅ Your computer was scanned for viruses recently ({(int)timeSinceLastScan.TotalDays} days ago).");
                    }
                }
                else
                {
                    result.AppendLine("\n⚠️ I couldn't find records of a recent virus scan.");
                }
                
                // Add recommendations
                result.AppendLine("\nWhat you can do to stay safe:");
                if (!defenderEnabled || !realtimeProtectionEnabled)
                {
                    result.AppendLine("1. Turn on Windows Defender and real-time protection: Click the Start button, type 'Windows Security', and make sure everything is turned on.");
                }
                
                if (lastScanTime == null || (DateTime.Now - lastScanTime.Value).TotalDays > 7)
                {
                    result.AppendLine("2. Run a virus scan: Click the Start button, type 'Windows Security', click on 'Virus & threat protection', and click 'Quick scan'.");
                }
                
                result.AppendLine("3. Be careful about opening email attachments or clicking on links from people you don't know.");
                result.AppendLine("4. Keep your computer updated by allowing Windows updates to install.");
            }
            catch (Exception ex)
            {
                result.Clear();
                result.AppendLine("I tried to check your computer's security status, but ran into a technical problem.");
                result.AppendLine("\nHere are some general security recommendations:");
                result.AppendLine("1. Make sure Windows Defender is turned on");
                result.AppendLine("2. Run regular virus scans (at least once a week)");
                result.AppendLine("3. Keep your computer updated");
                result.AppendLine("4. Be careful about opening email attachments or clicking on unfamiliar links");
            }
            
            return result.ToString();
        }
        
        /// <summary>
        /// Explains security terms in simple language for elderly users.
        /// </summary>
        private static async Task<string> ExplainSecurityTerm(JsonElement parameters)
        {
            try
            {
                string term = "";
                if (parameters.TryGetProperty("term", out JsonElement termElement))
                {
                    term = termElement.GetString().ToLower();
                }
                
                if (string.IsNullOrEmpty(term))
                {
                    return "I'd be happy to explain security terms to you. Please specify which term you'd like me to explain.";
                }
                
                Dictionary<string, string> securityTerms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "malware", "Malware is short for 'malicious software'. It's any program designed to harm your computer or steal your information. This includes viruses, spyware, and ransomware. Think of it like germs that can make your computer sick." },
                    { "virus", "A computer virus is a type of malware that spreads by attaching itself to programs or files, similar to how a human virus spreads from person to person. When you open an infected file, the virus can spread to other files and might damage your computer or steal information." },
                    { "firewall", "A firewall is like a security guard for your computer. It monitors what comes in and goes out of your computer through the internet, and blocks suspicious activity. It helps keep hackers from accessing your computer." },
                    { "phishing", "Phishing is when someone tries to trick you into giving them your personal information by pretending to be someone you trust. For example, you might get an email that looks like it's from your bank asking for your password. Real banks never do this." },
                    { "vpn", "VPN stands for Virtual Private Network. It's like a private tunnel on the internet that keeps your information secure and private, especially when using public WiFi like at a coffee shop or library." },
                    { "encryption", "Encryption is like putting your information in a locked box. It scrambles your data so that only people with the right key (password) can read it. This keeps your information safe when it's sent over the internet." },
                    { "two-factor authentication", "Two-factor authentication (sometimes called 2FA) adds an extra layer of security. Besides your password, you need a second form of identification, like a code sent to your phone. It's like having both a key and a security code to enter a building." },
                    { "ransomware", "Ransomware is a nasty type of malware that locks up your files and demands payment (ransom) to unlock them. It's like someone putting a padlock on your filing cabinet and demanding money for the key." },
                    { "spyware", "Spyware is software that secretly monitors what you do on your computer and sends this information to someone else without your knowledge. It's like having someone looking over your shoulder without you knowing." },
                    { "trojan", "A Trojan (or Trojan horse) is malware disguised as legitimate software. It tricks you into installing it, thinking it's something useful or harmless. Once installed, it can damage your computer or steal information. It's named after the wooden horse from Greek mythology that was used to sneak soldiers into Troy." },
                    { "adware", "Adware is software that automatically displays or downloads advertisements when you're online. While not always harmful, it can slow down your computer and be annoying with pop-up ads." },
                    { "backup", "A backup is a copy of your important files kept in a separate location. If something happens to your computer, like a virus attack or hardware failure, you can restore your files from the backup. It's like having a photocopy of important documents in case the originals are lost." },
                    { "patch", "A patch is a small update to a computer program that fixes security vulnerabilities or other bugs. Keeping your software patched is important for security, like fixing holes in a fence to keep intruders out." },
                    { "cookies", "Cookies are small pieces of information that websites store on your computer to remember things about you, like your login information or items in a shopping cart. Most cookies are helpful, but some can track your browsing habits." },
                    { "https", "HTTPS is a secure way for your browser to connect to websites. The 'S' stands for 'Secure'. When you see HTTPS and a padlock icon in your browser's address bar, it means your connection to that website is encrypted and safer." }
                };
                
                if (securityTerms.TryGetValue(term, out string explanation))
                {
                    return $"Let me explain what '{term}' means in simple terms:\n\n{explanation}";
                }
                else
                {
                    return $"I don't have a simple explanation for '{term}' in my database. Would you like me to explain a different security term, or would you like a general explanation about computer security?";
                }
            }
            catch (Exception ex)
            {
                return "I'm sorry, I had trouble explaining that term. Please try asking about a different security term like 'malware', 'virus', 'firewall', or 'phishing'.";
            }
        }
        
        /// <summary>
        /// Checks if Windows Defender is active and up to date.
        /// </summary>
        private static async Task<string> CheckWindowsDefenderStatus(JsonElement parameters)
        {
            StringBuilder result = new StringBuilder();
            
            try
            {
                bool defenderEnabled = IsWindowsDefenderEnabled();
                bool realtimeProtectionEnabled = IsRealtimeProtectionEnabled();
                DateTime? definitionUpdateTime = GetDefinitionUpdateTime();
                
                result.AppendLine("I've checked your Windows Defender status:");
                
                if (defenderEnabled)
                {
                    result.AppendLine("✅ Windows Defender is turned on");
                }
                else
                {
                    result.AppendLine("❌ Windows Defender appears to be turned off");
                }
                
                if (realtimeProtectionEnabled)
                {
                    result.AppendLine("✅ Real-time protection is enabled");
                }
                else
                {
                    result.AppendLine("❌ Real-time protection is disabled");
                }
                
                if (definitionUpdateTime.HasValue)
                {
                    TimeSpan timeSinceUpdate = DateTime.Now - definitionUpdateTime.Value;
                    if (timeSinceUpdate.TotalDays < 7)
                    {
                        result.AppendLine($"✅ Virus definitions are up to date (last updated {(int)timeSinceUpdate.TotalDays} days ago)");
                    }
                    else
                    {
                        result.AppendLine($"❌ Virus definitions might be outdated (last updated {(int)timeSinceUpdate.TotalDays} days ago)");
                    }
                }
                else
                {
                    result.AppendLine("❓ Couldn't determine when virus definitions were last updated");
                }
                
                // Add recommendations based on status
                result.AppendLine("\nWhat this means:");
                if (!defenderEnabled)
                {
                    result.AppendLine("Windows Defender is your computer's built-in protection against viruses and other threats. Since it's turned off, your computer might be vulnerable.");
                    result.AppendLine("\nHow to turn on Windows Defender:");
                    result.AppendLine("1. Click the Start button and type 'Windows Security'");
                    result.AppendLine("2. Click on 'Virus & threat protection'");
                    result.AppendLine("3. Make sure all the protection settings are turned on");
                }
                else if (!realtimeProtectionEnabled)
                {
                    result.AppendLine("Windows Defender is on, but real-time protection is off. This means your computer isn't actively checking for threats as they happen.");
                    result.AppendLine("\nHow to turn on real-time protection:");
                    result.AppendLine("1. Click the Start button and type 'Windows Security'");
                    result.AppendLine("2. Click on 'Virus & threat protection'");
                    result.AppendLine("3. Click on 'Manage settings' under 'Virus & threat protection settings'");
                    result.AppendLine("4. Turn on 'Real-time protection'");
                }
                else
                {
                    result.AppendLine("Your Windows Defender protection looks good! This means your computer has basic protection against viruses and other threats.");
                    
                    if (definitionUpdateTime.HasValue && (DateTime.Now - definitionUpdateTime.Value).TotalDays >= 7)
                    {
                        result.AppendLine("\nHowever, your virus definitions might need updating. These are the files that help Windows Defender recognize new viruses.");
                        result.AppendLine("\nHow to update virus definitions:");
                        result.AppendLine("1. Click the Start button and type 'Windows Security'");
                        result.AppendLine("2. Click on 'Virus & threat protection'");
                        result.AppendLine("3. Click on 'Check for updates' under 'Virus & threat protection updates'");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Clear();
                result.AppendLine("I tried to check your Windows Defender status, but ran into a technical problem.");
                result.AppendLine("\nTo check Windows Defender manually:");
                result.AppendLine("1. Click the Start button and type 'Windows Security'");
                result.AppendLine("2. Open Windows Security and click on 'Virus & threat protection'");
                result.AppendLine("3. Make sure all protection settings are turned on");
            }
            
            return result.ToString();
        }
        
        /// <summary>
        /// Suggests ways to improve system security.
        /// </summary>
        private static async Task<string> SuggestSecurityImprovements(JsonElement parameters)
        {
            StringBuilder result = new StringBuilder();
            
            try
            {
                result.AppendLine("Here are some simple ways to keep your computer safe:");
                
                // Check Windows Defender status
                bool defenderEnabled = IsWindowsDefenderEnabled();
                bool realtimeProtectionEnabled = IsRealtimeProtectionEnabled();
                
                if (!defenderEnabled || !realtimeProtectionEnabled)
                {
                    result.AppendLine("\n1. Turn on Windows Defender protection");
                    result.AppendLine("   Windows Defender is your computer's built-in security system.");
                    result.AppendLine("   • Click the Start button and type 'Windows Security'");
                    result.AppendLine("   • Click on 'Virus & threat protection'");
                    result.AppendLine("   • Make sure all protection settings are turned on");
                }
                else
                {
                    result.AppendLine("\n✅ Your Windows Defender protection is already turned on. Good job!");
                }
                
                // Check for Windows updates
                bool windowsUpdateEnabled = IsWindowsUpdateEnabled();
                if (!windowsUpdateEnabled)
                {
                    result.AppendLine("\n2. Turn on automatic Windows updates");
                    result.AppendLine("   Updates fix security problems in Windows.");
                    result.AppendLine("   • Click the Start button and type 'Windows Update'");
                    result.AppendLine("   • Click on 'Windows Update settings'");
                    result.AppendLine("   • Make sure automatic updates are turned on");
                }
                else
                {
                    result.AppendLine("\n✅ Your Windows updates are set to automatic. That's great!");
                }
                
                // Check user account type
                bool isAdminAccount = IsAdminAccount();
                if (isAdminAccount)
                {
                    result.AppendLine("\n3. Consider using a standard user account for daily tasks");
                    result.AppendLine("   Using an administrator account all the time can be risky.");
                    result.AppendLine("   • Click the Start button and type 'User accounts'");
                    result.AppendLine("   • Consider creating a standard account for everyday use");
                }
                
                // General recommendations
                result.AppendLine("\nOther important security tips:");
                
                result.AppendLine("\n4. Use strong, unique passwords");
                result.AppendLine("   • Make passwords at least 12 characters long");
                result.AppendLine("   • Use a mix of letters, numbers, and symbols");
                result.AppendLine("   • Don't use the same password for multiple accounts");
                result.AppendLine("   • Consider using a password manager to help remember them");
                
                result.AppendLine("\n5. Be careful with email and websites");
                result.AppendLine("   • Don't open attachments or click links from unknown senders");
                result.AppendLine("   • Be suspicious of emails asking for personal information");
                result.AppendLine("   • Look for 'https' and a padlock icon when shopping or banking online");
                
                result.AppendLine("\n6. Back up your important files");
                result.AppendLine("   • Keep copies of important photos and documents on an external drive");
                result.AppendLine("   • Consider using a cloud backup service like OneDrive or Google Drive");
                
                result.AppendLine("\n7. Be careful what you download");
                result.AppendLine("   • Only download programs from official websites");
                result.AppendLine("   • Be wary of 'free' software that seems too good to be true");
                
                result.AppendLine("\nWould you like help with any of these specific security improvements?");
            }
            catch (Exception ex)
            {
                result.Clear();
                result.AppendLine("Here are some general security recommendations to keep your computer safe:");
                
                result.AppendLine("\n1. Keep Windows Defender turned on");
                result.AppendLine("2. Keep your computer updated with the latest Windows updates");
                result.AppendLine("3. Use strong, unique passwords for your accounts");
                result.AppendLine("4. Be careful with email attachments and links");
                result.AppendLine("5. Back up your important files regularly");
                result.AppendLine("6. Only download software from trusted sources");
                
                result.AppendLine("\nWould you like more detailed information about any of these tips?");
            }
            
            return result.ToString();
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
        
        private static List<Process> FindSuspiciousProcesses()
        {
            List<Process> suspiciousProcesses = new List<Process>();
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
                            suspiciousProcesses.Add(process);
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access
                        continue;
                    }
                }
                
                // Limit to top 5 most resource-intensive
                return suspiciousProcesses.OrderByDescending(p => p.WorkingSet64).Take(5).ToList();
            }
            catch
            {
                return new List<Process>();
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
}
