using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Smartitecture.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object _mainContent;

        public object MainContent
        {
            get => _mainContent;
            set
            {
                _mainContent = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
