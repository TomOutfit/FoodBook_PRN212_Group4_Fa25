using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Net.Http;
using System.Windows.Media;

namespace Foodbook.Presentation.Services
{
    public class ImageService
    {
        private readonly string _imageDirectory;
        private readonly HttpClient _httpClient;

        public ImageService()
        {
            _imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(_imageDirectory))
            {
                Directory.CreateDirectory(_imageDirectory);
            }
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
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

        public async Task<BitmapImage?> LoadImageFromUrlAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            try
            {
                // Check if it's a local file path
                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) && uri.Scheme == "file")
                {
                    return await LoadImageAsync(imageUrl);
                }

                // Load from internet URL
                var response = await _httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                
                return await Task.Run(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(imageBytes);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image from URL {imageUrl}: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> SaveImageFromUrlAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            try
            {
                var response = await _httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(new Uri(imageUrl).LocalPath)}";
                var destinationPath = Path.Combine(_imageDirectory, fileName);

                await File.WriteAllBytesAsync(destinationPath, imageBytes);
                return destinationPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving image from URL {imageUrl}: {ex.Message}");
                return null;
            }
        }

        public bool IsValidImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                   (url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    url.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    url.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                    url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                    url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
        }
    }
}
