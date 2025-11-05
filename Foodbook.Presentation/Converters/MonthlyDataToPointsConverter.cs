using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Foodbook.Presentation.Converters
{
    public class MonthlyDataToPointsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.ObjectModel.ObservableCollection<int> monthlyData && monthlyData.Count > 0)
            {
                var points = new System.Windows.Media.PointCollection();
                var maxValue = Math.Max(monthlyData.Max(), 1); // Avoid division by zero
                var canvasHeight = 200.0;
                var canvasWidth = 300.0;
                var spacing = canvasWidth / (monthlyData.Count - 1);

                for (int i = 0; i < monthlyData.Count; i++)
                {
                    var x = i * spacing;
                    var y = canvasHeight - (monthlyData[i] * canvasHeight / maxValue);
                    points.Add(new Point(x, y));
                }

                return points;
            }

            // Return default points if no data
            return new System.Windows.Media.PointCollection
            {
                new Point(20, 180),
                new Point(60, 160),
                new Point(100, 140),
                new Point(140, 120),
                new Point(180, 100),
                new Point(220, 80),
                new Point(260, 60)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
