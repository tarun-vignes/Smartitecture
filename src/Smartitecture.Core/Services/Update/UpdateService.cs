using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.Core.Services.Update
{
    public interface IUpdateService
    {
        Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
        Task DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<double> progress, CancellationToken cancellationToken = default);
        Task InstallUpdateAsync(string updateFilePath, CancellationToken cancellationToken = default);
        Version GetCurrentVersion();
    }

    public class UpdateService : IUpdateService
    {
        private readonly HttpClient _httpClient;
        private readonly IFileSystemService _fileSystem;
        private readonly ILogger<UpdateService> _logger;
        private readonly string _updateUrl;
        private readonly string _updatePath;

        public UpdateService(
            HttpClient httpClient,
            IFileSystemService fileSystem,
            ILogger<UpdateService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _fileSystem = fileSystem;
            _logger = logger;
            _updateUrl = configuration.GetValue<string>("Update:Url") ?? 
                "https://api.smartitecture.com/updates";
            _updatePath = Path.Combine(_fileSystem.GetAppDataPath(), "updates");
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(_updateUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var updateInfo = await JsonSerializer.DeserializeAsync<UpdateInfo>(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);

                return updateInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for updates");
                throw;
            }
        }

        public async Task DownloadUpdateAsync(
            UpdateInfo updateInfo,
            IProgress<double> progress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var updateFilePath = Path.Combine(_updatePath, updateInfo.FileName);
                _fileSystem.CreateDirectory(_updatePath);

                using var response = await _httpClient.GetAsync(
                    updateInfo.DownloadUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadedBytes = 0L;

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(updateFilePath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    downloadedBytes += bytesRead;
                    progress.Report((double)downloadedBytes / totalBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading update");
                throw;
            }
        }

        public async Task InstallUpdateAsync(string updateFilePath, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Implement actual update installation logic
                // This will vary based on the installation method (MSI, EXE, ZIP, etc.)
                _logger.LogInformation("Installing update from {FilePath}", updateFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing update");
                throw;
            }
        }

        public Version GetCurrentVersion()
        {
            return typeof(UpdateService).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        }
    }

    public class UpdateInfo
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTime ReleaseDate { get; set; }
        public long SizeBytes { get; set; }
    }
}
