using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.Views
{
    public partial class RecipeSelectionDialog : Window
    {
        public Recipe? SelectedRecipe { get; private set; }
        public bool IsConfirmed { get; private set; }

        public RecipeSelectionDialog()
        {
            InitializeComponent();
        }

        public void LoadRecipes(IEnumerable<Recipe> recipes)
        {
            RecipeListBox.ItemsSource = recipes.ToList();
        }

        private void OnRecipeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecipeListBox.SelectedItem is Recipe selectedRecipe)
            {
                SelectedRecipe = selectedRecipe;
                AnalyzeButton.IsEnabled = true;
            }
            else
            {
                SelectedRecipe = null;
                AnalyzeButton.IsEnabled = false;
            }
        }

        private void OnAnalyzeClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedRecipe != null)
            {
                IsConfirmed = true;
                DialogResult = true;
                Close();
            }
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
