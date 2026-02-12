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

        private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
        {
            "af-ZA",
            "am-ET",
            "ar",
            "az-Latn-AZ",
            "eu-ES",
            "bn-BD",
            "bg-BG",
            "my-MM",
            "ca-ES",
            "zh-CN",
            "zh-TW",
            "hr-HR",
            "cs-CZ",
            "da-DK",
            "nl-NL",
            "en-US",
            "en-GB",
            "en-CA",
            "en-AU",
            "en-IN",
            "et-EE",
            "fil-PH",
            "fi-FI",
            "fr-FR",
            "gl-ES",
            "de-DE",
            "el-GR",
            "gu-IN",
            "he-IL",
            "hi-IN",
            "hu-HU",
            "is-IS",
            "id-ID",
            "it-IT",
            "ja-JP",
            "kn-IN",
            "km-KH",
            "ko-KR",
            "lv-LV",
            "lt-LT",
            "ms-MY",
            "ml-IN",
            "mr-IN",
            "ne-NP",
            "nb-NO",
            "fa-IR",
            "pl-PL",
            "pt-PT",
            "pt-BR",
            "pa-IN",
            "ro-RO",
            "ru-RU",
            "sr-RS",
            "si-LK",
            "sk-SK",
            "sl-SI",
            "es-ES",
            "sw-KE",
            "sv-SE",
            "ta-IN",
            "te-IN",
            "th-TH",
            "tr-TR",
            "uk-UA",
            "ur-PK",
            "vi-VN",
            "zu-ZA"
        };

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
    }
}
