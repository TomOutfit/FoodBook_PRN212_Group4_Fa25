using System.ComponentModel.DataAnnotations;

namespace Foodbook.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public bool IsAdmin { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
