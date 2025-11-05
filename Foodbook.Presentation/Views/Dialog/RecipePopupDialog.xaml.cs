using System;
using System.Windows;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.Views
{
    public partial class RecipePopupDialog : Window
    {
        public Recipe? Recipe { get; private set; }
        public bool IsEditMode { get; private set; }

        public RecipePopupDialog(Recipe? recipe = null)
        {
            InitializeComponent();
            
            if (recipe != null)
            {
                // Edit mode
                IsEditMode = true;
                Recipe = recipe;
                Title = "Edit Recipe";
                DataContext = new RecipeViewModel(recipe);
            }
            else
            {
                // Create mode
                IsEditMode = false;
                Title = "Create New Recipe";
                DataContext = new RecipeViewModel();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as RecipeViewModel;
            if (viewModel == null) return;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(viewModel.Title))
            {
                MessageBox.Show("Please enter a recipe title.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (viewModel.CookTime <= 0)
            {
                MessageBox.Show("Please enter a valid cook time.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (viewModel.Servings <= 0)
            {
                MessageBox.Show("Please enter a valid number of servings.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(viewModel.Instructions))
            {
                MessageBox.Show("Please enter recipe instructions.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create or update recipe
            if (IsEditMode && Recipe != null)
            {
                // Update existing recipe
                Recipe.Title = viewModel.Title;
                Recipe.Description = viewModel.Description;
                Recipe.Instructions = viewModel.Instructions;
                Recipe.CookTime = viewModel.CookTime;
                Recipe.Servings = viewModel.Servings;
                Recipe.Difficulty = viewModel.Difficulty;
                Recipe.Category = viewModel.Category;
                Recipe.ImageUrl = viewModel.ImageUrl;
                Recipe.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new recipe
                Recipe = new Recipe
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Instructions = viewModel.Instructions,
                    CookTime = viewModel.CookTime,
                    Servings = viewModel.Servings,
                    Difficulty = viewModel.Difficulty,
                    Category = viewModel.Category,
                    ImageUrl = viewModel.ImageUrl,
                    UserId = 1, // Default user ID
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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

    public class RecipeViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public int CookTime { get; set; }
        public int Servings { get; set; }
        public string Difficulty { get; set; } = "Easy";
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public RecipeViewModel() { }

        public RecipeViewModel(Recipe recipe)
        {
            Title = recipe.Title;
            Description = recipe.Description ?? string.Empty;
            Instructions = recipe.Instructions;
            CookTime = recipe.CookTime;
            Servings = recipe.Servings;
            Difficulty = recipe.Difficulty;
            Category = recipe.Category ?? string.Empty;
            ImageUrl = recipe.ImageUrl ?? string.Empty;
        }
    }
}
