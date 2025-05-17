using AIPal.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace AIPal.UI
{
    /// <summary>
    /// Page for interacting with the AI agent.
    /// </summary>
    public sealed partial class AgentPage : Page
    {
        /// <summary>
        /// Gets the view model for this page.
        /// </summary>
        public AgentViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the AgentPage class.
        /// </summary>
        public AgentPage()
        {
            this.InitializeComponent();
            
            // Get the view model from the dependency injection container
            ViewModel = App.Current.Services.GetService(typeof(AgentViewModel)) as AgentViewModel;
            
            // Subscribe to messages collection changes to auto-scroll
            ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        }

        /// <summary>
        /// Handles the key down event for the input text box.
        /// </summary>
        private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Send message when Enter is pressed (without Shift)
            if (e.Key == Windows.System.VirtualKey.Enter && !e.KeyStatus.IsMenuKeyDown && !e.KeyStatus.IsShiftKeyDown)
            {
                if (ViewModel.SendMessageCommand.CanExecute(null))
                {
                    ViewModel.SendMessageCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles changes to the messages collection to auto-scroll to the bottom.
        /// </summary>
        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // Scroll to the bottom when a new message is added
                MessagesScrollViewer.UpdateLayout();
                MessagesScrollViewer.ChangeView(null, MessagesScrollViewer.ScrollableHeight, null, false);
            }
        }
    }
}
