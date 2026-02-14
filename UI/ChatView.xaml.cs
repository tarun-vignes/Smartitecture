using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Smartitecture.Services;
using Smartitecture.Services.Automation;
using Smartitecture.Services.Core;

namespace Smartitecture.UI
{
    public partial class ChatView : UserControl
    {
        private readonly ILLMService _llmService = null!;
        private readonly ChatHistoryService _historyService = new ChatHistoryService();
        private readonly ToolExecutionService _toolExecutor = new ToolExecutionService();
        private string _conversationId;
        private readonly DispatcherTimer _typingTimer;
        private bool _isProcessing = false;
        private bool _isReplayingHistory = false;
        private bool _hasUserMessages = false;
        private bool _previewingDeleted = false;
        private string? _pendingDeleteId;
        private DeleteConfirmMode _deleteConfirmMode = DeleteConfirmMode.Soft;
        private ToolCall? _pendingToolConfirmation;

        private enum DeleteConfirmMode
        {
            Soft,
            Permanent
        }

        public ChatView(ILLMService llmService)
        {
            InitializeComponent();
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _conversationId = Guid.NewGuid().ToString();

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
                _llmService.ModeSwitched += OnModeSwitched;
                UpdateModelStatus();
            }

            // Setup placeholder text behavior
            SetupPlaceholderText();
            PopulateModelSelector();
            PopulateModeSelector();

            Loaded += ChatView_Loaded;
            Unloaded += ChatView_Unloaded;
        }

        private void ChatView_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            MessageInput.Focus();
        }

        private void ChatView_Unloaded(object sender, RoutedEventArgs e)
        {
            _typingTimer?.Stop();
            if (_llmService != null)
            {
                _llmService.ModelSwitched -= OnModelSwitched;
                _llmService.ModeSwitched -= OnModeSwitched;
            }
        }

        private void GoDashboard_Click(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoDashboard();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Alt)
            {
                if (e.Key == System.Windows.Input.Key.Left)
                {
                    GoDashboard_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.H)
                {
                    GoHome_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.S)
                {
                    OpenSettings_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }

        private void SetupPlaceholderText()
        {
            var placeholderText = GetString("Chat.Placeholder", "Type your message or command here...");
            MessageInput.Text = placeholderText;
            MessageInput.Foreground = GetPlaceholderBrush();

            MessageInput.GotFocus += (s, e) =>
            {
                if (MessageInput.Text == placeholderText)
                {
                    MessageInput.Text = "";
                    MessageInput.Foreground = GetOnSurfaceBrush();
                }
            };

            MessageInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(MessageInput.Text))
                {
                    MessageInput.Text = placeholderText;
                    MessageInput.Foreground = GetPlaceholderBrush();
                }
            };
        }

        private Brush GetOnSurfaceBrush()
        {
            return TryFindResource("Brush.OnSurface") as Brush ?? Brushes.White;
        }

        private Brush GetPlaceholderBrush()
        {
            return TryFindResource("Brush.Placeholder") as Brush ?? SystemColors.GrayTextBrush;
        }

        private Brush GetChatUserForeground()
        {
            return TryFindResource("Brush.ChatBubbleUserForeground") as Brush ?? GetOnSurfaceBrush();
        }

        private Brush GetChatAssistantForeground()
        {
            return TryFindResource("Brush.ChatBubbleAssistantForeground") as Brush ?? GetOnSurfaceBrush();
        }

        private Brush GetChatSystemForeground()
        {
            return TryFindResource("Brush.ChatBubbleSystemForeground") as Brush ?? GetOnSurfaceBrush();
        }

        private void OnModelSwitched(object? sender, ModelSwitchedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateModelStatus());
        }

        private void OnModeSwitched(object? sender, ModeSwitchedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateModelStatus();
                SyncModeSelector();
            });
        }

        private void UpdateModelStatus()
        {
            if (_llmService != null)
            {
                var modeLabel = GetModeDisplayName(_llmService.CurrentMode);
                ModelStatusText.Text = $"({_llmService.CurrentModel} \u2022 {modeLabel})";
            }
        }

        private void PopulateModelSelector()
        {
            if (ModelSelector == null)
            {
                return;
            }

            ModelSelector.Items.Clear();
            ComboBoxItem? selected = null;

            foreach (var model in _llmService.AvailableModels)
            {
                var item = new ComboBoxItem { Content = model };
                ModelSelector.Items.Add(item);
                if (string.Equals(model, _llmService.CurrentModel, StringComparison.OrdinalIgnoreCase))
                {
                    selected = item;
                }
            }

            if (selected != null)
            {
                ModelSelector.SelectedItem = selected;
            }
            else if (ModelSelector.Items.Count > 0)
            {
                ModelSelector.SelectedIndex = 0;
            }

            UpdateModelStatus();
        }

        private void PopulateModeSelector()
        {
            if (ModeSelector == null)
            {
                return;
            }

            ModeSelector.Items.Clear();
            ComboBoxItem? selected = null;

            foreach (var mode in _llmService.AvailableModes)
            {
                var label = GetModeDisplayName(mode);
                var item = new ComboBoxItem { Content = label, Tag = mode };
                ModeSelector.Items.Add(item);
                if (mode == _llmService.CurrentMode)
                {
                    selected = item;
                }
            }

            if (selected != null)
            {
                ModeSelector.SelectedItem = selected;
            }
            else if (ModeSelector.Items.Count > 0)
            {
                ModeSelector.SelectedIndex = 0;
            }
        }

        private void SyncModeSelector()
        {
            if (ModeSelector == null)
            {
                return;
            }

            foreach (var item in ModeSelector.Items.OfType<ComboBoxItem>())
            {
                if (item.Tag is AIModeType mode && mode == _llmService.CurrentMode)
                {
                    ModeSelector.SelectedItem = item;
                    break;
                }
            }
        }

        private string GetModeDisplayName(AIModeType mode)
        {
            return mode switch
            {
                AIModeType.Fortis => GetString("Chat.ModeFortis", "FORTIS"),
                AIModeType.Nexa => GetString("Chat.ModeNexa", "NEXA"),
                _ => GetString("Chat.ModeLumen", "LUMEN")
            };
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
            MessageInput.Foreground = GetOnSurfaceBrush();
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

                if (!string.IsNullOrEmpty(commandName))
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
                AddMessageToChat($"X Error: {ex.Message}", "system");
            }
            finally
            {
                HideTypingIndicator();
                _isProcessing = false;
                SendButton.IsEnabled = true;
                MessageInput.Focus();
            }
        }

        private async Task ExecuteCommandAsync(string commandName, Dictionary<string, object>? parameters, string originalMessage)
        {
            try
            {
                AddMessageToChat($"Executing {commandName} command...", "system");

                var toolArgs = parameters?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
                var result = await _toolExecutor.ExecuteToolAsync(commandName, toolArgs);

                if (result.RequiresConfirmation)
                {
                    var argsJson = toolArgs == null ? "{}" : System.Text.Json.JsonSerializer.Serialize(toolArgs);
                    ShowToolConfirmation(new ToolCall { Name = commandName, ArgumentsJson = argsJson });
                    return;
                }

                if (result.Success)
                {
                    AddMessageToChat(result.Message, "system");

                    // Also get AI commentary on the action
                    var aiResponse = await _llmService.GetResponseAsync(
                        $"I just executed the {commandName} command for the user. Please provide a brief, helpful response about what was done.",
                        _conversationId);

                    AddMessageToChat(aiResponse, "assistant");
                }
                else
                {
                    AddMessageToChat(result.Message, "system");

                    // Get AI help for failed command
                    var aiResponse = await _llmService.GetResponseAsync(
                        $"The {commandName} command failed. Please help the user understand what might have gone wrong and suggest alternatives.",
                        _conversationId);

                    AddMessageToChat(aiResponse, "assistant");
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"Executing command: {ex.Message}", "system");
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

                HandleAssistantPostProcess(response);
            }
            catch (Exception ex)
            {
                AddMessageToChat($"X AI Error: {ex.Message}", "system");
            }
        }

        private Border? _currentStreamingMessage;
        private TextBlock? _currentStreamingTextBlock;

        private void OnTokenReceived(string token)
        {
            Dispatcher.Invoke(() =>
            {
                if (_currentStreamingMessage == null)
                {
                    // Create new streaming message
                    _currentStreamingTextBlock = new TextBlock
                    {
                        Foreground = GetChatAssistantForeground(),
                        FontSize = 14,
                        TextWrapping = TextWrapping.Wrap,
                        Text = token
                    };

                    _currentStreamingMessage = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.Assistant"),
                        Child = _currentStreamingTextBlock
                    };

                    ChatMessagesPanel.Children.Add(_currentStreamingMessage);
                }
                else
                {
                    // Append to existing message
                    if (_currentStreamingTextBlock != null)
                    {
                        _currentStreamingTextBlock.Text += token;
                    }
                }

                // Auto-scroll to bottom
                ChatScrollViewer.ScrollToEnd();
            });
        }

        private void HandleAssistantPostProcess(string response)
        {
            if (ToolConfirmationParser.TryExtractConfirmation(response, out var cleaned, out var call) && call != null)
            {
                if (_currentStreamingTextBlock != null)
                {
                    _currentStreamingTextBlock.Text = cleaned;
                }

                ShowToolConfirmation(call);
                ChatScrollViewer.ScrollToEnd();
            }
        }

        private void AddMessageToChat(string message, string role)
        {
            if (ChatMessagesPanel == null || ChatScrollViewer == null)
            {
                return;
            }

            if (!_isReplayingHistory)
            {
                if (role.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    _hasUserMessages = true;
                    _historyService.AppendMessage(_conversationId, role, message);
                }
                else if (_hasUserMessages)
                {
                    _historyService.AppendMessage(_conversationId, role, message);
                }
            }

            ChatMessagesPanel.Children.Add(CreateMessageBubble(message, role));

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

        private void TypingTimer_Tick(object? sender, EventArgs e)
        {
            var currentText = TypingDots.Text;
            TypingDots.Text = currentText.Length >= 6 ? "." : currentText + ".";
        }

        private async void ModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelSelector.SelectedItem == null)
                return;

            if (ModelSelector.SelectedItem is not ComboBoxItem selectedItem)
                return;

            var selectedModel = selectedItem.Content?.ToString();
            if (string.IsNullOrWhiteSpace(selectedModel))
                return;

            try
            {
                var success = await _llmService.SwitchModelAsync(selectedModel);
                if (!IsLoaded || ChatMessagesPanel == null)
                {
                    UpdateModelStatus();
                    return;
                }

                if (success)
                {
                    AddMessageToChat(Format("Chat.SwitchedFormat", "Switched to {0}", selectedModel), "system");
                    UpdateModelStatus();
                }
                else
                {
                    AddMessageToChat(Format("Chat.SwitchFailedFormat", "Failed to switch to {0}", selectedModel), "system");
                    PopulateModelSelector();
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"X Error switching model: {ex.Message}", "system");
            }
        }

        private async void ModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeSelector.SelectedItem is not ComboBoxItem selectedItem)
            {
                return;
            }

            if (selectedItem.Tag is not AIModeType mode)
            {
                return;
            }

            try
            {
                var success = await _llmService.SwitchModeAsync(mode);
                if (!IsLoaded || ChatMessagesPanel == null)
                {
                    UpdateModelStatus();
                    return;
                }

                if (success)
                {
                    AddMessageToChat(Format("Chat.ModeSwitchedFormat", "Mode set to {0}", GetModeDisplayName(mode)), "system");
                    UpdateModelStatus();
                }
                else
                {
                    AddMessageToChat(Format("Chat.ModeSwitchFailedFormat", "Failed to switch mode to {0}", GetModeDisplayName(mode)), "system");
                    SyncModeSelector();
                }
            }
            catch (Exception ex)
            {
                AddMessageToChat($"X Error switching mode: {ex.Message}", "system");
            }
        }

        private async void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            // Silent clear for cleaner UX
            ChatMessagesPanel.Children.Clear();

            // Add welcome message back
            var welcomeBorder = new Border
            {
                Style = (Style)FindResource("Chat.Bubble.System"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = GetString("Chat.Welcome", "Welcome to Smartitecture AI Assistant! I can help you with automation, system commands, and more."),
                    Style = (Style)FindResource("Text.Caption"),
                    Foreground = GetChatSystemForeground(),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            };
            ChatMessagesPanel.Children.Add(welcomeBorder);
            // Clear conversation history
            if (_llmService != null)
            {
                await _llmService.ClearConversationAsync(_conversationId);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHistoryList();
            LoadDeletedList();
            SetHistoryTab(false);
            ShowHistoryOverlay();
        }

        private void HistoryClose_Click(object sender, RoutedEventArgs e)
        {
            HideHistoryOverlay();
        }

        private void HistoryOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_previewingDeleted)
            {
                return;
            }

            if (HistoryList.SelectedItem is not HistorySessionItem selected)
            {
                return;
            }

            var session = _historyService.GetSession(selected.Id);
            if (session == null)
            {
                return;
            }

            _isReplayingHistory = true;
            ChatMessagesPanel.Children.Clear();
            foreach (var msg in session.Messages)
            {
                ChatMessagesPanel.Children.Add(CreateMessageBubble(msg.Content, msg.Role));
            }
            _isReplayingHistory = false;

            _conversationId = session.Id;
            _hasUserMessages = session.Messages.Any(m => m.Role.Equals("user", StringComparison.OrdinalIgnoreCase));
            HideHistoryOverlay();
            ChatScrollViewer.ScrollToEnd();
        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListContainer != null && HistoryListContainer.Visibility != Visibility.Visible)
            {
                return;
            }

            if (HistoryList.SelectedItem is not HistorySessionItem selected)
            {
                HistoryPreviewPanel.Children.Clear();
                return;
            }

            var session = _historyService.GetSession(selected.Id);
            _previewingDeleted = false;
            if (DeletedList != null)
            {
                DeletedList.SelectedItem = null;
            }
            if (HistoryOpenButton != null)
            {
                HistoryOpenButton.IsEnabled = true;
            }
            RenderHistoryPreview(session);
        }

        private void DeletedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeletedListContainer != null && DeletedListContainer.Visibility != Visibility.Visible)
            {
                return;
            }

            if (DeletedList.SelectedItem is not DeletedSessionItem selected)
            {
                if (_previewingDeleted)
                {
                    HistoryPreviewPanel.Children.Clear();
                }
                return;
            }

            _previewingDeleted = true;
            if (HistoryList != null)
            {
                HistoryList.SelectedItem = null;
            }
            if (HistoryOpenButton != null)
            {
                HistoryOpenButton.IsEnabled = false;
            }

            var session = _historyService.GetDeletedSession(selected.Id);
            RenderHistoryPreview(session);
        }

        private void HistoryTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryTab(false);
        }

        private void DeletedTabButton_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryTab(true);
        }

        private void SetHistoryTab(bool showDeleted)
        {
            _previewingDeleted = showDeleted;

            if (HistoryListContainer != null)
            {
                HistoryListContainer.Visibility = showDeleted ? Visibility.Collapsed : Visibility.Visible;
            }

            if (DeletedListContainer != null)
            {
                DeletedListContainer.Visibility = showDeleted ? Visibility.Visible : Visibility.Collapsed;
            }

            if (HistoryList != null)
            {
                HistoryList.SelectedItem = null;
            }

            if (DeletedList != null)
            {
                DeletedList.SelectedItem = null;
            }

            HistoryPreviewPanel.Children.Clear();

            if (HistoryOpenButton != null)
            {
                HistoryOpenButton.IsEnabled = !showDeleted;
            }

            var primary = TryFindResource("Button.Primary") as Style;
            var glass = TryFindResource("Button.Glass") as Style;
            if (HistoryTabButton != null && primary != null && glass != null)
            {
                HistoryTabButton.Style = showDeleted ? glass : primary;
            }
            if (DeletedTabButton != null && primary != null && glass != null)
            {
                DeletedTabButton.Style = showDeleted ? primary : glass;
            }
        }

        private void RenderHistoryPreview(ChatHistorySession? session)
        {
            HistoryPreviewPanel.Children.Clear();
            if (session == null || session.Messages.Count == 0)
            {
                HistoryPreviewPanel.Children.Add(new TextBlock
                {
                    Text = GetString("Chat.HistoryEmpty", "No saved chats yet."),
                    Style = (Style)FindResource("Text.Subtle")
                });
                return;
            }

            foreach (var msg in session.Messages)
            {
                HistoryPreviewPanel.Children.Add(CreateMessageBubble(msg.Content, msg.Role));
            }
        }

        private void LoadHistoryList()
        {
            var sessions = _historyService.GetSessions();
            var items = sessions.Select(s => new HistorySessionItem
            {
                Id = s.Id,
                Title = string.IsNullOrWhiteSpace(s.Title) || s.Title == "New Chat"
                    ? GetString("Chat.HistoryUntitled", "New Chat")
                    : s.Title,
                Time = s.LastUpdated.ToString("g")
            }).ToList();

            HistoryList.ItemsSource = items;
            HistoryEmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            HistoryPreviewPanel.Children.Clear();
        }

        private void LoadDeletedList()
        {
            var sessions = _historyService.GetDeletedSessions();
            var items = sessions.Select(s => new DeletedSessionItem
            {
                Id = s.Id,
                Title = string.IsNullOrWhiteSpace(s.Title) || s.Title == "New Chat"
                    ? GetString("Chat.HistoryUntitled", "New Chat")
                    : s.Title,
                Time = (s.DeletedAt ?? s.LastUpdated).ToString("g")
            }).ToList();

            if (DeletedList != null)
            {
                DeletedList.ItemsSource = items;
            }

            if (DeletedEmptyText != null)
            {
                DeletedEmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ShowHistoryOverlay()
        {
            if (HistoryPanel == null)
            {
                return;
            }

            HistoryPanel.Opacity = 0;
            HistoryPanel.Visibility = Visibility.Visible;

            var panelTransform = HistoryPanel.RenderTransform as TranslateTransform ?? new TranslateTransform(40, 0);
            HistoryPanel.RenderTransform = panelTransform;

            var panelFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
            var panelSlide = new DoubleAnimation(panelTransform.X, 0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            HistoryPanel.BeginAnimation(OpacityProperty, panelFade);
            panelTransform.BeginAnimation(TranslateTransform.XProperty, panelSlide);
        }

        private void HideHistoryOverlay()
        {
            if (HistoryPanel == null)
            {
                return;
            }

            var panelTransform = HistoryPanel.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
            HistoryPanel.RenderTransform = panelTransform;

            var panelSlide = new DoubleAnimation(panelTransform.X, 40, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var panelFade = new DoubleAnimation(HistoryPanel.Opacity, 0, TimeSpan.FromMilliseconds(160));
            panelFade.Completed += (_, __) =>
            {
                HistoryPanel.Visibility = Visibility.Collapsed;
                HideDeleteConfirm();
                _previewingDeleted = false;
                if (HistoryOpenButton != null)
                {
                    HistoryOpenButton.IsEnabled = true;
                }
            };

            panelTransform.BeginAnimation(TranslateTransform.XProperty, panelSlide);
            HistoryPanel.BeginAnimation(OpacityProperty, panelFade);
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            ShowDeleteConfirm(DeleteConfirmMode.Soft, id);
        }

        private void RecoverDeleted_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            _historyService.Restore(id);
            LoadHistoryList();
            LoadDeletedList();

            if (_previewingDeleted)
            {
                _previewingDeleted = false;
                HistoryPreviewPanel.Children.Clear();
            }
        }

        private void DeleteForever_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            ShowDeleteConfirm(DeleteConfirmMode.Permanent, id);
        }

        private void DeleteCancel_Click(object sender, RoutedEventArgs e)
        {
            _pendingDeleteId = null;
            _deleteConfirmMode = DeleteConfirmMode.Soft;
            HideDeleteConfirm();
        }

        private void DeleteConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_pendingDeleteId))
            {
                HideDeleteConfirm();
                return;
            }

            if (_deleteConfirmMode == DeleteConfirmMode.Permanent)
            {
                _historyService.DeletePermanently(_pendingDeleteId);
            }
            else
            {
                _historyService.MarkDeleted(_pendingDeleteId);
            }

            if (_pendingDeleteId == _conversationId)
            {
                _conversationId = Guid.NewGuid().ToString();
                _hasUserMessages = false;
                ResetChatToWelcome();
            }

            _pendingDeleteId = null;
            _deleteConfirmMode = DeleteConfirmMode.Soft;
            LoadHistoryList();
            LoadDeletedList();
            HistoryPreviewPanel.Children.Clear();
            HideDeleteConfirm();
        }

        private void ShowDeleteConfirm(DeleteConfirmMode mode, string sessionId)
        {
            if (DeleteConfirmCard == null)
            {
                return;
            }

            _pendingDeleteId = sessionId;
            _deleteConfirmMode = mode;

            if (DeleteConfirmTitleText != null)
            {
                DeleteConfirmTitleText.Text = mode == DeleteConfirmMode.Permanent
                    ? GetString("Chat.DeleteForeverTitle", "Delete permanently?")
                    : GetString("Chat.DeleteTitle", "Delete this chat?");
            }

            if (DeleteConfirmWarningText != null)
            {
                DeleteConfirmWarningText.Text = mode == DeleteConfirmMode.Permanent
                    ? GetString("Chat.DeleteForeverWarning", "This permanently removes the chat and cannot be undone.")
                    : GetString("Chat.DeleteWarning", "Warning: you are about to delete this chat.");
            }

            if (DeleteConfirmRecoveryText != null)
            {
                if (mode == DeleteConfirmMode.Permanent)
                {
                    DeleteConfirmRecoveryText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DeleteConfirmRecoveryText.Visibility = Visibility.Visible;
                    DeleteConfirmRecoveryText.Text = GetString("Chat.DeleteRecovery", "Once you delete, you have 30 days to recover it before it is removed automatically.");
                }
            }

            DeleteConfirmCard.Visibility = Visibility.Visible;
            DeleteConfirmCard.Opacity = 0;
            var scale = DeleteConfirmCard.RenderTransform as ScaleTransform ?? new ScaleTransform(0.96, 0.96);
            DeleteConfirmCard.RenderTransform = scale;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(140));
            var scaleAnim = new DoubleAnimation(0.96, 1, TimeSpan.FromMilliseconds(160))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DeleteConfirmCard.BeginAnimation(OpacityProperty, fade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void HideDeleteConfirm()
        {
            if (DeleteConfirmCard == null || DeleteConfirmCard.Visibility != Visibility.Visible)
            {
                return;
            }

            var scale = DeleteConfirmCard.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
            DeleteConfirmCard.RenderTransform = scale;

            var fade = new DoubleAnimation(DeleteConfirmCard.Opacity, 0, TimeSpan.FromMilliseconds(120));
            var scaleAnim = new DoubleAnimation(scale.ScaleX, 0.96, TimeSpan.FromMilliseconds(120))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fade.Completed += (_, __) =>
            {
                DeleteConfirmCard.Visibility = Visibility.Collapsed;
            };

            DeleteConfirmCard.BeginAnimation(OpacityProperty, fade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void ShowToolConfirmation(ToolCall call)
        {
            _pendingToolConfirmation = call;
            if (ToolConfirmOverlay == null || ToolConfirmCard == null)
            {
                return;
            }

            if (ToolConfirmMessage != null)
            {
                ToolConfirmMessage.Text = Format(
                    "Chat.ToolConfirmMessage",
                    "You\u2019re about to run {0}. Continue?",
                    call.Name);
            }

            ToolConfirmOverlay.Visibility = Visibility.Visible;
            ToolConfirmOverlay.Opacity = 0;

            var scale = ToolConfirmCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(0.96, 0.96);
                ToolConfirmCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(160));
            var scaleIn = new DoubleAnimation(0.96, 1.0, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            ToolConfirmOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);
        }

        private void HideToolConfirmation()
        {
            if (ToolConfirmOverlay == null || ToolConfirmCard == null)
            {
                return;
            }

            var scale = ToolConfirmCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(1, 1);
                ToolConfirmCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(ToolConfirmOverlay.Opacity, 0, TimeSpan.FromMilliseconds(140));
            overlayFade.Completed += (_, __) => ToolConfirmOverlay.Visibility = Visibility.Collapsed;
            var scaleOut = new DoubleAnimation(scale.ScaleX, 0.96, TimeSpan.FromMilliseconds(140))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            ToolConfirmOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
        }

        private void ToolConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            _pendingToolConfirmation = null;
            HideToolConfirmation();
        }

        private async void ToolConfirmConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingToolConfirmation == null)
            {
                HideToolConfirmation();
                return;
            }

            var call = _pendingToolConfirmation;
            _pendingToolConfirmation = null;
            HideToolConfirmation();

            var result = await _toolExecutor.ExecuteToolAsync(call.Name, call.ArgumentsJson, true);
            AddMessageToChat(result.Message, "system");
        }

        private void ResetChatToWelcome()
        {
            _hasUserMessages = false;
            ChatMessagesPanel.Children.Clear();
            ChatMessagesPanel.Children.Add(new Border
            {
                Style = (Style)FindResource("Chat.Bubble.System"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new TextBlock
                {
                    Text = GetString("Chat.Welcome", "Welcome to Smartitecture AI Assistant! I can help you with automation, system commands, and more."),
                    Style = (Style)FindResource("Text.Caption"),
                    Foreground = GetChatSystemForeground(),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            });
            ChatScrollViewer.ScrollToEnd();
        }

        private Border CreateMessageBubble(string message, string role)
        {
            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = GetChatAssistantForeground(),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            Border messageBorder;
            switch (role.ToLower())
            {
                case "user":
                    messageBorder = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.User"),
                        Child = textBlock
                    };
                    textBlock.Foreground = GetChatUserForeground();
                    break;
                case "system":
                    messageBorder = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.System"),
                        Child = textBlock
                    };
                    textBlock.Foreground = GetChatSystemForeground();
                    textBlock.FontSize = 12;
                    break;
                default:
                    messageBorder = new Border
                    {
                        Style = (Style)FindResource("Chat.Bubble.Assistant"),
                        Child = textBlock
                    };
                    textBlock.Foreground = GetChatAssistantForeground();
                    break;
            }

            return messageBorder;
        }

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement file attachment functionality
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoHome();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoSettings();
        }

        private class HistorySessionItem
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Time { get; set; } = string.Empty;
        }

        private class DeletedSessionItem
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Time { get; set; } = string.Empty;
        }

        private static string GetString(string key, string fallback)
        {
            return Application.Current?.TryFindResource(key) as string ?? fallback;
        }

        private static string Format(string key, string fallback, params object[] args)
        {
            var template = GetString(key, fallback);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return string.Format(fallback, args);
            }
        }
    }
}
