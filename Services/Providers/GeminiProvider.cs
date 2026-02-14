using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Providers
{
    public sealed class GeminiProvider : ILLMProvider
    {
        private static readonly HttpClient Http = new HttpClient();
        private readonly ProviderSettings _settings = ProviderSettings.Load();
        public string Name => "Gemini";
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Gemini.ApiKey);

        public Task<LLMResponse> GetResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
            {
                return Task.FromResult(new LLMResponse
                {
                    Content = "This model is not available right now. Using the local assistant instead."
                });
            }

            return SendAsync(request, cancellationToken);
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
            {
                yield return "This model is not available right now. Using the local assistant instead.";
                yield break;
            }

            var response = await SendAsync(request, cancellationToken);
            foreach (var token in response.Content.Split(' '))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return (token == response.Content ? token : " " + token);
            }
        }

        private async Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            var model = _settings.Gemini.Model;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_settings.Gemini.ApiKey}";

            var contents = new List<object>();
            foreach (var msg in request.History)
            {
                contents.Add(new
                {
                    role = msg.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = msg.Content } }
                });
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = request.UserMessage } }
            });

            var payload = new
            {
                contents,
                systemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt)
                    ? null
                    : new { parts = new[] { new { text = request.SystemPrompt } } },
                generationConfig = new
                {
                    temperature = _settings.Gemini.Temperature
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            var options = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
            req.Content = new StringContent(JsonSerializer.Serialize(payload, options), Encoding.UTF8, "application/json");

            using var resp = await Http.SendAsync(req, cancellationToken);
            var json = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                return new LLMResponse { Content = $"Gemini error: {resp.StatusCode} {json}" };
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement.GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return new LLMResponse { Content = content ?? string.Empty };
            }
            catch (Exception ex)
            {
                return new LLMResponse { Content = $"Gemini response parse error: {ex.Message}" };
            }
        }
    }
}
