using Smartitecture.Services;
using Xunit;

namespace Smartitecture.Tests;

public sealed class ChatRoutingTests
{
    [Theory]
    [InlineData("How much battery percentage do I have?", "battery_status")]
    [InlineData("Did the scan find anything?", "defender_scan_status")]
    [InlineData("Any threats found?", "defender_scan_status")]
    [InlineData("Defender status", "defender_status")]
    [InlineData("Show my IP", "network_adapters")]
    [InlineData("Why is my PC slow?", "performance_snapshot")]
    [InlineData("What is using my RAM?", "list_processes")]
    [InlineData("What should I close to free RAM?", "list_processes")]
    public async Task ParseCommandAsync_RoutesLocalDiagnosticPrompts(string prompt, string expectedCommand)
    {
        var service = new MultiModelAIService();

        var (commandName, _) = await service.ParseCommandAsync(prompt);

        Assert.Equal(expectedCommand, commandName);
    }

    [Fact]
    public async Task ParseCommandAsync_RoutesDefenderScanWithFullFlag()
    {
        var service = new MultiModelAIService();

        var (commandName, parameters) = await service.ParseCommandAsync("Run a full Defender scan");

        Assert.Equal("defender_scan", commandName);
        Assert.NotNull(parameters);
        Assert.True((bool)parameters["full"]);
    }

    [Theory]
    [InlineData("Can you open camera?", "camera")]
    [InlineData("Open calculator", "calculator")]
    [InlineData("Launch notepad", "notepad")]
    public async Task ParseCommandAsync_RoutesAppLaunchTargets(string prompt, string expectedTarget)
    {
        var service = new MultiModelAIService();

        var (commandName, parameters) = await service.ParseCommandAsync(prompt);

        Assert.Equal("launch", commandName);
        Assert.NotNull(parameters);
        Assert.Equal(expectedTarget, parameters["target"]);
    }
}
