using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Smartitecture.UI
{
    /// <summary>
    /// Getting started page with simplified guidance for elderly and less tech-savvy users.
    /// </summary>
    public sealed partial class GettingStartedPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the GettingStartedPage class.
        /// </summary>
        public GettingStartedPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Navigates to the Chat page.
        /// </summary>
        private void GoToChat_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)App.Current.MainWindow;
            mainWindow.NavigateToPage("ChatPage");
        }

        /// <summary>
        /// Navigates to the Security page.
        /// </summary>
        private void GoToSecurity_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)App.Current.MainWindow;
            mainWindow.NavigateToPage("SecurityPage");
        }

        /// <summary>
        /// Navigates to the Screen Analysis page.
        /// </summary>
        private void GoToScreenAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)App.Current.MainWindow;
            mainWindow.NavigateToPage("ScreenAnalysisPage");
        }

        /// <summary>
        /// Navigates to the Agent page.
        /// </summary>
        private void GoToAgent_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)App.Current.MainWindow;
            mainWindow.NavigateToPage("AgentPage");
        }

        /// <summary>
        /// Handles clicks on common task buttons by navigating to the chat page with a predefined query.
        /// </summary>
        private void CommonTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag is string taskType)
            {
                var mainWindow = (MainWindow)App.Current.MainWindow;
                
                // Navigate to chat page
                mainWindow.NavigateToPage("ChatPage");
                
                // Set up a predefined query based on the task type
                string query = "";
                switch (taskType.ToLower())
                {
                    case "email":
                        query = "How do I send an email with an attachment?";
                        break;
                    case "printing":
                        query = "How do I print a document from my computer?";
                        break;
                    case "internet":
                        query = "How do I search for information on the internet safely?";
                        break;
                    case "photos":
                        query = "How do I view and organize photos on my computer?";
                        break;
                    case "files":
                        query = "How do I find and organize files on my computer?";
                        break;
                    case "video calls":
                        query = "How do I make a video call to my family?";
                        break;
                }
                
                // Send the query to the chat view model
                if (!string.IsNullOrEmpty(query))
                {
                    mainWindow.ChatViewModel.InputText = query;
                    // Note: We don't automatically send the message to allow the user to review it first
                }
            }
        }

        /// <summary>
        /// Handles the emergency help button click by navigating to the security page.
        /// </summary>
        private void EmergencyHelp_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to security page for emergency help
            var mainWindow = (MainWindow)App.Current.MainWindow;
            mainWindow.NavigateToPage("SecurityPage");
            
            // Show an emergency help dialog
            ShowEmergencyHelpDialog();
        }

        /// <summary>
        /// Shows an emergency help dialog with guidance for urgent computer issues.
        /// </summary>
        private async void ShowEmergencyHelpDialog()
        {
            ContentDialog emergencyDialog = new ContentDialog
            {
                Title = "Emergency Computer Help",
                Content = "If you think you've been scammed or your computer has a serious problem:\n\n" +
                          "1. Don't panic - most problems can be fixed\n\n" +
                          "2. If you shared financial information or passwords with someone, contact your bank immediately\n\n" +
                          "3. If you allowed remote access to your computer, disconnect from the internet by turning off your Wi-Fi or unplugging your internet cable\n\n" +
                          "4. Use the Security Check feature to scan for problems\n\n" +
                          "5. Consider getting help from a trusted family member or local computer repair shop",
                CloseButtonText = "I Understand",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await emergencyDialog.ShowAsync();
        }
    }
}
