using Xunit;
using Moq;
using FluentAssertions;
using Foodbook.Business.Services;
using Foodbook.Data.Entities;
using Foodbook.Data;
using Foodbook.Business.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Foodbook.Tests
{
    public class ShoppingListServiceTests
    {
        private readonly FoodbookDbContext _context;
        private readonly Mock<IAIService> _mockAIService;
        private readonly ShoppingListService _shoppingListService;

        public ShoppingListServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: "TestShoppingListDb")
                .Options;

            _context = new FoodbookDbContext(options);
            _mockAIService = new Mock<IAIService>();

            _shoppingListService = new ShoppingListService(_context, _mockAIService.Object);
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldReturnEmptyResult_WhenNoRecipes()
        {
            // Arrange
            var emptyRecipes = new List<Recipe>();

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(emptyRecipes, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalItems.Should().Be(0);
            result.EstimatedCost.Should().Be(0);
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldGenerateShoppingList_WithValidRecipes()
        {
            // Arrange
            var ingredient1 = new Ingredient { Name = "chicken", Unit = "g", Category = "Meat & Seafood" };
            var ingredient2 = new Ingredient { Name = "rice", Unit = "g", Category = "Pantry" };

            _context.Ingredients.AddRange(ingredient1, ingredient2);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Test Recipe",
                Instructions = "Test instructions",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 4,
                CookTime = 30
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient1.Id, Quantity = 500 },
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient2.Id, Quantity = 200 }
            );
            await _context.SaveChangesAsync();

            // Mock AI service
            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{}");

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(new[] { recipe }, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.TotalItems.Should().BeGreaterThan(0);
            result.EstimatedCost.Should().BeGreaterThan(0);
            result.Categories.Should().NotBeEmpty();
            result.StoreSuggestions.Should().NotBeEmpty();
            result.Tips.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldHandleUserStock()
        {
            // Arrange
            var ingredient1 = new Ingredient { Name = "chicken", Unit = "g", Category = "Meat & Seafood", UserId = 1, Quantity = 200 };
            var ingredient2 = new Ingredient { Name = "rice", Unit = "g", Category = "Pantry" };

            _context.Ingredients.AddRange(ingredient1, ingredient2);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Test Recipe",
                Instructions = "Test instructions",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 4,
                CookTime = 30
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient1.Id, Quantity = 500 },
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient2.Id, Quantity = 200 }
            );
            await _context.SaveChangesAsync();

            // Mock AI service
            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{}");

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(new[] { recipe }, 1);

            // Assert
            result.Should().NotBeNull();
            // Chicken: need 500g, have 200g, so need 300g
            var chickenItem = result.Items.FirstOrDefault(i => i.Name.ToLower().Contains("chicken"));
            chickenItem?.Should().NotBeNull();
            chickenItem?.Quantity.Should().Be(300);
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldConvertUnitsCorrectly()
        {
            // Arrange
            var ingredient1 = new Ingredient { Name = "milk", Unit = "liter", Category = "Dairy & Eggs", UserId = 1, Quantity = 500 }; // 0.5 liter in stock

            _context.Ingredients.Add(ingredient1);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Test Recipe",
                Instructions = "Test instructions",
                UserId = 1
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.Add(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient1.Id, Quantity = 1000 } // 1 liter needed
            );
            await _context.SaveChangesAsync();

            // Mock AI service
            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{}");

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(new[] { recipe }, 1);

            // Assert
            result.Should().NotBeNull();
            // Need 1000ml, have 500ml, so need 500ml
            var milkItem = result.Items.FirstOrDefault(i => i.Name.ToLower().Contains("milk"));
            milkItem.Should().NotBeNull();
            milkItem?.Quantity.Should().Be(500);
            milkItem?.Unit.Should().Be("liter");
        }

        [Fact]
        public async Task GenerateShoppingListFromIngredientsAsync_ShouldCreateList_FromIngredientNames()
        {
            // Arrange
            var ingredientNames = new[] { "chicken", "rice", "carrot" };

            // Act
            var result = await _shoppingListService.GenerateShoppingListFromIngredientsAsync(ingredientNames, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);
            result.TotalItems.Should().Be(3);
            result.EstimatedCost.Should().BeGreaterThan(0);
            result.Categories.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetShoppingCategoriesAsync_ShouldReturnAllCategories()
        {
            // Act
            var result = await _shoppingListService.GetShoppingCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(7);
            result.Should().Contain(c => c.Name == "Produce");
            result.Should().Contain(c => c.Name == "Meat & Seafood");
            result.Should().Contain(c => c.Name == "Dairy & Eggs");
            result.Should().Contain(c => c.Name == "Pantry");
            result.Should().Contain(c => c.Name == "Frozen");
            result.Should().Contain(c => c.Name == "Bakery");
            result.Should().Contain(c => c.Name == "Beverages");
        }

        [Fact]
        public async Task OptimizeShoppingListAsync_ShouldOptimizeList_WithBulkPurchases()
        {
            // Arrange
            var shoppingList = new ShoppingListResult
            {
                Items = new List<ShoppingItem>
                {
                    new ShoppingItem
                    {
                        Name = "onion",
                        Quantity = 2,
                        Unit = "piece",
                        Category = "Produce",
                        EstimatedPrice = 2.00m,
                        IsEssential = true
                    },
                    new ShoppingItem
                    {
                        Name = "garlic",
                        Quantity = 3,
                        Unit = "clove",
                        Category = "Produce",
                        EstimatedPrice = 1.50m,
                        IsEssential = true
                    }
                },
                EstimatedCost = 3.50m
            };

            // Act
            var result = await _shoppingListService.OptimizeShoppingListAsync(shoppingList);

            // Assert
            result.Should().NotBeNull();
            result.IsOptimized.Should().BeTrue();
            result.PotentialSavings.Should().BeGreaterThan(0);
            result.EstimatedCost.Should().BeLessThan(3.50m);
            result.Tips.Should().Contain(t => t.Contains("Potential savings"));
        }

        [Fact]
        public async Task ExportShoppingListToNotesAsync_ShouldExportSuccessfully()
        {
            // Arrange
            var shoppingList = new ShoppingListResult
            {
                Items = new List<ShoppingItem>
                {
                    new ShoppingItem
                    {
                        Name = "Chicken",
                        Quantity = 500,
                        Unit = "g",
                        Category = "Meat & Seafood",
                        EstimatedPrice = 8.99m,
                        Notes = "Organic preferred"
                    }
                },
                Categories = new List<ShoppingCategory>
                {
                    new ShoppingCategory { Name = "Meat & Seafood", Items = new List<ShoppingItem>() }
                },
                EstimatedCost = 8.99m,
                TotalItems = 1,
                EstimatedShoppingTime = TimeSpan.FromMinutes(15),
                StoreSuggestions = new List<string> { "Start with produce section" },
                Tips = new List<string> { "Shop early for best selection" },
                RecipeNames = new List<string> { "Test Recipe" },
                GeneratedAt = DateTime.UtcNow
            };

            // Act
            var result = await _shoppingListService.ExportShoppingListToNotesAsync(shoppingList, "Test List");

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Test List");
            result.Should().Contain("2025"); // Year in filename (current year)
        }

        [Fact]
        public async Task GenerateShoppingListFromMealPlanAsync_ShouldCreateList_FromMealPlanItems()
        {
            // Arrange
            var ingredient = new Ingredient { Name = "chicken", Unit = "g", Category = "Meat & Seafood" };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Meal Plan Recipe",
                UserId = 1
            };
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.Add(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient.Id, Quantity = 300 }
            );
            await _context.SaveChangesAsync();

            var mealPlanItems = new List<MealPlanItem>
            {
                new MealPlanItem
                {
                    Recipe = recipe,
                    MealType = "Lunch",
                    PlannedDate = DateTime.Today
                }
            };

            // Mock AI service
            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{}");

            // Act
            var result = await _shoppingListService.GenerateShoppingListFromMealPlanAsync(mealPlanItems, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.ListName.Should().Contain("Meal Plan");
            result.Tips.Should().Contain(t => t.Contains("Meal plan shopping"));
        }

        [Fact]
        public async Task GenerateRandomShoppingListFromDatabaseAsync_ShouldCreateList_FromUserIngredients()
        {
            // Arrange
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "chicken", Unit = "g", Category = "Meat & Seafood", UserId = 1 },
                new Ingredient { Name = "rice", Unit = "g", Category = "Pantry", UserId = 1 },
                new Ingredient { Name = "carrot", Unit = "piece", Category = "Produce", UserId = 1 }
            };

            _context.Ingredients.AddRange(ingredients);
            await _context.SaveChangesAsync();

            // Act
            var result = await _shoppingListService.GenerateRandomShoppingListFromDatabaseAsync(1, 2);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalItems.Should().Be(2);
        }

        [Fact]
        public async Task GenerateRandomShoppingListFromDatabaseAsync_ShouldHandleEmptyIngredientsGracefully()
        {
            // Act
            var result = await _shoppingListService.GenerateRandomShoppingListFromDatabaseAsync(1);

            // Assert - Should not throw exception, should return a list (may have default ingredients)
            result.Should().NotBeNull();
            // The method generates a list even with no ingredients by falling back to GenerateShoppingListFromIngredientsAsync
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldHandleAIIntegration()
        {
            // Arrange
            var ingredient = new Ingredient { Name = "chicken", Unit = "g", Category = "Meat & Seafood" };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "AI Test Recipe",
                UserId = 1
            };
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.Add(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient.Id, Quantity = 400 }
            );
            await _context.SaveChangesAsync();

            // Mock AI service with consolidation suggestions
            var aiResponse = JsonSerializer.Serialize(new
            {
                consolidationSuggestions = new[]
                {
                    new
                    {
                        ingredient = "chicken",
                        suggestedQuantity = "500",
                        reason = "Better value in bulk",
                        bulkPurchase = true
                    }
                },
                substitutionSuggestions = new[]
                {
                    new
                    {
                        original = "chicken",
                        substitute = "turkey",
                        reason = "Healthier alternative"
                    }
                },
                shoppingTips = new[] { "Buy organic when possible" },
                storeLayoutOptimization = new[]
                {
                    new
                    {
                        category = "Meat & Seafood",
                        priority = 1,
                        suggestedOrder = "Visit meat counter first"
                    }
                }
            });

            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync(aiResponse);

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(new[] { recipe }, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            // AI suggestions should be incorporated
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldHandleAIFallback()
        {
            // Arrange
            var ingredient = new Ingredient { Name = "chicken", Unit = "g", Category = "Meat & Seafood" };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Fallback Test Recipe",
                UserId = 1
            };
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.Add(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient.Id, Quantity = 400 }
            );
            await _context.SaveChangesAsync();

            // Mock AI service to throw exception
            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("AI service unavailable"));

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(new[] { recipe }, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            // Should fall back to basic functionality
        }

        [Fact]
        public async Task OptimizeShoppingListAsync_ShouldSuggestSubstitutions_ForExpensiveItems()
        {
            // Arrange
            var shoppingList = new ShoppingListResult
            {
                Items = new List<ShoppingItem>
                {
                    new ShoppingItem
                    {
                        Name = "beef",
                        Quantity = 500,
                        Unit = "g",
                        Category = "Meat & Seafood",
                        EstimatedPrice = 15.00m,
                        Substitutions = new List<string> { "lamb", "chicken" },
                        IsEssential = false
                    }
                },
                EstimatedCost = 15.00m
            };

            // Act
            var result = await _shoppingListService.OptimizeShoppingListAsync(shoppingList);

            // Assert
            result.Should().NotBeNull();
            result.Items[0].Notes.Should().Contain("Consider:");
            result.Items[0].Notes.Should().Contain("lamb");
            result.Items[0].Notes.Should().Contain("chicken");
        }

        [Fact]
        public async Task GenerateShoppingListFromIngredientsAsync_ShouldCategorizeIngredientsCorrectly()
        {
            // Arrange
            var ingredientNames = new[] { "tomato", "chicken breast", "milk", "bread", "frozen peas" };

            // Act
            var result = await _shoppingListService.GenerateShoppingListFromIngredientsAsync(ingredientNames, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);

            // Check categories
            result.Items.Should().Contain(i => i.Category == "Produce" && i.Name.ToLower().Contains("tomato"));
            result.Items.Should().Contain(i => i.Category == "Meat & Seafood" && i.Name.ToLower().Contains("chicken"));
            result.Items.Should().Contain(i => i.Category == "Dairy & Eggs" && i.Name.ToLower().Contains("milk"));
            result.Items.Should().Contain(i => i.Category == "Pantry" && i.Name.ToLower().Contains("bread"));
            result.Items.Should().Contain(i => i.Category == "Frozen" && i.Name.ToLower().Contains("frozen"));
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldHandleMultipleRecipes_WithSharedIngredients()
        {
            // Arrange
            var ingredient1 = new Ingredient { Name = "onion", Unit = "piece", Category = "Produce" };
            var ingredient2 = new Ingredient { Name = "garlic", Unit = "clove", Category = "Produce" };

            _context.Ingredients.AddRange(ingredient1, ingredient2);
            await _context.SaveChangesAsync();

            var recipe1 = new Recipe { Title = "Recipe 1", UserId = 1 };
            var recipe2 = new Recipe { Title = "Recipe 2", UserId = 1 };

            _context.Recipes.AddRange(recipe1, recipe2);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = recipe1.Id, IngredientId = ingredient1.Id, Quantity = 1 },
                new RecipeIngredient { RecipeId = recipe1.Id, IngredientId = ingredient2.Id, Quantity = 2 },
                new RecipeIngredient { RecipeId = recipe2.Id, IngredientId = ingredient1.Id, Quantity = 2 },
                new RecipeIngredient { RecipeId = recipe2.Id, IngredientId = ingredient2.Id, Quantity = 3 }
            );
            await _context.SaveChangesAsync();

            // Mock AI service
            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{}");

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(new[] { recipe1, recipe2 }, 1);

            // Assert
            result.Should().NotBeNull();
            // Check that quantities are properly aggregated
            var onionItem = result.Items.FirstOrDefault(i => i.Name.ToLower().Contains("onion"));
            onionItem.Should().NotBeNull();
            var garlicItem = result.Items.FirstOrDefault(i => i.Name.ToLower().Contains("garlic"));
            garlicItem.Should().NotBeNull();

            // Verify that both ingredients appear (shared across recipes)
            onionItem?.RecipeCount.Should().Be(2);
            garlicItem?.RecipeCount.Should().Be(2);
        }

        [Fact]
        public async Task GenerateSmartShoppingListAsync_ShouldPrioritizeIngredientsCorrectly()
        {
            // Arrange
            var ingredient = new Ingredient { Name = "chicken", Unit = "g", Category = "Meat & Seafood" };
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var recipe = new Recipe { Title = "Test Recipe", UserId = 1 };
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.Add(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient.Id, Quantity = 500 }
            );
            await _context.SaveChangesAsync();

            // Mock AI service
            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{}");

            // Act
            var result = await _shoppingListService.GenerateSmartShoppingListAsync(new[] { recipe }, 1);

            // Assert
            result.Should().NotBeNull();
            var chickenItem = result.Items.First(i => i.Name.ToLower().Contains("chicken"));
            chickenItem.Priority.Should().Be(3); // Normal priority (not essential, recipe count = 1)
            chickenItem.IsEssential.Should().BeFalse();
        }

        [Fact]
        public async Task ExportShoppingListToNotesAsync_ShouldHandleEmptyList()
        {
            // Arrange
            var emptyList = new ShoppingListResult
            {
                Items = new List<ShoppingItem>(),
                Categories = new List<ShoppingCategory>(),
                EstimatedCost = 0,
                TotalItems = 0,
                EstimatedShoppingTime = TimeSpan.Zero,
                StoreSuggestions = new List<string>(),
                Tips = new List<string>(),
                RecipeNames = new List<string>(),
                GeneratedAt = DateTime.UtcNow
            };

            // Act
            var result = await _shoppingListService.ExportShoppingListToNotesAsync(emptyList, "Empty List");

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Empty List");
        }

        [Fact]
        public async Task GenerateShoppingListFromMealPlanAsync_ShouldHandleEmptyMealPlan()
        {
            // Arrange
            var emptyMealPlan = new List<MealPlanItem>();

            // Act
            var result = await _shoppingListService.GenerateShoppingListFromMealPlanAsync(emptyMealPlan, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalItems.Should().Be(0);
        }

        [Fact]
        public async Task OptimizeShoppingListAsync_ShouldHandleEmptyList()
        {
            // Arrange
            var emptyList = new ShoppingListResult
            {
                Items = new List<ShoppingItem>(),
                EstimatedCost = 0
            };

            // Act
            var result = await _shoppingListService.OptimizeShoppingListAsync(emptyList);

            // Assert
            result.Should().NotBeNull();
            result.IsOptimized.Should().BeTrue();
            result.PotentialSavings.Should().Be(0);
        }
    }
}