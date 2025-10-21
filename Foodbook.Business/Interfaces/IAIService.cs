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
        
        // New methods for AI-powered nutrition analysis
        Task<List<ParsedIngredient>> ParseRecipeTextAsync(string recipeText);
        Task<string> GetHealthFeedbackAsync(NutritionAnalysisResult nutritionInfo, string userGoal = "general health");
        Task<NutritionAnalysisResult> AnalyzeNutritionWithAIAsync(string recipeText, string userGoal = "general health");
    }
}
