using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Foodbook.Presentation.Views
{
    public partial class ImageUploadDialog : Window
    {
        public byte[]? ImageData { get; private set; }
        private string? _selectedFilePath;

        public ImageUploadDialog()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Image",
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                LoadImagePreview();
                UploadButton.IsEnabled = true;
            }
        }

        private void LoadImagePreview()
        {
            if (string.IsNullOrEmpty(_selectedFilePath)) return;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_selectedFilePath);
                bitmap.DecodePixelWidth = 300; // Limit size for preview
                bitmap.EndInit();

                ImagePreview.Source = bitmap;
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath)) return;

            try
            {
                ImageData = System.IO.File.ReadAllBytes(_selectedFilePath);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}