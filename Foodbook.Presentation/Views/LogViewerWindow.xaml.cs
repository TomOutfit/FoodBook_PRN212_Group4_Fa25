using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using LogEntryEntity = Foodbook.Data.Entities.LogEntry;

namespace Foodbook.Presentation.Views
{
    public partial class LogViewerWindow : Window
    {
        private readonly ILoggingService _loggingService;

        public LogViewerWindow(ILoggingService loggingService)
        {
            InitializeComponent();
            _loggingService = loggingService;
            
            // Show loading message immediately
            ShowLoadingMessage();
            
            // Load logs asynchronously
            _ = LoadLogs();
        }
        
        private void ShowLoadingMessage()
        {
            LogsPanel.Children.Clear();
            var loadingText = new TextBlock
            {
                Text = "🔄 Loading logs...",
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            LogsPanel.Children.Add(loadingText);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLogs();
        }

        private async void FeatureFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadLogs();
        }

        private async void LogTypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadLogs();
        }

        private async void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadLogs();
        }

        private async Task LoadLogs()
        {
            try
            {
                if (_loggingService == null)
                {
                    MessageBox.Show("Logging service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var logs = await _loggingService.GetLogsAsync();
                
                // Apply filters
                var filteredLogs = ApplyFilters(logs);
                
                DisplayLogs(filteredLogs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private IEnumerable<LogEntryEntity> ApplyFilters(IEnumerable<LogEntryEntity> logs)
        {
            var filteredLogs = logs.AsEnumerable();

            // Apply log type filter
            if (LogTypeFilterComboBox.SelectedItem is ComboBoxItem selectedLogType)
            {
                var logType = selectedLogType.Content.ToString();
                if (logType != "All")
                {
                    filteredLogs = filteredLogs.Where(log => log.LogType == logType);
                }
            }

            // Apply time range filter
            if (TimeRangeComboBox.SelectedItem is ComboBoxItem selectedTimeRange)
            {
                var timeRange = selectedTimeRange.Content.ToString();
                var cutoffTime = timeRange switch
                {
                    "Last Hour" => DateTime.Now.AddHours(-1),
                    "Last 24 Hours" => DateTime.Now.AddDays(-1),
                    "Last 7 Days" => DateTime.Now.AddDays(-7),
                    _ => DateTime.MinValue
                };
                
                if (cutoffTime != DateTime.MinValue)
                {
                    filteredLogs = filteredLogs.Where(log => log.Timestamp >= cutoffTime);
                }
            }

            return filteredLogs.OrderByDescending(log => log.Timestamp);
        }

        private void DisplayLogs(IEnumerable<LogEntryEntity> logs)
        {
            LogsPanel.Children.Clear();

            if (!logs.Any())
            {
                var noLogsText = new TextBlock
                {
                    Text = "📝 No logs found. Start using AI features to see analytics!",
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                LogsPanel.Children.Add(noLogsText);
                return;
            }

            // Group logs by feature
            var groupedLogs = logs.GroupBy(l => l.FeatureName).OrderByDescending(g => g.Count());

            foreach (var featureGroup in groupedLogs)
            {
                // Feature header
#pragma warning disable CS8604 // Possible null reference argument.
                var featureHeader = CreateFeatureHeader(featureGroup.Key, featureGroup.Count());
#pragma warning restore CS8604 // Possible null reference argument.
                LogsPanel.Children.Add(featureHeader);

                // Logs for this feature
                foreach (var log in featureGroup.OrderByDescending(l => l.Timestamp))
                {
                    var logItem = CreateLogItem(log);
                    LogsPanel.Children.Add(logItem);
                }
            }
        }

        private Border CreateFeatureHeader(string featureName, int count)
        {
            var header = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var icon = new TextBlock
            {
                Text = GetFeatureIcon(featureName),
                FontSize = 20,
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var title = new TextBlock
            {
                Text = featureName,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            var countText = new TextBlock
            {
                Text = $"({count} logs)",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(191, 219, 254)),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(countText);

            header.Child = stackPanel;
            return header;
        }

        private Border CreateLogItem(LogEntryEntity log)
        {
            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240))
            };

            var stackPanel = new StackPanel();

            // Header row
            var headerRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

#pragma warning disable CS8604 // Possible null reference argument.
            var logTypeIcon = new TextBlock
            {
                Text = GetLogTypeIcon(log.LogType),
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
#pragma warning restore CS8604 // Possible null reference argument.

            var logType = new TextBlock
            {
                Text = log.LogType,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Foreground = GetLogTypeColor(log.LogType),
                VerticalAlignment = VerticalAlignment.Center
            };

            var timestamp = new TextBlock
            {
                Text = log.Timestamp.ToString("HH:mm:ss"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var duration = new TextBlock
            {
                Text = log.Duration.HasValue ? $"{log.Duration.Value.TotalMilliseconds:F0}ms" : "",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            headerRow.Children.Add(logTypeIcon);
            headerRow.Children.Add(logType);
            headerRow.Children.Add(timestamp);
            if (log.Duration.HasValue)
                headerRow.Children.Add(duration);

            // Message
            var message = new TextBlock
            {
                Text = log.Message,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Details (if available)
            var details = new TextBlock
            {
                Text = !string.IsNullOrEmpty(log.Details) ? log.Details : "",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Context (if available)
            var context = new TextBlock
            {
                Text = !string.IsNullOrEmpty(log.Context) ? $"Context: {log.Context}" : "",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(headerRow);
            stackPanel.Children.Add(message);
            
            if (!string.IsNullOrEmpty(log.Details))
                stackPanel.Children.Add(details);
            
            if (!string.IsNullOrEmpty(log.Context))
                stackPanel.Children.Add(context);

            border.Child = stackPanel;
            return border;
        }

        private string GetFeatureIcon(string featureName)
        {
            return featureName switch
            {
                "Chef Judge" => "👨‍🍳",
                "Generate Recipe" => "✨",
                "Smart Shopping List" => "🛒",
                "Nutrition Analysis" => "🍎",
                _ => "🤖"
            };
        }

        private string GetLogTypeIcon(string logType)
        {
            return logType switch
            {
                "Usage" => "📊",
                "Error" => "❌",
                "Performance" => "⚡",
                "AI" => "🧠",
                _ => "📝"
            };
        }

        private Brush GetLogTypeColor(string logType)
        {
            return logType switch
            {
                "Usage" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                "Error" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                "Performance" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                "AI" => new SolidColorBrush(Color.FromRgb(139, 92, 246)),
                _ => new SolidColorBrush(Color.FromRgb(100, 116, 139))
            };
        }
    }
}
