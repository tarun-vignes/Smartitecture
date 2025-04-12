using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIPal.Commands;

namespace AIPal.Services
{
    public class CommandMapper
    {
        private readonly Dictionary<string, ISystemCommand> _commands;

        public CommandMapper()
        {
            _commands = new Dictionary<string, ISystemCommand>(StringComparer.OrdinalIgnoreCase);
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            RegisterCommand(new OpenSettingsCommand());
            // Add more commands here
        }

        private void RegisterCommand(ISystemCommand command)
        {
            _commands[command.CommandName] = command;
        }

        public async Task<bool> ExecuteCommandAsync(string commandName, string[] parameters)
        {
            if (_commands.TryGetValue(commandName, out var command))
            {
                if (command.RequiresElevation && !IsElevated())
                {
                    // Handle elevation request
                    return false;
                }
                return await command.ExecuteAsync(parameters);
            }
            return false;
        }

        private bool IsElevated()
        {
            // TODO: Implement elevation check
            return false;
        }
    }
}
