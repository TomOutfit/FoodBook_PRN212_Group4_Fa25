using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Foodbook.Data;
using Foodbook.Business.Models;
using System.Text.Json;

namespace Foodbook.Business.Services
{
    public class NutritionService : INutritionService
    {
        private readonly FoodbookDbContext _context;
        private readonly IAIService _aiService;

        public NutritionService(FoodbookDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<NutritionAnalysisResult> AnalyzeRecipeNutritionAsync(Recipe recipe)
        {
            // Simulate AI processing delay
            await Task.Delay(2000);

            // Get recipe ingredients with quantities
            var recipeIngredients = await _context.RecipeIngredients
                .Include(ri => ri.Ingredient)
                .Where(ri => ri.RecipeId == recipe.Id)
                .ToListAsync();

            var nutrition = new NutritionAnalysisResult();

            // Calculate nutrition for each ingredient
            foreach (var ri in recipeIngredients)
            {
                var ingredientNutrition = CalculateIngredientNutrition(ri.Ingredient.Name, ri.Quantity, ri.Ingredient.Unit ?? "piece");
                
                nutrition.TotalCalories += ingredientNutrition.Calories;
                nutrition.TotalProtein += ingredientNutrition.Protein;
                nutrition.TotalCarbs += ingredientNutrition.Carbs;
                nutrition.TotalFat += ingredientNutrition.Fat;
                nutrition.TotalFiber += ingredientNutrition.Fiber;
                nutrition.TotalSugar += ingredientNutrition.Sugar;
                nutrition.TotalSodium += ingredientNutrition.Sodium;
                nutrition.TotalCholesterol += ingredientNutrition.Cholesterol;
                nutrition.TotalSaturatedFat += ingredientNutrition.SaturatedFat;
                nutrition.TotalTransFat += ingredientNutrition.TransFat;
            }

            // Calculate vitamins and minerals
            nutrition.Vitamins = CalculateVitamins(recipeIngredients);
            nutrition.Minerals = CalculateMinerals(recipeIngredients);

            // Generate rating and analysis
            nutrition.Rating = GenerateNutritionRating(nutrition);
            nutrition.Alerts = GenerateHealthAlerts(nutrition);
            nutrition.Recommendations = GenerateRecommendations(nutrition);
            nutrition.AnalysisSummary = GenerateAnalysisSummary(nutrition, recipe);

            return nutrition;
        }

        public async Task<NutritionAnalysisResult> AnalyzeMealPlanNutritionAsync(IEnumerable<Recipe> recipes)
        {
            // Simulate AI processing delay
            await Task.Delay(3000);

            var totalNutrition = new NutritionAnalysisResult();
            var recipeList = recipes.ToList();

            // Analyze each recipe and aggregate nutrition
            foreach (var recipe in recipeList)
            {
                var recipeNutrition = await AnalyzeRecipeNutritionAsync(recipe);
                
                totalNutrition.TotalCalories += recipeNutrition.TotalCalories;
                totalNutrition.TotalProtein += recipeNutrition.TotalProtein;
                totalNutrition.TotalCarbs += recipeNutrition.TotalCarbs;
                totalNutrition.TotalFat += recipeNutrition.TotalFat;
                totalNutrition.TotalFiber += recipeNutrition.TotalFiber;
                totalNutrition.TotalSugar += recipeNutrition.TotalSugar;
                totalNutrition.TotalSodium += recipeNutrition.TotalSodium;
                totalNutrition.TotalCholesterol += recipeNutrition.TotalCholesterol;
                totalNutrition.TotalSaturatedFat += recipeNutrition.TotalSaturatedFat;
                totalNutrition.TotalTransFat += recipeNutrition.TotalTransFat;
            }

            // Calculate meal plan specific analysis
            totalNutrition.Vitamins = CalculateMealPlanVitamins(recipeList);
            totalNutrition.Minerals = CalculateMealPlanMinerals(recipeList);
            totalNutrition.Rating = GenerateMealPlanRating(totalNutrition, recipeList.Count);
            totalNutrition.Alerts = GenerateMealPlanAlerts(totalNutrition);
            totalNutrition.Recommendations = GenerateMealPlanRecommendations(totalNutrition, recipeList.Count);
            totalNutrition.AnalysisSummary = GenerateMealPlanSummary(totalNutrition, recipeList);

            return totalNutrition;
        }

        public async Task<NutritionRecommendation> GetNutritionRecommendationsAsync(NutritionAnalysisResult analysis, string userGoal)
        {
            await Task.Delay(1000);

            var recommendation = new NutritionRecommendation
            {
                Goal = userGoal
            };

            switch (userGoal.ToLower())
            {
                case "weight loss":
                    recommendation.Suggestions.Add("Reduce calorie intake by 300-500 calories per day");
                    recommendation.Suggestions.Add("Increase protein intake to 25-30% of total calories");
                    recommendation.Suggestions.Add("Focus on high-fiber foods to increase satiety");
                    recommendation.FoodsToAdd.Add("Leafy greens, lean proteins, whole grains");
                    recommendation.FoodsToReduce.Add("Processed foods, sugary drinks, refined carbs");
                    break;

                case "muscle gain":
                    recommendation.Suggestions.Add("Increase protein intake to 1.6-2.2g per kg body weight");
                    recommendation.Suggestions.Add("Ensure adequate calorie surplus of 300-500 calories");
                    recommendation.Suggestions.Add("Time protein intake around workouts");
                    recommendation.FoodsToAdd.Add("Lean meats, eggs, dairy, legumes, nuts");
                    recommendation.FoodsToReduce.Add("Empty calories, excessive alcohol");
                    break;

                case "heart health":
                    recommendation.Suggestions.Add("Reduce sodium intake to less than 2300mg per day");
                    recommendation.Suggestions.Add("Increase omega-3 fatty acids");
                    recommendation.Suggestions.Add("Focus on plant-based proteins");
                    recommendation.FoodsToAdd.Add("Fatty fish, nuts, seeds, olive oil, vegetables");
                    recommendation.FoodsToReduce.Add("Red meat, processed foods, trans fats");
                    break;

                case "diabetes management":
                    recommendation.Suggestions.Add("Control carbohydrate portions and timing");
                    recommendation.Suggestions.Add("Choose low glycemic index foods");
                    recommendation.Suggestions.Add("Increase fiber intake");
                    recommendation.FoodsToAdd.Add("Non-starchy vegetables, whole grains, lean proteins");
                    recommendation.FoodsToReduce.Add("Sugary foods, refined grains, sweetened beverages");
                    break;

                default:
                    recommendation.Suggestions.Add("Maintain balanced macronutrient ratios");
                    recommendation.Suggestions.Add("Include variety of colorful fruits and vegetables");
                    recommendation.Suggestions.Add("Stay hydrated with adequate water intake");
                    break;
            }

            // Add specific recommendations based on analysis
            if (analysis.TotalSodium > 2300)
                recommendation.Suggestions.Add("⚠️ High sodium content - consider reducing salt");
            if (analysis.TotalFiber < 25)
                recommendation.Suggestions.Add("📈 Increase fiber intake with more vegetables and whole grains");
            if (analysis.TotalProtein < 50)
                recommendation.Suggestions.Add("💪 Add more protein sources to support muscle health");

            recommendation.MealTiming = "Eat every 3-4 hours to maintain stable blood sugar";
            recommendation.Hydration = "Drink 8-10 glasses of water daily, more if active";

            return recommendation;
        }

        public async Task<IEnumerable<HealthAlert>> GetHealthAlertsAsync(NutritionAnalysisResult analysis)
        {
            await Task.Delay(500);

            var alerts = new List<HealthAlert>();

            // High sodium alert
            if (analysis.TotalSodium > 2300)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Warning",
                    Message = "High sodium content may increase blood pressure risk",
                    Icon = "⚠️",
                    Color = "#FF9800"
                });
            }

            // Low fiber alert
            if (analysis.TotalFiber < 25)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Info",
                    Message = "Low fiber content - add more vegetables and whole grains",
                    Icon = "📈",
                    Color = "#2196F3"
                });
            }

            // High saturated fat alert
            if (analysis.TotalSaturatedFat > 20)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Warning",
                    Message = "High saturated fat may increase heart disease risk",
                    Icon = "❤️",
                    Color = "#F44336"
                });
            }

            // Good nutrition alerts
            if (analysis.TotalFiber >= 25)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Success",
                    Message = "Excellent fiber content for digestive health",
                    Icon = "✅",
                    Color = "#4CAF50"
                });
            }

            if (analysis.TotalProtein >= 50)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Success",
                    Message = "Good protein content for muscle health",
                    Icon = "💪",
                    Color = "#4CAF50"
                });
            }

            return alerts;
        }

        public async Task<NutritionAnalysisResult> AnalyzeUnstructuredRecipeAsync(string recipeText)
        {
            // Simulate AI processing delay
            await Task.Delay(3000);

            // Step 1: AI Parsing - Parse unstructured text into structured ingredients
            var parsedIngredients = await ParseRecipeTextWithAI(recipeText);
            
            // Step 2: Calculate nutrition for each parsed ingredient
            var nutrition = new NutritionAnalysisResult();
            
            foreach (var ingredient in parsedIngredients)
            {
                var ingredientNutrition = CalculateIngredientNutrition(ingredient.Name, ingredient.Quantity, ingredient.Unit);
                
                nutrition.TotalCalories += ingredientNutrition.Calories;
                nutrition.TotalProtein += ingredientNutrition.Protein;
                nutrition.TotalCarbs += ingredientNutrition.Carbs;
                nutrition.TotalFat += ingredientNutrition.Fat;
                nutrition.TotalFiber += ingredientNutrition.Fiber;
                nutrition.TotalSugar += ingredientNutrition.Sugar;
                nutrition.TotalSodium += ingredientNutrition.Sodium;
                nutrition.TotalCholesterol += ingredientNutrition.Cholesterol;
                nutrition.TotalSaturatedFat += ingredientNutrition.SaturatedFat;
                nutrition.TotalTransFat += ingredientNutrition.TransFat;
            }

            // Step 3: Generate AI-powered health assessment
            nutrition.Rating = GenerateNutritionRating(nutrition);
            nutrition.Alerts = GenerateHealthAlerts(nutrition);
            nutrition.Recommendations = GenerateRecommendations(nutrition);
            nutrition.AnalysisSummary = await GenerateAIAssessmentAsync(nutrition, recipeText);

            return nutrition;
        }

        private async Task<List<ParsedIngredient>> ParseRecipeTextWithAI(string recipeText)
        {
            // This would call Google Gemini API to parse unstructured text
            // For now, return mock data that simulates AI parsing
            await Task.Delay(1000);
            
            var mockParsedIngredients = new List<ParsedIngredient>();
            
            // Simple keyword-based parsing (in real implementation, this would be AI-powered)
            var text = recipeText.ToLower();
            
            if (text.Contains("thịt bò") || text.Contains("beef"))
            {
                var quantity = ExtractQuantity(text, "thịt bò", "beef");
                mockParsedIngredients.Add(new ParsedIngredient 
                { 
                    Name = "beef", 
                    Quantity = quantity, 
                    Unit = "g" 
                });
            }
            
            if (text.Contains("hành tây") || text.Contains("onion"))
            {
                var quantity = ExtractQuantity(text, "hành tây", "onion");
                mockParsedIngredients.Add(new ParsedIngredient 
                { 
                    Name = "onion", 
                    Quantity = quantity, 
                    Unit = "piece" 
                });
            }
            
            if (text.Contains("muối") || text.Contains("salt"))
            {
                var quantity = ExtractQuantity(text, "muối", "salt");
                mockParsedIngredients.Add(new ParsedIngredient 
                { 
                    Name = "salt", 
                    Quantity = quantity, 
                    Unit = "tsp" 
                });
            }
            
            if (text.Contains("nước tương") || text.Contains("soy sauce"))
            {
                var quantity = ExtractQuantity(text, "nước tương", "soy sauce");
                mockParsedIngredients.Add(new ParsedIngredient 
                { 
                    Name = "soy sauce", 
                    Quantity = quantity, 
                    Unit = "ml" 
                });
            }
            
            // Default fallback if no ingredients detected
            if (!mockParsedIngredients.Any())
            {
                mockParsedIngredients.Add(new ParsedIngredient 
                { 
                    Name = "mixed ingredients", 
                    Quantity = 200, 
                    Unit = "g" 
                });
            }
            
            return mockParsedIngredients;
        }

        private decimal ExtractQuantity(string text, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                var index = text.IndexOf(keyword);
                if (index > 0)
                {
                    // Look for numbers before the keyword
                    var beforeKeyword = text.Substring(0, index).Trim();
                    var words = beforeKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var word in words.Reverse())
                    {
                        if (decimal.TryParse(word, out var quantity))
                        {
                            return quantity;
                        }
                    }
                }
            }
            
            return 100; // Default quantity
        }

        private async Task<string> GenerateAIAssessmentAsync(NutritionAnalysisResult nutrition, string originalText)
        {
            // This would call Google Gemini API for AI-powered health assessment
            await Task.Delay(1000);
            
            var assessment = $"🤖 AI Health Assessment:\n\n";
            
            // Protein analysis
            if (nutrition.TotalProtein >= 20)
                assessment += "💪 Excellent protein content for muscle health\n";
            else
                assessment += "⚠️ Consider adding more protein sources\n";
            
            // Fiber analysis
            if (nutrition.TotalFiber >= 25)
                assessment += "🌾 Great fiber content for digestive health\n";
            else
                assessment += "📈 Add more vegetables and whole grains for fiber\n";
            
            // Sodium analysis
            if (nutrition.TotalSodium > 2300)
                assessment += "🧂 High sodium content - consider reducing salt\n";
            else
                assessment += "✅ Good sodium levels\n";
            
            // Overall assessment
            var grade = nutrition.Rating.Grade;
            assessment += $"\n📊 Overall Grade: {grade} - {nutrition.Rating.Description}\n";
            
            // AI recommendations
            assessment += "\n🎯 AI Recommendations:\n";
            foreach (var rec in nutrition.Recommendations.Take(3))
            {
                assessment += $"• {rec}\n";
            }
            
            return assessment;
        }

        public async Task<NutritionComparison> CompareNutritionAsync(Recipe recipe1, Recipe recipe2)
        {
            await Task.Delay(2000);

            var nutrition1 = await AnalyzeRecipeNutritionAsync(recipe1);
            var nutrition2 = await AnalyzeRecipeNutritionAsync(recipe2);

            var comparison = new NutritionComparison
            {
                Recipe1 = recipe1,
                Recipe2 = recipe2,
                Nutrition1 = nutrition1,
                Nutrition2 = nutrition2
            };

            // Compare key nutrients
            var comparisons = new List<ComparisonItem>
            {
                new() { Nutrient = "Calories", Recipe1Value = nutrition1.TotalCalories, Recipe2Value = nutrition2.TotalCalories },
                new() { Nutrient = "Protein", Recipe1Value = nutrition1.TotalProtein, Recipe2Value = nutrition2.TotalProtein },
                new() { Nutrient = "Carbs", Recipe1Value = nutrition1.TotalCarbs, Recipe2Value = nutrition2.TotalCarbs },
                new() { Nutrient = "Fat", Recipe1Value = nutrition1.TotalFat, Recipe2Value = nutrition2.TotalFat },
                new() { Nutrient = "Fiber", Recipe1Value = nutrition1.TotalFiber, Recipe2Value = nutrition2.TotalFiber },
                new() { Nutrient = "Sodium", Recipe1Value = nutrition1.TotalSodium, Recipe2Value = nutrition2.TotalSodium }
            };

            foreach (var comp in comparisons)
            {
                comp.Difference = Math.Abs(comp.Recipe1Value - comp.Recipe2Value);
                comp.BetterRecipe = comp.Recipe1Value > comp.Recipe2Value ? recipe1.Title : recipe2.Title;
            }

            comparison.Comparisons = comparisons;

            // Determine overall winner
            var recipe1Score = nutrition1.Rating.OverallScore;
            var recipe2Score = nutrition2.Rating.OverallScore;
            comparison.Winner = recipe1Score > recipe2Score ? recipe1.Title : recipe2.Title;
            comparison.Summary = GenerateComparisonSummary(comparison);

            return comparison;
        }

        private IngredientNutrition CalculateIngredientNutrition(string ingredientName, decimal quantity, string unit)
        {
            var name = ingredientName.ToLower();
            var multiplier = ConvertToGrams(quantity, unit) / 100; // Convert to per 100g basis

            // Base nutrition per 100g for common ingredients
            var nutrition = name switch
            {
                var n when n.Contains("chicken") => new IngredientNutrition { Calories = 165, Protein = 31, Carbs = 0, Fat = 3.6m, Fiber = 0, Sugar = 0, Sodium = 74, Cholesterol = 85, SaturatedFat = 1, TransFat = 0 },
                var n when n.Contains("beef") => new IngredientNutrition { Calories = 250, Protein = 26, Carbs = 0, Fat = 15, Fiber = 0, Sugar = 0, Sodium = 72, Cholesterol = 90, SaturatedFat = 6, TransFat = 0 },
                var n when n.Contains("fish") => new IngredientNutrition { Calories = 206, Protein = 22, Carbs = 0, Fat = 12, Fiber = 0, Sugar = 0, Sodium = 61, Cholesterol = 63, SaturatedFat = 3, TransFat = 0 },
                var n when n.Contains("rice") => new IngredientNutrition { Calories = 130, Protein = 2.7m, Carbs = 28, Fat = 0.3m, Fiber = 0.4m, Sugar = 0.1m, Sodium = 1, Cholesterol = 0, SaturatedFat = 0.1m, TransFat = 0 },
                var n when n.Contains("pasta") => new IngredientNutrition { Calories = 131, Protein = 5, Carbs = 25, Fat = 1.1m, Fiber = 1.8m, Sugar = 0.6m, Sodium = 1, Cholesterol = 0, SaturatedFat = 0.2m, TransFat = 0 },
                var n when n.Contains("tomato") => new IngredientNutrition { Calories = 18, Protein = 0.9m, Carbs = 3.9m, Fat = 0.2m, Fiber = 1.2m, Sugar = 2.6m, Sodium = 5, Cholesterol = 0, SaturatedFat = 0, TransFat = 0 },
                var n when n.Contains("onion") => new IngredientNutrition { Calories = 40, Protein = 1.1m, Carbs = 9.3m, Fat = 0.1m, Fiber = 1.7m, Sugar = 4.2m, Sodium = 4, Cholesterol = 0, SaturatedFat = 0, TransFat = 0 },
                var n when n.Contains("garlic") => new IngredientNutrition { Calories = 149, Protein = 6.4m, Carbs = 33, Fat = 0.5m, Fiber = 2.1m, Sugar = 1, Sodium = 17, Cholesterol = 0, SaturatedFat = 0.1m, TransFat = 0 },
                var n when n.Contains("carrot") => new IngredientNutrition { Calories = 41, Protein = 0.9m, Carbs = 9.6m, Fat = 0.2m, Fiber = 2.8m, Sugar = 4.7m, Sodium = 69, Cholesterol = 0, SaturatedFat = 0, TransFat = 0 },
                var n when n.Contains("potato") => new IngredientNutrition { Calories = 77, Protein = 2, Carbs = 17, Fat = 0.1m, Fiber = 2.2m, Sugar = 0.8m, Sodium = 6, Cholesterol = 0, SaturatedFat = 0, TransFat = 0 },
                var n when n.Contains("spinach") => new IngredientNutrition { Calories = 23, Protein = 2.9m, Carbs = 3.6m, Fat = 0.4m, Fiber = 2.2m, Sugar = 0.4m, Sodium = 79, Cholesterol = 0, SaturatedFat = 0.1m, TransFat = 0 },
                var n when n.Contains("broccoli") => new IngredientNutrition { Calories = 34, Protein = 2.8m, Carbs = 7, Fat = 0.4m, Fiber = 2.6m, Sugar = 1.5m, Sodium = 33, Cholesterol = 0, SaturatedFat = 0.1m, TransFat = 0 },
                var n when n.Contains("egg") => new IngredientNutrition { Calories = 155, Protein = 13, Carbs = 1.1m, Fat = 11, Fiber = 0, Sugar = 1.1m, Sodium = 124, Cholesterol = 373, SaturatedFat = 3.3m, TransFat = 0 },
                var n when n.Contains("cheese") => new IngredientNutrition { Calories = 113, Protein = 7, Carbs = 1, Fat = 9, Fiber = 0, Sugar = 0.5m, Sodium = 621, Cholesterol = 30, SaturatedFat = 5.4m, TransFat = 0 },
                var n when n.Contains("milk") => new IngredientNutrition { Calories = 42, Protein = 3.4m, Carbs = 5, Fat = 1, Fiber = 0, Sugar = 5, Sodium = 44, Cholesterol = 5, SaturatedFat = 0.6m, TransFat = 0 },
                var n when n.Contains("oil") => new IngredientNutrition { Calories = 884, Protein = 0, Carbs = 0, Fat = 100, Fiber = 0, Sugar = 0, Sodium = 0, Cholesterol = 0, SaturatedFat = 14, TransFat = 0 },
                var n when n.Contains("butter") => new IngredientNutrition { Calories = 717, Protein = 0.9m, Carbs = 0.1m, Fat = 81, Fiber = 0, Sugar = 0.1m, Sodium = 11, Cholesterol = 215, SaturatedFat = 51, TransFat = 3 },
                _ => new IngredientNutrition { Calories = 50, Protein = 2, Carbs = 8, Fat = 1, Fiber = 2, Sugar = 3, Sodium = 10, Cholesterol = 0, SaturatedFat = 0.2m, TransFat = 0 }
            };

            // Apply multiplier
            nutrition.Calories *= multiplier;
            nutrition.Protein *= multiplier;
            nutrition.Carbs *= multiplier;
            nutrition.Fat *= multiplier;
            nutrition.Fiber *= multiplier;
            nutrition.Sugar *= multiplier;
            nutrition.Sodium *= multiplier;
            nutrition.Cholesterol *= multiplier;
            nutrition.SaturatedFat *= multiplier;
            nutrition.TransFat *= multiplier;

            return nutrition;
        }

        private decimal ConvertToGrams(decimal quantity, string unit)
        {
            return unit.ToLower() switch
            {
                "gram" or "g" => quantity,
                "kilogram" or "kg" => quantity * 1000,
                "pound" or "lb" => quantity * 453.592m,
                "ounce" or "oz" => quantity * 28.3495m,
                "cup" => quantity * 240, // Approximate for most ingredients
                "tablespoon" or "tbsp" => quantity * 15,
                "teaspoon" or "tsp" => quantity * 5,
                "piece" or "pcs" => quantity * 100, // Average piece weight
                _ => quantity * 100 // Default assumption
            };
        }

        private List<VitaminInfo> CalculateVitamins(List<RecipeIngredient> ingredients)
        {
            var vitamins = new List<VitaminInfo>
            {
                new() { Name = "Vitamin A", Amount = 0, Unit = "mcg", DailyValue = 900, Benefits = "Eye health, immune function" },
                new() { Name = "Vitamin C", Amount = 0, Unit = "mg", DailyValue = 90, Benefits = "Immune support, collagen synthesis" },
                new() { Name = "Vitamin D", Amount = 0, Unit = "mcg", DailyValue = 20, Benefits = "Bone health, immune function" },
                new() { Name = "Vitamin E", Amount = 0, Unit = "mg", DailyValue = 15, Benefits = "Antioxidant, skin health" },
                new() { Name = "Vitamin K", Amount = 0, Unit = "mcg", DailyValue = 120, Benefits = "Blood clotting, bone health" }
            };

            // Calculate based on ingredients
            foreach (var ingredient in ingredients)
            {
                var name = ingredient.Ingredient.Name.ToLower();
                if (name.Contains("carrot") || name.Contains("spinach"))
                {
                    vitamins[0].Amount += 50; // Vitamin A
                    vitamins[1].Amount += 20; // Vitamin C
                }
                if (name.Contains("broccoli") || name.Contains("tomato"))
                {
                    vitamins[1].Amount += 30; // Vitamin C
                }
                if (name.Contains("egg") || name.Contains("fish"))
                {
                    vitamins[2].Amount += 5; // Vitamin D
                }
                if (name.Contains("oil") || name.Contains("nuts"))
                {
                    vitamins[3].Amount += 10; // Vitamin E
                }
                if (name.Contains("spinach") || name.Contains("broccoli"))
                {
                    vitamins[4].Amount += 25; // Vitamin K
                }
            }

            return vitamins;
        }

        private List<MineralInfo> CalculateMinerals(List<RecipeIngredient> ingredients)
        {
            var minerals = new List<MineralInfo>
            {
                new() { Name = "Calcium", Amount = 0, Unit = "mg", DailyValue = 1000, Benefits = "Bone health, muscle function" },
                new() { Name = "Iron", Amount = 0, Unit = "mg", DailyValue = 18, Benefits = "Oxygen transport, energy production" },
                new() { Name = "Magnesium", Amount = 0, Unit = "mg", DailyValue = 400, Benefits = "Muscle function, heart health" },
                new() { Name = "Potassium", Amount = 0, Unit = "mg", DailyValue = 3500, Benefits = "Blood pressure, heart health" },
                new() { Name = "Zinc", Amount = 0, Unit = "mg", DailyValue = 11, Benefits = "Immune function, wound healing" }
            };

            // Calculate based on ingredients
            foreach (var ingredient in ingredients)
            {
                var name = ingredient.Ingredient.Name.ToLower();
                if (name.Contains("cheese") || name.Contains("milk"))
                {
                    minerals[0].Amount += 200; // Calcium
                }
                if (name.Contains("beef") || name.Contains("spinach"))
                {
                    minerals[1].Amount += 3; // Iron
                }
                if (name.Contains("nuts") || name.Contains("spinach"))
                {
                    minerals[2].Amount += 50; // Magnesium
                }
                if (name.Contains("potato") || name.Contains("banana"))
                {
                    minerals[3].Amount += 400; // Potassium
                }
                if (name.Contains("beef") || name.Contains("chicken"))
                {
                    minerals[4].Amount += 2; // Zinc
                }
            }

            return minerals;
        }

        private List<VitaminInfo> CalculateMealPlanVitamins(List<Recipe> recipes)
        {
            // Simplified calculation for meal plans
            return new List<VitaminInfo>
            {
                new() { Name = "Vitamin A", Amount = recipes.Count * 25, Unit = "mcg", DailyValue = 900, Benefits = "Eye health, immune function" },
                new() { Name = "Vitamin C", Amount = recipes.Count * 30, Unit = "mg", DailyValue = 90, Benefits = "Immune support, collagen synthesis" },
                new() { Name = "Vitamin D", Amount = recipes.Count * 5, Unit = "mcg", DailyValue = 20, Benefits = "Bone health, immune function" },
                new() { Name = "Vitamin E", Amount = recipes.Count * 8, Unit = "mg", DailyValue = 15, Benefits = "Antioxidant, skin health" },
                new() { Name = "Vitamin K", Amount = recipes.Count * 20, Unit = "mcg", DailyValue = 120, Benefits = "Blood clotting, bone health" }
            };
        }

        private List<MineralInfo> CalculateMealPlanMinerals(List<Recipe> recipes)
        {
            return new List<MineralInfo>
            {
                new() { Name = "Calcium", Amount = recipes.Count * 150, Unit = "mg", DailyValue = 1000, Benefits = "Bone health, muscle function" },
                new() { Name = "Iron", Amount = recipes.Count * 2.5m, Unit = "mg", DailyValue = 18, Benefits = "Oxygen transport, energy production" },
                new() { Name = "Magnesium", Amount = recipes.Count * 40, Unit = "mg", DailyValue = 400, Benefits = "Muscle function, heart health" },
                new() { Name = "Potassium", Amount = recipes.Count * 300, Unit = "mg", DailyValue = 3500, Benefits = "Blood pressure, heart health" },
                new() { Name = "Zinc", Amount = recipes.Count * 1.5m, Unit = "mg", DailyValue = 11, Benefits = "Immune function, wound healing" }
            };
        }

        private NutritionRating GenerateNutritionRating(NutritionAnalysisResult nutrition)
        {
            var score = 0;
            var strengths = new List<string>();
            var improvements = new List<string>();

            // Score based on macronutrient balance
            if (nutrition.TotalProtein >= 20 && nutrition.TotalProtein <= 35) score += 20;
            else if (nutrition.TotalProtein < 20) improvements.Add("Increase protein intake");
            else strengths.Add("High protein content");

            if (nutrition.TotalCarbs >= 45 && nutrition.TotalCarbs <= 65) score += 20;
            else if (nutrition.TotalCarbs < 45) improvements.Add("Increase carbohydrate intake");
            else strengths.Add("Good carbohydrate balance");

            if (nutrition.TotalFat >= 20 && nutrition.TotalFat <= 35) score += 20;
            else if (nutrition.TotalFat < 20) improvements.Add("Increase healthy fats");
            else strengths.Add("Balanced fat content");

            // Score based on fiber
            if (nutrition.TotalFiber >= 25) { score += 20; strengths.Add("Excellent fiber content"); }
            else if (nutrition.TotalFiber >= 15) { score += 15; strengths.Add("Good fiber content"); }
            else { score += 5; improvements.Add("Increase fiber intake"); }

            // Score based on sodium
            if (nutrition.TotalSodium <= 2300) { score += 20; strengths.Add("Low sodium content"); }
            else if (nutrition.TotalSodium <= 3000) { score += 10; }
            else { improvements.Add("Reduce sodium intake"); }

            var grade = score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };

            var description = grade switch
            {
                "A" => "Excellent nutritional profile",
                "B" => "Good nutritional balance",
                "C" => "Average nutritional content",
                "D" => "Below average nutrition",
                "F" => "Poor nutritional profile",
                _ => "Unknown nutritional profile"
            };

            return new NutritionRating
            {
                OverallScore = score,
                Grade = grade,
                Description = description,
                Strengths = strengths,
                Improvements = improvements
            };
        }

        private NutritionRating GenerateMealPlanRating(NutritionAnalysisResult nutrition, int recipeCount)
        {
            var baseRating = GenerateNutritionRating(nutrition);
            
            // Adjust for meal plan
            if (recipeCount >= 3)
            {
                baseRating.OverallScore += 10;
                baseRating.Strengths.Add("Good meal variety");
            }
            
            if (nutrition.TotalCalories >= 1500 && nutrition.TotalCalories <= 2500)
            {
                baseRating.OverallScore += 10;
                baseRating.Strengths.Add("Appropriate calorie range");
            }

            return baseRating;
        }

        private List<HealthAlert> GenerateHealthAlerts(NutritionAnalysisResult nutrition)
        {
            var alerts = new List<HealthAlert>();

            if (nutrition.TotalSodium > 2300)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Warning",
                    Message = "High sodium content",
                    Icon = "⚠️",
                    Color = "#FF9800"
                });
            }

            if (nutrition.TotalFiber < 25)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Info",
                    Message = "Low fiber content",
                    Icon = "📈",
                    Color = "#2196F3"
                });
            }

            if (nutrition.TotalFiber >= 25)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Success",
                    Message = "Good fiber content",
                    Icon = "✅",
                    Color = "#4CAF50"
                });
            }

            return alerts;
        }

        private List<HealthAlert> GenerateMealPlanAlerts(NutritionAnalysisResult nutrition)
        {
            var alerts = GenerateHealthAlerts(nutrition);
            
            if (nutrition.TotalCalories < 1200)
            {
                alerts.Add(new HealthAlert
                {
                    Type = "Warning",
                    Message = "Very low calorie intake",
                    Icon = "⚠️",
                    Color = "#F44336"
                });
            }

            return alerts;
        }

        private List<string> GenerateRecommendations(NutritionAnalysisResult nutrition)
        {
            var recommendations = new List<string>();

            if (nutrition.TotalFiber < 25)
                recommendations.Add("Add more vegetables and whole grains for fiber");
            if (nutrition.TotalProtein < 50)
                recommendations.Add("Include lean proteins like chicken, fish, or legumes");
            if (nutrition.TotalSodium > 2300)
                recommendations.Add("Reduce salt and processed foods");
            if (nutrition.TotalFat < 20)
                recommendations.Add("Add healthy fats like olive oil, nuts, or avocado");

            return recommendations;
        }

        private List<string> GenerateMealPlanRecommendations(NutritionAnalysisResult nutrition, int recipeCount)
        {
            var recommendations = GenerateRecommendations(nutrition);
            
            if (recipeCount < 3)
                recommendations.Add("Add more variety to your meal plan");
            if (nutrition.TotalCalories < 1500)
                recommendations.Add("Consider adding snacks or larger portions");
            
            return recommendations;
        }

        private string GenerateAnalysisSummary(NutritionAnalysisResult nutrition, Recipe recipe)
        {
            return $"This {recipe.Title} provides {nutrition.TotalCalories:F0} calories with " +
                   $"{nutrition.TotalProtein:F1}g protein, {nutrition.TotalCarbs:F1}g carbs, and " +
                   $"{nutrition.TotalFat:F1}g fat. {nutrition.Rating.Description} " +
                   $"with a grade of {nutrition.Rating.Grade}.";
        }

        private string GenerateMealPlanSummary(NutritionAnalysisResult nutrition, List<Recipe> recipes)
        {
            return $"Your {recipes.Count}-recipe meal plan provides {nutrition.TotalCalories:F0} total calories " +
                   $"with balanced macronutrients. {nutrition.Rating.Description} " +
                   $"with an overall grade of {nutrition.Rating.Grade}.";
        }

        private string GenerateComparisonSummary(NutritionComparison comparison)
        {
            var winner = comparison.Winner;
            var score1 = comparison.Nutrition1.Rating.OverallScore;
            var score2 = comparison.Nutrition2.Rating.OverallScore;
            
            return $"{winner} has a better nutritional profile " +
                   $"({Math.Max(score1, score2)} vs {Math.Min(score1, score2)} score). " +
                   $"Consider the specific nutrient differences when making your choice.";
        }

        // New methods for enhanced AI integration

        public async Task<List<IngredientDto>> ParseIngredientsWithAIAsync(string recipeText)
        {
            try
            {
                // Pha 2b: Luồng AI Parsing - Gọi AI để giải mã văn bản tùy ý
                var aiPrompt = CreateIngredientParsingPrompt(recipeText);
                var aiResponse = await _aiService.AnalyzeNutritionAsync(aiPrompt);
                
                // Parse AI response thành IngredientDto list
                var ingredients = ParseAIResponseToIngredients(aiResponse);
                
                return ingredients;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Parsing Error: {ex.Message}");
                // Fallback to simple keyword-based parsing
                return await Task.FromResult(ParseIngredientsWithFallback(recipeText));
            }
        }

        public async Task<NutritionAnalysisResult> CalculateNutritionFromIngredientsAsync(List<IngredientDto> ingredients)
        {
            // Pha 2: Tính toán dinh dưỡng từ danh sách nguyên liệu đã được AI parse
            var nutrition = new NutritionAnalysisResult();

            foreach (var ingredient in ingredients)
            {
                var quantity = ConvertToGrams((decimal)ingredient.Quantity, ingredient.Unit);
                var multiplier = (double)(quantity / 100); // Convert to per 100g basis

                nutrition.TotalCalories += (decimal)(ingredient.CaloriesPerUnit * multiplier);
                nutrition.TotalProtein += (decimal)(ingredient.ProteinPerUnit * multiplier);
                nutrition.TotalCarbs += (decimal)(ingredient.CarbohydratesPerUnit * multiplier);
                nutrition.TotalFat += (decimal)(ingredient.FatPerUnit * multiplier);
                nutrition.TotalFiber += (decimal)(ingredient.FiberPerUnit * multiplier);
                nutrition.TotalSugar += (decimal)(ingredient.SugarPerUnit * multiplier);
                nutrition.TotalSodium += (decimal)(ingredient.SodiumPerUnit * multiplier);
            }

            // Generate rating and analysis
            nutrition.Rating = GenerateNutritionRating(nutrition);
            nutrition.Alerts = GenerateHealthAlerts(nutrition);
            nutrition.Recommendations = GenerateRecommendations(nutrition);
            nutrition.AnalysisSummary = GenerateIngredientBasedSummary(nutrition, ingredients);

            return await Task.FromResult(nutrition);
        }

        public async Task<string> GenerateHealthFeedbackAsync(NutritionAnalysisResult nutritionInfo)
        {
            try
            {
                // Pha 3: Đánh giá AI - Sinh ra báo cáo đánh giá bằng AI
                var feedbackPrompt = CreateHealthFeedbackPrompt(nutritionInfo);
                var aiFeedback = await _aiService.AnalyzeNutritionAsync(feedbackPrompt);
                
                return aiFeedback;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Health Feedback Error: {ex.Message}");
                return GenerateFallbackHealthFeedback(nutritionInfo);
            }
        }

        public async Task<NutritionAnalysisResult> AnalyzeCustomRecipeAsync(string recipeText, string userGoal = "General Health")
        {
            // Luồng hoạt động hoàn chỉnh: AI Parsing + Tính toán + Đánh giá AI
            try
            {
                // Bước 1: AI Parsing
                var ingredients = await ParseIngredientsWithAIAsync(recipeText);
                
                // Bước 2: Tính toán dinh dưỡng
                var nutrition = await CalculateNutritionFromIngredientsAsync(ingredients);
                
                // Bước 3: Đánh giá AI
                nutrition.AnalysisSummary = await GenerateHealthFeedbackAsync(nutrition);
                
                return nutrition;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Custom Recipe Analysis Error: {ex.Message}");
                // Fallback to existing method
                return await AnalyzeUnstructuredRecipeAsync(recipeText);
            }
        }

        private string CreateIngredientParsingPrompt(string recipeText)
        {
            return $@"Bạn là chuyên gia dinh dưỡng và đầu bếp chuyên nghiệp với 20 năm kinh nghiệm. Hãy phân tích công thức sau và trích xuất thông tin nguyên liệu một cách chính xác:

CÔNG THỨC: {recipeText}

YÊU CẦU:
1. Trích xuất TẤT CẢ nguyên liệu với định lượng chính xác
2. Chuẩn hóa tên nguyên liệu (VD: thịt bò → beef, hành tây → onion)
3. Ước tính định lượng hợp lý nếu không rõ ràng
4. Phân loại đơn vị phù hợp (g, ml, piece, cup, tbsp, tsp)

ĐỊNH DẠNG JSON:
{{
  ""ingredients"": [
    {{
      ""name"": ""tên_nguyên_liệu_chuẩn"",
      ""quantity"": số_lượng,
      ""unit"": ""đơn_vị"",
      ""category"": ""protein|vegetable|grain|dairy|fat|spice""
    }}
  ],
  ""servings"": số_phần_ăn,
  ""cooking_method"": ""phương_pháp_nấu""
}}

QUY TẮC:
- Ưu tiên đơn vị metric (g, ml) thay vì imperial
- Phân loại nguyên liệu để tính toán dinh dưỡng chính xác
- Ước tính serving size để tính calories per serving
- Bỏ qua gia vị nhỏ (muối, tiêu, đường) trừ khi có định lượng rõ ràng";
        }

        private string CreateHealthFeedbackPrompt(NutritionAnalysisResult nutrition)
        {
            return $@"
Bạn là chuyên gia dinh dưỡng với 20 năm kinh nghiệm. Hãy phân tích thông tin dinh dưỡng sau và đưa ra đánh giá chuyên nghiệp:

📊 THÔNG TIN DINH DƯỠNG:
- Calories: {nutrition.TotalCalories:F0} kcal
- Protein: {nutrition.TotalProtein:F1}g
- Carbohydrates: {nutrition.TotalCarbs:F1}g  
- Fat: {nutrition.TotalFat:F1}g
- Fiber: {nutrition.TotalFiber:F1}g
- Sodium: {nutrition.TotalSodium:F0}mg
- Grade: {nutrition.Rating.Grade} ({nutrition.Rating.OverallScore}/100)

Hãy đưa ra:
1. Đánh giá tổng quan về cân bằng dinh dưỡng
2. Điểm mạnh và điểm cần cải thiện
3. Khuyến nghị cụ thể để tối ưu hóa sức khỏe
4. Cảnh báo về các vấn đề dinh dưỡng (nếu có)

Sử dụng ngôn ngữ thân thiện, dễ hiểu với emoji phù hợp.
";
        }

        private List<IngredientDto> ParseAIResponseToIngredients(string aiResponse)
        {
            try
            {
                // Try to parse JSON response from AI
                var jsonStart = aiResponse.IndexOf('{');
                var jsonEnd = aiResponse.LastIndexOf('}');
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonString = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var response = JsonSerializer.Deserialize<AIRecipeResponse>(jsonString);
                    
                    if (response?.ingredients != null)
                    {
                        var ingredients = new List<IngredientDto>();
                        foreach (var ing in response.ingredients)
                        {
                            var ingredient = new IngredientDto
                            {
                                Name = ing.name ?? "Unknown",
                                Quantity = ing.quantity ?? 100,
                                Unit = ing.unit ?? "g"
                            };
                            
                            // Set nutritional values based on ingredient name
                            SetNutritionalValues(ingredient);
                            ingredients.Add(ingredient);
                        }
                        return ingredients;
                    }
                }
                
                // Fallback parsing
                return ParseIngredientsWithFallback(aiResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                return ParseIngredientsWithFallback(aiResponse);
            }
        }

        private List<IngredientDto> ParseIngredientsWithFallback(string text)
        {
            // Simple keyword-based parsing as fallback
            var ingredients = new List<IngredientDto>();
            var textLower = text.ToLower();
            
            // Common ingredient patterns
            var patterns = new Dictionary<string, (string name, decimal quantity, string unit)>
            {
                ["thịt bò"] = ("beef", 200, "g"),
                ["thịt gà"] = ("chicken", 150, "g"),
                ["cá"] = ("fish", 200, "g"),
                ["hành tây"] = ("onion", 1, "piece"),
                ["cà chua"] = ("tomato", 2, "piece"),
                ["khoai tây"] = ("potato", 2, "piece"),
                ["cà rốt"] = ("carrot", 1, "piece"),
                ["gạo"] = ("rice", 100, "g"),
                ["mì"] = ("noodles", 100, "g")
            };
            
            foreach (var pattern in patterns)
            {
                if (textLower.Contains(pattern.Key))
                {
                    var ingredient = new IngredientDto
                    {
                        Name = pattern.Value.name,
                        Quantity = (double)pattern.Value.quantity,
                        Unit = pattern.Value.unit
                    };
                    SetNutritionalValues(ingredient);
                    ingredients.Add(ingredient);
                }
            }
            
            // Default fallback
            if (!ingredients.Any())
            {
                ingredients.Add(new IngredientDto
                {
                    Name = "mixed ingredients",
                    Quantity = 200,
                    Unit = "g"
                });
                SetNutritionalValues(ingredients[0]);
            }
            
            return ingredients;
        }

        private void SetNutritionalValues(IngredientDto ingredient)
        {
            // Set nutritional values based on ingredient name
            var name = ingredient.Name.ToLower();
            var multiplier = (double)(ingredient.Quantity / 100); // Per 100g basis
            
            var nutrition = name switch
            {
                var n when n.Contains("chicken") => (calories: 165, protein: 31, carbs: 0, fat: 3.6, fiber: 0, sugar: 0, sodium: 74),
                var n when n.Contains("beef") => (calories: 250, protein: 26, carbs: 0, fat: 15, fiber: 0, sugar: 0, sodium: 72),
                var n when n.Contains("fish") => (calories: 206, protein: 22, carbs: 0, fat: 12, fiber: 0, sugar: 0, sodium: 61),
                var n when n.Contains("rice") => (calories: 130, protein: 2.7, carbs: 28, fat: 0.3, fiber: 0.4, sugar: 0.1, sodium: 1),
                var n when n.Contains("onion") => (calories: 40, protein: 1.1, carbs: 9.3, fat: 0.1, fiber: 1.7, sugar: 4.2, sodium: 4),
                var n when n.Contains("tomato") => (calories: 18, protein: 0.9, carbs: 3.9, fat: 0.2, fiber: 1.2, sugar: 2.6, sodium: 5),
                var n when n.Contains("potato") => (calories: 77, protein: 2, carbs: 17, fat: 0.1, fiber: 2.2, sugar: 0.8, sodium: 6),
                var n when n.Contains("carrot") => (calories: 41, protein: 0.9, carbs: 9.6, fat: 0.2, fiber: 2.8, sugar: 4.7, sodium: 69),
                _ => (calories: 50, protein: 2, carbs: 8, fat: 1, fiber: 2, sugar: 3, sodium: 10)
            };
            
            ingredient.CaloriesPerUnit = nutrition.calories * multiplier;
            ingredient.ProteinPerUnit = nutrition.protein * multiplier;
            ingredient.CarbohydratesPerUnit = nutrition.carbs * multiplier;
            ingredient.FatPerUnit = nutrition.fat * multiplier;
            ingredient.FiberPerUnit = nutrition.fiber * multiplier;
            ingredient.SugarPerUnit = nutrition.sugar * multiplier;
            ingredient.SodiumPerUnit = nutrition.sodium * multiplier;
        }

        private string GenerateIngredientBasedSummary(NutritionAnalysisResult nutrition, List<IngredientDto> ingredients)
        {
            var ingredientNames = string.Join(", ", ingredients.Select(i => i.Name));
            return $"Phân tích dinh dưỡng cho món ăn với {ingredients.Count} nguyên liệu: {ingredientNames}. " +
                   $"Tổng cộng {nutrition.TotalCalories:F0} calories với {nutrition.Rating.Grade} grade " +
                   $"({nutrition.Rating.OverallScore}/100). {nutrition.Rating.Description}";
        }

        private string GenerateFallbackHealthFeedback(NutritionAnalysisResult nutrition)
        {
            var feedback = $"🤖 AI Health Assessment:\n\n";
            
            // Protein analysis
            if (nutrition.TotalProtein >= 20)
                feedback += "💪 Excellent protein content for muscle health\n";
            else
                feedback += "⚠️ Consider adding more protein sources\n";
            
            // Fiber analysis
            if (nutrition.TotalFiber >= 25)
                feedback += "🌾 Great fiber content for digestive health\n";
            else
                feedback += "📈 Add more vegetables and whole grains for fiber\n";
            
            // Sodium analysis
            if (nutrition.TotalSodium > 2300)
                feedback += "🧂 High sodium content - consider reducing salt\n";
            else
                feedback += "✅ Good sodium levels\n";
            
            // Overall assessment
            feedback += $"\n📊 Overall Grade: {nutrition.Rating.Grade} - {nutrition.Rating.Description}\n";
            
            // Recommendations
            feedback += "\n🎯 Recommendations:\n";
            foreach (var rec in nutrition.Recommendations.Take(3))
            {
                feedback += $"• {rec}\n";
            }
            
            return feedback;
        }
    }

    public class IngredientNutrition
    {
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }
        public decimal Fiber { get; set; }
        public decimal Sugar { get; set; }
        public decimal Sodium { get; set; }
        public decimal Cholesterol { get; set; }
        public decimal SaturatedFat { get; set; }
        public decimal TransFat { get; set; }
    }

    public class ParsedIngredient
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    // AI Response Models for JSON parsing
    public class AIRecipeResponse
    {
        public List<AIIngredient>? ingredients { get; set; }
    }

    public class AIIngredient
    {
        public string? name { get; set; }
        public double? quantity { get; set; }
        public string? unit { get; set; }
    }
}
