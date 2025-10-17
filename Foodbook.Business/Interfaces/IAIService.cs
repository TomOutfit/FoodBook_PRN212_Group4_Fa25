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
    }
}
