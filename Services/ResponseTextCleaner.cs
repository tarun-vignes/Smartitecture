using System.Text.RegularExpressions;

namespace Smartitecture.Services
{
    public static class ResponseTextCleaner
    {
        public static string ForChatDisplay(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            var text = message.Replace("\r\n", "\n").Replace("\r", "\n");
            text = Regex.Replace(text, @"```[a-zA-Z0-9_-]*\n?", string.Empty);
            text = text.Replace("```", string.Empty);
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");
            text = Regex.Replace(text, @"__(.*?)__", "$1");
            text = Regex.Replace(text, @"`([^`]*)`", "$1");
            text = Regex.Replace(text, @"^\s{0,3}#{1,6}\s+", string.Empty, RegexOptions.Multiline);
            text = Regex.Replace(text, @"^\s*[-*]\s+", "- ", RegexOptions.Multiline);

            return text.Trim();
        }
    }
}
