using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class BooleanToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLoading)
            {
                return isLoading ? "LoadingButtonStyle" : "ButtonStyle";
            }
            return "ButtonStyle";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
