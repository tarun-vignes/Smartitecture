using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Smartitecture.Services.Core
{
    public static class ToolConfirmationParser
    {
        private static readonly Regex ConfirmBlockRegex = new Regex(
            @"```confirm\s*(?<json>\{[\s\S]*?\})\s*```",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryExtractConfirmation(string content, out string cleanedContent, out ToolCall? call)
        {
            call = null;
            cleanedContent = content ?? string.Empty;

            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            var match = ConfirmBlockRegex.Match(content);
            if (!match.Success)
            {
                return false;
            }

            var json = match.Groups["json"].Value;
            if (ToolCallParser.TryExtractToolCalls($"```tool\n{json}\n```", out _, out var calls) && calls.Count > 0)
            {
                call = calls[0];
                cleanedContent = ConfirmBlockRegex.Replace(content, string.Empty).Trim();
                return true;
            }

            return false;
        }
    }
}
