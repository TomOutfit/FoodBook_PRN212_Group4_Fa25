using System.Windows;

namespace Foodbook.Presentation.Views
{
    public partial class ImageManagementWindow : Window
    {
        public ImageManagementWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}