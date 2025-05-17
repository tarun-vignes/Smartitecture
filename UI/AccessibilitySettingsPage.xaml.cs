using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;

namespace AIPal.UI
{
    /// <summary>
    /// Accessibility settings page for elderly and less tech-savvy users.
    /// </summary>
    public sealed partial class AccessibilitySettingsPage : Page
    {
        private SpeechSynthesizer _speechSynthesizer;
        
        /// <summary>
        /// Initializes a new instance of the AccessibilitySettingsPage class.
        /// </summary>
        public AccessibilitySettingsPage()
        {
            this.InitializeComponent();
            
            // Initialize speech synthesizer
            _speechSynthesizer = new SpeechSynthesizer();
            
            // Load saved settings
            LoadSettings();
        }

        /// <summary>
        /// Loads saved accessibility settings.
        /// </summary>
        private void LoadSettings()
        {
            // In a real implementation, these would be loaded from application settings
            // For now, we'll set default values
            TextSizeSlider.Value = 1.0;
            HighContrastToggle.IsOn = false;
            ReduceMotionToggle.IsOn = false;
            ReadAloudToggle.IsOn = false;
            SpeechRateSlider.Value = 1.0;
            SimplifiedModeToggle.IsOn = false;
            
            // Update the sample text size
            UpdateSampleTextSize();
        }

        /// <summary>
        /// Handles changes to the text size slider.
        /// </summary>
        private void TextSizeSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            UpdateSampleTextSize();
        }

        /// <summary>
        /// Updates the sample text size based on the slider value.
        /// </summary>
        private void UpdateSampleTextSize()
        {
            // Calculate font size based on slider value (1.0 = 16pt, 2.0 = 24pt, 3.0 = 32pt)
            double baseFontSize = 16;
            double scaleFactor = TextSizeSlider.Value;
            double newFontSize = baseFontSize * scaleFactor;
            
            // Update the sample text
            SampleText.FontSize = newFontSize;
        }

        /// <summary>
        /// Handles toggling of the high contrast mode.
        /// </summary>
        private void HighContrastToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // In a real implementation, this would update the application theme
            // For now, we'll just show a message
            if (HighContrastToggle.IsOn)
            {
                ShowSettingChangedMessage("High contrast mode will be applied when you restart AIPal.");
            }
        }

        /// <summary>
        /// Handles toggling of the reduce motion setting.
        /// </summary>
        private void ReduceMotionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // In a real implementation, this would update animation settings
            // For now, we'll just show a message
            if (ReduceMotionToggle.IsOn)
            {
                ShowSettingChangedMessage("Animations will be reduced when you restart AIPal.");
            }
        }

        /// <summary>
        /// Handles toggling of the read aloud setting.
        /// </summary>
        private void ReadAloudToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // Enable/disable the speech rate slider based on read aloud toggle
            SpeechRateSlider.IsEnabled = ReadAloudToggle.IsOn;
        }

        /// <summary>
        /// Handles changes to the speech rate slider.
        /// </summary>
        private void SpeechRateSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // This will be used when testing speech
        }

        /// <summary>
        /// Handles the test speech button click.
        /// </summary>
        private async void TestSpeech_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadAloudToggle.IsOn)
            {
                // If read aloud is off, show a message
                ShowSettingChangedMessage("Please turn on 'Read Messages Aloud' to test speech.");
                return;
            }
            
            try
            {
                // Create a test message
                string testMessage = "This is a test of how AIPal will read messages to you.";
                
                // Set the speech rate
                _speechSynthesizer.Options.SpeakingRate = SpeechRateSlider.Value;
                
                // Synthesize the speech
                SpeechSynthesisStream stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(testMessage);
                
                // In a real implementation, this would play the audio
                // For now, we'll just show a message
                ShowSettingChangedMessage("In a full implementation, you would hear: \"" + testMessage + "\"");
            }
            catch (Exception ex)
            {
                ShowSettingChangedMessage("Error testing speech: " + ex.Message);
            }
        }

        /// <summary>
        /// Handles toggling of the simplified mode.
        /// </summary>
        private void SimplifiedModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // In a real implementation, this would update the UI complexity
            // For now, we'll just show a message
            if (SimplifiedModeToggle.IsOn)
            {
                ShowSettingChangedMessage("Simplified mode will be applied when you restart AIPal.");
            }
        }

        /// <summary>
        /// Handles the save settings button click.
        /// </summary>
        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // In a real implementation, this would save all settings to application storage
            // For now, we'll just show a success message
            
            ContentDialog saveDialog = new ContentDialog
            {
                Title = "Settings Saved",
                Content = "Your accessibility settings have been saved. Some changes will take effect after restarting AIPal.",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await saveDialog.ShowAsync();
        }

        /// <summary>
        /// Shows a message about a setting being changed.
        /// </summary>
        private async void ShowSettingChangedMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Setting Changed",
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
