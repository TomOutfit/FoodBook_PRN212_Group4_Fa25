using Xunit;
using Moq;
using FluentAssertions;
using Foodbook.Business.Services;
using Foodbook.Data.Entities;
using Foodbook.Data;
using Foodbook.Business.Interfaces;
using Foodbook.Business.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodbook.Tests
{
    public class NutritionServiceTests
    {
        private readonly FoodbookDbContext _context;
        private readonly Mock<IAIService> _mockAIService;
        private readonly NutritionService _nutritionService;

        public NutritionServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: "TestNutritionDb")
                .Options;

            _context = new FoodbookDbContext(options);
            _mockAIService = new Mock<IAIService>();

            _nutritionService = new NutritionService(_context, _mockAIService.Object);
        }

        [Fact]
        public async Task AnalyzeRecipeNutritionAsync_ShouldCalculateNutrition_WhenRecipeHasValidIngredients()
        {
            // Arrange - Setup test data in in-memory database
            var ingredient1 = new Ingredient { Name = "chicken", Unit = "g" };
            var ingredient2 = new Ingredient { Name = "rice", Unit = "g" };

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

            var recipeIngredient1 = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                IngredientId = ingredient1.Id,
                Quantity = 150
            };

            var recipeIngredient2 = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                IngredientId = ingredient2.Id,
                Quantity = 100
            };

            _context.RecipeIngredients.AddRange(recipeIngredient1, recipeIngredient2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _nutritionService.AnalyzeRecipeNutritionAsync(recipe);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.TotalProtein.Should().BeGreaterThan(0);
            result.TotalCarbs.Should().BeGreaterThan(0);
            result.TotalFat.Should().BeGreaterThan(0);
            result.Rating.Should().NotBeNull();
            result.Alerts.Should().NotBeNull();
            result.Recommendations.Should().NotBeNull();
            result.AnalysisSummary.Should().NotBeNull();
        }

        [Fact]
        public async Task AnalyzeRecipeNutritionAsync_ShouldHandleEmptyIngredients()
        {
            // Arrange
            var recipe = new Recipe
            {
                Title = "Empty Recipe",
                Instructions = "No ingredients",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 1,
                CookTime = 0
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            // Act
            var result = await _nutritionService.AnalyzeRecipeNutritionAsync(recipe);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().Be(0);
            result.TotalProtein.Should().Be(0);
            result.TotalCarbs.Should().Be(0);
            result.TotalFat.Should().Be(0);
        }

        [Fact]
        public async Task GetNutritionRecommendationsAsync_ShouldReturnRecommendations_ForWeightLoss()
        {
            // Arrange
            var analysis = new NutritionAnalysisResult
            {
                TotalCalories = 800,
                TotalProtein = 20,
                TotalCarbs = 100,
                TotalFat = 30,
                TotalFiber = 15,
                TotalSodium = 1500,
                TotalSugar = 50,
                TotalCholesterol = 200,
                TotalSaturatedFat = 10,
                TotalTransFat = 0
            };

            // Act
            var result = await _nutritionService.GetNutritionRecommendationsAsync(analysis, "weight loss");

            // Assert
            result.Should().NotBeNull();
            result.Goal.Should().Be("weight loss");
            result.Suggestions.Should().NotBeEmpty();
            result.FoodsToAdd.Should().NotBeEmpty();
            result.FoodsToReduce.Should().NotBeEmpty();
            result.MealTiming.Should().NotBeNull();
            result.Hydration.Should().NotBeNull();
        }

        [Fact]
        public async Task GetNutritionRecommendationsAsync_ShouldReturnRecommendations_ForMuscleGain()
        {
            // Arrange
            var analysis = new NutritionAnalysisResult
            {
                TotalCalories = 2500,
                TotalProtein = 150,
                TotalCarbs = 300,
                TotalFat = 80,
                TotalFiber = 40,
                TotalSodium = 3000,
                TotalSugar = 100,
                TotalCholesterol = 400,
                TotalSaturatedFat = 25,
                TotalTransFat = 1
            };

            // Act
            var result = await _nutritionService.GetNutritionRecommendationsAsync(analysis, "muscle gain");

            // Assert
            result.Should().NotBeNull();
            result.Goal.Should().Be("muscle gain");
            result.Suggestions.Should().Contain(s => s.Contains("protein"));
            result.FoodsToAdd.Should().Contain("Lean meats, eggs, dairy, legumes, nuts");
        }

        [Fact]
        public async Task GetNutritionRecommendationsAsync_ShouldReturnRecommendations_ForHeartHealth()
        {
            // Arrange
            var analysis = new NutritionAnalysisResult
            {
                TotalCalories = 2000,
                TotalProtein = 100,
                TotalCarbs = 250,
                TotalFat = 70,
                TotalFiber = 30,
                TotalSodium = 1500,
                TotalSugar = 50,
                TotalCholesterol = 300,
                TotalSaturatedFat = 15,
                TotalTransFat = 0
            };

            // Act
            var result = await _nutritionService.GetNutritionRecommendationsAsync(analysis, "heart health");

            // Assert
            result.Should().NotBeNull();
            result.Goal.Should().Be("heart health");
            result.Suggestions.Should().Contain("Reduce sodium intake to less than 2300mg per day");
            result.Suggestions.Should().Contain("Increase omega-3 fatty acids");
            result.FoodsToAdd.Should().Contain("Fatty fish, nuts, seeds, olive oil, vegetables");
            result.FoodsToReduce.Should().Contain("Red meat, processed foods, trans fats");
        }

        [Fact]
        public async Task GetNutritionRecommendationsAsync_ShouldReturnRecommendations_ForDiabetesManagement()
        {
            // Arrange
            var analysis = new NutritionAnalysisResult
            {
                TotalCalories = 1800,
                TotalProtein = 90,
                TotalCarbs = 200,
                TotalFat = 60,
                TotalFiber = 35,
                TotalSodium = 1200,
                TotalSugar = 40,
                TotalCholesterol = 250,
                TotalSaturatedFat = 12,
                TotalTransFat = 0
            };

            // Act
            var result = await _nutritionService.GetNutritionRecommendationsAsync(analysis, "diabetes management");

            // Assert
            result.Should().NotBeNull();
            result.Goal.Should().Be("diabetes management");
            result.Suggestions.Should().Contain("Control carbohydrate portions and timing");
            result.Suggestions.Should().Contain("Choose low glycemic index foods");
            result.FoodsToAdd.Should().Contain("Non-starchy vegetables, whole grains, lean proteins");
            result.FoodsToReduce.Should().Contain("Sugary foods, refined grains, sweetened beverages");
        }

        [Fact]
        public async Task GetHealthAlertsAsync_ShouldReturnAlerts_ForHighSodium()
        {
            // Arrange
            var analysis = new NutritionAnalysisResult
            {
                TotalSodium = 2500,
                TotalFiber = 30
            };

            // Act
            var alerts = await _nutritionService.GetHealthAlertsAsync(analysis);

            // Assert
            alerts.Should().NotBeEmpty();
            alerts.Should().Contain(a => a.Message.Contains("High sodium"));
            alerts.Should().Contain(a => a.Type == "Warning");
        }

        [Fact]
        public async Task GetHealthAlertsAsync_ShouldReturnAlerts_ForHighSaturatedFat()
        {
            // Arrange
            var analysis = new NutritionAnalysisResult
            {
                TotalSodium = 1800,
                TotalFiber = 30,
                TotalSaturatedFat = 25
            };

            // Act
            var alerts = await _nutritionService.GetHealthAlertsAsync(analysis);

            // Assert
            alerts.Should().NotBeEmpty();
            alerts.Should().Contain(a => a.Message.Contains("High saturated fat"));
            alerts.Should().Contain(a => a.Type == "Warning");
            alerts.Should().Contain(a => a.Icon == "❤️");
        }

        [Fact]
        public async Task GetHealthAlertsAsync_ShouldReturnAlerts_ForGoodProtein()
        {
            // Arrange
            var analysis = new NutritionAnalysisResult
            {
                TotalSodium = 1800,
                TotalFiber = 30,
                TotalProtein = 55
            };

            // Act
            var alerts = await _nutritionService.GetHealthAlertsAsync(analysis);

            // Assert
            alerts.Should().NotBeEmpty();
            alerts.Should().Contain(a => a.Message.Contains("Good protein content"));
            alerts.Should().Contain(a => a.Type == "Success");
            alerts.Should().Contain(a => a.Icon == "💪");
        }

        [Fact]
        public async Task AnalyzeUnstructuredRecipeAsync_ShouldReturnNutrition_ForValidText()
        {
            // Arrange
            var recipeText = "1 kg thịt bò, 2 củ hành tây, 100g muối";

            // Act
            var result = await _nutritionService.AnalyzeUnstructuredRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.Rating.Should().NotBeNull();
            result.AnalysisSummary.Should().NotBeNull();
        }

        [Fact]
        public async Task AnalyzeUnstructuredRecipeAsync_ShouldHandleSoySauce()
        {
            // Arrange
            var recipeText = "500g thịt gà, 50ml nước tương, 1 củ hành tây";

            // Act
            var result = await _nutritionService.AnalyzeUnstructuredRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.Rating.Should().NotBeNull();
        }

        [Fact]
        public async Task AnalyzeUnstructuredRecipeAsync_ShouldHandleFallbackWhenNoIngredientsDetected()
        {
            // Arrange
            var recipeText = "Some random text without ingredients";

            // Act
            var result = await _nutritionService.AnalyzeUnstructuredRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0); // Should have fallback ingredients
            result.Rating.Should().NotBeNull();
        }

        [Fact]
        public async Task CompareNutritionAsync_ShouldCompareTwoRecipes()
        {
            // Arrange
            var ingredient1 = new Ingredient { Name = "chicken", Unit = "g" };
            var ingredient2 = new Ingredient { Name = "rice", Unit = "g" };

            _context.Ingredients.AddRange(ingredient1, ingredient2);
            await _context.SaveChangesAsync();

            var recipe1 = new Recipe
            {
                Id = 1,
                Title = "High Protein Recipe",
                RecipeIngredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient
                    {
                        RecipeId = 1,
                        IngredientId = ingredient1.Id,
                        Ingredient = ingredient1,
                        Quantity = 200
                    }
                }
            };

            var recipe2 = new Recipe
            {
                Id = 2,
                Title = "Low Protein Recipe",
                RecipeIngredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient
                    {
                        RecipeId = 2,
                        IngredientId = ingredient2.Id,
                        Ingredient = ingredient2,
                        Quantity = 200
                    }
                }
            };

            // Act
            var comparison = await _nutritionService.CompareNutritionAsync(recipe1, recipe2);

            // Assert
            comparison.Should().NotBeNull();
            comparison.Recipe1.Should().Be(recipe1);
            comparison.Recipe2.Should().Be(recipe2);
            comparison.Nutrition1.Should().NotBeNull();
            comparison.Nutrition2.Should().NotBeNull();
            comparison.Comparisons.Should().NotBeEmpty();
            comparison.Winner.Should().NotBeNull();
            comparison.Summary.Should().NotBeNull();
        }

        [Fact]
        public async Task AnalyzeMealPlanNutritionAsync_ShouldAggregateNutrition_FromMultipleRecipes()
        {
            // Arrange
            var ingredient1 = new Ingredient { Name = "chicken", Unit = "g" };
            var ingredient2 = new Ingredient { Name = "rice", Unit = "g" };
            var ingredient3 = new Ingredient { Name = "beef", Unit = "g" };

            _context.Ingredients.AddRange(ingredient1, ingredient2, ingredient3);
            await _context.SaveChangesAsync();

            var recipe1 = new Recipe
            {
                Title = "Breakfast Recipe",
                Instructions = "Cook chicken",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 1,
                CookTime = 15
            };

            var recipe2 = new Recipe
            {
                Title = "Lunch Recipe",
                Instructions = "Cook rice",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 1,
                CookTime = 20
            };

            var recipe3 = new Recipe
            {
                Title = "Dinner Recipe",
                Instructions = "Cook beef",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 1,
                CookTime = 25
            };

            _context.Recipes.AddRange(recipe1, recipe2, recipe3);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = recipe1.Id, IngredientId = ingredient1.Id, Quantity = 100 },
                new RecipeIngredient { RecipeId = recipe2.Id, IngredientId = ingredient2.Id, Quantity = 100 },
                new RecipeIngredient { RecipeId = recipe3.Id, IngredientId = ingredient3.Id, Quantity = 100 }
            );
            await _context.SaveChangesAsync();

            var recipes = new List<Recipe> { recipe1, recipe2, recipe3 };

            // Act
            var result = await _nutritionService.AnalyzeMealPlanNutritionAsync(recipes);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.TotalProtein.Should().BeGreaterThan(0);
            result.TotalCarbs.Should().BeGreaterThan(0);
            result.TotalFat.Should().BeGreaterThan(0);
            result.Rating.Should().NotBeNull();
            result.Alerts.Should().NotBeNull();
            result.Recommendations.Should().NotBeNull();
            result.AnalysisSummary.Should().NotBeNull();
        }

        [Fact]
        public async Task ParseIngredientsWithAIAsync_ShouldReturnIngredients_WhenValidRecipeText()
        {
            // Arrange
            var recipeText = "2 chicken breasts, 1 cup rice, 2 carrots";

            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{\"ingredients\":[{\"name\":\"chicken\",\"quantity\":2,\"unit\":\"piece\"},{\"name\":\"rice\",\"quantity\":1,\"unit\":\"cup\"},{\"name\":\"carrot\",\"quantity\":2,\"unit\":\"piece\"}]}");

            // Act
            var result = await _nutritionService.ParseIngredientsWithAIAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().Contain(i => i.Name.Contains("chicken"));
        }

        [Fact]
        public async Task CalculateNutritionFromIngredientsAsync_ShouldCalculateNutrition_WhenValidIngredients()
        {
            // Arrange
            var ingredients = new List<IngredientDto>
            {
                new IngredientDto { Name = "chicken", Quantity = 200, Unit = "g" },
                new IngredientDto { Name = "rice", Quantity = 100, Unit = "g" }
            };

            // Mock nutritional values (since SetNutritionalValues is private)
            foreach (var ingredient in ingredients)
            {
                if (ingredient.Name.Contains("chicken"))
                {
                    ingredient.CaloriesPerUnit = 165;
                    ingredient.ProteinPerUnit = 31;
                    ingredient.CarbohydratesPerUnit = 0;
                    ingredient.FatPerUnit = 3.6;
                    ingredient.FiberPerUnit = 0;
                    ingredient.SugarPerUnit = 0;
                    ingredient.SodiumPerUnit = 74;
                }
                else if (ingredient.Name.Contains("rice"))
                {
                    ingredient.CaloriesPerUnit = 130;
                    ingredient.ProteinPerUnit = 2.7;
                    ingredient.CarbohydratesPerUnit = 28;
                    ingredient.FatPerUnit = 0.3;
                    ingredient.FiberPerUnit = 0.4;
                    ingredient.SugarPerUnit = 0.1;
                    ingredient.SodiumPerUnit = 1;
                }
            }

            // Act
            var result = await _nutritionService.CalculateNutritionFromIngredientsAsync(ingredients);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.TotalProtein.Should().BeGreaterThan(0);
            result.Rating.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateHealthFeedbackAsync_ShouldReturnFeedback_WhenValidNutrition()
        {
            // Arrange
            var nutrition = new NutritionAnalysisResult
            {
                TotalCalories = 500,
                TotalProtein = 25,
                TotalCarbs = 50,
                TotalFat = 20,
                TotalFiber = 10,
                TotalSodium = 800
            };

            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("Great nutrition profile with balanced macronutrients!");

            // Act
            var result = await _nutritionService.GenerateHealthFeedbackAsync(nutrition);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AnalyzeCustomRecipeAsync_ShouldReturnNutrition_WhenValidRecipeText()
        {
            // Arrange
            var recipeText = "500g chicken, 200g rice";

            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ReturnsAsync("{\"ingredients\":[{\"name\":\"chicken\",\"quantity\":500,\"unit\":\"g\"},{\"name\":\"rice\",\"quantity\":200,\"unit\":\"g\"}]}");

            // Act
            var result = await _nutritionService.AnalyzeCustomRecipeAsync(recipeText, "weight loss");

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.AnalysisSummary.Should().NotBeNull();
        }

        [Fact]
        public async Task CalculateIngredientNutrition_ShouldCalculateVitaminsCorrectly()
        {
            // This tests the vitamin calculation logic indirectly through AnalyzeRecipeNutritionAsync
            // Arrange
            var ingredient1 = new Ingredient { Name = "carrot", Unit = "g" }; // Contains Vitamin A and C
            var ingredient2 = new Ingredient { Name = "broccoli", Unit = "g" }; // Contains Vitamin C and K

            _context.Ingredients.AddRange(ingredient1, ingredient2);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Vitamin Rich Recipe",
                Instructions = "Mix ingredients",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 1,
                CookTime = 10
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient1.Id, Quantity = 100 },
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient2.Id, Quantity = 100 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _nutritionService.AnalyzeRecipeNutritionAsync(recipe);

            // Assert
            result.Should().NotBeNull();
            result.Vitamins.Should().NotBeNull();
            result.Vitamins.Should().HaveCount(5); // 5 vitamins: A, C, D, E, K
            result.Vitamins.First(v => v.Name.Contains("Vitamin A")).Amount.Should().BeGreaterThan(0);
            result.Vitamins.First(v => v.Name.Contains("Vitamin C")).Amount.Should().BeGreaterThan(30); // Both ingredients contribute
            result.Vitamins.First(v => v.Name.Contains("Vitamin K")).Amount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CalculateIngredientNutrition_ShouldCalculateMineralsCorrectly()
        {
            // This tests the mineral calculation logic indirectly through AnalyzeRecipeNutritionAsync
            // Arrange
            var ingredient1 = new Ingredient { Name = "cheese", Unit = "g" }; // Contains Calcium
            var ingredient2 = new Ingredient { Name = "spinach", Unit = "g" }; // Contains Iron, Magnesium, Potassium

            _context.Ingredients.AddRange(ingredient1, ingredient2);
            await _context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Mineral Rich Recipe",
                Instructions = "Mix ingredients",
                UserId = 1,
                Difficulty = "Easy",
                Servings = 1,
                CookTime = 10
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            _context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient1.Id, Quantity = 50 },
                new RecipeIngredient { RecipeId = recipe.Id, IngredientId = ingredient2.Id, Quantity = 100 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _nutritionService.AnalyzeRecipeNutritionAsync(recipe);

            // Assert
            result.Should().NotBeNull();
            result.Minerals.Should().NotBeNull();
            result.Minerals.Should().HaveCount(5); // 5 minerals: Calcium, Iron, Magnesium, Potassium, Zinc
            result.Minerals.First(m => m.Name.Contains("Calcium")).Amount.Should().BeGreaterThan(0);
            result.Minerals.First(m => m.Name.Contains("Iron")).Amount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AnalyzeUnstructuredRecipeAsync_ShouldHandleEggAndFishForVitaminD()
        {
            // Arrange
            var recipeText = "200g cá hồi, 2 quả trứng";

            // Act
            var result = await _nutritionService.AnalyzeUnstructuredRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.Vitamins.Should().NotBeNull();
            // Vitamin D should be present from fish and eggs
            result.Vitamins.First(v => v.Name.Contains("Vitamin D")).Amount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AnalyzeUnstructuredRecipeAsync_ShouldHandleOilAndNutsForVitaminE()
        {
            // Arrange
            var recipeText = "50ml dầu olive, 50g hạt điều";

            // Act
            var result = await _nutritionService.AnalyzeUnstructuredRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.Vitamins.Should().NotBeNull();
            // Vitamin E should be present from oil and nuts
            result.Vitamins.First(v => v.Name.Contains("Vitamin E")).Amount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AnalyzeUnstructuredRecipeAsync_ShouldHandleHighSodium()
        {
            // Arrange
            var recipeText = "300g thịt bò, 200g muối";

            // Act
            var result = await _nutritionService.AnalyzeUnstructuredRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.TotalSodium.Should().BeGreaterThan(2300); // High sodium from salt
            result.Alerts.Should().Contain(a => a.Message.Contains("High sodium"));
        }

        [Fact]
        public async Task AnalyzeUnstructuredRecipeAsync_ShouldHandlePotatoAndBananaForPotassium()
        {
            // Arrange
            var recipeText = "2 củ khoai tây, 2 quả chuối";

            // Act
            var result = await _nutritionService.AnalyzeUnstructuredRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            result.Minerals.Should().NotBeNull();
            // Potassium should be present from potatoes and bananas
            result.Minerals.First(m => m.Name.Contains("Potassium")).Amount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ParseIngredientsWithAIAsync_ShouldHandleExceptionAndFallback()
        {
            // Arrange
            var recipeText = "500g thịt gà";

            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("AI service unavailable"));

            // Act
            var result = await _nutritionService.ParseIngredientsWithAIAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            // Should fall back to keyword-based parsing
            result.Should().Contain(i => i.Name.Contains("chicken"));
        }

        [Fact]
        public async Task GenerateHealthFeedbackAsync_ShouldHandleExceptionAndFallback()
        {
            // Arrange
            var nutrition = new NutritionAnalysisResult
            {
                TotalCalories = 500,
                TotalProtein = 25,
                TotalCarbs = 50,
                TotalFat = 20,
                TotalFiber = 10,
                TotalSodium = 800
            };

            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("AI service unavailable"));

            // Act
            var result = await _nutritionService.GenerateHealthFeedbackAsync(nutrition);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().Contain("AI Health Assessment");
        }

        [Fact]
        public async Task AnalyzeCustomRecipeAsync_ShouldHandleExceptionAndFallback()
        {
            // Arrange
            var recipeText = "500g thịt gà, 100g gạo";

            _mockAIService.Setup(x => x.AnalyzeNutritionAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("AI service unavailable"));

            // Act
            var result = await _nutritionService.AnalyzeCustomRecipeAsync(recipeText);

            // Assert
            result.Should().NotBeNull();
            result.TotalCalories.Should().BeGreaterThan(0);
            // Should fall back to AnalyzeUnstructuredRecipeAsync
        }
    }
}