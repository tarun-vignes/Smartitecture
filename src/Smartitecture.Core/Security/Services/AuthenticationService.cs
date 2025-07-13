using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Smartitecture.Core.Security.Services
{
    public interface IAuthenticationService
    {
        Task<User> LoginAsync(string username, string password);
        Task<User> RegisterAsync(string username, string email, string password);
        string GenerateJwtToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IConfiguration configuration,
            ICacheService cacheService,
            ILogger<AuthenticationService> logger)
        {
            _configuration = configuration;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            // TODO: Implement actual user lookup
            var user = await _cacheService.GetAsync<User>($"user_{username}")
                ?? throw new UnauthorizedException("Invalid credentials");

            if (!VerifyPasswordHash(password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid credentials");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _cacheService.SetAsync($"user_{username}", user);

            return user;
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Invalid input", new[] { "Username, email, and password are required" });

            // Check if user exists
            if (await _cacheService.ExistsAsync($"user_{username}") || await _cacheService.ExistsAsync($"email_{email}"))
                throw new ValidationException("User already exists", new[] { "Username or email already exists" });

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Email = email,
                PasswordHash = CreatePasswordHash(password),
                CreatedAt = DateTime.UtcNow
            };

            // Add default role
            user.Roles.Add(new Role { Name = "User" });

            // Cache user
            await _cacheService.SetAsync($"user_{username}", user);
            await _cacheService.SetAsync($"email_{email}", user);

            return user;
        }

        public string GenerateJwtToken(User user)
        {
            var securitySettings = _configuration.GetSection("Security").Get<SecuritySettings>();
            var secret = Encoding.UTF8.GetBytes(securitySettings.JwtSecret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, string.Join(",", user.Roles.Select(r => r.Name)))
            };

            var token = new JwtSecurityToken(
                issuer: securitySettings.Issuer,
                audience: securitySettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(securitySettings.TokenExpirationMinutes),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secret),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var securitySettings = _configuration.GetSection("Security").Get<SecuritySettings>();
            var secret = Encoding.UTF8.GetBytes(securitySettings.JwtSecret);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = securitySettings.Issuer,
                    ValidAudience = securitySettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(secret)
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string CreatePasswordHash(string password)
        {
            var salt = new byte[128 / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            ));
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[128 / 8];
            Array.Copy(hashBytes, 0, salt, 0, salt.Length);

            var computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            ));

            return computedHash == storedHash;
        }
    }
}
