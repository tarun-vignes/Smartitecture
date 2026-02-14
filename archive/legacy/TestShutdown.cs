using System;
using System.Windows;
using Smartitecture.Core.Commands;

class TestShutdown
{
    [STAThread]
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Testing ShutdownCommand with 30-second delay...");
            
            // Test with 30 seconds delay
            var shutdown = new ShutdownCommand();
            bool result = await shutdown.ExecuteAsync(new[] { "30" });
            
            if (result)
            {
                Console.WriteLine("Shutdown scheduled successfully!");
                Console.WriteLine("You can cancel it by running 'shutdown /a' in Command Prompt");
            }
            else
            {
                Console.WriteLine("Shutdown was cancelled by user.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
