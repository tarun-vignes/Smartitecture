using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.Shared.Interfaces
{
    public interface IHandlerRegistry
    {
        void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler);
        Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default);
    }
}
