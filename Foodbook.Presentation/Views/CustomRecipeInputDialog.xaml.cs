using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Foodbook.Presentation.Views
{
    public partial class CustomRecipeInputDialog : Window
    {
        public string RecipeText { get; private set; } = string.Empty;
        public bool IsConfirmed { get; private set; }

        public CustomRecipeInputDialog()
        {
            InitializeComponent();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Action(() =>
            {
                if (AnalyzeButton != null)
                {
                    RecipeText = RecipeTextBox.Text.Trim();
                    AnalyzeButton.IsEnabled = !string.IsNullOrEmpty(RecipeText);
                }
            }));
        }

        private void OnAnalyzeClicked(object sender, RoutedEventArgs e)
        {
            RecipeText = RecipeTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(RecipeText))
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