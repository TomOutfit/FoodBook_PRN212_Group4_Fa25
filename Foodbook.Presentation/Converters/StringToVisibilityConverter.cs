using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            var stringValue = value as string ?? value?.ToString() ?? string.Empty;
            var parameterValue = parameter as string ?? parameter?.ToString() ?? string.Empty;

            return string.Equals(stringValue, parameterValue, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
