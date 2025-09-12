using Smartitecture.WPF.ViewModels;
using System.Windows;

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
