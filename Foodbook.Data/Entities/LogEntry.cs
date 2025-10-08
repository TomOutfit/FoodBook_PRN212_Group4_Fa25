using System.ComponentModel.DataAnnotations;

namespace Foodbook.Data.Entities
{
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string LogType { get; set; } = string.Empty; // Usage, Error, Performance, AI
        
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string Details { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Context { get; set; } = string.Empty;
        
        public TimeSpan? Duration { get; set; }
        
        [MaxLength(100)]
        public string ExceptionType { get; set; } = string.Empty;
        
        [MaxLength(4000)]
        public string StackTrace { get; set; } = string.Empty;
    }
}
