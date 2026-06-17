using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Providers
{
    public sealed class LocalFallbackProvider : ILLMProvider
    {
        private readonly NaturalConversationService _conversation = new NaturalConversationService();

        public string Name => "Local Fallback";
        public bool IsConfigured => true;

        public Task<LLMResponse> GetResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            return GetResponseCoreAsync(request);
        }

        private async Task<LLMResponse> GetResponseCoreAsync(LLMRequest request)
        {
            var history = request.History
                .Select(m => new ConversationMessage
                {
                    Role = m.Role,
                    Content = m.Content
                })
                .ToList();

            var response = ResponseTextCleaner.ForChatDisplay(
                await _conversation.GetResponseAsync(request.UserMessage, history));

            return new LLMResponse { Content = response };
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(LLMRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var response = (await GetResponseAsync(request, cancellationToken)).Content;
            var words = response.Split(' ');
            var buffer = string.Empty;
            foreach (var word in words)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                buffer = (buffer.Length == 0) ? word : buffer + " " + word;
                yield return (buffer.Length == word.Length) ? word : " " + word;
                await Task.Delay(20, cancellationToken);
            }
        }
    }
}
