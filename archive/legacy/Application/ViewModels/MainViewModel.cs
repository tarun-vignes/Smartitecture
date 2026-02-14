using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smartitecture.Services;

namespace Smartitecture.ViewModels
{
    /// <summary>
    /// View model for the main chat interface.
    /// Handles user input processing, message display, and command execution.
    /// Uses MVVM pattern with CommunityToolkit.Mvvm for property and command bindings.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        /// <summary>Service for natural language processing and command parsing</summary>
        private readonly ILLMService _llmService;

        /// <summary>Service for executing system commands</summary>
        private readonly CommandMapper _commandMapper;

        /// <summary>User's current input text in the chat box</summary>
        [ObservableProperty]
        private string inputText;

        /// <summary>Collection of chat messages displayed in the conversation</summary>
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        /// <summary>
        /// Initializes a new instance of the MainViewModel
        /// </summary>
        /// <param name="llmService">Service for natural language processing</param>
        /// <param name="commandMapper">Service for executing system commands</param>
        public MainViewModel(ILLMService llmService, CommandMapper commandMapper)
        {
            _llmService = llmService;
            _commandMapper = commandMapper;
        }

        /// <summary>
        /// Processes and sends the user's message.
        /// Attempts to parse the input as a command, executes it if possible,
        /// otherwise gets a conversational response from the LLM.
        /// </summary>
        [RelayCommand]
        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(InputText))
                return;

            var userMessage = new ChatMessage 
            { 
                Content = InputText,
                IsUser = true,
                Timestamp = DateTime.Now
            };
            Messages.Add(userMessage);

            var userInput = InputText;
            InputText = string.Empty;

            try
            {
                // Parse command
                var (commandName, parameters) = await _llmService.ParseCommandAsync(userInput);

                // If command is recognized, execute it
                if (commandName != "Unknown")
                {
                    var success = await _commandMapper.ExecuteCommandAsync(commandName, parameters);
                    if (success)
                    {
                        Messages.Add(new ChatMessage 
                        { 
                            Content = $"Executed command: {commandName}",
                            IsUser = false,
                            Timestamp = DateTime.Now
                        });
                        return;
                    }
                }

                // If no command or execution failed, get conversational response
                var response = await _llmService.GetResponseAsync(userInput);
                Messages.Add(new ChatMessage 
                { 
                    Content = response,
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage 
                { 
                    Content = $"Error: {ex.Message}",
                    IsUser = false,
                    Timestamp = DateTime.Now
                });
            }
        }

        public async Task StartVoiceInputAsync()
        {
            // TODO: Implement voice input
            Messages.Add(new ChatMessage 
            { 
                Content = "Voice input coming soon!",
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
    }

    public class ChatMessage
    {
        public string Content { get; set; }
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
        public string AvatarUri => IsUser ? "ms-appx:///Assets/UserAvatar.png" : "ms-appx:///Assets/BotAvatar.png";
    }
}
