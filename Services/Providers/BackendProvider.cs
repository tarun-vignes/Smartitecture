using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Providers
{
    public sealed class BackendProvider : ILLMProvider
    {
        private static readonly HttpClient Http = new HttpClient();
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ProviderSettings _settings = ProviderSettings.Load();

        public string Name => "Smartitecture Backend";
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Backend.BaseUrl);

        public async Task<LLMResponse> GetResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
            {
                return new LLMResponse
                {
                    Content = "Smartitecture backend is not configured. Using the local assistant instead."
                };
            }

            ConfigureTimeout();

            var payload = BuildRequest(request);
            var baseUrl = _settings.Backend.BaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/v1/chat";

            using var message = new HttpRequestMessage(HttpMethod.Post, url);
            var apiKey = _settings.Backend.ApiKey;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                message.Headers.Add("X-API-Key", apiKey);
            }

            message.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            using var response = await Http.SendAsync(message, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new LLMResponse
                {
                    Content = $"Backend error: {(int)response.StatusCode} {response.ReasonPhrase}. {json}"
                };
            }

            return ParseResponse(json);
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
            {
                yield return "Smartitecture backend is not configured. Using the local assistant instead.";
                yield break;
            }

            ConfigureTimeout();

            var payload = BuildRequest(request);
            var baseUrl = _settings.Backend.BaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/v1/chat/stream";

            using var message = new HttpRequestMessage(HttpMethod.Post, url);
            var apiKey = _settings.Backend.ApiKey;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                message.Headers.Add("X-API-Key", apiKey);
            }

            message.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            using var response = await Http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                yield return $"Backend error: {(int)response.StatusCode} {response.ReasonPhrase}. {error}";
                yield break;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new System.IO.StreamReader(stream);
            var finalToolCalls = new List<ToolCall>();
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!line.StartsWith("data: ", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var payloadLine = line.Substring("data: ".Length);
                if (string.IsNullOrWhiteSpace(payloadLine))
                {
                    continue;
                }

                if (!TryParseStreamPayload(payloadLine, out var token, out var done, out var parsedToolCalls, out var errorMessage))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    yield return errorMessage;
                    yield break;
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    yield return token;
                }

                if (parsedToolCalls != null)
                {
                    finalToolCalls.Clear();
                    finalToolCalls.AddRange(parsedToolCalls);
                }

                if (done)
                {
                    if (finalToolCalls.Count > 0)
                    {
                        var toolBlock = BuildToolBlock(finalToolCalls);
                        if (!string.IsNullOrWhiteSpace(toolBlock))
                        {
                            yield return toolBlock;
                        }
                    }
                    yield break;
                }
            }
        }

        private static bool TryParseStreamPayload(
            string payloadLine,
            out string? token,
            out bool done,
            out List<ToolCall>? toolCalls,
            out string? errorMessage)
        {
            token = null;
            done = false;
            toolCalls = null;
            errorMessage = null;

            try
            {
                using var doc = JsonDocument.Parse(payloadLine);
                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    errorMessage = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Backend error.";
                    return true;
                }

                if (doc.RootElement.TryGetProperty("token", out var tokenElement))
                {
                    token = tokenElement.GetString();
                }

                if (doc.RootElement.TryGetProperty("toolCalls", out var toolCallsElement) &&
                    toolCallsElement.ValueKind == JsonValueKind.Array)
                {
                    var parsed = new List<ToolCall>();
                    foreach (var tool in toolCallsElement.EnumerateArray())
                    {
                        if (!tool.TryGetProperty("name", out var nameElement))
                        {
                            continue;
                        }

                        var name = nameElement.GetString();
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        var args = tool.TryGetProperty("argumentsJson", out var argsElement)
                            ? argsElement.GetString() ?? "{}"
                            : "{}";

                        parsed.Add(new ToolCall
                        {
                            Name = name,
                            ArgumentsJson = args
                        });
                    }

                    toolCalls = parsed;
                }

                if (doc.RootElement.TryGetProperty("done", out var doneElement))
                {
                    done = doneElement.GetBoolean();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildToolBlock(IReadOnlyList<ToolCall> toolCalls)
        {
            if (toolCalls.Count == 0)
            {
                return string.Empty;
            }

            if (toolCalls.Count == 1)
            {
                var single = toolCalls[0];
                return $"\n```tool\n{{\"name\":\"{single.Name}\",\"arguments\":{single.ArgumentsJson}}}\n```\n";
            }

            var entries = new StringBuilder();
            for (var i = 0; i < toolCalls.Count; i++)
            {
                if (i > 0)
                {
                    entries.Append(",");
                }

                var call = toolCalls[i];
                entries.Append($"{{\"name\":\"{call.Name}\",\"arguments\":{call.ArgumentsJson}}}");
            }

            return $"\n```tool\n{{\"tool_calls\":[{entries}]}}\n```\n";
        }

        private static BackendChatRequest BuildRequest(LLMRequest request)
        {
            var messages = new List<BackendMessage>();

            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new BackendMessage
                {
                    Role = "system",
                    Content = request.SystemPrompt
                });
            }

            foreach (var message in request.History)
            {
                messages.Add(new BackendMessage
                {
                    Role = message.Role,
                    Content = message.Content
                });
            }

            messages.Add(new BackendMessage
            {
                Role = "user",
                Content = request.UserMessage
            });

            var tools = new List<BackendTool>();
            foreach (var tool in request.Tools)
            {
                tools.Add(new BackendTool
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    JsonSchema = tool.JsonSchema
                });
            }

            return new BackendChatRequest
            {
                Model = request.Model,
                Messages = messages,
                Tools = tools
            };
        }

        private static LLMResponse ParseResponse(string json)
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<BackendChatResponse>(json, JsonOptions);
                if (parsed != null && (!string.IsNullOrWhiteSpace(parsed.Content) || parsed.ToolCalls.Count > 0))
                {
                    return new LLMResponse
                    {
                        Content = parsed.Content ?? string.Empty,
                        ToolCalls = parsed.ToolCalls
                    };
                }
            }
            catch
            {
                // Fall through to best-effort parsing.
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("content", out var content))
                {
                    return new LLMResponse { Content = content.GetString() ?? string.Empty };
                }

                if (doc.RootElement.TryGetProperty("message", out var message))
                {
                    return new LLMResponse { Content = message.GetString() ?? string.Empty };
                }

                if (doc.RootElement.TryGetProperty("result", out var result))
                {
                    return new LLMResponse { Content = result.GetString() ?? string.Empty };
                }
            }
            catch
            {
                // Ignore parse issues.
            }

            return new LLMResponse { Content = json };
        }

        private void ConfigureTimeout()
        {
            if (_settings.Backend.TimeoutSeconds <= 0)
            {
                return;
            }

            Http.Timeout = TimeSpan.FromSeconds(_settings.Backend.TimeoutSeconds);
        }

        private sealed class BackendChatRequest
        {
            public string? Model { get; set; }
            public List<BackendMessage> Messages { get; set; } = new();
            public List<BackendTool> Tools { get; set; } = new();
        }

        private sealed class BackendMessage
        {
            public string Role { get; set; } = "user";
            public string Content { get; set; } = string.Empty;
        }

        private sealed class BackendTool
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string JsonSchema { get; set; } = string.Empty;
        }

        private sealed class BackendChatResponse
        {
            public string? Content { get; set; }
            public List<ToolCall> ToolCalls { get; set; } = new();
        }
    }
}
