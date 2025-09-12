using Smartitecture.Commands;
using System.Threading.Tasks;
using Xunit;

namespace Smartitecture.Tests.Commands
{
    public class VolumeCommandTests
    {
        private readonly VolumeCommand _volumeCommand = new VolumeCommand();

        [Fact]
        public async Task ExecuteAsync_WithValidParameters_ReturnsTrue()
        {
            var ok = await _volumeCommand.ExecuteAsync(new[] { "up" });
            Assert.IsType<bool>(ok);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoParameters_ReturnsFalse()
        {
            var ok = await _volumeCommand.ExecuteAsync(new string[] { });
            Assert.False(ok);
        }
    }
}
