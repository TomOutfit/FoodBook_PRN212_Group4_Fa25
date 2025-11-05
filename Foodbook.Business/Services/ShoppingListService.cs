using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Foodbook.Data;
using System.Text.Json;

namespace Foodbook.Business.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly FoodbookDbContext _context;
        private readonly IAIService? _aiService;

        public ShoppingListService(FoodbookDbContext context, IAIService? aiService = null)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<ShoppingListResult> GenerateSmartShoppingListAsync(IEnumerable<Recipe> selectedRecipes, int userId)
        {
            var recipes = selectedRecipes.ToList();
            if (!recipes.Any()) 
                return new ShoppingListResult();

            // Step 1: AI Analysis - Phân tích và tổng hợp nguyên liệu thông minh
            var aiAnalysisResult = await PerformAIIngredientAnalysisAsync(recipes);
            
            // Step 2: Truy vấn Nhu cầu - Tổng hợp nguyên liệu từ tất cả recipes
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
                    Category = g.First().Ingredient.Category ?? "Pantry",
                    RecipeCount = g.Count()
                })
                .ToListAsync();

            // Step 3: Truy vấn Kho (Stock) - Lấy tất cả nguyên liệu của user (kèm đơn vị)
            var userStockList = await _context.Ingredients
                .Where(i => i.UserId == userId)
                .Select(i => new
                {
                    Name = i.Name.ToLower(),
                    Quantity = i.Quantity ?? 0,
                    Unit = i.Unit ?? ""
                })
                .ToListAsync();
            var userStock = userStockList
                .GroupBy(x => x.Name)
                .ToDictionary(g => g.Key, g => new { Quantity = g.Sum(x => x.Quantity), Unit = g.First().Unit });

            // Step 4: AI Consolidation - Gộp và tối ưu hóa nguyên liệu
            var consolidatedIngredients = ConsolidateIngredientsWithAI(requiredIngredients, aiAnalysisResult);

            // Step 5: Đối chiếu & Trừ Kho - So sánh nhu cầu với kho hiện có
            var shoppingItems = new List<ShoppingItem>();
            var totalCost = 0m;

            foreach (var ingredient in consolidatedIngredients)
            {
                var neededQuantity = ingredient.TotalQuantity;

                // Kiểm tra kho hiện có với chuyển đổi đơn vị cơ bản
                if (userStock.ContainsKey(ingredient.IngredientName))
                {
                    var stock = userStock[ingredient.IngredientName];
                    var availableInRequiredUnit = ConvertQuantity(stock.Quantity, stock.Unit, ingredient.Unit);
                    neededQuantity = Math.Max(0, ingredient.TotalQuantity - availableInRequiredUnit);
                }

                // Chỉ thêm vào shopping list nếu cần mua
                if (neededQuantity > 0)
                {
                    var item = new ShoppingItem
                    {
                        Name = CapitalizeFirst(ingredient.IngredientName),
                        Quantity = neededQuantity,
                        Unit = ingredient.Unit,
                        Category = ingredient.Category,
                        IsEssential = ingredient.IsEssential,
                        IsOptional = ingredient.IsOptional,
                        EstimatedPrice = CalculateEstimatedPrice(ingredient.IngredientName, neededQuantity),
                        Notes = ingredient.ShoppingNotes,
                        Substitutions = ingredient.Substitutions,
                        RecipeCount = ingredient.RecipeCount,
                        StoreSection = GetStoreSection(ingredient.Category),
                        Priority = GetIngredientPriority(ingredient.IngredientName, ingredient.RecipeCount)
                    };

                    shoppingItems.Add(item);
                    totalCost += item.EstimatedPrice;
                }
            }

            // Step 6: Sắp xếp theo Category và tối ưu hóa
            var categories = OrganizeByCategories(shoppingItems);
            
            // Step 7: Tạo smart suggestions và tips
            var storeSuggestions = await GenerateSmartStoreSuggestionsAsync(categories, recipes);
            var tips = await GenerateSmartShoppingTipsAsync(shoppingItems, recipes);
            var estimatedTime = CalculateShoppingTime(shoppingItems.Count, categories.Count);

            // Step 8: Tối ưu hóa shopping list với AI
            var optimizedList = await OptimizeShoppingListAsync(new ShoppingListResult
            {
                Items = shoppingItems,
                Categories = categories,
                EstimatedCost = totalCost,
                TotalItems = shoppingItems.Count,
                EstimatedShoppingTime = estimatedTime,
                StoreSuggestions = storeSuggestions,
                Tips = tips,
                RecipeNames = recipes.Select(r => r.Title ?? "Unknown Recipe").ToList(),
                GeneratedAt = DateTime.UtcNow
            });

            return optimizedList;
        }

        private decimal ConvertQuantity(decimal quantity, string fromUnit, string toUnit)
        {
            var from = (fromUnit ?? string.Empty).Trim().ToLower();
            var to = (toUnit ?? string.Empty).Trim().ToLower();

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || from == to)
                return quantity;

            // Mass conversions
            if ((from == "kg" && to == "g") || (from == "kilogram" && to == "gram"))
                return quantity * 1000m;
            if ((from == "g" && to == "kg") || (from == "gram" && to == "kilogram"))
                return quantity / 1000m;

            // Volume conversions
            if ((from == "liter" || from == "l") && (to == "ml"))
                return quantity * 1000m;
            if ((from == "ml") && (to == "liter" || to == "l"))
                return quantity / 1000m;

            // Culinary spoons/cups (approximate)
            // 1 cup = 240 ml, 1 tbsp = 15 ml, 1 tsp = 5 ml
            if (IsCupSpoon(from) && IsCupSpoon(to))
            {
                var ml = quantity * UnitToMl(from);
                return ml / UnitToMl(to);
            }

            // Piece/dozen
            if (from == "dozen" && to == "piece") return quantity * 12m;
            if (from == "piece" && to == "dozen") return quantity / 12m;

            // Fallback: no conversion known
            return quantity;
        }

        private bool IsCupSpoon(string unit)
        {
            var u = (unit ?? string.Empty).Trim().ToLower();
            return u == "cup" || u == "tbsp" || u == "tsp";
        }

        private decimal UnitToMl(string unit)
        {
            var u = (unit ?? string.Empty).Trim().ToLower();
            return u switch
            {
                "cup" => 240m,
                "tbsp" => 15m,
                "tsp" => 5m,
                _ => 1m
            };
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
                    optimizedItem.IsBulkPurchase = true;
                    optimizedItem.BulkSavings = item.EstimatedPrice * 0.2m; // 20% savings
                    totalSavings += optimizedItem.BulkSavings;
                }

                // Suggest substitutions for expensive items
                if (item.EstimatedPrice > 10m && item.Substitutions.Any())
                {
                    optimizedItem.Notes += $" Consider: {string.Join(", ", item.Substitutions.Take(2))}";
                }

                // Add nutritional information
                optimizedItem.NutritionalInfo = GetNutritionalInfo(item.Name);

                optimizedItems.Add(optimizedItem);
            }

            shoppingList.Items = optimizedItems;
            shoppingList.EstimatedCost -= totalSavings;
            shoppingList.PotentialSavings = totalSavings;
            shoppingList.IsOptimized = true;
            shoppingList.Tips.Add($"💰 Potential savings: ${totalSavings:F2} with bulk purchases");

            return shoppingList;
        }

        public async Task<string> ExportShoppingListToNotesAsync(ShoppingListResult shoppingList, string listName)
        {
            try
            {
                var notesContent = GenerateNotesContent(shoppingList);
                
                // Simulate Notes API call
                // In real implementation, this would call the actual Notes API
                await Task.Delay(500);
                
                var fileName = $"{listName}_{DateTime.Now:yyyyMMdd_HHmm}.txt";
                Console.WriteLine($"📝 Shopping list exported to Notes: {fileName}");
                Console.WriteLine($"Content preview: {notesContent.Substring(0, Math.Min(200, notesContent.Length))}...");
                
                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to Notes: {ex.Message}");
                throw;
            }
        }

        public async Task<ShoppingListResult> GenerateShoppingListFromMealPlanAsync(IEnumerable<MealPlanItem> mealPlanItems, int userId)
        {
            var items = mealPlanItems.ToList();
            if (!items.Any()) 
                return new ShoppingListResult();

            // Group recipes by date and meal type for better organization
            var recipes = items.Select(mp => mp.Recipe).Distinct().ToList();
            
            // Generate shopping list with meal plan context
            var shoppingList = await GenerateSmartShoppingListAsync(recipes, userId);
            
            // Add meal plan specific information
            shoppingList.ListName = $"Meal Plan Shopping - {DateTime.Now:MMM dd}";
            shoppingList.Tips.AddRange(GenerateMealPlanTips(items));
            
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
                        Priority = GetCategoryPriority(item.Category),
                        StoreSection = GetStoreSection(item.Category),
                        ShoppingOrder = GetShoppingOrder(item.Category)
                    };
                }
                categories[item.Category].Items.Add(item);
            }

            // Calculate category totals and sort items by priority
            foreach (var category in categories.Values)
            {
                category.CategoryTotal = category.Items.Sum(item => item.EstimatedPrice);
                category.Items = category.Items.OrderBy(item => item.Priority).ThenBy(item => item.Name).ToList();
            }

            return categories.Values.OrderBy(c => c.Priority).ToList();
        }

        private string GetShoppingOrder(string category)
        {
            return category switch
            {
                "Produce" => "1. Start here for fresh vegetables",
                "Meat & Seafood" => "2. Visit meat counter",
                "Dairy & Eggs" => "3. Check dairy section",
                "Pantry" => "4. Browse dry goods aisles",
                "Frozen" => "5. End with frozen items",
                "Bakery" => "6. Fresh bread and pastries",
                "Beverages" => "7. Drinks and beverages",
                _ => "Check store layout"
            };
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

        private string GenerateNotesContent(ShoppingListResult shoppingList)
        {
            var content = new System.Text.StringBuilder();
            
            content.AppendLine($"🛒 SMART SHOPPING LIST - {shoppingList.GeneratedAt:dd/MM/yyyy HH:mm}");
            content.AppendLine($"📋 Generated for: {string.Join(", ", shoppingList.RecipeNames)}");
            content.AppendLine($"💰 Estimated Cost: ${shoppingList.EstimatedCost:F2}");
            content.AppendLine($"⏱️ Estimated Time: {shoppingList.EstimatedShoppingTime.TotalMinutes:F0} minutes");
            content.AppendLine($"📦 Total Items: {shoppingList.TotalItems}");
            content.AppendLine();
            
            foreach (var category in shoppingList.Categories)
            {
                content.AppendLine($"🏷️ {category.Name} ({category.Icon}) - {category.StoreSection}");
                content.AppendLine($"   Total: ${category.CategoryTotal:F2} | Items: {category.ItemCount}");
                content.AppendLine();
                
                foreach (var item in category.Items)
                {
                    var priority = item.Priority == 1 ? "🔥" : item.Priority == 2 ? "⭐" : "📝";
                    var bulk = item.IsBulkPurchase ? " (BULK)" : "";
                    var checkedStatus = item.IsChecked ? "✅" : "⬜";
                    
                    content.AppendLine($"   {checkedStatus} {priority} {item.Name} - {item.Quantity} {item.Unit} - ${item.EstimatedPrice:F2}{bulk}");
                    
                    if (!string.IsNullOrEmpty(item.Notes))
                        content.AppendLine($"      💡 {item.Notes}");
                    
                    if (item.Substitutions.Any())
                        content.AppendLine($"      🔄 Alternatives: {string.Join(", ", item.Substitutions)}");
                    
                    if (!string.IsNullOrEmpty(item.NutritionalInfo))
                        content.AppendLine($"      🥗 {item.NutritionalInfo}");
                    
                    content.AppendLine();
                }
                content.AppendLine();
            }
            
            if (shoppingList.StoreSuggestions.Any())
            {
                content.AppendLine("🗺️ STORE NAVIGATION TIPS:");
                foreach (var suggestion in shoppingList.StoreSuggestions)
                {
                    content.AppendLine($"   • {suggestion}");
                }
                content.AppendLine();
            }
            
            if (shoppingList.Tips.Any())
            {
                content.AppendLine("💡 SHOPPING TIPS:");
                foreach (var tip in shoppingList.Tips)
                {
                    content.AppendLine($"   • {tip}");
                }
                content.AppendLine();
            }
            
            if (shoppingList.PotentialSavings > 0)
            {
                content.AppendLine($"💰 POTENTIAL SAVINGS: ${shoppingList.PotentialSavings:F2}");
                content.AppendLine();
            }
            
            content.AppendLine("Generated by FoodBook Smart Shopping List 🍽️");
            
            return content.ToString();
        }

        private List<string> GenerateMealPlanTips(List<MealPlanItem> mealPlanItems)
        {
            var tips = new List<string>
            {
                "📅 Meal plan shopping - organize by meal types",
                "🍽️ Consider meal prep containers for efficiency"
            };

            var mealTypes = mealPlanItems.Select(mp => mp.MealType).Distinct().ToList();
            if (mealTypes.Count > 1)
            {
                tips.Add($"🍴 Shopping for {mealTypes.Count} meal types: {string.Join(", ", mealTypes)}");
            }

            var breakfastItems = mealPlanItems.Count(mp => mp.MealType.ToLower().Contains("breakfast"));
            if (breakfastItems > 0)
            {
                tips.Add("🌅 Don't forget breakfast essentials");
            }

            return tips;
        }

        private string GetNutritionalInfo(string ingredientName)
        {
            var name = ingredientName.ToLower();
            
            if (name.Contains("vegetable") || name.Contains("broccoli") || name.Contains("spinach"))
                return "High in fiber and vitamins";
            if (name.Contains("protein") || name.Contains("chicken") || name.Contains("beef"))
                return "Rich in protein";
            if (name.Contains("fruit") || name.Contains("apple") || name.Contains("banana"))
                return "Natural sugars and vitamins";
            if (name.Contains("dairy") || name.Contains("milk") || name.Contains("cheese"))
                return "Calcium and protein";
            if (name.Contains("grain") || name.Contains("rice") || name.Contains("bread"))
                return "Complex carbohydrates";
            
            return "Essential nutrients";
        }

        // AI-Powered Methods
        private async Task<AIIngredientAnalysis> PerformAIIngredientAnalysisAsync(List<Recipe> recipes)
        {
            if (_aiService == null)
            {
                return new AIIngredientAnalysis(); // Fallback to basic analysis
            }

            try
            {
                var recipeData = recipes.Select(r => new
                {
                    Title = r.Title ?? "Unknown",
                    Ingredients = r.RecipeIngredients?.Select(ri => new
                    {
                        Name = ri.Ingredient?.Name ?? "Unknown",
                        Quantity = ri.Quantity,
                        Unit = ri.Ingredient?.Unit ?? "piece"
                    }).Cast<object>().ToList()
                }).ToList();

                var prompt = $@"
Phân tích các công thức sau và đưa ra gợi ý thông minh cho danh sách mua sắm:

{JsonSerializer.Serialize(recipeData, new JsonSerializerOptions { WriteIndented = true })}

Hãy phân tích và trả về JSON với format:
{{
  ""consolidationSuggestions"": [
    {{
      ""ingredient"": ""tên nguyên liệu"",
      ""suggestedQuantity"": ""số lượng đề xuất"",
      ""reason"": ""lý do"",
      ""bulkPurchase"": true/false
    }}
  ],
  ""substitutionSuggestions"": [
    {{
      ""original"": ""nguyên liệu gốc"",
      ""substitute"": ""thay thế"",
      ""reason"": ""lý do""
    }}
  ],
  ""shoppingTips"": [""tip 1"", ""tip 2""],
  ""storeLayoutOptimization"": [
    {{
      ""category"": ""tên category"",
      ""priority"": 1-10,
      ""suggestedOrder"": ""thứ tự mua sắm""
    }}
  ]
}}";

                var response = await _aiService.AnalyzeNutritionAsync(JsonSerializer.Serialize(recipeData));
                return JsonSerializer.Deserialize<AIIngredientAnalysis>(response) ?? new AIIngredientAnalysis();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Analysis failed: {ex.Message}");
                return new AIIngredientAnalysis();
            }
        }

        private List<ConsolidatedIngredient> ConsolidateIngredientsWithAI(
            IEnumerable<object> requiredIngredients, AIIngredientAnalysis aiAnalysis)
        {
            var consolidated = new List<ConsolidatedIngredient>();

            foreach (var ingredient in requiredIngredients)
            {
                // Use reflection to access properties since we're using object type
                var ingredientName = ingredient.GetType().GetProperty("IngredientName")?.GetValue(ingredient)?.ToString() ?? "";
                var totalQuantity = Convert.ToDecimal(ingredient.GetType().GetProperty("TotalQuantity")?.GetValue(ingredient) ?? 0);
                var unit = ingredient.GetType().GetProperty("Unit")?.GetValue(ingredient)?.ToString() ?? "piece";
                var category = ingredient.GetType().GetProperty("Category")?.GetValue(ingredient)?.ToString() ?? "Pantry";
                var recipeCount = Convert.ToInt32(ingredient.GetType().GetProperty("RecipeCount")?.GetValue(ingredient) ?? 0);

                var consolidatedIngredient = new ConsolidatedIngredient
                {
                    IngredientName = ingredientName,
                    TotalQuantity = totalQuantity,
                    Unit = unit,
                    Category = category,
                    RecipeCount = recipeCount,
                    IsEssential = IsEssentialIngredient(ingredientName),
                    IsOptional = IsOptionalIngredient(ingredientName),
                    ShoppingNotes = GenerateShoppingNotes(ingredientName),
                    Substitutions = GetSubstitutions(ingredientName)
                };

                // Apply AI suggestions
                var aiSuggestion = aiAnalysis.ConsolidationSuggestions
                    .FirstOrDefault(s => s.Ingredient.ToLower() == ingredientName.ToLower());

                if (aiSuggestion != null)
                {
                    consolidatedIngredient.ShoppingNotes += $" | AI: {aiSuggestion.Reason}";
                    if (aiSuggestion.BulkPurchase)
                    {
                        consolidatedIngredient.TotalQuantity = Math.Ceiling(consolidatedIngredient.TotalQuantity * 1.5m);
                    }
                }

                consolidated.Add(consolidatedIngredient);
            }

            return consolidated;
        }

        private async Task<List<string>> GenerateSmartStoreSuggestionsAsync(
            List<ShoppingCategory> categories, List<Recipe> recipes)
        {
            var suggestions = new List<string>();

            // AI-powered store layout suggestions
            if (_aiService != null)
            {
                try
                {
                    var categoryData = categories.Select(c => new
                    {
                        Name = c.Name,
                        ItemCount = c.Items.Count,
                        Priority = c.Priority
                    }).ToList();

                    var prompt = $@"
Dựa trên danh sách mua sắm sau, đưa ra gợi ý tối ưu hóa lộ trình mua sắm trong siêu thị:

Categories: {JsonSerializer.Serialize(categoryData)}

Recipes: {string.Join(", ", recipes.Select(r => r.Title ?? "Unknown"))}

Hãy đưa ra 5-7 gợi ý cụ thể về:
1. Thứ tự mua sắm tối ưu
2. Mẹo tiết kiệm thời gian
3. Lưu ý về bảo quản thực phẩm
4. Gợi ý về chất lượng sản phẩm

Trả về dạng array of strings, mỗi string là một gợi ý cụ thể.";

                    var response = await _aiService.AnalyzeNutritionAsync(JsonSerializer.Serialize(categoryData));
                    var aiSuggestions = JsonSerializer.Deserialize<List<string>>(response) ?? new List<string>();
                    suggestions.AddRange(aiSuggestions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AI Store Suggestions failed: {ex.Message}");
                }
            }

            // Fallback suggestions
            if (!suggestions.Any())
            {
                suggestions.AddRange(GenerateStoreSuggestions(categories));
            }

            return suggestions;
        }

        private async Task<List<string>> GenerateSmartShoppingTipsAsync(
            List<ShoppingItem> items, List<Recipe> recipes)
        {
            var tips = new List<string>();

            if (_aiService != null)
            {
                try
                {
                    var itemData = items.Select(i => new
                    {
                        Name = i.Name,
                        Category = i.Category,
                        Price = i.EstimatedPrice,
                        IsEssential = i.IsEssential
                    }).ToList();

                    var prompt = $@"
Dựa trên danh sách mua sắm và công thức sau, đưa ra các mẹo thông minh:

Shopping Items: {JsonSerializer.Serialize(itemData.Take(10))} // Limit to avoid token limits
Recipes: {string.Join(", ", recipes.Select(r => r.Title ?? "Unknown"))}

Hãy đưa ra 5-6 mẹo cụ thể về:
1. Tiết kiệm chi phí
2. Chọn lựa chất lượng
3. Bảo quản thực phẩm
4. Thay thế nguyên liệu
5. Mẹo mua sắm thông minh

Trả về dạng array of strings, mỗi string là một mẹo cụ thể.";

                    var response = await _aiService.AnalyzeNutritionAsync(JsonSerializer.Serialize(itemData.Take(10)));
                    var aiTips = JsonSerializer.Deserialize<List<string>>(response) ?? new List<string>();
                    tips.AddRange(aiTips);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AI Shopping Tips failed: {ex.Message}");
                }
            }

            // Fallback tips
            if (!tips.Any())
            {
                tips.AddRange(GenerateShoppingTips(items, recipes.Count));
            }

            return tips;
        }

        private string GetStoreSection(string category)
        {
            return category switch
            {
                "Produce" => "Khu vực Rau củ quả",
                "Meat & Seafood" => "Khu vực Thịt & Hải sản",
                "Dairy & Eggs" => "Khu vực Sữa & Trứng",
                "Pantry" => "Khu vực Đồ khô",
                "Frozen" => "Khu vực Đông lạnh",
                "Bakery" => "Khu vực Bánh mì",
                "Beverages" => "Khu vực Đồ uống",
                _ => "Khu vực Tổng hợp"
            };
        }

        private int GetIngredientPriority(string ingredientName, int recipeCount)
        {
            var name = ingredientName.ToLower();
            
            // Essential ingredients get higher priority
            if (IsEssentialIngredient(name)) return 1;
            
            // Ingredients used in multiple recipes get higher priority
            if (recipeCount > 1) return 2;
            
            // Optional ingredients get lower priority
            if (IsOptionalIngredient(name)) return 4;
            
            return 3; // Normal priority
        }

        // Helper classes for AI analysis
        private class AIIngredientAnalysis
        {
            public List<ConsolidationSuggestion> ConsolidationSuggestions { get; set; } = new();
            public List<SubstitutionSuggestion> SubstitutionSuggestions { get; set; } = new();
            public List<string> ShoppingTips { get; set; } = new();
            public List<StoreLayoutOptimization> StoreLayoutOptimization { get; set; } = new();
        }

        private class ConsolidationSuggestion
        {
            public string Ingredient { get; set; } = string.Empty;
            public string SuggestedQuantity { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public bool BulkPurchase { get; set; }
        }

        private class SubstitutionSuggestion
        {
            public string Original { get; set; } = string.Empty;
            public string Substitute { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
        }

        private class StoreLayoutOptimization
        {
            public string Category { get; set; } = string.Empty;
            public int Priority { get; set; }
            public string SuggestedOrder { get; set; } = string.Empty;
        }

        private class ConsolidatedIngredient
        {
            public string IngredientName { get; set; } = string.Empty;
            public decimal TotalQuantity { get; set; }
            public string Unit { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int RecipeCount { get; set; }
            public bool IsEssential { get; set; }
            public bool IsOptional { get; set; }
            public string ShoppingNotes { get; set; } = string.Empty;
            public List<string> Substitutions { get; set; } = new();
        }
    }
}
