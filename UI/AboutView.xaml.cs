using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Smartitecture.UI
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void OpenDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "ARCHITECTURE.md");
                if (File.Exists(local))
                {
                    Process.Start(new ProcessStartInfo { FileName = local, UseShellExecute = true });
                }
                else
                {
                    Process.Start(new ProcessStartInfo { FileName = "https://github.com/tarun-vignes/Smartitecture", UseShellExecute = true });
                }
            }
            catch { }
        }

        private void BackClicked(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoDashboard();
        }

        private void HomeClicked(object sender, RoutedEventArgs e)
        {
            Smartitecture.Services.NavigationService.GoHome();
        }
    }
}
