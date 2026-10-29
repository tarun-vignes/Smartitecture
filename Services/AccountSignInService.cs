using System.Net.Http;
using System.Text.Json;

namespace Smartitecture.Services;

public sealed record SignInChallenge(string Code, string VerificationUrl, string DeviceCode,
    string TokenEndpoint, string ClientId, DateTimeOffset ExpiresAt, int Interval);

public sealed class AccountSignInService
{
    private readonly HttpClient _http;
    public AccountSignInService(HttpClient? http = null) => _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

    public async Task<SignInChallenge> BeginAsync(string serviceUrl, CancellationToken token)
    {
        if (!ConnectionAddress.IsValid(serviceUrl) || string.IsNullOrWhiteSpace(serviceUrl))
            throw new InvalidOperationException("Online AI is not available in this installation yet.");
        using var config = JsonDocument.Parse(await _http.GetStringAsync(serviceUrl.TrimEnd('/') + "/auth/config", token));
        var root = config.RootElement;
        var authority = SecureUrl(root.GetProperty("authority").GetString());
        var clientId = root.GetProperty("clientId").GetString();
        if (string.IsNullOrWhiteSpace(clientId)) throw new InvalidOperationException("Sign-in is not available yet.");
        using var discovery = JsonDocument.Parse(await _http.GetStringAsync(authority.TrimEnd('/') + "/.well-known/openid-configuration", token));
        var deviceEndpoint = SecureUrl(discovery.RootElement.GetProperty("device_authorization_endpoint").GetString());
        var tokenEndpoint = SecureUrl(discovery.RootElement.GetProperty("token_endpoint").GetString());
        var fields = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["scope"] = root.GetProperty("scope").GetString() ?? "openid profile",
            ["audience"] = root.GetProperty("audience").GetString() ?? ""
        };
        using var response = await _http.PostAsync(deviceEndpoint, new FormUrlEncodedContent(fields), token);
        response.EnsureSuccessStatusCode();
        using var challenge = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
        var data = challenge.RootElement;
        return new SignInChallenge(data.GetProperty("user_code").GetString()!,
            SecureUrl(data.GetProperty("verification_uri").GetString()), data.GetProperty("device_code").GetString()!,
            tokenEndpoint, clientId, DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(data.GetProperty("expires_in").GetInt32(), 1, 1800)),
            data.TryGetProperty("interval", out var interval) ? Math.Clamp(interval.GetInt32(), 5, 60) : 5);
    }

    public async Task<AccountSession> CompleteAsync(string serviceUrl, SignInChallenge challenge, CancellationToken token)
    {
        var interval = challenge.Interval;
        using var expiry = CancellationTokenSource.CreateLinkedTokenSource(token);
        var remaining = challenge.ExpiresAt - DateTimeOffset.UtcNow;
        if (remaining <= TimeSpan.Zero) throw new InvalidOperationException("Sign-in expired. Please try again.");
        expiry.CancelAfter(remaining);
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(interval), expiry.Token);
            using var response = await _http.PostAsync(SecureUrl(challenge.TokenEndpoint), new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                ["client_id"] = challenge.ClientId, ["device_code"] = challenge.DeviceCode
            }), expiry.Token);
            using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync(expiry.Token));
            var data = result.RootElement;
            if (response.IsSuccessStatusCode)
            {
                if (!data.GetProperty("token_type").GetString()!.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Unsupported sign-in response.");
                var session = new AccountSession { ServiceUrl = serviceUrl.TrimEnd('/'),
                    AccessToken = data.GetProperty("access_token").GetString()!,
                    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(data.GetProperty("expires_in").GetInt32()) };
                using var verification = new HttpRequestMessage(HttpMethod.Get, session.ServiceUrl + "/v1/account");
                verification.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", session.AccessToken);
                using var verified = await _http.SendAsync(verification, expiry.Token);
                verified.EnsureSuccessStatusCode();
                return session;
            }
            var error = data.GetProperty("error").GetString();
            if (error == "authorization_pending") continue;
            if (error == "slow_down") { interval = Math.Min(interval + 5, 300); continue; }
            throw new InvalidOperationException(error == "access_denied" ? "Sign-in was declined." : "Sign-in expired or could not finish. Please try again.");
        }
    }

    private static string SecureUrl(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo) ? uri.AbsoluteUri
        : throw new InvalidOperationException("Sign-in requires a secure service.");
}
