using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface IShoppingListService
    {
        Task<ShoppingListResult> GenerateSmartShoppingListAsync(IEnumerable<Recipe> selectedRecipes, int userId);
        Task<ShoppingListResult> GenerateShoppingListFromIngredientsAsync(IEnumerable<string> ingredientNames, int userId);
        Task<IEnumerable<ShoppingCategory>> GetShoppingCategoriesAsync();
        Task<ShoppingListResult> OptimizeShoppingListAsync(ShoppingListResult shoppingList);
        Task<string> ExportShoppingListToNotesAsync(ShoppingListResult shoppingList, string listName);
        Task<ShoppingListResult> GenerateShoppingListFromMealPlanAsync(IEnumerable<MealPlanItem> mealPlanItems, int userId);
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
        public List<string> RecipeNames { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string ListName { get; set; } = string.Empty;
        public bool IsOptimized { get; set; }
        public decimal PotentialSavings { get; set; }
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
        public int RecipeCount { get; set; }
        public string StoreSection { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsChecked { get; set; }
        public string NutritionalInfo { get; set; } = string.Empty;
        public bool IsBulkPurchase { get; set; }
        public decimal BulkSavings { get; set; }
    }

    public class ShoppingCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<ShoppingItem> Items { get; set; } = new();
        public int Priority { get; set; }
        public string StoreSection { get; set; } = string.Empty;
        public string ShoppingOrder { get; set; } = string.Empty;
        public decimal CategoryTotal { get; set; }
        public int ItemCount => Items.Count;
    }

    // Helper class for meal planning integration
    public class MealPlanItem
    {
        public Recipe Recipe { get; set; } = new();
        public DateTime PlannedDate { get; set; }
        public int Servings { get; set; }
        public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
    }
}
