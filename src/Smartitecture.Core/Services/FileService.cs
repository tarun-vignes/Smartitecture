using System.IO;
using System.Threading.Tasks;

namespace Smartitecture.Core.Services
{
    public interface IFileService
    {
        Task<bool> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadFileAsync(string fileName);
        Task<bool> DeleteFileAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        Task<IEnumerable<string>> GetFilesAsync();
        Task<bool> ValidateFileAsync(Stream fileStream, string contentType);
    }

    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        public FileService(IConfiguration configuration, ILogger<FileService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var uploadPath = _configuration["FileStorage:UploadPath"];
                if (string.IsNullOrEmpty(uploadPath))
                {
                    throw new ConfigurationException("File storage path not configured");
                }

                var filePath = Path.Combine(uploadPath, fileName);
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await fileStream.CopyToAsync(fileStream);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", fileName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            try
            {
                var uploadPath = _configuration["FileStorage:UploadPath"];
                var filePath = Path.Combine(uploadPath, fileName);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File {fileName} not found");
                }

                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var uploadPath = _configuration["FileStorage:UploadPath"];
                var filePath = Path.Combine(uploadPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                var uploadPath = _configuration["FileStorage:UploadPath"];
                var filePath = Path.Combine(uploadPath, fileName);
                return File.Exists(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence for {FileName}", fileName);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetFilesAsync()
        {
            try
            {
                var uploadPath = _configuration["FileStorage:UploadPath"];
                return Directory.GetFiles(uploadPath).Select(Path.GetFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files list");
                throw;
            }
        }

        public async Task<bool> ValidateFileAsync(Stream fileStream, string contentType)
        {
            try
            {
                // TODO: Implement file validation based on content type
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file");
                throw;
            }
        }
    }
}
