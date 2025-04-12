using System.Threading.Tasks;

namespace AIPal.Services
{
    public interface ILLMService
    {
        Task<string> GetResponseAsync(string userInput);
        Task<(string commandName, string[] parameters)> ParseCommandAsync(string userInput);
    }
}
