using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SmartitectureSimple.WorkflowDesigner
{
    /// <summary>
    /// Base class for all workflow nodes
    /// </summary>
    public abstract class WorkflowNode : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private double _x;
        private double _y;
        private bool _isSelected;
        private bool _isExecuting;
        private WorkflowNodeStatus _status;

        public WorkflowNode()
        {
            _id = Guid.NewGuid().ToString();
            _title = GetType().Name;
            Inputs = new List<WorkflowConnection>();
            Outputs = new List<WorkflowConnection>();
            Parameters = new Dictionary<string, object>();
            _status = WorkflowNodeStatus.Ready;
        }

        #region Properties

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            set => SetProperty(ref _isExecuting, value);
        }

        public WorkflowNodeStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public List<WorkflowConnection> Inputs { get; set; }
        public List<WorkflowConnection> Outputs { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        [JsonIgnore]
        public abstract string NodeType { get; }
        
        [JsonIgnore]
        public abstract string Description { get; }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Execute the node's functionality
        /// </summary>
        public abstract Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context);

        /// <summary>
        /// Validate the node configuration
        /// </summary>
        public abstract ValidationResult Validate();

        /// <summary>
        /// Get the node's configuration UI
        /// </summary>
        public abstract System.Windows.FrameworkElement GetConfigurationUI();

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Connection between workflow nodes
    /// </summary>
    public class WorkflowConnection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SourceNodeId { get; set; }
        public string TargetNodeId { get; set; }
        public string SourcePort { get; set; } = "Output";
        public string TargetPort { get; set; } = "Input";
        public string Label { get; set; } = "";
    }

    /// <summary>
    /// Workflow execution context
    /// </summary>
    public class WorkflowContext
    {
        public Dictionary<string, object> Variables { get; set; } = new();
        public Dictionary<string, object> GlobalParameters { get; set; } = new();
        public CancellationToken CancellationToken { get; set; }
        public IProgress<WorkflowProgress> Progress { get; set; }
        public Action<string> Logger { get; set; }
    }

    /// <summary>
    /// Result of workflow node execution
    /// </summary>
    public class WorkflowExecutionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public Dictionary<string, object> OutputData { get; set; } = new();
        public Exception Exception { get; set; }
        
        public static WorkflowExecutionResult CreateSuccess(string message = "", Dictionary<string, object> outputData = null)
        {
            return new WorkflowExecutionResult
            {
                Success = true,
                Message = message,
                OutputData = outputData ?? new Dictionary<string, object>()
            };
        }
        
        public static WorkflowExecutionResult CreateFailure(string message, Exception exception = null)
        {
            return new WorkflowExecutionResult
            {
                Success = false,
                Message = message,
                Exception = exception
            };
        }
    }

    /// <summary>
    /// Workflow progress information
    /// </summary>
    public class WorkflowProgress
    {
        public string CurrentNodeId { get; set; }
        public string CurrentNodeTitle { get; set; }
        public int CompletedNodes { get; set; }
        public int TotalNodes { get; set; }
        public double PercentComplete => TotalNodes > 0 ? (double)CompletedNodes / TotalNodes * 100 : 0;
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Validation result for node configuration
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        
        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Failure(params string[] errors) => new ValidationResult { IsValid = false, Errors = errors.ToList() };
    }

    /// <summary>
    /// Workflow node execution status
    /// </summary>
    public enum WorkflowNodeStatus
    {
        Ready,
        Executing,
        Completed,
        Failed,
        Skipped
    }
}
