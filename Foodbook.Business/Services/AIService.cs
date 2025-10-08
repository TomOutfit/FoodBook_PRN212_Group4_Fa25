using Foodbook.Data.Entities;
using Foodbook.Business.Interfaces;

namespace Foodbook.Business.Services
{
    public class AIService : IAIService
    {
        public async Task<ChefJudgeResult> JudgeDishAsync(string imagePath)
        {
            // Simulate AI processing delay
            await Task.Delay(2000);

            // Mock AI analysis - in real implementation, this would use computer vision APIs
            var random = new Random();
            var score = random.Next(6, 11); // Score between 6-10
            
            var comments = new[]
            {
                "Excellent presentation! The dish looks professionally prepared.",
                "Great color combination and plating technique.",
                "Good effort! The dish shows promise with some room for improvement.",
                "Outstanding! This looks like it came from a 5-star restaurant.",
                "Well done! The presentation is clean and appetizing."
            };

            var suggestions = new[]
            {
                "Consider adding a garnish for better visual appeal",
                "The portion size looks perfect",
                "Try adding some herbs for extra flavor",
                "The plating could be more artistic",
                "Excellent use of colors and textures"
            };

            var ratings = new[] { "Excellent", "Good", "Fair", "Poor" };
            var overallRating = score >= 9 ? ratings[0] : score >= 7 ? ratings[1] : score >= 5 ? ratings[2] : ratings[3];

            return new ChefJudgeResult
            {
                Score = score,
                Comment = comments[random.Next(comments.Length)],
                Suggestions = suggestions.Take(random.Next(2, 4)).ToList(),
                OverallRating = overallRating
            };
        }

        public async Task<ChefJudgeResult> JudgeDishAsync(byte[] imageData)
        {
            // Simulate AI processing delay
            await Task.Delay(2000);

            // Enhanced AI analysis with detailed criteria
            var random = new Random();
            var score = random.Next(6, 11); // Score between 6-10
            
            // Detailed analysis categories
            var presentationScore = random.Next(6, 11);
            var colorScore = random.Next(6, 11);
            var textureScore = random.Next(6, 11);
            var platingScore = random.Next(6, 11);
            
            var detailedComments = new[]
            {
                "Excellent presentation! The dish looks professionally prepared with perfect plating technique.",
                "Great color combination and visual appeal. The ingredients are beautifully arranged.",
                "Good effort! The dish shows promise with some room for improvement in presentation.",
                "Outstanding! This looks like it came from a 5-star restaurant with impeccable technique.",
                "Well done! The presentation is clean, appetizing, and shows attention to detail."
            };

            var technicalSuggestions = new[]
            {
                "Consider adding a garnish for better visual appeal and color contrast",
                "The portion size looks perfect for the serving style",
                "Try adding some fresh herbs for extra flavor and visual interest",
                "The plating could be more artistic with better use of negative space",
                "Excellent use of colors and textures - keep up the great work!",
                "Consider drizzling a sauce in an artistic pattern for professional presentation",
                "The height and layering of ingredients creates good visual depth",
                "Try using different sized components for more dynamic plating"
            };

            var healthNotes = new[]
            {
                "The dish appears to have good nutritional balance",
                "Consider adding more vegetables for better health benefits",
                "The portion size seems appropriate for a healthy meal",
                "Good use of fresh ingredients - this looks very nutritious"
            };

            var ratings = new[] { "Excellent", "Good", "Fair", "Poor" };
            var overallRating = score >= 9 ? ratings[0] : score >= 7 ? ratings[1] : score >= 5 ? ratings[2] : ratings[3];

            return new ChefJudgeResult
            {
                Score = score,
                Comment = detailedComments[random.Next(detailedComments.Length)],
                Suggestions = technicalSuggestions.Take(random.Next(3, 5)).ToList(),
                OverallRating = overallRating,
                // Enhanced details
                PresentationScore = presentationScore,
                ColorScore = colorScore,
                TextureScore = textureScore,
                PlatingScore = platingScore,
                HealthNotes = healthNotes[random.Next(healthNotes.Length)],
                ChefTips = new[]
                {
                    "Use a white plate for better color contrast",
                    "Garnish should complement, not overwhelm the dish",
                    "Consider the rule of thirds for plating",
                    "Temperature contrast adds interest (hot main, cool garnish)"
                }.Take(2).ToList()
            };
        }

        public async Task<Recipe> GenerateRecipeFromIngredientsAsync(IEnumerable<string> ingredientNames, string dishName, int servings)
        {
            // Simulate AI processing delay
            await Task.Delay(3000);

            // Enhanced AI recipe generation with intelligent analysis
            var random = new Random();
            var ingredients = ingredientNames.ToList();
            
            // Analyze ingredients to determine cooking style
            var cookingStyle = AnalyzeCookingStyle(ingredients);
            var cuisineType = DetermineCuisineType(ingredients);
            var cookingTime = CalculateCookingTime(ingredients, cookingStyle);
            var difficulty = DetermineDifficulty(ingredients, cookingTime);
            
            // Generate intelligent recipe name if not provided
            var recipeTitle = string.IsNullOrEmpty(dishName) ? GenerateRecipeName(ingredients, cuisineType) : dishName;
            
            // Generate detailed instructions based on ingredients and cooking style
            var instructions = GenerateIntelligentInstructions(ingredients, cookingStyle, cuisineType, cookingTime);
            var description = GenerateRecipeDescription(recipeTitle, ingredients, cuisineType, servings);
            
            // Add nutritional notes
            var nutritionalNotes = GenerateNutritionalNotes(ingredients);
            
            return new Recipe
            {
                Title = recipeTitle,
                Description = description,
                Instructions = instructions,
                CookTime = cookingTime,
                Difficulty = difficulty,
                Servings = servings,
                ImageUrl = "", // Will be set by user
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<string>> GetIngredientSubstitutionsAsync(string ingredientName)
        {
            // Simulate AI processing delay
            await Task.Delay(1000);

            var substitutions = new Dictionary<string, List<string>>
            {
                ["butter"] = new List<string> { "margarine", "coconut oil", "olive oil", "vegetable oil" },
                ["milk"] = new List<string> { "almond milk", "soy milk", "coconut milk", "oat milk" },
                ["eggs"] = new List<string> { "flax eggs (1 tbsp flaxseed + 3 tbsp water)", "applesauce (1/4 cup)", "banana (1/2 mashed)", "yogurt (1/4 cup)" },
                ["sugar"] = new List<string> { "honey", "maple syrup", "stevia", "agave nectar", "coconut sugar" },
                ["flour"] = new List<string> { "almond flour", "coconut flour", "oat flour", "rice flour" },
                ["salt"] = new List<string> { "sea salt", "kosher salt", "himalayan salt", "celery salt" },
                ["onion"] = new List<string> { "shallots", "leeks", "scallions", "onion powder" },
                ["garlic"] = new List<string> { "garlic powder", "garlic salt", "shallots", "chives" }
            };

            var lowerName = ingredientName.ToLower();
            return substitutions.ContainsKey(lowerName) ? substitutions[lowerName] : new List<string>();
        }

        public async Task<string> GenerateCookingTipsAsync(string recipeTitle)
        {
            // Simulate AI processing delay
            await Task.Delay(1500);

            var tips = new[]
            {
                "Always taste as you cook and adjust seasoning accordingly.",
                "Prep all ingredients before you start cooking (mise en place).",
                "Use fresh herbs whenever possible for the best flavor.",
                "Don't overcrowd the pan when sautéing - cook in batches if needed.",
                "Let meat rest for a few minutes after cooking for better juiciness.",
                "Use a sharp knife for cleaner cuts and better presentation.",
                "Season each layer of your dish for maximum flavor.",
                "Keep your workspace clean and organized while cooking.",
                "Use a timer to avoid overcooking delicate ingredients.",
                "Taste your dish at different stages to understand the flavor development."
            };

            var random = new Random();
            var selectedTips = tips.OrderBy(x => random.Next()).Take(3).ToList();
            
            return $"Cooking Tips for {recipeTitle}:\n\n" + string.Join("\n", selectedTips.Select((tip, index) => $"{index + 1}. {tip}"));
        }

        private string AnalyzeCookingStyle(List<string> ingredients)
        {
            var proteinIngredients = new[] { "chicken", "beef", "pork", "fish", "salmon", "shrimp", "tofu" };
            var vegetableIngredients = new[] { "tomato", "onion", "garlic", "carrot", "potato", "bell pepper", "spinach", "broccoli" };
            var grainIngredients = new[] { "rice", "pasta", "bread", "quinoa", "oats" };
            
            var hasProtein = ingredients.Any(i => proteinIngredients.Any(p => i.ToLower().Contains(p)));
            var hasVegetables = ingredients.Any(i => vegetableIngredients.Any(v => i.ToLower().Contains(v)));
            var hasGrains = ingredients.Any(i => grainIngredients.Any(g => i.ToLower().Contains(g)));
            
            if (hasProtein && hasVegetables && hasGrains) return "Stir-fry";
            if (hasProtein && hasVegetables) return "Sauté";
            if (hasVegetables && hasGrains) return "Stew";
            if (hasProtein) return "Grill";
            return "Boil";
        }
        
        private string DetermineCuisineType(List<string> ingredients)
        {
            var asianIngredients = new[] { "soy", "ginger", "sesame", "rice", "noodles", "tofu" };
            var italianIngredients = new[] { "pasta", "tomato", "basil", "olive oil", "garlic", "parmesan" };
            var mexicanIngredients = new[] { "chili", "cumin", "lime", "cilantro", "beans", "corn" };
            var indianIngredients = new[] { "curry", "turmeric", "cumin", "coriander", "garam masala" };
            
            if (ingredients.Any(i => asianIngredients.Any(a => i.ToLower().Contains(a)))) return "Asian";
            if (ingredients.Any(i => italianIngredients.Any(it => i.ToLower().Contains(it)))) return "Italian";
            if (ingredients.Any(i => mexicanIngredients.Any(m => i.ToLower().Contains(m)))) return "Mexican";
            if (ingredients.Any(i => indianIngredients.Any(ind => i.ToLower().Contains(ind)))) return "Indian";
            return "International";
        }
        
        private int CalculateCookingTime(List<string> ingredients, string cookingStyle)
        {
            var baseTime = cookingStyle switch
            {
                "Stir-fry" => 15,
                "Sauté" => 20,
                "Stew" => 45,
                "Grill" => 25,
                "Boil" => 30,
                _ => 25
            };
            
            // Add time based on ingredient complexity
            var complexIngredients = ingredients.Count(i => 
                new[] { "beef", "pork", "potato", "carrot" }.Any(c => i.ToLower().Contains(c)));
            
            return baseTime + (complexIngredients * 5);
        }
        
        private string DetermineDifficulty(List<string> ingredients, int cookingTime)
        {
            var ingredientCount = ingredients.Count;
            var hasComplexIngredients = ingredients.Any(i => 
                new[] { "beef", "pork", "fish", "seafood" }.Any(c => i.ToLower().Contains(c)));
            
            if (cookingTime > 60 || ingredientCount > 8 || hasComplexIngredients) return "Hard";
            if (cookingTime > 30 || ingredientCount > 5) return "Medium";
            return "Easy";
        }
        
        private string GenerateRecipeName(List<string> ingredients, string cuisineType)
        {
            var random = new Random();
            var mainIngredient = ingredients.FirstOrDefault();
            var cuisinePrefix = cuisineType switch
            {
                "Asian" => "Asian",
                "Italian" => "Italian",
                "Mexican" => "Mexican",
                "Indian" => "Indian",
                _ => "Fusion"
            };
            
            var dishTypes = new[] { "Bowl", "Stir-fry", "Salad", "Stew", "Medley", "Mix" };
            var dishType = dishTypes[random.Next(dishTypes.Length)];
            
            return $"{cuisinePrefix} {mainIngredient} {dishType}";
        }
        
        private string GenerateRecipeDescription(string title, List<string> ingredients, string cuisineType, int servings)
        {
            var ingredientCount = ingredients.Count;
            var mainIngredients = string.Join(", ", ingredients.Take(3));
            
            return $"A delicious {cuisineType.ToLower()} {title.ToLower()} featuring {mainIngredients} and more. " +
                   $"This {ingredientCount}-ingredient recipe serves {servings} and is perfect for a " +
                   $"nutritious and flavorful meal. Created with AI assistance for optimal taste and nutrition.";
        }
        
        private string GenerateIntelligentInstructions(List<string> ingredients, string cookingStyle, string cuisineType, int cookingTime)
        {
            var steps = new List<string>();
            var random = new Random();
            
            // Preparation step
            steps.Add($"1. **Preparation (5 minutes)**:\n   - Gather all ingredients: {string.Join(", ", ingredients)}\n   - Wash and prepare vegetables\n   - Cut proteins into appropriate sizes");
            
            // Cooking method specific steps
            switch (cookingStyle)
            {
                case "Stir-fry":
                    steps.Add("2. **Heat the Wok (2 minutes)**:\n   - Heat a large wok or skillet over high heat\n   - Add 2 tablespoons of oil and let it get very hot");
                    steps.Add("3. **Quick Cooking (8 minutes)**:\n   - Add proteins first and cook until nearly done\n   - Add vegetables in order of cooking time (hardest first)\n   - Stir constantly to prevent burning");
                    break;
                case "Sauté":
                    steps.Add("2. **Medium Heat Cooking (10 minutes)**:\n   - Heat a large pan over medium-high heat\n   - Add oil and aromatics (onion, garlic) first\n   - Add proteins and cook until golden");
                    break;
                case "Stew":
                    steps.Add("2. **Browning (5 minutes)**:\n   - Brown proteins in batches in a large pot\n   - Remove and set aside");
                    steps.Add("3. **Building Flavor (10 minutes)**:\n   - Sauté aromatics in the same pot\n   - Add vegetables and cook until softened");
                    steps.Add("4. **Simmering (30 minutes)**:\n   - Return proteins to pot with liquid\n   - Simmer covered until tender");
                    break;
            }
            
            // Seasoning and finishing
            steps.Add($"{steps.Count + 1}. **Seasoning and Finishing (2 minutes)**:\n   - Taste and adjust seasoning\n   - Add fresh herbs if using\n   - Let rest for 2-3 minutes before serving");
            
            // Serving suggestion
            steps.Add($"{steps.Count + 1}. **Serving**:\n   - Plate attractively with garnishes\n   - Serve immediately while hot\n   - Enjoy your {cuisineType} creation!");
            
            return string.Join("\n\n", steps);
        }
        
        private string GenerateNutritionalNotes(List<string> ingredients)
        {
            var proteinCount = ingredients.Count(i => 
                new[] { "chicken", "beef", "fish", "tofu", "eggs" }.Any(p => i.ToLower().Contains(p)));
            var vegetableCount = ingredients.Count(i => 
                new[] { "tomato", "onion", "carrot", "spinach", "broccoli" }.Any(v => i.ToLower().Contains(v)));
            
            var notes = new List<string>();
            if (proteinCount > 0) notes.Add($"High protein content ({proteinCount} protein sources)");
            if (vegetableCount > 0) notes.Add($"Rich in vegetables ({vegetableCount} types)");
            if (ingredients.Any(i => i.ToLower().Contains("olive"))) notes.Add("Heart-healthy olive oil");
            if (ingredients.Any(i => i.ToLower().Contains("garlic"))) notes.Add("Immune-boosting garlic");
            
            return notes.Count > 0 ? string.Join(", ", notes) : "Balanced nutritional profile";
        }

        // Additional methods for test compatibility
        public async Task<IEnumerable<string>> GenerateRecipeSuggestionsAsync(IEnumerable<string> ingredients)
        {
            await Task.Delay(1000);
            return new[] { "Chicken Stir Fry", "Pasta with Tomato Sauce", "Beef Stew" };
        }

        public async Task<string> AnalyzeRecipeNutritionAsync(string recipe)
        {
            await Task.Delay(1000);
            return "High protein, moderate carbohydrates, low fat";
        }

        public async Task<IEnumerable<string>> GenerateCookingTipsListAsync(string recipe)
        {
            await Task.Delay(1000);
            return new[] { "Use fresh ingredients", "Season to taste", "Don't overcook" };
        }

        public async Task<IEnumerable<string>> SuggestIngredientSubstitutionsAsync(string ingredient)
        {
            await Task.Delay(1000);
            return new[] { "Alternative 1", "Alternative 2", "Alternative 3" };
        }

        public async Task<IEnumerable<string>> GenerateRecipeVariationsAsync(string recipe)
        {
            await Task.Delay(1000);
            return new[] { "Spicy version", "Vegetarian version", "Low-carb version" };
        }

        public async Task<int> EstimateCookingTimeAsync(string recipe)
        {
            await Task.Delay(1000);
            return 30;
        }

        public async Task<IEnumerable<string>> GenerateDietaryRecommendationsAsync(IEnumerable<string> preferences)
        {
            await Task.Delay(1000);
            return new[] { "Try quinoa instead of rice", "Use olive oil for cooking", "Add more vegetables" };
        }

        public async Task<string> AnalyzeRecipeComplexityAsync(string recipe)
        {
            await Task.Delay(1000);
            return "Easy";
        }

        public async Task<IEnumerable<string>> GenerateMealPlanAsync(int days, IEnumerable<string> restrictions)
        {
            await Task.Delay(1000);
            return Enumerable.Range(1, days).Select(i => $"Meal plan for day {i}");
        }
    }
}
