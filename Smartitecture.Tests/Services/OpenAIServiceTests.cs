using System.Threading.Tasks;
using Smartitecture.Services;
using Xunit;

namespace Smartitecture.Tests.Services
{
    public class OpenAIServiceTests
    {
        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var ex = Record.Exception(() => new OpenAIService("test-api-key"));
            Assert.Null(ex);
        }

        [Fact]
        public async Task GetResponseAsync_UsesKnowledgeBase_WhenPossible()
        {
            var service = new OpenAIService("test-api-key");

            var result = await service.GetResponseAsync("What is the capital of France?");

            Assert.Contains("Paris", result);
        }

        [Fact]
        public void GetModelName_Default_IsGpt4()
        {
            var service = new OpenAIService("test-api-key");
            Assert.Equal("gpt-4", service.GetModelName());
        }
    }
}

