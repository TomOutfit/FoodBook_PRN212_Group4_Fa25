using Microsoft.EntityFrameworkCore;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Foodbook.Business.Interfaces;

namespace Foodbook.Business.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly FoodbookDbContext _context;

        public RecipeService(FoodbookDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Ratings)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Recipe>> SearchRecipesAsync(string? name, string? ingredient, int? cookTime, string? difficulty)
        {
            var query = _context.Recipes
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Ratings)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(r => r.Title.Contains(name) || r.Description.Contains(name));
            }

            if (!string.IsNullOrEmpty(ingredient))
            {
                query = query.Where(r => r.RecipeIngredients.Any(ri => ri.Ingredient.Name.Contains(ingredient)));
            }

            if (cookTime.HasValue)
            {
                query = query.Where(r => r.CookTime <= cookTime.Value);
            }

            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(r => r.Difficulty == difficulty);
            }

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.UpdatedAt = DateTime.UtcNow;
            
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<Recipe> UpdateRecipeAsync(Recipe recipe)
        {
            recipe.UpdatedAt = DateTime.UtcNow;
            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<bool> DeleteRecipeAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return false;

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Recipe> AdjustServingsAsync(int recipeId, int newServings)
        {
            var recipe = await GetRecipeByIdAsync(recipeId);
            if (recipe == null) throw new ArgumentException("Recipe not found");

            var originalServings = recipe.Servings;
            var ratio = (double)newServings / originalServings;

            // Adjust all ingredient quantities
            foreach (var recipeIngredient in recipe.RecipeIngredients)
            {
                recipeIngredient.Quantity = Math.Round((decimal)((double)recipeIngredient.Quantity * ratio), 2);
            }

            recipe.Servings = newServings;
            recipe.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByUserIdAsync(int userId)
        {
            return await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Ratings)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int recipeId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.RecipeId == recipeId)
                .Select(r => r.Score)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0.0;
        }
    }
}
