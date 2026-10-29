using Smartitecture.Services;
using Xunit;

namespace Smartitecture.Tests;

public sealed class ConnectionAddressTests
{
    [Theory]
    [InlineData("example.com/", "https://example.com")]
    [InlineData(" localhost:8080 ", "http://localhost:8080")]
    [InlineData("127.0.0.1:8080", "http://127.0.0.1:8080")]
    [InlineData("[::1]:8080", "http://[::1]:8080")]
    [InlineData("", "")]
    public void ConnectionAddress_Normalize_ShouldChooseSecureDefaults(string input, string expected)
        => Assert.Equal(expected, ConnectionAddress.Normalize(input));

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://localhost:8080", true)]
    [InlineData("http://example.com", false)]
    [InlineData("https://user:secret@example.com", false)]
    [InlineData("https://example.com?key=secret", false)]
    [InlineData("https://example.com#fragment", false)]
    [InlineData("ftp://example.com", false)]
    public void ConnectionAddress_IsValid_ShouldRejectUnsafeConnections(string input, bool expected)
        => Assert.Equal(expected, ConnectionAddress.IsValid(input));
}
