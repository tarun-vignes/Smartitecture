using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Smartitecture.Services
{
    public static class LocalizationManager
    {
        private const string StringsRoot = "Resources/Strings/";

        public sealed record LanguageOption(string Code, string DisplayName);

        private static readonly string[] SupportedLanguageCodes =
        [
            "af-ZA",
            "am-ET",
            "ar",
            "az-Latn-AZ",
            "bg-BG",
            "bn-BD",
            "ca-ES",
            "cs-CZ",
            "da-DK",
            "de-DE",
            "el-GR",
            "en-AU",
            "en-CA",
            "en-GB",
            "en-IN",
            "en-US",
            "es-ES",
            "et-EE",
            "eu-ES",
            "fa-IR",
            "fi-FI",
            "fil-PH",
            "fr-FR",
            "gl-ES",
            "gu-IN",
            "he-IL",
            "hi-IN",
            "hr-HR",
            "hu-HU",
            "id-ID",
            "is-IS",
            "it-IT",
            "ja-JP",
            "km-KH",
            "kn-IN",
            "ko-KR",
            "lt-LT",
            "lv-LV",
            "ml-IN",
            "mr-IN",
            "ms-MY",
            "my-MM",
            "nb-NO",
            "ne-NP",
            "nl-NL",
            "pa-IN",
            "pl-PL",
            "pt-BR",
            "pt-PT",
            "ro-RO",
            "ru-RU",
            "si-LK",
            "sk-SK",
            "sl-SI",
            "sr-RS",
            "sv-SE",
            "sw-KE",
            "ta-IN",
            "te-IN",
            "th-TH",
            "tr-TR",
            "uk-UA",
            "ur-PK",
            "vi-VN",
            "zh-CN",
            "zh-TW",
            "zu-ZA",
        ];

        private static readonly HashSet<string> SupportedLanguages = new(SupportedLanguageCodes, StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> DisplayNameMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["English (US)"] = "en-US",
                ["English (UK)"] = "en-GB",
                ["English (Canada)"] = "en-CA",
                ["English (Australia)"] = "en-AU",
                ["English (India)"] = "en-IN",
                ["Spanish"] = "es-ES",
                ["French"] = "fr-FR",
                ["German"] = "de-DE",
                ["Chinese (Simplified)"] = "zh-CN",
                ["Chinese (Traditional)"] = "zh-TW",
                ["Portuguese (Portugal)"] = "pt-PT",
                ["Portuguese (Brazil)"] = "pt-BR",
                ["Afrikaans"] = "af-ZA",
                ["Amharic"] = "am-ET",
                ["Arabic"] = "ar",
                ["Azerbaijani"] = "az-Latn-AZ",
                ["Basque"] = "eu-ES",
                ["Bengali"] = "bn-BD",
                ["Bulgarian"] = "bg-BG",
                ["Burmese"] = "my-MM",
                ["Catalan"] = "ca-ES",
                ["Croatian"] = "hr-HR",
                ["Czech"] = "cs-CZ",
                ["Danish"] = "da-DK",
                ["Dutch"] = "nl-NL",
                ["Estonian"] = "et-EE",
                ["Filipino"] = "fil-PH",
                ["Finnish"] = "fi-FI",
                ["Galician"] = "gl-ES",
                ["Greek"] = "el-GR",
                ["Gujarati"] = "gu-IN",
                ["Hebrew"] = "he-IL",
                ["Hindi"] = "hi-IN",
                ["Hungarian"] = "hu-HU",
                ["Icelandic"] = "is-IS",
                ["Indonesian"] = "id-ID",
                ["Italian"] = "it-IT",
                ["Japanese"] = "ja-JP",
                ["Kannada"] = "kn-IN",
                ["Khmer"] = "km-KH",
                ["Korean"] = "ko-KR",
                ["Latvian"] = "lv-LV",
                ["Lithuanian"] = "lt-LT",
                ["Malay"] = "ms-MY",
                ["Malayalam"] = "ml-IN",
                ["Marathi"] = "mr-IN",
                ["Nepali"] = "ne-NP",
                ["Norwegian"] = "nb-NO",
                ["Persian"] = "fa-IR",
                ["Polish"] = "pl-PL",
                ["Portuguese"] = "pt-PT",
                ["Punjabi"] = "pa-IN",
                ["Romanian"] = "ro-RO",
                ["Russian"] = "ru-RU",
                ["Serbian"] = "sr-RS",
                ["Sinhala"] = "si-LK",
                ["Slovak"] = "sk-SK",
                ["Slovenian"] = "sl-SI",
                ["Swahili"] = "sw-KE",
                ["Swedish"] = "sv-SE",
                ["Tamil"] = "ta-IN",
                ["Telugu"] = "te-IN",
                ["Thai"] = "th-TH",
                ["Turkish"] = "tr-TR",
                ["Ukrainian"] = "uk-UA",
                ["Urdu"] = "ur-PK",
                ["Vietnamese"] = "vi-VN",
                ["Zulu"] = "zu-ZA"
            };

        public static void Apply(string? language)
        {
            var (file, cultureName) = ResolveLanguage(language);
            var app = Application.Current;
            if (app == null)
            {
                return;
            }

            var merged = app.Resources.MergedDictionaries;
            var toRemove = new List<ResourceDictionary>();
            foreach (var dict in merged)
            {
                if (dict.Source != null && dict.Source.OriginalString.Contains(StringsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    toRemove.Add(dict);
                }
            }
            foreach (var dict in toRemove)
            {
                merged.Remove(dict);
            }

            merged.Add(new ResourceDictionary
            {
                Source = new Uri($"{StringsRoot}en-US.xaml", UriKind.Relative)
            });

            if (!string.Equals(file, "en-US.xaml", StringComparison.OrdinalIgnoreCase))
            {
                merged.Add(new ResourceDictionary
                {
                    Source = new Uri($"{StringsRoot}{file}", UriKind.Relative)
                });
            }

            TryApplyCulture(cultureName);
            ApplyFlowDirection(cultureName);
        }

        public static IReadOnlyList<LanguageOption> GetSupportedLanguageOptions()
        {
            var options = new List<LanguageOption>(SupportedLanguageCodes.Length);
            foreach (var code in SupportedLanguageCodes)
            {
                options.Add(new LanguageOption(code, GetDisplayName(code)));
            }

            return options;
        }

        private static (string file, string culture) ResolveLanguage(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return ("en-US.xaml", "en-US");
            }

            var key = language.Trim();
            if (DisplayNameMap.TryGetValue(key, out var mapped))
            {
                key = mapped;
            }

            if (SupportedLanguages.Contains(key))
            {
                return ($"{key}.xaml", key);
            }

            return ("en-US.xaml", "en-US");
        }

        private static string GetDisplayName(string code)
        {
            try
            {
                var culture = new CultureInfo(code);
                return $"{culture.EnglishName} ({code})";
            }
            catch
            {
                return code;
            }
        }

        private static void TryApplyCulture(string cultureName)
        {
            try
            {
                var culture = new CultureInfo(cultureName);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch
            {
            }
        }

        private static void ApplyFlowDirection(string cultureName)
        {
            try
            {
                var culture = new CultureInfo(cultureName);
                var direction = culture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

                foreach (Window window in Application.Current.Windows)
                {
                    window.FlowDirection = direction;
                }
            }
            catch
            {
            }
        }
    }
}
