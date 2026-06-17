using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;

namespace Smartitecture.Services
{
    public sealed class VoiceInputResult
    {
        public bool Success { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public sealed class VoiceInputService
    {
        public async Task<VoiceInputResult> ListenOnceAsync()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                return new VoiceInputResult
                {
                    Success = false,
                    Message = "Voice input requires Windows 10 version 2004 or newer."
                };
            }

            try
            {
                using var recognizer = CreateRecognizer();
                recognizer.Constraints.Add(new SpeechRecognitionTopicConstraint(
                    SpeechRecognitionScenario.Dictation,
                    "SmartitectureDictation"));

                var compilation = await recognizer.CompileConstraintsAsync();
                if (compilation.Status != SpeechRecognitionResultStatus.Success)
                {
                    return new VoiceInputResult
                    {
                        Success = false,
                        Message = $"Voice input is not ready: {compilation.Status}."
                    };
                }

                var result = await recognizer.RecognizeAsync();
                if (result.Status != SpeechRecognitionResultStatus.Success)
                {
                    return new VoiceInputResult
                    {
                        Success = false,
                        Message = $"Voice input stopped: {result.Status}."
                    };
                }

                return new VoiceInputResult
                {
                    Success = true,
                    Text = result.Text,
                    Message = result.Text
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new VoiceInputResult
                {
                    Success = false,
                    Message = GetPermissionHelpMessage()
                };
            }
            catch (Exception ex)
            {
                if (IsSpeechPrivacyException(ex))
                {
                    return new VoiceInputResult
                    {
                        Success = false,
                        Message = GetPermissionHelpMessage()
                    };
                }

                return new VoiceInputResult
                {
                    Success = false,
                    Message = $"Voice input unavailable: {ex.Message}"
                };
            }
        }

        private static SpeechRecognizer CreateRecognizer()
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            var supported = SpeechRecognizer.SupportedTopicLanguages
                .FirstOrDefault(language => string.Equals(language.LanguageTag, culture, StringComparison.OrdinalIgnoreCase));

            return supported == null
                ? new SpeechRecognizer()
                : new SpeechRecognizer(new Language(supported.LanguageTag));
        }

        private static bool IsSpeechPrivacyException(Exception ex)
        {
            var message = ex.Message ?? string.Empty;
            return message.Contains("speech privacy policy", StringComparison.OrdinalIgnoreCase)
                || message.Contains("privacy", StringComparison.OrdinalIgnoreCase)
                || ex.HResult == unchecked((int)0x80045509);
        }

        private static string GetPermissionHelpMessage()
        {
            return "Voice input needs Windows speech and microphone permission. Open Windows Settings > Privacy & security > Speech and enable Online speech recognition, then open Privacy & security > Microphone and allow microphone access for desktop apps.";
        }
    }
}
