using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Smartitecture.Core.Services;

namespace Smartitecture.WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PythonApiService _pythonApiService;
        private readonly ILogger<MainViewModel> _logger;
        
        private string _inputText = "";
        private string _outputText = "";
        private bool _isProcessing = false;
        private string _statusMessage = "Ready";

        public MainViewModel(PythonApiService pythonApiService, ILogger<MainViewModel> logger)
        {
            _pythonApiService = pythonApiService;
            _logger = logger;
            
            ProcessCommand = new RelayCommand(async () => await ProcessTextAsync(), () => !IsProcessing && !string.IsNullOrWhiteSpace(InputText));
            ClearCommand = new RelayCommand(ClearText);
            
            InitializeAsync();
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessCommand).RaiseCanExecuteChanged();
            }
        }

        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged();
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged();
                ((RelayCommand)ProcessCommand).RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand ProcessCommand { get; }
        public ICommand ClearCommand { get; }

        private async void InitializeAsync()
        {
            try
            {
                StatusMessage = "Connecting to Python backend...";
                
                // Wait for Python backend to be ready
                await WaitForBackendAsync();
                
                StatusMessage = "Ready";
                _logger.LogInformation("MainViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error: Python backend not available";
                _logger.LogError(ex, "Failed to initialize MainViewModel");
            }
        }

        private async Task WaitForBackendAsync()
        {
            const int maxRetries = 30;
            for (int i = 0; i < maxRetries; i++)
            {
                if (await _pythonApiService.IsHealthyAsync())
                {
                    return;
                }
                await Task.Delay(1000);
            }
            throw new TimeoutException("Python backend failed to start");
        }

        private async Task ProcessTextAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText))
                return;

            try
            {
                IsProcessing = true;
                StatusMessage = "Processing...";
                
                _logger.LogInformation("Processing text: {InputText}", InputText);
                
                var result = await _pythonApiService.ProcessTextAsync(InputText);
                OutputText = result;
                
                StatusMessage = "Completed";
                _logger.LogInformation("Text processing completed successfully");
            }
            catch (Exception ex)
            {
                OutputText = $"Error: {ex.Message}";
                StatusMessage = "Error occurred";
                _logger.LogError(ex, "Failed to process text");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ClearText()
        {
            InputText = "";
            OutputText = "";
            StatusMessage = "Ready";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _executeAsync = () => { execute(); return Task.CompletedTask; };
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object parameter)
        {
            if (_executeAsync != null)
                await _executeAsync();
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
