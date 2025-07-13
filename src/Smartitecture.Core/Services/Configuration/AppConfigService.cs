using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace Smartitecture.Core.Services.Configuration
{
    public interface IAppConfigService
    {
        T GetSetting<T>(string key);
        void SetSetting<T>(string key, T value);
        void Save();
        void Load();
    }

    public class AppConfigService : IAppConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly string _configPath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<AppConfigService> _logger;

        public AppConfigService(IConfiguration configuration, ILogger<AppConfigService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "Smartitecture", "config.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        public T GetSetting<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }

        public void SetSetting<T>(string key, T value)
        {
            var config = new Dictionary<string, object?>();
            if (File.Exists(_configPath))
            {
                config = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    File.ReadAllText(_configPath), _jsonOptions) ?? new Dictionary<string, object?>();
            }

            config[key] = value;
            Save();
        }

        public void Save()
        {
            try
            {
                var config = new Dictionary<string, object?>();
                if (File.Exists(_configPath))
                {
                    config = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        File.ReadAllText(_configPath), _jsonOptions) ?? new Dictionary<string, object?>();
                }

                var configDir = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                File.WriteAllText(_configPath, JsonSerializer.Serialize(config, _jsonOptions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
                throw;
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var config = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        File.ReadAllText(_configPath), _jsonOptions);
                    
                    foreach (var (key, value) in config ?? new Dictionary<string, object?>())
                    {
                        _configuration[key] = value?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration");
                throw;
            }
        }
    }
}
