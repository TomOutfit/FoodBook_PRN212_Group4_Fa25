using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Foodbook.Presentation.Converters
{
    public class TabButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selectedTab && parameter is string targetTab)
            {
                if (selectedTab == targetTab)
                {
                    // Return active style properties
                    return new
                    {
                        Background = new SolidColorBrush(Color.FromRgb(224, 242, 241)), // #E0F2F1
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 121, 107)), // #00796B
                        FontWeight = FontWeights.Bold
                    };
                }
                else
                {
                    // Return normal style properties
                    return new
                    {
                        Background = Brushes.Transparent,
                        Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)), // #6B7280
                        FontWeight = FontWeights.Medium
                    };
                }
            }
            return new
            {
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                FontWeight = FontWeights.Medium
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
