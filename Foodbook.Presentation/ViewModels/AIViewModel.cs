using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Business;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Commands;
using Foodbook.Presentation.Views;
using Foodbook.Presentation.ViewModels;

namespace Foodbook.Presentation.ViewModels
{
	public class AIViewModel : BaseViewModel
	{
		private readonly IAIService? _aiService;
		private readonly IShoppingListService? _shoppingListService;
		private readonly ILoggingService? _loggingService;
		private readonly IUserService? _userService;
		private readonly InventoryViewModel? _inventoryViewModel;
		private readonly IIngredientService? _ingredientService;
		private readonly IRecipeService? _recipeService;

		private bool _isBusy;
		public bool IsBusy
		{
			get => _isBusy;
			private set => SetProperty(ref _isBusy, value);
		}

		private string _selectedGenerationMode = "Random Generation List";
		public string SelectedGenerationMode
		{
			get => _selectedGenerationMode;
			set => SetProperty(ref _selectedGenerationMode, value);
		}

		public List<string> GenerationModes { get; } = new List<string>
		{
			"Random Generation List",
			"User List"
		};

		public ICommand JudgeDishCommand { get; }
		public ICommand GenerateRecipeCommand { get; }
		public ICommand GenerateShoppingListCommand { get; }
		public ICommand OpenNutritionAnalysisCommand { get; }

		public AIViewModel(
			IAIService aiService,
			IShoppingListService shoppingListService,
			ILoggingService loggingService,
			IUserService userService,
			InventoryViewModel inventoryViewModel,
			IIngredientService? ingredientService = null,
			IRecipeService? recipeService = null)
		{
			_aiService = aiService;
			_shoppingListService = shoppingListService;
			_loggingService = loggingService;
			_userService = userService;
			_inventoryViewModel = inventoryViewModel;
			_ingredientService = ingredientService;
			_recipeService = recipeService;

			JudgeDishCommand = new RelayCommand(async () => await JudgeDishAsync(), () => !IsBusy);
			GenerateRecipeCommand = new RelayCommand(async () => await GenerateRecipeAsync(), () => !IsBusy);
			GenerateShoppingListCommand = new RelayCommand(async () => await GenerateShoppingListAsync(), () => !IsBusy);
			OpenNutritionAnalysisCommand = new RelayCommand(async () => await OpenNutritionAnalysisAsync(), () => !IsBusy);
		}

		// Design-time constructor
		public AIViewModel()
		{
			JudgeDishCommand = new RelayCommand(async () => await Task.CompletedTask, () => true);
			GenerateRecipeCommand = new RelayCommand(async () => await Task.CompletedTask, () => true);
			GenerateShoppingListCommand = new RelayCommand(async () => await Task.CompletedTask, () => true);
			OpenNutritionAnalysisCommand = new RelayCommand(async () => await Task.CompletedTask, () => true);
		}

		public async Task LoadIngredientsForAIAsync()
		{
			try
			{
				if (_inventoryViewModel != null)
				{
					await _inventoryViewModel.LoadIngredientsAsync();
				}
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "LoadIngredientsForAI") ?? Task.CompletedTask);
			}
		}

		private async Task JudgeDishAsync()
		{
			try
			{
				IsBusy = true;
				// Open image upload dialog
				var uploadDialog = new ImageUploadDialog
				{
					Owner = Application.Current?.MainWindow
				};
				var uploadResult = uploadDialog.ShowDialog();
				if (uploadResult == true && uploadDialog.ImageData != null && _aiService != null)
				{
					// Call AI service to judge the dish
					var judge = await _aiService.JudgeDishAsync(uploadDialog.ImageData);
					// Show result dialog
					var resultDialog = new JudgeResultDialog
					{
						Owner = Application.Current?.MainWindow
					};
					resultDialog.SetJudgeResult(
						judge.Score,
						judge.OverallRating,
						judge.Comment,
						judge.PresentationScore,
						judge.ColorScore,
						judge.TextureScore,
						judge.PlatingScore,
						judge.HealthNotes,
						string.Join("\n", judge.ChefTips),
						string.Join("\n", judge.Suggestions)
					);
					resultDialog.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "JudgeDish") ?? Task.CompletedTask);
			}
			finally { IsBusy = false; }
		}

		private async Task GenerateRecipeAsync()
		{
			try
			{
				IsBusy = true;

				// Get current user ID
				int userId = 1; // Default fallback
				if (_userService != null)
				{
					// Try to get current user ID from service if available
					// userId = await _userService.GetCurrentUserIdAsync();
				}

				// Load available ingredients from pantry
				if (_inventoryViewModel == null || _ingredientService == null)
				{
					MessageBox.Show("Ingredient service is not available.", "Error", 
						MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				await _inventoryViewModel.LoadIngredientsAsync();
				var availableIngredients = _inventoryViewModel.Ingredients
					.Where(i => i.Quantity.HasValue && i.Quantity.Value > 0)
					.ToList();

				if (!availableIngredients.Any())
				{
					MessageBox.Show(
						"⚠️ Không có nguyên liệu trong pantry.\n\nVui lòng thêm nguyên liệu vào 'My Ingredient Pantry' trước khi tạo công thức.",
						"Không có nguyên liệu",
						MessageBoxButton.OK,
						MessageBoxImage.Warning);
					return;
				}

				// Open GenerateRecipeDialog
				var dialog = new GenerateRecipeDialog(availableIngredients)
				{
					Owner = Application.Current?.MainWindow
				};

				var dialogResult = dialog.ShowDialog();
				if (dialogResult != true)
				{
					return; // User cancelled
				}

				// Get user input
				var dishName = dialog.DishName;
				var servings = dialog.Servings;
				var customPreferences = dialog.CustomPreferences;

				// Get existing recipes for deduplication
				var existingRecipes = new List<Recipe>();
				if (_recipeService != null)
				{
					var allRecipes = await _recipeService.GetAllRecipesAsync();
					existingRecipes = allRecipes.ToList();
				}

				// Generate recipe with AI
				if (_aiService == null)
				{
					MessageBox.Show("AI service is not available.", "Error", 
						MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// Close dialog and show progress in main window
				dialog.Close();

				// Show progress message
				var progressResult = MessageBox.Show(
					"🤖 Đang tạo công thức với AI...\n\nVui lòng đợi trong giây lát.",
					"AI Chef - Đang xử lý",
					MessageBoxButton.OK,
					MessageBoxImage.Information);

				Recipe? generatedRecipe = null;
				try
				{
					generatedRecipe = await _aiService.GenerateRecipeWithDeduplicationAsync(
						availableIngredients,
						string.IsNullOrWhiteSpace(dishName) ? null : dishName,
						servings,
						string.IsNullOrWhiteSpace(customPreferences) ? null : customPreferences,
						userId,
						existingRecipes);
				}
				catch (Exception ex)
				{
					await (_loggingService?.LogErrorAsync("AI", "system", ex, "GenerateRecipe") ?? Task.CompletedTask);
					MessageBox.Show(
						$"❌ Lỗi khi tạo công thức:\n{ex.Message}",
						"Lỗi",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
				}

				if (generatedRecipe == null)
				{
					MessageBox.Show("Không thể tạo công thức. Vui lòng thử lại.", "Lỗi", 
						MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// Show preview dialog and ask for confirmation
				var confirmDialog = MessageBox.Show(
					$"✨ Công thức đã được tạo thành công!\n\n" +
					$"📝 Tên món: {generatedRecipe.Title}\n" +
					$"⏱️ Thời gian nấu: {generatedRecipe.CookTime} phút\n" +
					$"👥 Số phần: {generatedRecipe.Servings}\n" +
					$"🔥 Độ khó: {generatedRecipe.Difficulty}\n\n" +
					$"Bạn có muốn lưu công thức này và cập nhật pantry không?",
					"🤖 AI Chef - Xác nhận",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question);

				if (confirmDialog != MessageBoxResult.Yes)
				{
					return; // User declined
				}

				// Save recipe to database
				if (_recipeService == null)
				{
					MessageBox.Show("Recipe service is not available.", "Error", 
						MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				var savedRecipe = await _recipeService.CreateRecipeAsync(generatedRecipe);

				// Extract ingredient usage from recipe and deduct from pantry
				// Note: This is a simplified approach - in production, you'd parse the recipe instructions
				// to determine actual ingredient quantities used
				var ingredientUsage = new Dictionary<string, decimal>();
				foreach (var ingredient in availableIngredients)
				{
					// Simple heuristic: use 1 unit per ingredient if mentioned in recipe
					if (generatedRecipe.Instructions.ToLower().Contains(ingredient.Name.ToLower()) ||
					    generatedRecipe.Title.ToLower().Contains(ingredient.Name.ToLower()))
					{
						// Use a reasonable amount (e.g., 1 unit or 10% of available quantity)
						var usageAmount = ingredient.Quantity.HasValue 
							? Math.Min(1, ingredient.Quantity.Value * 0.1m)
							: 1;
						ingredientUsage[ingredient.Name] = usageAmount;
					}
				}

				if (ingredientUsage.Any())
				{
					await _ingredientService.DeductIngredientsFromPantryAsync(userId, ingredientUsage);
					// Refresh inventory view
					await _inventoryViewModel.LoadIngredientsAsync();
				}

				// Navigate to Recipes tab to see the new recipe
				if (Application.Current?.MainWindow?.DataContext is MainViewModel shell)
				{
					shell.SelectedTab = "Recipes";
				}

				MessageBox.Show(
					$"✅ Công thức '{savedRecipe.Title}' đã được lưu thành công!\n\n" +
					$"Pantry đã được cập nhật với số lượng nguyên liệu đã sử dụng.",
					"Thành công",
					MessageBoxButton.OK,
					MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "GenerateRecipe") ?? Task.CompletedTask);
				MessageBox.Show(
					$"❌ Lỗi khi tạo công thức:\n{ex.Message}",
					"Lỗi",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			finally { IsBusy = false; }
		}

		private async Task GenerateShoppingListAsync()
		{
			try
			{
				IsBusy = true;
				
				// Lấy userId hiện tại (hoặc dùng giá trị mặc định)
				int userId = 1; // Có thể lấy từ _userService nếu có
				if (_userService != null)
				{
					// Có thể lấy userId từ service nếu cần
					// userId = await _userService.GetCurrentUserIdAsync();
				}
				
				if (_shoppingListService == null)
				{
					MessageBox.Show("Shopping list service is not available.", "Error", 
						MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				// Mở dialog với chức năng tích hợp - tất cả trong một cửa sổ
				var dialog = new ShoppingListDialog(_shoppingListService, _ingredientService, userId);
				dialog.Owner = Application.Current?.MainWindow;
				
				// Set initial generation mode sau khi dialog được tạo
				dialog.Loaded += (s, e) =>
				{
					if (dialog.DataContext is ShoppingListDialogViewModel viewModel)
					{
						viewModel.SelectedGenerationMode = SelectedGenerationMode;
					}
				};
				
				dialog.ShowDialog();
			}
			catch (InvalidOperationException ex)
			{
				// Xử lý trường hợp không có nguyên liệu trong database
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "GenerateShoppingList") ?? Task.CompletedTask);
				MessageBox.Show(
					$"⚠️ {ex.Message}\n\nVui lòng thêm nguyên liệu vào database trước khi tạo shopping list.",
					"Không có dữ liệu",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "GenerateShoppingList") ?? Task.CompletedTask);
				MessageBox.Show(
					$"❌ Lỗi khi tạo shopping list:\n{ex.Message}",
					"Lỗi",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			finally { IsBusy = false; }
		}

        private async Task OpenNutritionAnalysisAsync()
		{
			try
			{
				IsBusy = true;
                // Open standalone NutritionWindow with NutritionViewModel
                var nutritionService = ServiceContainer.GetService<INutritionService>();
                var aiService = ServiceContainer.GetService<IAIService>();
                var recipeService = ServiceContainer.GetService<IRecipeService>();

                var vm = new NutritionViewModel(nutritionService, aiService, recipeService);
                var window = new NutritionWindow
                {
                    Owner = Application.Current?.MainWindow,
                    DataContext = vm
                };
                window.Show();
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "OpenNutritionAnalysis") ?? Task.CompletedTask);
			}
			finally { IsBusy = false; }
		}
	}
}


