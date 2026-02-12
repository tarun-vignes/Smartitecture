using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smartitecture.Core.Commands
{
    public interface IAppCommand
    {
        string CommandName { get; }
        string Description { get; }
        bool RequiresElevation { get; }
        Task<bool> ExecuteAsync(string[] parameters);
    }

    public sealed class LaunchAppCommand : IAppCommand
    {
        public string CommandName => "launch";
        public string Description => "Launches a Windows application";
        public bool RequiresElevation => false;

        public Task<bool> ExecuteAsync(string[] parameters)
        {
            try
            {
                if (parameters == null || parameters.Length == 0) return Task.FromResult(false);
                var target = parameters[0]?.Trim();
                if (string.IsNullOrWhiteSpace(target)) return Task.FromResult(false);

                // Common aliases
                if (string.Equals(target, "calculator", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(target, "calc", StringComparison.OrdinalIgnoreCase))
                {
                    Process.Start(new ProcessStartInfo { FileName = "calc", UseShellExecute = true });
                    return Task.FromResult(true);
                }

                // Try to shell-execute whatever was provided (path, URI, AppsFolder moniker, etc.)
                Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }

    public sealed class ShutdownCommand : IAppCommand
    {
        public string CommandName => "shutdown";
        public string Description => "Requests a system shutdown";
        public bool RequiresElevation => true;

        public Task<bool> ExecuteAsync(string[] parameters)
        {
            // Safety: do not actually shut down; return false to prompt guidance from AI layer
            return Task.FromResult(false);
        }
    }

    // Optional convenience commands referenced by older UI
    public sealed class CalculatorCommand : IAppCommand
    {
        public string CommandName => "calculator";
        public string Description => "Opens Windows Calculator";
        public bool RequiresElevation => false;
        public Task<bool> ExecuteAsync(string[] parameters)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "calc", UseShellExecute = true }); return Task.FromResult(true); } catch { return Task.FromResult(false); }
        }
    }

    public sealed class ExplorerCommand : IAppCommand
    {
        public string CommandName => "explorer";
        public string Description => "Opens File Explorer";
        public bool RequiresElevation => false;
        public Task<bool> ExecuteAsync(string[] parameters)
        {
            var target = (parameters != null && parameters.Length > 0 && !string.IsNullOrWhiteSpace(parameters[0])) ? parameters[0] : null;
            try { Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = target ?? string.Empty, UseShellExecute = true }); return Task.FromResult(true); } catch { return Task.FromResult(false); }
        }
    }

    public sealed class TaskManagerCommand : IAppCommand
    {
        public string CommandName => "taskmgr";
        public string Description => "Opens Task Manager";
        public bool RequiresElevation => false;
        public Task<bool> ExecuteAsync(string[] parameters)
        {
            try { Process.Start(new ProcessStartInfo { FileName = "taskmgr.exe", UseShellExecute = true }); return Task.FromResult(true); } catch { return Task.FromResult(false); }
        }
    }
}
