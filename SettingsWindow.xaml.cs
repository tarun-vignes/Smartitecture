using System.Windows;

namespace SmartitectureSimple
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Placeholder: hook up to UI theme settings if needed
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder: persist settings via ConfigurationService
            this.Close();
        }
    }
}

