using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodbook.Data.Entities
{
    [Table("Ingredients")]
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? NutritionInfo { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Quantity { get; set; }

        public int? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Inventory management fields
        public DateTime? ExpiryDate { get; set; }

        public DateTime? PurchasedAt { get; set; }

        [MaxLength(50)]
        public string? Location { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinQuantity { get; set; }

        // Navigation properties
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
