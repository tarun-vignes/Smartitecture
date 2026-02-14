using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Smartitecture.UI.Converters
{
    public class BoolToErrorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isError && isError)
            {
                return new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
            return new SolidColorBrush(Microsoft.UI.Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
