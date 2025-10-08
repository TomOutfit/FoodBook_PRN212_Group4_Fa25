using System.Collections.ObjectModel;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
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
        private readonly IAuthenticationService _authenticationService;
        public ILoggingService LoggingService => _loggingService;

        private ObservableCollection<Recipe> _recipes = new();
        private ObservableCollection<Ingredient> _ingredients = new();
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

        // Analytics properties
        private int _totalRecipes = 0;
        private string _mostUsedIngredient = "None";
        private double _averageCookTime = 0;
        private int _aiJudgments = 0;
        private int _generatedRecipes = 0;
        private double _successRate = 0;
        private ISeries[] _recipeTrendSeries = Array.Empty<ISeries>();
        private ISeries[] _ingredientUsageSeries = Array.Empty<ISeries>();
        private ISeries[] _aiPerformanceSeries = Array.Empty<ISeries>();
        private ISeries[] _cookTimeDistributionSeries = Array.Empty<ISeries>();
        private User? _currentUser;

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
            DeleteRecipeCommand = new RelayCommand(async () => await DeleteRecipeAsync());
            AdjustServingsCommand = new RelayCommand(async () => await AdjustServingsAsync());
            GenerateRecipeCommand = new RelayCommand(async () => await GenerateRecipeAsync());
            JudgeDishCommand = new RelayCommand(async () => await JudgeDishAsync());
            GenerateShoppingListCommand = new RelayCommand(async () => await GenerateShoppingListAsync());
            AnalyzeNutritionCommand = new RelayCommand(async () => await AnalyzeNutritionAsync());
            SelectTabCommand = new RelayCommand<string>(SelectTab);
            AddNewRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            AddNewIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            EditRecipeCommand = new RelayCommand(async () => await EditRecipeAsync(SelectedRecipe!));
            EditIngredientCommand = new RelayCommand(async () => await EditIngredientAsync(null!));
            DeleteIngredientCommand = new RelayCommand(async () => await DeleteIngredientAsync(null!));
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
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
            
            // Initialize all commands
            LoadRecipesCommand = new RelayCommand(async () => await LoadRecipesAsync());
            SearchRecipesCommand = new RelayCommand(async () => await SearchRecipesAsync());
            LoadIngredientsCommand = new RelayCommand(async () => await LoadIngredientsAsync());
            DeleteRecipeCommand = new RelayCommand(async () => await DeleteRecipeAsync());
            AdjustServingsCommand = new RelayCommand(async () => await AdjustServingsAsync());
            GenerateRecipeCommand = new RelayCommand(async () => await GenerateRecipeAsync());
            JudgeDishCommand = new RelayCommand(async () => await JudgeDishAsync());
            GenerateShoppingListCommand = new RelayCommand(async () => await GenerateShoppingListAsync());
            AnalyzeNutritionCommand = new RelayCommand(async () => await AnalyzeNutritionAsync());
            SelectTabCommand = new RelayCommand<string>(SelectTab);
            AddNewRecipeCommand = new RelayCommand(async () => await AddNewRecipeAsync());
            AddNewIngredientCommand = new RelayCommand(async () => await AddNewIngredientAsync());
            EditRecipeCommand = new RelayCommand<Recipe>(async (recipe) => await EditRecipeAsync(recipe));
            EditIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await EditIngredientAsync(ingredient));
            DeleteIngredientCommand = new RelayCommand<Ingredient>(async (ingredient) => await DeleteIngredientAsync(ingredient));
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
            
            // Load current user
            _ = LoadCurrentUserAsync();
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
        public ICommand SelectTabCommand { get; }
        public ICommand AddNewRecipeCommand { get; }
        public ICommand AddNewIngredientCommand { get; }
        public ICommand EditRecipeCommand { get; }
        public ICommand EditIngredientCommand { get; }
        public ICommand DeleteIngredientCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }
        
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

        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        private async Task LoadRecipesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading recipes...";
                
                var recipes = await _recipeService.GetAllRecipesAsync();
                Recipes.Clear();
                
                if (recipes != null && recipes.Any())
                {
                    foreach (var recipe in recipes)
                    {
                        Recipes.Add(recipe);
                    }
                    StatusMessage = $"Loaded {Recipes.Count} recipes";
                }
                else
                {
                    StatusMessage = "No recipes found. Click 'Add New Recipe' to get started!";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading recipes: {ex.Message}";
                // Add some sample data for demo
                if (Recipes.Count == 0)
                {
                    var sampleRecipe = new Recipe
                    {
                        Id = 1,
                        Title = "Sample Recipe",
                        Description = "This is a sample recipe for demonstration",
                        Instructions = "1. Prepare ingredients\n2. Cook according to instructions\n3. Serve hot",
                        CookTime = 30,
                        Difficulty = "Easy",
                        UserId = 1
                    };
                    Recipes.Add(sampleRecipe);
                    StatusMessage = "Loaded sample recipe (database connection issue)";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchRecipesAsync()
        {
            try
            {
                IsLoading = true;
                
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    // If search text is empty, load all recipes
                    await LoadRecipesAsync();
                    return;
                }
                
                StatusMessage = "Searching recipes...";
                
                var recipes = await _recipeService.SearchRecipesAsync(SearchText, null, null, null);
                Recipes.Clear();
                foreach (var recipe in recipes)
                {
                    Recipes.Add(recipe);
                }
                
                StatusMessage = $"Found {Recipes.Count} recipes for '{SearchText}'";
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

        private async Task LoadIngredientsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading ingredients...";
                
                // For demo purposes, using user ID 1
                var ingredients = await _ingredientService.GetUserIngredientsAsync(1);
                Ingredients.Clear();
                
                if (ingredients != null && ingredients.Any())
                {
                    foreach (var ingredient in ingredients)
                    {
                        Ingredients.Add(ingredient);
                    }
                    StatusMessage = $"Loaded {Ingredients.Count} ingredients";
                }
                else
                {
                    StatusMessage = "No ingredients found. Click 'Add New Ingredient' to get started!";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading ingredients: {ex.Message}";
                // Add some sample data for demo
                if (Ingredients.Count == 0)
                {
                    var sampleIngredients = new[]
                    {
                        new Ingredient { Id = 1, Name = "Rice", Unit = "cup", Quantity = 2, UserId = 1 },
                        new Ingredient { Id = 2, Name = "Chicken", Unit = "gram", Quantity = 500, UserId = 1 },
                        new Ingredient { Id = 3, Name = "Onion", Unit = "piece", Quantity = 1, UserId = 1 }
                    };
                    
                    foreach (var ingredient in sampleIngredients)
                    {
                        Ingredients.Add(ingredient);
                    }
                    StatusMessage = "Loaded sample ingredients (database connection issue)";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            try
            {
                var dialog = new Views.ConfirmationDialog(
                    $"Are you sure you want to delete the recipe '{SelectedRecipe.Title}'?\n\nThis action cannot be undone.", 
                    "Delete Recipe");
                
                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Deleting recipe...";
                    
                    var success = await _recipeService.DeleteRecipeAsync(SelectedRecipe.Id);
                    if (success)
                    {
                        Recipes.Remove(SelectedRecipe);
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
                    generatedRecipe?.Title ?? "Failed", 
                    stopwatch.Elapsed);
                
                if (generatedRecipe != null)
                {
                    generatedRecipe.UserId = 1; // Demo user
                    var createdRecipe = await _recipeService.CreateRecipeAsync(generatedRecipe);
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
                    
                    var result = await _aiService.JudgeDishAsync(imageDialog.ImageData);
                    
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

        private async void SelectTab(string tabName)
        {
            try
            {
                SelectedTab = tabName;
                StatusMessage = $"Switched to {tabName} tab";
                
                // Load data based on selected tab
                switch (tabName)
                {
                    case "Dashboard":
                        await LoadRecipesAsync();
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
                        // Settings don't need data loading
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
                var dialog = new Views.RecipeDialog();
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
                var dialog = new Views.IngredientDialog();
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
                var dialog = new Views.RecipeDialog(recipe);
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

        private async Task EditIngredientAsync(Ingredient ingredient)
        {
            if (ingredient == null) return;

            try
            {
                var dialog = new Views.IngredientDialog(ingredient);
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
                StatusMessage = "Analyzing nutrition...";
                
                // Log feature usage
                await _loggingService.LogFeatureUsageAsync("Nutrition Analysis", "1", 
                    $"Analyzing {Recipes.Count} recipes for comprehensive nutrition analysis");
                
                // Analyze nutrition for all recipes or create sample recipes
                var recipesToAnalyze = Recipes.Count > 0 
                    ? Recipes.ToList() 
                    : new List<Recipe> 
                    {
                        new Recipe { Id = 1, Title = "Sample Healthy Recipe", UserId = 1 },
                        new Recipe { Id = 2, Title = "Sample Balanced Meal", UserId = 1 }
                    };
                
                var nutritionAnalysis = await _nutritionService.AnalyzeMealPlanNutritionAsync(recipesToAnalyze);
                
                // Get health alerts
                var healthAlerts = await _nutritionService.GetHealthAlertsAsync(nutritionAnalysis);
                
                // Get recommendations for general health
                var recommendations = await _nutritionService.GetNutritionRecommendationsAsync(nutritionAnalysis, "General Health");
                
                // Log AI activity
                stopwatch.Stop();
                await _loggingService.LogAIActivityAsync("Nutrition Analysis", "1", 
                    $"Recipes: {string.Join(", ", Recipes.Select(r => r.Title))}", 
                    $"Analysis: {nutritionAnalysis.TotalCalories:F0} cal, Grade: {nutritionAnalysis.Rating.Grade}", 
                    stopwatch.Elapsed);
                
                // Show nutrition analysis dialog
                var nutritionDialog = new Views.NutritionAnalysisDialog();
                nutritionDialog.SetNutritionAnalysis(nutritionAnalysis, healthAlerts, new[] { recommendations });
                nutritionDialog.ShowDialog();
                
                StatusMessage = $"🍎 Nutrition analysis complete! " +
                              $"Total: {nutritionAnalysis.TotalCalories:F0} calories, " +
                              $"{nutritionAnalysis.TotalProtein:F1}g protein, " +
                              $"{nutritionAnalysis.TotalCarbs:F1}g carbs, " +
                              $"{nutritionAnalysis.TotalFat:F1}g fat. " +
                              $"Grade: {nutritionAnalysis.Rating.Grade} ({nutritionAnalysis.Rating.OverallScore}/100). " +
                              $"{healthAlerts.Count()} health alerts found.";
                
                // Log performance
                await _loggingService.LogPerformanceAsync("Nutrition Analysis", "1", stopwatch.Elapsed, 
                    $"Analysis completed: {nutritionAnalysis.TotalCalories:F0} calories, Grade {nutritionAnalysis.Rating.Grade}, {healthAlerts.Count()} alerts");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error analyzing nutrition: {ex.Message}";
                await _loggingService.LogErrorAsync("Nutrition Analysis", "1", ex, "Unexpected error in nutrition analysis");
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

                // Load basic statistics
                var recipes = await _recipeService.GetAllRecipesAsync();
                var ingredients = await _ingredientService.GetUserIngredientsAsync(1);
                var logs = await _loggingService.GetLogsAsync();

                // Update basic stats
                TotalRecipes = recipes?.Count() ?? 0;
                
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
                    AverageCookTime = recipes.Average(r => r.CookTime);
                }
                else
                {
                    AverageCookTime = 0;
                }

                // AI statistics
                var aiLogs = logs?.Where(l => l.FeatureName.Contains("AI") || l.FeatureName.Contains("Generate") || l.FeatureName.Contains("Judge")).ToList() ?? new List<Foodbook.Data.Entities.LogEntry>();
                AIJudgments = aiLogs.Count(l => l.FeatureName.Contains("Judge"));
                GeneratedRecipes = aiLogs.Count(l => l.FeatureName.Contains("Generate"));
                
                var successfulOperations = aiLogs.Count(l => !l.Message.Contains("Error") && !l.Message.Contains("Failed"));
                SuccessRate = aiLogs.Any() ? (double)successfulOperations / aiLogs.Count * 100 : 0;

                // Create chart data
                await CreateChartDataAsync(recipes, ingredients, logs);

                StatusMessage = "Analytics data loaded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading analytics: {ex.Message}";
                // Set default values for demo
                TotalRecipes = 5;
                MostUsedIngredient = "Chicken";
                AverageCookTime = 35.5;
                AIJudgments = 12;
                GeneratedRecipes = 8;
                SuccessRate = 85.5;
                await CreateDemoChartDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Task CreateChartDataAsync(IEnumerable<Recipe>? recipes, IEnumerable<Ingredient>? ingredients, IEnumerable<Foodbook.Data.Entities.LogEntry>? logs)
        {
            // Recipe trend chart (last 7 days)
            var recipeTrendData = new List<double>();
            var recipeLabels = new List<string>();
            
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                var dayRecipes = recipes?.Count(r => r.CreatedAt.Date == date.Date) ?? 0;
                recipeTrendData.Add(dayRecipes);
                recipeLabels.Add(date.ToString("MMM dd"));
            }

            RecipeTrendSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = recipeTrendData,
                    Name = "Recipes Created",
                    Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 3),
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(50))
                }
            };

            // Ingredient usage chart
            if (ingredients?.Any() == true)
            {
                var ingredientUsage = ingredients
                    .GroupBy(i => i.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                var ingredientValues = ingredientUsage.Select(x => (double)x.Count).ToArray();
                var ingredientNames = ingredientUsage.Select(x => x.Name).ToArray();

                IngredientUsageSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = ingredientValues,
                        Name = "Usage Count",
                        Fill = new SolidColorPaint(SKColor.Parse("#10B981"))
                    }
                };
            }
            else
            {
                IngredientUsageSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = new double[] { 0 },
                        Name = "No Data",
                        Fill = new SolidColorPaint(SKColor.Parse("#6B7280"))
                    }
                };
            }

            // AI Performance pie chart
            var aiSuccessCount = logs?.Count(l => l.FeatureName.Contains("AI") && !l.Message.Contains("Error")) ?? 0;
            var aiErrorCount = logs?.Count(l => l.FeatureName.Contains("AI") && l.Message.Contains("Error")) ?? 0;

            AIPerformanceSeries = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values = new double[] { aiSuccessCount, aiErrorCount },
                    Name = "AI Performance",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F0}"
                }
            };

            // Cook time distribution
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

                CookTimeDistributionSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = cookTimeValues,
                        Name = "Cook Time Distribution",
                        Fill = new SolidColorPaint(SKColor.Parse("#F59E0B"))
                    }
                };
            }
            else
            {
                CookTimeDistributionSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = new double[] { 0 },
                        Name = "No Data",
                        Fill = new SolidColorPaint(SKColor.Parse("#6B7280"))
                    }
                };
            }
            
            return Task.CompletedTask;
        }

        private Task CreateDemoChartDataAsync()
        {
            // Demo data for when no real data is available
            RecipeTrendSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 2, 1, 3, 2, 4, 3, 5 },
                    Name = "Recipes Created",
                    Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6"), 3),
                    Fill = new SolidColorPaint(SKColor.Parse("#3B82F6").WithAlpha(50))
                }
            };

            IngredientUsageSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = new double[] { 8, 6, 5, 4, 3 },
                    Name = "Usage Count",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981"))
                }
            };

            AIPerformanceSeries = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values = new double[] { 85, 15 },
                    Name = "AI Performance",
                    Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                    DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F0}%"
                }
            };

            CookTimeDistributionSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = new double[] { 2, 5, 3, 2, 1 },
                    Name = "Cook Time Distribution",
                    Fill = new SolidColorPaint(SKColor.Parse("#F59E0B"))
                }
            };
            
            return Task.CompletedTask;
        }

        public async Task LoadCurrentUserAsync()
        {
            try
            {
                CurrentUser = await _authenticationService.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                // Handle error silently for now
                System.Diagnostics.Debug.WriteLine($"Error loading current user: {ex.Message}");
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
