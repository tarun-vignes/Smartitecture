using System.Runtime.InteropServices;
using System.Text.Json;

namespace Smartitecture.Core.Services.CrashReporting
{
    public interface ICrashReportingService
    {
        void Initialize();
        void ReportException(Exception ex, string? additionalInfo = null);
        void ReportCrash(string message, string? additionalInfo = null);
        void SendReports();
    }

    public class CrashReportingService : ICrashReportingService
    {
        private readonly HttpClient _httpClient;
        private readonly IFileSystemService _fileSystem;
        private readonly ILogger<CrashReportingService> _logger;
        private readonly string _reportUrl;
        private readonly string _reportsPath;
        private readonly List<CrashReport> _pendingReports = new();

        public CrashReportingService(
            HttpClient httpClient,
            IFileSystemService fileSystem,
            ILogger<CrashReportingService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _fileSystem = fileSystem;
            _logger = logger;
            _reportUrl = configuration.GetValue<string>("CrashReporting:Url") ?? 
                "https://api.smartitecture.com/crashes";
            _reportsPath = Path.Combine(_fileSystem.GetAppDataPath(), "crash_reports");
        }

        public void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            _fileSystem.CreateDirectory(_reportsPath);
            LoadPendingReports();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                ReportException(ex, "Unhandled exception in application");
            }
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            ReportException(e.Exception, "Unobserved task exception");
        }

        public void ReportException(Exception ex, string? additionalInfo = null)
        {
            var report = new CrashReport
            {
                Timestamp = DateTime.UtcNow,
                ExceptionType = ex.GetType().FullName,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                AdditionalInfo = additionalInfo,
                MachineInfo = GetMachineInfo(),
                AppVersion = typeof(CrashReportingService).Assembly.GetName().Version?.ToString()
            };

            _pendingReports.Add(report);
            SaveReport(report);
            SendReports();
        }

        public void ReportCrash(string message, string? additionalInfo = null)
        {
            var report = new CrashReport
            {
                Timestamp = DateTime.UtcNow,
                ExceptionType = "Crash",
                Message = message,
                StackTrace = Environment.StackTrace,
                AdditionalInfo = additionalInfo,
                MachineInfo = GetMachineInfo(),
                AppVersion = typeof(CrashReportingService).Assembly.GetName().Version?.ToString()
            };

            _pendingReports.Add(report);
            SaveReport(report);
            SendReports();
        }

        public void SendReports()
        {
            try
            {
                foreach (var report in _pendingReports)
                {
                    var json = JsonSerializer.Serialize(report);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = _httpClient.PostAsync(_reportUrl, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        _pendingReports.Remove(report);
                        DeleteReport(report);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending crash reports");
            }
        }

        private void SaveReport(CrashReport report)
        {
            try
            {
                var fileName = Path.Combine(_reportsPath, 
                    $"crash_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
                var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                _fileSystem.SaveFile(fileName, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving crash report");
            }
        }

        private void DeleteReport(CrashReport report)
        {
            try
            {
                var fileName = Path.Combine(_reportsPath, 
                    $"crash_{report.Timestamp:yyyyMMdd_HHmmss}.json");
                _fileSystem.DeleteFile(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting crash report");
            }
        }

        private void LoadPendingReports()
        {
            try
            {
                if (!_fileSystem.DirectoryExists(_reportsPath))
                {
                    return;
                }

                var files = _fileSystem.GetFiles(_reportsPath);
                foreach (var file in files)
                {
                    if (file.EndsWith(".json"))
                    {
                        var report = _fileSystem.LoadFile<CrashReport>(file);
                        if (report != null)
                        {
                            _pendingReports.Add(report);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending crash reports");
            }
        }

        private Dictionary<string, string> GetMachineInfo()
        {
            return new Dictionary<string, string>
            {
                { "OSVersion", Environment.OSVersion.ToString() },
                { "MachineName", Environment.MachineName },
                { "ProcessorCount", Environment.ProcessorCount.ToString() },
                { "Is64Bit", Environment.Is64BitOperatingSystem.ToString() },
                { "ClrVersion", Environment.Version.ToString() },
                { "WorkingSet", Environment.WorkingSet.ToString() }
            };
        }
    }

    public class CrashReport
    {
        public DateTime Timestamp { get; set; }
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string? AdditionalInfo { get; set; }
        public Dictionary<string, string> MachineInfo { get; set; }
        public string? AppVersion { get; set; }
    }
}
