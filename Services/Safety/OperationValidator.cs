using System;
using System.Collections.Generic;

namespace Smartitecture.Services.Safety
{
    public sealed class OperationValidator
    {
        private static readonly HashSet<string> DestructiveTools = new(StringComparer.OrdinalIgnoreCase)
        {
            "shutdown",
            "delete_file",
            "kill_process",
            "registry_write",
            "firewall_rule"
        };

        public bool RequiresConfirmation(string toolName)
        {
            return DestructiveTools.Contains(toolName);
        }
    }
}
