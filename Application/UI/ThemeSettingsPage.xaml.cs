using Smartitecture.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace Smartitecture.UI
{
    /// <summary>
    /// Page for theme and appearance settings.
    /// </summary>
    public sealed partial class ThemeSettingsPage : Page
    {
        private readonly ThemeService _themeService;
        private ObservableCollection<AccentColorItem> _accentColors;
        private bool _isInitializing = true;
        
        /// <summary>
        /// Initializes a new instance of the ThemeSettingsPage class.
        /// </summary>
        public ThemeSettingsPage()
        {
            this.InitializeComponent();
            
            // Get the theme service from the app
            _themeService = App.Current.Services.GetService(typeof(ThemeService)) as ThemeService;
            
            // Initialize accent colors
            InitializeAccentColors();
            
            // Load current settings
            LoadSettings();
            
            _isInitializing = false;
        }

        /// <summary>
        /// Initializes the accent color collection.
        /// </summary>
        private void InitializeAccentColors()
        {
            _accentColors = new ObservableCollection<AccentColorItem>
            {
                new AccentColorItem { Name = "Blue", Color = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)) },
                new AccentColorItem { Name = "Purple", Color = new SolidColorBrush(Color.FromArgb(255, 119, 25, 170)) },
                new AccentColorItem { Name = "Pink", Color = new SolidColorBrush(Color.FromArgb(255, 227, 0, 140)) },
                new AccentColorItem { Name = "Red", Color = new SolidColorBrush(Color.FromArgb(255, 232, 17, 35)) },
                new AccentColorItem { Name = "Orange", Color = new SolidColorBrush(Color.FromArgb(255, 202, 80, 16)) },
                new AccentColorItem { Name = "Yellow", Color = new SolidColorBrush(Color.FromArgb(255, 255, 185, 0)) },
                new AccentColorItem { Name = "Green", Color = new SolidColorBrush(Color.FromArgb(255, 16, 137, 62)) },
                new AccentColorItem { Name = "Teal", Color = new SolidColorBrush(Color.FromArgb(255, 0, 178, 148)) },
                new AccentColorItem { Name = "Cyan", Color = new SolidColorBrush(Color.FromArgb(255, 27, 161, 226)) },
                new AccentColorItem { Name = "Gray", Color = new SolidColorBrush(Color.FromArgb(255, 96, 96, 96)) }
            };
            
            // Set the first item as selected by default
            _accentColors[0].IsSelected = true;
            
            // Set the accent colors as the GridView's item source
            AccentColorGridView.ItemsSource = _accentColors;
        }

        /// <summary>
        /// Loads the current theme settings.
        /// </summary>
        private void LoadSettings()
        {
            // Set the appropriate radio button based on the current theme
            switch (_themeService.CurrentTheme)
            {
                case AppTheme.Light:
                    LightThemeRadioButton.IsChecked = true;
                    ThemeToggleSwitch.IsOn = false;
                    break;
                    
                case AppTheme.Dark:
                    DarkThemeRadioButton.IsChecked = true;
                    ThemeToggleSwitch.IsOn = true;
                    break;
                    
                case AppTheme.System:
                default:
                    SystemThemeRadioButton.IsChecked = true;
                    
                    // Set the toggle switch based on the actual theme
                    var rootElement = (Application.Current as App)?.MainWindow?.Content as FrameworkElement;
                    ThemeToggleSwitch.IsOn = rootElement?.ActualTheme == ElementTheme.Dark;
                    break;
            }
            
            // In a real implementation, we would also load the accent color setting
            // For now, we'll just select the first color
            AccentColorGridView.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles selection changes in the theme radio buttons.
        /// </summary>
        private async void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;
                
            var selectedButton = ThemeRadioButtons.SelectedItem as RadioButton;
            if (selectedButton != null && selectedButton.Tag is string themeString)
            {
                if (Enum.TryParse<AppTheme>(themeString, out var theme))
                {
                    await _themeService.SetThemeAsync(theme);
                    
                    // Update the toggle switch to match the selected theme
                    if (theme == AppTheme.Dark)
                    {
                        ThemeToggleSwitch.IsOn = true;
                    }
                    else if (theme == AppTheme.Light)
                    {
                        ThemeToggleSwitch.IsOn = false;
                    }
                    else
                    {
                        // For system theme, set based on actual theme
                        var rootElement = (Application.Current as App)?.MainWindow?.Content as FrameworkElement;
                        ThemeToggleSwitch.IsOn = rootElement?.ActualTheme == ElementTheme.Dark;
                    }
                }
            }
        }

        /// <summary>
        /// Handles toggling of the theme toggle switch.
        /// </summary>
        private async void ThemeToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
                return;
                
            // Set the theme based on the toggle switch
            await _themeService.SetThemeAsync(ThemeToggleSwitch.IsOn ? AppTheme.Dark : AppTheme.Light);
            
            // Update the radio buttons to match
            if (ThemeToggleSwitch.IsOn)
            {
                DarkThemeRadioButton.IsChecked = true;
            }
            else
            {
                LightThemeRadioButton.IsChecked = true;
            }
        }

        /// <summary>
        /// Handles selection changes in the accent color grid view.
        /// </summary>
        private void AccentColorGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;
                
            // Update the selected state of the accent colors
            foreach (var item in _accentColors)
            {
                item.IsSelected = false;
            }
            
            if (AccentColorGridView.SelectedItem is AccentColorItem selectedItem)
            {
                selectedItem.IsSelected = true;
                
                // In a real implementation, we would save the selected accent color
                // For now, we'll just show a message
                // SaveAccentColor(selectedItem.Name);
            }
        }

        /// <summary>
        /// Handles the save settings button click.
        /// </summary>
        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // In a real implementation, we would save all settings here
            // For now, we'll just show a success message
            
            ContentDialog saveDialog = new ContentDialog
            {
                Title = "Settings Saved",
                Content = "Your appearance settings have been saved.",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            await saveDialog.ShowAsync();
        }
    }

    /// <summary>
    /// Represents an accent color item.
    /// </summary>
    public class AccentColorItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        
        /// <summary>
        /// Gets or sets the name of the accent color.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the accent color brush.
        /// </summary>
        public SolidColorBrush Color { get; set; }
        
        /// <summary>
        /// Gets or sets whether this accent color is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }
        
        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
