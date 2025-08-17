using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using SmartitectureSimple.WorkflowDesigner;
using SmartitectureSimple.WorkflowDesigner.Nodes;

namespace SmartitectureSimple
{
    public partial class WorkflowDesignerWindow : Window
    {
        private readonly LoggingService _logger;
        private readonly WorkflowEngine _workflowEngine;
        private Workflow _currentWorkflow;
        private WorkflowNode _selectedNode;
        private CancellationTokenSource _executionCancellationTokenSource;
        private readonly Dictionary<string, Border> _nodeVisuals = new();
        private readonly Dictionary<string, Line> _connectionVisuals = new();
        private bool _isDragging;
        private Point _dragStartPoint;

        public WorkflowDesignerWindow(LoggingService logger)
        {
            InitializeComponent();
            _logger = logger;
            _workflowEngine = new WorkflowEngine(_logger);
            _currentWorkflow = new Workflow();
            
            InitializeToolbox();
            UpdateUI();
            
            _logger.Info("Workflow Designer opened");
        }

        #region Initialization

        private void InitializeToolbox()
        {
            // Set up drag and drop for toolbox items
            ActionNodesListBox.PreviewMouseLeftButtonDown += ToolboxItem_PreviewMouseLeftButtonDown;
            ActionNodesListBox.MouseMove += ToolboxItem_MouseMove;
            
            // Set up drag and drop for control nodes
            ControlNodesListBox.PreviewMouseLeftButtonDown += ToolboxItem_PreviewMouseLeftButtonDown;
            ControlNodesListBox.MouseMove += ToolboxItem_MouseMove;
        }

        #endregion

        #region Toolbar Events

        private void NewWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmUnsavedChanges())
            {
                _currentWorkflow = new Workflow();
                ClearCanvas();
                UpdateUI();
                LogMessage("New workflow created");
            }
        }

        private async void OpenWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmUnsavedChanges()) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Workflow files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Open Workflow"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _currentWorkflow = await Workflow.LoadFromFileAsync(openFileDialog.FileName);
                    LoadWorkflowToCanvas();
                    UpdateUI();
                    LogMessage($"Workflow loaded: {openFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load workflow: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    _logger.Error("Failed to load workflow", ex);
                }
            }
        }

        private async void SaveWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Workflow files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Save Workflow",
                FileName = $"{_currentWorkflow.Name}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _currentWorkflow.SaveToFileAsync(saveFileDialog.FileName);
                    LogMessage($"Workflow saved: {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save workflow: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    _logger.Error("Failed to save workflow", ex);
                }
            }
        }

        private async void RunWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWorkflow.Nodes.Count == 0)
            {
                MessageBox.Show("Workflow is empty. Add some nodes first.", "No Nodes", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Validate workflow before execution
            var validation = _currentWorkflow.Validate();
            if (!validation.IsValid)
            {
                var message = "Workflow validation failed:\n" + string.Join("\n", validation.Errors);
                MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                RunWorkflowButton.IsEnabled = false;
                StopWorkflowButton.IsEnabled = true;
                StatusLabel.Text = "Executing workflow...";

                _executionCancellationTokenSource = new CancellationTokenSource();
                
                var progress = new Progress<WorkflowProgress>(OnWorkflowProgress);
                
                LogMessage("Starting workflow execution...");
                var summary = await _workflowEngine.ExecuteWorkflowAsync(
                    _currentWorkflow, 
                    progress, 
                    _executionCancellationTokenSource.Token);

                LogMessage($"Workflow execution completed - Status: {summary.Status}");
                LogMessage($"Successful nodes: {summary.SuccessfulNodes}/{summary.TotalNodes}");
                LogMessage($"Duration: {summary.Duration.TotalSeconds:F1} seconds");

                if (summary.Status == WorkflowExecutionStatus.Failed)
                {
                    LogMessage($"Error: {summary.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Execution failed: {ex.Message}");
                _logger.Error("Workflow execution failed", ex);
            }
            finally
            {
                RunWorkflowButton.IsEnabled = true;
                StopWorkflowButton.IsEnabled = false;
                StatusLabel.Text = "Ready";
                _executionCancellationTokenSource?.Dispose();
                _executionCancellationTokenSource = null;
            }
        }

        private void StopWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            _executionCancellationTokenSource?.Cancel();
            LogMessage("Workflow execution cancelled by user");
        }

        private void ValidateWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            var validation = _currentWorkflow.Validate();
            
            if (validation.IsValid)
            {
                var message = "Workflow validation passed!";
                if (validation.Warnings.Any())
                {
                    message += "\n\nWarnings:\n" + string.Join("\n", validation.Warnings);
                }
                MessageBox.Show(message, "Validation Result", MessageBoxButton.OK, MessageBoxImage.Information);
                LogMessage("Workflow validation passed");
            }
            else
            {
                var message = "Workflow validation failed:\n\nErrors:\n" + string.Join("\n", validation.Errors);
                if (validation.Warnings.Any())
                {
                    message += "\n\nWarnings:\n" + string.Join("\n", validation.Warnings);
                }
                MessageBox.Show(message, "Validation Result", MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage("Workflow validation failed");
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WorkflowCanvas != null)
            {
                var scaleTransform = new System.Windows.Media.ScaleTransform(e.NewValue, e.NewValue);
                WorkflowCanvas.LayoutTransform = scaleTransform;
                ZoomLabel.Text = $"{e.NewValue * 100:F0}%";
            }
        }

        #endregion

        #region Drag and Drop

        private void ToolboxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(null);
        }

        private void ToolboxItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(null);
                var diff = _dragStartPoint - currentPosition;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var listBox = sender as ListBox;
                    var selectedItem = listBox?.SelectedItem as ListBoxItem;
                    
                    if (selectedItem?.Tag is string nodeType)
                    {
                        var dragData = new DataObject("NodeType", nodeType);
                        DragDrop.DoDragDrop(selectedItem, dragData, DragDropEffects.Copy);
                    }
                    
                    _isDragging = false;
                }
            }
        }

        private void WorkflowCanvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("NodeType"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void WorkflowCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("NodeType"))
            {
                var nodeType = e.Data.GetData("NodeType") as string;
                var dropPosition = e.GetPosition(WorkflowCanvas);
                
                CreateNodeAtPosition(nodeType, dropPosition);
            }
        }

        #endregion

        #region Node Management

        private void CreateNodeAtPosition(string nodeType, Point position)
        {
            WorkflowNode node = nodeType switch
            {
                "Screenshot" => new ScreenshotNode(),
                "Calculator" => new CalculatorNode(),
                "SystemInfo" => new SystemInfoNode(),
                "Delay" => new DelayNode(),
                "Decision" => new DecisionNode(),
                "Loop" => new LoopNode(),
                "Input" => new InputNode(),
                "Output" => new OutputNode(),
                _ => null
            };

            if (node != null)
            {
                node.X = position.X;
                node.Y = position.Y;
                
                _currentWorkflow.Nodes.Add(node);
                CreateNodeVisual(node);
                UpdateUI();
                
                LogMessage($"Added {nodeType} node at ({position.X:F0}, {position.Y:F0})");
            }
        }

        private void CreateNodeVisual(WorkflowNode node)
        {
            // Create main container for node with ports
            var nodeContainer = new Canvas
            {
                Width = 170,
                Height = 100
            };

            // Main node body
            var border = new Border
            {
                Width = 150,
                Height = 80,
                Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Cursor = Cursors.Hand
            };

            var textBlock = new TextBlock
            {
                Text = node.Title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;
            
            // Position main node in center of container
            Canvas.SetLeft(border, 10);
            Canvas.SetTop(border, 10);
            nodeContainer.Children.Add(border);

            // Create input port (left side)
            var inputPort = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = Brushes.LightGreen,
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 2,
                Cursor = Cursors.Cross
            };
            Canvas.SetLeft(inputPort, 0); // Left edge
            Canvas.SetTop(inputPort, 44); // Center vertically
            nodeContainer.Children.Add(inputPort);

            // Create output port (right side)
            var outputPort = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 2,
                Cursor = Cursors.Cross
            };
            Canvas.SetLeft(outputPort, 158); // Right edge
            Canvas.SetTop(outputPort, 44); // Center vertically
            nodeContainer.Children.Add(outputPort);

            // Add hover effects for ports
            inputPort.MouseEnter += (s, e) => {
                inputPort.Fill = Brushes.Green;
                LogMessage($"Input port hover: {node.Title}");
            };
            inputPort.MouseLeave += (s, e) => inputPort.Fill = Brushes.LightGreen;

            outputPort.MouseEnter += (s, e) => {
                outputPort.Fill = Brushes.Blue;
                LogMessage($"Output port hover: {node.Title}");
            };
            outputPort.MouseLeave += (s, e) => outputPort.Fill = Brushes.LightBlue;

            // Store port references for future connection system
            node.Parameters["_inputPort"] = inputPort;
            node.Parameters["_outputPort"] = outputPort;
            node.Parameters["_nodeContainer"] = nodeContainer;
            
            // Position the entire node container
            Canvas.SetLeft(nodeContainer, node.X);
            Canvas.SetTop(nodeContainer, node.Y);
            
            // Add event handlers with proper event handling
            border.MouseLeftButtonDown += (s, e) => {
                e.Handled = true;
                SelectNode(node);
                LogMessage($"Node clicked: {node.Title}");
            };
            border.MouseRightButtonDown += (s, e) => ShowNodeContextMenu(node, e);
            
            // Also add the event to the text block inside
            textBlock.MouseLeftButtonDown += (s, e) => {
                e.Handled = true;
                SelectNode(node);
                LogMessage($"Node text clicked: {node.Title}");
            };
            
            // Add the container to canvas and store reference
            WorkflowCanvas.Children.Add(nodeContainer);
            _nodeVisuals[node.Id] = border; // Keep border reference for selection highlighting
        }

        private void SelectNode(WorkflowNode node)
        {
            // Clear previous selection
            if (_selectedNode != null)
            {
                _selectedNode.IsSelected = false;
                if (_nodeVisuals.TryGetValue(_selectedNode.Id, out var prevBorder))
                {
                    prevBorder.BorderBrush = Brushes.White;
                }
            }

            // Select new node
            _selectedNode = node;
            node.IsSelected = true;
            
            if (_nodeVisuals.TryGetValue(node.Id, out var border))
            {
                border.BorderBrush = Brushes.Yellow;
            }

            ShowNodeProperties(node);
            LogMessage($"Selected node: {node.Title}");
        }

        private void ShowNodeProperties(WorkflowNode node)
        {
            PropertiesPanel.Children.Clear();
            
            // Node title
            var titleBlock = new TextBlock
            {
                Text = $"Node: {node.Title}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            PropertiesPanel.Children.Add(titleBlock);

            // Node type
            var typeBlock = new TextBlock
            {
                Text = $"Type: {node.NodeType}",
                Margin = new Thickness(0, 0, 0, 5)
            };
            PropertiesPanel.Children.Add(typeBlock);

            // Description
            var descBlock = new TextBlock
            {
                Text = node.Description,
                TextWrapping = TextWrapping.Wrap,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 0, 0, 10)
            };
            PropertiesPanel.Children.Add(descBlock);

            // Configuration UI
            var configUI = node.GetConfigurationUI();
            if (configUI != null)
            {
                var separator = new Border
                {
                    Height = 1,
                    Background = Brushes.Gray,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                PropertiesPanel.Children.Add(separator);
                
                var configLabel = new TextBlock
                {
                    Text = "Configuration:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                PropertiesPanel.Children.Add(configLabel);
                
                PropertiesPanel.Children.Add(configUI);
            }
        }

        private void ShowNodeContextMenu(WorkflowNode node, MouseButtonEventArgs e)
        {
            var contextMenu = new ContextMenu();
            
            var deleteItem = new MenuItem { Header = "Delete Node" };
            deleteItem.Click += (s, args) => DeleteNode(node);
            contextMenu.Items.Add(deleteItem);
            
            var duplicateItem = new MenuItem { Header = "Duplicate Node" };
            duplicateItem.Click += (s, args) => DuplicateNode(node);
            contextMenu.Items.Add(duplicateItem);
            
            contextMenu.IsOpen = true;
        }

        private void DeleteNode(WorkflowNode node)
        {
            _currentWorkflow.Nodes.Remove(node);
            
            if (_nodeVisuals.TryGetValue(node.Id, out var visual))
            {
                WorkflowCanvas.Children.Remove(visual);
                _nodeVisuals.Remove(node.Id);
            }
            
            if (_selectedNode == node)
            {
                _selectedNode = null;
                PropertiesPanel.Children.Clear();
                PropertiesPanel.Children.Add(new TextBlock 
                { 
                    Text = "Select a node to view properties",
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            
            UpdateUI();
            LogMessage($"Deleted node: {node.Title}");
        }

        private void DuplicateNode(WorkflowNode node)
        {
            // Create a copy of the node (simplified - in real implementation would use serialization)
            WorkflowNode newNode = node.NodeType switch
            {
                "Screenshot" => new ScreenshotNode(),
                "Calculator" => new CalculatorNode(),
                "SystemInfo" => new SystemInfoNode(),
                "Delay" => new DelayNode(),
                _ => null
            };

            if (newNode != null)
            {
                newNode.X = node.X + 20;
                newNode.Y = node.Y + 20;
                newNode.Title = node.Title + " Copy";
                
                // Copy parameters
                foreach (var param in node.Parameters)
                {
                    newNode.Parameters[param.Key] = param.Value;
                }
                
                _currentWorkflow.Nodes.Add(newNode);
                CreateNodeVisual(newNode);
                UpdateUI();
                
                LogMessage($"Duplicated node: {node.Title}");
            }
        }

        #endregion

        #region Canvas Events

        private void WorkflowCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clear selection when clicking on empty canvas
            if (_selectedNode != null)
            {
                _selectedNode.IsSelected = false;
                if (_nodeVisuals.TryGetValue(_selectedNode.Id, out var border))
                {
                    border.BorderBrush = Brushes.White;
                }
                _selectedNode = null;
                
                PropertiesPanel.Children.Clear();
                PropertiesPanel.Children.Add(new TextBlock 
                { 
                    Text = "Select a node to view properties",
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
        }

        #endregion

        #region Workflow Execution

        private void OnWorkflowProgress(WorkflowProgress progress)
        {
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Text = progress.Message;
                LogMessage($"[{progress.PercentComplete:F0}%] {progress.Message}");
                
                // Highlight currently executing node
                foreach (var kvp in _nodeVisuals)
                {
                    var border = kvp.Value;
                    if (kvp.Key == progress.CurrentNodeId)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00)); // Orange for executing
                    }
                    else
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC)); // Default blue
                    }
                }
            });
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            WorkflowNameLabel.Text = $"📋 {_currentWorkflow.Name}";
            NodeCountLabel.Text = $"Nodes: {_currentWorkflow.Nodes.Count}";
            ConnectionCountLabel.Text = $"Connections: {_currentWorkflow.Connections.Count}";
        }

        private void ClearCanvas()
        {
            WorkflowCanvas.Children.Clear();
            _nodeVisuals.Clear();
            _connectionVisuals.Clear();
            _selectedNode = null;
            
            PropertiesPanel.Children.Clear();
            PropertiesPanel.Children.Add(new TextBlock 
            { 
                Text = "Select a node to view properties",
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private void LoadWorkflowToCanvas()
        {
            ClearCanvas();
            
            foreach (var node in _currentWorkflow.Nodes)
            {
                CreateNodeVisual(node);
            }
            
            // TODO: Load connections when connection system is implemented
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                var logEntry = $"[{timestamp}] {message}\n";
                ExecutionLogTextBlock.Text += logEntry;
                
                // Auto-scroll to bottom
                LogScrollViewer.ScrollToEnd();
            });
        }

        private bool ConfirmUnsavedChanges()
        {
            // In a real implementation, check if workflow has unsaved changes
            return true;
        }

        #endregion
    }
}
