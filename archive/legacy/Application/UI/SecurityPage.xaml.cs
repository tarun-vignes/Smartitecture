using Smartitecture.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Smartitecture.UI
{
    /// <summary>
    /// Page for security and optimization features.
    /// </summary>
    public sealed partial class SecurityPage : Page
    {
        /// <summary>
        /// Gets the view model for this page.
        /// </summary>
        public SecurityViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the SecurityPage class.
        /// </summary>
        public SecurityPage()
        {
            this.InitializeComponent();
            
            // Get the view model from the main window
            ViewModel = ((MainWindow)App.Current.MainWindow).SecurityViewModel;
        }

        /// <summary>
        /// Handles the run security check button click event.
        /// </summary>
        private async void RunSecurityCheck_Click(object sender, RoutedEventArgs e)
        {
            bool detailed = DetailedCheckBox.IsChecked ?? false;
            await ViewModel.RunSecurityCheckAsync(detailed);
        }

        /// <summary>
        /// Handles the run optimization button click event.
        /// </summary>
        private async void RunOptimization_Click(object sender, RoutedEventArgs e)
        {
            bool aggressive = AggressiveOptimizationCheckBox.IsChecked ?? false;
            await ViewModel.RunOptimizationAsync(aggressive);
        }

        /// <summary>
        /// Handles the explain alert button click event.
        /// </summary>
        private async void ExplainAlert_Click(object sender, RoutedEventArgs e)
        {
            string alertText = AlertDescriptionTextBox.Text;
            if (!string.IsNullOrWhiteSpace(alertText))
            {
                await ViewModel.ExplainAlertAsync(alertText);
            }
        }
    }
}
