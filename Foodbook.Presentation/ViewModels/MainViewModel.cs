using System.Collections.ObjectModel;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using System.Threading;

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
        private readonly IAuthenticationService? _authenticationService;
        public ILoggingService LoggingService => _loggingService;
        
        // Semaphore to prevent concurrent DbContext access
        private static readonly SemaphoreSlim _dbContextSemaphore = new SemaphoreSlim(1, 1);

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

        // Analytics properties
        private int _totalRecipes = 0;
        private string _mostUsedIngredient = "None";
        private double _averageCookTime = 0;
        private int _aiJudgments = 0;
        private int _generatedRecipes = 0;
        private double _successRate = 0;
        private object[] _recipeTrendSeries = new object[] { };
        private object[] _ingredientUsageSeries = new object[] { };
        private object[] _aiPerformanceSeries = new object[] { };
        private object[] _cookTimeDistributionSeries = new object[] { };
        private object[] _recipeDistributionSeries = new object[] { };
        private User? _currentUser;

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

        public MainViewModel(
            IRecipeService recipeService,
            IIngredientService ingredientService,
            IAIService aiService,
            IUserService userService,
            IShoppingListService shoppingListService,
            INutritionService nutritionService,
            ILoggingService loggingService,
            ISettingsService settingsService,
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
            _authenticationService = authenticationService;
            
            // Initialize all commands
            LoadRecipesCommand = new RelayCommand(async () => await LoadRecipesAsync());
            SearchRecipesCommand = new RelayCommand(async () => await SearchRecipesAsync());
            LoadIngredientsCommand = new RelayCommand(async () => await LoadIngredientsAsync());
            DeleteRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await DeleteRecipeAsync(recipe));
            AdjustServingsCommand = new RelayCommand(async () => await AdjustServingsAsync());
            GenerateRecipeCommand = new RelayCommand(async () => await GenerateRecipeAsync());
            JudgeDishCommand = new RelayCommand(async () => await JudgeDishAsync());
            GenerateShoppingListCommand = new RelayCommand(async () => await GenerateShoppingListAsync());
            AnalyzeNutritionCommand = new RelayCommand(async () => await AnalyzeNutritionAsync());
            AnalyzeCustomNutritionCommand = new RelayCommand(async () => await AnalyzeCustomNutritionAsync());
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
            ViewLogsCommand = new RelayCommand(async () => await ViewLogsAsync());
            RefreshUserProfileCommand = new RelayCommand(async () => await LoadCurrentUserAsync());
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            
            // Initialize ingredient commands
            AddIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            ViewIngredientAnalyticsCommand = new RelayCommand(async () => await SelectTab("Analytics"));
            
            // Initialize ingredient tab navigation commands
            NavigateToAllIngredientsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("All"));
            NavigateToProteinsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Proteins"));
            NavigateToGrainsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Grains"));
            NavigateToVegetablesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Vegetables"));
            NavigateToSpicesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Spices"));
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
            AnalyzeNutritionCommand = new RelayCommand(async () => await AnalyzeNutritionAsync());
            AnalyzeCustomNutritionCommand = new RelayCommand(async () => await AnalyzeCustomNutritionAsync());
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
            ViewLogsCommand = new RelayCommand(async () => await ViewLogsAsync());
            RefreshUserProfileCommand = new RelayCommand(async () => await LoadCurrentUserAsync());
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            
            // Initialize ingredient filter commands
            SortIngredientsByCommand = new RelayCommand<string>(async (sortBy) => await SortIngredientsAsync(sortBy));
            FilterIngredientsByCategoryCommand = new RelayCommand<string>(async (category) => await FilterIngredientsByCategoryAsync(category));
            
            // Initialize ingredient commands
            AddIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            ViewIngredientAnalyticsCommand = new RelayCommand(async () => await SelectTab("Analytics"));
            
            // Initialize ingredient tab navigation commands
            NavigateToAllIngredientsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("All"));
            NavigateToProteinsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Proteins"));
            NavigateToGrainsCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Grains"));
            NavigateToVegetablesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Vegetables"));
            NavigateToSpicesCommand = new RelayCommand(async () => await NavigateToIngredientCategory("Spices"));
            
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

        public ICommand LoadRecipesCommand { get; }
        public ICommand SearchRecipesCommand { get; }
        public ICommand LoadIngredientsCommand { get; }
        public ICommand DeleteRecipeCommand { get; }
        public ICommand AdjustServingsCommand { get; }
        public ICommand GenerateRecipeCommand { get; }
        public ICommand JudgeDishCommand { get; }
        public ICommand GenerateShoppingListCommand { get; }
        public ICommand AnalyzeNutritionCommand { get; }
        public ICommand AnalyzeCustomNutritionCommand { get; }
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

        // Analytics Properties
        public int TotalRecipes
        {
            get => _totalRecipes;
            set => SetProperty(ref _totalRecipes, value);
        }

        public string MostUsedIngredient
        {
            get => _mostUsedIngredient;
            set => SetProperty(ref _mostUsedIngredient, value);
        }

        public double AverageCookTime
        {
            get => _averageCookTime;
            set => SetProperty(ref _averageCookTime, value);
        }

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

        public object[] RecipeTrendSeries
        {
            get => _recipeTrendSeries;
            set => SetProperty(ref _recipeTrendSeries, value);
        }

        public object[] IngredientUsageSeries
        {
            get => _ingredientUsageSeries;
            set => SetProperty(ref _ingredientUsageSeries, value);
        }

        public object[] AIPerformanceSeries
        {
            get => _aiPerformanceSeries;
            set => SetProperty(ref _aiPerformanceSeries, value);
        }

        public object[] CookTimeDistributionSeries
        {
            get => _cookTimeDistributionSeries;
            set => SetProperty(ref _cookTimeDistributionSeries, value);
        }

        public object[] RecipeDistributionSeries
        {
            get => _recipeDistributionSeries;
            set => SetProperty(ref _recipeDistributionSeries, value);
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
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
            await _dbContextSemaphore.WaitAsync();
            try
            {
                IsLoading = true;
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
                        MyAverageCookTime = Recipes.Average(r => r.CookTime);
                        MyMostCommonDifficulty = Recipes.GroupBy(r => r.Difficulty).OrderByDescending(g => g.Count()).First().Key;
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
                IsLoading = false;
                _dbContextSemaphore.Release();
            }
        }

        private async Task LoadIngredientsAsync()
        {
            await _dbContextSemaphore.WaitAsync();
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
                    }
                    else
                    {
                        StatusMessage = "No ingredients found in database. Please check FoodBook.sql data.";
                    }
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
                _dbContextSemaphore.Release();
            }
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
                var ingredientNames = Ingredients.Count > 0 
                    ? Ingredients.Take(3).Select(i => i.Name).ToList()
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
                IsLoading = true;
                StatusMessage = "Generating smart shopping list...";
                
                // Log feature usage
                await _loggingService.LogFeatureUsageAsync("Smart Shopping List", "1", 
                    $"Processing {Recipes.Count} recipes for shopping list generation");
                
                // Generate shopping list from available recipes or create sample recipes
                var selectedRecipes = Recipes.Count > 0 
                    ? Recipes.Take(3).ToList() 
                    : new List<Recipe> 
                    {
                        new Recipe { Id = 1, Title = "Sample Recipe 1", UserId = 1 },
                        new Recipe { Id = 2, Title = "Sample Recipe 2", UserId = 1 },
                        new Recipe { Id = 3, Title = "Sample Recipe 3", UserId = 1 }
                    };
                
                var shoppingList = await _shoppingListService.GenerateSmartShoppingListAsync(selectedRecipes, 1);
                
                // Optimize the shopping list
                var optimizedList = await _shoppingListService.OptimizeShoppingListAsync(shoppingList);
                
                // Log AI activity
                stopwatch.Stop();
                await _loggingService.LogAIActivityAsync("Smart Shopping List", "1", 
                    $"Recipes: {string.Join(", ", selectedRecipes.Select(r => r.Title))}", 
                    $"Generated {optimizedList.TotalItems} items, ${optimizedList.EstimatedCost:F2} cost", 
                    stopwatch.Elapsed);
                
                // Show shopping list dialog
                var shoppingDialog = new Views.ShoppingListDialog();
                shoppingDialog.SetShoppingList(optimizedList);
                shoppingDialog.ShowDialog();
                
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
                StatusMessage = $"Error generating shopping list: {ex.Message}";
                await _loggingService.LogErrorAsync("Smart Shopping List", "1", ex, "Unexpected error in shopping list generation");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AnalyzeNutritionAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                IsLoading = true;
                StatusMessage = "🍎 Analyzing nutrition from saved recipes...";
                
                // Log feature usage
                await _loggingService.LogFeatureUsageAsync("Database Nutrition Analysis", "1", 
                    $"Analyzing {Recipes.Count} saved recipes for comprehensive nutrition analysis");
                
                // Pha 1: Xác định luồng - Database Flow (Recipe đã lưu)
                var recipesToAnalyze = Recipes.Count > 0 
                    ? Recipes.ToList() 
                    : new List<Recipe> 
                    {
                        new Recipe { Id = 1, Title = "Sample Healthy Recipe", UserId = 1 },
                        new Recipe { Id = 2, Title = "Sample Balanced Meal", UserId = 1 }
                    };
                
                // Pha 2: Tính toán dinh dưỡng từ CSDL
                StatusMessage = "📊 Calculating nutrition from database...";
                var nutritionAnalysis = await _nutritionService.AnalyzeMealPlanNutritionAsync(recipesToAnalyze);
                
                // Get health alerts
                var healthAlerts = await _nutritionService.GetHealthAlertsAsync(nutritionAnalysis);
                
                // Get recommendations for general health
                var recommendations = await _nutritionService.GetNutritionRecommendationsAsync(nutritionAnalysis, "General Health");
                
                // Pha 3: AI-powered health assessment
                StatusMessage = "🤖 AI is analyzing your nutrition data...";
                
                // Log AI activity
                stopwatch.Stop();
                await _loggingService.LogAIActivityAsync("Database Nutrition Analysis", "1", 
                    $"Recipes: {string.Join(", ", Recipes.Select(r => r.Title))}", 
                    $"Analysis: {nutritionAnalysis.TotalCalories:F0} cal, Grade: {nutritionAnalysis.Rating.Grade}", 
                    stopwatch.Elapsed);
                
                // Show nutrition analysis dialog
                var nutritionDialog = new Views.NutritionAnalysisDialog();
                nutritionDialog.SetNutritionAnalysis(nutritionAnalysis, healthAlerts, new[] { recommendations });
                nutritionDialog.ShowDialog();
                
                StatusMessage = $"🍎 Database nutrition analysis complete! " +
                              $"Total: {nutritionAnalysis.TotalCalories:F0} calories, " +
                              $"{nutritionAnalysis.TotalProtein:F1}g protein, " +
                              $"{nutritionAnalysis.TotalCarbs:F1}g carbs, " +
                              $"{nutritionAnalysis.TotalFat:F1}g fat. " +
                              $"Grade: {nutritionAnalysis.Rating.Grade} ({nutritionAnalysis.Rating.OverallScore}/100). " +
                              $"{healthAlerts.Count()} health alerts found.";
                
                // Log performance
                await _loggingService.LogPerformanceAsync("Database Nutrition Analysis", "1", stopwatch.Elapsed, 
                    $"Analysis completed: {nutritionAnalysis.TotalCalories:F0} calories, Grade {nutritionAnalysis.Rating.Grade}, {healthAlerts.Count()} alerts");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error analyzing nutrition: {ex.Message}";
                await _loggingService.LogErrorAsync("Database Nutrition Analysis", "1", ex, "Unexpected error in database nutrition analysis");
            }
            finally
            {
                IsLoading = false;
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
                StatusMessage = "🤖 AI is parsing your custom recipe text...";
                
                // Log feature usage
                await _loggingService.LogFeatureUsageAsync("AI Parsing Nutrition Analysis", "1", 
                    $"Analyzing custom recipe text: {CustomRecipeText.Length} characters");
                
                // Pha 1: Xác định luồng - AI Parsing Flow (Văn bản tùy ý)
                StatusMessage = "🧠 AI is extracting ingredients from your text...";
                
                // Pha 2: AI Parsing - Giải mã văn bản tùy ý
                var nutritionAnalysis = await _nutritionService.AnalyzeUnstructuredRecipeAsync(CustomRecipeText);
                
                // Get health alerts
                var healthAlerts = await _nutritionService.GetHealthAlertsAsync(nutritionAnalysis);
                
                // Get recommendations for general health
                var recommendations = await _nutritionService.GetNutritionRecommendationsAsync(nutritionAnalysis, "General Health");
                
                // Pha 3: AI-powered health assessment
                StatusMessage = "🤖 AI nutritionist is analyzing your data...";
                
                // Log AI activity
                stopwatch.Stop();
                await _loggingService.LogAIActivityAsync("AI Parsing Nutrition Analysis", "1", 
                    $"Custom recipe: {CustomRecipeText.Substring(0, Math.Min(50, CustomRecipeText.Length))}...", 
                    $"Analysis: {nutritionAnalysis.TotalCalories:F0} cal, Grade: {nutritionAnalysis.Rating.Grade}", 
                    stopwatch.Elapsed);
                
                // Show custom nutrition analysis dialog
                var customNutritionDialog = new Views.CustomNutritionDialog();
                customNutritionDialog.SetNutritionAnalysis(nutritionAnalysis, healthAlerts, new[] { recommendations });
                customNutritionDialog.ShowDialog();
                
                StatusMessage = $"🤖 AI-powered nutrition analysis complete! " +
                              $"Total: {nutritionAnalysis.TotalCalories:F0} calories, " +
                              $"{nutritionAnalysis.TotalProtein:F1}g protein, " +
                              $"{nutritionAnalysis.TotalCarbs:F1}g carbs, " +
                              $"{nutritionAnalysis.TotalFat:F1}g fat. " +
                              $"Grade: {nutritionAnalysis.Rating.Grade} ({nutritionAnalysis.Rating.OverallScore}/100). " +
                              $"{healthAlerts.Count()} health alerts found.";
                
                // Log performance
                await _loggingService.LogPerformanceAsync("AI Parsing Nutrition Analysis", "1", stopwatch.Elapsed, 
                    $"Analysis completed: {nutritionAnalysis.TotalCalories:F0} calories, Grade {nutritionAnalysis.Rating.Grade}, {healthAlerts.Count()} alerts");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error analyzing custom nutrition: {ex.Message}";
                await _loggingService.LogErrorAsync("AI Parsing Nutrition Analysis", "1", ex, "Unexpected error in AI parsing nutrition analysis");
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
                SelectedTheme = settings.Theme;
                SelectedLanguage = settings.Language;
                NotificationsEnabled = settings.NotificationsEnabled;
                DefaultServings = settings.DefaultServings;
                
                StatusMessage = "Settings loaded successfully";
            }
            catch (Exception ex)
            {
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

        private async Task LoadAnalyticsDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading analytics data...";

                // Load basic statistics from database
                var recipes = await _recipeService.GetAllRecipesAsync();
                var ingredients = await _ingredientService.GetUserIngredientsAsync(1); // Use user 1 for demo
                var logs = await _loggingService.GetLogsAsync();

                // Update basic stats with real data
                TotalRecipes = recipes?.Count() ?? 0;
                
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
                    AverageCookTime = Math.Round(recipes.Average(r => r.CookTime), 1);
                }
                else
                {
                    AverageCookTime = 0;
                }

                // AI statistics from logs
                var aiLogs = logs?.Where(l => l.FeatureName?.Contains("AI") == true || l.FeatureName?.Contains("Generate") == true || l.FeatureName?.Contains("Judge") == true).ToList() ?? new List<Foodbook.Data.Entities.LogEntry>();
                AIJudgments = aiLogs.Count(l => l.FeatureName?.Contains("Judge") == true);
                GeneratedRecipes = aiLogs.Count(l => l.FeatureName?.Contains("Generate") == true);
                
                var successfulOperations = aiLogs.Count(l => !l.Message.Contains("Error") && !l.Message.Contains("Failed"));
                SuccessRate = aiLogs.Any() ? Math.Round((double)successfulOperations / aiLogs.Count * 100, 1) : 0;

                // Create chart data with real database data
                await CreateChartDataAsync(recipes, ingredients, logs);

                StatusMessage = $"Analytics loaded: {TotalRecipes} recipes, {ingredients?.Count() ?? 0} ingredients";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading analytics: {ex.Message}";
                // Set fallback values
                TotalRecipes = 0;
                MostUsedIngredient = "None";
                AverageCookTime = 0;
                AIJudgments = 0;
                GeneratedRecipes = 0;
                SuccessRate = 0;
                await CreateDemoChartDataAsync();
            }
            finally
            {
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

            RecipeTrendSeries = new object[]
            {
                new
                {
                    Values = recipeTrendData,
                    Name = "Recipes Created"
                }
            };

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

                    IngredientUsageSeries = new object[]
                    {
                        new
                        {
                            Values = ingredientValues,
                            Name = "Usage Count"
                        }
                    };
                }
                else
                {
                    // Show empty chart when no real ingredients - real-time data
                    IngredientUsageSeries = new object[]
                    {
                        new
                        {
                            Values = new double[] { 0 },
                            Name = "No Ingredients"
                        }
                    };
                }
            }
            else
            {
                // Show empty chart when no ingredients available - real-time data
                IngredientUsageSeries = new object[]
                {
                    new
                    {
                        Values = new double[] { 0 },
                        Name = "No Ingredients"
                    }
                };
            }

            // AI Performance pie chart - using real log data
            var aiSuccessCount = logs?.Count(l => l.FeatureName?.Contains("AI") == true && !l.Message.Contains("Error")) ?? 0;
            var aiErrorCount = logs?.Count(l => l.FeatureName?.Contains("AI") == true && l.Message.Contains("Error")) ?? 0;

            // Use real AI performance data - if no AI operations, show 0
            // This gives accurate real-time statistics

            AIPerformanceSeries = new object[]
            {
                new
                {
                    Values = new double[] { aiSuccessCount, aiErrorCount },
                    Name = "AI Performance"
                }
            };

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
                RecipeDistributionSeries = new object[]
                {
                    new
                    {
                        Values = categoryValues,
                        Name = "Recipe Distribution"
                    }
                };
            }
            else
            {
                // Show empty chart when no recipes available
                RecipeDistributionSeries = new object[]
                {
                    new
                    {
                        Values = new double[] { 0 },
                        Name = "No Recipes"
                    }
                };
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

                CookTimeDistributionSeries = new object[]
                {
                    new
                    {
                        Values = cookTimeValues,
                        Name = "Cook Time Distribution"
                    }
                };
            }
            else
            {
                // Show empty chart when no recipes available - real-time data
                CookTimeDistributionSeries = new object[]
                {
                    new
                    {
                        Values = new double[] { 0 },
                        Name = "No Recipes"
                    }
                };
            }
            
            return Task.CompletedTask;
        }

        private void InitializeCharts()
        {
            // Initialize charts with empty data to ensure they display
            RecipeTrendSeries = new object[]
            {
                new
                {
                    Values = new double[] { 0 },
                    Name = "Recipes Created"
                }
            };

            IngredientUsageSeries = new object[]
            {
                new
                {
                    Values = new double[] { 0 },
                    Name = "Usage Count"        
                }
            };

            AIPerformanceSeries = new object[]
            {
                new
                {
                    Values = new double[] { 0 },
                    Name = "AI Performance"
                }
            };

            CookTimeDistributionSeries = new object[]
            {
                new
                {
                    Values = new double[] { 0 },
                    Name = "Cook Time Distribution"
                }
            };

            RecipeDistributionSeries = new object[]
            {
                new
                {
                    Values = new double[] { 0 },
                    Name = "Recipe Distribution"
                }
            };
        }

        private Task CreateDemoChartDataAsync()
        {
            // Demo data for when no real data is available
            RecipeTrendSeries = new object[]
            {
                new
                {
                    Values = new double[] { 2, 1, 3, 2, 4, 3, 5 },
                    Name = "Recipes Created"
                }
            };

            IngredientUsageSeries = new object[]
            {
                new
                {
                    Values = new double[] { 8, 6, 5, 4, 3 },
                    Name = "Usage Count"
                }
            };

            AIPerformanceSeries = new object[]
            {
                new
                {
                    Values = new double[] { 85, 15 },
                    Name = "AI Performance"
                }
            };

            CookTimeDistributionSeries = new object[]
            {
                new
                {
                    Values = new double[] { 2, 5, 3, 2, 1 },
                    Name = "Cook Time Distribution"
                }
            };
            
            return Task.CompletedTask;
        }

        public async Task LoadCurrentUserAsync()
        {
            try
            {
                if (_authenticationService != null)
                {
                    CurrentUser = await _authenticationService.GetCurrentUserAsync();
                    StatusMessage = CurrentUser != null ? $"Welcome back, {CurrentUser.Username}!" : "Please log in to continue";
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
                StatusMessage = "Guest mode - Welcome, Guest!";
                System.Diagnostics.Debug.WriteLine($"Error loading current user: {ex.Message}");
            }
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

        private async Task ViewLogsAsync()
        {
            try
            {
                StatusMessage = "Opening log viewer...";
                var logViewer = new Views.LogViewerWindow(_loggingService);
                logViewer.ShowDialog();
                StatusMessage = "Log viewer opened";
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening log viewer: {ex.Message}";
            }
        }

        private async Task ToggleSidebarAsync()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            StatusMessage = IsSidebarCollapsed ? "Sidebar collapsed" : "Sidebar expanded";
            await Task.CompletedTask;
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
