// Foodbook.Presentation/ViewModels/AnalyticsViewModel.cs
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using Foodbook.Business.Interfaces;
using Foodbook.Data.Entities;
using System.Threading.Tasks;

namespace Foodbook.Presentation.ViewModels
{
    public class AnalyticsViewModel : BaseViewModel
    {
        // ... (Giữ các fields Chart và KPI liên quan: _totalRecipeCount, _mainDishesCount, _recipeTrendSeries, etc.)

        // Expose Chart Series and KPIs
        public ISeries[] RecipeTrendSeries { get; set; } = [];
        public ISeries[] AIPerformanceSeries { get; set; } = [];
        public ISeries[] CookTimeDistributionSeries { get; set; } = [];
        public int MainDishesCount { get; set; }
        public int DessertsCount { get; set; }
        public double PantryCoveragePercent { get; set; } // Cần được cập nhật từ InventoryVM nếu có thể
        // ... (Expose tất cả các properties khác cho Dashboard và Analytics View)

        public ICommand LoadAnalyticsCommand { get; }
        public ICommand RefreshDashboardCommand { get; }

        public AnalyticsViewModel(IRecipeService recipeService, IIngredientService ingredientService, ILoggingService loggingService)
        {
            // ... (Setup services)
            LoadAnalyticsCommand = new RelayCommand(async () => await LoadAnalyticsDataAsync());
            RefreshDashboardCommand = new RelayCommand(async () => await LoadAnalyticsDataAsync());
            // InitializeCharts(); // Khởi tạo chart trống
        }
        
        // Placeholder for Designtime
        public AnalyticsViewModel() { } 

        // public async Task LoadAnalyticsDataAsync() { /* ... Logic di chuyển từ MainViewModel ... */ }
        // private void CalculateDashboardChartData(IEnumerable<Recipe> recipes) { /* ... Logic di chuyển từ MainViewModel ... */ }
    }
}