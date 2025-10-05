using System;
using System.Threading.Tasks;
using System.Windows;

namespace Smartitecture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LaunchAppButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Testing Launch App Command...";
                
                // Create and test the launch app command
                var launchCommand = new Smartitecture.Core.Commands.LaunchAppCommand();
                var result = await launchCommand.ExecuteAsync(new[] { "notepad" });
                
                StatusText.Text = result ? "✅ Launch command executed successfully!" : "❌ Launch command failed";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Error: {ex.Message}";
            }
        }

        private async void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Testing Shutdown Command...";
                
                // Show confirmation dialog
                var result = MessageBox.Show(
                    "This will test the shutdown command (with 300 second delay). Continue?", 
                    "Confirm Shutdown Test", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var shutdownCommand = new Smartitecture.Core.Commands.ShutdownCommand();
                    var success = await shutdownCommand.ExecuteAsync(new[] { "300" }); // 5 minute delay
                    
                    StatusText.Text = success ? "✅ Shutdown command executed (5 min delay)!" : "❌ Shutdown command failed";
                }
                else
                {
                    StatusText.Text = "Shutdown test cancelled";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Error: {ex.Message}";
            }
        }
    }
}
