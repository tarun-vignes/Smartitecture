using System;
using System.Windows;

namespace SimpleWpfTest
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Console.WriteLine("Starting simple WPF test...");
                
                var app = new Application();
                var window = new Window
                {
                    Title = "Simple WPF Test",
                    Width = 400,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                Console.WriteLine("Window created, showing...");
                app.Run(window);
                Console.WriteLine("Application exited normally");
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
