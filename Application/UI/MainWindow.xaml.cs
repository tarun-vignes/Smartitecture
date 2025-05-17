using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media;
using AIPal.ViewModels;
using AIPal.Services;
using System;
using System.Threading.Tasks;

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
        /// Gets the view model that handles the security features
        /// </summary>
        public SecurityViewModel SecurityViewModel { get; }
        
        /// <summary>
        /// Gets the view model that handles the screen analysis features
        /// </summary>
        public ScreenAnalysisViewModel ScreenAnalysisViewModel { get; }
        
        /// <summary>
        /// Gets the theme service for managing application theme
        /// </summary>
        private ThemeService ThemeService { get; }

        /// <summary>
        /// Initializes the main window and sets up the initial layout.
        /// Configures window size, position, and gets the view models from DI.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Get the view models and services from dependency injection
            ChatViewModel = App.Current.Services.GetService(typeof(MainViewModel)) as MainViewModel;
            AgentViewModel = App.Current.Services.GetService(typeof(AgentViewModel)) as AgentViewModel;
            SecurityViewModel = App.Current.Services.GetService(typeof(SecurityViewModel)) as SecurityViewModel;
            ScreenAnalysisViewModel = App.Current.Services.GetService(typeof(ScreenAnalysisViewModel)) as ScreenAnalysisViewModel;
            ThemeService = App.Current.Services.GetService(typeof(ThemeService)) as ThemeService;
            
            // Set up theme toggle button
            InitializeThemeToggle();
            
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
            
            // Navigate to the default page (Getting Started for elderly users)
            ContentFrame.Navigate(typeof(GettingStartedPage));
        }

        /// <summary>
        /// Handles navigation view selection changes to switch between pages.
        /// </summary>
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavigateToPage(navItemTag);
            }
        }
        
        /// <summary>
        /// Navigates to the specified page by tag name.
        /// </summary>
        /// <param name="pageName">The tag name of the page to navigate to</param>
        public void NavigateToPage(string pageName)
        {
            switch (pageName)
            {
                case "GettingStartedPage":
                    ContentFrame.Navigate(typeof(GettingStartedPage));
                    break;
                    
                case "ChatPage":
                    ContentFrame.Navigate(typeof(ChatPage));
                    break;
                    
                case "AgentPage":
                    ContentFrame.Navigate(typeof(AgentPage));
                    break;
                    
                case "SecurityPage":
                    ContentFrame.Navigate(typeof(SecurityPage));
                    break;
                    
                case "ScreenAnalysisPage":
                    ContentFrame.Navigate(typeof(ScreenAnalysisPage));
                    break;
                    
                case "ThemeSettingsPage":
                    ContentFrame.Navigate(typeof(ThemeSettingsPage));
                    break;
                    
                case "LanguageSettingsPage":
                    ContentFrame.Navigate(typeof(LanguageSettingsPage));
                    break;
                    
                case "AccessibilitySettingsPage":
                    ContentFrame.Navigate(typeof(AccessibilitySettingsPage));
                    break;
                    
                default:
                    ContentFrame.Navigate(typeof(GettingStartedPage));
                    break;
            }
        }
        /// <summary>
        /// Initializes the theme toggle button based on the current theme.
        /// </summary>
        private void InitializeThemeToggle()
        {
            // Set up the theme toggle button
            ThemeService.ThemeChanged += ThemeService_ThemeChanged;
            
            // Set the initial state of the toggle button
            UpdateThemeToggleState();
        }
        
        /// <summary>
        /// Updates the theme toggle button state based on the current theme.
        /// </summary>
        private void UpdateThemeToggleState()
        {
            // Get the current theme
            var currentTheme = ThemeService.CurrentTheme;
            
            // Update the toggle button state
            if (currentTheme == AppTheme.Dark)
            {
                ThemeToggleButton.IsChecked = true;
                ThemeIcon.Glyph = "\uE706"; // Sun icon
            }
            else
            {
                ThemeToggleButton.IsChecked = false;
                ThemeIcon.Glyph = "\uE793"; // Moon icon
            }
        }
        
        /// <summary>
        /// Handles theme changes from the theme service.
        /// </summary>
        private void ThemeService_ThemeChanged(object sender, AppTheme e)
        {
            // Update the toggle button state
            UpdateThemeToggleState();
        }
        
        /// <summary>
        /// Handles the theme toggle button click.
        /// </summary>
        private async void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the theme
            await ThemeService.ToggleThemeAsync();
            
            // Update the toggle button state
            UpdateThemeToggleState();
        }
    }
}
