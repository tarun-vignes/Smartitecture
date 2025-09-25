using AIPal.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace AIPal.UI
{
    /// <summary>
    /// Page for the standard chat interface.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        /// <summary>
        /// Gets the view model for this page.
        /// </summary>
        public MainViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the ChatPage class.
        /// </summary>
        public ChatPage()
        {
            this.InitializeComponent();
            
            // Get the view model from the main window
            ViewModel = ((MainWindow)App.Current.MainWindow).ChatViewModel;
        }

        /// <summary>
        /// Handles the send button click event.
        /// Forwards the message to the view model for processing.
        /// </summary>
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SendMessageAsync();
        }

        /// <summary>
        /// Handles the voice input button click event.
        /// Initiates voice recognition for hands-free input.
        /// </summary>
        private async void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.StartVoiceInputAsync();
        }

        /// <summary>
        /// Handles keyboard input in the message box.
        /// Sends the message when Enter is pressed (without modifier keys).
        /// </summary>
        private async void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown)
            {
                e.Handled = true;
                await ViewModel.SendMessageAsync();
            }
        }
    }
}
