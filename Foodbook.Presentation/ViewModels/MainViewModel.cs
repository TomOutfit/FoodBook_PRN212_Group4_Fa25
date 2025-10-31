// Foodbook.Presentation/ViewModels/MainViewModel.cs (RÚT GỌN)
using System.Collections.ObjectModel;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using System.Threading.Tasks;
using System;
using Foodbook.Presentation.Commands;
using Foodbook.Business;

namespace Foodbook.Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // 1. SERVICES (Cần cho việc khởi tạo các ViewModel con & Localization)
        private readonly ILocalizationService? _localizationService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService? _authService;
        // ... (Giữ tất cả các Service readonly fields cần thiết)
        
        // 2. VIEWMODEL CON (Được inject/khởi tạo)
        public RecipeListViewModel RecipeListVM { get; }
        public InventoryViewModel InventoryVM { get; }
        public AnalyticsViewModel AnalyticsVM { get; }
        public SettingsViewModel SettingsVM { get; }
        public AIViewModel AIVM { get; } // AI-related logic
        public NutritionViewModel NutritionVM { get; } // Nutrition analysis ViewModel

        // 3. CORE SHELL PROPERTIES & COMMANDS
        private string _selectedTab = "Dashboard";
        private bool _isNavigatingTabs = false;
        private DateTime _lastNavigationAt = DateTime.MinValue;
        public string SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (value == _selectedTab) return; // avoid redundant navigations
                if (SetProperty(ref _selectedTab, value))
                {
                    _ = SelectTab(value);
                }
            }
        }
        
        private bool _isSidebarCollapsed = false;
        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set => SetProperty(ref _isSidebarCollapsed, value);
        }
        
        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }
        
        // Localization Properties (Giữ lại để Sidebar binding không bị đứt)
        public string Loc_Dashboard => _localizationService?.GetString("Dashboard") ?? "Dashboard";
        public string Loc_MyRecipes => _localizationService?.GetString("My Recipes") ?? "My Recipes";
        public string Loc_Ingredients => _localizationService?.GetString("Ingredients") ?? "Ingredients";
        public string Loc_Analytics => _localizationService?.GetString("Analytics") ?? "Analytics";
        public string Loc_Settings => _localizationService?.GetString("Settings") ?? "Settings";
        public string Loc_AI => _localizationService?.GetString("AI Chef") ?? "AI Chef"; // Thêm Loc_AI
        
        // Shell Commands
        public ICommand SelectTabCommand { get; }
        public ICommand ToggleSidebarCommand { get; }

        // Delegate settings to SettingsVM for window code-behind
        public string SelectedTheme => SettingsVM.SelectedTheme;
        public string SelectedLanguage => SettingsVM.SelectedLanguage;
        public ICommand LoadSettingsCommand => SettingsVM.LoadSettingsCommand;

        // Forward commonly used commands for XAML conveniences
        public ICommand PrevPageCommand => RecipeListVM.PrevPageCommand;
        public ICommand NextPageCommand => RecipeListVM.NextPageCommand;
        public ICommand ViewRecipeCommand => RecipeListVM.ViewRecipeCommand;
        public ICommand EditRecipeCommand => RecipeListVM.EditRecipeCommand;

        public MainViewModel(
            IRecipeService recipeService, 
            IIngredientService ingredientService,
            IAIService aiService,
            IUserService userService,
            IShoppingListService shoppingListService,
            INutritionService nutritionService,
            ILoggingService loggingService,
            ISettingsService settingsService,
            ILocalizationService localizationService,
            IAuthenticationService authenticationService)
        {
            _localizationService = localizationService;
            _userService = userService;
            _authService = authenticationService;

            // 4. KHỞI TẠO CÁC VIEWMODEL CON VỚI SERVICES CHUYÊN BIỆT
            RecipeListVM = new RecipeListViewModel(recipeService, loggingService, (IUnsplashImageService)ServiceContainer.GetService<IUnsplashImageService>());
            InventoryVM = new InventoryViewModel(ingredientService, loggingService);
            AnalyticsVM = new AnalyticsViewModel(recipeService, ingredientService, loggingService);
            SettingsVM = new SettingsViewModel(settingsService, localizationService, recipeService, aiService, loggingService);
            AIVM = new AIViewModel(aiService, shoppingListService, loggingService, userService, InventoryVM);
            NutritionVM = new NutritionViewModel(nutritionService, aiService, recipeService);
            
            // 5. THIẾT LẬP CÁC COMMAND CHÍNH
            // Chỉ cập nhật SelectedTab; setter sẽ gọi SelectTab để nạp dữ liệu và cập nhật UI
            SelectTabCommand = new RelayCommand<string>((tabName) =>
            {
                if (!string.IsNullOrWhiteSpace(tabName))
                {
                    SelectedTab = tabName;
                }
            });
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            
            // Bắt đầu tải dữ liệu chung
            _ = LoadCurrentUserAsync();

        }

        // Khởi tạo không tham số cho Designtime (nếu cần)
        public MainViewModel() 
        {
            _userService = null!;
            // Thiết lập giá trị mặc định cho Design View
            RecipeListVM = new RecipeListViewModel();
            InventoryVM = new InventoryViewModel();
            AnalyticsVM = new AnalyticsViewModel();
            SettingsVM = new SettingsViewModel();
            AIVM = new AIViewModel();
            NutritionVM = new NutritionViewModel();

            // Chỉ cập nhật SelectedTab; setter sẽ gọi SelectTab
            SelectTabCommand = new RelayCommand<string>((tabName) =>
            {
                if (!string.IsNullOrWhiteSpace(tabName))
                {
                    SelectedTab = tabName;
                }
            });
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            CurrentUser = new User { Id = 1, Username = "DesignUser", Email = "design@foodbook.com" };
        }

        // 6. LOGIC NAVIGATION
        private async Task SelectTab(string tabName)
        {
            // Debounce rapid repeat navigations and reentrancy guard
            var now = DateTime.UtcNow;
            if (_isNavigatingTabs) return;
            if ((now - _lastNavigationAt).TotalMilliseconds < 100) return;
            _isNavigatingTabs = true;
            _lastNavigationAt = now;
            System.Diagnostics.Debug.WriteLine($"Shell navigating to: {tabName}");
            
            // Kích hoạt logic tải/refresh dữ liệu trên các ViewModel con
            switch (tabName)
            {
                case "Dashboard":
                    await AnalyticsVM.LoadDashboardDataAsync(); // Dashboard cần data của Analytics
                    break;
                case "Recipes":
                    await RecipeListVM.LoadRecipesAsync();
                    break;
                case "Ingredients":
                    await InventoryVM.LoadIngredientsAsync();
                    break;
                case "Analytics":
                    await AnalyticsVM.LoadAnalyticsDataAsync();
                    break;
                case "Settings":
                    await SettingsVM.LoadSettingsAsync();
                    break;
                case "AI":
                    await AIVM.LoadIngredientsForAIAsync(); // Tải nguyên liệu để dùng cho AI
                    break;
                case "Nutrition":
                    if (NutritionVM.LoadRecipesCommand.CanExecute(null))
                    {
                        NutritionVM.LoadRecipesCommand.Execute(null);
                    }
                    break;
            }
            _isNavigatingTabs = false;
        }
        
        private async Task LoadCurrentUserAsync()
        {
            try
            {
                if (_authService != null)
                {
                    var user = await _authService.GetCurrentUserAsync();
                    if (user != null)
                    {
                        CurrentUser = user;
                        return;
                    }
                }

                // Fallback (design/demo): keep existing demo if nothing in session
                if (CurrentUser == null)
                {
                    CurrentUser = new User { Id = 1, Username = "DemoUser", Email = "demo@foodbook.com" };
                }
            }
            catch
            {
                // Silent fallback to demo user to avoid UI break
                if (CurrentUser == null)
                {
                    CurrentUser = new User { Id = 1, Username = "DemoUser", Email = "demo@foodbook.com" };
                }
            }
        }
        
        private async Task ToggleSidebarAsync()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            await Task.CompletedTask;
        }

        // Tích hợp các hàm tiện ích còn lại (vd: BuildGreeting, RefreshLocalization)
        // ... (CÁC PHẦN CÒN LẠI NHƯ BuildGreeting, RefreshLocalization SẼ ĐƯỢC GIỮ NGUYÊN HOẶC CHUYỂN)
        public void RefreshLocalization()
        {
            OnPropertyChanged(nameof(Loc_Dashboard));
            OnPropertyChanged(nameof(Loc_MyRecipes));
            OnPropertyChanged(nameof(Loc_Ingredients));
            OnPropertyChanged(nameof(Loc_Analytics));
            OnPropertyChanged(nameof(Loc_Settings));
            OnPropertyChanged(nameof(Loc_AI));
        }
    }
}