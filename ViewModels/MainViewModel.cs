using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPal.Services;

namespace AIPal.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ILLMService _llmService;
        private readonly CommandMapper _commandMapper;

        [ObservableProperty]
        private string inputText;

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public MainViewModel(ILLMService llmService, CommandMapper commandMapper)
        {
            _llmService = llmService;
            _commandMapper = commandMapper;
        }

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
