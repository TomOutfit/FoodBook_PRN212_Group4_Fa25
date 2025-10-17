using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface IShoppingListService
    {
        Task<ShoppingListResult> GenerateSmartShoppingListAsync(IEnumerable<Recipe> selectedRecipes, int userId);
        Task<ShoppingListResult> GenerateShoppingListFromIngredientsAsync(IEnumerable<string> ingredientNames, int userId);
        Task<IEnumerable<ShoppingCategory>> GetShoppingCategoriesAsync();
        Task<ShoppingListResult> OptimizeShoppingListAsync(ShoppingListResult shoppingList);
    }

    public class ShoppingListResult
    {
        public List<ShoppingItem> Items { get; set; } = new();
        public List<ShoppingCategory> Categories { get; set; } = new();
        public decimal EstimatedCost { get; set; }
        public int TotalItems { get; set; }
        public TimeSpan EstimatedShoppingTime { get; set; }
        public List<string> StoreSuggestions { get; set; } = new();
        public List<string> Tips { get; set; } = new();
    }

    public class ShoppingItem
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsEssential { get; set; }
        public bool IsOptional { get; set; }
        public decimal EstimatedPrice { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<string> Substitutions { get; set; } = new();
    }

    public class ShoppingCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<ShoppingItem> Items { get; set; } = new();
        public int Priority { get; set; }
    }
}
