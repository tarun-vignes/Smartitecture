using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using SmartitectureSimple.WorkflowDesigner.Nodes;

namespace SmartitectureSimple.WorkflowDesigner
{
    /// <summary>
    /// Engine for executing visual workflows
    /// </summary>
    public class WorkflowEngine
    {
        private readonly LoggingService _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public WorkflowEngine(LoggingService logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute a workflow with progress reporting
        /// </summary>
        public async Task<WorkflowExecutionSummary> ExecuteWorkflowAsync(
            Workflow workflow, 
            IProgress<WorkflowProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            var summary = new WorkflowExecutionSummary
            {
                WorkflowId = workflow.Id,
                StartTime = DateTime.Now,
                TotalNodes = workflow.Nodes.Count
            };

            try
            {
                _logger.Info($"Starting workflow execution: {workflow.Name}");

                // Create execution context
                var context = new WorkflowContext
                {
                    CancellationToken = cancellationToken,
                    Progress = progress,
                    Logger = message => _logger.Info($"[Workflow] {message}")
                };

                // Reset all node statuses
                foreach (var node in workflow.Nodes)
                {
                    node.Status = WorkflowNodeStatus.Ready;
                    node.IsExecuting = false;
                }

                // Get execution order (topological sort)
                var executionOrder = GetExecutionOrder(workflow);
                if (executionOrder == null)
                {
                    throw new InvalidOperationException("Workflow contains circular dependencies");
                }

                // Execute nodes in order
                var completedNodes = 0;
                foreach (var node in executionOrder)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        summary.Status = WorkflowExecutionStatus.Cancelled;
                        break;
                    }

                    // Report progress
                    progress?.Report(new WorkflowProgress
                    {
                        CurrentNodeId = node.Id,
                        CurrentNodeTitle = node.Title,
                        CompletedNodes = completedNodes,
                        TotalNodes = workflow.Nodes.Count,
                        Message = $"Executing: {node.Title}"
                    });

                    // Execute the node
                    var result = await ExecuteNodeAsync(node, context);
                    summary.NodeResults[node.Id] = result;

                    if (result.Success)
                    {
                        completedNodes++;
                        summary.SuccessfulNodes++;
                        
                        // Pass output data to context for next nodes
                        foreach (var kvp in result.OutputData)
                        {
                            context.Variables[$"{node.Id}.{kvp.Key}"] = kvp.Value;
                        }
                    }
                    else
                    {
                        summary.FailedNodes++;
                        _logger.Error($"Node execution failed: {node.Title} - {result.Message}", result.Exception);
                        
                        // Stop execution on failure (could be configurable)
                        summary.Status = WorkflowExecutionStatus.Failed;
                        summary.ErrorMessage = result.Message;
                        break;
                    }
                }

                // Final progress report
                progress?.Report(new WorkflowProgress
                {
                    CompletedNodes = completedNodes,
                    TotalNodes = workflow.Nodes.Count,
                    Message = summary.Status == WorkflowExecutionStatus.Failed ? "Workflow failed" : "Workflow completed"
                });

                if (summary.Status == WorkflowExecutionStatus.Running)
                {
                    summary.Status = WorkflowExecutionStatus.Completed;
                }

                summary.EndTime = DateTime.Now;
                summary.Duration = summary.EndTime - summary.StartTime;

                _logger.Info($"Workflow execution completed: {workflow.Name} - Status: {summary.Status}");
                return summary;
            }
            catch (Exception ex)
            {
                summary.Status = WorkflowExecutionStatus.Failed;
                summary.ErrorMessage = ex.Message;
                summary.EndTime = DateTime.Now;
                summary.Duration = summary.EndTime - summary.StartTime;

                _logger.Error($"Workflow execution failed: {workflow.Name}", ex);
                return summary;
            }
        }

        /// <summary>
        /// Execute a single node
        /// </summary>
        private async Task<WorkflowExecutionResult> ExecuteNodeAsync(WorkflowNode node, WorkflowContext context)
        {
            try
            {
                // Validate node before execution
                var validation = node.Validate();
                if (!validation.IsValid)
                {
                    return WorkflowExecutionResult.CreateFailure($"Node validation failed: {string.Join(", ", validation.Errors)}");
                }

                // Execute the node
                var result = await node.ExecuteAsync(context);
                return result;
            }
            catch (Exception ex)
            {
                return WorkflowExecutionResult.CreateFailure($"Node execution exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get the execution order for workflow nodes (topological sort)
        /// </summary>
        private List<WorkflowNode> GetExecutionOrder(Workflow workflow)
        {
            var nodes = workflow.Nodes.ToDictionary(n => n.Id, n => n);
            var inDegree = workflow.Nodes.ToDictionary(n => n.Id, n => 0);
            var adjacencyList = workflow.Nodes.ToDictionary(n => n.Id, n => new List<string>());

            // Build adjacency list and calculate in-degrees
            foreach (var connection in workflow.Connections)
            {
                if (nodes.ContainsKey(connection.SourceNodeId) && nodes.ContainsKey(connection.TargetNodeId))
                {
                    adjacencyList[connection.SourceNodeId].Add(connection.TargetNodeId);
                    inDegree[connection.TargetNodeId]++;
                }
            }

            // Topological sort using Kahn's algorithm
            var queue = new Queue<string>();
            var result = new List<WorkflowNode>();

            // Start with nodes that have no dependencies
            foreach (var kvp in inDegree.Where(kvp => kvp.Value == 0))
            {
                queue.Enqueue(kvp.Key);
            }

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                result.Add(nodes[currentId]);

                // Process all neighbors
                foreach (var neighborId in adjacencyList[currentId])
                {
                    inDegree[neighborId]--;
                    if (inDegree[neighborId] == 0)
                    {
                        queue.Enqueue(neighborId);
                    }
                }
            }

            // Check for circular dependencies
            if (result.Count != workflow.Nodes.Count)
            {
                return null; // Circular dependency detected
            }

            return result;
        }

        /// <summary>
        /// Cancel workflow execution
        /// </summary>
        public void CancelExecution()
        {
            _cancellationTokenSource?.Cancel();
        }
    }

    /// <summary>
    /// Represents a complete workflow
    /// </summary>
    public class Workflow
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "New Workflow";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public List<WorkflowNode> Nodes { get; set; } = new();
        public List<WorkflowConnection> Connections { get; set; } = new();
        public Dictionary<string, object> GlobalParameters { get; set; } = new();

        /// <summary>
        /// Save workflow to JSON file
        /// </summary>
        public async Task SaveToFileAsync(string filePath)
        {
            ModifiedDate = DateTime.Now;
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Load workflow from JSON file
        /// </summary>
        public static async Task<Workflow> LoadFromFileAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<Workflow>(json, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Validate the entire workflow
        /// </summary>
        public ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate all nodes
            foreach (var node in Nodes)
            {
                var nodeValidation = node.Validate();
                if (!nodeValidation.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(nodeValidation.Errors.Select(e => $"{node.Title}: {e}"));
                }
                result.Warnings.AddRange(nodeValidation.Warnings.Select(w => $"{node.Title}: {w}"));
            }

            // Check for orphaned nodes (no connections)
            var connectedNodes = new HashSet<string>();
            foreach (var connection in Connections)
            {
                connectedNodes.Add(connection.SourceNodeId);
                connectedNodes.Add(connection.TargetNodeId);
            }

            var orphanedNodes = Nodes.Where(n => !connectedNodes.Contains(n.Id) && Nodes.Count > 1).ToList();
            if (orphanedNodes.Any())
            {
                result.Warnings.AddRange(orphanedNodes.Select(n => $"Node '{n.Title}' is not connected to the workflow"));
            }

            return result;
        }
    }

    /// <summary>
    /// Summary of workflow execution
    /// </summary>
    public class WorkflowExecutionSummary
    {
        public string WorkflowId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public WorkflowExecutionStatus Status { get; set; } = WorkflowExecutionStatus.Running;
        public int TotalNodes { get; set; }
        public int SuccessfulNodes { get; set; }
        public int FailedNodes { get; set; }
        public string ErrorMessage { get; set; } = "";
        public Dictionary<string, WorkflowExecutionResult> NodeResults { get; set; } = new();
    }

    /// <summary>
    /// Workflow execution status
    /// </summary>
    public enum WorkflowExecutionStatus
    {
        Running,
        Completed,
        Failed,
        Cancelled
    }
}
