using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Services;

namespace Foodbook.Presentation.ViewModels
{
    public class DataManagementViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private readonly IIngredientService _ingredientService;
        private readonly JsonService _jsonService;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;

        public DataManagementViewModel(
            IRecipeService recipeService,
            IIngredientService ingredientService,
            JsonService jsonService)
        {
            _recipeService = recipeService;
            _ingredientService = ingredientService;
            _jsonService = jsonService;

            ExportRecipesCommand = new RelayCommand(async () => await ExportRecipesAsync());
            ImportRecipesCommand = new RelayCommand(async () => await ImportRecipesAsync());
            ExportIngredientsCommand = new RelayCommand(async () => await ExportIngredientsAsync());
            ImportIngredientsCommand = new RelayCommand(async () => await ImportIngredientsAsync());
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ICommand ExportRecipesCommand { get; }
        public ICommand ImportRecipesCommand { get; }
        public ICommand ExportIngredientsCommand { get; }
        public ICommand ImportIngredientsCommand { get; }

        private async Task ExportRecipesAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Exporting recipes...";

                var recipes = await _recipeService.GetAllRecipesAsync();
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Recipes",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"recipes_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var success = await _jsonService.ExportRecipesToJsonAsync(recipes, saveFileDialog.FileName);
                    StatusMessage = success ? "Recipes exported successfully!" : "Failed to export recipes.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting recipes: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ImportRecipesAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Importing recipes...";

                var openFileDialog = new OpenFileDialog
                {
                    Title = "Import Recipes",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var recipes = await _jsonService.ImportRecipesFromJsonAsync(openFileDialog.FileName);
                    if (recipes != null)
                    {
                        int count = 0;
                        foreach (var recipe in recipes)
                        {
                            recipe.UserId = 1; // Demo user
                            await _recipeService.CreateRecipeAsync(recipe);
                            count++;
                        }
                        StatusMessage = $"Successfully imported {count} recipes!";
                    }
                    else
                    {
                        StatusMessage = "Failed to import recipes. Invalid file format.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error importing recipes: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ExportIngredientsAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Exporting ingredients...";

                var ingredients = await _ingredientService.GetUserIngredientsAsync(1);
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Ingredients",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"ingredients_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var success = await _jsonService.ExportIngredientsToJsonAsync(ingredients, saveFileDialog.FileName);
                    StatusMessage = success ? "Ingredients exported successfully!" : "Failed to export ingredients.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting ingredients: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ImportIngredientsAsync()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Importing ingredients...";

                var openFileDialog = new OpenFileDialog
                {
                    Title = "Import Ingredients",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var ingredients = await _jsonService.ImportIngredientsFromJsonAsync(openFileDialog.FileName);
                    if (ingredients != null)
                    {
                        int count = 0;
                        foreach (var ingredient in ingredients)
                        {
                            ingredient.UserId = 1; // Demo user
                            await _ingredientService.AddIngredientAsync(ingredient);
                            count++;
                        }
                        StatusMessage = $"Successfully imported {count} ingredients!";
                    }
                    else
                    {
                        StatusMessage = "Failed to import ingredients. Invalid file format.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error importing ingredients: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
