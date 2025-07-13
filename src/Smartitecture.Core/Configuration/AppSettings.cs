using System.ComponentModel.DataAnnotations;

namespace Smartitecture.Core.Configuration
{
    public class AppSettings
    {
        [Required]
        public string Environment { get; set; } = "Development";

        public LoggingSettings Logging { get; set; } = new LoggingSettings();
        public SecuritySettings Security { get; set; } = new SecuritySettings();
        public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();
    }

    public class LoggingSettings
    {
        public string LogLevel { get; set; } = "Information";
        public bool EnableFileLogging { get; set; } = true;
        public bool EnableConsoleLogging { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/app.log";
    }

    public class SecuritySettings
    {
        public string JwtSecret { get; set; } = "YourSecretKeyHere";
        public int TokenExpirationMinutes { get; set; } = 60;
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }

    public class OpenAIConfig
    {
        [Required]
        public string ApiKey { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = "https://api.openai.com/v1";
        public int MaxRetries { get; set; } = 3;
        public int RequestTimeoutSeconds { get; set; } = 30;
    }
}
