using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Smartitecture.Services
{
    /// <summary>
    /// Lightweight Anthropic Claude client with retry, basic rate limiting, and cost tracking.
    /// Uses HTTP API directly to avoid hard dependency on SDK.
    /// </summary>
    public class ClaudeService
    {
        private readonly HttpClient _http;
        private readonly ConfigurationService _config;
        private readonly HumanLikeConversationEngine _fallback;

        // Usage tracking
        public int LastInputTokens { get; private set; }
        public int LastOutputTokens { get; private set; }
        public decimal LastEstimatedCostUSD { get; private set; }

        public ClaudeService()
        {
            _http = new HttpClient();
            _config = new ConfigurationService();
            _fallback = new HumanLikeConversationEngine();
        }

        public bool IsConfigured()
        {
            return _config.IsClaudeConfigured();
        }

        public async Task<string> GetResponseAsync(string message, List<ConversationMessage> history = null, string modelOverride = null)
        {
            if (!IsConfigured())
            {
                return await _fallback.GetResponseAsync(message, "claude-fallback", history);
            }

            var apiKey = _config.GetClaudeApiKey();
            var settings = _config.GetConfiguration().Claude;
            var model = modelOverride ?? settings.DefaultModel;

            var payload = BuildMessagesPayload(model, message, history, settings);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", settings.ApiVersion);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var responseText = await SendWithRetryAsync(request, settings.MaxRetries);
            if (responseText == null)
            {
                return await _fallback.GetResponseAsync(message, "claude-error", history);
            }

            var parsed = JsonSerializer.Deserialize<ClaudeMessageResponse>(responseText);
            var content = parsed?.ContentText ?? "";

            // Usage/cost tracking (rough; values depend on model pricing)
            LastInputTokens = parsed?.Usage?.InputTokens ?? 0;
            LastOutputTokens = parsed?.Usage?.OutputTokens ?? 0;
            LastEstimatedCostUSD = EstimateCostUSD(model, LastInputTokens, LastOutputTokens);

            // Append usage footer for visibility in UI
            if (LastInputTokens > 0 || LastOutputTokens > 0)
            {
                content += $"\n\n— Tokens in/out: {LastInputTokens}/{LastOutputTokens} • Est. cost: ${LastEstimatedCostUSD:0.0000}";
            }

            return content;
        }

        public async Task<string> GetStreamingResponseAsync(string message, Action<string> onToken, List<ConversationMessage> history = null, string modelOverride = null)
        {
            // Use non-streaming API call then emit tokens progressively for simplicity
            var full = await GetResponseAsync(message, history, modelOverride);
            if (string.IsNullOrEmpty(full)) return full;

            var words = full.Split(' ');
            var acc = new StringBuilder();
            foreach (var w in words)
            {
                var token = (acc.Length == 0 ? "" : " ") + w;
                acc.Append(token);
                onToken?.Invoke(token);
                await Task.Delay(15);
            }
            return acc.ToString();
        }

        private async Task<string> SendWithRetryAsync(HttpRequestMessage request, int maxRetries)
        {
            var attempt = 0;
            while (true)
            {
                try
                {
                    using var resp = await _http.SendAsync(request.Clone());
                    if ((int)resp.StatusCode == 429 || (int)resp.StatusCode >= 500)
                    {
                        attempt++;
                        if (attempt > maxRetries) return null;
                        var delay = TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt));
                        await Task.Delay(delay);
                        continue;
                    }
                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadAsStringAsync();
                }
                catch
                {
                    attempt++;
                    if (attempt > maxRetries) return null;
                    var delay = TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt));
                    await Task.Delay(delay);
                }
            }
        }

        private static object BuildMessagesPayload(string model, string message, List<ConversationMessage> history, ClaudeConfig settings)
        {
            var messages = new List<object>();

            if (history != null)
            {
                foreach (var m in history)
                {
                    messages.Add(new { role = m.Role == "assistant" ? "assistant" : "user", content = m.Content });
                }
            }
            messages.Add(new { role = "user", content = message });

            return new
            {
                model,
                max_tokens = settings.MaxTokens,
                temperature = settings.Temperature,
                messages
            };
        }

        private static decimal EstimateCostUSD(string model, int inputTokens, int outputTokens)
        {
            // Rough pricing; adjust as needed. Costs per 1M tokens.
            decimal inPerM = 0, outPerM = 0;
            var m = model.ToLowerInvariant();
            if (m.Contains("sonnet")) { inPerM = 3.0m; outPerM = 15.0m; }
            else if (m.Contains("haiku")) { inPerM = 0.25m; outPerM = 1.25m; }
            else { inPerM = 3.0m; outPerM = 15.0m; }

            var cost = (inputTokens / 1_000_000m) * inPerM + (outputTokens / 1_000_000m) * outPerM;
            return cost;
        }

        private class ClaudeMessageResponse
        {
            [JsonPropertyName("content")] public List<ClaudeContent> Content { get; set; }
            [JsonPropertyName("usage")] public ClaudeUsage Usage { get; set; }

            [JsonIgnore]
            public string ContentText => Content != null && Content.Count > 0 ? Content[0]?.Text ?? string.Empty : string.Empty;
        }

        private class ClaudeContent
        {
            [JsonPropertyName("type")] public string Type { get; set; }
            [JsonPropertyName("text")] public string Text { get; set; }
        }

        private class ClaudeUsage
        {
            [JsonPropertyName("input_tokens")] public int InputTokens { get; set; }
            [JsonPropertyName("output_tokens")] public int OutputTokens { get; set; }
        }
    }

    internal static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var h in request.Headers)
                clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
            if (request.Content != null)
            {
                var ms = new System.IO.MemoryStream();
                request.Content.CopyToAsync(ms).GetAwaiter().GetResult();
                ms.Position = 0;
                var newContent = new StreamContent(ms);
                foreach (var h in request.Content.Headers)
                    newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
                clone.Content = newContent;
            }
            return clone;
        }
    }
}
