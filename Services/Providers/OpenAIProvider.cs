using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Providers
{
    public sealed class OpenAIProvider : ILLMProvider
    {
        private readonly ConfigurationService _configService = new ConfigurationService();
        private readonly ProviderSettings _settings = ProviderSettings.Load();
        private readonly Dictionary<string, OpenAIService> _clients = new(StringComparer.OrdinalIgnoreCase);

        public string Name => "OpenAI";
        public bool IsConfigured => _configService.IsOpenAIConfigured() || !string.IsNullOrWhiteSpace(_settings.OpenAI.ApiKey);

        public async Task<LLMResponse> GetResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
            {
                return new LLMResponse { Content = "This model is not available right now. Using the local assistant instead." };
            }

            var client = GetClient(request.Model ?? "gpt-4");
            var history = ToConversationHistory(request.History);
            var response = await client.GetResponseAsync(request.UserMessage, history, request.SystemPrompt);
            return new LLMResponse { Content = response };
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
            {
                yield return "This model is not available right now. Using the local assistant instead.";
                yield break;
            }

            var client = GetClient(request.Model ?? "gpt-4");
            var history = ToConversationHistory(request.History);
            var channel = Channel.CreateUnbounded<string>();

            _ = Task.Run(async () =>
            {
                try
                {
                    await client.GetStreamingResponseAsync(request.UserMessage, token =>
                    {
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            channel.Writer.TryWrite(token);
                        }
                    }, history, request.SystemPrompt);
                }
                finally
                {
                    channel.Writer.TryComplete();
                }
            }, cancellationToken);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var token))
                {
                    yield return token;
                }
            }
        }

        private OpenAIService GetClient(string model)
        {
            if (_clients.TryGetValue(model, out var client))
            {
                return client;
            }

            var apiKey = _configService.GetOpenAIApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = _settings.OpenAI.ApiKey;
            }
            client = new OpenAIService(apiKey, model);
            _clients[model] = client;
            return client;
        }

        private static List<ConversationMessage> ToConversationHistory(IReadOnlyList<LLMMessage> history)
        {
            return history
                .Select(m => new ConversationMessage { Role = m.Role, Content = m.Content })
                .ToList();
        }
    }
}
