using Smartitecture.Services;
using System.Threading.Tasks;

namespace Smartitecture.Tests.Services
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
        public Task<string> GetResponseAsync(string userInput)
        {
            return Task.FromResult(_predefinedResponse);
        }

        /// <summary>
        /// Mock implementation that returns Unknown command
        /// </summary>
        public Task<(string commandName, string[] parameters)> ParseCommandAsync(string userInput)
        {
            return Task.FromResult(("Unknown", new string[0]));
        }
    }
}
