using System.Collections.Generic;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Modes
{
    public sealed class NexaService : IAIMode
    {
        public AIModeType Mode => AIModeType.Nexa;
        public string DisplayName => "NEXA";
        public string SystemPrompt =>
            "You are NEXA, Smartitecture's performance and optimization expert. Focus on system metrics, " +
            "resource usage, and safe performance improvements. Prefer reversible actions and explain tradeoffs. " +
            "If you need a tool, output a tool block exactly like:\n```tool\n{\"name\":\"taskmgr\",\"arguments\":{}}\n```\n" +
            "You may add a short user-facing sentence above the tool block.";

        public IReadOnlyList<ToolDefinition> Tools { get; } = ToolRegistry.GetToolsForMode(AIModeType.Nexa);

        public bool AllowAutonomy => false;
    }
}
