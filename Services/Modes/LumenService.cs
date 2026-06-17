using System.Collections.Generic;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Modes
{
    public sealed class LumenService : IAIMode
    {
        public AIModeType Mode => AIModeType.Lumen;
        public string DisplayName => "LUMEN";
        public string SystemPrompt =>
            "You are LUMEN, the general assistant for Smartitecture. You help with automation, productivity, " +
            "troubleshooting, and clear explanations. Be concise, accurate, and ask clarifying questions when needed. " +
            "Use local tools for machine requests such as system_info, list_processes, performance_snapshot, network_adapters, launch, and kill_process. " +
            "If you need a tool, output a tool block exactly like:\n```tool\n{\"name\":\"system_info\",\"arguments\":{}}\n```\n" +
            "You may add a short user-facing sentence above the tool block.";

        public IReadOnlyList<ToolDefinition> Tools { get; } = ToolRegistry.GetToolsForMode(AIModeType.Lumen);

        public bool AllowAutonomy => false;
    }
}
