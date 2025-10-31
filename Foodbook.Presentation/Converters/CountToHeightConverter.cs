using System;
using System.Globalization;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class CountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // Scale the height based on count, with max height of 160
                var maxHeight = 160.0;
                var maxCount = 20.0; // Assume max count for scaling
                
                var height = Math.Min(count * maxHeight / maxCount, maxHeight);
                return Math.Max(height, 20); // Minimum height of 20
            }
            
            return 20;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
