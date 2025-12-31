using Smartitecture.Commands;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Smartitecture.Tests.Commands
{
    public class VolumeCommandTests
    {
        [Fact]
        public void Metadata_IsCorrect()
        {
            var cmd = new VolumeCommand();
            Assert.Equal("Volume", cmd.CommandName);
            Assert.False(cmd.RequiresElevation);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoParameters_ReturnsFalse()
        {
            var cmd = new VolumeCommand();
            var result = await cmd.ExecuteAsync(Array.Empty<string>());
            Assert.False(result);
        }

        [Fact]
        public async Task ExecuteAsync_WithNullParameters_ReturnsFalse()
        {
            var cmd = new VolumeCommand();
            var result = await cmd.ExecuteAsync(null!);
            Assert.False(result);
        }
    }
}
