using System.Threading.Tasks;

namespace AIPal.Commands
{
    public interface ISystemCommand
    {
        string CommandName { get; }
        string Description { get; }
        bool RequiresElevation { get; }
        Task<bool> ExecuteAsync(string[] parameters);
    }
}
