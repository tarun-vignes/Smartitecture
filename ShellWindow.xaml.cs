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
        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromMilliseconds(200));

        public ShellWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                AppNavigator.Initialize(MainFrame);
                MainFrame.Navigated += MainFrame_Navigated;
                AppNavigator.Navigate(new WelcomePage());
            };
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var content = e.Content as FrameworkElement;
            if (content == null) return;

            content.Opacity = 0;
            var translate = new TranslateTransform(12, 0);
            content.RenderTransform = translate;

            var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            var opacity = new DoubleAnimation(0, 1, TransitionDuration) { EasingFunction = ease };
            var slide = new DoubleAnimation(12, 0, TransitionDuration) { EasingFunction = ease };

            content.BeginAnimation(OpacityProperty, opacity);
            translate.BeginAnimation(TranslateTransform.XProperty, slide);
        }
    }
}
