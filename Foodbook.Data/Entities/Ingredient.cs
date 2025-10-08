using System.ComponentModel.DataAnnotations;

namespace Foodbook.Data.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Unit { get; set; } = "piece"; // piece, gram, ml, cup, etc.
        
        public decimal Quantity { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public int UserId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}
