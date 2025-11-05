using System.ComponentModel.DataAnnotations;

namespace Foodbook.Data.Entities
{
    public class NutritionInfo
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        
        // Macronutrients (per serving)
        public double Calories { get; set; }
        public double Protein { get; set; } // grams
        public double Carbohydrates { get; set; } // grams
        public double Fat { get; set; } // grams
        public double Fiber { get; set; } // grams
        public double Sugar { get; set; } // grams
        public double Sodium { get; set; } // mg
        
        // Micronutrients
        public double VitaminA { get; set; } // IU
        public double VitaminC { get; set; } // mg
        public double Calcium { get; set; } // mg
        public double Iron { get; set; } // mg
        
        // AI Analysis
        public string? HealthScore { get; set; } // A, B, C, D, F
        public string? AIFeedback { get; set; } // AI-generated health recommendations
        public string? Warnings { get; set; } // High sodium, sugar warnings
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
