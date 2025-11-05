using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodbook.Data.Entities
{
    [Table("LogEntries")]
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Level { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? Exception { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? Source { get; set; }

        public int? UserId { get; set; }

        [MaxLength(100)]
        public string? FeatureName { get; set; }

        [MaxLength(50)]
        public string? LogType { get; set; }

        public TimeSpan? Duration { get; set; }

        public string? Details { get; set; }

        public string? Context { get; set; }

        [MaxLength(100)]
        public string? ExceptionType { get; set; }

        public string? StackTrace { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
