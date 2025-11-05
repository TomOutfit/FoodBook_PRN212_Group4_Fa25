using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetAllRecipesAsync();
        Task<Recipe?> GetRecipeByIdAsync(int id);
        Task<IEnumerable<Recipe>> SearchRecipesAsync(string? name, string? ingredient, int? cookTime, string? difficulty);
        Task<Recipe> CreateRecipeAsync(Recipe recipe);
        Task<Recipe> UpdateRecipeAsync(Recipe recipe);
        Task<bool> DeleteRecipeAsync(int id);
        Task<Recipe> AdjustServingsAsync(int recipeId, int newServings);
        Task<IEnumerable<Recipe>> GetRecipesByUserIdAsync(int userId);
        Task<double> GetAverageRatingAsync(int recipeId);
    }
}
