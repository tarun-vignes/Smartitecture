using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smartitecture.Core.Services;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smartitecture.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IPythonApiService _pythonApiService;

        [ObservableProperty]
        private string _userInput;

        [ObservableProperty]
        private string _agentResponse;

        public MainViewModel(IPythonApiService pythonApiService)
        {
            _pythonApiService = pythonApiService;
        }

        [RelayCommand]
        private async Task RunAgentAsync()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
            {
                AgentResponse = "Please enter a command.";
                return;
            }

            AgentResponse = "Agent is thinking...";

            try
            {
                var response = await _pythonApiService.RunAgentAsync(UserInput);
                AgentResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                AgentResponse = $"An error occurred: {ex.Message}";
            }
        }
    }
}
