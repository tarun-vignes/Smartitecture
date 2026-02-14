using System.Collections.Generic;
using Smartitecture.Services.Core;

namespace Smartitecture.Services.Interfaces
{
    public interface IAIMode
    {
        AIModeType Mode { get; }
        string DisplayName { get; }
        string SystemPrompt { get; }
        IReadOnlyList<ToolDefinition> Tools { get; }
        bool AllowAutonomy { get; }
    }
}
