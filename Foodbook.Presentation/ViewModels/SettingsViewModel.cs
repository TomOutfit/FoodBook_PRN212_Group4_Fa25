// Foodbook.Presentation/ViewModels/SettingsViewModel.cs
using System.Windows.Input;
using Foodbook.Business.Interfaces;

namespace Foodbook.Presentation.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;
        
        // Settings properties
        public string SelectedTheme { get; set; } = "Light";
        public string SelectedLanguage { get; set; } = "English";
        public bool NotificationsEnabled { get; set; } = true;
        public int DefaultServings { get; set; } = 4;
        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }
        // ... (Các Commands/Properties liên quan đến Database/AI Settings đã chuyển từ MainViewModel)
        
        // Localization properties (chỉ để refresh UI)
        public string Loc_Theme => _localizationService?.GetString("Theme") ?? "Theme";
        // ... (Giữ lại tất cả các Loc_ properties liên quan)
        
        public SettingsViewModel(ISettingsService settingsService, ILocalizationService localizationService)
        {
            _settingsService = settingsService;
            _localizationService = localizationService;
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
        }
        
        public SettingsViewModel() { } // Placeholder

        // public async Task SaveSettingsAsync() { /* ... Logic di chuyển từ MainViewModel ... */ }
        // public async Task LoadSettingsAsync() { /* ... Logic di chuyển từ MainViewModel ... */ }
        // private void RefreshLocalization() { /* ... Logic di chuyển từ MainViewModel ... */ }
    }
}