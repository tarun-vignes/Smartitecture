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
    public sealed class AzureOpenAIProvider : ILLMProvider
    {
        private static readonly HttpClient Http = new HttpClient();
        private readonly ProviderSettings _settings = ProviderSettings.Load();
        public string Name => "Azure OpenAI";
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.Endpoint) &&
            !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.ApiKey) &&
            !string.IsNullOrWhiteSpace(_settings.AzureOpenAI.DeploymentName);

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
            var endpoint = _settings.AzureOpenAI.Endpoint.TrimEnd('/');
            var deployment = _settings.AzureOpenAI.DeploymentName;
            var apiVersion = _settings.AzureOpenAI.ApiVersion;
            var url = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

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
                messages,
                temperature = _settings.AzureOpenAI.Temperature,
                max_tokens = _settings.AzureOpenAI.MaxTokens
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("api-key", _settings.AzureOpenAI.ApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var resp = await Http.SendAsync(req, cancellationToken);
            var json = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                return new LLMResponse { Content = $"Azure OpenAI error: {resp.StatusCode} {json}" };
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return new LLMResponse { Content = content ?? string.Empty };
            }
            catch (Exception ex)
            {
                return new LLMResponse { Content = $"Azure OpenAI response parse error: {ex.Message}" };
            }
        }
    }
}
