using System;

namespace Foodbook.Business.Interfaces
{
    public interface ILoggingService
    {
        Task LogFeatureUsageAsync(string featureName, string userId, string details = "");
        Task LogErrorAsync(string featureName, string userId, Exception exception, string context = "");
        Task LogPerformanceAsync(string featureName, string userId, TimeSpan duration, string details = "");
        Task LogAIActivityAsync(string aiFeature, string userId, string input, string output, TimeSpan processingTime);
        Task<IEnumerable<Data.Entities.LogEntry>> GetLogsAsync(string? featureName = null, string? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string LogType { get; set; } = string.Empty; // Usage, Error, Performance, AI
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public TimeSpan? Duration { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
    }
}
