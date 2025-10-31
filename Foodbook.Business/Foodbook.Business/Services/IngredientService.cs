using Microsoft.EntityFrameworkCore;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Foodbook.Business.Interfaces;

namespace Foodbook.Business.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly FoodbookDbContext _context;

        public IngredientService(FoodbookDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ingredient>> GetUserIngredientsAsync(int userId)
        {
            return await _context.Ingredients
                .Where(i => i.UserId == userId)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Ingredient> AddIngredientAsync(Ingredient ingredient)
        {
            ingredient.CreatedAt = DateTime.UtcNow;
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient)
        {
            _context.Ingredients.Update(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<bool> DeleteIngredientAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return false;

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string name)
        {
            return await _context.Ingredients
                .Where(i => i.Name.Contains(name))
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ingredient>> GetSubstituteIngredientsAsync(string ingredientName)
        {
            // This is a simplified substitution logic
            // In a real application, this would use AI or a comprehensive database
            var substitutions = new Dictionary<string, List<string>>
            {
                ["butter"] = new List<string> { "margarine", "coconut oil", "olive oil" },
                ["milk"] = new List<string> { "almond milk", "soy milk", "coconut milk" },
                ["eggs"] = new List<string> { "flax eggs", "applesauce", "banana" },
                ["sugar"] = new List<string> { "honey", "maple syrup", "stevia" },
                ["flour"] = new List<string> { "almond flour", "coconut flour", "oat flour" },
                ["salt"] = new List<string> { "sea salt", "kosher salt", "himalayan salt" }
            };

            var lowerName = ingredientName.ToLower();
            var substitutes = substitutions.ContainsKey(lowerName) ? substitutions[lowerName] : new List<string>();

            // Find actual ingredients in the database that match the substitutes
            var substituteIngredients = new List<Ingredient>();
            foreach (var substitute in substitutes)
            {
                var ingredients = await _context.Ingredients
                    .Where(i => i.Name.ToLower().Contains(substitute.ToLower()))
                    .ToListAsync();
                substituteIngredients.AddRange(ingredients);
            }

            return substituteIngredients.DistinctBy(i => i.Name);
        }

        public async Task<Ingredient?> GetIngredientByNameAsync(string name, int userId)
        {
            return await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower() && i.UserId == userId);
        }
    }
}
