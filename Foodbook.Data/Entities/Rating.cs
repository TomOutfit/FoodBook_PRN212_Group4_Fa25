using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodbook.Data.Entities
{
    [Table("Ratings")]
    public class Rating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RecipeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        [Column("Rating")]
        public int Score { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("RecipeId")]
        public virtual Recipe Recipe { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
