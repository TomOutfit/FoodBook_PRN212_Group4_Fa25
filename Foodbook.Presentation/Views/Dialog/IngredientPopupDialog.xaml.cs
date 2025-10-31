using System;
using System.Windows;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.Views
{
    public partial class IngredientPopupDialog : Window
    {
        public Ingredient? Ingredient { get; private set; }
        public bool IsEditMode { get; private set; }

        public IngredientPopupDialog(Ingredient? ingredient = null)
        {
            InitializeComponent();
            
            if (ingredient != null)
            {
                // Edit mode
                IsEditMode = true;
                Ingredient = ingredient;
                Title = "Edit Ingredient";
                DataContext = new IngredientViewModel(ingredient);
            }
            else
            {
                // Create mode
                IsEditMode = false;
                Title = "Create New Ingredient";
                DataContext = new IngredientViewModel();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as IngredientViewModel;
            if (viewModel == null) return;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(viewModel.Name))
            {
                MessageBox.Show("Please enter an ingredient name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (viewModel.ExpiryDate == null)
            {
                MessageBox.Show("Please select an expiry date.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create or update ingredient
            if (IsEditMode && Ingredient != null)
            {
                // Update existing ingredient
                Ingredient.Name = viewModel.Name;
                Ingredient.Category = viewModel.Category;
                Ingredient.NutritionInfo = viewModel.NutritionInfo;
                Ingredient.Unit = viewModel.Unit;
                Ingredient.Quantity = viewModel.Quantity;
                Ingredient.ExpiryDate = viewModel.ExpiryDate;
                Ingredient.PurchasedAt = viewModel.PurchasedAt;
                Ingredient.Location = viewModel.Location;
                Ingredient.MinQuantity = viewModel.MinQuantity;
            }
            else
            {
                // Create new ingredient
                Ingredient = new Ingredient
                {
                    Name = viewModel.Name,
                    Category = viewModel.Category,
                    NutritionInfo = viewModel.NutritionInfo,
                    Unit = viewModel.Unit,
                    Quantity = viewModel.Quantity,
                    ExpiryDate = viewModel.ExpiryDate,
                    PurchasedAt = viewModel.PurchasedAt ?? DateTime.UtcNow,
                    Location = viewModel.Location,
                    MinQuantity = viewModel.MinQuantity,
                    UserId = 1, // Default user ID
                    CreatedAt = DateTime.UtcNow
                };
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class IngredientViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string NutritionInfo { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal? MinQuantity { get; set; }

        public IngredientViewModel() { }

        public IngredientViewModel(Ingredient ingredient)
        {
            Name = ingredient.Name;
            Category = ingredient.Category ?? string.Empty;
            NutritionInfo = ingredient.NutritionInfo ?? string.Empty;
            Unit = ingredient.Unit ?? string.Empty;
            Quantity = ingredient.Quantity ?? 0;
            ExpiryDate = ingredient.ExpiryDate;
            PurchasedAt = ingredient.PurchasedAt;
            Location = ingredient.Location ?? string.Empty;
            MinQuantity = ingredient.MinQuantity;
        }
    }
}
