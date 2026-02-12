using System.Windows;

namespace Smartitecture
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Smartitecture.Services.NavigationService.Initialize(MainContent);
            Smartitecture.Services.NavigationService.GoHome();
        }
    }
}
