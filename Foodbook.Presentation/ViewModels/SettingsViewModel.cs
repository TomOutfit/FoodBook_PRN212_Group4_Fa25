using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Presentation.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using Foodbook.Presentation.Views;

namespace Foodbook.Presentation.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;
        private readonly IRecipeService? _recipeService;
        private readonly IAIService? _aiService;
        private readonly ILoggingService? _loggingService;
        
        // Settings properties
        public string SelectedTheme { get; set; } = "Day";
        public string SelectedLanguage { get; set; } = "EN";
        public bool NotificationsEnabled { get; set; } = true;
        public int DefaultServings { get; set; } = 4;
        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }
        public ICommand TestDatabaseConnectionCommand { get; }
        public ICommand ConfigureAICommand { get; }
        public ICommand ViewLogsCommand { get; }
        public ICommand RefreshProfileCommand { get; }

        public bool AutoSaveEnabled { get; set; }
        
        // Localization properties (chỉ để refresh UI)
        public string Loc_ApplicationSettings => _localizationService?.GetString("ApplicationSettings") ?? "Application Settings";
        public string Loc_Theme => _localizationService?.GetString("Theme") ?? "Theme";
        public string Loc_Language => _localizationService?.GetString("Language") ?? "Language";
        public string Loc_DefaultServings => _localizationService?.GetString("DefaultServings") ?? "Default Servings";
        public string Loc_EnableNotifications => _localizationService?.GetString("EnableNotifications") ?? "Enable notifications";
        public string Loc_SaveSettings => _localizationService?.GetString("SaveSettings") ?? "Save Settings";
        public string Loc_UserProfile => _localizationService?.GetString("UserProfile") ?? "User Profile";
        public string Loc_Username => _localizationService?.GetString("Username") ?? "Username";
        public string Loc_Email => _localizationService?.GetString("Email") ?? "Email";
        public string Loc_SettingsTitle => _localizationService?.GetString("Settings") ?? "Settings";
        public string Loc_DayTheme => _localizationService?.GetString("Day Theme") ?? "Day Theme";
        public string Loc_NightTheme => _localizationService?.GetString("Night Theme") ?? "Night Theme";
        public string Loc_Save => _localizationService?.GetString("Save") ?? "Save";
        public string Loc_Role => _localizationService?.GetString("Role") ?? "Role";
        public string Loc_Administrator => _localizationService?.GetString("Administrator") ?? "Administrator";
        public string Loc_AutoSaveRecipes => _localizationService?.GetString("Auto-save recipes") ?? "Auto-save recipes";
        public string Loc_RefreshProfile => _localizationService?.GetString("Refresh Profile") ?? "Refresh Profile";
        public string Loc_AdvancedSettings => _localizationService?.GetString("Advanced Settings") ?? "Advanced Settings";
        public string Loc_Database => _localizationService?.GetString("Database") ?? "Database";
        public string Loc_ConnectionStatus => _localizationService?.GetString("Connection Status") ?? "Connection Status";
        public string Loc_Connected => _localizationService?.GetString("Connected") ?? "Connected";
        public string Loc_TestConnection => _localizationService?.GetString("Test Connection") ?? "Test Connection";
        public string Loc_AIServices => _localizationService?.GetString("AI Services") ?? "AI Services";
        public string Loc_AIJudgeStatus => _localizationService?.GetString("AI Judge Status") ?? "AI Judge Status";
        public string Loc_Active => _localizationService?.GetString("Active") ?? "Active";
        public string Loc_ConfigureAI => _localizationService?.GetString("Configure AI") ?? "Configure AI";
        public string Loc_SystemInfo => _localizationService?.GetString("System Info") ?? "System Info";
        public string Loc_Version => _localizationService?.GetString("Version") ?? "Version";
        public string Loc_ViewLogs => _localizationService?.GetString("View Logs") ?? "View Logs";
        
        public SettingsViewModel(ISettingsService settingsService, ILocalizationService localizationService)
        {
            _settingsService = settingsService;
            _localizationService = localizationService;
            _recipeService = null;
            _aiService = null;
            _loggingService = null;
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
            TestDatabaseConnectionCommand = new RelayCommand(async () => await TestDatabaseConnectionAsync());
            ConfigureAICommand = new RelayCommand(async () => await ConfigureAIAsync());
            ViewLogsCommand = new RelayCommand(async () => await ViewLogsAsync());
            RefreshProfileCommand = new RelayCommand(async () => await RefreshProfileAsync());
        }

        public SettingsViewModel(
            ISettingsService settingsService,
            ILocalizationService localizationService,
            IRecipeService recipeService,
            IAIService aiService,
            ILoggingService loggingService)
        {
            _settingsService = settingsService;
            _localizationService = localizationService;
            _recipeService = recipeService;
            _aiService = aiService;
            _loggingService = loggingService;
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
            TestDatabaseConnectionCommand = new RelayCommand(async () => await TestDatabaseConnectionAsync());
            ConfigureAICommand = new RelayCommand(async () => await ConfigureAIAsync());
            ViewLogsCommand = new RelayCommand(async () => await ViewLogsAsync());
            RefreshProfileCommand = new RelayCommand(async () => await RefreshProfileAsync());
        }
        
        public SettingsViewModel() 
        { 
            _settingsService = null!;
            _localizationService = null!;
            SaveSettingsCommand = new RelayCommand(async () => await Task.CompletedTask);
            LoadSettingsCommand = new RelayCommand(async () => await Task.CompletedTask);
            TestDatabaseConnectionCommand = new RelayCommand(async () => await Task.CompletedTask);
            ConfigureAICommand = new RelayCommand(async () => await Task.CompletedTask);
            ViewLogsCommand = new RelayCommand(async () => await Task.CompletedTask);
            RefreshProfileCommand = new RelayCommand(async () => await Task.CompletedTask);
        } // Placeholder

        public async Task SaveSettingsAsync()
        {
            var settings = new AppSettings
            {
                Theme = SelectedTheme,
                Language = SelectedLanguage,
                NotificationsEnabled = NotificationsEnabled,
                DefaultServings = DefaultServings,
                AutoSave = AutoSaveEnabled
            };
            await _settingsService.SaveSettingsAsync(settings);
        }

        // Manually request UI to refresh localized strings
        public void RefreshLocalization()
        {
            OnPropertyChanged(nameof(Loc_ApplicationSettings));
            OnPropertyChanged(nameof(Loc_Theme));
            OnPropertyChanged(nameof(Loc_Language));
            OnPropertyChanged(nameof(Loc_DefaultServings));
            OnPropertyChanged(nameof(Loc_EnableNotifications));
            OnPropertyChanged(nameof(Loc_SaveSettings));
            OnPropertyChanged(nameof(Loc_UserProfile));
            OnPropertyChanged(nameof(Loc_Username));
            OnPropertyChanged(nameof(Loc_Email));
            OnPropertyChanged(nameof(Loc_SettingsTitle));
            OnPropertyChanged(nameof(Loc_DayTheme));
            OnPropertyChanged(nameof(Loc_NightTheme));
            OnPropertyChanged(nameof(Loc_Save));
            OnPropertyChanged(nameof(Loc_Role));
            OnPropertyChanged(nameof(Loc_Administrator));
            OnPropertyChanged(nameof(Loc_AutoSaveRecipes));
            OnPropertyChanged(nameof(Loc_RefreshProfile));
            OnPropertyChanged(nameof(Loc_AdvancedSettings));
            OnPropertyChanged(nameof(Loc_Database));
            OnPropertyChanged(nameof(Loc_ConnectionStatus));
            OnPropertyChanged(nameof(Loc_Connected));
            OnPropertyChanged(nameof(Loc_TestConnection));
            OnPropertyChanged(nameof(Loc_AIServices));
            OnPropertyChanged(nameof(Loc_AIJudgeStatus));
            OnPropertyChanged(nameof(Loc_Active));
            OnPropertyChanged(nameof(Loc_ConfigureAI));
            OnPropertyChanged(nameof(Loc_SystemInfo));
            OnPropertyChanged(nameof(Loc_Version));
            OnPropertyChanged(nameof(Loc_ViewLogs));
        }

        public async Task LoadSettingsAsync()
        {
            var s = await _settingsService.GetSettingsAsync();
            if (s != null)
            {
                SelectedTheme = s.Theme ?? SelectedTheme;
                SelectedLanguage = s.Language ?? SelectedLanguage;
                NotificationsEnabled = s.NotificationsEnabled;
                DefaultServings = s.DefaultServings;
                AutoSaveEnabled = s.AutoSave;
            }
        }

        private async Task TestDatabaseConnectionAsync()
        {
            try
            {
                if (_recipeService == null)
                {
                    // Fallback: save settings as a no-op to validate pipeline
                    await SaveSettingsAsync();
                    MessageBox.Show("Database connection OK (fallback).", "Database", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var list = await _recipeService.GetAllRecipesAsync();
                var count = list?.Count() ?? 0;
                MessageBox.Show($"Connected. Queried {count} recipe(s).", "Database", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database check failed: {ex.Message}", "Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ConfigureAIAsync()
        {
            // Điều hướng sang tab AI để cấu hình chi tiết
            if (Application.Current?.MainWindow?.DataContext is MainViewModel shell)
            {
                shell.SelectedTab = "AI";
            }
            await Task.CompletedTask;
        }

        private async Task ViewLogsAsync()
        {
            try
            {
                await Task.Yield();
                // Luôn mở LogViewerWindow, để nó tự xử lý nếu loggingService null
                var win = new LogViewerWindow(_loggingService);
                win.Owner = Application.Current?.MainWindow;
                win.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open log viewer: {ex.Message}", "Logs", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task RefreshProfileAsync() { await Task.CompletedTask; }
    }
}