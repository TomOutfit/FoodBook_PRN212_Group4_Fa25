// Foodbook.Presentation/ViewModels/InventoryViewModel.cs
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly IIngredientService _ingredientService;
        // ... (Giữ các fields Inventory và KPI liên quan: _ingredients, _allIngredients, _nearExpiryCount, etc.)
        
        // Expose data
        public ObservableCollection<Ingredient> Ingredients { get; set; } = new();
        public int NearExpiryCount { get; set; } // Thay thế cho PantryExpiringSoon3Days
        public int ShoppingAlertsCount { get; set; }
        public int TotalIngredients { get; set; }
        public ObservableCollection<KeyValuePair<string,int>> TopIngredients { get; set; } = new();
        
        // Commands
        public ICommand LoadIngredientsCommand { get; }
        public ICommand SearchIngredientsCommand { get; }
        public ICommand AddIngredientCommand { get; }
        public ICommand AnalyzePantryCommand { get; }
        public ICommand MarkUsedCommand { get; }
        // ... (Giữ các Commands liên quan đến Ingredients)
        
        public InventoryViewModel(IIngredientService ingredientService, ILoggingService loggingService)
        {
            _ingredientService = ingredientService;
            LoadIngredientsCommand = new RelayCommand(async () => await LoadIngredientsAsync());
            AnalyzePantryCommand = new RelayCommand(async () => await AnalyzePantryAsync());
            // ... (Initialize other commands with their logic moved from MainViewModel)
        }
        
        // Placeholder for Designtime
        public InventoryViewModel() { }

        // public async Task LoadIngredientsAsync() { /* ... Logic di chuyển từ MainViewModel ... */ }
        // private async Task AnalyzePantryAsync() { /* ... Logic di chuyển từ MainViewModel ... */ }
    }
}