using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.Core.Services
{
    public interface IBaseService
    {
        ILogger Logger { get; }
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task ShutdownAsync(CancellationToken cancellationToken = default);
    }

    public interface IService<T> : IBaseService
    {
        Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
