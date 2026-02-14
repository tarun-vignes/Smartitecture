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
    public sealed class AnthropicProvider : ILLMProvider
    {
        private static readonly HttpClient Http = new HttpClient();
        private readonly ProviderSettings _settings = ProviderSettings.Load();
        public string Name => "Anthropic";
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Anthropic.ApiKey);

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
            var messages = new List<object>();
            foreach (var msg in request.History)
            {
                if (msg.Role.Equals("system", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                messages.Add(new { role = msg.Role, content = msg.Content });
            }
            messages.Add(new { role = "user", content = request.UserMessage });

            var payload = new
            {
                model = _settings.Anthropic.Model,
                max_tokens = _settings.Anthropic.MaxTokens,
                temperature = _settings.Anthropic.Temperature,
                system = request.SystemPrompt,
                messages
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            req.Headers.Add("x-api-key", _settings.Anthropic.ApiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await Http.SendAsync(req, cancellationToken);
            var json = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                return new LLMResponse { Content = $"Anthropic error: {resp.StatusCode} {json}" };
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
                return new LLMResponse { Content = content ?? string.Empty };
            }
            catch (Exception ex)
            {
                return new LLMResponse { Content = $"Anthropic response parse error: {ex.Message}" };
            }
        }
    }
}
