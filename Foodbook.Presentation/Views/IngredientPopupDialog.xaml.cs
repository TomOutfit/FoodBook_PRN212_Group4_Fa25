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

            // Create or update ingredient
            if (IsEditMode && Ingredient != null)
            {
                // Update existing ingredient
                Ingredient.Name = viewModel.Name;
                Ingredient.Category = viewModel.Category;
                Ingredient.NutritionInfo = viewModel.NutritionInfo;
                Ingredient.Unit = viewModel.Unit;
                Ingredient.Quantity = viewModel.Quantity;
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

        public IngredientViewModel() { }

        public IngredientViewModel(Ingredient ingredient)
        {
            Name = ingredient.Name;
            Category = ingredient.Category ?? string.Empty;
            NutritionInfo = ingredient.NutritionInfo ?? string.Empty;
            Unit = ingredient.Unit ?? string.Empty;
            Quantity = ingredient.Quantity ?? 0;
        }
    }
}
