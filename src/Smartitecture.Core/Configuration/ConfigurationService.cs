using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Smartitecture.Core.Configuration
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationService> _logger;
        private readonly AppSettings _appSettings;

        public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _appSettings = configuration.Get<AppSettings>();
            ValidateConfiguration();
        }

        public AppSettings GetAppSettings() => _appSettings;

        private void ValidateConfiguration()
        {
            var validationContext = new ValidationContext(_appSettings);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(_appSettings, validationContext, validationResults, true))
            {
                foreach (var validationResult in validationResults)
                {
                    _logger.LogError("Configuration validation error: {ErrorMessage}", validationResult.ErrorMessage);
                }
                throw new ConfigurationException("Configuration validation failed. Please check the logs for details.");
            }

            _logger.LogInformation("Configuration validated successfully");
        }
    }
}
