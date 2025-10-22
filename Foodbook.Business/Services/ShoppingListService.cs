using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Foodbook.Data;

namespace Foodbook.Business.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly FoodbookDbContext _context;

        public ShoppingListService(FoodbookDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingListResult> GenerateSmartShoppingListAsync(IEnumerable<Recipe> selectedRecipes, int userId)
        {
            // Simulate AI processing delay
            await Task.Delay(2000);

            var recipes = selectedRecipes.ToList();
            
            // Step 1: Truy vấn Nhu cầu - Tổng hợp nguyên liệu từ tất cả recipes
            var recipeIds = recipes.Select(r => r.Id).ToList();
            var requiredIngredients = await _context.RecipeIngredients
                .Include(ri => ri.Ingredient)
                .Where(ri => recipeIds.Contains(ri.RecipeId))
                .GroupBy(ri => ri.Ingredient.Name.ToLower())
                .Select(g => new
                {
                    IngredientName = g.Key,
                    TotalQuantity = g.Sum(ri => ri.Quantity),
                    Unit = g.First().Ingredient.Unit ?? "piece",
                    Category = g.First().Ingredient.Category ?? "Pantry"
                })
                .ToListAsync();

            // Step 2: Truy vấn Kho (Stock) - Lấy tất cả nguyên liệu của user
            var userStock = await _context.Ingredients
                .Where(i => i.UserId == userId)
                .ToDictionaryAsync(i => i.Name.ToLower(), i => i.Quantity ?? 0);

            // Step 3: Đối chiếu & Trừ Kho - So sánh nhu cầu với kho hiện có
            var shoppingItems = new List<ShoppingItem>();
            var totalCost = 0m;

            foreach (var required in requiredIngredients)
            {
                var neededQuantity = required.TotalQuantity;
                
                // Kiểm tra kho hiện có
                if (userStock.ContainsKey(required.IngredientName))
                {
                    var availableStock = userStock[required.IngredientName];
                    neededQuantity = Math.Max(0, required.TotalQuantity - availableStock);
                }

                // Chỉ thêm vào shopping list nếu cần mua
                if (neededQuantity > 0)
                {
                    var item = new ShoppingItem
                    {
                        Name = CapitalizeFirst(required.IngredientName),
                        Quantity = neededQuantity,
                        Unit = required.Unit,
                        Category = CategorizeIngredient(required.IngredientName),
                        IsEssential = IsEssentialIngredient(required.IngredientName),
                        IsOptional = IsOptionalIngredient(required.IngredientName),
                        EstimatedPrice = CalculateEstimatedPrice(required.IngredientName, neededQuantity),
                        Notes = GenerateShoppingNotes(required.IngredientName),
                        Substitutions = GetSubstitutions(required.IngredientName)
                    };

                    shoppingItems.Add(item);
                    totalCost += item.EstimatedPrice;
                }
            }

            // Step 4: Sắp xếp theo Category và tối ưu hóa
            var categories = OrganizeByCategories(shoppingItems);
            
            // Step 5: Tạo smart suggestions và tips
            var storeSuggestions = GenerateStoreSuggestions(categories);
            var tips = GenerateShoppingTips(shoppingItems, recipes.Count);
            var estimatedTime = CalculateShoppingTime(shoppingItems.Count, categories.Count);

            // Step 6: Tối ưu hóa shopping list
            var optimizedList = await OptimizeShoppingListAsync(new ShoppingListResult
            {
                Items = shoppingItems,
                Categories = categories,
                EstimatedCost = totalCost,
                TotalItems = shoppingItems.Count,
                EstimatedShoppingTime = estimatedTime,
                StoreSuggestions = storeSuggestions,
                Tips = tips
            });

            return optimizedList;
        }

        public async Task<ShoppingListResult> GenerateShoppingListFromIngredientsAsync(IEnumerable<string> ingredientNames, int userId)
        {
            // Simulate AI processing delay
            await Task.Delay(1500);

            var ingredients = ingredientNames.ToList();
            var shoppingItems = new List<ShoppingItem>();
            var totalCost = 0m;

            foreach (var ingredientName in ingredients)
            {
                var item = new ShoppingItem
                {
                    Name = CapitalizeFirst(ingredientName),
                    Quantity = 1, // Default quantity
                    Unit = GetDefaultUnit(ingredientName),
                    Category = CategorizeIngredient(ingredientName),
                    IsEssential = IsEssentialIngredient(ingredientName),
                    IsOptional = IsOptionalIngredient(ingredientName),
                    EstimatedPrice = CalculateEstimatedPrice(ingredientName, 1),
                    Notes = GenerateShoppingNotes(ingredientName),
                    Substitutions = GetSubstitutions(ingredientName)
                };

                shoppingItems.Add(item);
                totalCost += item.EstimatedPrice;
            }

            var categories = OrganizeByCategories(shoppingItems);
            var storeSuggestions = GenerateStoreSuggestions(categories);
            var tips = GenerateShoppingTips(shoppingItems, 1);
            var estimatedTime = CalculateShoppingTime(shoppingItems.Count, categories.Count);

            return new ShoppingListResult
            {
                Items = shoppingItems,
                Categories = categories,
                EstimatedCost = totalCost,
                TotalItems = shoppingItems.Count,
                EstimatedShoppingTime = estimatedTime,
                StoreSuggestions = storeSuggestions,
                Tips = tips
            };
        }

        public async Task<IEnumerable<ShoppingCategory>> GetShoppingCategoriesAsync()
        {
            await Task.Delay(500);

            return new List<ShoppingCategory>
            {
                new() { Name = "Produce", Icon = "🥬", Color = "#4CAF50", Priority = 1 },
                new() { Name = "Meat & Seafood", Icon = "🥩", Color = "#F44336", Priority = 2 },
                new() { Name = "Dairy & Eggs", Icon = "🥛", Color = "#FFC107", Priority = 3 },
                new() { Name = "Pantry", Icon = "🥫", Color = "#FF9800", Priority = 4 },
                new() { Name = "Frozen", Icon = "🧊", Color = "#2196F3", Priority = 5 },
                new() { Name = "Bakery", Icon = "🍞", Color = "#795548", Priority = 6 },
                new() { Name = "Beverages", Icon = "🥤", Color = "#9C27B0", Priority = 7 }
            };
        }

        public async Task<ShoppingListResult> OptimizeShoppingListAsync(ShoppingListResult shoppingList)
        {
            await Task.Delay(1000);

            // Optimize quantities and suggest bulk purchases
            var optimizedItems = new List<ShoppingItem>();
            var totalSavings = 0m;

            foreach (var item in shoppingList.Items)
            {
                var optimizedItem = item;
                
                // Suggest bulk purchases for frequently used items
                if (IsFrequentlyUsed(item.Name))
                {
                    optimizedItem.Quantity = Math.Ceiling(item.Quantity * 1.5m); // Buy 50% more
                    optimizedItem.Notes += " (Bulk purchase recommended)";
                    totalSavings += item.EstimatedPrice * 0.2m; // 20% savings
                }

                // Suggest substitutions for expensive items
                if (item.EstimatedPrice > 10m && item.Substitutions.Any())
                {
                    optimizedItem.Notes += $" Consider: {string.Join(", ", item.Substitutions.Take(2))}";
                }

                optimizedItems.Add(optimizedItem);
            }

            shoppingList.Items = optimizedItems;
            shoppingList.EstimatedCost -= totalSavings;
            shoppingList.Tips.Add($"💰 Potential savings: ${totalSavings:F2} with bulk purchases");

            return shoppingList;
        }

        private string CategorizeIngredient(string ingredientName)
        {
            var name = ingredientName.ToLower();
            
            if (new[] { "tomato", "onion", "garlic", "carrot", "potato", "bell pepper", "spinach", "broccoli", "lettuce", "cucumber" }.Any(x => name.Contains(x)))
                return "Produce";
            if (new[] { "chicken", "beef", "pork", "fish", "salmon", "shrimp", "turkey", "lamb" }.Any(x => name.Contains(x)))
                return "Meat & Seafood";
            if (new[] { "milk", "cheese", "yogurt", "butter", "cream", "eggs" }.Any(x => name.Contains(x)))
                return "Dairy & Eggs";
            if (new[] { "rice", "pasta", "bread", "flour", "sugar", "oil", "vinegar", "spices" }.Any(x => name.Contains(x)))
                return "Pantry";
            if (new[] { "frozen", "ice cream", "frozen vegetables" }.Any(x => name.Contains(x)))
                return "Frozen";
            
            return "Pantry";
        }

        private bool IsEssentialIngredient(string ingredientName)
        {
            var name = ingredientName.ToLower();
            var essential = new[] { "salt", "pepper", "oil", "onion", "garlic", "butter" };
            return essential.Any(x => name.Contains(x));
        }

        private bool IsOptionalIngredient(string ingredientName)
        {
            var name = ingredientName.ToLower();
            var optional = new[] { "garnish", "decoration", "optional", "garnish" };
            return optional.Any(x => name.Contains(x));
        }

        private decimal CalculateEstimatedPrice(string ingredientName, decimal quantity)
        {
            var name = ingredientName.ToLower();
            var basePrice = name switch
            {
                var n when n.Contains("chicken") => 8.99m,
                var n when n.Contains("beef") => 12.99m,
                var n when n.Contains("fish") => 15.99m,
                var n when n.Contains("vegetable") => 2.99m,
                var n when n.Contains("spice") => 4.99m,
                var n when n.Contains("oil") => 6.99m,
                _ => 3.99m
            };
            return basePrice * (decimal)Math.Ceiling((double)quantity);
        }

        private string GenerateShoppingNotes(string ingredientName)
        {
            var name = ingredientName.ToLower();
            var notes = new List<string>();

            if (name.Contains("chicken")) notes.Add("Look for organic, free-range");
            if (name.Contains("fish")) notes.Add("Check for freshness, clear eyes");
            if (name.Contains("vegetable")) notes.Add("Choose firm, vibrant colors");
            if (name.Contains("spice")) notes.Add("Check expiration date");
            if (name.Contains("oil")) notes.Add("Extra virgin recommended");

            return notes.Count > 0 ? string.Join("; ", notes) : "Choose fresh, high-quality";
        }

        private List<string> GetSubstitutions(string ingredientName)
        {
            var name = ingredientName.ToLower();
            var substitutions = new Dictionary<string, List<string>>
            {
                ["butter"] = new() { "margarine", "coconut oil", "olive oil" },
                ["milk"] = new() { "almond milk", "soy milk", "oat milk" },
                ["chicken"] = new() { "turkey", "tofu", "tempeh" },
                ["beef"] = new() { "lamb", "pork", "mushrooms" },
                ["onion"] = new() { "shallots", "leeks", "onion powder" },
                ["garlic"] = new() { "garlic powder", "shallots" }
            };

            return substitutions.FirstOrDefault(s => s.Key == name).Value ?? new List<string>();
        }

        private List<ShoppingCategory> OrganizeByCategories(List<ShoppingItem> items)
        {
            var categories = new Dictionary<string, ShoppingCategory>();

            foreach (var item in items)
            {
                if (!categories.ContainsKey(item.Category))
                {
                    categories[item.Category] = new ShoppingCategory
                    {
                        Name = item.Category,
                        Icon = GetCategoryIcon(item.Category),
                        Color = GetCategoryColor(item.Category),
                        Priority = GetCategoryPriority(item.Category)
                    };
                }
                categories[item.Category].Items.Add(item);
            }

            return categories.Values.OrderBy(c => c.Priority).ToList();
        }

        private string GetCategoryIcon(string category)
        {
            return category switch
            {
                "Produce" => "🥬",
                "Meat & Seafood" => "🥩",
                "Dairy & Eggs" => "🥛",
                "Pantry" => "🥫",
                "Frozen" => "🧊",
                "Bakery" => "🍞",
                "Beverages" => "🥤",
                _ => "📦"
            };
        }

        private string GetCategoryColor(string category)
        {
            return category switch
            {
                "Produce" => "#4CAF50",
                "Meat & Seafood" => "#F44336",
                "Dairy & Eggs" => "#FFC107",
                "Pantry" => "#FF9800",
                "Frozen" => "#2196F3",
                "Bakery" => "#795548",
                "Beverages" => "#9C27B0",
                _ => "#607D8B"
            };
        }

        private int GetCategoryPriority(string category)
        {
            return category switch
            {
                "Produce" => 1,
                "Meat & Seafood" => 2,
                "Dairy & Eggs" => 3,
                "Pantry" => 4,
                "Frozen" => 5,
                "Bakery" => 6,
                "Beverages" => 7,
                _ => 8
            };
        }

        private List<string> GenerateStoreSuggestions(List<ShoppingCategory> categories)
        {
            var suggestions = new List<string>();

            if (categories.Any(c => c.Name == "Produce"))
                suggestions.Add("Start with the produce section for fresh vegetables");
            if (categories.Any(c => c.Name == "Meat & Seafood"))
                suggestions.Add("Visit meat counter for quality proteins");
            if (categories.Any(c => c.Name == "Dairy & Eggs"))
                suggestions.Add("Check dairy section for milk, cheese, and eggs");
            if (categories.Any(c => c.Name == "Pantry"))
                suggestions.Add("Browse pantry aisles for dry goods and spices");

            suggestions.Add("End with frozen foods to keep them cold");
            suggestions.Add("Don't forget to check expiration dates");

            return suggestions;
        }

        private List<string> GenerateShoppingTips(List<ShoppingItem> items, int recipeCount)
        {
            var tips = new List<string>
            {
                $"📝 Shopping for {recipeCount} recipe{(recipeCount > 1 ? "s" : "")}",
                "🕒 Shop early in the morning for best selection",
                "💰 Compare prices and look for sales",
                "📱 Use store apps for digital coupons"
            };

            if (items.Any(i => i.Category == "Meat & Seafood"))
                tips.Add("🥩 Buy meat last to keep it cold");
            if (items.Any(i => i.Category == "Produce"))
                tips.Add("🥬 Choose seasonal vegetables for better taste and price");
            if (items.Count > 10)
                tips.Add("🛒 Consider using a shopping cart for efficiency");

            return tips;
        }

        private TimeSpan CalculateShoppingTime(int itemCount, int categoryCount)
        {
            var baseTime = 5; // 5 minutes base
            var itemTime = itemCount * 0.5; // 30 seconds per item
            var categoryTime = categoryCount * 2; // 2 minutes per category
            var totalMinutes = baseTime + itemTime + categoryTime;
            
            return TimeSpan.FromMinutes(Math.Max(10, totalMinutes)); // Minimum 10 minutes
        }

        private bool IsFrequentlyUsed(string ingredientName)
        {
            var name = ingredientName.ToLower();
            var frequent = new[] { "onion", "garlic", "salt", "pepper", "oil", "butter", "flour", "sugar" };
            return frequent.Any(x => name.Contains(x));
        }

        private string GetDefaultUnit(string ingredientName)
        {
            var name = ingredientName.ToLower();
            if (name.Contains("liquid") || name.Contains("oil") || name.Contains("milk")) return "cup";
            if (name.Contains("spice") || name.Contains("herb")) return "tsp";
            if (name.Contains("vegetable") || name.Contains("fruit")) return "piece";
            return "cup";
        }

        private string CapitalizeFirst(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}
