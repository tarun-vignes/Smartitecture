using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smartitecture.Services.Automation
{
    // Named to avoid confusion with Windows' Task Scheduler COM
    public class SmartitectureTaskScheduler
    {
        private readonly List<string> _tasks = new();

        public async Task<string> ScheduleTaskAsync(string name, DateTime when, string action)
        {
            // Placeholder: integrate with Windows Task Scheduler in a real impl
            var id = Guid.NewGuid().ToString();
            _tasks.Add($"{id}|{name}|{when:O}|{action}");
            await Task.CompletedTask;
            return id;
        }

        public IReadOnlyList<string> ListTasks() => _tasks.AsReadOnly();

        public async Task<bool> CancelTaskAsync(string id)
        {
            var idx = _tasks.FindIndex(t => t.StartsWith(id + "|", StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) _tasks.RemoveAt(idx);
            await Task.CompletedTask;
            return idx >= 0;
        }
    }
}

