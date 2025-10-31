using System.Threading.Tasks;
using Xunit;
using Smartitecture.Services;

namespace Smartitecture.Tests.Services
{
    public class ClaudeServiceTests
    {
        [Fact]
        public async Task ReturnsFallback_WhenNotConfigured()
        {
            var service = new ClaudeService();
            if (service.IsConfigured())
            {
                // Skip if configured in the environment to avoid real calls in CI
                return;
            }

            var result = await service.GetResponseAsync("What is 2 + 2?");
            Assert.False(string.IsNullOrWhiteSpace(result));
        }
    }
}

