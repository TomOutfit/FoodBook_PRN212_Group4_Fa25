using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Presentation.Commands;

namespace Foodbook.Presentation.ViewModels
{
    public class ShoppingListDialogViewModel : BaseViewModel
    {
        private readonly IShoppingListService? _shoppingListService;
        private readonly IIngredientService? _ingredientService;
        private ShoppingListResult? _shoppingList;
        private string _searchText = string.Empty;
        private ObservableCollection<ShoppingCategory> _filteredCategories = new();
        private string _selectedGenerationMode = "Random Generation List";
        private string _selectedExportType = "Text File (.txt)";
        private string _exportFileName = "ShoppingList";
        private ObservableCollection<string> _availableIngredients = new();
        private ObservableCollection<string> _selectedIngredients = new();
        private bool _showIngredientSelection = false;
        private bool _showExportOptions = false;

        public ShoppingListDialogViewModel(
            IShoppingListService? shoppingListService = null, 
            IIngredientService? ingredientService = null,
            int userId = 1)
        {
            _shoppingListService = shoppingListService;
            _ingredientService = ingredientService;
            CurrentUserId = userId;
            
            SearchCommand = new RelayCommand(() => FilterItems(), (Func<bool>?)null);
            GenerateCommand = new RelayCommand(async () => await GenerateShoppingListAsync(), CanGenerate);
            ExportCommand = new RelayCommand(async () => await ExportShoppingListAsync(), CanExport);
            PrintCommand = new RelayCommand(() => PrintShoppingList(), CanPrint);
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke(), (Func<bool>?)null);
            ShowIngredientSelectionCommand = new RelayCommand(() => ShowIngredientSelection = true, (Func<bool>?)null);
            ShowExportOptionsCommand = new RelayCommand(() => ShowExportOptions = true, (Func<bool>?)null);
            
            GenerationModes = new List<string> { "Random Generation List", "User List" };
            ExportTypes = new List<string> { "Text File (.txt)", "Notes Integration", "PDF Document" };
            
            LoadIngredientsAsync();
        }

        public event Action? CloseRequested;
        public event Action? GenerateRequested;
        public int CurrentUserId { get; set; } = 1;

        #region Properties

        public ShoppingListResult? ShoppingList
        {
            get => _shoppingList;
            set
            {
                if (SetProperty(ref _shoppingList, value))
                {
                    UpdateFilteredCategories();
                    OnPropertyChanged(nameof(TotalItems));
                    OnPropertyChanged(nameof(EstimatedCost));
                    OnPropertyChanged(nameof(ShoppingTime));
                    OnPropertyChanged(nameof(IsListAvailable));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterItems();
                }
            }
        }

        public ObservableCollection<ShoppingCategory> FilteredCategories
        {
            get => _filteredCategories;
            set => SetProperty(ref _filteredCategories, value);
        }

        public int TotalItems => ShoppingList?.TotalItems ?? 0;

        public string EstimatedCost => ShoppingList != null ? $"${ShoppingList.EstimatedCost:F2}" : "$0.00";

        public string ShoppingTime => ShoppingList != null 
            ? $"{ShoppingList.EstimatedShoppingTime.TotalMinutes:F0} min" 
            : "0 min";

        public ObservableCollection<string> Tips => ShoppingList != null 
            ? new ObservableCollection<string>(ShoppingList.Tips) 
            : new ObservableCollection<string>();

        public ObservableCollection<string> StoreSuggestions => ShoppingList != null 
            ? new ObservableCollection<string>(ShoppingList.StoreSuggestions) 
            : new ObservableCollection<string>();

        public bool IsListAvailable => ShoppingList != null;

        public string SelectedGenerationMode
        {
            get => _selectedGenerationMode;
            set => SetProperty(ref _selectedGenerationMode, value);
        }

        public List<string> GenerationModes { get; }

        public string SelectedExportType
        {
            get => _selectedExportType;
            set => SetProperty(ref _selectedExportType, value);
        }

        public List<string> ExportTypes { get; }

        public string ExportFileName
        {
            get => _exportFileName;
            set => SetProperty(ref _exportFileName, value);
        }

        public ObservableCollection<string> AvailableIngredients
        {
            get => _availableIngredients;
            set => SetProperty(ref _availableIngredients, value);
        }

        public ObservableCollection<string> SelectedIngredients
        {
            get => _selectedIngredients;
            set => SetProperty(ref _selectedIngredients, value);
        }

        public bool ShowIngredientSelection
        {
            get => _showIngredientSelection;
            set => SetProperty(ref _showIngredientSelection, value);
        }

        public bool ShowExportOptions
        {
            get => _showExportOptions;
            set => SetProperty(ref _showExportOptions, value);
        }

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowIngredientSelectionCommand { get; }
        public ICommand ShowExportOptionsCommand { get; }

        #endregion

        #region Methods

        public void SetShoppingList(ShoppingListResult shoppingList)
        {
            ShoppingList = shoppingList;
        }

        public void FilterItems()
        {
            if (ShoppingList == null)
            {
                FilteredCategories.Clear();
                return;
            }

            var searchText = SearchText?.Trim().ToLowerInvariant() ?? string.Empty;

            if (string.IsNullOrEmpty(searchText))
            {
                UpdateFilteredCategories();
                return;
            }

            var filtered = ShoppingList.Categories
                .Where(category =>
                    category.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    category.StoreSection.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    category.Items.Any(item =>
                        item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        item.Notes?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                        item.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                .Select(category => new ShoppingCategory
                {
                    Name = category.Name,
                    Icon = category.Icon,
                    Color = category.Color,
                    Priority = category.Priority,
                    StoreSection = category.StoreSection,
                    ShoppingOrder = category.ShoppingOrder,
                    Items = category.Items.Where(item =>
                        item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        item.Notes?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                        item.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                        category.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList(),
                    CategoryTotal = category.Items
                        .Where(item =>
                            item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                            item.Notes?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                            item.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        .Sum(item => item.EstimatedPrice)
                })
                .Where(category => category.Items.Any())
                .ToList();

            FilteredCategories = new ObservableCollection<ShoppingCategory>(filtered);
        }

        private void UpdateFilteredCategories()
        {
            if (ShoppingList == null)
            {
                FilteredCategories.Clear();
                return;
            }

            FilteredCategories = new ObservableCollection<ShoppingCategory>(ShoppingList.Categories);
            OnPropertyChanged(nameof(Tips));
            OnPropertyChanged(nameof(StoreSuggestions));
        }

        public void ToggleItemChecked(ShoppingItem item)
        {
            if (item == null || ShoppingList == null) return;

            item.IsChecked = !item.IsChecked;
            OnPropertyChanged(nameof(TotalItems));
        }

        public async Task GenerateShoppingListAsync()
        {
            if (_shoppingListService == null)
            {
                return;
            }

            try
            {
                if (SelectedGenerationMode == "Random Generation List" || SelectedGenerationMode == "Random")
                {
                    // Generate random shopping list
                    var shoppingList = await _shoppingListService.GenerateRandomShoppingListFromDatabaseAsync(CurrentUserId, itemCount: 0);
                    ShoppingList = shoppingList;
                    ShowIngredientSelection = false;
                }
                else // User List
                {
                    if (!SelectedIngredients.Any())
                    {
                        ShowIngredientSelection = true;
                        return;
                    }

                    // Generate from selected ingredients
                    var shoppingList = await _shoppingListService.GenerateShoppingListFromIngredientsAsync(SelectedIngredients, CurrentUserId);
                    ShoppingList = shoppingList;
                    ShowIngredientSelection = false;
                }

                GenerateRequested?.Invoke();
            }
            catch (InvalidOperationException)
            {
                // Will be handled by view
                throw;
            }
        }

        private Task ExportShoppingListAsync()
        {
            if (ShoppingList == null || _shoppingListService == null) 
                return Task.CompletedTask;

            ShowExportOptions = true;
            return Task.CompletedTask;
        }

        private void PrintShoppingList()
        {
            if (ShoppingList == null) return;
            // Printing will be handled by view code-behind
        }

        private bool CanGenerate()
        {
            return _shoppingListService != null;
        }

        private bool CanExport()
        {
            return IsListAvailable && ShoppingList != null;
        }

        private bool CanPrint()
        {
            return IsListAvailable && ShoppingList != null;
        }

        private async void LoadIngredientsAsync()
        {
            try
            {
                if (_ingredientService != null)
                {
                    var userIngredients = await _ingredientService.GetUserIngredientsAsync(CurrentUserId);
                    var ingredientNames = userIngredients.Select(i => i.Name).Distinct().OrderBy(i => i).ToList();
                    
                    var allFound = new HashSet<string>(ingredientNames);
                    var searchTerms = new[] { "a", "e", "i", "o", "u", "b", "c", "d", "f", "g", "h", "m", "p", "r", "s", "t" };
                    foreach (var term in searchTerms.Take(5))
                    {
                        var found = await _ingredientService.SearchIngredientsAsync(term);
                        foreach (var ing in found)
                        {
                            allFound.Add(ing.Name);
                        }
                    }
                    
                    AvailableIngredients = new ObservableCollection<string>(allFound.OrderBy(i => i));
                }
                else
                {
                    // Fallback to sample ingredients
                    AvailableIngredients = new ObservableCollection<string>
                    {
                        "Tomato", "Onion", "Garlic", "Chicken", "Beef", 
                        "Rice", "Pasta", "Milk", "Eggs", "Bread",
                        "Butter", "Oil", "Salt", "Pepper", "Spinach",
                        "Potato", "Carrot", "Bell Pepper", "Broccoli", "Lettuce",
                        "Fish", "Salmon", "Shrimp", "Cheese", "Yogurt"
                    };
                }
            }
            catch
            {
                // Use sample ingredients on error
                AvailableIngredients = new ObservableCollection<string>
                {
                    "Tomato", "Onion", "Garlic", "Chicken", "Beef", 
                    "Rice", "Pasta", "Milk", "Eggs", "Bread"
                };
            }
        }

        #endregion
    }
}

