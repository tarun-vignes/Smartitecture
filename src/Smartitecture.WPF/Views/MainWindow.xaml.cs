using System.Windows;
using Smartitecture.WPF.ViewModels;

namespace Smartitecture.WPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
