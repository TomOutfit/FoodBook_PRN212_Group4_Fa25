using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Foodbook.Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private readonly IIngredientService _ingredientService;
        private readonly IAIService _aiService;
        private readonly IUserService _userService;
        private readonly IShoppingListService _shoppingListService;
        private readonly INutritionService _nutritionService;
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService? _localizationService;
        private readonly IAuthenticationService? _authenticationService;
        public ILoggingService LoggingService => _loggingService;

        // Nutrition Analysis ViewModel
        private NutritionViewModel? _nutritionViewModel;
        public NutritionViewModel? NutritionViewModel
        {
            get => _nutritionViewModel;
            set
            {
                _nutritionViewModel = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Recipe> _recipes = new();
        private ObservableCollection<Ingredient> _ingredients = new();
        private List<Recipe> _allRecipes = new(); // Store all recipes for filtering
        private Recipe? _selectedRecipe;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private string _selectedTab = "Dashboard";
        
        // Settings properties
        private string _selectedTheme = "Light";
        private string _selectedLanguage = "English";
        private bool _notificationsEnabled = true;
        private int _defaultServings = 4;
        
        // Recipe Collection properties
        private string _sortBy = "Name A-Z";
        private string _selectedCategory = "All";
        private bool _autoSaveEnabled = true;
        
        // Ingredient Collection properties
        private string _ingredientSortBy = "Name A-Z";
        private string _selectedIngredientCategory = "All";
        private List<Ingredient> _allIngredients = new(); // Store all ingredients for filtering
        
        // Sidebar properties
        private bool _isSidebarCollapsed = false;
        
        // Log viewer state
        private bool _isOpeningLogs = false;

        // Analytics properties
        // Removed - using _myTotalRecipes instead
        private string _mostUsedIngredient = "None";
        // Removed - using _myAverageCookTime instead
        private int _aiJudgments = 0;
        private int _generatedRecipes = 0;
        private double _successRate = 0;
        private ISeries[] _recipeTrendSeries = [];
        private ISeries[] _ingredientUsageSeries = [];
        private ISeries[] _aiPerformanceSeries = [];
        private ISeries[] _cookTimeDistributionSeries = [];
        private ISeries[] _recipeDistributionSeries = [];

        // Dashboard Chart private fields
        private int _totalRecipeCount = 0;
        private int _mainDishesCount = 0;
        private int _dessertsCount = 0;
        private int _quickMealsCount = 0;
        private int _cookingTime0To30Count = 0;
        private int _cookingTime30To60Count = 0;
        private int _cookingTime60To90Count = 0;
        private int _cookingTime90To120Count = 0;
        private int _cookingTime120PlusCount = 0;
        private int _easyDifficultyCount = 0;
        private int _mediumDifficultyCount = 0;
        private int _hardDifficultyCount = 0;
        private ObservableCollection<int> _monthlyRecipeCounts = new();
        private User? _currentUser;
        private string _greeting = string.Empty;

        // Dashboard KPI properties (FoodBook context)
        private int _totalIngredients;
        private int _expiringSoonCount;
        private int _expiredCount;
        private int _aiOperationsCount;
        private int _aiErrorCount;
        private int _totalLogsCount;
        private ObservableCollection<KeyValuePair<string,int>> _topIngredients = new();

        // Serialize DB access to avoid DbContext concurrent usage across async flows
        private readonly SemaphoreSlim _dbAccessLock = new(1, 1);

        // My Recipes statistics properties
        private int _myTotalRecipes = 0;
        private double _myAverageCookTime = 0;
        private string _myMostCommonDifficulty = "Easy";
        private int _myAIGeneratedRecipes = 0;
        
        // Chef Judge Evaluation Mode
        private string _selectedEvaluationMode = "😊 Casual (Encouraging)";
        public string SelectedEvaluationMode
        {
            get => _selectedEvaluationMode;
            set
            {
                _selectedEvaluationMode = value;
                OnPropertyChanged();
            }
        }
        
        public List<string> EvaluationModes { get; } = new List<string> { "😊 Casual (Encouraging)", "👨‍⚖️ Strict (Professional)" };
        
        // Custom nutrition analysis properties
        private string _customRecipeText = string.Empty;
        public string CustomRecipeText 
        { 
            get => _customRecipeText; 
            set 
            { 
                _customRecipeText = value; 
                OnPropertyChanged(); 
            } 
        }

        // Recipe and Ingredient search properties
        private string _recipeSearchText = string.Empty;
        public string RecipeSearchText 
        { 
            get => _recipeSearchText; 
            set 
            { 
                if (SetProperty(ref _recipeSearchText, value))
                {
                    // Trigger search when text changes
                    _ = Task.Run(async () => await SearchRecipesAsync());
                }
            } 
        }

        private string _ingredientSearchText = string.Empty;
        public string IngredientSearchText 
        { 
            get => _ingredientSearchText; 
            set 
            { 
                if (SetProperty(ref _ingredientSearchText, value))
                {
                    // Trigger search when text changes
                    _ = Task.Run(async () => await SearchIngredientsAsync());
                }
            } 
        }

        // Smart Inventory metrics
        private int _nearExpiryCount;
        public int NearExpiryCount
        {
            get => _nearExpiryCount;
            private set => SetProperty(ref _nearExpiryCount, value);
        }

        private int _shoppingAlertsCount;
        public int ShoppingAlertsCount
        {
            get => _shoppingAlertsCount;
            private set => SetProperty(ref _shoppingAlertsCount, value);
        }

        // Commands for Smart Inventory actions
        public ICommand AnalyzePantryCommand { get; private set; } = null!;
        public ICommand MarkUsedCommand { get; private set; } = null!;
        public ICommand MarkFinishedCommand { get; private set; } = null!;

        public MainViewModel(
            IRecipeService recipeService,
            IIngredientService ingredientService,
            IAIService aiService,
            IUserService userService,
            IShoppingListService shoppingListService,
            INutritionService nutritionService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            ILocalizationService localizationService,
            IAuthenticationService authenticationService)
        {
            _recipeService = recipeService;
            _ingredientService = ingredientService;
            _aiService = aiService;
            _userService = userService;
            _shoppingListService = shoppingListService;
            _nutritionService = nutritionService;
            _loggingService = loggingService;
            _settingsService = settingsService;
            _localizationService = localizationService;
            _authenticationService = authenticationService;
            
            // Initialize NutritionViewModel
            NutritionViewModel = new NutritionViewModel(nutritionService, aiService, recipeService);
            
            // Initialize all commands
            LoadRecipesCommand = new RelayCommand(async () => await LoadRecipesAsync());
            SearchRecipesCommand = new RelayCommand(async () => await SearchRecipesAsync());
            LoadIngredientsCommand = new RelayCommand(async () => await LoadIngredientsAsync());
            DeleteRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await DeleteRecipeAsync(recipe));
            AdjustServingsCommand = new RelayCommand(async () => await AdjustServingsAsync());
            GenerateRecipeCommand = new RelayCommand(async () => await GenerateRecipeAsync());
            JudgeDishCommand = new RelayCommand(async () => await JudgeDishAsync());
            GenerateShoppingListCommand = new RelayCommand(async () => await GenerateShoppingListAsync());
            AnalyzeCustomNutritionCommand = new RelayCommand(async () => await AnalyzeCustomNutritionAsync());
            OpenNutritionAnalysisCommand = new RelayCommand(async () => await OpenNutritionAnalysis());
            SelectTabCommand = new RelayCommand<string>(async (tabName) => await SelectTab(tabName));
            AddNewRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            AddNewIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            EditRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await EditRecipeAsync(recipe));
            DeleteRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await DeleteRecipeAsync(recipe));
            ViewRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await ViewRecipeAsync(recipe));
            EditIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await EditIngredientAsync(ingredient));
            DeleteIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await DeleteIngredientAsync(ingredient));
            ViewIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await ViewIngredientAsync(ingredient));
            CreateRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            CreateIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
            SearchIngredientsCommand = new RelayCommand(async () => await SearchIngredientsAsync());
            
            // Initialize additional commands
            AddRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            ViewRecipeAnalyticsCommand = new RelayCommand(async () => await SelectTab("Analytics"));
            SortByCommand = new RelayCommand<string>(async (sortBy) => await SortRecipesAsync(sortBy));
            FilterByCategoryCommand = new RelayCommand<string>(async (category) => await FilterRecipesByCategoryAsync(category));
            RefreshProfileCommand = new RelayCommand(async () => await LoadCurrentUserAsync());
            TestDatabaseConnectionCommand = new RelayCommand(async () => await TestDatabaseConnectionAsync());
            ConfigureAICommand = new RelayCommand(async () => await ConfigureAISettingsAsync());
            ViewLogsCommand = new RelayCommand(() => { ViewLogsAsync(); return Task.CompletedTask; });
            RefreshUserProfileCommand = new RelayCommand(async () => await LoadCurrentUserAsync());
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            RefreshDashboardCommand = new RelayCommand(async () => await RefreshDashboardDataAsync());
            
            // Initialize ingredient commands
            AddIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            ViewIngredientAnalyticsCommand = new RelayCommand(async () => await SelectTab("Analytics"));
            AnalyzePantryCommand = new RelayCommand(async () => await AnalyzePantryAsync());
            MarkUsedCommand = new RelayCommand<Ingredient>(async (ingredient) => await MarkUsedAsync(ingredient, 0));
            MarkFinishedCommand = new RelayCommand<Ingredient>(async (ingredient) => await MarkFinishedAsync(ingredient));
            
            // Initialize ingredient tab navigation commands
            NavigateToAllIngredientsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("All"));
            NavigateToProteinsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Proteins"));
            NavigateToGrainsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Grains"));
            NavigateToVegetablesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Vegetables"));
            NavigateToSpicesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Spices"));
            
            // Load settings asynchronously on initialization
            _ = LoadSettingsAsync();
        }

        // Parameterless constructor for fallback
        public MainViewModel()
        {
            // Initialize with null services - will be handled gracefully
            _recipeService = null!;
            _ingredientService = null!;
            _aiService = null!;
            _userService = null!;
            _shoppingListService = null!;
            _nutritionService = null!;
            _loggingService = null!;
            _settingsService = null!;
            
            // Initialize charts first
            InitializeCharts();
            
            // Initialize all commands
            LoadRecipesCommand = new RelayCommand(async () => await LoadRecipesAsync());
            SearchRecipesCommand = new RelayCommand(async () => await SearchRecipesAsync());
            LoadIngredientsCommand = new RelayCommand(async () => await LoadIngredientsAsync());
            DeleteRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await DeleteRecipeAsync(recipe));
            AdjustServingsCommand = new RelayCommand(async () => await AdjustServingsAsync());
            GenerateRecipeCommand = new RelayCommand(async () => await GenerateRecipeAsync());
            JudgeDishCommand = new RelayCommand(async () => await JudgeDishAsync());
            GenerateShoppingListCommand = new RelayCommand(async () => await GenerateShoppingListAsync());
            AnalyzeCustomNutritionCommand = new RelayCommand(async () => await AnalyzeCustomNutritionAsync());
            OpenNutritionAnalysisCommand = new RelayCommand(async () => await OpenNutritionAnalysis());
            SelectTabCommand = new RelayCommand<string>(async (tabName) => await SelectTab(tabName));
            AddNewRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            AddNewIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            EditRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await EditRecipeAsync(recipe));
            DeleteRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await DeleteRecipeAsync(recipe));
            ViewRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await ViewRecipeAsync(recipe));
            EditIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await EditIngredientAsync(ingredient));
            DeleteIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await DeleteIngredientAsync(ingredient));
            ViewIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await ViewIngredientAsync(ingredient));
            CreateRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            CreateIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
            SearchIngredientsCommand = new RelayCommand(async () => await SearchIngredientsAsync());
            
            // Initialize additional commands
            AddRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            ViewRecipeAnalyticsCommand = new RelayCommand(async () => await SelectTab("Analytics"));
            SortByCommand = new RelayCommand<string>(async (sortBy) => await SortRecipesAsync(sortBy));
            FilterByCategoryCommand = new RelayCommand<string>(async (category) => await FilterRecipesByCategoryAsync(category));
            RefreshProfileCommand = new RelayCommand(async () => await LoadCurrentUserAsync());
            TestDatabaseConnectionCommand = new RelayCommand(async () => await TestDatabaseConnectionAsync());
            ConfigureAICommand = new RelayCommand(async () => await ConfigureAISettingsAsync());
            ViewLogsCommand = new RelayCommand(() => { ViewLogsAsync(); return Task.CompletedTask; });
            RefreshUserProfileCommand = new RelayCommand(async () => await LoadCurrentUserAsync());
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            
            // Initialize ingredient filter commands
            SortIngredientsByCommand = new RelayCommand<string>(async (sortBy) => await SortIngredientsAsync(sortBy));
            FilterIngredientsByCategoryCommand = new RelayCommand<string>(async (category) => await FilterIngredientsByCategoryAsync(category));
            
            // Initialize ingredient commands
            AddIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            ViewIngredientAnalyticsCommand = new RelayCommand(async () => await SelectTab("Analytics"));
            AnalyzePantryCommand = new RelayCommand(async () => await AnalyzePantryAsync());
            MarkUsedCommand = new RelayCommand<Ingredient>(async (ingredient) => await MarkUsedAsync(ingredient, 0));
            MarkFinishedCommand = new RelayCommand<Ingredient>(async (ingredient) => await MarkFinishedAsync(ingredient));
            
            // Initialize ingredient tab navigation commands
            NavigateToAllIngredientsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("All"));
            NavigateToProteinsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Proteins"));
            NavigateToGrainsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Grains"));
            NavigateToVegetablesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Vegetables"));
            NavigateToSpicesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Spices"));
            
            // Paging commands (always executable; page is clamped in setter)
            NextPageCommand = new RelayCommand(
                () => { CurrentPage = Math.Min(CurrentPage + 1, TotalPages == 0 ? 1 : TotalPages); return Task.CompletedTask; }
            );
            PrevPageCommand = new RelayCommand(
                () => { CurrentPage = Math.Max(CurrentPage - 1, 1); return Task.CompletedTask; }
            );

            // Load current user
            try
            {
                _ = LoadCurrentUserAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading current user in constructor: {ex.Message}");
            }
        }

        public ObservableCollection<Recipe> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        // Paging for Recipes (3 cards per page)
        private ObservableCollection<Recipe> _pagedRecipes = new();
        public ObservableCollection<Recipe> PagedRecipes
        {
            get => _pagedRecipes;
            set => SetProperty(ref _pagedRecipes, value);
        }

        private int _pageSize = 3;
        public int PageSize
        {
            get => _pageSize;
            set { if (SetProperty(ref _pageSize, value)) UpdatePaging(); }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set 
            { 
                var v = Math.Max(1, Math.Min(value, TotalPages == 0 ? 1 : TotalPages)); 
                if (SetProperty(ref _currentPage, v)) 
                { 
                    UpdatePaging();
                    // notify command system to re-evaluate can-execute for paging buttons
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            private set => SetProperty(ref _totalPages, value);
        }

        // Paging options for quick jump
        private ObservableCollection<int> _pageOptions = new();
        public ObservableCollection<int> PageOptions
        {
            get => _pageOptions;
            private set => SetProperty(ref _pageOptions, value);
        }

        private int _selectedPage = 1;
        public int SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (SetProperty(ref _selectedPage, value))
                {
                    // Keep CurrentPage in sync when user jumps via selector
                    CurrentPage = value;
                }
            }
        }

        public ObservableCollection<Ingredient> Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        public Recipe? SelectedRecipe
        {
            get => _selectedRecipe;
            set => SetProperty(ref _selectedRecipe, value);
        }

        public string SearchText
        {
            get => _searchText;
            set 
            { 
                if (SetProperty(ref _searchText, value))
                {
                    // Trigger search when text changes
                    _ = Task.Run(async () => await SearchRecipesAsync());
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        // Smart Pantry overview counters
        public int PantryExpiringSoon3Days { get; set; }
        public int PantryExpiringSoon7Days { get; set; }
        public int PantryExpired { get; set; }
        public int PantryNeedRestock { get; set; }
        public int PantryOverstock { get; set; }

        public ICommand LoadRecipesCommand { get; }
        public ICommand SearchRecipesCommand { get; }
        public ICommand LoadIngredientsCommand { get; }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand AdjustServingsCommand { get; }
        public ICommand GenerateRecipeCommand { get; }
        public ICommand JudgeDishCommand { get; }
        public ICommand GenerateShoppingListCommand { get; }
        public ICommand AnalyzeCustomNutritionCommand { get; }
        public ICommand OpenNutritionAnalysisCommand { get; }
        public ICommand SelectTabCommand { get; }
        public ICommand AddNewRecipeCommand { get; }
        public ICommand AddNewIngredientCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand ViewRecipeCommand { get; }
        public ICommand EditIngredientCommand { get; }
        public ICommand ViewIngredientCommand { get; } = null!;
        public ICommand CreateRecipeCommand { get; } = null!;
        public ICommand CreateIngredientCommand { get; } = null!;
        public ICommand DeleteIngredientCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }
        public ICommand SearchIngredientsCommand { get; }
        
        // Additional commands for Recipe Collection page
        public ICommand AddRecipeCommand { get; }
        public ICommand ViewRecipeAnalyticsCommand { get; }
        public ICommand SortByCommand { get; }
        public ICommand FilterByCategoryCommand { get; }
        public ICommand RefreshProfileCommand { get; }
        public ICommand TestDatabaseConnectionCommand { get; }
        public ICommand ConfigureAICommand { get; }
        public ICommand ViewLogsCommand { get; }
        public ICommand RefreshUserProfileCommand { get; }
        public ICommand ToggleSidebarCommand { get; }
        public ICommand RefreshDashboardCommand { get; } = null!;
        public ICommand NextPageCommand { get; } = null!;
        public ICommand PrevPageCommand { get; } = null!;
        
        // Ingredient Filter Commands
        public ICommand SortIngredientsByCommand { get; } = null!;
        public ICommand FilterIngredientsByCategoryCommand { get; } = null!;
        
        // Ingredient Commands
        public ICommand AddIngredientCommand { get; } = null!;
        public ICommand ViewIngredientAnalyticsCommand { get; } = null!;
        
        // Ingredient Tab Navigation Commands
        public ICommand NavigateToAllIngredientsCommand { get; } = null!;
        public ICommand NavigateToProteinsCommand { get; } = null!;
        public ICommand NavigateToGrainsCommand { get; } = null!;
        public ICommand NavigateToVegetablesCommand { get; } = null!;
        public ICommand NavigateToSpicesCommand { get; } = null!;
        
        // Settings Properties
        public string SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }
        
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }
        
        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set => SetProperty(ref _notificationsEnabled, value);
        }
        
        public int DefaultServings
        {
            get => _defaultServings;
            set => SetProperty(ref _defaultServings, value);
        }
        
        // Localization Properties
        public string Loc_Dashboard => _localizationService?.GetString("Dashboard") ?? "Dashboard";
        public string Loc_MyRecipes => _localizationService?.GetString("My Recipes") ?? "My Recipes";
        public string Loc_Ingredients => _localizationService?.GetString("Ingredients") ?? "Ingredients";
        public string Loc_ShoppingList => _localizationService?.GetString("Shopping List") ?? "Shopping List";
        public string Loc_Analytics => _localizationService?.GetString("Analytics") ?? "Analytics";
        public string Loc_Settings => _localizationService?.GetString("Settings") ?? "Settings";
        public string Loc_ApplicationSettings => _localizationService?.GetString("Application Settings") ?? "Application Settings";
        public string Loc_Theme => _localizationService?.GetString("Theme") ?? "Theme";
        public string Loc_Language => _localizationService?.GetString("Language") ?? "Language";
        public string Loc_DefaultServings => _localizationService?.GetString("Default Servings") ?? "Default Servings";
        public string Loc_EnableNotifications => _localizationService?.GetString("Enable Notifications") ?? "Enable Notifications";
        public string Loc_SaveSettings => _localizationService?.GetString("Save Settings") ?? "Save Settings";
        public string Loc_UserProfile => _localizationService?.GetString("User Profile") ?? "User Profile";
        public string Loc_Username => _localizationService?.GetString("Username") ?? "Username";
        public string Loc_Email => _localizationService?.GetString("Email") ?? "Email";
        public string Loc_TotalRecipes => _localizationService?.GetString("Total Recipes") ?? "Total Recipes";
        public string Loc_MainDishes => _localizationService?.GetString("Main Dishes") ?? "Main Dishes";
        public string Loc_Desserts => _localizationService?.GetString("Desserts") ?? "Desserts";
        public string Loc_QuickMeals => _localizationService?.GetString("Quick Meals") ?? "Quick Meals";
        public string Loc_AverageCookTime => _localizationService?.GetString("Average Cook Time") ?? "Average Cook Time";
        public string Loc_MostUsedIngredient => _localizationService?.GetString("Most Used Ingredient") ?? "Most Used Ingredient";
        public string Loc_AIRecipe => _localizationService?.GetString("AI Recipes") ?? "AI Recipes";
        public string Loc_FavoriteRecipes => _localizationService?.GetString("Favorite Recipes") ?? "Favorite Recipes";

        // Analytics Properties
        // Removed - using MyTotalRecipes instead

        public string MostUsedIngredient
        {
            get => _mostUsedIngredient;
            set => SetProperty(ref _mostUsedIngredient, value);
        }

        // Removed - using MyAverageCookTime instead

        public int AIJudgments
        {
            get => _aiJudgments;
            set => SetProperty(ref _aiJudgments, value);
        }

        public int GeneratedRecipes
        {
            get => _generatedRecipes;
            set => SetProperty(ref _generatedRecipes, value);
        }

        public double SuccessRate
        {
            get => _successRate;
            set => SetProperty(ref _successRate, value);
        }

        public ISeries[] RecipeTrendSeries
        {
            get => _recipeTrendSeries;
            set => SetProperty(ref _recipeTrendSeries, value);
        }

        public ISeries[] IngredientUsageSeries
        {
            get => _ingredientUsageSeries;
            set => SetProperty(ref _ingredientUsageSeries, value);
        }

        public ISeries[] AIPerformanceSeries
        {
            get => _aiPerformanceSeries;
            set => SetProperty(ref _aiPerformanceSeries, value);
        }

        public ISeries[] CookTimeDistributionSeries
        {
            get => _cookTimeDistributionSeries;
            set => SetProperty(ref _cookTimeDistributionSeries, value);
        }

        public ISeries[] RecipeDistributionSeries
        {
            get => _recipeDistributionSeries;
            set => SetProperty(ref _recipeDistributionSeries, value);
        }

        // Dashboard Chart Properties for hardcoded charts
        public int TotalRecipeCount
        {
            get => _totalRecipeCount;
            set => SetProperty(ref _totalRecipeCount, value);
        }

        public int MainDishesCount
        {
            get => _mainDishesCount;
            set => SetProperty(ref _mainDishesCount, value);
        }

        public int DessertsCount
        {
            get => _dessertsCount;
            set => SetProperty(ref _dessertsCount, value);
        }

        public int QuickMealsCount
        {
            get => _quickMealsCount;
            set => SetProperty(ref _quickMealsCount, value);
        }

        public int CookingTime0To30Count
        {
            get => _cookingTime0To30Count;
            set => SetProperty(ref _cookingTime0To30Count, value);
        }

        public int CookingTime30To60Count
        {
            get => _cookingTime30To60Count;
            set => SetProperty(ref _cookingTime30To60Count, value);
        }

        public int CookingTime60To90Count
        {
            get => _cookingTime60To90Count;
            set => SetProperty(ref _cookingTime60To90Count, value);
        }

        public int CookingTime90To120Count
        {
            get => _cookingTime90To120Count;
            set => SetProperty(ref _cookingTime90To120Count, value);
        }

        public int CookingTime120PlusCount
        {
            get => _cookingTime120PlusCount;
            set => SetProperty(ref _cookingTime120PlusCount, value);
        }

        public int EasyDifficultyCount
        {
            get => _easyDifficultyCount;
            set => SetProperty(ref _easyDifficultyCount, value);
        }

        public int MediumDifficultyCount
        {
            get => _mediumDifficultyCount;
            set => SetProperty(ref _mediumDifficultyCount, value);
        }

        public int HardDifficultyCount
        {
            get => _hardDifficultyCount;
            set => SetProperty(ref _hardDifficultyCount, value);
        }

        public ObservableCollection<int> MonthlyRecipeCounts
        {
            get => _monthlyRecipeCounts;
            set => SetProperty(ref _monthlyRecipeCounts, value);
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string Greeting
        {
            get => _greeting;
            set => SetProperty(ref _greeting, value);
        }

        // Exposed Dashboard KPI bindings
        public int TotalIngredients
        {
            get => _totalIngredients;
            set => SetProperty(ref _totalIngredients, value);
        }

        public int ExpiringSoonCount
        {
            get => _expiringSoonCount;
            set => SetProperty(ref _expiringSoonCount, value);
        }

        public int ExpiredCount
        {
            get => _expiredCount;
            set => SetProperty(ref _expiredCount, value);
        }

        public int AIOperationsCount
        {
            get => _aiOperationsCount;
            set => SetProperty(ref _aiOperationsCount, value);
        }

        public int AIErrorCount
        {
            get => _aiErrorCount;
            set => SetProperty(ref _aiErrorCount, value);
        }

        public int TotalLogsCount
        {
            get => _totalLogsCount;
            set => SetProperty(ref _totalLogsCount, value);
        }

        public ObservableCollection<KeyValuePair<string,int>> TopIngredients
        {
            get => _topIngredients;
            set => SetProperty(ref _topIngredients, value);
        }

        // My Recipes Statistics Properties
        public int MyTotalRecipes
        {
            get => _myTotalRecipes;
            set => SetProperty(ref _myTotalRecipes, value);
        }

        public double MyAverageCookTime
        {
            get => _myAverageCookTime;
            set => SetProperty(ref _myAverageCookTime, value);
        }

        public string MyMostCommonDifficulty
        {
            get => _myMostCommonDifficulty;
            set => SetProperty(ref _myMostCommonDifficulty, value);
        }

        public int MyAIGeneratedRecipes
        {
            get => _myAIGeneratedRecipes;
            set => SetProperty(ref _myAIGeneratedRecipes, value);
        }

        // New analytics KPIs to differentiate Dashboard vs Analytics
        private double _pantryCoveragePercent;
        public double PantryCoveragePercent
        {
            get => _pantryCoveragePercent;
            set => SetProperty(ref _pantryCoveragePercent, value);
        }

        private double _aiEngagementPercent;
        public double AIEngagementPercent
        {
            get => _aiEngagementPercent;
            set => SetProperty(ref _aiEngagementPercent, value);
        }

        private double _averageRating;
        public double AverageRating
        {
            get => _averageRating;
            set => SetProperty(ref _averageRating, value);
        }

        // Recipe Collection Properties
        public string SortBy
        {
            get => _sortBy;
            set => SetProperty(ref _sortBy, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set => SetProperty(ref _autoSaveEnabled, value);
        }
        
        // Ingredient Collection Properties
        public string IngredientSortBy
        {
            get => _ingredientSortBy;
            set => SetProperty(ref _ingredientSortBy, value);
        }
        
        public string SelectedIngredientCategory
        {
            get => _selectedIngredientCategory;
            set => SetProperty(ref _selectedIngredientCategory, value);
        }

        // Sidebar Properties
        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set => SetProperty(ref _isSidebarCollapsed, value);
        }


        private async Task LoadRecipesAsync()
        {
            try
            {
                IsLoading = true;
                await _dbAccessLock.WaitAsync();
                StatusMessage = "Loading recipes from database...";
                
                // Check if service is available
                if (_recipeService == null)
                {
                    StatusMessage = "Recipe service not available. Please restart the application.";
                    return;
                }
                
                // Load from database - FoodBook.sql
                var recipes = await _recipeService.GetAllRecipesAsync();
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    _allRecipes.Clear(); // Clear previous data
                    
                    if (recipes != null && recipes.Any())
                    {
                        foreach (var recipe in recipes)
                        {
                            Recipes.Add(recipe);
                            _allRecipes.Add(recipe); // Store in all recipes list
                        }
                        
                        StatusMessage = $"Loaded {Recipes.Count} recipes from FoodBook database";
                        
                        // Calculate statistics from real database data
                        MyTotalRecipes = Recipes.Count;
                        MyAverageCookTime = Recipes.Any() ? Recipes.Average(r => r.CookTime) : 0;
                        var difficultyGroup = Recipes.GroupBy(r => r.Difficulty).OrderByDescending(g => g.Count()).FirstOrDefault();
                        MyMostCommonDifficulty = difficultyGroup?.Key ?? "Easy";
                        MyAIGeneratedRecipes = Recipes.Count(r => r.Category?.Contains("AI") == true || r.Title?.Contains("AI") == true);
                    }
                    else
                    {
                        StatusMessage = "No recipes found in database. Please check FoodBook.sql data.";
                        MyTotalRecipes = 0;
                        MyAverageCookTime = 0;
                        MyMostCommonDifficulty = "Easy";
                        MyAIGeneratedRecipes = 0;
                    }
                });
                UpdatePaging();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading recipes from database: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadRecipesAsync Error: {ex}");
                
                // Show error but don't fallback to sample data
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    MyTotalRecipes = 0;
                    MyAverageCookTime = 0;
                    MyMostCommonDifficulty = "Easy";
                    MyAIGeneratedRecipes = 0;
                });
            }
            finally
            {
                if (_dbAccessLock.CurrentCount == 0) _dbAccessLock.Release();
                IsLoading = false;
            }
        }

        private async Task LoadIngredientsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading ingredients from database...";
                
                // Check if service is available
                if (_ingredientService == null)
                {
                    StatusMessage = "Ingredient service not available. Please restart the application.";
                    return;
                }
                
                // Load from database - FoodBook.sql
                var ingredients = await _ingredientService.GetUserIngredientsAsync(1);
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Ingredients.Clear();
                    _allIngredients.Clear(); // Clear previous data
                    
                    if (ingredients != null && ingredients.Any())
                    {
                        foreach (var ingredient in ingredients)
                        {
                            Ingredients.Add(ingredient);
                            _allIngredients.Add(ingredient); // Store in all ingredients list
                        }
                        
                        StatusMessage = $"Loaded {Ingredients.Count} ingredients from FoodBook database";

                        // Compute Smart Pantry status counters
                        var now = DateTime.UtcNow;
                        var expiring3Days = Ingredients.Count(i => i.ExpiryDate != null && i.ExpiryDate <= now.AddDays(3) && i.ExpiryDate >= now);
                        var expiring7Days = Ingredients.Count(i => i.ExpiryDate != null && i.ExpiryDate <= now.AddDays(7) && i.ExpiryDate >= now);
                        var expired = Ingredients.Count(i => i.ExpiryDate != null && i.ExpiryDate < now);
                        var needRestock = Ingredients.Count(i => (i.MinQuantity ?? 0) > 0 && (i.Quantity ?? 0) <= (i.MinQuantity ?? 0));
                        var overstock = Ingredients.Count(i => (i.MinQuantity ?? 0) > 0 && (i.Quantity ?? 0) >= (i.MinQuantity ?? 0) * 2);

                        PantryExpiringSoon3Days = expiring3Days;
                        PantryExpiringSoon7Days = expiring7Days;
                        PantryExpired = expired;
                        PantryNeedRestock = needRestock;
                        PantryOverstock = overstock;
                    }
                    else
                    {
                        StatusMessage = "No ingredients found in database. Please check FoodBook.sql data.";
                        PantryExpiringSoon3Days = 0;
                        PantryExpiringSoon7Days = 0;
                        PantryExpired = 0;
                        PantryNeedRestock = 0;
                        PantryOverstock = 0;
                    }
                    // Update Smart Inventory metrics
                    UpdateInventoryMetrics();
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading ingredients from database: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadIngredientsAsync Error: {ex}");
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Ingredients.Clear();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateInventoryMetrics()
        {
            try
            {
                var now = DateTime.UtcNow;
                var ingredientsSnapshot = Ingredients.ToList();
                NearExpiryCount = ingredientsSnapshot
                    .Count(i => i.ExpiryDate.HasValue && (i.ExpiryDate.Value - now).TotalDays <= 7 && (i.ExpiryDate.Value - now).TotalDays >= -0.01);
                ShoppingAlertsCount = ingredientsSnapshot
                    .Count(i => i.MinQuantity.HasValue && (i.Quantity ?? 0) <= i.MinQuantity.Value);
                TotalIngredients = ingredientsSnapshot.Count;
            }
            catch { /* best-effort metrics */ }
        }

        private Task AnalyzePantryAsync()
        {
            try
            {
                // Build a compact pantry string for AI Chef
                var parts = Ingredients
                    .Select(i =>
                    {
                        var qty = i.Quantity.HasValue ? i.Quantity.Value.ToString("0.##") : string.Empty;
                        var unit = string.IsNullOrWhiteSpace(i.Unit) ? string.Empty : i.Unit;
                        var amount = string.IsNullOrWhiteSpace(qty + unit) ? string.Empty : $" {qty}{(string.IsNullOrWhiteSpace(unit) ? string.Empty : unit)}";
                        return $"{i.Name}{amount}";
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s));

                var pantryText = string.Join("; ", parts);

                // Navigate to AI tab and prefill prompt
                SelectedTab = "AI";
                CustomRecipeText = $"Here is my pantry: {pantryText}. Please suggest recipes prioritizing items near expiration.";
            }
            catch { }
            return Task.CompletedTask;
        }

        private async Task MarkUsedAsync(Ingredient? ingredient, decimal usedAmount)
        {
            if (ingredient == null) return;
            try
            {
                var newQty = Math.Max(0m, (ingredient.Quantity ?? 0m) - (usedAmount <= 0 ? 1m : usedAmount));
                ingredient.Quantity = newQty;
                if (_ingredientService != null)
                {
                    await _ingredientService.UpdateIngredientAsync(ingredient);
                }
                await LoadIngredientsAsync();
            }
            catch { }
        }

        private async Task MarkFinishedAsync(Ingredient? ingredient)
        {
            if (ingredient == null) return;
            try
            {
                ingredient.Quantity = 0m;
                if (_ingredientService != null)
                {
                    await _ingredientService.UpdateIngredientAsync(ingredient);
                }
                await LoadIngredientsAsync();
            }
            catch { }
        }


        private async Task SearchRecipesAsync()
        {
            try
            {
                IsLoading = true;
                
                if (string.IsNullOrWhiteSpace(RecipeSearchText))
                {
                    // If search text is empty, show all recipes based on current filter
                    await FilterRecipesByCategoryAsync(SelectedCategory);
                    return;
                }
                
                StatusMessage = "Searching recipes...";
                
                // Search within current filtered results or all recipes
                var searchBase = _allRecipes.AsEnumerable();
                
                // Apply current category filter if not "All"
                if (SelectedCategory != "All")
                {
                    searchBase = searchBase.Where(r => 
                    {
                        if (SelectedCategory == "Main Dishes")
                            return r.Category?.Contains("Main", StringComparison.OrdinalIgnoreCase) == true ||
                                   r.Category?.Contains("Dish", StringComparison.OrdinalIgnoreCase) == true;
                        else if (SelectedCategory == "Desserts")
                            return r.Category?.Contains("Dessert", StringComparison.OrdinalIgnoreCase) == true ||
                                   r.Category?.Contains("Sweet", StringComparison.OrdinalIgnoreCase) == true;
                        else if (SelectedCategory == "Quick")
                            return r.CookTime <= 30;
                        else
                            return r.Category?.Contains(SelectedCategory, StringComparison.OrdinalIgnoreCase) == true ||
                                   r.Title?.Contains(SelectedCategory, StringComparison.OrdinalIgnoreCase) == true;
                    });
                }
                
                // Apply search filter
                var searchResults = searchBase.Where(r => 
                    r.Title?.Contains(RecipeSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    r.Description?.Contains(RecipeSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    r.Category?.Contains(RecipeSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    r.Difficulty?.Contains(RecipeSearchText, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
                
                // Use Dispatcher to update UI on the correct thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in searchResults)
                    {
                        Recipes.Add(recipe);
                    }
                    
                    StatusMessage = $"Found {Recipes.Count} recipes for '{RecipeSearchText}'";
                });
                UpdatePaging();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching recipes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchIngredientsAsync()
        {
            try
            {
                IsLoading = true;
                
                if (string.IsNullOrWhiteSpace(IngredientSearchText))
                {
                    // If search text is empty, load all ingredients
                    await LoadIngredientsAsync();
                    return;
                }
                
                StatusMessage = "Searching ingredients...";
                
                // For demo purposes, using user ID 1
                var ingredients = await _ingredientService.GetUserIngredientsAsync(1);
                
                // Filter ingredients based on search text
                var filteredIngredients = ingredients?.Where(i => 
                    i.Name.Contains(IngredientSearchText, StringComparison.OrdinalIgnoreCase) ||
                    (i.Category?.Contains(IngredientSearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (i.NutritionInfo?.Contains(IngredientSearchText, StringComparison.OrdinalIgnoreCase) == true)
                ).ToList() ?? new List<Ingredient>();
                
                // Use Dispatcher to update UI on the correct thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Ingredients.Clear();
                    foreach (var ingredient in filteredIngredients)
                    {
                        Ingredients.Add(ingredient);
                    }
                    
                    StatusMessage = $"Found {Ingredients.Count} ingredients for '{IngredientSearchText}'";
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching ingredients: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }


        private async Task DeleteRecipeAsync(Recipe recipe)
        {
            if (recipe == null) return;

            try
            {
                var dialog = new Views.ConfirmationDialog(
                    $"Are you sure you want to delete the recipe '{recipe.Title}'?\n\nThis action cannot be undone.", 
                    "Delete Recipe");
                
                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Deleting recipe...";
                    
                    var success = await _recipeService.DeleteRecipeAsync(recipe.Id);
                    if (success)
                    {
                        Recipes.Remove(recipe);
                        if (SelectedRecipe == recipe)
                            SelectedRecipe = null;
                        StatusMessage = "Recipe deleted successfully";
                    }
                    else
                    {
                        StatusMessage = "Failed to delete recipe";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting recipe: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AdjustServingsAsync()
        {
            if (SelectedRecipe == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Adjusting servings...";
                
                // For demo, we'll double the quantities of all ingredients
                var updatedRecipe = await _recipeService.AdjustServingsAsync(SelectedRecipe.Id, 2);
                var index = Recipes.IndexOf(SelectedRecipe);
                if (index >= 0)
                {
                    Recipes[index] = updatedRecipe;
                    SelectedRecipe = updatedRecipe;
                }
                
                StatusMessage = "Servings adjusted successfully (doubled all quantities)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adjusting servings: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateRecipeAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                IsLoading = true;
                StatusMessage = "Generating recipe with AI...";
                
                // Log feature usage
                await _loggingService.LogFeatureUsageAsync("Generate Recipe", "1", 
                    $"Ingredients: {string.Join(", ", Ingredients.Take(3).Select(i => i.Name))}");
                
                // Use default ingredients if none available
                // Prioritize ingredients by earliest expiry to reduce waste
                var ingredientNames = Ingredients.Count > 0 
                    ? Ingredients
                        .OrderBy(i => i.ExpiryDate ?? DateTime.MaxValue)
                        .ThenByDescending(i => i.Quantity ?? 0)
                        .Take(8)
                        .Select(i => i.Name)
                        .ToList()
                    : new List<string> { "chicken", "rice", "vegetables", "garlic", "onion" };
                var generatedRecipe = await _aiService.GenerateRecipeFromIngredientsAsync(
                    ingredientNames, "AI Generated Dish", 4);
                
                // Log AI activity
                stopwatch.Stop();
                await _loggingService.LogAIActivityAsync("Generate Recipe", "1", 
                    string.Join(", ", ingredientNames), 
                    "AI Generated Recipe", 
                    stopwatch.Elapsed);
                
                if (generatedRecipe != null)
                {
                    // Use the AI-generated Recipe object directly
                    var newRecipe = generatedRecipe;
                    
                    var createdRecipe = await _recipeService.CreateRecipeAsync(newRecipe);
                    if (createdRecipe != null)
                    {
                        Recipes.Insert(0, createdRecipe);
                        SelectedRecipe = createdRecipe;
                        StatusMessage = $"Recipe '{createdRecipe.Title}' generated successfully!";
                        
                        // Log performance
                        await _loggingService.LogPerformanceAsync("Generate Recipe", "1", stopwatch.Elapsed, 
                            $"Successfully generated and saved recipe: {createdRecipe.Title}");
                    }
                    else
                    {
                        StatusMessage = "Failed to save generated recipe";
                        await _loggingService.LogErrorAsync("Generate Recipe", "1", 
                            new InvalidOperationException("Failed to save recipe to database"), "Database save operation failed");
                    }
                }
                else
                {
                    StatusMessage = "AI failed to generate recipe. Please try again.";
                    await _loggingService.LogErrorAsync("Generate Recipe", "1", 
                        new InvalidOperationException("AI service returned null"), "AI service failed to generate recipe");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error generating recipe: {ex.Message}";
                await _loggingService.LogErrorAsync("Generate Recipe", "1", ex, "Unexpected error in recipe generation");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task JudgeDishAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var imageDialog = new Views.ImageUploadDialog();
                if (imageDialog.ShowDialog() == true && imageDialog.ImageData != null)
                {
                    IsLoading = true;
                    StatusMessage = "AI is analyzing your dish...";
                    
                    // Log feature usage
                    await _loggingService.LogFeatureUsageAsync("Chef Judge", "1", 
                        $"Image uploaded, size: {imageDialog.ImageData.Length} bytes");
                    
                    // Use byte array directly for AI processing with selected evaluation mode
                    var evaluationMode = SelectedEvaluationMode.Contains("Strict") ? "Strict" : "Casual";
                    var result = await _aiService.JudgeDishAsync(imageDialog.ImageData, evaluationMode);
                    
                    // Log AI activity
                    stopwatch.Stop();
                    await _loggingService.LogAIActivityAsync("Chef Judge", "1", 
                        $"Image data ({imageDialog.ImageData.Length} bytes)", 
                        result != null ? $"Score: {result.Score}/10, Rating: {result.OverallRating}" : "Failed", 
                        stopwatch.Elapsed);
                    
                    if (result != null)
                    {
                        // Show detailed results dialog
                        var resultDialog = new Views.JudgeResultDialog();
                        resultDialog.SetJudgeResult(
                            result.Score,
                            result.OverallRating,
                            result.Comment,
                            result.PresentationScore,
                            result.ColorScore,
                            result.TextureScore,
                            result.PlatingScore,
                            result.HealthNotes,
                            string.Join("\n• ", result.ChefTips),
                            string.Join("\n• ", result.Suggestions)
                        );
                        resultDialog.ShowDialog();
                        
                        StatusMessage = $"🎯 AI Judge Complete! Score: {result.Score}/10 - {result.OverallRating}";
                        
                        // Log performance
                        await _loggingService.LogPerformanceAsync("Chef Judge", "1", stopwatch.Elapsed, 
                            $"Analysis completed with score {result.Score}/10, rating: {result.OverallRating}");
                    }
                    else
                    {
                        StatusMessage = "AI analysis failed. Please try again.";
                        await _loggingService.LogErrorAsync("Chef Judge", "1", 
                            new InvalidOperationException("AI service returned null"), "AI analysis failed to return results");
                    }
                }
                else
                {
                    await _loggingService.LogFeatureUsageAsync("Chef Judge", "1", "User cancelled image upload");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error judging dish: {ex.Message}";
                await _loggingService.LogErrorAsync("Chef Judge", "1", ex, "Unexpected error in dish analysis");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SelectTab(string tabName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SelectTab called with: {tabName}");
                SelectedTab = tabName;
                StatusMessage = $"Switched to {tabName} tab";
                System.Diagnostics.Debug.WriteLine($"SelectedTab set to: {SelectedTab}");
                System.Diagnostics.Debug.WriteLine($"PropertyChanged event should fire for SelectedTab");
                
                // Load data based on selected tab
                switch (tabName)
                {
                    case "Dashboard":
                        await LoadRecipesAsync();
                        await LoadAnalyticsDataAsync(); // Load analytics for dashboard
                        break;
                    case "Recipes":
                        await LoadRecipesAsync();
                        break;
                    case "Ingredients":
                        await LoadIngredientsAsync();
                        break;
                    case "AI":
                        // AI features don't need data loading
                        break;
                    case "Analytics":
                        await LoadAnalyticsDataAsync();
                        break;
                    case "Settings":
                        await LoadSettingsAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error switching to {tabName}: {ex.Message}";
            }
        }

        private async Task AddNewRecipeAsync()
        {
            try
            {
                var dialog = new Views.RecipePopupDialog();
                if (dialog.ShowDialog() == true && dialog.Recipe != null)
                {
                    IsLoading = true;
                    StatusMessage = "Creating new recipe...";
                    
                    var createdRecipe = await _recipeService.CreateRecipeAsync(dialog.Recipe);
                    if (createdRecipe != null)
                    {
                        Recipes.Insert(0, createdRecipe);
                        SelectedRecipe = createdRecipe;
                        StatusMessage = $"Recipe '{createdRecipe.Title}' created successfully!";
                    }
                    else
                    {
                        StatusMessage = "Failed to create recipe";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating recipe: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddNewIngredientAsync()
        {
            try
            {
                var dialog = new Views.IngredientPopupDialog();
                if (dialog.ShowDialog() == true && dialog.Ingredient != null)
                {
                    IsLoading = true;
                    StatusMessage = "Adding new ingredient...";
                    
                    var createdIngredient = await _ingredientService.AddIngredientAsync(dialog.Ingredient);
                    if (createdIngredient != null)
                    {
                        Ingredients.Add(createdIngredient);
                        StatusMessage = $"Ingredient '{createdIngredient.Name}' added successfully!";
                    }
                    else
                    {
                        StatusMessage = "Failed to add ingredient";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding ingredient: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditRecipeAsync(Recipe recipe)
        {
            if (recipe == null) return;

            try
            {
                var dialog = new Views.RecipePopupDialog(recipe);
                if (dialog.ShowDialog() == true && dialog.Recipe != null)
                {
                    IsLoading = true;
                    StatusMessage = "Updating recipe...";
                    
                    var updatedRecipe = await _recipeService.UpdateRecipeAsync(dialog.Recipe);
                    if (updatedRecipe != null)
                    {
                        var index = Recipes.IndexOf(recipe);
                        if (index >= 0)
                        {
                            Recipes[index] = updatedRecipe;
                            SelectedRecipe = updatedRecipe;
                        }
                        StatusMessage = $"Recipe '{updatedRecipe.Title}' updated successfully!";
                    }
                    else
                    {
                        StatusMessage = "Failed to update recipe";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error editing recipe: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewRecipeAsync(Recipe recipe)
        {
            if (recipe == null) return;

            try
            {
                // Open recipe details dialog in read-only mode
                var dialog = new Views.RecipeDialog(recipe); // Open for viewing
                dialog.ShowDialog();
                StatusMessage = $"Viewing recipe: {recipe.Title}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error viewing recipe: {ex.Message}";
                await _loggingService.LogErrorAsync("Recipe View", "1", ex, "Error viewing recipe");
            }
        }

        private async Task ViewIngredientAsync(Ingredient ingredient)
        {
            if (ingredient == null) return;

            try
            {
                // Open ingredient details dialog in read-only mode
                var dialog = new Views.IngredientDialog(ingredient); // Open for viewing
                dialog.ShowDialog();
                StatusMessage = $"Viewing ingredient: {ingredient.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error viewing ingredient: {ex.Message}";
                await _loggingService.LogErrorAsync("Ingredient View", "1", ex, "Error viewing ingredient");
            }
        }

        private async Task EditIngredientAsync(Ingredient ingredient)
        {
            if (ingredient == null) return;

            try
            {
                var dialog = new Views.IngredientPopupDialog(ingredient);
                if (dialog.ShowDialog() == true && dialog.Ingredient != null)
                {
                    IsLoading = true;
                    StatusMessage = "Updating ingredient...";
                    
                    var updatedIngredient = await _ingredientService.UpdateIngredientAsync(dialog.Ingredient);
                    if (updatedIngredient != null)
                    {
                        var index = Ingredients.IndexOf(ingredient);
                        if (index >= 0)
                        {
                            Ingredients[index] = updatedIngredient;
                        }
                        StatusMessage = $"Ingredient '{updatedIngredient.Name}' updated successfully!";
                    }
                    else
                    {
                        StatusMessage = "Failed to update ingredient";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error editing ingredient: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteIngredientAsync(Ingredient ingredient)
        {
            if (ingredient == null) return;

            try
            {
                var dialog = new Views.ConfirmationDialog(
                    $"Are you sure you want to delete the ingredient '{ingredient.Name}'?\n\nThis action cannot be undone.", 
                    "Delete Ingredient");
                
                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Deleting ingredient...";
                    
                    var success = await _ingredientService.DeleteIngredientAsync(ingredient.Id);
                    if (success)
                    {
                        Ingredients.Remove(ingredient);
                        StatusMessage = "Ingredient deleted successfully";
                    }
                    else
                    {
                        StatusMessage = "Failed to delete ingredient";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting ingredient: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateShoppingListAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                Console.WriteLine("=== GENERATE SHOPPING LIST STARTED ===");
                IsLoading = true;
                StatusMessage = "Generating smart shopping list...";
                
                // Log feature usage
                await _loggingService.LogFeatureUsageAsync("Smart Shopping List", "1", 
                    $"Processing {Recipes.Count} recipes for shopping list generation");
                
                // Generate shopping list from available recipes
                var selectedRecipes = Recipes.Count > 0 
                    ? Recipes.Take(5).ToList() 
                    : new List<Recipe>();
                
                if (selectedRecipes.Count == 0)
                {
                    StatusMessage = "⚠️ No recipes available to generate shopping list";
                    return;
                }
                
                Console.WriteLine($"Selected {selectedRecipes.Count} recipes for shopping list");
                
                // Generate smart shopping list using the service
                Console.WriteLine("Generating smart shopping list from service...");
                var shoppingList = await _shoppingListService.GenerateSmartShoppingListAsync(selectedRecipes, 1);
                
                // Update metadata
                shoppingList.ListName = $"Shopping List for {selectedRecipes.Count} Recipes - {DateTime.Now:MM/dd/yyyy HH:mm}";
                shoppingList.GeneratedAt = DateTime.Now;
                
                Console.WriteLine($"Generated shopping list with {shoppingList.TotalItems} items from {shoppingList.Categories.Count} categories");
                
                // Optimize the shopping list if it has less than 3 categories
                var optimizedList = shoppingList;
                if (shoppingList.Categories.Count < 3 || shoppingList.TotalItems < 5)
                {
                    Console.WriteLine("Optimizing shopping list...");
                    optimizedList = await _shoppingListService.OptimizeShoppingListAsync(shoppingList);
                }
                
                // Log AI activity
                stopwatch.Stop();
                await _loggingService.LogAIActivityAsync("Smart Shopping List", "1", 
                    $"Recipes: {string.Join(", ", selectedRecipes.Select(r => r.Title))}", 
                    $"Generated {optimizedList.TotalItems} items, ${optimizedList.EstimatedCost:F2} cost", 
                    stopwatch.Elapsed);
                
                // Show shopping list dialog
                Console.WriteLine("Creating shopping list dialog...");
                var shoppingDialog = new Views.ShoppingListDialog(_shoppingListService);
                Console.WriteLine("Setting shopping list data...");
                shoppingDialog.SetShoppingList(optimizedList);
                Console.WriteLine("Showing dialog...");
                shoppingDialog.ShowDialog();
                Console.WriteLine("Dialog closed");
                
                StatusMessage = $"🛒 Smart shopping list generated! {optimizedList.TotalItems} items, " +
                              $"${optimizedList.EstimatedCost:F2} estimated cost, " +
                              $"{optimizedList.EstimatedShoppingTime.TotalMinutes:F0} minutes shopping time. " +
                              $"Organized by {optimizedList.Categories.Count} categories for efficient shopping.";
                
                // Log performance
                await _loggingService.LogPerformanceAsync("Smart Shopping List", "1", stopwatch.Elapsed, 
                    $"Generated {optimizedList.TotalItems} items with ${optimizedList.EstimatedCost:F2} estimated cost");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GenerateShoppingListAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                StatusMessage = $"Error generating shopping list: {ex.Message}";
                await _loggingService.LogErrorAsync("Smart Shopping List", "1", ex, "Unexpected error in shopping list generation");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OpenNutritionAnalysis()
        {
            try
            {
                // Show NutritionView in a new window
                var nutritionView = new Views.NutritionView();
                nutritionView.DataContext = NutritionViewModel;
                
                var nutritionWindow = new Window
                {
                    Title = "🥗 Nutrition Analysis - FoodBook",
                    Content = nutritionView,
                    Width = 1400,
                    Height = 800,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
                };
                
                nutritionWindow.ShowDialog();
                
                StatusMessage = "🍎 Nutrition Analysis view opened successfully!";
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening nutrition analysis: {ex.Message}";
            }
        }

        private async Task AnalyzeCustomNutritionAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (string.IsNullOrWhiteSpace(CustomRecipeText))
                {
                    StatusMessage = "Please enter a recipe text to analyze.";
                    return;
                }

                IsLoading = true;
                StatusMessage = "🤖 AI is parsing and analyzing your custom recipe...";
                
                // Log feature usage
                await _loggingService.LogFeatureUsageAsync("Custom Nutrition Analysis", "1", 
                    $"Analyzing custom recipe text: {CustomRecipeText.Length} characters");
                
                // Use the new unstructured recipe analysis
                var nutritionAnalysis = await _nutritionService.AnalyzeUnstructuredRecipeAsync(CustomRecipeText);
                
                // Get health alerts
                var healthAlerts = await _nutritionService.GetHealthAlertsAsync(nutritionAnalysis);
                
                // Get recommendations for general health
                var recommendations = await _nutritionService.GetNutritionRecommendationsAsync(nutritionAnalysis, "General Health");
                
                // Log AI activity
                stopwatch.Stop();
                await _loggingService.LogAIActivityAsync("Custom Nutrition Analysis", "1", 
                    $"Custom recipe: {CustomRecipeText.Substring(0, Math.Min(50, CustomRecipeText.Length))}...", 
                    $"Analysis: {nutritionAnalysis.TotalCalories:F0} cal, Grade: {nutritionAnalysis.Rating.Grade}", 
                    stopwatch.Elapsed);
                
                // Show custom nutrition analysis window
                var nutritionView = new Views.NutritionView();
                var nutritionWindow = new Window
                {
                    Title = "AI-powered Nutrition Analysis",
                    Content = nutritionView,
                    Width = 600,
                    Height = 800,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
                };
                
                nutritionWindow.ShowDialog();
                
                StatusMessage = $"🤖 AI-powered nutrition analysis complete! " +
                              $"Total: {nutritionAnalysis.TotalCalories:F0} calories, " +
                              $"{nutritionAnalysis.TotalProtein:F1}g protein, " +
                              $"{nutritionAnalysis.TotalCarbs:F1}g carbs, " +
                              $"{nutritionAnalysis.TotalFat:F1}g fat. " +
                              $"Grade: {nutritionAnalysis.Rating.Grade} ({nutritionAnalysis.Rating.OverallScore}/100). " +
                              $"{healthAlerts.Count()} health alerts found.";
                
                // Log performance
                await _loggingService.LogPerformanceAsync("Custom Nutrition Analysis", "1", stopwatch.Elapsed, 
                    $"Analysis completed: {nutritionAnalysis.TotalCalories:F0} calories, Grade {nutritionAnalysis.Rating.Grade}, {healthAlerts.Count()} alerts");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error analyzing custom nutrition: {ex.Message}";
                await _loggingService.LogErrorAsync("Custom Nutrition Analysis", "1", ex, "Unexpected error in custom nutrition analysis");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadSettingsAsync()
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                
                // Ensure default values if empty
                SelectedTheme = string.IsNullOrWhiteSpace(settings.Theme) ? "Light" : settings.Theme;
                var loadedLanguage = string.IsNullOrWhiteSpace(settings.Language) ? "English" : settings.Language;
                NotificationsEnabled = settings.NotificationsEnabled;
                DefaultServings = settings.DefaultServings > 0 ? settings.DefaultServings : 4;
                
                // Set SelectedLanguage and refresh localization so bound texts update
                SelectedLanguage = loadedLanguage;
                RefreshLocalization();
                
                // If the loaded values are different from defaults, update them
                if (SelectedTheme != settings.Theme || SelectedLanguage != settings.Language || DefaultServings != settings.DefaultServings)
                {
                    var updatedSettings = new AppSettings
                    {
                        Theme = SelectedTheme,
                        Language = SelectedLanguage,
                        NotificationsEnabled = NotificationsEnabled,
                        DefaultServings = DefaultServings
                    };
                    await _settingsService.SaveSettingsAsync(updatedSettings);
                }
                
                StatusMessage = "Settings loaded successfully";
            }
            catch (Exception ex)
            {
                // Use default values on error
                SelectedTheme = "Light";
                SelectedLanguage = "English";
                NotificationsEnabled = true;
                DefaultServings = 4;
                StatusMessage = $"Error loading settings: {ex.Message}";
            }
        }
        
        private async Task SaveSettingsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Saving settings...";
                
                var settings = new AppSettings
                {
                    Theme = SelectedTheme,
                    Language = SelectedLanguage,
                    NotificationsEnabled = NotificationsEnabled,
                    DefaultServings = DefaultServings
                };
                
                await _settingsService.SaveSettingsAsync(settings);
                
                StatusMessage = "✅ Settings saved successfully!";
                
                // Log settings change
                await _loggingService.LogFeatureUsageAsync("Settings", "1", 
                    $"Settings updated: Theme={SelectedTheme}, Language={SelectedLanguage}, Notifications={NotificationsEnabled}, Servings={DefaultServings}");

                // Apply localization changes after save so UI updates
                RefreshLocalization();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving settings: {ex.Message}";
                await _loggingService.LogErrorAsync("Settings", "1", ex, "Failed to save settings");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RefreshLocalization()
        {
            try
            {
                if (_localizationService != null)
                {
                    _localizationService.ChangeLanguage(SelectedLanguage);

                    OnPropertyChanged(nameof(Loc_Dashboard));
                    OnPropertyChanged(nameof(Loc_MyRecipes));
                    OnPropertyChanged(nameof(Loc_Ingredients));
                    OnPropertyChanged(nameof(Loc_ShoppingList));
                    OnPropertyChanged(nameof(Loc_Analytics));
                    OnPropertyChanged(nameof(Loc_Settings));
                    OnPropertyChanged(nameof(Loc_ApplicationSettings));
                    OnPropertyChanged(nameof(Loc_Theme));
                    OnPropertyChanged(nameof(Loc_Language));
                    OnPropertyChanged(nameof(Loc_DefaultServings));
                    OnPropertyChanged(nameof(Loc_EnableNotifications));
                    OnPropertyChanged(nameof(Loc_SaveSettings));
                    OnPropertyChanged(nameof(Loc_UserProfile));
                    OnPropertyChanged(nameof(Loc_Username));
                    OnPropertyChanged(nameof(Loc_Email));
                    OnPropertyChanged(nameof(Loc_TotalRecipes));
                    OnPropertyChanged(nameof(Loc_MainDishes));
                    OnPropertyChanged(nameof(Loc_Desserts));
                    OnPropertyChanged(nameof(Loc_QuickMeals));
                    OnPropertyChanged(nameof(Loc_AverageCookTime));
                    OnPropertyChanged(nameof(Loc_MostUsedIngredient));
                    OnPropertyChanged(nameof(Loc_AIRecipe));
                    OnPropertyChanged(nameof(Loc_FavoriteRecipes));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshLocalization error: {ex.Message}");
            }
        }

        private async Task LoadAnalyticsDataAsync()
        {
            try
            {
                IsLoading = true;
                await _dbAccessLock.WaitAsync();
                StatusMessage = "Loading analytics data...";

                // Load basic statistics from database
                var recipes = await _recipeService.GetAllRecipesAsync();
                var ingredients = await _ingredientService.GetUserIngredientsAsync(1); // Use user 1 for demo
                var logs = await _loggingService.GetLogsAsync();
                
                System.Diagnostics.Debug.WriteLine($"Analytics Debug - Recipes: {recipes?.Count() ?? 0}, Ingredients: {ingredients?.Count() ?? 0}, Logs: {logs?.Count() ?? 0}");

                // Update basic stats with real data
                MyTotalRecipes = recipes?.Count() ?? 0;
                TotalIngredients = ingredients?.Count() ?? 0;
                TotalLogsCount = logs?.Count() ?? 0;
                
                // Find most used ingredient from ingredients
                if (ingredients?.Any() == true)
                {
                    var mostUsed = ingredients
                        .GroupBy(i => i.Name)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault();
                    MostUsedIngredient = mostUsed?.Key ?? "None";
                }
                else
                {
                    MostUsedIngredient = "None";
                }

                if (recipes?.Any() == true)
                {
                    MyAverageCookTime = Math.Round(recipes.Average(r => r.CookTime), 1);
                }
                else
                {
                    MyAverageCookTime = 0;
                }

                // AI statistics from logs
                var aiLogs = logs?.Where(l => l.FeatureName?.Contains("AI") == true || l.FeatureName?.Contains("Generate") == true || l.FeatureName?.Contains("Judge") == true).ToList() ?? new List<Foodbook.Data.Entities.LogEntry>();
                AIJudgments = aiLogs.Count(l => l.FeatureName?.Contains("Judge") == true);
                GeneratedRecipes = aiLogs.Count(l => l.FeatureName?.Contains("Generate") == true);
                
                var successfulOperations = aiLogs.Count(l => !l.Message.Contains("Error") && !l.Message.Contains("Failed"));
                SuccessRate = aiLogs.Any() ? Math.Round((double)successfulOperations / aiLogs.Count * 100, 1) : 0;
                AIEngagementPercent = SuccessRate; // use success rate as proxy for engagement
                AIOperationsCount = aiLogs.Count;
                AIErrorCount = aiLogs.Count(l => l.Message.Contains("Error") || l.Level.Equals("Error", StringComparison.OrdinalIgnoreCase));

                // Pantry coverage: percent of ingredients currently available (>0 qty)
                if (ingredients?.Any() == true)
                {
                    var available = ingredients.Count(i => (i.Quantity ?? 0) > 0);
                    PantryCoveragePercent = Math.Round((double)available / Math.Max(ingredients.Count(), 1) * 100, 1);

                    // Smart Pantry KPIs
                    ExpiringSoonCount = ingredients.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value.Date >= DateTime.UtcNow.Date && i.ExpiryDate.Value <= DateTime.UtcNow.AddDays(3));
                    ExpiredCount = ingredients.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value < DateTime.UtcNow);

                    // Top ingredients by occurrences (name), take top 5
                    var top = ingredients
                        .GroupBy(i => i.Name)
                        .Select(g => new KeyValuePair<string,int>(g.Key, g.Count()))
                        .OrderByDescending(kv => kv.Value)
                        .Take(5)
                        .ToList();
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        TopIngredients.Clear();
                        foreach (var kv in top) TopIngredients.Add(kv);
                    });
                }
                else
                {
                    PantryCoveragePercent = 0;
                    ExpiringSoonCount = 0;
                    ExpiredCount = 0;
                    TopIngredients.Clear();
                }

                // Create chart data with real database data
                System.Diagnostics.Debug.WriteLine("Creating chart data with real data...");
                await CreateChartDataAsync(recipes, ingredients, logs);
                System.Diagnostics.Debug.WriteLine("Chart data created successfully");

                StatusMessage = $"Analytics loaded: {MyTotalRecipes} recipes, {ingredients?.Count() ?? 0} ingredients";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading analytics: {ex.Message}";
                // Set fallback values
                MyTotalRecipes = 0;
                MyAverageCookTime = 0;
                MyMostCommonDifficulty = "Easy";
                MyAIGeneratedRecipes = 0;
                MostUsedIngredient = "None";
                AIJudgments = 0;
                GeneratedRecipes = 0;
                SuccessRate = 0;
                await CreateDemoChartDataAsync();
            }
            finally
            {
                if (_dbAccessLock.CurrentCount == 0) _dbAccessLock.Release();
                IsLoading = false;
            }
        }


        private async Task CalculateMyRecipesStatistics(IEnumerable<Recipe> recipes)
        {
            try
            {
                // Total recipes
                MyTotalRecipes = recipes.Count();

                // Average cook time
                if (recipes.Any())
                {
                    MyAverageCookTime = Math.Round(recipes.Average(r => r.CookTime), 1);
                }
                else
                {
                    MyAverageCookTime = 0;
                }

                // Most common difficulty
                if (recipes.Any())
                {
                    var difficultyGroups = recipes
                        .GroupBy(r => r.Difficulty)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault();
                    MyMostCommonDifficulty = difficultyGroups?.Key ?? "Easy";
                }
                else
                {
                    MyMostCommonDifficulty = "Easy";
                }

                // AI Generated recipes (recipes with "AI" in category or title)
                MyAIGeneratedRecipes = recipes.Count(r => 
                    (r.Category?.Contains("AI") == true) || 
                    (r.Title?.Contains("AI") == true) ||
                    (r.Description?.Contains("AI") == true));

                // Alternative: Count from logs if available
                try
                {
                    var logs = await _loggingService.GetLogsAsync();
                    var aiGeneratedLogs = logs?.Count(l => 
                        l.FeatureName?.Contains("AI") == true && 
                        l.FeatureName?.Contains("Generate") == true) ?? 0;
                    
                    // Use the higher count between recipes and logs
                    MyAIGeneratedRecipes = Math.Max(MyAIGeneratedRecipes, aiGeneratedLogs);
                }
                catch
                {
                    // Keep the recipe-based count if logs fail
                }
            }
            catch
            {
                // Set default values on error
                MyTotalRecipes = 0;
                MyAverageCookTime = 0;
                MyMostCommonDifficulty = "Easy";
                MyAIGeneratedRecipes = 0;
            }
        }

        private Task CreateChartDataAsync(IEnumerable<Recipe>? recipes, IEnumerable<Ingredient>? ingredients, IEnumerable<Foodbook.Data.Entities.LogEntry>? logs)
        {
            // Recipe trend chart (last 7 days) - using real data
            var recipeTrendData = new List<double>();
            var recipeLabels = new List<string>();
            
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                var dayRecipes = recipes?.Count(r => r.CreatedAt.Date == date.Date) ?? 0;
                recipeTrendData.Add(dayRecipes);
                recipeLabels.Add(date.ToString("MMM dd"));
            }

            // Use real data from database - if no recipes created in last 7 days, show 0
            // This gives accurate real-time statistics

            RecipeTrendSeries =
            [
                new LineSeries<double>
                {
                    Values = recipeTrendData,
                    Name = "Recipes Created",
                    Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 3),
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(50))
                }
            ];

            // Ingredient usage chart - using available ingredients data
            if (ingredients?.Any() == true)
            {
                var ingredientUsage = ingredients
                    .GroupBy(i => i.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                if (ingredientUsage.Any())
                {
                    var ingredientValues = ingredientUsage.Select(x => (double)x.Count).ToArray();

                    IngredientUsageSeries =
                    [
                        new ColumnSeries<double>
                        {
                            Values = ingredientValues,
                            Name = "Usage Count",
                            Fill = new SolidColorPaint(SKColor.Parse("#10B981"))
                        }
                    ];
                }
                else
                {
                    // Show empty chart when no real ingredients - real-time data
                    IngredientUsageSeries =
                    [
                        new ColumnSeries<double>
                        {
                            Values = [0],
                            Name = "No Ingredients",
                            Fill = new SolidColorPaint(SKColor.Parse("#6B7280"))
                        }
                    ];
                }
            }
            else
            {
                // Show empty chart when no ingredients available - real-time data
                IngredientUsageSeries =
                [
                    new ColumnSeries<double>
                    {
                        Values = [0],
                        Name = "No Ingredients",
                        Fill = new SolidColorPaint(SKColor.Parse("#6B7280"))
                    }
                ];
            }

            // AI Performance pie chart - using real log data
            var aiSuccessCount = logs?.Count(l => l.FeatureName?.Contains("AI") == true && !l.Message.Contains("Error")) ?? 0;
            var aiErrorCount = logs?.Count(l => l.FeatureName?.Contains("AI") == true && l.Message.Contains("Error")) ?? 0;

            // Use real AI performance data - if no AI operations, show 0
            // This gives accurate real-time statistics

            AIPerformanceSeries =
            [
                new PieSeries<double>
                {
                    Values = [aiSuccessCount, aiErrorCount],
                    Name = "AI Performance",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F0}"
                }
            ];

            // Recipe Distribution Chart - using real recipe data by category
            if (recipes?.Any() == true)
            {
                var categoryGroups = recipes
                    .GroupBy(r => r.Category ?? "Other")
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var categoryValues = categoryGroups.Select(x => (double)x.Count).ToArray();
                var categoryLabels = categoryGroups.Select(x => x.Category).ToArray();

                // Create pie chart for recipe distribution
                RecipeDistributionSeries =
                [
                    new PieSeries<double>
                    {
                        Values = categoryValues,
                        Name = "Recipe Distribution",
                        Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                        DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F0}"
                    }
                ];
            }
            else
            {
                // Show empty chart when no recipes available
                RecipeDistributionSeries =
                [
                    new PieSeries<double>
                    {
                        Values = [0],
                        Name = "No Recipes",
                        Fill = new SolidColorPaint(SKColor.Parse("#6B7280"))
                    }
                ];
            }

            // Cook time distribution - using real recipe data
            if (recipes?.Any() == true)
            {
                var cookTimeRanges = new[]
                {
                    ("0-15 min", recipes.Count(r => r.CookTime <= 15)),
                    ("16-30 min", recipes.Count(r => r.CookTime > 15 && r.CookTime <= 30)),
                    ("31-45 min", recipes.Count(r => r.CookTime > 30 && r.CookTime <= 45)),
                    ("46-60 min", recipes.Count(r => r.CookTime > 45 && r.CookTime <= 60)),
                    ("60+ min", recipes.Count(r => r.CookTime > 60))
                };

                var cookTimeValues = cookTimeRanges.Select(x => (double)x.Item2).ToArray();

                // Use real cook time data from database - if no recipes, show 0
                // This gives accurate real-time statistics

                CookTimeDistributionSeries =
                [
                    new ColumnSeries<double>
                    {
                        Values = cookTimeValues,
                        Name = "Cook Time Distribution",
                        Fill = new SolidColorPaint(SKColor.Parse("#F59E0B"))
                    }
                ];
            }
            else
            {
                // Show empty chart when no recipes available - real-time data
                CookTimeDistributionSeries =
                [
                    new ColumnSeries<double>
                    {
                        Values = [0],
                        Name = "No Recipes",
                        Fill = new SolidColorPaint(SKColor.Parse("#6B7280"))
                    }
                ];
            }

            // Calculate Dashboard Chart Data from real database
            CalculateDashboardChartData(recipes);
            
            return Task.CompletedTask;
        }

        private void CalculateDashboardChartData(IEnumerable<Recipe>? recipes)
        {
            if (recipes?.Any() != true)
            {
                // Set default values when no recipes
                TotalRecipeCount = 0;
                MainDishesCount = 0;
                DessertsCount = 0;
                QuickMealsCount = 0;
                CookingTime0To30Count = 0;
                CookingTime30To60Count = 0;
                CookingTime60To90Count = 0;
                CookingTime90To120Count = 0;
                CookingTime120PlusCount = 0;
                EasyDifficultyCount = 0;
                MediumDifficultyCount = 0;
                HardDifficultyCount = 0;
                MonthlyRecipeCounts.Clear();
                return;
            }

            // Recipe Distribution by Category
            TotalRecipeCount = recipes.Count();
            
            var categoryGroups = recipes
                .GroupBy(r => r.Category?.ToLower() ?? "other")
                .ToDictionary(g => g.Key, g => g.Count());

            MainDishesCount = categoryGroups.GetValueOrDefault("main dish", 0) + 
                             categoryGroups.GetValueOrDefault("main dishes", 0) +
                             categoryGroups.GetValueOrDefault("entree", 0);
            
            DessertsCount = categoryGroups.GetValueOrDefault("dessert", 0) + 
                           categoryGroups.GetValueOrDefault("desserts", 0) +
                           categoryGroups.GetValueOrDefault("sweet", 0);
            
            QuickMealsCount = categoryGroups.GetValueOrDefault("quick meal", 0) + 
                             categoryGroups.GetValueOrDefault("quick meals", 0) +
                             categoryGroups.GetValueOrDefault("fast", 0) +
                             categoryGroups.GetValueOrDefault("snack", 0);

            // Cooking Time Distribution
            CookingTime0To30Count = recipes.Count(r => r.CookTime <= 30);
            CookingTime30To60Count = recipes.Count(r => r.CookTime > 30 && r.CookTime <= 60);
            CookingTime60To90Count = recipes.Count(r => r.CookTime > 60 && r.CookTime <= 90);
            CookingTime90To120Count = recipes.Count(r => r.CookTime > 90 && r.CookTime <= 120);
            CookingTime120PlusCount = recipes.Count(r => r.CookTime > 120);

            // Difficulty Level Distribution
            EasyDifficultyCount = recipes.Count(r => r.Difficulty?.ToLower() == "easy");
            MediumDifficultyCount = recipes.Count(r => r.Difficulty?.ToLower() == "medium");
            HardDifficultyCount = recipes.Count(r => r.Difficulty?.ToLower() == "hard");

            // Monthly Recipe Creation (last 7 months)
            MonthlyRecipeCounts.Clear();
            for (int i = 6; i >= 0; i--)
            {
                var month = DateTime.Now.AddMonths(-i);
                var monthRecipes = recipes.Count(r => 
                    r.CreatedAt.Year == month.Year && 
                    r.CreatedAt.Month == month.Month);
                MonthlyRecipeCounts.Add(monthRecipes);
            }
        }

        private void InitializeCharts()
        {
            // Initialize charts with empty data to ensure they display
            RecipeTrendSeries =
            [
                new LineSeries<double>
                {
                    Values = [0],
                    Name = "Recipes Created",
                    Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 3),
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(50))
                }
            ];

            IngredientUsageSeries =
            [
                new ColumnSeries<double>
                {
                    Values = [0],
                    Name = "Usage Count",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981"))
                }
            ];

            AIPerformanceSeries =
            [
                new PieSeries<double>
                {
                    Values = [0],
                    Name = "AI Performance",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F0}"
                }
            ];

            CookTimeDistributionSeries =
            [
                new ColumnSeries<double>
                {
                    Values = [0],
                    Name = "Cook Time Distribution",
                    Fill = new SolidColorPaint(SKColor.Parse("#F59E0B"))
                }
            ];

            RecipeDistributionSeries =
            [
                new PieSeries<double>
                {
                    Values = [0],
                    Name = "Recipe Distribution",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981"))
                }
            ];
        }

        private Task CreateDemoChartDataAsync()
        {
            // Demo data for when no real data is available
            RecipeTrendSeries =
            [
                new LineSeries<double>
                {
                    Values = [2, 1, 3, 2, 4, 3, 5],
                    Name = "Recipes Created",
                    Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 3),
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(50))
                }
            ];

            IngredientUsageSeries =
            [
                new ColumnSeries<double>
                {
                    Values = [8, 6, 5, 4, 3],
                    Name = "Usage Count",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981"))
                }
            ];

            AIPerformanceSeries =
            [
                new PieSeries<double>
                {
                    Values = [85, 15],
                    Name = "AI Performance",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F0}%"
                }
            ];

            CookTimeDistributionSeries =
            [
                new ColumnSeries<double>
                {
                    Values = [2, 5, 3, 2, 1],
                    Name = "Cook Time Distribution",
                    Fill = new SolidColorPaint(SKColor.Parse("#F59E0B"))
                }
            ];
            
            return Task.CompletedTask;
        }

        public async Task LoadCurrentUserAsync()
        {
            try
            {
                if (_authenticationService != null)
                {
                    CurrentUser = await _authenticationService.GetCurrentUserAsync();
                    if (CurrentUser != null)
                    {
                        Greeting = BuildGreeting(CurrentUser.Username);
                        StatusMessage = $"Welcome back, {CurrentUser.Username}!";
                    }
                    else
                    {
                        Greeting = BuildGreeting("Guest");
                        StatusMessage = "Please log in to continue";
                    }
                }
                else
                {
                    // Create a demo user for testing
                    CurrentUser = new User 
                    { 
                        Id = 1, 
                        Username = "DemoUser", 
                        Email = "demo@foodbook.com",
                        CreatedAt = DateTime.Now
                    };
                    Greeting = BuildGreeting(CurrentUser.Username);
                    StatusMessage = "Demo mode - Welcome, DemoUser!";
                }
            }
            catch (Exception ex)
            {
                // Create a fallback user
                CurrentUser = new User 
                { 
                    Id = 1, 
                    Username = "Guest", 
                    Email = "guest@foodbook.com",
                    CreatedAt = DateTime.Now
                };
                Greeting = BuildGreeting(CurrentUser.Username);
                StatusMessage = "Guest mode - Welcome, Guest!";
                System.Diagnostics.Debug.WriteLine($"Error loading current user: {ex.Message}");
            }
        }

        private static string BuildGreeting(string? username)
        {
            var hour = DateTime.Now.Hour;
            var salutation = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
            return string.IsNullOrWhiteSpace(username) ? salutation : $"{salutation}, {username}!";
        }

        // Additional command implementations
        private async Task SortRecipesAsync(string sortBy)
        {
            try
            {
                SortBy = sortBy;
                StatusMessage = $"Sorting recipes by {sortBy}...";
                
                // Get current filtered recipes
                var currentRecipes = Recipes.ToList();
                
                switch (sortBy)
                {
                    case "Name A-Z":
                        currentRecipes = currentRecipes.OrderBy(r => r.Title).ToList();
                        break;
                    case "Name Z-A":
                        currentRecipes = currentRecipes.OrderByDescending(r => r.Title).ToList();
                        break;
                    case "Time":
                        currentRecipes = currentRecipes.OrderBy(r => r.CookTime).ToList();
                        break;
                    case "Time Desc":
                        currentRecipes = currentRecipes.OrderByDescending(r => r.CookTime).ToList();
                        break;
                    case "Difficulty":
                        // Sort by difficulty: Easy, Medium, Hard
                        var difficultyOrder = new Dictionary<string, int> { { "Easy", 1 }, { "Medium", 2 }, { "Hard", 3 } };
                        currentRecipes = currentRecipes.OrderBy(r => difficultyOrder.GetValueOrDefault(r.Difficulty ?? "Easy", 1)).ToList();
                        break;
                    case "Difficulty Desc":
                        var difficultyOrderDesc = new Dictionary<string, int> { { "Easy", 3 }, { "Medium", 2 }, { "Hard", 1 } };
                        currentRecipes = currentRecipes.OrderBy(r => difficultyOrderDesc.GetValueOrDefault(r.Difficulty ?? "Easy", 3)).ToList();
                        break;
                    case "Date Created":
                        currentRecipes = currentRecipes.OrderByDescending(r => r.CreatedAt).ToList();
                        break;
                }
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in currentRecipes)
                    {
                        Recipes.Add(recipe);
                    }
                });
                
                StatusMessage = $"Recipes sorted by {sortBy}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error sorting recipes: {ex.Message}";
            }
        }

        private async Task FilterRecipesByCategoryAsync(string category)
        {
            try
            {
                SelectedCategory = category;
                StatusMessage = $"Filtering recipes by {category}...";
                
                List<Recipe> filteredRecipes;
                
                if (category == "All")
                {
                    filteredRecipes = _allRecipes.ToList();
                }
                else
                {
                    // Filter based on category
                    filteredRecipes = _allRecipes.Where(r => 
                    {
                        if (category == "Main Dishes")
                            return r.Category?.Contains("Main", StringComparison.OrdinalIgnoreCase) == true ||
                                   r.Category?.Contains("Dish", StringComparison.OrdinalIgnoreCase) == true;
                        else if (category == "Desserts")
                            return r.Category?.Contains("Dessert", StringComparison.OrdinalIgnoreCase) == true ||
                                   r.Category?.Contains("Sweet", StringComparison.OrdinalIgnoreCase) == true;
                        else if (category == "Quick")
                            return r.CookTime <= 30; // Quick recipes are 30 minutes or less
                        else
                            return r.Category?.Contains(category, StringComparison.OrdinalIgnoreCase) == true ||
                                   r.Title?.Contains(category, StringComparison.OrdinalIgnoreCase) == true;
                    }).ToList();
                }
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in filteredRecipes)
                    {
                        Recipes.Add(recipe);
                    }
                });
                
                StatusMessage = $"Filtered {Recipes.Count} recipes by {category}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error filtering recipes: {ex.Message}";
            }
        }

        private async Task TestDatabaseConnectionAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Testing database connection...";
                
                // Test connection by loading recipes
                await LoadRecipesAsync();
                
                StatusMessage = "✅ Database connection successful!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Database connection failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ConfigureAISettingsAsync()
        {
            try
            {
                StatusMessage = "Opening AI configuration...";
                // For now, just switch to AI tab
                await SelectTab("AI");
                StatusMessage = "AI configuration opened";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening AI configuration: {ex.Message}";
            }
        }

        private void ViewLogsAsync()
        {
            // Prevent multiple simultaneous calls
            if (_isOpeningLogs)
            {
                return;
            }

            _isOpeningLogs = true;
            
            try
            {
                if (_loggingService == null)
                {
                    MessageBox.Show("Logging service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Open dialog directly
                var logViewer = new Views.LogViewerWindow(_loggingService);
                logViewer.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                logViewer.Topmost = true;
                
                logViewer.ShowDialog();
                
                StatusMessage = "Log viewer closed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log viewer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"LogViewer Error: {ex.Message}");
            }
            finally
            {
                _isOpeningLogs = false;
            }
        }

        private async Task ToggleSidebarAsync()
        {
            var oldValue = IsSidebarCollapsed;
            IsSidebarCollapsed = !IsSidebarCollapsed;
            StatusMessage = IsSidebarCollapsed ? "Sidebar collapsed" : "Sidebar expanded";
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"ToggleSidebar: {oldValue} -> {IsSidebarCollapsed}");
            
            await Task.CompletedTask;
        }

        // Method to refresh dashboard data from database
        public async Task RefreshDashboardDataAsync()
        {
            try
            {
                await LoadRecipesAsync();
                await LoadAnalyticsDataAsync();
                StatusMessage = "Dashboard data refreshed from FoodBook database";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing dashboard: {ex.Message}";
            }
        }
        
        private void UpdatePaging()
        {
            try
            {
                var source = Recipes ?? new ObservableCollection<Recipe>();
                TotalPages = (int)Math.Ceiling(source.Count / (double)Math.Max(1, PageSize));
                if (TotalPages == 0) TotalPages = 1;
                if (CurrentPage > TotalPages) CurrentPage = TotalPages;
                var start = (CurrentPage - 1) * Math.Max(1, PageSize);
                var pageItems = source.Skip(start).Take(Math.Max(1, PageSize)).ToList();
                PagedRecipes.Clear();
                foreach (var r in pageItems) PagedRecipes.Add(r);

                // Refresh quick-jump options and selection
                if (PageOptions.Count != TotalPages)
                {
                    PageOptions.Clear();
                    for (int i = 1; i <= TotalPages; i++) PageOptions.Add(i);
                }
                if (SelectedPage != CurrentPage)
                {
                    _selectedPage = CurrentPage; // avoid recursion through setter
                    OnPropertyChanged(nameof(SelectedPage));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePaging error: {ex.Message}");
            }
        }
        
        // Ingredient Filter Methods
        private async Task SortIngredientsAsync(string sortBy)
        {
            try
            {
                IngredientSortBy = sortBy;
                StatusMessage = $"Sorting ingredients by {sortBy}...";
                
                // Get current filtered ingredients
                var currentIngredients = Ingredients.ToList();
                
                switch (sortBy)
                {
                    case "Name A-Z":
                        currentIngredients = currentIngredients.OrderBy(i => i.Name).ToList();
                        break;
                    case "Name Z-A":
                        currentIngredients = currentIngredients.OrderByDescending(i => i.Name).ToList();
                        break;
                    case "Category":
                        currentIngredients = currentIngredients.OrderBy(i => i.Category).ToList();
                        break;
                    case "Date Added":
                        currentIngredients = currentIngredients.OrderByDescending(i => i.CreatedAt).ToList();
                        break;
                }
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Ingredients.Clear();
                    foreach (var ingredient in currentIngredients)
                    {
                        Ingredients.Add(ingredient);
                    }
                });
                
                StatusMessage = $"Ingredients sorted by {sortBy}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error sorting ingredients: {ex.Message}";
            }
        }
        
        private async Task FilterIngredientsByCategoryAsync(string category)
        {
            try
            {
                SelectedIngredientCategory = category;
                StatusMessage = $"Filtering ingredients by {category}...";
                
                List<Ingredient> filteredIngredients;
                
                if (category == "All")
                {
                    filteredIngredients = _allIngredients.ToList();
                }
                else
                {
                    // Filter based on category
                    filteredIngredients = _allIngredients.Where(i => 
                    {
                        if (category == "Proteins")
                            return i.Category?.Contains("Protein", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Meat", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Fish", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Chicken", StringComparison.OrdinalIgnoreCase) == true;
                        else if (category == "Grains")
                            return i.Category?.Contains("Grain", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Rice", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Wheat", StringComparison.OrdinalIgnoreCase) == true;
                        else if (category == "Vegetables")
                            return i.Category?.Contains("Vegetable", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Vegetable", StringComparison.OrdinalIgnoreCase) == true;
                        else if (category == "Spices")
                            return i.Category?.Contains("Spice", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Herb", StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Category?.Contains("Seasoning", StringComparison.OrdinalIgnoreCase) == true;
                        else
                            return i.Category?.Contains(category, StringComparison.OrdinalIgnoreCase) == true ||
                                   i.Name?.Contains(category, StringComparison.OrdinalIgnoreCase) == true;
                    }).ToList();
                }
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Ingredients.Clear();
                    foreach (var ingredient in filteredIngredients)
                    {
                        Ingredients.Add(ingredient);
                    }
                });
                
                StatusMessage = $"Filtered {Ingredients.Count} ingredients by {category}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error filtering ingredients: {ex.Message}";
            }
        }
        
        private async Task NavigateToIngredientCategory(string category)
        {
            try
            {
                // Set the selected category
                SelectedIngredientCategory = category;
                
                // Filter ingredients by category
                await FilterIngredientsByCategoryAsync(category);
                
                StatusMessage = $"Switched to {category} ingredients";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error navigating to {category}: {ex.Message}";
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public async void Execute(object? parameter)
        {
            await _execute();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke((T)parameter!) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute((T)parameter!);
        }
    }
}
