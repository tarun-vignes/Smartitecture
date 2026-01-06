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
        private AppConfig _config;

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
            // Prefer stored config; fall back to environment variables for developer convenience
            if (!string.IsNullOrWhiteSpace(_config.OpenAI.ApiKey))
                return _config.OpenAI.ApiKey;

            var env = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            return env ?? string.Empty;
        }

        public bool IsOpenAIConfigured()
        {
            return !string.IsNullOrWhiteSpace(GetOpenAIApiKey());
        }

        public async Task<bool> SetClaudeApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            _config.Claude.ApiKey = apiKey;
            await SaveConfigurationAsync(_config);
            return true;
        }

        public string GetClaudeApiKey()
        {
            // Prefer stored config; fall back to environment variables for developer convenience
            if (!string.IsNullOrWhiteSpace(_config.Claude.ApiKey))
                return _config.Claude.ApiKey;

            var env = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
                      ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
            return env ?? string.Empty;
        }

        public bool IsClaudeConfigured()
        {
            return !string.IsNullOrWhiteSpace(GetClaudeApiKey());
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
        public ClaudeConfig Claude { get; set; } = new ClaudeConfig();
        public UIConfig UI { get; set; } = new UIConfig();
        public SystemConfig System { get; set; } = new SystemConfig();
    }

    /// <summary>
    /// Claude (Anthropic) configuration settings
    /// </summary>
    public class ClaudeConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "claude-3-5-sonnet-20240620";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 2000;
        public int MaxRetries { get; set; } = 3;
        public int RateLimitPerMinute { get; set; } = 50;
        public string ApiVersion { get; set; } = "2023-06-01";
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
