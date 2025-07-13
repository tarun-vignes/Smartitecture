using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.Shared.Interfaces
{
    public interface IRequestHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
