using Foodbook.Business.Interfaces;
using Foodbook.Business.Models;
using Foodbook.Data.Entities;
using Foodbook.Presentation.Views;
using Foodbook.Presentation.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;

namespace Foodbook.Presentation.ViewModels
{
    public class NutritionViewModel : INotifyPropertyChanged
    {
        private readonly INutritionService _nutritionService;
        private readonly IAIService _aiService;
        private readonly IRecipeService _recipeService;

        // Properties for UI binding
        private bool _isLoading;
        private string _selectedRecipeId = string.Empty;
        private string _customRecipeText = string.Empty;
        private NutritionAnalysisResult? _analysisResult;
        private string _aiAssessment = string.Empty;
        private ObservableCollection<HealthAlert> _healthAlerts = new();
        private ObservableCollection<string> _recommendations = new();
        private ObservableCollection<Recipe> _recipes = new();
        private string _selectedDataSource = "AI"; // "Database" or "AI"
        private Recipe? _selectedRecipe;
        private string _userGoal = "Sức Khỏe Tổng Quát";
        private NutritionRecommendation? _nutritionRecommendation;

        // Commands
        public ICommand AnalyzeNutritionCommand { get; }
        public ICommand ClearAnalysisCommand { get; }
        public ICommand LoadRecipesCommand { get; }
        public ICommand GenerateAIAssessmentCommand { get; }

        public NutritionViewModel(INutritionService nutritionService, IAIService aiService, IRecipeService recipeService)
        {
            System.Diagnostics.Debug.WriteLine("=== NUTRITION VIEWMODEL CONSTRUCTOR STARTED ===");
            
            _nutritionService = nutritionService;
            _aiService = aiService;
            _recipeService = recipeService;

            // Initialize default values
            SelectedDataSource = "AI"; // Default to AI mode
            UserGoal = "Sức Khỏe Tổng Quát";
            
            System.Diagnostics.Debug.WriteLine($"Initialized with DataSource: {SelectedDataSource}, Goal: {UserGoal}");

            // Initialize commands
            AnalyzeNutritionCommand = new RelayCommand(async () => await AnalyzeNutritionAsync(), () => !IsLoading);
            ClearAnalysisCommand = new RelayCommand(async () => { ClearAnalysis(); await Task.CompletedTask; });
            LoadRecipesCommand = new RelayCommand(async () => await LoadRecipesAsync());
            GenerateAIAssessmentCommand = new RelayCommand(async () => await GenerateAIAssessmentAsync(), () => AnalysisResult != null);

            System.Diagnostics.Debug.WriteLine("Commands initialized successfully");

            // Defer recipe loading to when the Nutrition tab is selected to avoid
            // concurrent DbContext operations during app startup.
            System.Diagnostics.Debug.WriteLine("=== NUTRITION VIEWMODEL CONSTRUCTOR COMPLETED (deferred load) ===");
        }

        // Design-time / default constructor
        public NutritionViewModel()
        {
            _nutritionService = null!;
            _aiService = null!;
            _recipeService = null!;
            AnalyzeNutritionCommand = new RelayCommand(async () => await Task.CompletedTask, () => true);
            ClearAnalysisCommand = new RelayCommand(async () => await Task.CompletedTask);
            LoadRecipesCommand = new RelayCommand(async () => await Task.CompletedTask);
            GenerateAIAssessmentCommand = new RelayCommand(async () => await Task.CompletedTask, () => true);
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        public bool IsNotLoading => !IsLoading;

        public string SelectedRecipeId
        {
            get => _selectedRecipeId;
            set
            {
                _selectedRecipeId = value;
                OnPropertyChanged();
            }
        }

        public string CustomRecipeText
        {
            get => _customRecipeText;
            set
            {
                _customRecipeText = value;
                OnPropertyChanged();
            }
        }

        public NutritionAnalysisResult? AnalysisResult
        {
            get => _analysisResult;
            set
            {
                _analysisResult = value;
                OnPropertyChanged();
            }
        }

        public string AIAssessment
        {
            get => _aiAssessment;
            set
            {
                _aiAssessment = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<HealthAlert> HealthAlerts
        {
            get => _healthAlerts;
            set
            {
                _healthAlerts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Recommendations
        {
            get => _recommendations;
            set
            {
                _recommendations = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Recipe> Recipes
        {
            get => _recipes;
            set
            {
                _recipes = value;
                OnPropertyChanged();
            }
        }

        public string SelectedDataSource
        {
            get => _selectedDataSource;
            set
            {
                _selectedDataSource = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"SelectedDataSource changed to: {value}");
            }
        }

        public Recipe? SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                _selectedRecipe = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"SelectedRecipe changed to: {value?.Title}");
            }
        }

        public string UserGoal
        {
            get => _userGoal;
            set
            {
                _userGoal = value;
                OnPropertyChanged();
            }
        }

        public NutritionRecommendation? NutritionRecommendation
        {
            get => _nutritionRecommendation;
            set
            {
                _nutritionRecommendation = value;
                OnPropertyChanged();
            }
        }

        // Properties for nutrition values display
        public double TotalCalories => (double)(AnalysisResult?.TotalCalories ?? 0);
        public double TotalProtein => (double)(AnalysisResult?.TotalProtein ?? 0);
        public double TotalCarbs => (double)(AnalysisResult?.TotalCarbs ?? 0);
        public double TotalFat => (double)(AnalysisResult?.TotalFat ?? 0);

        public ObservableCollection<string> NutritionRecommendations
        {
            get => _recommendations;
            set
            {
                _recommendations = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands Implementation

        private async Task AnalyzeNutritionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== NUTRITION ANALYSIS STARTED ===");
                System.Diagnostics.Debug.WriteLine($"DataSource: {SelectedDataSource}");
                System.Diagnostics.Debug.WriteLine($"SelectedRecipe: {SelectedRecipe?.Title}");
                System.Diagnostics.Debug.WriteLine($"CustomText: {CustomRecipeText}");
                System.Diagnostics.Debug.WriteLine($"Recipes Count: {Recipes.Count}");
                
                IsLoading = true;
                AnalysisResult = null;
                AIAssessment = string.Empty;
                HealthAlerts.Clear();
                Recommendations.Clear();

                // Smart logic: Check data source and available data
                if (SelectedDataSource == "Database" && SelectedRecipe != null)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Using Database mode with selected recipe");
                    // Pha 2a: Luồng CSDL - Tính toán từ dữ liệu có cấu trúc
                    await AnalyzeFromDatabaseAsync();
                }
                else if (SelectedDataSource == "AI" && !string.IsNullOrEmpty(CustomRecipeText))
                {
                    System.Diagnostics.Debug.WriteLine("✅ Using AI mode with custom recipe text");
                    // Pha 2b: Luồng AI Parsing - Giải mã văn bản tùy ý
                    await AnalyzeWithAIAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Invalid state detected:");
                    System.Diagnostics.Debug.WriteLine($"  - DataSource: {SelectedDataSource}");
                    System.Diagnostics.Debug.WriteLine($"  - SelectedRecipe: {SelectedRecipe?.Title ?? "null"}");
                    System.Diagnostics.Debug.WriteLine($"  - CustomText: '{CustomRecipeText}' (empty: {string.IsNullOrEmpty(CustomRecipeText)})");
                    AIAssessment = "❌ Vui lòng chọn công thức từ danh sách hoặc nhập công thức tùy chỉnh.";
                    return;
                }

                // Pha 3: Đánh giá AI và Phản hồi
                if (AnalysisResult != null)
                {
                    System.Diagnostics.Debug.WriteLine("✅ AnalysisResult created, generating additional data...");
                    await GenerateHealthAlertsAsync();
                    await GenerateRecommendationsAsync();
                    await GenerateAIAssessmentAsync();
                    
                    // Trigger property change notifications for nutrition values
                    OnPropertyChanged(nameof(TotalCalories));
                    OnPropertyChanged(nameof(TotalProtein));
                    OnPropertyChanged(nameof(TotalCarbs));
                    OnPropertyChanged(nameof(TotalFat));
                    
                    // Hiển thị kết quả trong Result Panel
                    System.Diagnostics.Debug.WriteLine("✅ Showing nutrition results in Result Panel...");
                    ShowNutritionResults();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ AnalysisResult is null - no results to show");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception in AnalyzeNutritionAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Handle errors gracefully
                AIAssessment = $"❌ Error during analysis: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("=== NUTRITION ANALYSIS COMPLETED ===");
            }
        }

        private async Task AnalyzeFromDatabaseAsync()
        {
            // Pha 2a: Luồng CSDL - Tính toán Macro/Calo từ FoodBook.sql
            System.Diagnostics.Debug.WriteLine($"Starting database analysis for Recipe: {SelectedRecipe?.Title}");
            if (SelectedRecipe != null)
            {
                System.Diagnostics.Debug.WriteLine($"Analyzing recipe from FoodBook.sql: {SelectedRecipe.Title}");
                AnalysisResult = await _nutritionService.AnalyzeRecipeNutritionAsync(SelectedRecipe);
                System.Diagnostics.Debug.WriteLine($"Database analysis completed. Calories: {AnalysisResult?.TotalCalories}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"SelectedRecipe is null");
            }
        }

        private async Task AnalyzeWithAIAsync()
        {
            // Pha 2b: Luồng AI Parsing - Giải mã văn bản tùy ý bằng AI
            System.Diagnostics.Debug.WriteLine($"Starting AI analysis for: {CustomRecipeText}");
            AnalysisResult = await _nutritionService.AnalyzeCustomRecipeAsync(CustomRecipeText, UserGoal);
            System.Diagnostics.Debug.WriteLine($"AI analysis completed. Calories: {AnalysisResult?.TotalCalories}");
        }

        private async Task GenerateHealthAlertsAsync()
        {
            if (AnalysisResult != null)
            {
                var alerts = await _nutritionService.GetHealthAlertsAsync(AnalysisResult);
                HealthAlerts.Clear();
                foreach (var alert in alerts)
                {
                    HealthAlerts.Add(alert);
                }
            }
        }

        private async Task GenerateRecommendationsAsync()
        {
            if (AnalysisResult != null)
            {
                var recommendation = await _nutritionService.GetNutritionRecommendationsAsync(AnalysisResult, UserGoal);
                NutritionRecommendation = recommendation;
                
                Recommendations.Clear();
                foreach (var rec in recommendation.Suggestions)
                {
                    Recommendations.Add(rec);
                }
            }
        }

        private async Task GenerateAIAssessmentAsync()
        {
            if (AnalysisResult != null)
            {
                // Pha 3: Đánh giá AI - Sinh ra báo cáo đánh giá thông qua NutritionService
                System.Diagnostics.Debug.WriteLine("Generating AI health assessment...");
                var assessment = await _nutritionService.GenerateHealthFeedbackAsync(AnalysisResult);
                AIAssessment = assessment;
                System.Diagnostics.Debug.WriteLine($"AI assessment completed. Length: {assessment?.Length ?? 0} characters");
            }
        }

        private string GenerateNutritionSummary()
        {
            if (AnalysisResult == null) return string.Empty;

            return $"Nutrition Analysis Summary:\n\n" +
                   $"📊 Total Calories: {AnalysisResult.TotalCalories:F0}\n" +
                   $"🥩 Protein: {AnalysisResult.TotalProtein:F1}g\n" +
                   $"🍞 Carbohydrates: {AnalysisResult.TotalCarbs:F1}g\n" +
                   $"🥑 Fat: {AnalysisResult.TotalFat:F1}g\n" +
                   $"🌾 Fiber: {AnalysisResult.TotalFiber:F1}g\n" +
                   $"🧂 Sodium: {AnalysisResult.TotalSodium:F0}mg\n" +
                   $"⭐ Overall Grade: {AnalysisResult.Rating.Grade} ({AnalysisResult.Rating.OverallScore}/100)\n" +
                   $"📝 Description: {AnalysisResult.Rating.Description}";
        }

        private void ClearAnalysis()
        {
            // Clear analysis results
            AnalysisResult = null;
            AIAssessment = string.Empty;
            HealthAlerts.Clear();
            Recommendations.Clear();
            NutritionRecommendation = null;
            
            // Reset to default values
            SelectedDataSource = "AI";
            SelectedRecipe = null;
            CustomRecipeText = string.Empty;
            UserGoal = "Sức Khỏe Tổng Quát";
            
            // Trigger property change notifications for nutrition values
            OnPropertyChanged(nameof(TotalCalories));
            OnPropertyChanged(nameof(TotalProtein));
            OnPropertyChanged(nameof(TotalCarbs));
            OnPropertyChanged(nameof(TotalFat));
            
            System.Diagnostics.Debug.WriteLine("Analysis cleared and reset to default values");
        }

        private async Task LoadRecipesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading recipes from FoodBook.sql...");
                var recipes = await _recipeService.GetAllRecipesAsync();
                
                // Use Dispatcher to update UI thread safely
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in recipes)
                    {
                        Recipes.Add(recipe);
                    }
                });
                
                System.Diagnostics.Debug.WriteLine($"Loaded {Recipes.Count} recipes from FoodBook.sql");
                
                // Show success message if recipes were loaded
                if (Recipes.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Successfully loaded {Recipes.Count} recipes from database");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No recipes found in database");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error loading recipes from FoodBook.sql: {ex.Message}");
                // You could also show a user-friendly error message here
            }
        }

        #endregion

        #region UI Methods

        private void ShowNutritionResults()
        {
            if (AnalysisResult == null) 
            {
                System.Diagnostics.Debug.WriteLine("No analysis result to display");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Showing nutrition results for DataSource: {SelectedDataSource}");
                System.Diagnostics.Debug.WriteLine($"Calories: {AnalysisResult.TotalCalories}, Protein: {AnalysisResult.TotalProtein}g");
                
                // Hiển thị kết quả trong cột phải của view (Result Panel)
                System.Diagnostics.Debug.WriteLine("Nutrition analysis completed - results displayed in Result Panel");
            }
            catch {
                // Handle error silently
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
