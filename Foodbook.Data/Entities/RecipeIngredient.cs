using System.ComponentModel.DataAnnotations;

namespace Foodbook.Data.Entities
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }
        
        [MaxLength(50)]
        public string Unit { get; set; } = "piece";
        
        [MaxLength(200)]
        public string? Notes { get; set; }
        
        // Navigation properties
        public virtual Recipe Recipe { get; set; } = null!;
        public virtual Ingredient Ingredient { get; set; } = null!;
    }
}
