using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.Views
{
    /// <summary>
    /// Interaction logic for GenerateRecipeDialog.xaml
    /// </summary>
    public partial class GenerateRecipeDialog : Window
    {
        public string DishName { get; private set; } = string.Empty;
        public int Servings { get; private set; } = 4;
        public string CustomPreferences { get; private set; } = string.Empty;
        public List<Ingredient> AvailableIngredients { get; set; } = new();

        private DispatcherTimer? _loadingDotsTimer;
        private int _dotCount = 0;

        public GenerateRecipeDialog(List<Ingredient> availableIngredients)
        {
            InitializeComponent();
            AvailableIngredients = availableIngredients ?? new List<Ingredient>();
            UpdateAvailableIngredientsDisplay();
        }

        private void UpdateAvailableIngredientsDisplay()
        {
            if (AvailableIngredients == null || !AvailableIngredients.Any())
            {
                AvailableIngredientsText.Text = "⚠️ Chưa có nguyên liệu trong pantry. Vui lòng thêm nguyên liệu vào 'My Ingredient Pantry' trước.";
                GenerateButton.IsEnabled = false;
            }
            else
            {
                var ingredientList = AvailableIngredients
                    .Where(i => i.Quantity.HasValue && i.Quantity.Value > 0)
                    .Select(i => $"{i.Name} ({i.Quantity} {i.Unit ?? "pcs"})")
                    .Take(10)
                    .ToList();
                
                var displayText = string.Join(", ", ingredientList);
                if (AvailableIngredients.Count > 10)
                {
                    displayText += $" và {AvailableIngredients.Count - 10} nguyên liệu khác...";
                }
                
                AvailableIngredientsText.Text = displayText;
                GenerateButton.IsEnabled = true;
            }
        }

        private void QuickPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string preset)
            {
                var currentPrefs = PreferencesTextBox.Text.Trim();
                if (string.IsNullOrEmpty(currentPrefs))
                {
                    PreferencesTextBox.Text = preset;
                }
                else
                {
                    PreferencesTextBox.Text = $"{currentPrefs}, {preset}";
                }
                PreferencesTextBox.Focus();
                PreferencesTextBox.CaretIndex = PreferencesTextBox.Text.Length;
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate servings
            if (!int.TryParse(ServingsTextBox.Text, out int servings) || servings <= 0)
            {
                MessageBox.Show("Vui lòng nhập số phần ăn hợp lệ (lớn hơn 0).", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ServingsTextBox.Focus();
                return;
            }

            DishName = DishNameTextBox.Text.Trim();
            Servings = servings;
            CustomPreferences = PreferencesTextBox.Text.Trim();

            // Show loading state
            ShowLoadingState();
            
            // DialogResult will be set by the caller after generation completes
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowLoadingState()
        {
            StatusBorder.Visibility = Visibility.Visible;
            GenerateButton.IsEnabled = false;
            DishNameTextBox.IsEnabled = false;
            ServingsTextBox.IsEnabled = false;
            PreferencesTextBox.IsEnabled = false;

            // Animate loading dots
            _loadingDotsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _loadingDotsTimer.Tick += LoadingDotsTimer_Tick;
            _loadingDotsTimer.Start();
        }

        private void LoadingDotsTimer_Tick(object? sender, EventArgs e)
        {
            _dotCount = (_dotCount % 3) + 1;
            LoadingDots.Text = new string('.', _dotCount);
        }

        protected override void OnClosed(EventArgs e)
        {
            _loadingDotsTimer?.Stop();
            base.OnClosed(e);
        }

        public void HideLoadingState()
        {
            StatusBorder.Visibility = Visibility.Collapsed;
            GenerateButton.IsEnabled = true;
            DishNameTextBox.IsEnabled = true;
            ServingsTextBox.IsEnabled = true;
            PreferencesTextBox.IsEnabled = true;
            _loadingDotsTimer?.Stop();
        }

        public void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }
    }
}

