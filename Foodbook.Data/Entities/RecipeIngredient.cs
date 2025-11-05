using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foodbook.Data.Entities
{
    [Table("RecipeIngredients")]
    public class RecipeIngredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RecipeId { get; set; }

        [Required]
        public int IngredientId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("RecipeId")]
        public virtual Recipe Recipe { get; set; } = null!;

        [ForeignKey("IngredientId")]
        public virtual Ingredient Ingredient { get; set; } = null!;
    }
}
