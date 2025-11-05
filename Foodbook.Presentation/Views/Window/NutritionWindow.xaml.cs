using System.Windows;

namespace Foodbook.Presentation.Views
{
    public partial class NutritionWindow : Window
    {
        public NutritionWindow()
        {
            InitializeComponent();
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


