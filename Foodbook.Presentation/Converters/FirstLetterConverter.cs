using System;
using System.Globalization;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var str = value.ToString() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;
            
            // Lấy chữ cái đầu tiên và chuyển thành chữ hoa
            return str.Trim().Substring(0, 1).ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

