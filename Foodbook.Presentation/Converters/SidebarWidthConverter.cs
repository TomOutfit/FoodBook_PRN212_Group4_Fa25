using System;
using System.Globalization;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class SidebarWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? 80 : 300; // Collapsed width: 80px, Expanded width: 300px
            }
            return 300; // Default expanded width
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
