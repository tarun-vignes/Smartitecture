using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smartitecture.Services
{
    /// <summary>
    /// Service for managing application configuration including API keys
    /// </summary>
    public class ConfigurationService
    {
        private readonly string _configPath;
        private AppConfig _config = new AppConfig();

        public ConfigurationService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var smartitectureFolder = Path.Combine(appDataPath, "Smartitecture");
            
            if (!Directory.Exists(smartitectureFolder))
            {
                Directory.CreateDirectory(smartitectureFolder);
            }
            
            _configPath = Path.Combine(smartitectureFolder, "config.json");
            LoadConfiguration();
        }

        public AppConfig GetConfiguration()
        {
            return _config;
        }

        public async Task SaveConfigurationAsync(AppConfig config)
        {
            _config = config;
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(_config, options);
            await File.WriteAllTextAsync(_configPath, json);
        }

        public async Task<bool> SetOpenAIApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            _config.OpenAI.ApiKey = apiKey;
            await SaveConfigurationAsync(_config);
            return true;
        }

        public string GetOpenAIApiKey()
        {
            return _config.OpenAI.ApiKey;
        }

        public bool IsOpenAIConfigured()
        {
            return !string.IsNullOrWhiteSpace(_config.OpenAI.ApiKey);
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                else
                {
                    _config = new AppConfig();
                }
            }
            catch (Exception)
            {
                // If config is corrupted, create new one
                _config = new AppConfig();
            }
        }
    }

    /// <summary>
    /// Application configuration model
    /// </summary>
    public class AppConfig
    {
        public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();
        public UIConfig UI { get; set; } = new UIConfig();
        public SystemConfig System { get; set; } = new SystemConfig();
    }

    /// <summary>
    /// UI configuration settings
    /// </summary>
    public class UIConfig
    {
        public string Theme { get; set; } = "Dark";
        public bool EnableAnimations { get; set; } = true;
        public bool ShowTypingIndicator { get; set; } = true;
        public string DefaultModel { get; set; } = "Advanced AI Assistant";
    }

    /// <summary>
    /// System configuration settings
    /// </summary>
    public class SystemConfig
    {
        public bool EnableSystemCommands { get; set; } = true;
        public bool EnableFileOperations { get; set; } = true;
        public bool EnableNetworkOperations { get; set; } = false;
        public int MaxConversationHistory { get; set; } = 50;
    }
}
