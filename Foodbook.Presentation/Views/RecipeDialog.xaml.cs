using System.Windows;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Services;
using System.Windows.Media.Imaging;

namespace Foodbook.Presentation.Views
{
    /// <summary>
    /// Interaction logic for RecipeDialog.xaml
    /// </summary>
    public partial class RecipeDialog : Window
    {
        private readonly ImageService _imageService;
        public Recipe? Recipe { get; private set; }
        public bool IsEditMode { get; private set; }

        public RecipeDialog(Recipe? recipe = null)
        {
            InitializeComponent();
            
            _imageService = new ImageService();
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

        private async void PreviewImage_Click(object sender, RoutedEventArgs e)
        {
            var imageUrl = ImageUrlTextBox.Text?.Trim();
            
            if (string.IsNullOrEmpty(imageUrl))
            {
                MessageBox.Show("Please enter an image URL first.", "No URL", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_imageService.IsValidImageUrl(imageUrl))
            {
                MessageBox.Show("Please enter a valid image URL (jpg, jpeg, png, bmp, gif, webp).", "Invalid URL", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Show loading state
                ImagePreviewBorder.Visibility = Visibility.Visible;
                ImagePreview.Source = null;

                var bitmap = await _imageService.LoadImageFromUrlAsync(imageUrl);
                
                if (bitmap != null)
                {
                    ImagePreview.Source = bitmap;
                    MessageBox.Show("Image loaded successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ImagePreviewBorder.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Failed to load image from URL. Please check the URL and try again.", "Load Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ImagePreviewBorder.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
