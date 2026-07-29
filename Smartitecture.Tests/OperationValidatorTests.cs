using Smartitecture.Services.Safety;
using Xunit;

namespace Smartitecture.Tests;

public sealed class OperationValidatorTests
{
    [Theory]
    [InlineData("shutdown")]
    [InlineData("kill_process")]
    [InlineData("defender_scan")]
    [InlineData("registry_write")]
    [InlineData("firewall_rule")]
    public void RequiresConfirmation_ReturnsTrueForSensitiveTools(string toolName)
    {
        var validator = new OperationValidator();

        Assert.True(validator.RequiresConfirmation(toolName));
    }

    [Theory]
    [InlineData("system_info")]
    [InlineData("performance_snapshot")]
    [InlineData("network_adapters")]
    [InlineData("battery_status")]
    [InlineData("launch")]
    public void RequiresConfirmation_ReturnsFalseForReadOnlyOrLowRiskTools(string toolName)
    {
        var validator = new OperationValidator();

        Assert.False(validator.RequiresConfirmation(toolName));
    }
}
