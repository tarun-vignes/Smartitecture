using System;
using System.Windows;
using System.Windows.Controls;

namespace Smartitecture.UI
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void HomeClicked(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoHome();
        }

        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoSettings();
        }

        private void OpenAbout_Click(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoAbout();
        }

        private void OpenChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Smartitecture.Services.NavigationService.GoChat();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
