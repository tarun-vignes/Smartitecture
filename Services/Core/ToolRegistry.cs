using System.Collections.Generic;

namespace Smartitecture.Services.Core
{
    public static class ToolRegistry
    {
        private static readonly IReadOnlyList<ToolDefinition> LumenTools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "launch",
                Description = "Launch an application by name or path.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"target\": { \"type\": \"string\" } }, \"required\": [\"target\"] }"
            },
            new ToolDefinition
            {
                Name = "explorer",
                Description = "Open File Explorer optionally at a target path.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"path\": { \"type\": \"string\" } }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "calculator",
                Description = "Open Windows Calculator.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            }
        };

        private static readonly IReadOnlyList<ToolDefinition> FortisTools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "defender_scan",
                Description = "Run a Windows Defender scan (quick or full).",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"full\": { \"type\": \"boolean\" } }, \"required\": [] }"
            }
        };

        private static readonly IReadOnlyList<ToolDefinition> NexaTools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "taskmgr",
                Description = "Open Windows Task Manager.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            }
        };

        public static IReadOnlyList<ToolDefinition> GetToolsForMode(AIModeType mode)
        {
            return mode switch
            {
                AIModeType.Fortis => FortisTools,
                AIModeType.Nexa => NexaTools,
                _ => LumenTools
            };
        }
    }
}
