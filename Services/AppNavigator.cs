using System;
using System.Windows.Controls;

namespace Smartitecture.Services
{
    public static class AppNavigator
    {
        private static Frame? _frame;

        public static void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public static void Navigate(Page page)
        {
            _frame?.Navigate(page);
        }

        public static void GoBack()
        {
            if (_frame?.CanGoBack == true)
            {
                _frame.GoBack();
            }
        }
    }
}

