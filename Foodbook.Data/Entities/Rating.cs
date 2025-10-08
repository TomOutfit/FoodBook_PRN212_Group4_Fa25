using System.ComponentModel.DataAnnotations;

namespace Foodbook.Data.Entities
{
    public class Rating
    {
        public int Id { get; set; }
        
        [Range(1, 5)]
        public int Score { get; set; }
        
        [MaxLength(500)]
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Recipe Recipe { get; set; } = null!;
    }
}
