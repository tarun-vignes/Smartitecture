using System.Windows;
using System.Windows.Controls;

namespace Smartitecture.Controls
{
    public partial class AppTopBar : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(AppTopBar), new PropertyMetadata("Smartitecture", OnTitleChanged));

        public static readonly DependencyProperty ShowBackProperty = DependencyProperty.Register(
            nameof(ShowBack), typeof(bool), typeof(AppTopBar), new PropertyMetadata(false, OnShowBackChanged));

        public event RoutedEventHandler? BackClicked;
        public event RoutedEventHandler? HomeClicked;
        public event RoutedEventHandler? SettingsClicked;

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool ShowBack
        {
            get => (bool)GetValue(ShowBackProperty);
            set => SetValue(ShowBackProperty, value);
        }

        public AppTopBar()
        {
            InitializeComponent();
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AppTopBar bar)
            {
                bar.TitleBlock.Text = e.NewValue?.ToString() ?? "Smartitecture";
            }
        }

        private static void OnShowBackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AppTopBar bar)
            {
                bar.BackButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackClicked?.Invoke(this, e);
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            HomeClicked?.Invoke(this, e);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsClicked?.Invoke(this, e);
        }
    }
}

