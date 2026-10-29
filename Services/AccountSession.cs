using System.IO;
using System.Text.Json;

namespace Smartitecture.Services;

public sealed class AccountSession
{
    public string ServiceUrl { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class AccountSessionStore
{
    private readonly string _path;
    public AccountSessionStore(string? path = null) => _path = path ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smartitecture", "account.session");

    public AccountSession? Load(string serviceUrl)
    {
        try
        {
            var session = JsonSerializer.Deserialize<AccountSession>(WindowsCredentialProtection.Unprotect(File.ReadAllText(_path)));
            return session != null && session.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30)
                && session.ServiceUrl == serviceUrl.TrimEnd('/') ? session : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
            or FormatException or System.Security.Cryptography.CryptographicException) { return null; }
    }

    public void Save(AccountSession session)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, WindowsCredentialProtection.Protect(JsonSerializer.Serialize(session)));
            File.Move(temporary, _path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    public void Clear() { if (File.Exists(_path)) File.Delete(_path); }
}
