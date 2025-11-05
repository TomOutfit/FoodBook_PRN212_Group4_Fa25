using System;
using System.Threading.Tasks;

namespace Foodbook.Business.Interfaces
{
    public interface ISettingsService
    {
        Task<AppSettings> GetSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
        Task<string> GetThemeAsync();
        Task SetThemeAsync(string theme);
        Task<string> GetLanguageAsync();
        Task SetLanguageAsync(string language);
        Task<bool> GetNotificationsEnabledAsync();
        Task SetNotificationsEnabledAsync(bool enabled);
        Task<int> GetDefaultServingsAsync();
        Task SetDefaultServingsAsync(int servings);
    }

    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "English";
        public bool NotificationsEnabled { get; set; } = true;
        public int DefaultServings { get; set; } = 4;
        public bool AutoSave { get; set; } = true;
        public string DefaultDifficulty { get; set; } = "Easy";
        public bool ShowNutritionInfo { get; set; } = true;
        public bool EnableAIFeatures { get; set; } = true;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

