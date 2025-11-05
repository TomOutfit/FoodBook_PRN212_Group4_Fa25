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
                
            string stringValue = value.ToString();
            string parameterValue = parameter.ToString();
            
            return stringValue.Equals(parameterValue, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
