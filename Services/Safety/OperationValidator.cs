using System.Collections.Generic;

namespace Smartitecture.Services.Safety
{
    public class OperationValidator
    {
        public IReadOnlyList<string> ValidateDestructiveAction(string description)
        {
            var warnings = new List<string>();
            if (string.IsNullOrWhiteSpace(description)) warnings.Add("No operation description provided");
            // Add more guardrails as needed (path checks, scope, permissions)
            return warnings;
        }
    }
}

