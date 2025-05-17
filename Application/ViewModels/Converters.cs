using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AIPal.Application.ViewModels
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool boolValue && boolValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    /// <summary>
    /// Converts a boolean value to its negation.
    /// </summary>
    public class BoolNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool boolValue && boolValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool boolValue && boolValue);
        }
    }

    /// <summary>
    /// Converts a count value to a Visibility value.
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is int count && count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a DateTime value to a formatted string.
    /// </summary>
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                // Format: "Today, 3:45 PM" or "May 16, 3:45 PM"
                if (dateTime.Date == DateTime.Today)
                {
                    return $"Today, {dateTime.ToString("h:mm tt")}";
                }
                else
                {
                    return $"{dateTime.ToString("MMM d")}, {dateTime.ToString("h:mm tt")}";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a Dictionary to a formatted string.
    /// </summary>
    public class DictionaryToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Dictionary<string, object> dictionary)
            {
                var json = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true });
                return json;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an object to a formatted string.
    /// </summary>
    public class ObjectToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
                    return json;
                }
                catch
                {
                    return value.ToString();
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean value to an error brush.
    /// </summary>
    public class BoolToErrorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool isError && isError) 
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 196, 43, 28)) 
                : new SolidColorBrush(Microsoft.UI.Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Selects a data template based on whether the message is from the user or the agent.
    /// </summary>
    public class MessageTemplateSelector : Microsoft.UI.Xaml.Controls.DataTemplateSelector
    {
        public Microsoft.UI.Xaml.DataTemplate UserTemplate { get; set; }
        public Microsoft.UI.Xaml.DataTemplate AgentTemplate { get; set; }

        protected override Microsoft.UI.Xaml.DataTemplate SelectTemplateCore(object item)
        {
            if (item is ChatMessageViewModel message)
            {
                return message.IsUser ? UserTemplate : AgentTemplate;
            }
            return base.SelectTemplateCore(item);
        }
    }
}
