using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIPal.Services.AgentTools
{
    /// <summary>
    /// Provides system-related tools for the AI agent to use.
    /// </summary>
    public static class SystemTools
    {
        /// <summary>
        /// Creates a tool for launching applications.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <returns>An agent tool for launching applications</returns>
        public static AgentTool CreateLaunchAppTool(ILogger logger)
        {
            return new AgentTool
            {
                Name = "LaunchApplication",
                Description = "Launches a Windows application by name or path",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["applicationName"] = new ToolParameter
                    {
                        Name = "applicationName",
                        Description = "The name or path of the application to launch",
                        Type = "string",
                        Required = true
                    },
                    ["arguments"] = new ToolParameter
                    {
                        Name = "arguments",
                        Description = "Optional arguments to pass to the application",
                        Type = "string",
                        Required = false
                    }
                },
                Execute = async (parameters) =>
                {
                    try
                    {
                        var appName = parameters["applicationName"].ToString();
                        var arguments = parameters.ContainsKey("arguments") ? parameters["arguments"].ToString() : "";

                        logger.LogInformation("Launching application: {AppName} with arguments: {Arguments}", appName, arguments);

                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = appName,
                                Arguments = arguments,
                                UseShellExecute = true
                            }
                        };

                        process.Start();
                        await Task.Delay(500); // Small delay to allow process to start

                        return new
                        {
                            Success = true,
                            Message = $"Successfully launched {appName}"
                        };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error launching application");
                        return new
                        {
                            Success = false,
                            Error = ex.Message
                        };
                    }
                }
            };
        }

        /// <summary>
        /// Creates a tool for getting system information.
        /// </summary>
        /// <returns>An agent tool for getting system information</returns>
        public static AgentTool CreateSystemInfoTool()
        {
            return new AgentTool
            {
                Name = "GetSystemInfo",
                Description = "Gets basic information about the system",
                Parameters = new Dictionary<string, ToolParameter>(),
                Execute = async (parameters) =>
                {
                    await Task.CompletedTask; // No async work needed

                    return new
                    {
                        OperatingSystem = Environment.OSVersion.ToString(),
                        MachineName = Environment.MachineName,
                        ProcessorCount = Environment.ProcessorCount,
                        SystemDirectory = Environment.SystemDirectory,
                        UserName = Environment.UserName,
                        UserDomainName = Environment.UserDomainName,
                        Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                        Is64BitProcess = Environment.Is64BitProcess,
                        SystemDriveInfo = GetDriveInfo()
                    };
                }
            };
        }

        /// <summary>
        /// Creates a tool for searching files.
        /// </summary>
        /// <returns>An agent tool for searching files</returns>
        public static AgentTool CreateFileSearchTool()
        {
            return new AgentTool
            {
                Name = "SearchFiles",
                Description = "Searches for files matching a pattern in a specified directory",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["directory"] = new ToolParameter
                    {
                        Name = "directory",
                        Description = "The directory to search in",
                        Type = "string",
                        Required = true
                    },
                    ["searchPattern"] = new ToolParameter
                    {
                        Name = "searchPattern",
                        Description = "The search pattern (e.g., '*.txt', 'document*.docx')",
                        Type = "string",
                        Required = true
                    },
                    ["recursive"] = new ToolParameter
                    {
                        Name = "recursive",
                        Description = "Whether to search recursively in subdirectories",
                        Type = "boolean",
                        Required = false
                    }
                },
                Execute = async (parameters) =>
                {
                    try
                    {
                        var directory = parameters["directory"].ToString();
                        var searchPattern = parameters["searchPattern"].ToString();
                        var recursive = parameters.ContainsKey("recursive") && 
                                       Convert.ToBoolean(parameters["recursive"]);

                        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        
                        await Task.CompletedTask; // No async work needed

                        var files = Directory.GetFiles(directory, searchPattern, searchOption)
                            .Select(path => new
                            {
                                FullPath = path,
                                FileName = Path.GetFileName(path),
                                Extension = Path.GetExtension(path),
                                SizeBytes = new FileInfo(path).Length,
                                LastModified = File.GetLastWriteTime(path)
                            })
                            .ToArray();

                        return new
                        {
                            SearchDirectory = directory,
                            SearchPattern = searchPattern,
                            Recursive = recursive,
                            FileCount = files.Length,
                            Files = files
                        };
                    }
                    catch (Exception ex)
                    {
                        return new
                        {
                            Success = false,
                            Error = ex.Message
                        };
                    }
                }
            };
        }

        /// <summary>
        /// Creates a tool for getting the current date and time.
        /// </summary>
        /// <returns>An agent tool for getting the current date and time</returns>
        public static AgentTool CreateDateTimeTool()
        {
            return new AgentTool
            {
                Name = "GetDateTime",
                Description = "Gets the current date and time information",
                Parameters = new Dictionary<string, ToolParameter>(),
                Execute = async (parameters) =>
                {
                    await Task.CompletedTask; // No async work needed

                    var now = DateTime.Now;
                    var utcNow = DateTime.UtcNow;

                    return new
                    {
                        LocalDateTime = now.ToString(),
                        UtcDateTime = utcNow.ToString(),
                        Date = now.ToShortDateString(),
                        Time = now.ToShortTimeString(),
                        DayOfWeek = now.DayOfWeek.ToString(),
                        TimeZone = TimeZoneInfo.Local.DisplayName
                    };
                }
            };
        }

        /// <summary>
        /// Gets information about system drives.
        /// </summary>
        /// <returns>Information about system drives</returns>
        private static object GetDriveInfo()
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => new
                {
                    Name = d.Name,
                    VolumeLabel = d.VolumeLabel,
                    DriveType = d.DriveType.ToString(),
                    DriveFormat = d.DriveFormat,
                    TotalSizeGB = Math.Round(d.TotalSize / (1024.0 * 1024 * 1024), 2),
                    AvailableFreeSpaceGB = Math.Round(d.AvailableFreeSpace / (1024.0 * 1024 * 1024), 2)
                })
                .ToArray();

            return drives;
        }
    }
}
