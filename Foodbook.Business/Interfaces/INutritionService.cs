using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface INutritionService
    {
        Task<NutritionAnalysisResult> AnalyzeRecipeNutritionAsync(Recipe recipe);
        Task<NutritionAnalysisResult> AnalyzeMealPlanNutritionAsync(IEnumerable<Recipe> recipes);
        Task<NutritionRecommendation> GetNutritionRecommendationsAsync(NutritionAnalysisResult analysis, string userGoal);
        Task<IEnumerable<HealthAlert>> GetHealthAlertsAsync(NutritionAnalysisResult analysis);
        Task<NutritionComparison> CompareNutritionAsync(Recipe recipe1, Recipe recipe2);
        Task<NutritionAnalysisResult> AnalyzeUnstructuredRecipeAsync(string recipeText);
        
        // New methods for AI-powered nutrition analysis
        Task<NutritionAnalysisResult> AnalyzeNutritionWithAIAsync(string recipeText, string userGoal = "general health");
        Task<List<ParsedIngredient>> ParseRecipeTextWithAIAsync(string recipeText);
        Task<string> GetAIHealthFeedbackAsync(NutritionAnalysisResult analysis, string userGoal = "general health");
    }

    public class NutritionAnalysisResult
    {
        public decimal TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFat { get; set; }
        public decimal TotalFiber { get; set; }
        public decimal TotalSugar { get; set; }
        public decimal TotalSodium { get; set; }
        public decimal TotalCholesterol { get; set; }
        public decimal TotalSaturatedFat { get; set; }
        public decimal TotalTransFat { get; set; }
        public List<VitaminInfo> Vitamins { get; set; } = new();
        public List<MineralInfo> Minerals { get; set; } = new();
        public NutritionRating Rating { get; set; } = new();
        public List<HealthAlert> Alerts { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string AnalysisSummary { get; set; } = string.Empty;
    }

    public class VitaminInfo
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal DailyValue { get; set; }
        public string Benefits { get; set; } = string.Empty;
    }

    public class MineralInfo
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal DailyValue { get; set; }
        public string Benefits { get; set; } = string.Empty;
    }

    public class NutritionRating
    {
        public int OverallScore { get; set; } // 1-100
        public string Grade { get; set; } = string.Empty; // A, B, C, D, F
        public string Description { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public List<string> Improvements { get; set; } = new();
    }

    public class HealthAlert
    {
        public string Type { get; set; } = string.Empty; // Warning, Info, Success
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class NutritionRecommendation
    {
        public string Goal { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public List<string> FoodsToAdd { get; set; } = new();
        public List<string> FoodsToReduce { get; set; } = new();
        public string MealTiming { get; set; } = string.Empty;
        public string Hydration { get; set; } = string.Empty;
    }

    public class NutritionComparison
    {
        public Recipe Recipe1 { get; set; } = new();
        public Recipe Recipe2 { get; set; } = new();
        public NutritionAnalysisResult Nutrition1 { get; set; } = new();
        public NutritionAnalysisResult Nutrition2 { get; set; } = new();
        public List<ComparisonItem> Comparisons { get; set; } = new();
        public string Winner { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class ComparisonItem
    {
        public string Nutrient { get; set; } = string.Empty;
        public decimal Recipe1Value { get; set; }
        public decimal Recipe2Value { get; set; }
        public string BetterRecipe { get; set; } = string.Empty;
        public decimal Difference { get; set; }
    }

    public class ParsedIngredient
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
