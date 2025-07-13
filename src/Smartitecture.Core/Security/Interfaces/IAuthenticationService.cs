using System.Threading.Tasks;
using System.Threading;

namespace Smartitecture.Core.Security.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
        Task<bool> RegisterAsync(string username, string password, CancellationToken cancellationToken = default);
        Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        bool IsAuthenticated { get; }
        string CurrentUsername { get; }
    }
}
