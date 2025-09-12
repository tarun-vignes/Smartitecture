using Smartitecture.Services;
using System.Threading.Tasks;
using Xunit;

namespace Smartitecture.Tests.Services
{
    public class CommandMapperTests
    {
        [Fact]
        public async Task ExecuteCommandAsync_WithUnknownCommand_ReturnsFalse()
        {
            var mapper = new CommandMapper();
            var ok = await mapper.ExecuteCommandAsync("unknown", new[] { "param" });
            Assert.False(ok);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithNoParameters_ReturnsFalse()
        {
            var mapper = new CommandMapper();
            var ok = await mapper.ExecuteCommandAsync("Volume", System.Array.Empty<string>());
            Assert.False(ok);
        }
    }
}
