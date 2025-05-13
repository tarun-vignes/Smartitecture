using AIPal.Services;
using System.Threading.Tasks;

namespace AIPal.Tests.Services
{
    /// <summary>
    /// Mock implementation of ILLMService for testing purposes
    /// </summary>
    public class MockLLMService : ILLMService
    {
        private readonly string _predefinedResponse;

        public MockLLMService(string predefinedResponse = "Mock response")
        {
            _predefinedResponse = predefinedResponse;
        }

        /// <summary>
        /// Returns a predefined response instead of calling the actual LLM service
        /// </summary>
        public Task<string> GenerateResponseAsync(string userInput)
        {
            return Task.FromResult(_predefinedResponse);
        }

        /// <summary>
        /// Mock implementation that always returns true
        /// </summary>
        public Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Mock implementation that always returns success
        /// </summary>
        public Task<bool> ValidateConfigurationAsync()
        {
            return Task.FromResult(true);
        }
    }
}
