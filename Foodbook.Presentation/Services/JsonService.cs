using System.IO;
using System.Text.Json;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.Services
{
    public class JsonService
    {
        private readonly JsonSerializerOptions _options;

        public JsonService()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<bool> ExportRecipesToJsonAsync(IEnumerable<Recipe> recipes, string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(recipes, _options);
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Recipe>?> ImportRecipesFromJsonAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var recipes = JsonSerializer.Deserialize<IEnumerable<Recipe>>(json, _options);
                return recipes;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ExportIngredientsToJsonAsync(IEnumerable<Ingredient> ingredients, string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(ingredients, _options);
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Ingredient>?> ImportIngredientsFromJsonAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var ingredients = JsonSerializer.Deserialize<IEnumerable<Ingredient>>(json, _options);
                return ingredients;
            }
            catch
            {
                return null;
            }
        }

        public string GetDefaultExportPath(string type)
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileName = $"{type}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            return Path.Combine(directory, fileName);
        }
    }
}
