using System.ComponentModel.DataAnnotations;

namespace Foodbook.Data.Entities
{
    public class Recipe
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Instructions { get; set; } = string.Empty;
        
        public int CookTime { get; set; } // in minutes
        
        [MaxLength(50)]
        public string Difficulty { get; set; } = "Easy"; // Easy, Medium, Hard
        
        public string? ImageUrl { get; set; }
        
        public int Servings { get; set; } = 4;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public int UserId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
