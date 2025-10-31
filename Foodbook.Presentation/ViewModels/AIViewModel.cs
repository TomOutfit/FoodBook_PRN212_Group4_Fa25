using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Presentation.Commands;
using Foodbook.Presentation.Views;

namespace Foodbook.Presentation.ViewModels
{
	public class AIViewModel : BaseViewModel
	{
		private readonly IAIService? _aiService;
		private readonly IShoppingListService? _shoppingListService;
		private readonly ILoggingService? _loggingService;
		private readonly IUserService? _userService;
		private readonly InventoryViewModel? _inventoryViewModel;

		private bool _isBusy;
		public bool IsBusy
		{
			get => _isBusy;
			private set => SetProperty(ref _isBusy, value);
		}

		public ICommand JudgeDishCommand { get; }
		public ICommand GenerateRecipeCommand { get; }
		public ICommand GenerateShoppingListCommand { get; }
		public ICommand OpenNutritionAnalysisCommand { get; }

		public AIViewModel(
			IAIService aiService,
			IShoppingListService shoppingListService,
			ILoggingService loggingService,
			IUserService userService,
			InventoryViewModel inventoryViewModel)
		{
			_aiService = aiService;
			_shoppingListService = shoppingListService;
			_loggingService = loggingService;
			_userService = userService;
			_inventoryViewModel = inventoryViewModel;

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
				// Navigate to Recipes tab in the shell (simple UX for now)
				if (Application.Current?.MainWindow?.DataContext is MainViewModel shell)
				{
					shell.SelectedTab = "Recipes";
				}
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "GenerateRecipe") ?? Task.CompletedTask);
			}
			finally { IsBusy = false; }
		}

		private async Task GenerateShoppingListAsync()
		{
			try
			{
				IsBusy = true;
				// Open the shopping list dialog (detailed view)
				var dialog = new ShoppingListDialog(_shoppingListService!);
				dialog.Owner = Application.Current?.MainWindow;
				dialog.ShowDialog();
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "GenerateShoppingList") ?? Task.CompletedTask);
			}
			finally { IsBusy = false; }
		}

		private async Task OpenNutritionAnalysisAsync()
		{
			try
			{
				IsBusy = true;
				// Navigate to the in-app Nutrition view instead of dialog
				if (Application.Current?.MainWindow?.DataContext is MainViewModel shell)
				{
					shell.SelectedTab = "Nutrition";
				}
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await (_loggingService?.LogErrorAsync("AI", "system", ex, "OpenNutritionAnalysis") ?? Task.CompletedTask);
			}
			finally { IsBusy = false; }
		}
	}
}


