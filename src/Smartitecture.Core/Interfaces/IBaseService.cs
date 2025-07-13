using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.Core.Interfaces
{
    public interface IBaseService
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task ShutdownAsync(CancellationToken cancellationToken = default);
        Task<TResponse> ExecuteAsync<TResponse>(Func<Task<TResponse>> action, CancellationToken cancellationToken = default);
    }
}
