using System.Windows;

namespace SmartitectureSimple
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Simple startup - no complex dependency injection
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
