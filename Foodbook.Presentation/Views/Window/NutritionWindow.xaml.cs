using System.Windows;
using Foodbook.Presentation.ViewModels;

namespace Foodbook.Presentation.Views
{
    public partial class NutritionWindow : Window
    {
        public NutritionWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is NutritionViewModel viewModel)
            {
                await viewModel.LoadRecipesAsync();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}


