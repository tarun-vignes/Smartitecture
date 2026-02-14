using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Providers
{
    public sealed class LocalFallbackProvider : ILLMProvider
    {
        private readonly KnowledgeBaseService _knowledgeBase = new KnowledgeBaseService();

        public string Name => "Local Fallback";
        public bool IsConfigured => true;

        public Task<LLMResponse> GetResponseAsync(LLMRequest request, CancellationToken cancellationToken)
        {
            var kb = _knowledgeBase.GetAnswer(request.UserMessage);
            var response = !string.IsNullOrWhiteSpace(kb)
                ? kb
                : "Smartitecture local assistant is active.";

            return Task.FromResult(new LLMResponse { Content = response });
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(LLMRequest request, CancellationToken cancellationToken)
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
