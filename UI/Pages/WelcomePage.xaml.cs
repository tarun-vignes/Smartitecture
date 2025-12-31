using System.Windows.Controls;
using Smartitecture.Services;

namespace Smartitecture.UI.Pages
{
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void LetsGo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppNavigator.Navigate(new StartupPage());
        }
    }
}

