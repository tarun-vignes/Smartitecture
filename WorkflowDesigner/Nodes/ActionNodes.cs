using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SmartitectureSimple.WorkflowDesigner;

namespace SmartitectureSimple.WorkflowDesigner.Nodes
{
    /// <summary>
    /// Node for taking screenshots
    /// </summary>
    public class ScreenshotNode : WorkflowNode
    {
        public override string NodeType => "Screenshot";
        public override string Description => "Captures a screenshot of the desktop";

        public ScreenshotNode()
        {
            Title = "Take Screenshot";
            Parameters["filename"] = "screenshot.png";
            Parameters["directory"] = "";
            Parameters["quality"] = 90;
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                context.Logger?.Invoke($"Executing screenshot node: {Title}");
                
                // Simulate screenshot capture (integrate with existing screenshot tool)
                var filename = Parameters.GetValueOrDefault("filename", "screenshot.png").ToString();
                var directory = Parameters.GetValueOrDefault("directory", "").ToString();
                
                await Task.Delay(1000); // Simulate processing time
                
                var outputData = new Dictionary<string, object>
                {
                    ["filename"] = filename,
                    ["filepath"] = string.IsNullOrEmpty(directory) ? filename : System.IO.Path.Combine(directory, filename),
                    ["timestamp"] = DateTime.Now
                };

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                return WorkflowExecutionResult.CreateSuccess($"Screenshot saved: {filename}", outputData);
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"Screenshot failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };
            
            if (string.IsNullOrWhiteSpace(Parameters.GetValueOrDefault("filename", "").ToString()))
            {
                result.IsValid = false;
                result.Errors.Add("Filename is required");
            }
            
            return result;
        }

        public override FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            // Filename input
            panel.Children.Add(new TextBlock { Text = "Filename:", FontWeight = FontWeights.Bold });
            var filenameBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("filename", "screenshot.png").ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            filenameBox.TextChanged += (s, e) => Parameters["filename"] = filenameBox.Text;
            panel.Children.Add(filenameBox);
            
            // Directory input
            panel.Children.Add(new TextBlock { Text = "Directory (optional):", FontWeight = FontWeights.Bold });
            var directoryBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("directory", "").ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            directoryBox.TextChanged += (s, e) => Parameters["directory"] = directoryBox.Text;
            panel.Children.Add(directoryBox);
            
            return panel;
        }
    }

    /// <summary>
    /// Node for mathematical calculations
    /// </summary>
    public class CalculatorNode : WorkflowNode
    {
        public override string NodeType => "Calculator";
        public override string Description => "Performs mathematical calculations";

        public CalculatorNode()
        {
            Title = "Calculate";
            Parameters["expression"] = "2 + 2";
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                context.Logger?.Invoke($"Executing calculator node: {Title}");
                
                var expression = Parameters.GetValueOrDefault("expression", "").ToString();
                
                // Simple calculation (integrate with existing calculator tool)
                await Task.Delay(500); // Simulate processing time
                
                // For demo purposes, just return the expression
                var result = $"Result of '{expression}'";
                
                var outputData = new Dictionary<string, object>
                {
                    ["expression"] = expression,
                    ["result"] = result,
                    ["timestamp"] = DateTime.Now
                };

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                return WorkflowExecutionResult.CreateSuccess($"Calculation completed: {result}", outputData);
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"Calculation failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };
            
            if (string.IsNullOrWhiteSpace(Parameters.GetValueOrDefault("expression", "").ToString()))
            {
                result.IsValid = false;
                result.Errors.Add("Mathematical expression is required");
            }
            
            return result;
        }

        public override FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            panel.Children.Add(new TextBlock { Text = "Mathematical Expression:", FontWeight = FontWeights.Bold });
            var expressionBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("expression", "2 + 2").ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            expressionBox.TextChanged += (s, e) => Parameters["expression"] = expressionBox.Text;
            panel.Children.Add(expressionBox);
            
            panel.Children.Add(new TextBlock 
            { 
                Text = "Examples: 25 * 4 + 17, (100 - 25) / 3, sqrt(144)",
                FontStyle = FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Gray
            });
            
            return panel;
        }
    }

    /// <summary>
    /// Node for system information retrieval
    /// </summary>
    public class SystemInfoNode : WorkflowNode
    {
        public override string NodeType => "SystemInfo";
        public override string Description => "Retrieves system information";

        public SystemInfoNode()
        {
            Title = "Get System Info";
            Parameters["infoType"] = "all";
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                context.Logger?.Invoke($"Executing system info node: {Title}");
                
                var infoType = Parameters.GetValueOrDefault("infoType", "all").ToString();
                
                await Task.Delay(1000); // Simulate processing time
                
                var outputData = new Dictionary<string, object>
                {
                    ["infoType"] = infoType,
                    ["computerName"] = Environment.MachineName,
                    ["userName"] = Environment.UserName,
                    ["osVersion"] = Environment.OSVersion.ToString(),
                    ["timestamp"] = DateTime.Now
                };

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                return WorkflowExecutionResult.CreateSuccess($"System info retrieved: {infoType}", outputData);
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"System info failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            return ValidationResult.Success();
        }

        public override FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            panel.Children.Add(new TextBlock { Text = "Information Type:", FontWeight = FontWeights.Bold });
            
            var comboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 10) };
            comboBox.Items.Add("All Information");
            comboBox.Items.Add("Hardware Only");
            comboBox.Items.Add("Software Only");
            comboBox.Items.Add("Network Only");
            
            var currentType = Parameters.GetValueOrDefault("infoType", "all").ToString();
            comboBox.SelectedIndex = currentType switch
            {
                "hardware" => 1,
                "software" => 2,
                "network" => 3,
                _ => 0
            };
            
            comboBox.SelectionChanged += (s, e) =>
            {
                Parameters["infoType"] = comboBox.SelectedIndex switch
                {
                    1 => "hardware",
                    2 => "software", 
                    3 => "network",
                    _ => "all"
                };
            };
            
            panel.Children.Add(comboBox);
            
            return panel;
        }
    }

    /// <summary>
    /// Node for delays and waiting
    /// </summary>
    public class DelayNode : WorkflowNode
    {
        public override string NodeType => "Delay";
        public override string Description => "Waits for a specified amount of time";

        public DelayNode()
        {
            Title = "Wait";
            Parameters["seconds"] = 1;
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                var seconds = Convert.ToDouble(Parameters.GetValueOrDefault("seconds", 1));
                
                context.Logger?.Invoke($"Waiting for {seconds} seconds...");
                
                await Task.Delay(TimeSpan.FromSeconds(seconds), context.CancellationToken);

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                return WorkflowExecutionResult.CreateSuccess($"Waited for {seconds} seconds");
            }
            catch (OperationCanceledException)
            {
                Status = WorkflowNodeStatus.Skipped;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure("Delay was cancelled");
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"Delay failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            var result = new ValidationResult { IsValid = true };
            
            if (!double.TryParse(Parameters.GetValueOrDefault("seconds", "1").ToString(), out var seconds) || seconds < 0)
            {
                result.IsValid = false;
                result.Errors.Add("Delay must be a positive number");
            }
            
            return result;
        }

        public override FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            panel.Children.Add(new TextBlock { Text = "Delay (seconds):", FontWeight = FontWeights.Bold });
            var secondsBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("seconds", "1").ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            secondsBox.TextChanged += (s, e) => 
            {
                if (double.TryParse(secondsBox.Text, out var value))
                    Parameters["seconds"] = value;
            };
            panel.Children.Add(secondsBox);
            
            return panel;
        }
    }

    /// <summary>
    /// Node for decision making and branching logic
    /// </summary>
    public class DecisionNode : WorkflowNode
    {
        public override string NodeType => "Decision";
        public override string Description => "Makes decisions based on conditions and branches execution";

        public DecisionNode()
        {
            Title = "Decision";
            Parameters["condition"] = "value > 0";
            Parameters["leftValue"] = "5";
            Parameters["operator"] = ">";
            Parameters["rightValue"] = "3";
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                context.Logger?.Invoke($"Evaluating decision: {Title}");
                
                var leftValue = Parameters.GetValueOrDefault("leftValue", "5").ToString();
                var rightValue = Parameters.GetValueOrDefault("rightValue", "3").ToString();
                var operatorStr = Parameters.GetValueOrDefault("operator", ">").ToString();

                // Simple numeric comparison
                bool result = false;
                if (double.TryParse(leftValue, out double left) && double.TryParse(rightValue, out double right))
                {
                    result = operatorStr switch
                    {
                        ">" => left > right,
                        "<" => left < right,
                        ">=" => left >= right,
                        "<=" => left <= right,
                        "==" => Math.Abs(left - right) < 0.0001,
                        "!=" => Math.Abs(left - right) >= 0.0001,
                        _ => false
                    };
                }

                var outputData = new Dictionary<string, object>
                {
                    ["result"] = result,
                    ["condition"] = $"{leftValue} {operatorStr} {rightValue}",
                    ["timestamp"] = DateTime.Now
                };

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                context.Logger?.Invoke($"Decision result: {result} ({leftValue} {operatorStr} {rightValue} = {result})");

                return WorkflowExecutionResult.CreateSuccess($"Decision evaluated: {result}", outputData);
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"Decision evaluation failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            return ValidationResult.Success();
        }

        public override System.Windows.FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            panel.Children.Add(new TextBlock { Text = "Left Value:", FontWeight = FontWeights.Bold });
            var leftValueBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("leftValue", "5").ToString(),
                Margin = new Thickness(0, 0, 0, 5)
            };
            leftValueBox.TextChanged += (s, e) => Parameters["leftValue"] = leftValueBox.Text;
            panel.Children.Add(leftValueBox);

            panel.Children.Add(new TextBlock { Text = "Operator:", FontWeight = FontWeights.Bold });
            var operatorCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 5) };
            var operators = new[] { ">", "<", ">=", "<=", "==", "!=" };
            foreach (var op in operators) operatorCombo.Items.Add(op);
            operatorCombo.SelectedItem = Parameters.GetValueOrDefault("operator", ">");
            operatorCombo.SelectionChanged += (s, e) => Parameters["operator"] = operatorCombo.SelectedItem?.ToString() ?? ">";
            panel.Children.Add(operatorCombo);

            panel.Children.Add(new TextBlock { Text = "Right Value:", FontWeight = FontWeights.Bold });
            var rightValueBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("rightValue", "3").ToString(),
                Margin = new Thickness(0, 0, 0, 5)
            };
            rightValueBox.TextChanged += (s, e) => Parameters["rightValue"] = rightValueBox.Text;
            panel.Children.Add(rightValueBox);
            
            return panel;
        }
    }

    /// <summary>
    /// Node for loop/iteration logic
    /// </summary>
    public class LoopNode : WorkflowNode
    {
        public override string NodeType => "Loop";
        public override string Description => "Repeats execution for a specified number of times";

        public LoopNode()
        {
            Title = "Loop";
            Parameters["iterations"] = 3;
            Parameters["loopType"] = "count"; // count, condition
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                context.Logger?.Invoke($"Executing loop: {Title}");
                
                var iterations = Convert.ToInt32(Parameters.GetValueOrDefault("iterations", 3));
                
                await Task.Delay(500); // Simulate processing time
                
                var outputData = new Dictionary<string, object>
                {
                    ["iterations"] = iterations,
                    ["loopType"] = Parameters.GetValueOrDefault("loopType", "count"),
                    ["timestamp"] = DateTime.Now
                };

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                context.Logger?.Invoke($"Loop completed: {iterations} iterations");

                return WorkflowExecutionResult.CreateSuccess($"Loop executed {iterations} times", outputData);
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"Loop failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            return ValidationResult.Success();
        }

        public override System.Windows.FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            panel.Children.Add(new TextBlock { Text = "Iterations:", FontWeight = FontWeights.Bold });
            var iterationsBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("iterations", 3).ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            iterationsBox.TextChanged += (s, e) => 
            {
                if (int.TryParse(iterationsBox.Text, out int value))
                    Parameters["iterations"] = value;
            };
            panel.Children.Add(iterationsBox);
            
            return panel;
        }
    }

    /// <summary>
    /// Node for user input
    /// </summary>
    public class InputNode : WorkflowNode
    {
        public override string NodeType => "Input";
        public override string Description => "Prompts user for input";

        public InputNode()
        {
            Title = "User Input";
            Parameters["prompt"] = "Enter value:";
            Parameters["inputType"] = "text"; // text, number, boolean
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                context.Logger?.Invoke($"Requesting user input: {Title}");
                
                var prompt = Parameters.GetValueOrDefault("prompt", "Enter value:").ToString();
                
                await Task.Delay(500); // Simulate user input time
                
                // For demo purposes, simulate user input
                var userInput = "Sample Input";
                
                var outputData = new Dictionary<string, object>
                {
                    ["userInput"] = userInput,
                    ["prompt"] = prompt,
                    ["inputType"] = Parameters.GetValueOrDefault("inputType", "text"),
                    ["timestamp"] = DateTime.Now
                };

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                context.Logger?.Invoke($"User input received: {userInput}");

                return WorkflowExecutionResult.CreateSuccess($"Input received: {userInput}", outputData);
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"Input failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            return ValidationResult.Success();
        }

        public override System.Windows.FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            panel.Children.Add(new TextBlock { Text = "Prompt:", FontWeight = FontWeights.Bold });
            var promptBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("prompt", "Enter value:").ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            promptBox.TextChanged += (s, e) => Parameters["prompt"] = promptBox.Text;
            panel.Children.Add(promptBox);
            
            return panel;
        }
    }

    /// <summary>
    /// Node for output/display
    /// </summary>
    public class OutputNode : WorkflowNode
    {
        public override string NodeType => "Output";
        public override string Description => "Displays output to user";

        public OutputNode()
        {
            Title = "Display Output";
            Parameters["message"] = "Hello World!";
            Parameters["outputType"] = "message"; // message, file, screen
        }

        public override async Task<WorkflowExecutionResult> ExecuteAsync(WorkflowContext context)
        {
            try
            {
                Status = WorkflowNodeStatus.Executing;
                IsExecuting = true;

                context.Logger?.Invoke($"Displaying output: {Title}");
                
                var message = Parameters.GetValueOrDefault("message", "Hello World!").ToString();
                
                await Task.Delay(500); // Simulate display time
                
                var outputData = new Dictionary<string, object>
                {
                    ["message"] = message,
                    ["outputType"] = Parameters.GetValueOrDefault("outputType", "message"),
                    ["timestamp"] = DateTime.Now
                };

                Status = WorkflowNodeStatus.Completed;
                IsExecuting = false;

                context.Logger?.Invoke($"Output displayed: {message}");

                return WorkflowExecutionResult.CreateSuccess($"Output: {message}", outputData);
            }
            catch (Exception ex)
            {
                Status = WorkflowNodeStatus.Failed;
                IsExecuting = false;
                return WorkflowExecutionResult.CreateFailure($"Output failed: {ex.Message}", ex);
            }
        }

        public override ValidationResult Validate()
        {
            return ValidationResult.Success();
        }

        public override System.Windows.FrameworkElement GetConfigurationUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            
            panel.Children.Add(new TextBlock { Text = "Message:", FontWeight = FontWeights.Bold });
            var messageBox = new TextBox 
            { 
                Text = Parameters.GetValueOrDefault("message", "Hello World!").ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };
            messageBox.TextChanged += (s, e) => Parameters["message"] = messageBox.Text;
            panel.Children.Add(messageBox);
            
            return panel;
        }
    }
}
