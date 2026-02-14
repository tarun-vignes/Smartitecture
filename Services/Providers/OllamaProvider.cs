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
    public sealed class OllamaProvider : ILLMProvider
    {
        private static readonly HttpClient Http = new HttpClient();
        private readonly ProviderSettings _settings = ProviderSettings.Load();
        public string Name => "Ollama";
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Ollama.Endpoint);

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
            var endpoint = _settings.Ollama.Endpoint.TrimEnd('/');
            var url = $"{endpoint}/api/chat";

            var messages = new List<object>();
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new { role = "system", content = request.SystemPrompt });
            }

            foreach (var msg in request.History)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }

            messages.Add(new { role = "user", content = request.UserMessage });

            var payload = new
            {
                model = request.Model ?? _settings.Ollama.Model,
                stream = false,
                messages
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await Http.SendAsync(req, cancellationToken);
            var json = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                return new LLMResponse { Content = $"Ollama error: {resp.StatusCode} {json}" };
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement.GetProperty("message").GetProperty("content").GetString();
                return new LLMResponse { Content = content ?? string.Empty };
            }
            catch (Exception ex)
            {
                return new LLMResponse { Content = $"Ollama response parse error: {ex.Message}" };
            }
        }
    }
}
