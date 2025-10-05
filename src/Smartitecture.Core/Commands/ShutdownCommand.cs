using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;  // Added for MessageBox

namespace Smartitecture.Core.Commands
{
    public class ShutdownCommand : IAppCommand
    {
        public string CommandName => "shutdown";
        public string Description => "Schedules a system shutdown with optional delay (default: 5 minutes)";
        public bool RequiresElevation => true;

        public async Task<bool> ExecuteAsync(string[] args)
        {
            try
            {
                // Default delay of 5 minutes (300 seconds) if no time is provided
                int delaySeconds = 300; 

                if (args != null && args.Length > 0 && int.TryParse(args[0], out int userDelay) && userDelay > 0)
                {
                    delaySeconds = userDelay;
                }

                // Show confirmation dialog with the delay time
                var timeSpan = TimeSpan.FromSeconds(delaySeconds);
                var message = $"System will shut down in {timeSpan.Minutes} minutes and {timeSpan.Seconds} seconds.\n\n" +
                             "You can cancel this by typing: shutdown /a\n\n" +
                             "Do you want to continue?";

                var result = MessageBox.Show(
                    message,
                    "Confirm Shutdown",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Schedule the shutdown with the specified delay
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "shutdown",
                            Arguments = $"/s /t {delaySeconds} /c \"Smartitecture: System will shut down in {timeSpan.Minutes}m {timeSpan.Seconds}s. Type 'shutdown /a' to cancel.\"",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };
                    process.Start();
                    
                    // Show a confirmation message
                    MessageBox.Show(
                        $"Shutdown scheduled in {timeSpan.Minutes} minutes and {timeSpan.Seconds} seconds.\n\n" +
                        "To cancel, open Command Prompt and type: shutdown /a",
                        "Shutdown Scheduled",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error executing shutdown: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return false;
            }
        }
    }
}
