using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Foodbook.Data;

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
            var recipeIngredients = _context.RecipeIngredients
                .Where(ri => ri.RecipeId == recipe.Id)
                .ToList();

            var nutrition = new NutritionAnalysisResult();

            // Calculate nutrition for each ingredient
            foreach (var ri in recipeIngredients)
            {
                var ingredientNutrition = CalculateIngredientNutrition(ri.Ingredient?.Name ?? "unknown", ri.Quantity, ri.Ingredient?.Unit ?? "piece");
                
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
            
            // Step 3: AI-powered health assessment for database recipes
            nutrition.AnalysisSummary = await GetAIHealthFeedbackAsync(nutrition, "general health");

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
            var parsedIngredients = await ParseRecipeTextWithAIAsync(recipeText);
            
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
            
            // Step 4: AI-powered health feedback for custom recipes
            nutrition.AnalysisSummary = await GetAIHealthFeedbackAsync(nutrition, "general health");

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

        // New method that calls AI service for parsing
        public async Task<List<ParsedIngredient>> ParseRecipeTextWithAIAsync(string recipeText)
        {
            try
            {
                // Call AI service to parse recipe text
                return await _aiService.ParseRecipeTextAsync(recipeText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Parsing Error: {ex.Message}");
                // Fallback to basic parsing
                return await ParseRecipeTextWithAI(recipeText);
            }
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

        // New AI-powered nutrition analysis methods
        public async Task<NutritionAnalysisResult> AnalyzeNutritionWithAIAsync(string recipeText, string userGoal = "general health")
        {
            try
            {
                // Step 1: AI Parsing - Parse unstructured text into structured ingredients
                var parsedIngredients = await ParseRecipeTextWithAIAsync(recipeText);
                
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
                nutrition.AnalysisSummary = await GetAIHealthFeedbackAsync(nutrition, userGoal);

                return nutrition;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Nutrition Analysis Error: {ex.Message}");
                // Fallback to basic analysis
                return await AnalyzeUnstructuredRecipeAsync(recipeText);
            }
        }

        public async Task<string> GetAIHealthFeedbackAsync(NutritionAnalysisResult analysis, string userGoal = "general health")
        {
            try
            {
                // Call AI service for health feedback
                return await _aiService.GetHealthFeedbackAsync(analysis, userGoal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Health Feedback Error: {ex.Message}");
                // Fallback to basic assessment
                return await GenerateAIAssessmentAsync(analysis, $"Recipe with {analysis.TotalCalories:F0} calories");
            }
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

}
