using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface IIngredientService
    {
        Task<IEnumerable<Ingredient>> GetUserIngredientsAsync(int userId);
        Task<Ingredient> AddIngredientAsync(Ingredient ingredient);
        Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient);
        Task<bool> DeleteIngredientAsync(int id);
        Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string name);
        Task<IEnumerable<Ingredient>> GetSubstituteIngredientsAsync(string ingredientName);
        Task<Ingredient?> GetIngredientByNameAsync(string name, int userId);

        // Deduct quantities from pantry for a given user
        Task DeductIngredientsFromPantryAsync(int userId, IDictionary<string, decimal> ingredientUsage);
    }
}
