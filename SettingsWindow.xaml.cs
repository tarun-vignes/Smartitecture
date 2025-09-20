using System;
using System.Windows;
using System.Windows.Controls;

namespace SmartitectureSimple
{
    public partial class SettingsWindow : Window
    {
        public string SelectedTheme { get; private set; } = "Dark";
        public bool AutoStartBackend { get; private set; } = false;
        public int BackendPort { get; private set; } = 8001;
        public bool SettingsChanged { get; private set; } = false;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Set default selections
            ThemeComboBox.SelectedIndex = 0; // Dark theme by default
            AutoStartCheckBox.IsChecked = AutoStartBackend;
            PortTextBox.Text = BackendPort.ToString();
        }

        public void SetCurrentTheme(string theme)
        {
            SelectedTheme = theme;
            switch (theme)
            {
                case "Dark":
                    ThemeComboBox.SelectedIndex = 0;
                    break;
                case "Light":
                    ThemeComboBox.SelectedIndex = 1;
                    break;
                case "System":
                    ThemeComboBox.SelectedIndex = 2;
                    break;
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string theme)
            {
                SelectedTheme = theme;
                SettingsChanged = true;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate port number
                if (int.TryParse(PortTextBox.Text, out int port) && port > 0 && port < 65536)
                {
                    BackendPort = port;
                }
                else
                {
                    MessageBox.Show("Please enter a valid port number (1-65535).", "Invalid Port", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AutoStartBackend = AutoStartCheckBox.IsChecked ?? false;
                SettingsChanged = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SettingsChanged = false;
            DialogResult = false;
            Close();
        }
    }
}
