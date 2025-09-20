using System.Windows;

namespace SmartitectureSimple
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Create and show the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
