// MODIFIED: Rebranded from AIPal to Smartitecture
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace Smartitecture.Services
{
    /// <summary>
    /// Service for managing application language and localization.
    /// </summary>
    public class LanguageService
    {
        private const string LanguageSettingKey = "AppLanguage";
        private readonly ApplicationDataContainer _localSettings;
        
        /// <summary>
        /// Event raised when the language changes.
        /// </summary>
        public event EventHandler<string> LanguageChanged;
        
        /// <summary>
        /// Gets the list of supported languages.
        /// </summary>
        public List<LanguageInfo> SupportedLanguages { get; }
        
        /// <summary>
        /// Initializes a new instance of the LanguageService class.
        /// </summary>
        public LanguageService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
            
            // Initialize supported languages
            SupportedLanguages = new List<LanguageInfo>
            {
                new LanguageInfo { Code = "en-US", Name = "English (United States)", NativeName = "English (United States)" },
                new LanguageInfo { Code = "es-ES", Name = "Spanish (Spain)", NativeName = "Español (España)" },
                new LanguageInfo { Code = "fr-FR", Name = "French (France)", NativeName = "Français (France)" },
                new LanguageInfo { Code = "de-DE", Name = "German (Germany)", NativeName = "Deutsch (Deutschland)" },
                new LanguageInfo { Code = "it-IT", Name = "Italian (Italy)", NativeName = "Italiano (Italia)" },
                new LanguageInfo { Code = "pt-BR", Name = "Portuguese (Brazil)", NativeName = "Português (Brasil)" },
                new LanguageInfo { Code = "ru-RU", Name = "Russian (Russia)", NativeName = "Русский (Россия)" },
                new LanguageInfo { Code = "zh-CN", Name = "Chinese Simplified (China)", NativeName = "简体中文 (中国)" },
                new LanguageInfo { Code = "ja-JP", Name = "Japanese (Japan)", NativeName = "日本語 (日本)" },
                new LanguageInfo { Code = "ko-KR", Name = "Korean (Korea)", NativeName = "한국어 (대한민국)" },
                new LanguageInfo { Code = "ar-SA", Name = "Arabic (Saudi Arabia)", NativeName = "العربية (المملكة العربية السعودية)" },
                new LanguageInfo { Code = "hi-IN", Name = "Hindi (India)", NativeName = "हिन्दी (भारत)" },
                new LanguageInfo { Code = "tr-TR", Name = "Turkish (Turkey)", NativeName = "Türkçe (Türkiye)" },
                new LanguageInfo { Code = "nl-NL", Name = "Dutch (Netherlands)", NativeName = "Nederlands (Nederland)" },
                new LanguageInfo { Code = "pl-PL", Name = "Polish (Poland)", NativeName = "Polski (Polska)" },
                new LanguageInfo { Code = "sv-SE", Name = "Swedish (Sweden)", NativeName = "Svenska (Sverige)" },
                new LanguageInfo { Code = "fi-FI", Name = "Finnish (Finland)", NativeName = "Suomi (Suomi)" },
                new LanguageInfo { Code = "da-DK", Name = "Danish (Denmark)", NativeName = "Dansk (Danmark)" },
                new LanguageInfo { Code = "no-NO", Name = "Norwegian (Norway)", NativeName = "Norsk (Norge)" },
                new LanguageInfo { Code = "cs-CZ", Name = "Czech (Czech Republic)", NativeName = "Čeština (Česká republika)" },
                new LanguageInfo { Code = "hu-HU", Name = "Hungarian (Hungary)", NativeName = "Magyar (Magyarország)" },
                new LanguageInfo { Code = "el-GR", Name = "Greek (Greece)", NativeName = "Ελληνικά (Ελλάδα)" },
                new LanguageInfo { Code = "he-IL", Name = "Hebrew (Israel)", NativeName = "עברית (ישראל)" },
                new LanguageInfo { Code = "th-TH", Name = "Thai (Thailand)", NativeName = "ไทย (ประเทศไทย)" },
                new LanguageInfo { Code = "vi-VN", Name = "Vietnamese (Vietnam)", NativeName = "Tiếng Việt (Việt Nam)" },
                new LanguageInfo { Code = "id-ID", Name = "Indonesian (Indonesia)", NativeName = "Bahasa Indonesia (Indonesia)" },
                new LanguageInfo { Code = "ms-MY", Name = "Malay (Malaysia)", NativeName = "Bahasa Melayu (Malaysia)" },
                new LanguageInfo { Code = "fil-PH", Name = "Filipino (Philippines)", NativeName = "Filipino (Pilipinas)" },
                new LanguageInfo { Code = "uk-UA", Name = "Ukrainian (Ukraine)", NativeName = "Українська (Україна)" },
                new LanguageInfo { Code = "ro-RO", Name = "Romanian (Romania)", NativeName = "Română (România)" }
            };
        }
        
        /// <summary>
        /// Gets the current language code.
        /// </summary>
        public string CurrentLanguage
        {
            get
            {
                if (_localSettings.Values.TryGetValue(LanguageSettingKey, out var value) && 
                    value is string languageCode)
                {
                    return languageCode;
                }
                
                return "en-US"; // Default to English
            }
        }
        
        /// <summary>
        /// Gets the current language info.
        /// </summary>
        public LanguageInfo CurrentLanguageInfo
        {
            get
            {
                string code = CurrentLanguage;
                return SupportedLanguages.Find(l => l.Code == code) ?? SupportedLanguages[0];
            }
        }
        
        /// <summary>
        /// Sets the application language.
        /// </summary>
        /// <param name="languageCode">The language code to set</param>
        public async Task SetLanguageAsync(string languageCode)
        {
            // Validate the language code
            if (!SupportedLanguages.Exists(l => l.Code == languageCode))
            {
                throw new ArgumentException($"Unsupported language code: {languageCode}");
            }
            
            // Save the language setting
            _localSettings.Values[LanguageSettingKey] = languageCode;
            
            // Apply the language
            await ApplyLanguageAsync(languageCode);
            
            // Raise the language changed event
            LanguageChanged?.Invoke(this, languageCode);
        }
        
        /// <summary>
        /// Initializes the language based on saved settings.
        /// </summary>
        public async Task InitializeLanguageAsync()
        {
            await ApplyLanguageAsync(CurrentLanguage);
        }
        
        /// <summary>
        /// Applies the specified language to the application.
        /// </summary>
        /// <param name="languageCode">The language code to apply</param>
        private async Task ApplyLanguageAsync(string languageCode)
        {
            try
            {
                // Set the current culture
                var culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                
                // In a real implementation, we would also update the ResourceLoader
                // For now, we'll just simulate the language change
                
                // Wait for the UI thread to process the language change
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error applying language: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets a localized string for the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The resource key</param>
        /// <returns>The localized string</returns>
        public string GetString(string resourceKey)
        {
            try
            {
                // In a real implementation, we would use the ResourceLoader to get localized strings
                // For now, we'll just return the resource key
                return resourceKey;
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error getting localized string: {ex.Message}");
                return resourceKey;
            }
        }
    }
    
    /// <summary>
    /// Represents information about a language.
    /// </summary>
    public class LanguageInfo
    {
        /// <summary>
        /// Gets or sets the language code.
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Gets or sets the language name in English.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the language name in its native language.
        /// </summary>
        public string NativeName { get; set; }
    }
}
