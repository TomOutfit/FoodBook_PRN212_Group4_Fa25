using Microsoft.EntityFrameworkCore;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Foodbook.Business.Interfaces;

namespace Foodbook.Business.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly FoodbookDbContext _context;
        private readonly IUnsplashImageService? _imageService;

        public RecipeService(FoodbookDbContext context, IUnsplashImageService? imageService = null)
        {
            _context = context;
            _imageService = imageService;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
        {
            var recipes = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Ratings)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Tự động lấy ảnh cho các recipes chưa có ảnh
            await LoadMissingImagesAsync(recipes);

            return recipes;
        }

        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe != null && string.IsNullOrEmpty(recipe.ImageUrl))
            {
                await LoadMissingImageAsync(recipe);
            }

            return recipe;
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
                query = query.Where(r => (r.Title != null && r.Title.Contains(name)) || (r.Description != null && r.Description.Contains(name)));
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

            var recipes = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            
            // Tự động lấy ảnh cho các recipes chưa có ảnh
            await LoadMissingImagesAsync(recipes);

            return recipes;
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
            var recipes = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Ratings)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Tự động lấy ảnh cho các recipes chưa có ảnh
            await LoadMissingImagesAsync(recipes);

            return recipes;
        }

        public async Task<double> GetAverageRatingAsync(int recipeId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.RecipeId == recipeId)
                .Select(r => r.Score)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0.0;
        }

        // Helper methods để tự động load ảnh từ Internet
        private async Task LoadMissingImagesAsync(IEnumerable<Recipe> recipes)
        {
            var recipesToLoad = recipes.Where(r => string.IsNullOrEmpty(r.ImageUrl)).ToList();
            
            // Process images in parallel but with limited concurrency to avoid overwhelming the API
            var semaphore = new SemaphoreSlim(3, 3); // Limit to 3 concurrent requests
            var tasks = recipesToLoad.Select(async recipe =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await LoadMissingImageAsync(recipe);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            await Task.WhenAll(tasks);
        }

        private async Task LoadMissingImageAsync(Recipe recipe)
        {
            try
            {
                // Chỉ fetch ảnh nếu có imageService
                if (_imageService != null && !string.IsNullOrEmpty(recipe.Title))
                {
                    // Lấy ảnh từ Internet dựa trên tên món ăn
                    var imageUrl = await _imageService.SearchFoodImageAsync(recipe.Title);
                    
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // Chỉ set URL cho object, không update database ngay
                        // Database sẽ được update sau khi recipe được save trong session hiện tại
                        recipe.ImageUrl = imageUrl;
                        recipe.UpdatedAt = DateTime.UtcNow;
                        
                        Console.WriteLine($"✅ Đã tải ảnh cho: {recipe.Title}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Không thể tải ảnh cho {recipe.Title}: {ex.Message}");
            }
        }
    }
}
