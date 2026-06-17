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
            },
            new ToolDefinition
            {
                Name = "system_info",
                Description = "Read local machine information such as OS, uptime, drives, processor count, and process count.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "list_processes",
                Description = "List running local processes sorted by memory usage.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"count\": { \"type\": \"integer\" } }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "performance_snapshot",
                Description = "Read current local CPU, memory, and top process usage.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "network_adapters",
                Description = "List local network adapters and IP addresses.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "battery_status",
                Description = "Read local battery charge percentage and charging status when a battery is present.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "kill_process",
                Description = "Stop a local process by PID or name. Requires user confirmation.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"pid\": { \"type\": \"integer\" }, \"name\": { \"type\": \"string\" } }, \"required\": [] }"
            }
        };

        private static readonly IReadOnlyList<ToolDefinition> FortisTools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "defender_scan",
                Description = "Run a Windows Defender scan (quick or full).",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"full\": { \"type\": \"boolean\" } }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "defender_status",
                Description = "Read local Windows Defender and antivirus product status.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "defender_scan_status",
                Description = "Read recent Microsoft Defender scan status and result events.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "network_adapters",
                Description = "List local network adapters and IP addresses.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "list_processes",
                Description = "List running local processes sorted by memory usage.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"count\": { \"type\": \"integer\" } }, \"required\": [] }"
            }
        };

        private static readonly IReadOnlyList<ToolDefinition> NexaTools = new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "taskmgr",
                Description = "Open Windows Task Manager.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "performance_snapshot",
                Description = "Read current local CPU, memory, and top process usage.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "system_info",
                Description = "Read local machine information such as OS, uptime, drives, processor count, and process count.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "battery_status",
                Description = "Read local battery charge percentage and charging status when a battery is present.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { }, \"required\": [] }"
            },
            new ToolDefinition
            {
                Name = "list_processes",
                Description = "List running local processes sorted by memory usage.",
                JsonSchema = "{ \"type\": \"object\", \"properties\": { \"count\": { \"type\": \"integer\" } }, \"required\": [] }"
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
