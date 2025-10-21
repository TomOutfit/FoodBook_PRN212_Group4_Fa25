using System;
using System.Windows;

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
        
        private void OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Use Dispatcher to ensure UI is fully loaded
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (AnalyzeButton != null)
                {
                    AnalyzeButton.IsEnabled = !string.IsNullOrWhiteSpace(RecipeTextBox.Text);
                }
            }));
        }
        
        private void OnAnalyzeClick(object sender, RoutedEventArgs e)
        {
            RecipeText = RecipeTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(RecipeText))
            {
                IsConfirmed = true;
                DialogResult = true;
                Close();
            }
        }
        
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
