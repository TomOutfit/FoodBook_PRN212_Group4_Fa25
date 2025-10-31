using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Commands;

namespace Foodbook.Presentation.ViewModels
{
	public class AnalyticsViewModel : BaseViewModel
	{
		private readonly IRecipeService? _recipeService;
		private readonly IIngredientService? _ingredientService;
		private readonly ILoggingService? _loggingService;

		private bool _isLoading;
		private string _errorMessage = string.Empty;

		private ISeries[] _recipeTrendSeries = Array.Empty<ISeries>();
		private ISeries[] _aiPerformanceSeries = Array.Empty<ISeries>();
		private ISeries[] _cookTimeDistributionSeries = Array.Empty<ISeries>();

		private int _mainDishesCount;
		private int _dessertsCount;
		private int _quickMealsCount;
		private double _pantryCoveragePercent;

		// Difficulty buckets
		private int _easyDifficultyCount;
		private int _mediumDifficultyCount;
		private int _hardDifficultyCount;

		// Cooking time buckets
		private int _cookingTime0To30Count;
		private int _cookingTime30To60Count;
		private int _cookingTime60To90Count;
		private int _cookingTime90To120Count;
		private int _cookingTime120PlusCount;

		// AI metrics (basic placeholders until telemetry exists)
		private double _aiEngagementPercent;
		private double _successRate;

		// Monthly counts for simple polyline
		private IReadOnlyList<int> _monthlyRecipeCounts = Array.Empty<int>();

		// Dashboard KPIs
		private int _myTotalRecipes;
		private int _totalIngredients;
		private int _expiringSoonCount;
		private int _expiredCount;
		private int _aiOperationsCount;
		private IReadOnlyList<KeyValuePair<string, int>> _topIngredients = Array.Empty<KeyValuePair<string, int>>();

		public ISeries[] RecipeTrendSeries
		{
			get => _recipeTrendSeries;
			private set { _recipeTrendSeries = value; OnPropertyChanged(); }
		}

		public ISeries[] AIPerformanceSeries
		{
			get => _aiPerformanceSeries;
			private set { _aiPerformanceSeries = value; OnPropertyChanged(); }
		}

		public ISeries[] CookTimeDistributionSeries
		{
			get => _cookTimeDistributionSeries;
			private set { _cookTimeDistributionSeries = value; OnPropertyChanged(); }
		}

		public int MainDishesCount
		{
			get => _mainDishesCount;
			private set { _mainDishesCount = value; OnPropertyChanged(); }
		}

		public int DessertsCount
		{
			get => _dessertsCount;
			private set { _dessertsCount = value; OnPropertyChanged(); }
		}

		public int QuickMealsCount
		{
			get => _quickMealsCount;
			private set { _quickMealsCount = value; OnPropertyChanged(); }
		}

		public double PantryCoveragePercent
		{
			get => _pantryCoveragePercent;
			private set { _pantryCoveragePercent = value; OnPropertyChanged(); }
		}

		public int MyTotalRecipes
		{
			get => _myTotalRecipes;
			private set { _myTotalRecipes = value; OnPropertyChanged(); }
		}

		public int TotalIngredients
		{
			get => _totalIngredients;
			private set { _totalIngredients = value; OnPropertyChanged(); }
		}

		public int ExpiringSoonCount
		{
			get => _expiringSoonCount;
			private set { _expiringSoonCount = value; OnPropertyChanged(); }
		}

		public int ExpiredCount
		{
			get => _expiredCount;
			private set { _expiredCount = value; OnPropertyChanged(); }
		}

		public int AIOperationsCount
		{
			get => _aiOperationsCount;
			private set { _aiOperationsCount = value; OnPropertyChanged(); }
		}

		public double AIEngagementPercent
		{
			get => _aiEngagementPercent;
			private set { _aiEngagementPercent = value; OnPropertyChanged(); }
		}

		public double SuccessRate
		{
			get => _successRate;
			private set { _successRate = value; OnPropertyChanged(); }
		}

		public int EasyDifficultyCount { get => _easyDifficultyCount; private set { _easyDifficultyCount = value; OnPropertyChanged(); } }
		public int MediumDifficultyCount { get => _mediumDifficultyCount; private set { _mediumDifficultyCount = value; OnPropertyChanged(); } }
		public int HardDifficultyCount { get => _hardDifficultyCount; private set { _hardDifficultyCount = value; OnPropertyChanged(); } }

		public int CookingTime0To30Count { get => _cookingTime0To30Count; private set { _cookingTime0To30Count = value; OnPropertyChanged(); } }
		public int CookingTime30To60Count { get => _cookingTime30To60Count; private set { _cookingTime30To60Count = value; OnPropertyChanged(); } }
		public int CookingTime60To90Count { get => _cookingTime60To90Count; private set { _cookingTime60To90Count = value; OnPropertyChanged(); } }
		public int CookingTime90To120Count { get => _cookingTime90To120Count; private set { _cookingTime90To120Count = value; OnPropertyChanged(); } }
		public int CookingTime120PlusCount { get => _cookingTime120PlusCount; private set { _cookingTime120PlusCount = value; OnPropertyChanged(); } }

		public IReadOnlyList<int> MonthlyRecipeCounts
		{
			get => _monthlyRecipeCounts;
			private set { _monthlyRecipeCounts = value; OnPropertyChanged(); }
		}

		// Provide a simple label to satisfy bindings in DashboardView
		public string Loc_Dashboard => "Dashboard";

		public IReadOnlyList<KeyValuePair<string, int>> TopIngredients
		{
			get => _topIngredients;
			private set { _topIngredients = value; OnPropertyChanged(); }
		}

		public bool IsLoading
		{
			get => _isLoading;
			private set { _isLoading = value; OnPropertyChanged(); }
		}

		public string ErrorMessage
		{
			get => _errorMessage;
			private set { _errorMessage = value; OnPropertyChanged(); }
		}

		public ICommand LoadAnalyticsCommand { get; }
		public ICommand RefreshDashboardCommand { get; }

		public AnalyticsViewModel(
			IRecipeService recipeService,
			IIngredientService ingredientService,
			ILoggingService loggingService)
		{
			_recipeService = recipeService;
			_ingredientService = ingredientService;
			_loggingService = loggingService;

			LoadAnalyticsCommand = new RelayCommand(async () => await LoadAnalyticsDataAsync());
			RefreshDashboardCommand = new RelayCommand(async () => await LoadAnalyticsDataAsync());
		}

		// For design-time only
		public AnalyticsViewModel()
		{
			LoadAnalyticsCommand = new RelayCommand(async () => await Task.CompletedTask);
			RefreshDashboardCommand = new RelayCommand(async () => await Task.CompletedTask);
		}

		public async Task LoadAnalyticsDataAsync()
		{
			if (IsLoading) return;
			IsLoading = true;
			ErrorMessage = string.Empty;

			try
			{
				// Load real data from DB via services
				var recipes = await GetAllRecipesOrEmptyAsync();

				// If service returns null, keep charts empty (no hardcoded data)
				if (recipes.Count == 0) { SetEmptyCharts(); SetEmptyKpis(); return; }

				CalculateKpis(recipes);
				BuildCharts(recipes);
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
				await (_loggingService?.LogErrorAsync("Analytics", "system", ex, "LoadAnalyticsData") ?? Task.CompletedTask);
				SetEmptyCharts();
			}
			finally
			{
				IsLoading = false;
			}
		}

		public async Task LoadDashboardDataAsync()
		{
			if (IsLoading) return;
			IsLoading = true;
			ErrorMessage = string.Empty;
			try
			{
				// Recipes
				var recipes = await GetAllRecipesOrEmptyAsync();
				MyTotalRecipes = recipes.Count;

				// Ingredients
				var ingredients = await TryGetIngredientsAsync();
				TotalIngredients = ingredients.Count;

				var now = DateTime.UtcNow;
				ExpiringSoonCount = ingredients.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value >= now && (i.ExpiryDate.Value - now).TotalDays <= 3);
				ExpiredCount = ingredients.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value < now);

				// Pantry coverage: % items that are stocked (qty >= min) and not expired
				var stockedCount = ingredients.Count(i =>
					(i.Quantity ?? 0) >= (i.MinQuantity ?? 0) && (!i.ExpiryDate.HasValue || i.ExpiryDate.Value >= now));
				PantryCoveragePercent = TotalIngredients > 0
					? Math.Round(stockedCount * 100.0 / TotalIngredients, 1)
					: 0;

				// Top ingredients by frequency (category/name counts)
				TopIngredients = ingredients
					.GroupBy(i => i.Name)
					.Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
					.OrderByDescending(kv => kv.Value)
					.Take(5)
					.ToList();

				// AI telemetry from logs (FoodBook.sql seeds include AI logs)
				var logs = _loggingService != null
					? (await _loggingService.GetLogsAsync())?.ToList() ?? new List<Foodbook.Data.Entities.LogEntry>()
					: new List<Foodbook.Data.Entities.LogEntry>();

				var aiLogs = logs
					.Where(l =>
						(l.Source != null && l.Source.Contains("FoodBook.AI", StringComparison.OrdinalIgnoreCase))
						|| (l.LogType != null && l.LogType.Contains("AI", StringComparison.OrdinalIgnoreCase))
						|| (l.FeatureName != null && l.FeatureName.StartsWith("AI", StringComparison.OrdinalIgnoreCase)))
					.ToList();

				AIOperationsCount = aiLogs.Count;
				AIEngagementPercent = logs.Count > 0
					? Math.Round(aiLogs.Count * 100.0 / logs.Count, 1)
					: 0;

				var aiSuccesses = aiLogs.Count(l => string.Equals(l.Level, "Information", StringComparison.OrdinalIgnoreCase));
				SuccessRate = aiLogs.Count > 0
					? Math.Round(aiSuccesses * 100.0 / aiLogs.Count, 1)
					: 0;
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
				await (_loggingService?.LogErrorAsync("Dashboard", "system", ex, "LoadDashboardData") ?? Task.CompletedTask);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private async Task<IReadOnlyList<Recipe>> GetAllRecipesOrEmptyAsync()
		{
			try
			{
				if (_recipeService == null) return Array.Empty<Recipe>();
				var list = await _recipeService.GetAllRecipesAsync();
				if (list == null) return Array.Empty<Recipe>();
				return list.ToList();
			}
			catch { return Array.Empty<Recipe>(); }
		}

		private async Task<List<Ingredient>> TryGetIngredientsAsync()
		{
			if (_ingredientService == null)
			{
				return new List<Ingredient>();
			}
			try
			{
				// Try a broad search to fetch all
				var results = await _ingredientService.SearchIngredientsAsync("");
				return results?.ToList() ?? new List<Ingredient>();
			}
			catch
			{
				return new List<Ingredient>();
			}
		}

		private void CalculateKpis(IReadOnlyList<Recipe> recipes)
		{
			MainDishesCount = recipes.Count(r => string.Equals(r.Category, "Main", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(r.Category, "MainDish", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(r.Category, "Main Dishes", StringComparison.OrdinalIgnoreCase));

			DessertsCount = recipes.Count(r => string.Equals(r.Category, "Dessert", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(r.Category, "Desserts", StringComparison.OrdinalIgnoreCase));

			QuickMealsCount = recipes.Count(r =>
				string.Equals(r.Category, "Quick Meals", StringComparison.OrdinalIgnoreCase)
				|| r.CookTime > 0 && r.CookTime <= 30);

			// Pantry coverage is computed in dashboard load where ingredients are available
			PantryCoveragePercent = PantryCoveragePercent;

			// Difficulty buckets (assumes Difficulty is one of Easy/Medium/Hard or similar)
			EasyDifficultyCount = recipes.Count(r => string.Equals(r.Difficulty, "Easy", StringComparison.OrdinalIgnoreCase));
			MediumDifficultyCount = recipes.Count(r => string.Equals(r.Difficulty, "Medium", StringComparison.OrdinalIgnoreCase));
			HardDifficultyCount = recipes.Count(r => string.Equals(r.Difficulty, "Hard", StringComparison.OrdinalIgnoreCase));

			// Cook time buckets
			CookingTime0To30Count = recipes.Count(r => r.CookTime > 0 && r.CookTime <= 30);
			CookingTime30To60Count = recipes.Count(r => r.CookTime > 30 && r.CookTime <= 60);
			CookingTime60To90Count = recipes.Count(r => r.CookTime > 60 && r.CookTime <= 90);
			CookingTime90To120Count = recipes.Count(r => r.CookTime > 90 && r.CookTime <= 120);
			CookingTime120PlusCount = recipes.Count(r => r.CookTime > 120);

			// AI metrics are computed in dashboard load from logs
		}

		private void BuildCharts(IReadOnlyList<Recipe> recipes)
		{
			// Trend: group by month of CreatedAt if available; else leave empty
			var hasDate = recipes.Any(r => r.CreatedAt != default);
			if (hasDate)
			{
				var byMonth = recipes
					.Where(r => r.CreatedAt != default)
					.GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
					.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
					.Select(g => (label: $"{g.Key.Year}-{g.Key.Month:00}", count: g.Count()))
					.ToList();

				RecipeTrendSeries = new ISeries[]
				{
					new ColumnSeries<int>
					{
						Name = "Recipes",
						Values = byMonth.Select(x => x.count).ToArray(),
						Fill = new SolidColorPaint(new SKColor(33, 150, 243))
					}
				};
			}
			else
			{
				RecipeTrendSeries = Array.Empty<ISeries>();
			}

			// Monthly counts for current year (1..12)
			var year = DateTime.Now.Year;
			var monthCounts = Enumerable.Range(1, 12)
				.Select(m => recipes.Count(r => r.CreatedAt.Year == year && r.CreatedAt.Month == m))
				.ToList();
			MonthlyRecipeCounts = monthCounts;

			// AI performance: if no real metrics yet, leave empty (no hardcode)
			AIPerformanceSeries = Array.Empty<ISeries>();

			// Cook time distribution: group by ranges if CookTime available; else empty
			var hasCook = recipes.Any(r => r.CookTime > 0);
			if (hasCook)
			{
				int Short(Recipe r) => r.CookTime <= 15 ? 1 : 0;
				int Medium(Recipe r) => r.CookTime > 15 && r.CookTime <= 45 ? 1 : 0;
				int Long(Recipe r) => r.CookTime > 45 ? 1 : 0;

				var shorts = recipes.Sum(Short);
				var mediums = recipes.Sum(Medium);
				var longs = recipes.Sum(Long);

				CookTimeDistributionSeries = new ISeries[]
				{
					new PieSeries<int> { Name = "≤15'", Values = new[] { shorts }, Fill = new SolidColorPaint(new SKColor(76, 175, 80)) },
					new PieSeries<int> { Name = "16-45'", Values = new[] { mediums }, Fill = new SolidColorPaint(new SKColor(255, 193, 7)) },
					new PieSeries<int> { Name = ">45'", Values = new[] { longs }, Fill = new SolidColorPaint(new SKColor(244, 67, 54)) }
				};
			}
			else
			{
				CookTimeDistributionSeries = Array.Empty<ISeries>();
			}
		}

		private void SetEmptyCharts()
		{
			RecipeTrendSeries = Array.Empty<ISeries>();
			AIPerformanceSeries = Array.Empty<ISeries>();
			CookTimeDistributionSeries = Array.Empty<ISeries>();
		}

		private void SetEmptyKpis()
		{
			MainDishesCount = 0;
			DessertsCount = 0;
			PantryCoveragePercent = 0;
		}
	}
}