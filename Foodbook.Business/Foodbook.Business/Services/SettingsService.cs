using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Foodbook.Business.Interfaces;
using System.Threading;

namespace Foodbook.Business.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings? _cachedSettings;
        private readonly SemaphoreSlim _ioLock = new SemaphoreSlim(1, 1);

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
                await _ioLock.WaitAsync().ConfigureAwait(false);
                if (File.Exists(_settingsFilePath))
                {
                    using var fs = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    _cachedSettings = await JsonSerializer.DeserializeAsync<AppSettings>(fs).ConfigureAwait(false) ?? new AppSettings();
                }
                else
                {
                    _cachedSettings = new AppSettings();
                    // Save initial settings to create the file
                    await InternalSaveAsync(_cachedSettings).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                _cachedSettings = new AppSettings();
            }
            finally
            {
                if (_ioLock.CurrentCount == 0) _ioLock.Release();
            }

            return _cachedSettings;
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                await _ioLock.WaitAsync().ConfigureAwait(false);
                settings.LastUpdated = DateTime.UtcNow;
                await InternalSaveAsync(settings).ConfigureAwait(false);
                _cachedSettings = settings;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save settings: {ex.Message}", ex);
            }
            finally
            {
                if (_ioLock.CurrentCount == 0) _ioLock.Release();
            }
        }

        private async Task InternalSaveAsync(AppSettings settings)
        {
            var tempPath = _settingsFilePath + ".tmp";
            // Write to temp file first
            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(fs, settings, new JsonSerializerOptions { WriteIndented = true }).ConfigureAwait(false);
                await fs.FlushAsync().ConfigureAwait(false);
            }
            // Replace atomically if available; fallback to overwrite move
            try
            {
                File.Move(tempPath, _settingsFilePath, true);
            }
            catch
            {
                if (File.Exists(_settingsFilePath)) File.Delete(_settingsFilePath);
                File.Move(tempPath, _settingsFilePath);
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

