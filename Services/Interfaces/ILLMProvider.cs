using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Smartitecture.Services.Core;

namespace Smartitecture.Services.Interfaces
{
    public interface ILLMProvider
    {
        string Name { get; }
        bool IsConfigured { get; }
        Task<LLMResponse> GetResponseAsync(LLMRequest request, CancellationToken cancellationToken);
        IAsyncEnumerable<string> StreamResponseAsync(LLMRequest request, CancellationToken cancellationToken);
    }
}
