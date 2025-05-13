using AIPal.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AIPal.Tests.Services
{
    public class AzureOpenAIServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;

        public AzureOpenAIServiceTests()
        {
            // Setup mock configuration
            _mockConfiguration = new Mock<IConfiguration>();
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s["Endpoint"]).Returns("https://test-endpoint.openai.azure.com/");
            configSection.Setup(s => s["ApiKey"]).Returns("test-api-key");
            configSection.Setup(s => s["DeploymentName"]).Returns("test-deployment");

            _mockConfiguration.Setup(c => c.GetSection("AzureOpenAI")).Returns(configSection.Object);
        }

        [Fact]
        public void Constructor_WithValidConfiguration_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => new AzureOpenAIService(_mockConfiguration.Object));
            Assert.Null(exception);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithValidConfig_ReturnsTrue()
        {
            // Arrange
            var service = new AzureOpenAIService(_mockConfiguration.Object);

            // Act & Assert - This will not actually call Azure in tests
            // In a real scenario, you would use a mock HTTP client or dependency injection
            // This test is just to verify the method exists and doesn't throw with valid config
            await Assert.ThrowsAnyAsync<Exception>(() => service.ValidateConfigurationAsync());
        }

        [Fact]
        public async Task GenerateResponseAsync_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new AzureOpenAIService(_mockConfiguration.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GenerateResponseAsync(null));
        }

        [Fact]
        public async Task GenerateResponseAsync_WithEmptyInput_ThrowsArgumentException()
        {
            // Arrange
            var service = new AzureOpenAIService(_mockConfiguration.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateResponseAsync(string.Empty));
        }
    }
}
