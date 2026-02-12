using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Smartitecture.UI.Converters
{
    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return 0d;
            }

            var width = ToDouble(values[0]);
            var value = ToDouble(values[1]);
            var max = ToDouble(values[2]);

            if (width <= 0 || max <= 0)
            {
                return 0d;
            }

            var ratio = value / max;
            if (ratio < 0) ratio = 0;
            if (ratio > 1) ratio = 1;

            return width * ratio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static double ToDouble(object value)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
            {
                return 0d;
            }

            try
            {
                return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0d;
            }
        }
    }
}
