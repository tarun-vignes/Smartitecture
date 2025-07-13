using System.IO;
using System.Text.Json;

namespace Smartitecture.Core.Services.FileSystem
{
    public interface IFileSystemService
    {
        string GetAppDataPath();
        string GetTempPath();
        string GetDocumentsPath();
        bool FileExists(string path);
        bool DirectoryExists(string path);
        string CreateTempFile(string extension = "tmp");
        string CreateTempDirectory();
        void SaveFile<T>(string path, T data);
        T LoadFile<T>(string path);
        void DeleteFile(string path);
        void DeleteDirectory(string path);
        void CopyFile(string source, string destination);
        void MoveFile(string source, string destination);
        void CreateDirectory(string path);
    }

    public class FileSystemService : IFileSystemService
    {
        private readonly ILogger<FileSystemService> _logger;

        public FileSystemService(ILogger<FileSystemService> logger)
        {
            _logger = logger;
        }

        public string GetAppDataPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Smartitecture");
        }

        public string GetTempPath()
        {
            return Path.Combine(Path.GetTempPath(), "Smartitecture");
        }

        public string GetDocumentsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string CreateTempFile(string extension = "tmp")
        {
            var tempPath = GetTempPath();
            if (!DirectoryExists(tempPath))
            {
                CreateDirectory(tempPath);
            }

            return Path.Combine(tempPath, Path.GetRandomFileName() + "." + extension);
        }

        public string CreateTempDirectory()
        {
            var tempPath = GetTempPath();
            if (!DirectoryExists(tempPath))
            {
                CreateDirectory(tempPath);
            }

            return Path.Combine(tempPath, Path.GetRandomFileName());
        }

        public void SaveFile<T>(string path, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var directory = Path.GetDirectoryName(path);
                if (!DirectoryExists(directory))
                {
                    CreateDirectory(directory);
                }

                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file {Path}", path);
                throw;
            }
        }

        public T LoadFile<T>(string path)
        {
            try
            {
                if (!FileExists(path))
                {
                    throw new FileNotFoundException($"File not found: {path}");
                }

                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }) ?? throw new JsonException($"Failed to deserialize file: {path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading file {Path}", path);
                throw;
            }
        }

        public void DeleteFile(string path)
        {
            try
            {
                if (FileExists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {Path}", path);
                throw;
            }
        }

        public void DeleteDirectory(string path)
        {
            try
            {
                if (DirectoryExists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting directory {Path}", path);
                throw;
            }
        }

        public void CopyFile(string source, string destination)
        {
            try
            {
                if (!FileExists(source))
                {
                    throw new FileNotFoundException($"Source file not found: {source}");
                }

                var directory = Path.GetDirectoryName(destination);
                if (!DirectoryExists(directory))
                {
                    CreateDirectory(directory);
                }

                File.Copy(source, destination, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying file from {Source} to {Destination}", source, destination);
                throw;
            }
        }

        public void MoveFile(string source, string destination)
        {
            try
            {
                CopyFile(source, destination);
                DeleteFile(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving file from {Source} to {Destination}", source, destination);
                throw;
            }
        }

        public void CreateDirectory(string path)
        {
            try
            {
                if (!DirectoryExists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directory {Path}", path);
                throw;
            }
        }
    }
}
