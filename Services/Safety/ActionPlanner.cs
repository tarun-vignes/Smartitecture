using System.Collections.Generic;

namespace Smartitecture.Services.Safety
{
    public sealed class ActionPlanner
    {
        public IReadOnlyList<string> BuildPlan(string userGoal)
        {
            if (string.IsNullOrWhiteSpace(userGoal))
            {
                return new List<string>();
            }

            return new List<string>
            {
                "Understand the request and required permissions.",
                "Propose safe actions and request confirmation if needed.",
                "Execute approved actions and report results."
            };
        }
    }
}
