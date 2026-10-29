using Smartitecture.Services;
using Xunit;

namespace Smartitecture.Tests;

public sealed class AccountSessionTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SmartitectureTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void AccountSessionStore_Save_ShouldEncryptAndBindSessionToService()
    {
        var path = Path.Combine(_directory, "account.session");
        var store = new AccountSessionStore(path);
        store.Save(new AccountSession { ServiceUrl = "https://service.example", AccessToken = "test-access-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10) });

        Assert.DoesNotContain("test-access-token", File.ReadAllText(path));
        Assert.Equal("test-access-token", store.Load("https://service.example/")?.AccessToken);
        Assert.Null(store.Load("https://other.example"));
        store.Clear();
        Assert.Null(store.Load("https://service.example"));
    }

    [Fact]
    public void AccountSessionStore_Load_ShouldRejectExpiredSessions()
    {
        var store = new AccountSessionStore(Path.Combine(_directory, "account.session"));
        store.Save(new AccountSession { ServiceUrl = "https://service.example", AccessToken = "expired",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1) });
        Assert.Null(store.Load("https://service.example"));
    }

    [Fact]
    public async Task AccountSignInService_CompleteAsync_ShouldRejectExpiredChallenge()
    {
        var service = new AccountSignInService();
        var challenge = new SignInChallenge("code", "https://login.example", "device", "https://login.example/token",
            "client", DateTimeOffset.UtcNow.AddMinutes(-1), 5);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CompleteAsync("https://service.example", challenge, default));
    }

    [Fact]
    public async Task AccountSignInService_BeginAsync_ShouldRejectInsecureRemoteService()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => new AccountSignInService().BeginAsync("http://remote.example", default));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}
