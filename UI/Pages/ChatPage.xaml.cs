using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Smartitecture.Services;

namespace Smartitecture.UI.Pages
{
    public partial class ChatPage : Page
    {
        private readonly ILLMService _llmService;
        private readonly string _conversationId;
        private readonly DispatcherTimer _typingTimer;
        private bool _isProcessing;

        private Border? _currentStreamingBorder;
        private TextBlock? _currentStreamingText;

        private const string PlaceholderText = "Type your message here...";

        public ChatPage(ILLMService llmService)
        {
            InitializeComponent();

            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _conversationId = Guid.NewGuid().ToString();

            _typingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
            _typingTimer.Tick += TypingTimer_Tick;

            _llmService.ModelSwitched += OnModelSwitched;
            UpdateModelStatus();

            SetupPlaceholder();
            AddSystemMessage("Welcome to Smartitecture! Ask a question to get started.");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AppNavigator.Navigate(new DashboardPage());
        }

        private void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesPanel.Children.Clear();
            _currentStreamingBorder = null;
            _currentStreamingText = null;
            AddSystemMessage("Chat cleared.");
        }

        private void SetupPlaceholder()
        {
            MessageInput.Text = PlaceholderText;
            MessageInput.Foreground = GetAppBrush("Brush.PlaceholderText", Brushes.Gray);

            MessageInput.GotFocus += (_, _) =>
            {
                if (MessageInput.Text == PlaceholderText)
                {
                    MessageInput.Text = string.Empty;
                    MessageInput.Foreground = GetAppBrush("Brush.OnSurface", Brushes.White);
                }
            };

            MessageInput.LostFocus += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(MessageInput.Text))
                {
                    MessageInput.Text = PlaceholderText;
                    MessageInput.Foreground = GetAppBrush("Brush.PlaceholderText", Brushes.Gray);
                }
            };
        }

        private Brush GetAppBrush(string key, Brush fallback)
        {
            try
            {
                var value = Application.Current?.Resources?[key];
                return value as Brush ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private void OnModelSwitched(object sender, ModelSwitchedEventArgs e)
        {
            Dispatcher.Invoke(UpdateModelStatus);
        }

        private void UpdateModelStatus()
        {
            try
            {
                ModelStatusText.Text = string.IsNullOrWhiteSpace(_llmService.CurrentModel) ? string.Empty : $"({_llmService.CurrentModel})";
            }
            catch
            {
                ModelStatusText.Text = string.Empty;
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
            if (_isProcessing) return;
            var message = MessageInput.Text?.Trim();

            if (string.IsNullOrWhiteSpace(message) || message == PlaceholderText) return;

            _isProcessing = true;
            SendButton.IsEnabled = false;

            try
            {
                AddMessage(message, isUser: true);
                MessageInput.Text = string.Empty;
                MessageInput.Foreground = GetAppBrush("Brush.OnSurface", Brushes.White);

                ShowTypingIndicator(true);

                var response = await _llmService.GetStreamingResponseAsync(message, OnTokenReceived, _conversationId);
                FinalizeStreamingResponse(response);
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Error: {ex.Message}");
            }
            finally
            {
                ShowTypingIndicator(false);
                _isProcessing = false;
                SendButton.IsEnabled = true;
                MessageInput.Focus();
            }
        }

        private void OnTokenReceived(string token)
        {
            Dispatcher.Invoke(() =>
            {
                if (_currentStreamingBorder == null)
                {
                    _currentStreamingBorder = new Border { Style = (Style)FindResource("AssistantMessageStyle") };
                    _currentStreamingText = new TextBlock
                    {
                        Foreground = GetAppBrush("Brush.OnSurface", Brushes.White),
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 13
                    };
                    _currentStreamingBorder.Child = _currentStreamingText;
                    ChatMessagesPanel.Children.Add(_currentStreamingBorder);
                }

                _currentStreamingText!.Text += token;
                ScrollToBottom();
            });
        }

        private void FinalizeStreamingResponse(string fullResponse)
        {
            if (_currentStreamingText != null)
            {
                _currentStreamingText.Text = fullResponse;
            }

            _currentStreamingBorder = null;
            _currentStreamingText = null;
            ScrollToBottom();
        }

        private void AddMessage(string message, bool isUser)
        {
            var border = new Border { Style = (Style)FindResource(isUser ? "UserMessageStyle" : "AssistantMessageStyle") };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = isUser ? GetAppBrush("Brush.ButtonPrimaryForeground", Brushes.White) : GetAppBrush("Brush.OnSurface", Brushes.White),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13
            };

            border.Child = textBlock;
            ChatMessagesPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void AddSystemMessage(string message)
        {
            var border = new Border { Style = (Style)FindResource("SystemMessageStyle") };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = GetAppBrush("Brush.CaptionText", Brushes.Gray),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            border.Child = textBlock;
            ChatMessagesPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }

        private void ShowTypingIndicator(bool show)
        {
            TypingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            if (show)
            {
                TypingDots.Text = "...";
                _typingTimer.Start();
            }
            else
            {
                _typingTimer.Stop();
                TypingDots.Text = "...";
            }
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            TypingDots.Text = TypingDots.Text == "." ? ".." :
                              TypingDots.Text == ".." ? "..." :
                              ".";
        }
    }
}
