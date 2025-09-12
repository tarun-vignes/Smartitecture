using System;
using System.Windows;

namespace SimpleWpfTest
{
    public class SimpleApp : Application
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Console.WriteLine("Starting simple WPF application...");
                
                var app = new SimpleApp();
                var window = new Window
                {
                    Title = "Simple WPF Test - Smartitecture Debug",
                    Width = 400,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new System.Windows.Controls.TextBlock
                    {
                        Text = "If you can see this window, WPF is working!\n\nThis means the issue is with the complex Smartitecture application setup, not with WPF itself.",
                        Margin = new Thickness(20),
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14
                    }
                };
                
                Console.WriteLine("Showing window...");
                app.Run(window);
                Console.WriteLine("Application closed normally");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
