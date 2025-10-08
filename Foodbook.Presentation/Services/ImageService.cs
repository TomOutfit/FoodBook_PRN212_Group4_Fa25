using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Foodbook.Presentation.Services
{
    public class ImageService
    {
        private readonly string _imageDirectory;

        public ImageService()
        {
            _imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(_imageDirectory))
            {
                Directory.CreateDirectory(_imageDirectory);
            }
        }

        public async Task<string?> SelectAndSaveImageAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select an Image",
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return await SaveImageAsync(openFileDialog.FileName);
            }

            return null;
        }

        public async Task<string> SaveImageAsync(string sourcePath)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";
            var destinationPath = Path.Combine(_imageDirectory, fileName);

            await Task.Run(() =>
            {
                File.Copy(sourcePath, destinationPath, true);
            });

            return destinationPath;
        }

        public async Task<BitmapImage?> LoadImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                return null;

            return await Task.Run(() =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            });
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                return false;

            return await Task.Run(() =>
            {
                try
                {
                    File.Delete(imagePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return string.Empty;

            return $"file:///{imagePath.Replace("\\", "/")}";
        }
    }
}
