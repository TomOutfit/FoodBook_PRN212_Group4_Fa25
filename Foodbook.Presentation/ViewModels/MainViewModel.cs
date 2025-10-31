// Foodbook.Presentation/ViewModels/MainViewModel.cs (RÚT GỌN)
using System.Collections.ObjectModel;
using System.Windows.Input;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using System.Threading.Tasks;
using System;

namespace Foodbook.Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // 1. SERVICES (Cần cho việc khởi tạo các ViewModel con & Localization)
        private readonly ILocalizationService? _localizationService;
        private readonly IUserService _userService;
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
        public string SelectedTab
        {
            get => _selectedTab;
            set
            {
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
            // ... (Initialize other services)

            // 4. KHỞI TẠO CÁC VIEWMODEL CON VỚI SERVICES CHUYÊN BIỆT
            RecipeListVM = new RecipeListViewModel(recipeService, loggingService);
            InventoryVM = new InventoryViewModel(ingredientService, loggingService);
            AnalyticsVM = new AnalyticsViewModel(recipeService, ingredientService, loggingService);
            SettingsVM = new SettingsViewModel(settingsService, localizationService);
            AIVM = new AIViewModel(aiService, shoppingListService, loggingService, userService, InventoryVM);
            NutritionVM = new NutritionViewModel(nutritionService, aiService, recipeService);
            
            // 5. THIẾT LẬP CÁC COMMAND CHÍNH
            SelectTabCommand = new RelayCommand<string>(async (tabName) => await SelectTab(tabName));
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            
            // Bắt đầu tải dữ liệu chung
            _ = LoadCurrentUserAsync();
        }

        // Khởi tạo không tham số cho Designtime (nếu cần)
        public MainViewModel() 
        {
            // Thiết lập giá trị mặc định cho Design View
            RecipeListVM = new RecipeListViewModel();
            InventoryVM = new InventoryViewModel();
            AnalyticsVM = new AnalyticsViewModel();
            SettingsVM = new SettingsViewModel();
            AIVM = new AIViewModel();
            NutritionVM = new NutritionViewModel();

            SelectTabCommand = new RelayCommand<string>(async (tabName) => await SelectTab(tabName));
            ToggleSidebarCommand = new RelayCommand(async () => await ToggleSidebarAsync());
            CurrentUser = new User { Id = 1, Username = "DesignUser", Email = "design@foodbook.com" };
        }

        // 6. LOGIC NAVIGATION
        private async Task SelectTab(string tabName)
        {
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
            }
        }
        
        private async Task LoadCurrentUserAsync()
        {
            // Logic tải User (chuyển từ file gốc)
            // ... (giữ lại logic BuildGreeting và set CurrentUser)
            await Task.Delay(1); // placeholder
            CurrentUser = new User { Id = 1, Username = "DemoUser", Email = "demo@foodbook.com" };
        }
        
        private async Task ToggleSidebarAsync()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            await Task.CompletedTask;
        }

        // Tích hợp các hàm tiện ích còn lại (vd: BuildGreeting, RefreshLocalization)
        // ... (CÁC PHẦN CÒN LẠI NHƯ BuildGreeting, RefreshLocalization SẼ ĐƯỢC GIỮ NGUYÊN HOẶC CHUYỂN)
    }
}