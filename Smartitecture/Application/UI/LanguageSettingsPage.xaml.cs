using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using AIPal.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AIPal.UI
{
    /// <summary>
    /// Page for selecting the application language.
    /// Designed to be accessible for elderly users with:
    /// - Large, clear text
    /// - Native language names prominently displayed
    /// - Simple, straightforward interface
    /// </summary>
    public sealed partial class LanguageSettingsPage : Page
    {
        private LanguageService _languageService;
        
        /// <summary>
        /// Initializes a new instance of the LanguageSettingsPage class.
        /// </summary>
        public LanguageSettingsPage()
        {
            this.InitializeComponent();
            
            // Get the language service from dependency injection
            _languageService = App.Current.Services.GetService(typeof(LanguageService)) as LanguageService;
            
            // Load the supported languages
            LoadLanguages();
        }
        
        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Select the current language in the list
            SelectCurrentLanguage();
        }
        
        /// <summary>
        /// Loads the supported languages into the list.
        /// </summary>
        private void LoadLanguages()
        {
            // Sort languages alphabetically by native name for easier browsing
            var sortedLanguages = _languageService.SupportedLanguages
                .OrderBy(l => l.NativeName)
                .ToList();
            
            // Set the languages as the data source for the list
            LanguagesList.ItemsSource = sortedLanguages;
        }
        
        /// <summary>
        /// Selects the current language in the list.
        /// </summary>
        private void SelectCurrentLanguage()
        {
            // Get the current language
            var currentLanguage = _languageService.CurrentLanguageInfo;
            
            // Find the index of the current language in the list
            var index = LanguagesList.Items.Cast<LanguageInfo>()
                .ToList()
                .FindIndex(l => l.Code == currentLanguage.Code);
            
            if (index >= 0)
            {
                // Select the current language in the list
                LanguagesList.SelectedIndex = index;
                
                // Scroll to the selected item
                LanguagesList.ScrollIntoView(LanguagesList.Items[index]);
            }
        }
        
        /// <summary>
        /// Handles the selection changed event for the languages list.
        /// </summary>
        private async void LanguagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguagesList.SelectedItem is LanguageInfo selectedLanguage)
            {
                // Check if the selected language is different from the current language
                if (selectedLanguage.Code != _languageService.CurrentLanguage)
                {
                    // Show the loading overlay
                    LoadingOverlay.Visibility = Visibility.Visible;
                    
                    try
                    {
                        // Set the selected language
                        await _languageService.SetLanguageAsync(selectedLanguage.Code);
                        
                        // Show a confirmation dialog
                        await ShowLanguageChangedDialog(selectedLanguage);
                    }
                    catch (Exception ex)
                    {
                        // Show an error dialog
                        await ShowErrorDialog(ex.Message);
                        
                        // Reselect the current language
                        SelectCurrentLanguage();
                    }
                    finally
                    {
                        // Hide the loading overlay
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        
        /// <summary>
        /// Shows a dialog confirming that the language has been changed.
        /// </summary>
        private async Task ShowLanguageChangedDialog(LanguageInfo language)
        {
            // Create a content dialog
            var dialog = new ContentDialog
            {
                Title = "Language Changed",
                Content = $"The language has been changed to {language.Name} ({language.NativeName}).\n\nSome changes may require restarting the application to take full effect.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            
            // Show the dialog
            await dialog.ShowAsync();
        }
        
        /// <summary>
        /// Shows an error dialog.
        /// </summary>
        private async Task ShowErrorDialog(string message)
        {
            // Create a content dialog
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = $"An error occurred while changing the language: {message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            
            // Show the dialog
            await dialog.ShowAsync();
        }
    }
}
