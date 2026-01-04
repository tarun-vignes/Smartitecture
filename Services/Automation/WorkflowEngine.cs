using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smartitecture.Services.Automation
{
    public class WorkflowEngine
    {
        public class WorkflowStep
        {
            public string Name { get; set; } = string.Empty;
            public Func<Task<bool>> Action { get; set; } = () => Task.FromResult(true);
        }

        public async Task<(bool success, List<string> log)> RunWorkflowAsync(IEnumerable<WorkflowStep> steps)
        {
            var log = new List<string>();
            foreach (var step in steps)
            {
                try
                {
                    var ok = await step.Action();
                    log.Add($"{DateTime.Now:O} {step.Name}: {(ok ? "OK" : "FAILED")}");
                    if (!ok) return (false, log);
                }
                catch (Exception ex)
                {
                    log.Add($"{DateTime.Now:O} {step.Name}: ERROR {ex.Message}");
                    return (false, log);
                }
            }
            return (true, log);
        }
    }
}

