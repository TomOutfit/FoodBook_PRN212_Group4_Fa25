using System;
using System.Globalization;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class StringCleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var str = value.ToString() ?? string.Empty;
            
            // Remove ComboBoxItem object notation
            if (str.Contains("System.Windows.Controls.ComboBoxItem:"))
            {
                // Extract the actual value after the colon
                var parts = str.Split(':');
                if (parts.Length > 1 && parts[1] != null)
                {
                    return parts[1].Trim();
                }
            }
            
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

