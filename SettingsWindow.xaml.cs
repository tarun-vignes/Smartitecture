using System.Windows;
using Smartitecture.UI.Pages;

namespace SmartitectureSimple
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            SettingsFrame.Navigate(new SettingsPage());
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
