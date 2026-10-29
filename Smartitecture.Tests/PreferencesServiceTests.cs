using System.Text.Json;
using Smartitecture.Services;
using Xunit;

namespace Smartitecture.Tests;

public sealed class PreferencesServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SmartitectureTests", Guid.NewGuid().ToString("N"));
    private string PreferencesPath => Path.Combine(_directory, "prefs.json");

    [Fact]
    public void PreferencesService_Save_ShouldProtectCredentialAndPreserveCaller()
    {
        var service = new PreferencesService(PreferencesPath);
        var preferences = new UserPreferences { BackendApiKey = "test-secret-value", Theme = "Light" };

        service.Save(preferences);

        Assert.DoesNotContain("test-secret-value", File.ReadAllText(PreferencesPath));
        Assert.Equal("test-secret-value", preferences.BackendApiKey);
        Assert.Equal("test-secret-value", service.Load().BackendApiKey);
        Assert.Equal("Light", service.Load().Theme);
    }

    [Fact]
    public void PreferencesService_Load_ShouldMigratePlaintextWithoutPlaintextBackup()
    {
        Directory.CreateDirectory(_directory);
        var json = JsonSerializer.Serialize(new UserPreferences { BackendApiKey = "legacy-secret" });
        File.WriteAllText(PreferencesPath, json);
        File.WriteAllText(PreferencesPath + ".bak", json);

        var preferences = new PreferencesService(PreferencesPath).Load();

        Assert.Equal("legacy-secret", preferences.BackendApiKey);
        Assert.DoesNotContain("legacy-secret", File.ReadAllText(PreferencesPath));
        Assert.False(File.Exists(PreferencesPath + ".bak"));
    }

    [Fact]
    public void PreferencesService_Load_ShouldRecoverBackupAfterCorruption()
    {
        var service = new PreferencesService(PreferencesPath);
        service.Save(new UserPreferences { Theme = "Light", BackendApiKey = "backup-secret" });
        service.Save(new UserPreferences { Theme = "Dark" });
        File.WriteAllText(PreferencesPath, "{broken");

        var preferences = service.Load();

        Assert.Equal("Light", preferences.Theme);
        Assert.Equal("backup-secret", preferences.BackendApiKey);
        Assert.NotNull(service.LoadWarning);
        service.Save(preferences);
        Assert.Equal("Light", service.Load().Theme);
        Assert.Null(service.LoadWarning);
    }

    [Fact]
    public void PreferencesService_Load_ShouldKeepOtherSettingsWhenCredentialIsDamaged()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(PreferencesPath, JsonSerializer.Serialize(new UserPreferences
        {
            Theme = "Light", BackendApiKey = "dpapi:invalid!"
        }));
        var service = new PreferencesService(PreferencesPath);

        var preferences = service.Load();

        Assert.Equal("Light", preferences.Theme);
        Assert.Empty(preferences.BackendApiKey);
        Assert.NotNull(service.LoadWarning);
    }

    [Fact]
    public void PreferencesService_Save_ShouldReportFailureAndLeaveExistingSettingsIntact()
    {
        var service = new PreferencesService(PreferencesPath);
        service.Save(new UserPreferences { Theme = "Light" });
        using (File.Open(PreferencesPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Assert.ThrowsAny<IOException>(() => service.Save(new UserPreferences { Theme = "Dark" }));

        Assert.Equal("Light", service.Load().Theme);
        Assert.Empty(Directory.GetFiles(_directory, "*.tmp"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}
