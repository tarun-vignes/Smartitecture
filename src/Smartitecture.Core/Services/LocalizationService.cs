using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace Smartitecture.Core.Services
{
    public interface ILocalizationService
    {
        string GetResource(string key, string culture = "en-US");
        string GetResource(string key, string[] args, string culture = "en-US");
        Task<string> GetResourceAsync(string key, string culture = "en-US");
        Task<string> GetResourceAsync(string key, string[] args, string culture = "en-US");
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly ILogger<LocalizationService> _logger;

        public LocalizationService(IStringLocalizerFactory localizerFactory, ILogger<LocalizationService> logger)
        {
            _localizerFactory = localizerFactory;
            _logger = logger;
        }

        public string GetResource(string key, string culture = "en-US")
        {
            try
            {
                var localizer = _localizerFactory.Create(typeof(LocalizationService));
                return localizer.WithCulture(new CultureInfo(culture))[key].Value ?? key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource for key {Key}", key);
                return key;
            }
        }

        public string GetResource(string key, string[] args, string culture = "en-US")
        {
            try
            {
                var localizer = _localizerFactory.Create(typeof(LocalizationService));
                return string.Format(localizer.WithCulture(new CultureInfo(culture))[key].Value ?? key, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resource for key {Key}", key);
                return key;
            }
        }

        public async Task<string> GetResourceAsync(string key, string culture = "en-US")
        {
            return GetResource(key, culture);
        }

        public async Task<string> GetResourceAsync(string key, string[] args, string culture = "en-US")
        {
            return GetResource(key, args, culture);
        }
    }
}
