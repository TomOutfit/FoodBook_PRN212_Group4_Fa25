using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Foodbook.Business.Interfaces;

namespace Foodbook.Business.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings? _cachedSettings;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "Foodbook");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
        }

        public async Task<AppSettings> GetSettingsAsync()
        {
            if (_cachedSettings != null)
                return _cachedSettings;

            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_settingsFilePath);
                    _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _cachedSettings = new AppSettings();
                    await SaveSettingsAsync(_cachedSettings);
                }
            }
            catch (Exception)
            {
                _cachedSettings = new AppSettings();
            }

            return _cachedSettings;
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                settings.LastUpdated = DateTime.UtcNow;
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(_settingsFilePath, json);
                _cachedSettings = settings;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save settings: {ex.Message}", ex);
            }
        }

        public async Task<string> GetThemeAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.Theme;
        }

        public async Task SetThemeAsync(string theme)
        {
            var settings = await GetSettingsAsync();
            settings.Theme = theme;
            await SaveSettingsAsync(settings);
        }

        public async Task<string> GetLanguageAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.Language;
        }

        public async Task SetLanguageAsync(string language)
        {
            var settings = await GetSettingsAsync();
            settings.Language = language;
            await SaveSettingsAsync(settings);
        }

        public async Task<bool> GetNotificationsEnabledAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.NotificationsEnabled;
        }

        public async Task SetNotificationsEnabledAsync(bool enabled)
        {
            var settings = await GetSettingsAsync();
            settings.NotificationsEnabled = enabled;
            await SaveSettingsAsync(settings);
        }

        public async Task<int> GetDefaultServingsAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.DefaultServings;
        }

        public async Task SetDefaultServingsAsync(int servings)
        {
            var settings = await GetSettingsAsync();
            settings.DefaultServings = servings;
            await SaveSettingsAsync(settings);
        }
    }
}

