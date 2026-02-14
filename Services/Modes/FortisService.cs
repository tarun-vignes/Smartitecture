using System.Collections.Generic;
using Smartitecture.Services.Core;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Modes
{
    public sealed class FortisService : IAIMode
    {
        public AIModeType Mode => AIModeType.Fortis;
        public string DisplayName => "FORTIS";
        public string SystemPrompt =>
            "You are FORTIS, Smartitecture's security and defense expert. Prioritize safety, verification, " +
            "and least-privilege. Explain risks before suggesting actions and request confirmation for any " +
            "destructive or security-sensitive steps. If you need a tool, output a tool block exactly like:\n" +
            "```tool\n{\"name\":\"defender_scan\",\"arguments\":{\"full\":false}}\n```\n" +
            "You may add a short user-facing sentence above the tool block.";

        public IReadOnlyList<ToolDefinition> Tools { get; } = ToolRegistry.GetToolsForMode(AIModeType.Fortis);

        public bool AllowAutonomy => false;
    }
}
