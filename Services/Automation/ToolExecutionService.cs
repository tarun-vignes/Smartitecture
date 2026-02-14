using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Smartitecture.Core.Commands;
using Smartitecture.Services.Connectors;
using Smartitecture.Services.Safety;

namespace Smartitecture.Services.Automation
{
    public sealed class ToolExecutionResult
    {
        public bool Success { get; set; }
        public bool RequiresConfirmation { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class ToolExecutionService
    {
        private readonly OperationValidator _validator = new OperationValidator();
        private readonly UserConfirmationService _confirmation = new UserConfirmationService();
        private readonly AuditLogger _audit = new AuditLogger();
        private readonly WindowsDefenderConnector _defender = new WindowsDefenderConnector();

        private readonly Dictionary<string, IAppCommand> _commands = new(StringComparer.OrdinalIgnoreCase)
        {
            ["launch"] = new LaunchAppCommand(),
            ["explorer"] = new ExplorerCommand(),
            ["calculator"] = new CalculatorCommand(),
            ["taskmgr"] = new TaskManagerCommand(),
            ["shutdown"] = new ShutdownCommand()
        };

        public async Task<ToolExecutionResult> ExecuteToolAsync(string toolName, Dictionary<string, object?>? args = null, bool confirmed = false)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return new ToolExecutionResult { Success = false, Message = "No tool specified." };
            }

            if (toolName.Equals("launch", StringComparison.OrdinalIgnoreCase))
            {
                var target = ExtractTarget(args);
                if (string.IsNullOrWhiteSpace(target))
                {
                    return new ToolExecutionResult
                    {
                        Success = false,
                        Message = "Tell me which app to open (for example: \"Open Calculator\" or \"Open Notepad\")."
                    };
                }

                args ??= new Dictionary<string, object?>();
                args["target"] = target;
            }

            if (_validator.RequiresConfirmation(toolName) && !confirmed && !_confirmation.IsConfirmed(toolName))
            {
                return new ToolExecutionResult
                {
                    Success = false,
                    RequiresConfirmation = true,
                    Message = BuildConfirmationMessage(toolName, args)
                };
            }

            _audit.Log($"Executing tool '{toolName}'.");

            switch (toolName.ToLowerInvariant())
            {
                case "defender_scan":
                    return await RunDefenderScanAsync(args);
            }

            if (_commands.TryGetValue(toolName, out var command))
            {
                var parameters = ExtractParameters(toolName, args);
                var success = await command.ExecuteAsync(parameters);
                return new ToolExecutionResult
                {
                    Success = success,
                    Message = success
                        ? $"Tool '{toolName}' executed successfully."
                        : $"Tool '{toolName}' failed to execute."
                };
            }

            return new ToolExecutionResult { Success = false, Message = $"Unknown tool '{toolName}'." };
        }

        public async Task<ToolExecutionResult> ExecuteToolAsync(string toolName, string argumentsJson, bool confirmed = false)
        {
            Dictionary<string, object?>? args = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(argumentsJson))
                {
                    args = JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson);
                }
            }
            catch
            {
                return new ToolExecutionResult { Success = false, Message = "Invalid tool arguments." };
            }

            return await ExecuteToolAsync(toolName, args, confirmed);
        }

        private async Task<ToolExecutionResult> RunDefenderScanAsync(Dictionary<string, object?>? args)
        {
            if (!_defender.IsAvailable())
            {
                return new ToolExecutionResult { Success = false, Message = "Windows Defender not available on this device." };
            }

            var full = false;
            if (args != null && args.TryGetValue("full", out var fullValue))
            {
                bool.TryParse(fullValue?.ToString(), out full);
            }

            var output = await _defender.RunScanAsync(full);
            return new ToolExecutionResult
            {
                Success = true,
                Message = output
            };
        }

        private static string[] ExtractParameters(string toolName, Dictionary<string, object?>? args)
        {
            if (args == null)
            {
                return Array.Empty<string>();
            }

            return toolName.ToLowerInvariant() switch
            {
                "launch" => new[] { ExtractTarget(args) ?? string.Empty },
                "explorer" => new[] { args.TryGetValue("path", out var path) ? path?.ToString() ?? string.Empty : string.Empty },
                _ => Array.Empty<string>()
            };
        }

        private static string? ExtractTarget(Dictionary<string, object?>? args)
        {
            if (args == null)
            {
                return null;
            }

            string? raw = null;
            if (args.TryGetValue("target", out var targetValue))
            {
                raw = targetValue?.ToString();
            }
            else if (args.TryGetValue("app", out var appValue))
            {
                raw = appValue?.ToString();
            }
            else if (args.TryGetValue("application", out var applicationValue))
            {
                raw = applicationValue?.ToString();
            }
            else if (args.TryGetValue("name", out var nameValue))
            {
                raw = nameValue?.ToString();
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var target = raw.Trim();
            target = target.Trim('\"', '\'');
            target = target.TrimEnd('.', ',', '!', '?', ';', ':');

            var lowerTarget = target.ToLowerInvariant();
            if (lowerTarget.StartsWith("open "))
            {
                target = target.Substring(5).Trim();
            }
            else if (lowerTarget.StartsWith("launch "))
            {
                target = target.Substring(7).Trim();
            }
            else if (lowerTarget.StartsWith("start "))
            {
                target = target.Substring(6).Trim();
            }
            else if (lowerTarget.StartsWith("run "))
            {
                target = target.Substring(4).Trim();
            }

            if (target.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
            {
                target = target.Substring(4).Trim();
            }

            if (target.EndsWith(" app", StringComparison.OrdinalIgnoreCase))
            {
                target = target.Substring(0, target.Length - 4).Trim();
            }
            else if (target.EndsWith(" application", StringComparison.OrdinalIgnoreCase))
            {
                target = target.Substring(0, target.Length - 12).Trim();
            }

            if (target.Equals("calculator", StringComparison.OrdinalIgnoreCase))
            {
                return "calc";
            }

            if (target.Equals("task manager", StringComparison.OrdinalIgnoreCase))
            {
                return "taskmgr";
            }

            if (target.Equals("file explorer", StringComparison.OrdinalIgnoreCase) ||
                target.Equals("explorer", StringComparison.OrdinalIgnoreCase))
            {
                return "explorer";
            }

            if (target.Equals("settings", StringComparison.OrdinalIgnoreCase))
            {
                return "ms-settings:";
            }

            return target;
        }

        private static string BuildConfirmationMessage(string toolName, Dictionary<string, object?>? args)
        {
            var safeArgs = args ?? new Dictionary<string, object?>();
            var json = JsonSerializer.Serialize(safeArgs);
            return $"Confirmation required before running '{toolName}'.\n```confirm\n{{\"name\":\"{toolName}\",\"arguments\":{json}}}\n```";
        }
    }
}
