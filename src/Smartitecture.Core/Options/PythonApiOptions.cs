using System;
using System.Collections.Generic;

namespace Smartitecture.Core.Options
{
    /// <summary>
    /// Configuration options for the Python API service.
    /// </summary>
    public class PythonApiOptions
    {
        /// <summary>
        /// The configuration section name for these options.
        /// </summary>
        public const string SectionName = "PythonApi";
        
        /// <summary>
        /// Gets or sets the base URL of the Python API.
        /// </summary>
        public string BaseUrl { get; set; } = "http://127.0.0.1:8000";
        
        /// <summary>
        /// Gets or sets the port number for the Python API.
        /// </summary>
        public int Port { get; set; } = 8000;
        
        /// <summary>
        /// Gets or sets the timeout in seconds for API requests.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Gets or sets the number of retry attempts for failed requests.
        /// </summary>
        public int RetryCount { get; set; } = 3;
        
        /// <summary>
        /// Gets or sets the maximum number of concurrent requests allowed.
        /// </summary>
        public int MaxConcurrentRequests { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the path to the Python executable.
        /// If not specified, the service will attempt to find it automatically.
        /// </summary>
        public string PythonExecutablePath { get; set; }
        
        /// <summary>
        /// Gets or sets the path to the Python backend directory.
        /// </summary>
        public string PythonBackendPath { get; set; } = "backend-python";
        
        /// <summary>
        /// Gets or sets the name of the Python script to run (without extension).
        /// </summary>
        public string ApiScriptName { get; set; } = "start_api";
        
        /// <summary>
        /// Gets or sets whether to automatically start the Python backend.
        /// </summary>
        public bool AutoStartBackend { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to restart the Python backend if it crashes.
        /// </summary>
        public bool AutoRestartOnCrash { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the maximum number of restart attempts when the backend crashes.
        /// </summary>
        public int MaxRestartAttempts { get; set; } = 3;
        
        /// <summary>
        /// Gets or sets the delay in seconds between restart attempts.
        /// </summary>
        public int RestartDelaySeconds { get; set; } = 5;
        
        /// <summary>
        /// Gets or sets additional environment variables to pass to the Python process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
        
        /// <summary>
        /// Gets or sets additional HTTP headers to include in API requests.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the log level for the Python process.
        /// </summary>
        public string LogLevel { get; set; } = "Information";
        
        /// <summary>
        /// Gets or sets whether to enable debug mode for the Python process.
        /// </summary>
        public bool DebugMode { get; set; }
        
        /// <summary>
        /// Gets or sets the path where log files should be written.
        /// </summary>
        public string LogFilePath { get; set; } = "logs";
        
        /// <summary>
        /// Gets or sets the maximum size of log files in MB.
        /// </summary>
        public int MaxLogFileSizeMB { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the maximum number of log files to keep.
        /// </summary>
        public int MaxLogFiles { get; set; } = 5;
    }
}
