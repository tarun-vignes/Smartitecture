using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using AIPal.ViewModels;

namespace AIPal.UI
{
    /// <summary>
    /// Main window of the AIPal application.
    /// Implements a modern chat interface with Windows 11 design language.
    /// Features:
    /// - Dockable window (similar to Windows Copilot)
    /// - Chat message display
    /// - Text and voice input
    /// - Adaptive sizing and positioning
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Gets the view model that handles the chat interface logic
        /// </summary>
        public MainViewModel ViewModel { get; }

        /// <summary>
        /// Initializes the main window and sets up the initial layout.
        /// Configures window size, position, and gets the view model from DI.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Get the view model from dependency injection
            ViewModel = App.Current.Services.GetService<MainViewModel>();
            
            // Get the native window handle for Win32 interop
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            
            // Set default window size (400x700 is optimal for chat interface)
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 400, Height = 700 });
            
            // Position the window on the right side of the screen (Copilot-style)
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
            appWindow.Move(new Windows.Graphics.PointInt32 
            { 
                X = displayArea.WorkArea.Width - 420,  // 20px margin from screen edge
                Y = (displayArea.WorkArea.Height - 700) / 2  // Centered vertically
            });
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
