using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using AIPal.ViewModels;
using System;

namespace AIPal.UI
{
    /// <summary>
    /// Main window of the AIPal application.
    /// Implements a modern interface with Windows 11 design language.
    /// Features:
    /// - Navigation between chat and agent interfaces
    /// - Dockable window (similar to Windows Copilot)
    /// - Text and voice input
    /// - Adaptive sizing and positioning
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Gets the view model that handles the chat interface logic
        /// </summary>
        public MainViewModel ChatViewModel { get; }
        
        /// <summary>
        /// Gets the view model that handles the agent interface logic
        /// </summary>
        public AgentViewModel AgentViewModel { get; }

        /// <summary>
        /// Initializes the main window and sets up the initial layout.
        /// Configures window size, position, and gets the view models from DI.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Get the view models from dependency injection
            ChatViewModel = App.Current.Services.GetService(typeof(MainViewModel)) as MainViewModel;
            AgentViewModel = App.Current.Services.GetService(typeof(AgentViewModel)) as AgentViewModel;
            
            // Get the native window handle for Win32 interop
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            
            // Set default window size (500x700 is optimal for our interface)
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 500, Height = 700 });
            
            // Position the window on the right side of the screen (Copilot-style)
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
            appWindow.Move(new Windows.Graphics.PointInt32 
            { 
                X = displayArea.WorkArea.Width - 520,  // 20px margin from screen edge
                Y = (displayArea.WorkArea.Height - 700) / 2  // Centered vertically
            });
            
            // Navigate to the default page (Chat)
            ContentFrame.Navigate(typeof(ChatPage));
        }

        /// <summary>
        /// Handles navigation view selection changes to switch between pages.
        /// </summary>
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                
                switch (navItemTag)
                {
                    case "ChatPage":
                        ContentFrame.Navigate(typeof(ChatPage));
                        break;
                        
                    case "AgentPage":
                        ContentFrame.Navigate(typeof(AgentPage));
                        break;
                }
            }
        }
    }
}
