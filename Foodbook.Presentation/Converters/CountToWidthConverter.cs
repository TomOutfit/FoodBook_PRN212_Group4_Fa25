using System;
using System.Globalization;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class CountToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // Scale the width based on count, with max width of 200
                var maxWidth = 200.0;
                var maxCount = 30.0; // Assume max count for scaling
                
                var width = Math.Min(count * maxWidth / maxCount, maxWidth);
                return Math.Max(width, 20); // Minimum width of 20
            }
            
            return 20;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
