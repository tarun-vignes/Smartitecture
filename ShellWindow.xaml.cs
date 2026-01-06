using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Smartitecture.Services;
using Smartitecture.UI.Pages;

namespace Smartitecture
{
    public partial class ShellWindow : Window
    {
        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromMilliseconds(180));
        private bool _updatingNav;

        public ShellWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                AppNavigator.Initialize(MainFrame);
                MainFrame.Navigated += MainFrame_Navigated;
                NavigateDashboard();
            };
        }

        private void NavigateDashboard()
        {
            AppNavigator.Navigate(new DashboardPage());
        }

        private void NavigateChat()
        {
            AppNavigator.Navigate(new ChatPage(AppSession.LlmService));
        }

        private void NavigateSettings()
        {
            AppNavigator.Navigate(new SettingsPage());
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_updatingNav) return;
            NavigateDashboard();
        }

        private void NavChat_Click(object sender, RoutedEventArgs e)
        {
            if (_updatingNav) return;
            NavigateChat();
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_updatingNav) return;
            NavigateSettings();
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateNavSelection(e.Content);

            if (e.Content is not FrameworkElement content)
            {
                return;
            }

            content.Opacity = 0;
            var translate = new TranslateTransform(12, 0);
            content.RenderTransform = translate;

            var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            var opacity = new DoubleAnimation(0, 1, TransitionDuration) { EasingFunction = ease };
            var slide = new DoubleAnimation(12, 0, TransitionDuration) { EasingFunction = ease };

            content.BeginAnimation(OpacityProperty, opacity);
            translate.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        private void UpdateNavSelection(object content)
        {
            _updatingNav = true;
            try
            {
                NavDashboard.IsChecked = content is DashboardPage;
                NavChat.IsChecked = content is ChatPage;
                NavSettings.IsChecked = content is SettingsPage;
            }
            finally
            {
                _updatingNav = false;
            }
        }
    }
}
