using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Smartitecture.Core.Commands;
using Smartitecture.Services;

namespace Smartitecture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILLMService _llmService;
        private readonly Dictionary<string, IAppCommand> _commands;
        private readonly string _conversationId;
        private readonly DispatcherTimer _typingTimer;
        private bool _isProcessing = false;
        private Border _currentStreamingMessage;
        private TextBlock _currentStreamingTextBlock;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize LLM service
            _llmService = new MultiModelAIService();
            _conversationId = Guid.NewGuid().ToString();
            
            // Initialize available commands
            _commands = new Dictionary<string, IAppCommand>
            {
                ["launch"] = new LaunchAppCommand(),
                ["shutdown"] = new ShutdownCommand(),
                ["calculator"] = new CalculatorCommand(),
                ["calc"] = new CalculatorCommand(),
                ["explorer"] = new ExplorerCommand(),
                ["files"] = new ExplorerCommand(),
                ["taskmanager"] = new TaskManagerCommand(),
                ["taskmgr"] = new TaskManagerCommand()
            };

            // Setup typing animation timer
            _typingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _typingTimer.Tick += TypingTimer_Tick;

            // Subscribe to model switching events
            if (_llmService != null)
            {
                _llmService.ModelSwitched += OnModelSwitched;
                UpdateModelStatus();
            }

            // Setup placeholder text behavior for chat input
            SetupPlaceholderText();
        }

        #region Navigation Methods

        private void OpenChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Opening AI Chat Assistant...";
                ShowChatView();
                StatusText.Text = "AI Chat Assistant ready";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Error opening chat: {ex.Message}";
                MessageBox.Show($"Error opening chat: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMainView();
            StatusText.Text = "Ready";
        }

        private void ShowChatView()
        {
            MainView.Visibility = Visibility.Collapsed;
            ChatView.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Visible;
            HeaderTitle.Text = "🤖 AI Chat Assistant";
            MessageInput.Focus();
        }

        private void ShowMainView()
        {
            ChatView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Collapsed;
            HeaderTitle.Text = "🏗️ Smartitecture - AI Desktop Assistant";
        }

        #endregion

        #region Original Command Methods

        private async void LaunchAppButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Testing Launch App Command...";
                
                // Create and test the launch app command
                var launchCommand = new Smartitecture.Core.Commands.LaunchAppCommand();
                var result = await launchCommand.ExecuteAsync(new[] { "notepad" });
                
                StatusText.Text = result ? "✅ Launch command executed successfully!" : "❌ Launch command failed";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Error: {ex.Message}";
            }
        }

        private async void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Testing Shutdown Command...";
                
                // Show confirmation dialog
                var result = MessageBox.Show(
                    "This will test the shutdown command (with 300 second delay). Continue?", 
                    "Confirm Shutdown Test", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var shutdownCommand = new Smartitecture.Core.Commands.ShutdownCommand();
                    var success = await shutdownCommand.ExecuteAsync(new[] { "300" }); // 5 minute delay
                    
                    StatusText.Text = success ? "✅ Shutdown command executed (5 min delay)!" : "❌ Shutdown command failed";
                }
                else
                {
                    StatusText.Text = "Shutdown test cancelled";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Error: {ex.Message}";
            }
        }

        #endregion

        #region Chat Functionality

        private void SetupPlaceholderText()
        {
            var placeholderText = "Type your message or command here...";
            MessageInput.Text = placeholderText;
            MessageInput.Foreground = Brushes.Gray;

            MessageInput.GotFocus += (s, e) =>
            {
                if (MessageInput.Text == placeholderText)
                {
                    MessageInput.Text = "";
                    MessageInput.Foreground = Brushes.White;
                }
            };

            MessageInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(MessageInput.Text))
                {
                    MessageInput.Text = placeholderText;
                    MessageInput.Foreground = Brushes.Gray;
                }
            };
        }

        private void OnModelSwitched(object sender, ModelSwitchedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateModelStatus());
        }

        private void UpdateModelStatus()
        {
            if (_llmService != null)
            {
                ModelStatusText.Text = $"({_llmService.CurrentModel})";
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            var placeholderText = "Type your message or command here...";
            if (_isProcessing || string.IsNullOrWhiteSpace(MessageInput.Text) || MessageInput.Text == placeholderText)
                return;

            var userMessage = MessageInput.Text.Trim();
            MessageInput.Text = string.Empty;
            MessageInput.Foreground = Brushes.White;
            _isProcessing = true;
            SendButton.IsEnabled = false;

            try
            {
                // Add user message to chat
                AddMessageToChat(userMessage, "user");

                // Show typing indicator
                ShowTypingIndicator();

                // Check if message contains a command
                var (commandName, parameters) = await _llmService.ParseCommandAsync(userMessage);
                
                if (!string.IsNullOrEmpty(commandName) && _commands.ContainsKey(commandName.ToLower()))
                {
                    // Execute command
                    await ExecuteCommandAsync(commandName.ToLower(), parameters, userMessage);
                }
                else
                {
                    // Get AI response
                    await GetAIResponseAsync(userMessage);
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"❌ Error: {ex.Message}", "system");
            }
            finally
            {
                HideTypingIndicator();
                _isProcessing = false;
                SendButton.IsEnabled = true;
                MessageInput.Focus();
            }
        }

        private async Task ExecuteCommandAsync(string commandName, Dictionary<string, object> parameters, string originalMessage)
        {
            try
            {
                var command = _commands[commandName];
                
                // Convert parameters to string array (simplified)
                var args = parameters?.Values?.Select(v => v?.ToString())?.ToArray() ?? new string[0];
                
                AddMessageToChat($"🔧 Executing {commandName} command...", "system");
                
                var success = await command.ExecuteAsync(args);
                
                if (success)
                {
                    AddMessageToChat($"✅ Command '{commandName}' executed successfully!", "system");
                    
                    // Also get AI commentary on the action
                    var aiResponse = await _llmService.GetResponseAsync(
                        $"I just executed the {commandName} command for the user. Please provide a brief, helpful response about what was done.",
                        _conversationId);
                    
                    AddMessageToChat(aiResponse, "assistant");
                }
                else
                {
                    AddMessageToChat($"❌ Command '{commandName}' failed to execute.", "system");
                    
                    // Get AI help for failed command
                    var aiResponse = await _llmService.GetResponseAsync(
                        $"The {commandName} command failed. Please help the user understand what might have gone wrong and suggest alternatives.",
                        _conversationId);
                    
                    AddMessageToChat(aiResponse, "assistant");
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"❌ Error executing command: {ex.Message}", "system");
            }
        }

        private async Task GetAIResponseAsync(string userMessage)
        {
            try
            {
                var response = await _llmService.GetStreamingResponseAsync(
                    userMessage,
                    OnTokenReceived,
                    _conversationId);
                
                // The streaming response will be built up via OnTokenReceived
                // Final response is already added, so we don't need to add it again
            }
            catch (Exception ex)
            {
                AddMessageToChat($"❌ AI Error: {ex.Message}", "system");
            }
        }

        private void OnTokenReceived(string token)
        {
            Dispatcher.Invoke(() =>
            {
                if (_currentStreamingMessage == null)
                {
                    // Create new streaming message
                    _currentStreamingTextBlock = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontSize = 14,
                        TextWrapping = TextWrapping.Wrap,
                        Text = token
                    };

                    _currentStreamingMessage = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        CornerRadius = new CornerRadius(15, 15, 15, 5),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(10, 5, 50, 5),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Child = _currentStreamingTextBlock
                    };

                    ChatMessagesPanel.Children.Add(_currentStreamingMessage);
                }
                else
                {
                    // Append to existing message
                    _currentStreamingTextBlock.Text += token;
                }

                // Auto-scroll to bottom
                ChatScrollViewer.ScrollToEnd();
            });
        }

        private void AddMessageToChat(string message, string role)
        {
            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            Border messageBorder;
            switch (role.ToLower())
            {
                case "user":
                    messageBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(53, 120, 212)),
                        CornerRadius = new CornerRadius(15, 15, 5, 15),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(50, 5, 10, 5),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Child = textBlock
                    };
                    break;
                case "system":
                    messageBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(8, 4, 8, 4),
                        Margin = new Thickness(20, 5, 20, 5),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Child = textBlock
                    };
                    textBlock.Foreground = Brushes.LightGray;
                    textBlock.FontSize = 12;
                    break;
                default: // assistant
                    messageBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                        CornerRadius = new CornerRadius(15, 15, 15, 5),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(10, 5, 50, 5),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Child = textBlock
                    };
                    break;
            }

            ChatMessagesPanel.Children.Add(messageBorder);
            
            // Reset streaming message reference if this is a complete message
            if (role != "assistant" || _currentStreamingMessage == null)
            {
                _currentStreamingMessage = null;
                _currentStreamingTextBlock = null;
            }

            // Auto-scroll to bottom
            ChatScrollViewer.ScrollToEnd();
        }

        private void ShowTypingIndicator()
        {
            TypingIndicator.Visibility = Visibility.Visible;
            _typingTimer.Start();
        }

        private void HideTypingIndicator()
        {
            TypingIndicator.Visibility = Visibility.Collapsed;
            _typingTimer.Stop();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            var currentText = TypingDots.Text;
            TypingDots.Text = currentText.Length >= 6 ? "." : currentText + ".";
        }

        private async void ModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_llmService == null || ModelSelector.SelectedItem == null)
                return;

            var selectedModel = ((ComboBoxItem)ModelSelector.SelectedItem).Content.ToString();
            
            try
            {
                var success = await _llmService.SwitchModelAsync(selectedModel);
                if (success)
                {
                    AddMessageToChat($"🔄 Switched to {selectedModel}", "system");
                    UpdateModelStatus();
                }
                else
                {
                    AddMessageToChat($"❌ Failed to switch to {selectedModel}", "system");
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"❌ Error switching model: {ex.Message}", "system");
            }
        }

        private async void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear the chat history?",
                "Clear Chat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear UI
                ChatMessagesPanel.Children.Clear();
                
                // Add welcome message back
                var welcomeBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(8, 4, 8, 4),
                    Margin = new Thickness(20, 5, 20, 5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = "🏗️ Welcome to Smartitecture AI Assistant! I can help you with automation, system commands, and more.",
                        Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };
                ChatMessagesPanel.Children.Add(welcomeBorder);

                // Clear conversation history
                if (_llmService != null)
                {
                    await _llmService.ClearConversationAsync(_conversationId);
                }
            }
        }

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement file attachment functionality
            MessageBox.Show("File attachment feature coming soon!", "Feature Preview", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _typingTimer?.Stop();
            if (_llmService != null)
            {
                _llmService.ModelSwitched -= OnModelSwitched;
            }
            base.OnClosed(e);
        }
    }
}
