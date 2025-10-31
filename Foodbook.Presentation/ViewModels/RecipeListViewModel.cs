// Foodbook.Presentation/ViewModels/RecipeListViewModel.cs
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.ViewModels
{
    public class RecipeListViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private List<Recipe> _allRecipes = new(); // Master list
        private ObservableCollection<Recipe> _recipes = new();
        private ObservableCollection<Recipe> _pagedRecipes = new();
        
        // Paging properties
        private int _pageSize = 3;
        private int _currentPage = 1;
        private int _totalPages;
        private ObservableCollection<int> _pageOptions = new();
        private int _selectedPage = 1;
        
        // Filter/Sort properties
        private string _recipeSearchText = string.Empty;
        private string _sortBy = "Name A-Z";
        private string _selectedCategory = "All";
        
        // Expose data
        public ObservableCollection<Recipe> Recipes { get => _recipes; set => SetProperty(ref _recipes, value); }
        public ObservableCollection<Recipe> PagedRecipes { get => _pagedRecipes; set => SetProperty(ref _pagedRecipes, value); }
        public int CurrentPage { get => _currentPage; set { /* ... Paging logic ... */ } }
        public int TotalPages { get => _totalPages; private set => SetProperty(ref _totalPages, value); }
        public ObservableCollection<int> PageOptions { get => _pageOptions; private set => SetProperty(ref _pageOptions, value); }
        public int SelectedPage { get => _selectedPage; set { /* ... Paging logic ... */ } }
        public string RecipeSearchText { get => _recipeSearchText; set { /* ... Search logic ... */ } }
        
        // Commands
        public ICommand LoadRecipesCommand { get; }
        public ICommand SearchRecipesCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand AddRecipeCommand { get; }
        public ICommand ViewRecipeCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand FilterByCategoryCommand { get; }
        public ICommand SortByCommand { get; }
        
        // Constructor and core methods (LoadRecipesAsync, UpdatePaging, Filter, Sort)
        public RecipeListViewModel(IRecipeService recipeService, ILoggingService loggingService)
        {
            _recipeService = recipeService;
            LoadRecipesCommand = new RelayCommand(async () => await LoadRecipesAsync());
            FilterByCategoryCommand = new RelayCommand<string>(async (c) => await FilterRecipesByCategoryAsync(c));
            // ... (Initialize other commands with their logic moved from MainViewModel)
        }
        
        // Placeholder for Designtime
        public RecipeListViewModel() { } 

        // public async Task LoadRecipesAsync() { /* ... Logic di chuyển từ MainViewModel ... */ }
        // private async Task FilterRecipesByCategoryAsync(string category) { /* ... Logic di chuyển từ MainViewModel ... */ }
        // private void UpdatePaging() { /* ... Logic di chuyển từ MainViewModel ... */ }
    }
}