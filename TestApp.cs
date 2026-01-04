using System;
using System.Windows;

namespace Smartitecture
{
    public class TestApp
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Console.WriteLine("Starting test application...");
                
                var app = new Application();
                var window = new Window
                {
                    Title = "Smartitecture Test",
                    Width = 400,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                Console.WriteLine("Window created, showing...");
                app.Run(window);
                
                Console.WriteLine("Application closed normally.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.ReadKey();
            }
        }
    }
}
