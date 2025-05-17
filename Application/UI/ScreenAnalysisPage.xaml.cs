using AIPal.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace AIPal.UI
{
    /// <summary>
    /// Page for screen capture and analysis features.
    /// </summary>
    public sealed partial class ScreenAnalysisPage : Page
    {
        /// <summary>
        /// Gets the view model for this page.
        /// </summary>
        public ScreenAnalysisViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the ScreenAnalysisPage class.
        /// </summary>
        public ScreenAnalysisPage()
        {
            this.InitializeComponent();
            
            // Get the view model from the main window
            ViewModel = ((MainWindow)App.Current.MainWindow).ScreenAnalysisViewModel;
        }

        /// <summary>
        /// Handles the capture button click event.
        /// </summary>
        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected analysis type
            var selectedItem = AnalysisTypeComboBox.SelectedItem as ComboBoxItem;
            string analysisType = selectedItem?.Tag?.ToString() ?? "general";
            
            // Get the capture type
            bool captureFullScreen = FullScreenRadioButton.IsChecked ?? true;
            
            // Run the screen analysis
            await ViewModel.AnalyzeScreenAsync(captureFullScreen, analysisType);
        }
    }
}
