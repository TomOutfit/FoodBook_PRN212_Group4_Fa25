// Foodbook.Presentation/ViewModels/InventoryViewModel.cs
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
    public class InventoryViewModel : BaseViewModel
    {
        private readonly IIngredientService _ingredientService = null!;

        private List<Ingredient> _allIngredients = new();
        private ObservableCollection<Ingredient> _ingredients = new();

        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // KPIs
        private int _nearExpiryCount;
        private int _shoppingAlertsCount;
        private int _totalIngredients;

        // Sorting/Filtering
        private string _ingredientSortBy = "Name A-Z";
        private string _selectedGroup = "All"; // All, Proteins, Grains, Vegetables, Spices

        // External context
        public int CurrentUserId { get; set; } // set by shell after login; if 0, fallback to search all

        // Expose data
        public ObservableCollection<Ingredient> Ingredients { get => _ingredients; private set => SetProperty(ref _ingredients, value); }
        public int NearExpiryCount { get => _nearExpiryCount; private set => SetProperty(ref _nearExpiryCount, value); }
        public int ShoppingAlertsCount { get => _shoppingAlertsCount; private set => SetProperty(ref _shoppingAlertsCount, value); }
        public int TotalIngredients { get => _totalIngredients; private set => SetProperty(ref _totalIngredients, value); }

        public string IngredientSortBy
        {
            get => _ingredientSortBy;
            set
            {
                if (SetProperty(ref _ingredientSortBy, value))
                {
                    ApplyTransforms();
                }
            }
        }

        public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
        public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

        // Commands used by IngredientsView.xaml
        public ICommand LoadIngredientsCommand { get; }
        public ICommand AddIngredientCommand { get; }
        public ICommand ViewIngredientAnalyticsCommand { get; }
        public ICommand NavigateToAllIngredientsCommand { get; }
        public ICommand NavigateToProteinsCommand { get; }
        public ICommand NavigateToGrainsCommand { get; }
        public ICommand NavigateToVegetablesCommand { get; }
        public ICommand NavigateToSpicesCommand { get; }

        public InventoryViewModel(IIngredientService ingredientService, ILoggingService loggingService)
        {
            _ingredientService = ingredientService;

            LoadIngredientsCommand = new RelayCommand(async () => await LoadIngredientsAsync(), () => true);
            AddIngredientCommand = new RelayCommand(async () => await OpenAddIngredientDialogAsync(), () => true);
            ViewIngredientAnalyticsCommand = new RelayCommand(new Action(() => NavigateToAnalytics()), () => true);
            NavigateToAllIngredientsCommand = new RelayCommand(new Action(() => SetGroup("All")), () => true);
            NavigateToProteinsCommand = new RelayCommand(new Action(() => SetGroup("Proteins")), () => true);
            NavigateToGrainsCommand = new RelayCommand(new Action(() => SetGroup("Grains")), () => true);
            NavigateToVegetablesCommand = new RelayCommand(new Action(() => SetGroup("Vegetables")), () => true);
            NavigateToSpicesCommand = new RelayCommand(new Action(() => SetGroup("Spices")), () => true);
        }

        // Design-time: init commands as no-ops to avoid nulls
        public InventoryViewModel()
        {
            LoadIngredientsCommand = new RelayCommand(new Action(() => { }), () => true);
            AddIngredientCommand = new RelayCommand(new Action(() => { }), () => true);
            ViewIngredientAnalyticsCommand = new RelayCommand(new Action(() => { }), () => true);
            NavigateToAllIngredientsCommand = new RelayCommand(new Action(() => { }), () => true);
            NavigateToProteinsCommand = new RelayCommand(new Action(() => { }), () => true);
            NavigateToGrainsCommand = new RelayCommand(new Action(() => { }), () => true);
            NavigateToVegetablesCommand = new RelayCommand(new Action(() => { }), () => true);
            NavigateToSpicesCommand = new RelayCommand(new Action(() => { }), () => true);
        }

        public async Task LoadIngredientsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                IEnumerable<Ingredient> data;
                if (CurrentUserId > 0)
                {
                    data = await _ingredientService.GetUserIngredientsAsync(CurrentUserId);
                }
                else
                {
                    // Fallback: empty search returns all because string.Contains("") is true
                    data = await _ingredientService.SearchIngredientsAsync("");
                }

                _allIngredients = data?.ToList() ?? new List<Ingredient>();
                ComputeKpis(_allIngredients);
                ApplyTransforms();
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

        private async Task OpenAddIngredientDialogAsync()
        {
            try
            {
                var dialog = new IngredientDialog();
                dialog.Owner = Application.Current?.MainWindow;
                var result = dialog.ShowDialog();
                if (result == true && dialog.Ingredient != null)
                {
                    await _ingredientService.AddIngredientAsync(dialog.Ingredient);
                    await LoadIngredientsAsync();
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

        private void SetGroup(string group)
        {
            _selectedGroup = group;
            ApplyTransforms();
        }

        private void ApplyTransforms()
        {
            IEnumerable<Ingredient> query = _allIngredients;

            // group filter
            if (!string.Equals(_selectedGroup, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => IsInGroup(i, _selectedGroup));
            }

            // sort
            query = IngredientSortBy switch
            {
                "📝 Name A-Z" or "Name A-Z" => query.OrderBy(i => i.Name),
                "📝 Name Z-A" or "Name Z-A" => query.OrderByDescending(i => i.Name),
                "📂 Category" or "Category" => query.OrderBy(i => i.Category).ThenBy(i => i.Name),
                "📅 Date Added" or "Date Added" => query.OrderByDescending(i => i.CreatedAt),
                _ => query.OrderBy(i => i.Name),
            };

            Ingredients = new ObservableCollection<Ingredient>(query);
        }

        private static bool IsInGroup(Ingredient ingredient, string group)
        {
            var cat = ingredient.Category?.ToLowerInvariant() ?? string.Empty;
            return group switch
            {
                "Proteins" => cat.Contains("protein") || cat.Contains("meat") || cat.Contains("fish") || cat.Contains("seafood") || cat.Contains("egg"),
                "Grains" => cat.Contains("grain") || cat.Contains("rice") || cat.Contains("wheat") || cat.Contains("cereal") || cat.Contains("pasta") || cat.Contains("bread"),
                "Vegetables" => cat.Contains("vegetable") || cat.Contains("veg") || cat.Contains("greens") || cat.Contains("leafy") || cat.Contains("root"),
                "Spices" => cat.Contains("spice") || cat.Contains("herb") || cat.Contains("seasoning") || cat.Contains("condiment"),
                _ => true,
            };
        }

        private void ComputeKpis(IEnumerable<Ingredient> list)
        {
            var now = DateTime.UtcNow;
            NearExpiryCount = list.Count(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value >= now && (i.ExpiryDate.Value - now).TotalDays <= 7);
            ShoppingAlertsCount = list.Count(i => i.MinQuantity.HasValue && i.Quantity.HasValue && i.Quantity.Value <= i.MinQuantity.Value);
            TotalIngredients = list.Count();
        }

        // Placeholder; to be wired to AI features if available
        public async Task AnalyzePantryAsync()
        {
            await Task.CompletedTask;
        }
    }
}