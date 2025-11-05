using Foodbook.Business.Models;
using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface IAIService
    {
        Task<ChefJudgeResult> JudgeDishAsync(string imagePath, string evaluationMode = "Casual");
        Task<ChefJudgeResult> JudgeDishAsync(byte[] imageData, string evaluationMode = "Casual");
        Task<string> GenerateRecipeSuggestionAsync(string ingredients);
        Task<string> AnalyzeNutritionAsync(string recipeDescription);
        Task<Recipe> GenerateRecipeFromIngredientsAsync(IEnumerable<string> ingredientNames, string dishName, int servings);
        Task<IEnumerable<string>> GetIngredientSubstitutionsAsync(string ingredientName);
        
        // Enhanced methods for Nutrition Analysis
        Task<string> ParseRecipeIngredientsAsync(string recipeText);
        Task<string> GenerateHealthAssessmentAsync(string nutritionData);
        Task<string> GenerateNutritionalAdviceAsync(string nutritionInfo, string userGoal);
        Task<List<IngredientDto>> ExtractIngredientsFromTextAsync(string recipeText);
    }
}
