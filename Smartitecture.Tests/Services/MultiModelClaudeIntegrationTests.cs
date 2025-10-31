using System.Threading.Tasks;
using Xunit;
using Smartitecture.Services;

namespace Smartitecture.Tests.Services
{
    public class MultiModelClaudeIntegrationTests
    {
        [Fact]
        public async Task SwitchModel_Supports_ClaudeModels()
        {
            var svc = new MultiModelAIService();

            Assert.Contains("Anthropic Claude 3.5 Sonnet", svc.AvailableModels);
            Assert.Contains("Anthropic Claude 3 Haiku", svc.AvailableModels);

            var ok1 = await svc.SwitchModelAsync("Anthropic Claude 3.5 Sonnet");
            Assert.True(ok1);

            var ok2 = await svc.SwitchModelAsync("Anthropic Claude 3 Haiku");
            Assert.True(ok2);
        }
    }
}

