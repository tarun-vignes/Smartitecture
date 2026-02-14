using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Smartitecture.Services.Core
{
    public static class ToolCallParser
    {
        private static readonly Regex ToolBlockRegex = new Regex(
            @"```tool\s*(?<json>\{[\s\S]*?\})\s*```",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryExtractToolCalls(string content, out string cleanedContent, out List<ToolCall> calls)
        {
            calls = new List<ToolCall>();
            cleanedContent = content ?? string.Empty;

            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            var matches = ToolBlockRegex.Matches(content);
            if (matches.Count == 0)
            {
                return false;
            }

            foreach (Match match in matches)
            {
                var json = match.Groups["json"].Value;
                if (TryParseToolJson(json, out var parsed))
                {
                    calls.AddRange(parsed);
                }
            }

            cleanedContent = ToolBlockRegex.Replace(content, string.Empty).Trim();
            return calls.Count > 0;
        }

        private static bool TryParseToolJson(string json, out List<ToolCall> calls)
        {
            calls = new List<ToolCall>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in toolCalls.EnumerateArray())
                        {
                            var call = ParseSingleCall(item);
                            if (call != null)
                            {
                                calls.Add(call);
                            }
                        }
                        return calls.Count > 0;
                    }

                    var single = ParseSingleCall(root);
                    if (single != null)
                    {
                        calls.Add(single);
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static ToolCall? ParseSingleCall(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            string? name = null;
            if (element.TryGetProperty("name", out var nameProp))
            {
                name = nameProp.GetString();
            }
            else if (element.TryGetProperty("tool", out var toolProp))
            {
                name = toolProp.GetString();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string argsJson = "{}";
            if (element.TryGetProperty("arguments", out var argsProp))
            {
                argsJson = argsProp.GetRawText();
            }
            else if (element.TryGetProperty("args", out var argsAlt))
            {
                argsJson = argsAlt.GetRawText();
            }

            return new ToolCall
            {
                Name = name.Trim(),
                ArgumentsJson = argsJson
            };
        }
    }
}
