using System.Security.Claims;
using System.Security.Principal;

namespace Smartitecture.Core.Security.Services
{
    public interface IAuthorizationService
    {
        bool HasPermission(ClaimsPrincipal user, string permission);
        bool HasRole(ClaimsPrincipal user, string role);
        Task<bool> AuthorizeAsync(ClaimsPrincipal user, string permission);
        Task<bool> AuthorizeAsync(ClaimsPrincipal user, string role);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            ICacheService cacheService,
            ILogger<AuthorizationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public bool HasPermission(ClaimsPrincipal user, string permission)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;

            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (!userRoles.Any()) return false;

            // Check cache first
            var cachedPermissions = _cacheService
                .GetAsync<string[]>($"permissions_{userId}")
                .Result;

            if (cachedPermissions != null && cachedPermissions.Contains(permission))
                return true;

            // TODO: Implement actual permission lookup
            return false;
        }

        public bool HasRole(ClaimsPrincipal user, string role)
        {
            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            return userRoles.Contains(role);
        }

        public async Task<bool> AuthorizeAsync(ClaimsPrincipal user, string permission)
        {
            if (!user.Identity?.IsAuthenticated ?? false)
                throw new UnauthorizedException("User is not authenticated");

            if (HasPermission(user, permission))
                return true;

            throw new ForbiddenException($"User does not have permission: {permission}");
        }

        public async Task<bool> AuthorizeAsync(ClaimsPrincipal user, string role)
        {
            if (!user.Identity?.IsAuthenticated ?? false)
                throw new UnauthorizedException("User is not authenticated");

            if (HasRole(user, role))
                return true;

            throw new ForbiddenException($"User does not have role: {role}");
        }
    }
}
