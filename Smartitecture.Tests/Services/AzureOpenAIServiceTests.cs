using Smartitecture.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;

namespace Smartitecture.Tests.Services
{
    public class AzureOpenAIServiceTests
    {
        private static IOptions<AzureOpenAIConfiguration> CreateOptions()
        {
            // Fake values are fine; service methods catch exceptions and return error strings
            return Options.Create(new AzureOpenAIConfiguration
            {
                Endpoint = "https://example.com/",
                ApiKey = "test-api-key",
                DeploymentName = "test-deployment"
            });
        }

        [Fact]
        public void Constructor_WithValidConfiguration_DoesNotThrow()
        {
            var opts = CreateOptions();
            var ex = Record.Exception(() => new AzureOpenAIService(opts));
            Assert.Null(ex);
        }

        [Fact]
        public async Task GetResponseAsync_ReturnsStringEvenOnError()
        {
            var service = new AzureOpenAIService(CreateOptions());
            var result = await service.GetResponseAsync("hello");
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public async Task ParseCommandAsync_WithGarbage_ReturnsUnknown()
        {
            var service = new AzureOpenAIService(CreateOptions());
            var (command, parameters) = await service.ParseCommandAsync("gibberish");
            Assert.Equal("Unknown", command);
            Assert.NotNull(parameters);
        }
    }
}
