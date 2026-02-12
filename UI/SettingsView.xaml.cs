using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Threading;

namespace Smartitecture.UI
{
    public partial class SettingsView : UserControl
    {
        private bool _isInitializing = true;
        private bool _hasPendingChanges = false;
        private Action? _pendingExitAction;
        private Smartitecture.Services.UserPreferences? _initialPreferences;
        private readonly DispatcherTimer _toastTimer = new DispatcherTimer();
        private readonly DispatcherTimer _savePopupTimer = new DispatcherTimer();

        public SettingsView()
        {
            InitializeComponent();
            Loaded += SettingsView_Loaded;

            _toastTimer.Interval = TimeSpan.FromSeconds(2.2);
            _toastTimer.Tick += (_, __) =>
            {
                _toastTimer.Stop();
                HideToast();
            };

            _savePopupTimer.Interval = TimeSpan.FromSeconds(1.6);
            _savePopupTimer.Tick += (_, __) =>
            {
                _savePopupTimer.Stop();
                HideSavePopup();
            };
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var prefs = new Smartitecture.Services.PreferencesService();
                var loaded = prefs.Load();
                _initialPreferences = ClonePreferences(loaded);

                SelectComboByContent(ThemeComboBox, loaded.Theme);
                SelectComboByContent(LanguageComboBox, loaded.Language);
                SelectComboByContent(RegionComboBox, loaded.Region);
                SelectComboByContent(TimeFormatComboBox, loaded.TimeFormat);
                SelectComboByContent(UnitsComboBox, loaded.Units);
                SelectComboByContent(TextSizeComboBox, loaded.TextSize);
                SelectComboByContent(UpdateChannelComboBox, loaded.UpdateChannel);

                AutoStartCheckBox.IsChecked = loaded.StartWithWindows;
                StartMinimizedCheckBox.IsChecked = loaded.StartMinimized;
                NotificationsCheckBox.IsChecked = loaded.NotificationsEnabled;
                ReduceMotionCheckBox.IsChecked = loaded.ReduceMotion;
                AutoUpdateCheckBox.IsChecked = loaded.AutoUpdate;
                DiagnosticsCheckBox.IsChecked = loaded.ShareDiagnostics;

                PortTextBox.Text = loaded.ApiPort.ToString();
            }
            catch { }
            finally
            {
                _isInitializing = false;
                _hasPendingChanges = false;
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (_isInitializing) return;
                if (ThemeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
                {
                    var sel = (item.Tag?.ToString() ?? item.Content?.ToString() ?? "Dark").Trim();
                    ApplyThemeFromValue(sel);
                    MarkPendingChanges();
                }
            }
            catch { }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_isInitializing) return;
                var language = GetComboValue(LanguageComboBox, "en-US");
                Smartitecture.Services.LocalizationManager.Apply(language);
                MarkPendingChanges();
            }
            catch { }
        }

        private void SettingChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            MarkPendingChanges();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SavePreferences();
            ShowSavePopup(GetString("Settings.SavedMessage", "You have successfully changed your preferences."));
        }

        private void BackClicked(object sender, RoutedEventArgs e)
        {
            HandleExitRequest(() => Smartitecture.Services.NavigationService.GoDashboard());
        }

        private void HomeClicked(object sender, RoutedEventArgs e)
        {
            HandleExitRequest(() => Smartitecture.Services.NavigationService.GoHome());
        }

        private void UnsavedCancel_Click(object sender, RoutedEventArgs e)
        {
            _pendingExitAction = null;
            HideUnsavedPopup();
        }

        private void UnsavedConfirm_Click(object sender, RoutedEventArgs e)
        {
            RevertToInitialPreferences();
            _hasPendingChanges = false;
            HideUnsavedPopup();
            _pendingExitAction?.Invoke();
            _pendingExitAction = null;
        }

        private void HandleExitRequest(Action exitAction)
        {
            if (_hasPendingChanges)
            {
                _pendingExitAction = exitAction;
                ShowUnsavedPopup();
                return;
            }

            exitAction();
        }

        private void MarkPendingChanges()
        {
            _hasPendingChanges = true;
            ShowToast(GetString("Settings.ChangesPending", "Changes pending"));
        }

        private void SavePreferences()
        {
            try
            {
                var prefs = new Smartitecture.Services.PreferencesService();
                var loaded = prefs.Load();
                loaded.Theme = GetComboValue(ThemeComboBox, loaded.Theme);
                loaded.Language = GetComboValue(LanguageComboBox, loaded.Language);
                loaded.Region = GetComboValue(RegionComboBox, loaded.Region);
                loaded.TimeFormat = GetComboValue(TimeFormatComboBox, loaded.TimeFormat);
                loaded.Units = GetComboValue(UnitsComboBox, loaded.Units);
                loaded.TextSize = GetComboValue(TextSizeComboBox, loaded.TextSize);
                loaded.UpdateChannel = GetComboValue(UpdateChannelComboBox, loaded.UpdateChannel);

                loaded.StartWithWindows = AutoStartCheckBox.IsChecked == true;
                loaded.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
                loaded.NotificationsEnabled = NotificationsCheckBox.IsChecked == true;
                loaded.ReduceMotion = ReduceMotionCheckBox.IsChecked == true;
                loaded.AutoUpdate = AutoUpdateCheckBox.IsChecked == true;
                loaded.ShareDiagnostics = DiagnosticsCheckBox.IsChecked == true;

                if (int.TryParse(PortTextBox.Text, out var port) && port > 0 && port <= 65535)
                {
                    loaded.ApiPort = port;
                }
                prefs.Save(loaded);
                _initialPreferences = ClonePreferences(loaded);
            }
            catch { }

            _hasPendingChanges = false;
        }

        private void RevertToInitialPreferences()
        {
            if (_initialPreferences == null)
            {
                return;
            }

            _isInitializing = true;

            SelectComboByContent(ThemeComboBox, _initialPreferences.Theme);
            SelectComboByContent(LanguageComboBox, _initialPreferences.Language);
            SelectComboByContent(RegionComboBox, _initialPreferences.Region);
            SelectComboByContent(TimeFormatComboBox, _initialPreferences.TimeFormat);
            SelectComboByContent(UnitsComboBox, _initialPreferences.Units);
            SelectComboByContent(TextSizeComboBox, _initialPreferences.TextSize);
            SelectComboByContent(UpdateChannelComboBox, _initialPreferences.UpdateChannel);

            AutoStartCheckBox.IsChecked = _initialPreferences.StartWithWindows;
            StartMinimizedCheckBox.IsChecked = _initialPreferences.StartMinimized;
            NotificationsCheckBox.IsChecked = _initialPreferences.NotificationsEnabled;
            ReduceMotionCheckBox.IsChecked = _initialPreferences.ReduceMotion;
            AutoUpdateCheckBox.IsChecked = _initialPreferences.AutoUpdate;
            DiagnosticsCheckBox.IsChecked = _initialPreferences.ShareDiagnostics;
            PortTextBox.Text = _initialPreferences.ApiPort.ToString();

            _isInitializing = false;

            ApplyThemeFromValue(_initialPreferences.Theme);
            Smartitecture.Services.LocalizationManager.Apply(_initialPreferences.Language);
        }

        private static Smartitecture.Services.UserPreferences ClonePreferences(Smartitecture.Services.UserPreferences prefs)
        {
            return new Smartitecture.Services.UserPreferences
            {
                Theme = prefs.Theme,
                NotificationsEnabled = prefs.NotificationsEnabled,
                Language = prefs.Language,
                Region = prefs.Region,
                TimeFormat = prefs.TimeFormat,
                Units = prefs.Units,
                TextSize = prefs.TextSize,
                StartWithWindows = prefs.StartWithWindows,
                StartMinimized = prefs.StartMinimized,
                ReduceMotion = prefs.ReduceMotion,
                AutoUpdate = prefs.AutoUpdate,
                UpdateChannel = prefs.UpdateChannel,
                ShareDiagnostics = prefs.ShareDiagnostics,
                ApiPort = prefs.ApiPort
            };
        }

        private static void ApplyThemeFromValue(string? value)
        {
            var sel = (value ?? "Dark").Trim();
            var theme = sel.Equals("Light", StringComparison.OrdinalIgnoreCase)
                ? Smartitecture.Services.AppColorTheme.Light
                : sel.Equals("System", StringComparison.OrdinalIgnoreCase)
                    ? Smartitecture.Services.AppColorTheme.System
                    : Smartitecture.Services.AppColorTheme.Dark;
            Smartitecture.Services.ThemeManager.Apply(theme);
        }

        private void ShowToast(string message)
        {
            if (SettingsToastText == null || SettingsToast == null)
            {
                return;
            }

            SettingsToastText.Text = message;
            SettingsToast.Visibility = Visibility.Visible;

            var transform = SettingsToast.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform(0, 6);
                SettingsToast.RenderTransform = transform;
            }

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var slideIn = new DoubleAnimation(6, 0, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };

            SettingsToast.BeginAnimation(OpacityProperty, fadeIn);
            transform.BeginAnimation(TranslateTransform.YProperty, slideIn);

            _toastTimer.Stop();
            _toastTimer.Start();
        }

        private void HideToast()
        {
            if (SettingsToast == null)
            {
                return;
            }

            var transform = SettingsToast.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform(0, 0);
                SettingsToast.RenderTransform = transform;
            }

            var fadeOut = new DoubleAnimation(SettingsToast.Opacity, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            fadeOut.Completed += (_, __) => SettingsToast.Visibility = Visibility.Collapsed;
            var slideOut = new DoubleAnimation(0, 6, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };

            SettingsToast.BeginAnimation(OpacityProperty, fadeOut);
            transform.BeginAnimation(TranslateTransform.YProperty, slideOut);
        }

        private void ShowSavePopup(string message)
        {
            if (SavePopupOverlay == null || SavePopupCard == null || SavePopupText == null)
            {
                return;
            }

            SavePopupText.Text = message;
            SavePopupOverlay.Visibility = Visibility.Visible;

            var scale = SavePopupCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(0.96, 0.96);
                SavePopupCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var scaleIn = new DoubleAnimation(0.96, 1.0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };

            SavePopupOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);

            _savePopupTimer.Stop();
            _savePopupTimer.Start();
        }

        private void HideSavePopup()
        {
            if (SavePopupOverlay == null || SavePopupCard == null)
            {
                return;
            }

            var scale = SavePopupCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(1.0, 1.0);
                SavePopupCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(SavePopupOverlay.Opacity, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            overlayFade.Completed += (_, __) => SavePopupOverlay.Visibility = Visibility.Collapsed;
            var scaleOut = new DoubleAnimation(1.0, 0.96, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };

            SavePopupOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
        }

        private void ShowUnsavedPopup()
        {
            if (UnsavedPopupOverlay == null || UnsavedPopupCard == null)
            {
                return;
            }

            UnsavedPopupOverlay.Visibility = Visibility.Visible;

            var scale = UnsavedPopupCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(0.96, 0.96);
                UnsavedPopupCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var scaleIn = new DoubleAnimation(0.96, 1.0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };

            UnsavedPopupOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);
        }

        private void HideUnsavedPopup()
        {
            if (UnsavedPopupOverlay == null || UnsavedPopupCard == null)
            {
                return;
            }

            var scale = UnsavedPopupCard.RenderTransform as ScaleTransform;
            if (scale == null)
            {
                scale = new ScaleTransform(1.0, 1.0);
                UnsavedPopupCard.RenderTransform = scale;
            }

            var overlayFade = new DoubleAnimation(UnsavedPopupOverlay.Opacity, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            overlayFade.Completed += (_, __) => UnsavedPopupOverlay.Visibility = Visibility.Collapsed;
            var scaleOut = new DoubleAnimation(1.0, 0.96, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };

            UnsavedPopupOverlay.BeginAnimation(OpacityProperty, overlayFade);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
        }

        private static void SelectComboByContent(ComboBox comboBox, string? value)
        {
            if (comboBox == null || comboBox.Items.Count == 0)
            {
                return;
            }

            var target = (value ?? string.Empty).Trim();
            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem combo)
                {
                    var tag = (combo.Tag?.ToString() ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(tag) && string.Equals(tag, target, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBox.SelectedItem = combo;
                        return;
                    }

                    var text = (combo.Content?.ToString() ?? string.Empty).Trim();
                    if (string.Equals(text, target, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBox.SelectedItem = combo;
                        return;
                    }
                }
            }
        }

        private static string GetComboValue(ComboBox comboBox, string fallback)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag?.ToString();
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    return tag.Trim();
                }

                var content = item.Content?.ToString();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    return content.Trim();
                }
            }

            return fallback;
        }

        private static string GetString(string key, string fallback)
        {
            return Application.Current?.TryFindResource(key) as string ?? fallback;
        }
    }
}
