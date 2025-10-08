using System.Windows;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.Views
{
    /// <summary>
    /// Interaction logic for RecipeDialog.xaml
    /// </summary>
    public partial class RecipeDialog : Window
    {
        public Recipe? Recipe { get; private set; }
        public bool IsEditMode { get; private set; }

        public RecipeDialog(Recipe? recipe = null)
        {
            InitializeComponent();
            
            IsEditMode = recipe != null;
            Recipe = recipe ?? new Recipe
            {
                Title = "",
                Description = "",
                Instructions = "",
                CookTime = 30,
                Difficulty = "Easy",
                ImageUrl = "",
                UserId = 1 // Demo user
            };

            DataContext = Recipe;
            
            // Set focus to title textbox
            TitleTextBox.Focus();
            TitleTextBox.SelectAll();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Recipe?.Title))
            {
                MessageBox.Show("Please enter a title for the recipe.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Recipe?.Instructions))
            {
                MessageBox.Show("Please enter instructions for the recipe.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                InstructionsTextBox.Focus();
                return;
            }

            if (Recipe.CookTime <= 0)
            {
                MessageBox.Show("Please enter a valid cook time (greater than 0).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                // CookTimeTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Recipe.Difficulty))
            {
                MessageBox.Show("Please select a difficulty level.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                // DifficultyComboBox.Focus();
                return;
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
}
