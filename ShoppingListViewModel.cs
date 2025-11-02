using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Commands;
using Foodbook.Presentation.Views;
using System.Windows;

namespace Foodbook.Presentation.ViewModels
{
    // Wrapper class for Recipe with selection support
    public class RecipeSelectionItem : BaseViewModel
    {
        private bool _isSelected;
        private Recipe _recipe;

        public RecipeSelectionItem(Recipe recipe)
        {
            _recipe = recipe;
        }

        public Recipe Recipe => _recipe;
        
        public int Id => _recipe.Id;
        public string Title => _recipe.Title ?? "Unknown Recipe";
        public string Description => _recipe.Description ?? "";
        public int Servings => _recipe.Servings;
        public int CookTime => _recipe.CookTime;
        public string Difficulty => _recipe.Difficulty ?? "";
        public string Category => _recipe.Category ?? "";
        public string ImageUrl => _recipe.ImageUrl ?? "";

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class ShoppingListViewModel : BaseViewModel
    {
        private readonly IShoppingListService _shoppingListService;
        private readonly IRecipeService _recipeService;
        private readonly IUserService _userService;

        private ObservableCollection<RecipeSelectionItem> _availableRecipes = new();
        private ObservableCollection<Recipe> _selectedRecipes = new();
        private ShoppingListResult? _currentShoppingList;
        private bool _isGenerating = false;
        private string _generationStatus = string.Empty;
        private int _currentUserId = 1; // Default user ID

        public ShoppingListViewModel(
            IShoppingListService shoppingListService,
            IRecipeService recipeService,
            IUserService userService)
        {
            _shoppingListService = shoppingListService;
            _recipeService = recipeService;
            _userService = userService;

            GenerateShoppingListCommand = new RelayCommand(async () => await GenerateShoppingListAsync(), CanGenerateShoppingList);
            ExportShoppingListCommand = new RelayCommand(() => ExportShoppingListAsync(), CanExportShoppingList);
            AddRecipeCommand = new RelayCommand<Recipe>(AddRecipe);
            RemoveRecipeCommand = new RelayCommand<Recipe>(RemoveRecipe);
            ClearSelectionCommand = new RelayCommand(async () => await ClearSelectionAsync());
            OptimizeListCommand = new RelayCommand(async () => await OptimizeShoppingListAsync(), CanOptimizeList);
            ViewDetailedListCommand = new RelayCommand(() => ViewDetailedListAsync(), CanViewDetailedList);

            _ = LoadRecipesAsync();
        }

        #region Properties

        public ObservableCollection<RecipeSelectionItem> AvailableRecipes
        {
            get => _availableRecipes;
            set => SetProperty(ref _availableRecipes, value);
        }

        public ObservableCollection<Recipe> SelectedRecipes
        {
            get => _selectedRecipes;
            set => SetProperty(ref _selectedRecipes, value);
        }

        public ShoppingListResult? CurrentShoppingList
        {
            get => _currentShoppingList;
            set => SetProperty(ref _currentShoppingList, value);
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set => SetProperty(ref _isGenerating, value);
        }

        public string GenerationStatus
        {
            get => _generationStatus;
            set => SetProperty(ref _generationStatus, value);
        }

        public int CurrentUserId
        {
            get => _currentUserId;
            set => SetProperty(ref _currentUserId, value);
        }

        #endregion

        #region Commands

        public ICommand GenerateShoppingListCommand { get; }
        public ICommand ExportShoppingListCommand { get; }
        public ICommand AddRecipeCommand { get; }
        public ICommand RemoveRecipeCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand OptimizeListCommand { get; }
        public ICommand ViewDetailedListCommand { get; }

        #endregion

        #region Methods

        private async Task LoadRecipesAsync()
        {
            try
            {
                var recipes = await _recipeService.GetAllRecipesAsync();
                AvailableRecipes.Clear();
                foreach (var recipe in recipes)
                {
                    var recipeItem = new RecipeSelectionItem(recipe);
                    recipeItem.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(RecipeSelectionItem.IsSelected))
                        {
                            UpdateSelectedRecipes();
                        }
                    };
                    AvailableRecipes.Add(recipeItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recipes: {ex.Message}");
            }
        }

        private void UpdateSelectedRecipes()
        {
            SelectedRecipes.Clear();
            foreach (var recipeItem in AvailableRecipes.Where(r => r.IsSelected))
            {
                SelectedRecipes.Add(recipeItem.Recipe);
            }
            OnPropertyChanged(nameof(SelectedRecipes));
        }

        private async Task GenerateShoppingListAsync()
        {
            if (!SelectedRecipes.Any())
            {
                GenerationStatus = "Please select at least one recipe";
                return;
            }

            IsGenerating = true;
            GenerationStatus = "🤖 AI is analyzing recipes and generating smart shopping list...";

            try
            {
                var shoppingList = await _shoppingListService.GenerateSmartShoppingListAsync(SelectedRecipes, CurrentUserId);
                CurrentShoppingList = shoppingList;
                GenerationStatus = $"✅ Generated shopping list with {shoppingList.TotalItems} items";
                
                // Automatically open the detailed shopping list dialog
                await ViewDetailedListAsync();
            }
            catch (Exception ex)
            {
                GenerationStatus = $"❌ Error generating shopping list: {ex.Message}";
                Console.WriteLine($"Error generating shopping list: {ex.Message}");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private Task ExportShoppingListAsync()
        {
            if (CurrentShoppingList == null) return Task.CompletedTask;

            try
            {
                var dialog = new ShoppingListDialog(_shoppingListService);
                dialog.SetShoppingList(CurrentShoppingList);
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening export dialog: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private async Task OptimizeShoppingListAsync()
        {
            if (CurrentShoppingList == null) return;

            IsGenerating = true;
            GenerationStatus = "🔧 Optimizing shopping list...";

            try
            {
                var optimizedList = await _shoppingListService.OptimizeShoppingListAsync(CurrentShoppingList);
                CurrentShoppingList = optimizedList;
                GenerationStatus = $"✅ List optimized! Potential savings: ${optimizedList.PotentialSavings:F2}";
            }
            catch (Exception ex)
            {
                GenerationStatus = $"❌ Error optimizing list: {ex.Message}";
                Console.WriteLine($"Error optimizing shopping list: {ex.Message}");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private void AddRecipe(Recipe recipe)
        {
            if (recipe != null && !SelectedRecipes.Contains(recipe))
            {
                SelectedRecipes.Add(recipe);
                OnPropertyChanged(nameof(SelectedRecipes));
            }
        }

        private void RemoveRecipe(Recipe recipe)
        {
            if (recipe != null && SelectedRecipes.Contains(recipe))
            {
                SelectedRecipes.Remove(recipe);
                OnPropertyChanged(nameof(SelectedRecipes));
            }
        }

        private void ClearSelection()
        {
            // Clear selection in AvailableRecipes
            foreach (var recipeItem in AvailableRecipes)
            {
                recipeItem.IsSelected = false;
            }
            SelectedRecipes.Clear();
            CurrentShoppingList = null;
            GenerationStatus = string.Empty;
        }

        private async Task ClearSelectionAsync()
        {
            await Task.Run(() =>
            {
                // Clear selection in AvailableRecipes
                foreach (var recipeItem in AvailableRecipes)
                {
                    recipeItem.IsSelected = false;
                }
                SelectedRecipes.Clear();
                CurrentShoppingList = null;
                GenerationStatus = string.Empty;
            });
        }

        private Task ViewDetailedListAsync()
        {
            try
            {
                if (CurrentShoppingList == null)
                {
                    MessageBox.Show("No shopping list available to view.", "No Shopping List", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return Task.CompletedTask;
                }

                // Create and show the detailed shopping list dialog
                var dialog = new ShoppingListDialog(_shoppingListService);
                dialog.SetShoppingList(CurrentShoppingList);
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening detailed shopping list: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Command CanExecute Methods

        private bool CanGenerateShoppingList()
        {
            return !IsGenerating && SelectedRecipes.Any();
        }

        private bool CanExportShoppingList()
        {
            return !IsGenerating && CurrentShoppingList != null;
        }

        private bool CanOptimizeList()
        {
            return !IsGenerating && CurrentShoppingList != null && !CurrentShoppingList.IsOptimized;
        }

        private bool CanViewDetailedList()
        {
            return !IsGenerating && CurrentShoppingList != null;
        }

        #endregion

        #region Public Methods for Integration

        public async Task GenerateShoppingListFromMealPlanAsync(IEnumerable<MealPlanItem> mealPlanItems)
        {
            if (!mealPlanItems.Any()) return;

            IsGenerating = true;
            GenerationStatus = "📅 Generating shopping list from meal plan...";

            try
            {
                var shoppingList = await _shoppingListService.GenerateShoppingListFromMealPlanAsync(mealPlanItems, CurrentUserId);
                CurrentShoppingList = shoppingList;
                GenerationStatus = $"✅ Generated meal plan shopping list with {shoppingList.TotalItems} items";
            }
            catch (Exception ex)
            {
                GenerationStatus = $"❌ Error generating meal plan shopping list: {ex.Message}";
                Console.WriteLine($"Error generating meal plan shopping list: {ex.Message}");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        public async Task GenerateShoppingListFromIngredientsAsync(IEnumerable<string> ingredientNames)
        {
            if (!ingredientNames.Any()) return;

            IsGenerating = true;
            GenerationStatus = "🥘 Generating shopping list from ingredients...";

            try
            {
                var shoppingList = await _shoppingListService.GenerateShoppingListFromIngredientsAsync(ingredientNames, CurrentUserId);
                CurrentShoppingList = shoppingList;
                GenerationStatus = $"✅ Generated ingredient shopping list with {shoppingList.TotalItems} items";
            }
            catch (Exception ex)
            {
                GenerationStatus = $"❌ Error generating ingredient shopping list: {ex.Message}";
                Console.WriteLine($"Error generating ingredient shopping list: {ex.Message}");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        public void SetCurrentUser(int userId)
        {
            CurrentUserId = userId;
        }

        #endregion
    }
}
