using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Foodbook.Data;
using System.Diagnostics;
using LogEntryEntity = Foodbook.Data.Entities.LogEntry;

namespace Foodbook.Business.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly FoodbookDbContext _context;

        public LoggingService(FoodbookDbContext context)
        {
            _context = context;
        }

        public async Task LogFeatureUsageAsync(string featureName, string userId, string details = "")
        {
            try
            {
                var logEntry = new LogEntryEntity
                {
                    Timestamp = DateTime.UtcNow,
                    FeatureName = featureName,
                    UserId = userId,
                    LogType = "Usage",
                    Message = $"User {userId} used feature: {featureName}",
                    Details = details,
                    Context = "Feature Usage Tracking"
                };

                _context.LogEntries.Add(logEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Fallback logging to console if database logging fails
                Console.WriteLine($"Logging Error: {ex.Message}");
            }
        }

        public async Task LogErrorAsync(string featureName, string userId, Exception exception, string context = "")
        {
            try
            {
                var logEntry = new LogEntryEntity
                {
                    Timestamp = DateTime.UtcNow,
                    FeatureName = featureName,
                    UserId = userId,
                    LogType = "Error",
                    Message = $"Error in {featureName}: {exception.Message}",
                    Details = exception.ToString(),
                    Context = context,
                    ExceptionType = exception.GetType().Name,
                    StackTrace = exception.StackTrace ?? ""
                };

                _context.LogEntries.Add(logEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Logging Failed: {ex.Message}");
            }
        }

        public async Task LogPerformanceAsync(string featureName, string userId, TimeSpan duration, string details = "")
        {
            try
            {
                var logEntry = new LogEntryEntity
                {
                    Timestamp = DateTime.UtcNow,
                    FeatureName = featureName,
                    UserId = userId,
                    LogType = "Performance",
                    Message = $"Performance: {featureName} took {duration.TotalMilliseconds:F2}ms",
                    Details = details,
                    Context = "Performance Monitoring",
                    Duration = duration
                };

                _context.LogEntries.Add(logEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Performance Logging Failed: {ex.Message}");
            }
        }

        public async Task LogAIActivityAsync(string aiFeature, string userId, string input, string output, TimeSpan processingTime)
        {
            try
            {
                var logEntry = new LogEntryEntity
                {
                    Timestamp = DateTime.UtcNow,
                    FeatureName = aiFeature,
                    UserId = userId,
                    LogType = "AI",
                    Message = $"AI {aiFeature} processed in {processingTime.TotalMilliseconds:F2}ms",
                    Details = $"Input: {input.Substring(0, Math.Min(100, input.Length))}... | Output: {output.Substring(0, Math.Min(100, output.Length))}...",
                    Context = "AI Activity Tracking",
                    Duration = processingTime
                };

                _context.LogEntries.Add(logEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Logging Failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Data.Entities.LogEntry>> GetLogsAsync(string? featureName = null, string? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.LogEntries.AsQueryable();

                if (!string.IsNullOrEmpty(featureName))
                {
                    query = query.Where(l => l.FeatureName == featureName);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(l => l.UserId == userId);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp <= toDate.Value);
                }

                return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Logs Failed: {ex.Message}");
                return new List<Data.Entities.LogEntry>();
            }
        }
    }
}
