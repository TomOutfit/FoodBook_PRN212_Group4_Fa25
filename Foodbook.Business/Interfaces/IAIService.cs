using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface IAIService
    {
        Task<ChefJudgeResult> JudgeDishAsync(string imagePath);
        Task<ChefJudgeResult> JudgeDishAsync(byte[] imageData);
        Task<Recipe> GenerateRecipeFromIngredientsAsync(IEnumerable<string> ingredientNames, string dishName, int servings);
        Task<IEnumerable<string>> GetIngredientSubstitutionsAsync(string ingredientName);
        Task<string> GenerateCookingTipsAsync(string recipeTitle);
    }

    public class ChefJudgeResult
    {
        public int Score { get; set; } // 1-10
        public string Comment { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public string OverallRating { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        
        // Enhanced detailed analysis
        public int PresentationScore { get; set; } // 1-10
        public int ColorScore { get; set; } // 1-10
        public int TextureScore { get; set; } // 1-10
        public int PlatingScore { get; set; } // 1-10
        public string HealthNotes { get; set; } = string.Empty;
        public List<string> ChefTips { get; set; } = new();
    }
}
