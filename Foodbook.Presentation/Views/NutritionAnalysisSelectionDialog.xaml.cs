using System.Windows;

namespace Foodbook.Presentation.Views
{
    public partial class NutritionAnalysisSelectionDialog : Window
    {
        public enum AnalysisType
        {
            SavedRecipe,
            CustomRecipe
        }

        public AnalysisType SelectedType { get; private set; }
        public bool IsConfirmed { get; private set; }

        public NutritionAnalysisSelectionDialog()
        {
            InitializeComponent();
        }

        private void OnSavedRecipeClicked(object sender, RoutedEventArgs e)
        {
            SelectedType = AnalysisType.SavedRecipe;
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void OnCustomRecipeClicked(object sender, RoutedEventArgs e)
        {
            SelectedType = AnalysisType.CustomRecipe;
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
