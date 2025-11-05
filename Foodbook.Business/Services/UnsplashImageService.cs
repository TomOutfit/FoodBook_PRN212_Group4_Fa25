using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Foodbook.Business.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Foodbook.Business.Services
{
    public class UnsplashImageService : IUnsplashImageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessKey;
        private readonly bool _useFoodishFallback;
        private readonly string _baseUrl = "https://api.unsplash.com";
        private readonly string _cacheFolder;

        public UnsplashImageService(IConfiguration? configuration = null)
        {
            _httpClient = new HttpClient();
            
            // Unsplash API - Sử dụng demo key hoặc access key nếu có
            try
            {
                _accessKey = configuration?["UnsplashAPI:AccessKey"] ?? "";
                _useFoodishFallback = bool.TryParse(configuration?["UnsplashAPI:UseFoodishFallback"], out var useFoodish) && useFoodish;
                if (!string.IsNullOrEmpty(_accessKey) && !_useFoodishFallback)
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_accessKey}");
                }
            }
            catch
            {
                _accessKey = string.Empty;
                _useFoodishFallback = true;
            }

            // Tạo thư mục cache
            try
            {
                _cacheFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageCache");
                if (!Directory.Exists(_cacheFolder))
                {
                    Directory.CreateDirectory(_cacheFolder);
                }
            }
            catch
            {
                _cacheFolder = Path.Combine(Path.GetTempPath(), "FoodBookImageCache");
                if (!Directory.Exists(_cacheFolder))
                {
                    Directory.CreateDirectory(_cacheFolder);
                }
            }
        }

        public async Task<string?> SearchFoodImageAsync(string dishName, int width = 800, int height = 600)
        {
            try
            {
                // Làm sạch tên món ăn
                var searchQuery = CleanSearchQuery(dishName);
                
                // Nếu cấu hình yêu cầu fallback hoặc không có access key → dùng Foodish ngay
                if (_useFoodishFallback || string.IsNullOrWhiteSpace(_accessKey))
                {
                    return await GetFoodishImageAsync(searchQuery, width, height);
                }

                // Thử lấy ảnh từ Unsplash trước
                var imageUrl = await SearchUnsplashAsync(searchQuery, width, height);
                if (!string.IsNullOrEmpty(imageUrl)) return imageUrl;

                // Fallback: Sử dụng Foodish
                return await GetFoodishImageAsync(searchQuery, width, height);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unsplash Search Error: {ex.Message}");
                // Fallback về Foodish
                return await GetFoodishImageAsync(dishName, width, height);
            }
        }

        public Task<string?> GetRandomFoodImageAsync(string category = "food")
        {
            try
            {
                // Sử dụng Foodish API - hoàn toàn miễn phí và không cần API key
                return Task.FromResult<string?>($"https://foodish-api.com/images/{category}/{category}{Random.Shared.Next(1, 1000)}.jpg");
            }
            catch
            {
                return Task.FromResult<string?>(null);
            }
        }

        public async Task<string?> GetImageByKeywordAsync(string keyword, int width = 800, int height = 600)
        {
            try
            {
                var searchQuery = CleanSearchQuery(keyword);
                return await SearchUnsplashAsync(searchQuery, width, height);
            }
            catch
            {
                // Fallback
                return await GetFoodishImageAsync(keyword, width, height);
            }
        }

        public async Task<string?> DownloadAndCacheImageAsync(string imageUrl, string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return null;

                var cacheFilePath = Path.Combine(_cacheFolder, fileName);
                
                // Kiểm tra cache
                if (File.Exists(cacheFilePath))
                {
                    return cacheFilePath;
                }

                // Tải ảnh về
                var response = await _httpClient.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(cacheFilePath, imageBytes);
                    return cacheFilePath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image Download Error: {ex.Message}");
            }

            return null;
        }

        private async Task<string?> SearchUnsplashAsync(string query, int width, int height)
        {
            try
            {
                var url = $"{_baseUrl}/search/photos?query={Uri.EscapeDataString(query)}&per_page=1&orientation=landscape";
                
                // Add timeout to prevent hanging requests
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync(url, cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<UnsplashResponse>(json);
                    
                    if (data?.Results?.Length > 0)
                    {
                        // Lấy ảnh đẹp nhất
                        var image = data.Results[0];
                        return $"{image.Urls?.Regular}?w={width}&h={height}&fit=crop";
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Unsplash API timeout for query: {query}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Unsplash API HTTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unsplash API Error: {ex.Message}");
            }

            return null;
        }

        private async Task<string?> GetFoodishImageAsync(string keyword, int width, int height)
        {
            try
            {
                // Foodish API - Trả về ảnh món ăn thật ngẫu nhiên
                var response = await _httpClient.GetAsync("https://foodish-api.com/images/biryani/biryani1.jpg");
                
                // Thử các loại món ăn phổ biến
                var foodTypes = new[] { "biryani", "burger", "butter-chicken", "dessert", "dosa", 
                                        "idly", "pasta", "pizza", "samosa", "shrimp" };
                
                var randomType = foodTypes[Random.Shared.Next(foodTypes.Length)];
                var randomNumber = Random.Shared.Next(1, 10);
                
                return $"https://foodish-api.com/images/{randomType}/{randomType}{randomNumber}.jpg";
            }
            catch
            {
                return null;
            }
        }

        private string CleanSearchQuery(string query)
        {
            // Làm sạch query để tìm kiếm tốt hơn
            query = query.Replace("recipe", "").Replace("dish", "").Replace("cook", "");
            query = query.Trim();
            
            // Nếu quá dài, chỉ lấy 2-3 từ đầu
            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 3)
            {
                query = string.Join(" ", words.Take(3));
            }

            return query;
        }
    }

    // Response models for Unsplash API
    public class UnsplashResponse
    {
        public UnsplashPhoto[]? Results { get; set; }
        public int Total { get; set; }
    }

    public class UnsplashPhoto
    {
        public string? Id { get; set; }
        public UnsplashUrls? Urls { get; set; }
        public string? Description { get; set; }
    }

    public class UnsplashUrls
    {
        public string? Raw { get; set; }
        public string? Full { get; set; }
        public string? Regular { get; set; }
        public string? Small { get; set; }
        public string? Thumb { get; set; }
    }
}

