using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Foodbook.Presentation.Converters
{
    public class SidebarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible; // Default visible
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
