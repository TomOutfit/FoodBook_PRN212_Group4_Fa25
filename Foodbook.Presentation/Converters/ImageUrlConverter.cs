using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System.IO;

namespace Foodbook.Presentation.Converters
{
    public class ImageUrlConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, BitmapImage> _imageCache = new Dictionary<string, BitmapImage>();
        private static readonly object _cacheLock = new object();

        // Cached default image
        private static BitmapImage? _defaultImage = null;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string imageUrl && !string.IsNullOrEmpty(imageUrl))
                {
                    // Check cache first
                    lock (_imageCache)
                    {
                        if (_imageCache.ContainsKey(imageUrl))
                        {
                            return _imageCache[imageUrl];
                        }
                    }

                    // Try to load image from URL with robust error handling
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                        // Use OnDemand to avoid immediate color context processing
                        bitmap.CacheOption = BitmapCacheOption.OnDemand;
                        // Use smaller, safer dimensions to avoid ArgumentException
                        bitmap.DecodePixelWidth = 150;
                        bitmap.DecodePixelHeight = 150;
                        // Disable color context processing to avoid ArgumentException
                        bitmap.CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile;
                        bitmap.EndInit();
                        
                        // DON'T freeze immediately - let it load asynchronously
                        // The image will freeze automatically when it's done loading
                        
                        // Cache the image
                        lock (_imageCache)
                        {
                            if (_imageCache.Count < 50) // Reduced cache size
                            {
                                _imageCache[imageUrl] = bitmap;
                            }
                        }
                        
                        return bitmap;
                    }
                    catch (UriFormatException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Invalid URI format for image URL {imageUrl}: {ex.Message}");
                        return GetDefaultImage();
                    }
                    catch (ArgumentException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ArgumentException loading image from URL {imageUrl}: {ex.Message}");
                        // Try alternative loading method
                        return TryLoadImageAlternative(imageUrl);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading image from URL {imageUrl}: {ex.Message}");
                        // Try alternative loading method
                        return TryLoadImageAlternative(imageUrl);
                    }
                }

                return GetDefaultImage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Critical error in ImageUrlConverter: {ex.Message}");
                return GetDefaultImage();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static BitmapImage TryLoadImageAlternative(string imageUrl)
        {
            try
            {
                // Alternative method: download image data first, then create BitmapImage
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5); // Short timeout
                
                var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result;
                
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(imageBytes);
                bitmap.CacheOption = BitmapCacheOption.OnDemand;
                bitmap.DecodePixelWidth = 150;
                bitmap.DecodePixelHeight = 150;
                bitmap.CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile;
                bitmap.EndInit();
                
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Alternative image loading failed for {imageUrl}: {ex.Message}");
                return GetDefaultImage();
            }
        }

        private static BitmapImage GetDefaultImage()
        {
            // Return cached default image if available
            if (_defaultImage != null)
            {
                return _defaultImage;
            }

            // Create a default placeholder image
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Images/FoodBook_Logo.png");
                // Use OnDemand to avoid immediate color context processing
                bitmap.CacheOption = BitmapCacheOption.OnDemand;
                // Use safe dimensions for default image too
                bitmap.DecodePixelWidth = 150;
                bitmap.DecodePixelHeight = 150;
                // Disable color context processing to avoid ArgumentException
                bitmap.CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile;
                bitmap.EndInit();
                _defaultImage = bitmap;
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading default image: {ex.Message}");
                // Fallback to null (UI will show emoji placeholder)
                _defaultImage = null;
                return _defaultImage!;
            }
        }
    }
}
