using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static readonly (string Alias, string Target)[] KnownAppAliases =
        {
            ("calculator", "calc"),
            ("calc", "calc"),
            ("camera", "microsoft.windows.camera:"),
            ("windows camera", "microsoft.windows.camera:"),
            ("settings", "ms-settings:"),
            ("windows settings", "ms-settings:"),
            ("microsoft store", "ms-windows-store:"),
            ("store", "ms-windows-store:"),
            ("photos", "ms-photos:"),
            ("mail", "outlookmail:"),
            ("calendar", "outlookcal:"),
            ("notepad", "notepad"),
            ("paint", "mspaint"),
            ("task manager", "taskmgr"),
            ("file explorer", "explorer"),
            ("explorer", "explorer")
        };

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

                target = NormalizeTarget(target);
                var knownTarget = ResolveKnownAlias(target);
                if (!string.IsNullOrWhiteSpace(knownTarget) && TryStart(knownTarget))
                {
                    return Task.FromResult(true);
                }

                if (TryStart(target))
                {
                    return Task.FromResult(true);
                }

                if (TryStartFromStartMenuShortcut(target))
                {
                    return Task.FromResult(true);
                }

                if (TryStartFromAppsFolder(target))
                {
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private static string NormalizeTarget(string target)
        {
            target = target.Trim().Trim('"', '\'').TrimEnd('.', ',', '!', '?', ';', ':');

            foreach (var prefix in new[] { "open ", "launch ", "start ", "run " })
            {
                if (target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    target = target.Substring(prefix.Length).Trim();
                    break;
                }
            }

            if (target.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
            {
                target = target.Substring(4).Trim();
            }

            foreach (var suffix in new[] { " app", " application", " program" })
            {
                if (target.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    target = target.Substring(0, target.Length - suffix.Length).Trim();
                    break;
                }
            }

            return target;
        }

        private static string? ResolveKnownAlias(string target)
        {
            return KnownAppAliases
                .FirstOrDefault(alias => string.Equals(alias.Alias, target, StringComparison.OrdinalIgnoreCase))
                .Target;
        }

        private static bool TryStart(string target)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryStartFromStartMenuShortcut(string target)
        {
            foreach (var root in GetStartMenuRoots())
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                {
                    continue;
                }

                var shortcut = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                    .Where(path => path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ||
                                   path.EndsWith(".appref-ms", StringComparison.OrdinalIgnoreCase))
                    .Select(path => new FileInfo(path))
                    .OrderBy(file => GetMatchScore(Path.GetFileNameWithoutExtension(file.Name), target))
                    .FirstOrDefault(file => GetMatchScore(Path.GetFileNameWithoutExtension(file.Name), target) < 100);

                if (shortcut != null && TryStart(shortcut.FullName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryStartFromAppsFolder(string target)
        {
            try
            {
                var escapedTarget = target.Replace("'", "''");
                var command =
                    "$target = '" + escapedTarget + "'; " +
                    "$app = Get-StartApps | Where-Object { $_.Name -ieq $target } | Select-Object -First 1; " +
                    "if (-not $app) { $app = Get-StartApps | Where-Object { $_.Name -like \"*$target*\" } | Select-Object -First 1 }; " +
                    "if ($app) { $app.AppID }";

                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });

                if (process == null)
                {
                    return false;
                }

                var appId = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(3000);

                if (string.IsNullOrWhiteSpace(appId))
                {
                    return false;
                }

                return TryStartWithExplorer($"shell:AppsFolder\\{appId}");
            }
            catch
            {
                return false;
            }
        }

        private static bool TryStartWithExplorer(string shellPath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = shellPath,
                    UseShellExecute = true
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string[] GetStartMenuRoots()
        {
            return new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
            };
        }

        private static int GetMatchScore(string candidate, string target)
        {
            if (string.Equals(candidate, target, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (candidate.StartsWith(target, StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (candidate.Contains(target, StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return 100;
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
