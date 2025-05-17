using System.Threading.Tasks;

namespace AIPal.Application.Commands
{
    /// <summary>
    /// Interface for command pattern implementation.
    /// </summary>
    /// <typeparam name="TParameter">The type of parameter for the command.</typeparam>
    /// <typeparam name="TResult">The type of result returned by the command.</typeparam>
    public interface ICommand<TParameter, TResult>
    {
        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        /// <returns>The result of the command execution.</returns>
        Task<TResult> ExecuteAsync(TParameter parameter);
    }
    
    /// <summary>
    /// Interface for command pattern implementation with no parameter.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the command.</typeparam>
    public interface ICommand<TResult>
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>The result of the command execution.</returns>
        Task<TResult> ExecuteAsync();
    }
    
    /// <summary>
    /// Interface for command pattern implementation with no result.
    /// </summary>
    /// <typeparam name="TParameter">The type of parameter for the command.</typeparam>
    public interface IVoidCommand<TParameter>
    {
        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(TParameter parameter);
    }
    
    /// <summary>
    /// Interface for command pattern implementation with no parameter and no result.
    /// </summary>
    public interface IVoidCommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync();
    }
}
