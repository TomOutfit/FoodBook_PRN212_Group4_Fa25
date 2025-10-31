// Foodbook.Presentation/ViewModels/RecipeListViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Commands;
using System.Windows;
using Foodbook.Presentation.Views;

namespace Foodbook.Presentation.ViewModels
{
    public class RecipeListViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService = null!;
        private readonly IUnsplashImageService? _unsplashImageService;

        private List<Recipe> _allRecipes = new();
        private ObservableCollection<Recipe> _recipes = new();
        private ObservableCollection<Recipe> _pagedRecipes = new();
        
        // Paging
        private int _pageSize = 9;
        private int _currentPage = 1;
        private int _totalPages;
        private ObservableCollection<int> _pageOptions = new();
        private int _selectedPage = 1;
        
        // Search/Filter/Sort
        private string _searchText = string.Empty;
        private string _sortBy = "Name A-Z";
        private string _selectedCategory = "All";
        
        // KPIs shown in header
        private double _myAverageCookTime;
        private string _myMostCommonDifficulty = string.Empty;
        private int _myAIGeneratedRecipes; // if no indicator available, will remain 0

        // State
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // Exposed data
        public ObservableCollection<Recipe> Recipes { get => _recipes; private set => SetProperty(ref _recipes, value); }
        public ObservableCollection<Recipe> PagedRecipes { get => _pagedRecipes; private set => SetProperty(ref _pagedRecipes, value); }

        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    _selectedPage = _currentPage;
                    OnPropertyChanged(nameof(SelectedPage));
                    UpdatePaging();
                }
            }
        }

        public int TotalPages { get => _totalPages; private set => SetProperty(ref _totalPages, value); }
        public ObservableCollection<int> PageOptions { get => _pageOptions; private set => SetProperty(ref _pageOptions, value); }

        public int SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (SetProperty(ref _selectedPage, value))
                {
                    if (_selectedPage >= 1 && _selectedPage <= Math.Max(1, TotalPages))
                    {
                        _currentPage = _selectedPage;
                        OnPropertyChanged(nameof(CurrentPage));
                        UpdatePaging();
                    }
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // live filter local list without hitting the service
                    ApplyAllTransforms();
                }
            }
        }

        public string SortBy
        {
            get => _sortBy;
            set
            {
                if (SetProperty(ref _sortBy, value))
                {
                    ApplyAllTransforms();
                }
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            private set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    ApplyAllTransforms();
                }
            }
        }

        public double MyAverageCookTime { get => _myAverageCookTime; private set => SetProperty(ref _myAverageCookTime, value); }
        public string MyMostCommonDifficulty { get => _myMostCommonDifficulty; private set => SetProperty(ref _myMostCommonDifficulty, value); }
        public int MyAIGeneratedRecipes { get => _myAIGeneratedRecipes; private set => SetProperty(ref _myAIGeneratedRecipes, value); }

        public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
        public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
        
        // Commands
        public ICommand LoadRecipesCommand { get; }
        public ICommand SearchRecipesCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand AddRecipeCommand { get; }
        public ICommand AnalyzePantryCommand { get; }
        public ICommand ViewRecipeAnalyticsCommand { get; }
        public ICommand ViewRecipeCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand FilterByCategoryCommand { get; }
        
        public RecipeListViewModel(IRecipeService recipeService, ILoggingService loggingService)
        {
            _recipeService = recipeService;
            _unsplashImageService = null;

            LoadRecipesCommand = new RelayCommand(async () => await LoadRecipesAsync(), () => true);
            SearchRecipesCommand = new RelayCommand(async () => await SearchViaServiceAsync(), () => true);
            NextPageCommand = new RelayCommand(new Action(() => { if (CurrentPage < TotalPages) CurrentPage += 1; }), () => true);
            PrevPageCommand = new RelayCommand(new Action(() => { if (CurrentPage > 1) CurrentPage -= 1; }), () => true);
            AddRecipeCommand = new RelayCommand(async () => await OpenAddRecipeDialogAsync(), () => true);
            AnalyzePantryCommand = new RelayCommand(new Action(() => NavigateToAnalytics()), () => true);
            ViewRecipeAnalyticsCommand = new RelayCommand(new Action(() => NavigateToAnalytics()), () => true);
            ViewRecipeCommand = new RelayCommand<Recipe>(async r => await OpenViewEditRecipeDialogAsync(r), r => r != null);
            EditRecipeCommand = new RelayCommand<Recipe>(async r => await OpenViewEditRecipeDialogAsync(r), r => r != null);
            DeleteRecipeCommand = new RelayCommand<Recipe>(_ => { /* optionally call service to delete then refresh */ }, _ => true);
            FilterByCategoryCommand = new RelayCommand<string>(async c => await FilterRecipesByCategoryAsync(c), _ => true);
        }

        // For design-time
        public RecipeListViewModel()
        {
            // Initialize commands to safe no-ops for design-time to avoid nulls
            LoadRecipesCommand = new RelayCommand(new Action(() => { }), () => true);
            SearchRecipesCommand = new RelayCommand(new Action(() => { }), () => true);
            NextPageCommand = new RelayCommand(new Action(() => { }), () => true);
            PrevPageCommand = new RelayCommand(new Action(() => { }), () => true);
            AddRecipeCommand = new RelayCommand(new Action(() => { }), () => true);
            AnalyzePantryCommand = new RelayCommand(new Action(() => { }), () => true);
            ViewRecipeAnalyticsCommand = new RelayCommand(new Action(() => { }), () => true);
            ViewRecipeCommand = new RelayCommand<Recipe>(_ => { }, _ => true);
            EditRecipeCommand = new RelayCommand<Recipe>(_ => { }, _ => true);
            DeleteRecipeCommand = new RelayCommand<Recipe>(_ => { }, _ => true);
            FilterByCategoryCommand = new RelayCommand<string>(_ => { }, _ => true);
        }

        public RecipeListViewModel(IRecipeService recipeService, ILoggingService loggingService, IUnsplashImageService unsplashImageService)
            : this(recipeService, loggingService)
        {
            _unsplashImageService = unsplashImageService;
        }

        public async Task LoadRecipesAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var result = await _recipeService.GetAllRecipesAsync();
                _allRecipes = result?.ToList() ?? new List<Recipe>();
                Recipes = new ObservableCollection<Recipe>(_allRecipes);
                ComputeKpis(_allRecipes);
                ResetPaging();
                ApplyAllTransforms();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchViaServiceAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var result = await _recipeService.SearchRecipesAsync(SearchText, null, null, null);
                _allRecipes = result?.ToList() ?? new List<Recipe>();
                Recipes = new ObservableCollection<Recipe>(_allRecipes);
                ComputeKpis(_allRecipes);
                ResetPaging();
                ApplyAllTransforms();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OpenAddRecipeDialogAsync()
        {
            try
            {
                var dialog = new RecipeDialog();
                dialog.Owner = Application.Current?.MainWindow;
                var result = dialog.ShowDialog();
                if (result == true && dialog.Recipe != null)
                {
                    await _recipeService.CreateRecipeAsync(dialog.Recipe);
                    await LoadRecipesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task OpenViewEditRecipeDialogAsync(Recipe? recipe)
        {
            if (recipe == null) return;
            try
            {
                // Work on a shallow copy to avoid mutating the list item if cancelled
                var editable = new Recipe
                {
                    Id = recipe.Id,
                    Title = recipe.Title,
                    Description = recipe.Description,
                    Instructions = recipe.Instructions,
                    CookTime = recipe.CookTime,
                    Difficulty = recipe.Difficulty,
                    ImageUrl = recipe.ImageUrl,
                    Servings = recipe.Servings,
                    Category = recipe.Category,
                    UserId = recipe.UserId,
                    CreatedAt = recipe.CreatedAt,
                    UpdatedAt = recipe.UpdatedAt
                };

                var dialog = new RecipeDialog(editable) { Owner = Application.Current?.MainWindow };
                var result = dialog.ShowDialog();
                if (result == true && dialog.Recipe != null)
                {
                    await _recipeService.UpdateRecipeAsync(dialog.Recipe);
                    await LoadRecipesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private static void NavigateToAnalytics()
        {
            if (Application.Current?.MainWindow?.DataContext is MainViewModel shell)
            {
                shell.SelectedTab = "Analytics";
            }
        }

        private async Task FilterRecipesByCategoryAsync(string category)
        {
            SelectedCategory = string.IsNullOrWhiteSpace(category) ? "All" : category;
            await Task.CompletedTask;
        }

        private void ApplyAllTransforms()
        {
            IEnumerable<Recipe> query = _allRecipes;

            // filter by category
            if (!string.Equals(SelectedCategory, "All", StringComparison.OrdinalIgnoreCase))
            {
                switch (SelectedCategory)
                {
                    case "Main Dishes":
                        query = query.Where(r => string.Equals(r.Category, "Main Dishes", StringComparison.OrdinalIgnoreCase) || string.Equals(r.Category, "Main", StringComparison.OrdinalIgnoreCase));
                        break;
                    case "Desserts":
                        query = query.Where(r => string.Equals(r.Category, "Desserts", StringComparison.OrdinalIgnoreCase) || string.Equals(r.Category, "Dessert", StringComparison.OrdinalIgnoreCase));
                        break;
                    case "Quick":
                        query = query.Where(r => r.CookTime > 0 && r.CookTime <= 20);
                        break;
                    case "Healthy":
                        query = query.Where(r => string.Equals(r.Category, "Healthy", StringComparison.OrdinalIgnoreCase));
                        break;
                    default:
                        query = query.Where(r => string.Equals(r.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }

            // search text (local)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var st = SearchText.Trim();
                query = query.Where(r =>
                    (!string.IsNullOrEmpty(r.Title) && r.Title.Contains(st, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.Description) && r.Description.Contains(st, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(r.Category) && r.Category.Contains(st, StringComparison.OrdinalIgnoreCase))
                );
            }

            // sort
            query = SortQuery(query, SortBy);

            Recipes = new ObservableCollection<Recipe>(query);
            ResetPaging();
            UpdatePaging();
        }

        private static IEnumerable<Recipe> SortQuery(IEnumerable<Recipe> query, string sortBy)
        {
            return sortBy switch
            {
                "📝 Name A-Z" or "Name A-Z" => query.OrderBy(r => r.Title),
                "📝 Name Z-A" or "Name Z-A" => query.OrderByDescending(r => r.Title),
                "⏱️ Time (Low to High)" or "Time (Low to High)" => query.OrderBy(r => r.CookTime),
                "⏱️ Time (High to Low)" or "Time (High to Low)" => query.OrderByDescending(r => r.CookTime),
                "🔥 Difficulty (Easy to Hard)" or "Difficulty (Easy to Hard)" => query.OrderBy(r => r.Difficulty),
                "🔥 Difficulty (Hard to Easy)" or "Difficulty (Hard to Easy)" => query.OrderByDescending(r => r.Difficulty),
                "📅 Date Created" or "Date Created" => query.OrderByDescending(r => r.CreatedAt),
                _ => query.OrderBy(r => r.Title),
            };
        }

        private void ResetPaging()
        {
            TotalPages = Math.Max(1, (int)Math.Ceiling((Recipes.Count) / (double)_pageSize));
            PageOptions = new ObservableCollection<int>(Enumerable.Range(1, TotalPages));
            CurrentPage = 1;
        }

        private void UpdatePaging()
        {
            var skip = (CurrentPage - 1) * _pageSize;
            var pageItems = Recipes.Skip(skip).Take(_pageSize).ToList();
            PagedRecipes = new ObservableCollection<Recipe>(pageItems);
            _ = EnsureImagesForPageAsync(pageItems);
        }

        private async Task EnsureImagesForPageAsync(IEnumerable<Recipe> pageItems)
        {
            if (_unsplashImageService == null) return;
            foreach (var r in pageItems)
            {
                if (string.IsNullOrWhiteSpace(r.ImageUrl) && !string.IsNullOrWhiteSpace(r.Title))
                {
                    try
                    {
                        var url = await _unsplashImageService.SearchFoodImageAsync(r.Title, 800, 600);
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            r.ImageUrl = url;
                            await _recipeService.UpdateRecipeAsync(r);
                        }
                    }
                    catch { /* ignore per-item failures */ }
                }
            }
            // Refresh current page binding
            OnPropertyChanged(nameof(PagedRecipes));
        }

        private void ComputeKpis(IEnumerable<Recipe> recipes)
        {
            var list = recipes.ToList();
            var cookTimes = list.Select(r => r.CookTime).Where(t => t > 0).ToList();
            MyAverageCookTime = cookTimes.Any() ? cookTimes.Average() : 0;

            var diff = list
                .Where(r => !string.IsNullOrWhiteSpace(r.Difficulty))
                .GroupBy(r => r.Difficulty!)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? string.Empty;
            MyMostCommonDifficulty = diff;

            // No explicit AI marker available in entity -> keep 0 to avoid guessing
            MyAIGeneratedRecipes = 0;
        }
    }
}