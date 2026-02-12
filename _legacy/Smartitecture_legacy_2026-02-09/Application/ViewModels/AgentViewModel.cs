using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Smartitecture.API.Models;
using Smartitecture.Services;
using Microsoft.UI.Dispatching;

namespace Smartitecture.ViewModels
{
    /// <summary>
    /// ViewModel for the agent interaction UI.
    /// </summary>
    public class AgentViewModel : ViewModelBase
    {
        private readonly IAgentService _agentService;
        private readonly DispatcherQueue _dispatcherQueue;
        private string _userInput;
        private bool _isProcessing;
        private string _contextId;

        /// <summary>
        /// Initializes a new instance of the AgentViewModel class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        public AgentViewModel(IAgentService agentService)
        {
            _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _contextId = Guid.NewGuid().ToString();
            
            Messages = new ObservableCollection<ChatMessageViewModel>();
            SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), CanSendMessage);
            ClearConversationCommand = new RelayCommand(ClearConversation);
        }

        /// <summary>
        /// Gets the collection of messages in the conversation.
        /// </summary>
        public ObservableCollection<ChatMessageViewModel> Messages { get; }

        /// <summary>
        /// Gets or sets the user input text.
        /// </summary>
        public string UserInput
        {
            get => _userInput;
            set
            {
                if (SetProperty(ref _userInput, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a request is being processed.
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            private set => SetProperty(ref _isProcessing, value);
        }

        /// <summary>
        /// Gets the command for sending a message.
        /// </summary>
        public ICommand SendMessageCommand { get; }

        /// <summary>
        /// Gets the command for clearing the conversation.
        /// </summary>
        public ICommand ClearConversationCommand { get; }

        /// <summary>
        /// Determines whether a message can be sent.
        /// </summary>
        /// <returns>True if a message can be sent, false otherwise</returns>
        private bool CanSendMessage()
        {
            return !IsProcessing && !string.IsNullOrWhiteSpace(UserInput);
        }

        /// <summary>
        /// Sends a message to the agent and processes the response.
        /// </summary>
        private async Task SendMessageAsync()
        {
            if (IsProcessing || string.IsNullOrWhiteSpace(UserInput))
            {
                return;
            }

            try
            {
                IsProcessing = true;

                // Add user message to the conversation
                var userMessage = new ChatMessageViewModel
                {
                    Content = UserInput,
                    IsUser = true,
                    Timestamp = DateTime.Now
                };

                _dispatcherQueue.TryEnqueue(() => Messages.Add(userMessage));

                // Clear input
                string input = UserInput;
                UserInput = string.Empty;

                // Process the request through the agent
                var response = await _agentService.ProcessRequestAsync(input, _contextId);

                // Add agent message to the conversation
                var agentMessage = new ChatMessageViewModel
                {
                    Content = response.Response,
                    IsUser = false,
                    Timestamp = DateTime.Now,
                    Actions = new ObservableCollection<AgentAction>(response.Actions)
                };

                _dispatcherQueue.TryEnqueue(() => Messages.Add(agentMessage));
            }
            catch (Exception ex)
            {
                // Add error message to the conversation
                var errorMessage = new ChatMessageViewModel
                {
                    Content = $"Error: {ex.Message}",
                    IsUser = false,
                    IsError = true,
                    Timestamp = DateTime.Now
                };

                _dispatcherQueue.TryEnqueue(() => Messages.Add(errorMessage));
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Clears the conversation history.
        /// </summary>
        private void ClearConversation()
        {
            Messages.Clear();
            _contextId = Guid.NewGuid().ToString();
        }
    }
}
