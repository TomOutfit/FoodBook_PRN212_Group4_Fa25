using System.Threading.Tasks;

namespace Foodbook.Business.Interfaces
{
    public interface IUnsplashImageService
    {
        /// <summary>
        /// Tìm kiếm ảnh món ăn từ Unsplash
        /// </summary>
        Task<string?> SearchFoodImageAsync(string dishName, int width = 800, int height = 600);
        
        /// <summary>
        /// Lấy ảnh ngẫu nhiên theo danh mục
        /// </summary>
        Task<string?> GetRandomFoodImageAsync(string category = "food");
        
        /// <summary>
        /// Lấy ảnh theo từ khóa cụ thể
        /// </summary>
        Task<string?> GetImageByKeywordAsync(string keyword, int width = 800, int height = 600);
        
        /// <summary>
        /// Tải ảnh xuống và lưu vào cache
        /// </summary>
        Task<string?> DownloadAndCacheImageAsync(string imageUrl, string fileName);
    }
}

