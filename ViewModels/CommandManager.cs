using System;

namespace AIPal.ViewModels
{
    /// <summary>
    /// Provides a way to invalidate command execution status.
    /// </summary>
    public static class CommandManager
    {
        /// <summary>
        /// Occurs when changes occur that affect whether or not commands should execute.
        /// </summary>
        public static event EventHandler RequerySuggested;

        /// <summary>
        /// Raises the RequerySuggested event to indicate that command execution status should be reevaluated.
        /// </summary>
        public static void InvalidateRequerySuggested()
        {
            RequerySuggested?.Invoke(null, EventArgs.Empty);
        }
    }
}
