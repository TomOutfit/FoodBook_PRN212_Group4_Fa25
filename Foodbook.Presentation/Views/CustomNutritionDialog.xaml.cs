using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Foodbook.Business.Interfaces;

namespace Foodbook.Presentation.Views
{
    public partial class CustomNutritionDialog : Window
    {
        private NutritionAnalysisResult? _nutritionAnalysis;
        private IEnumerable<HealthAlert>? _healthAlerts;
        private IEnumerable<NutritionRecommendation>? _recommendations;

        public CustomNutritionDialog()
        {
            InitializeComponent();
        }

        public void SetNutritionAnalysis(
            NutritionAnalysisResult nutritionAnalysis, 
            IEnumerable<HealthAlert> healthAlerts, 
            IEnumerable<NutritionRecommendation> recommendations)
        {
            _nutritionAnalysis = nutritionAnalysis;
            _healthAlerts = healthAlerts;
            _recommendations = recommendations;

            // Update UI with nutrition data
            UpdateNutritionDisplay();
            
            // Show results panel
            ResultsPanel.Visibility = Visibility.Visible;
        }

        private void UpdateNutritionDisplay()
        {
            if (_nutritionAnalysis == null) return;

            // Update nutrition summary
            TotalCaloriesText.Text = $"{_nutritionAnalysis.TotalCalories:F0}";
            TotalProteinText.Text = $"{_nutritionAnalysis.TotalProtein:F1}g";
            TotalCarbsText.Text = $"{_nutritionAnalysis.TotalCarbs:F1}g";
            TotalFatText.Text = $"{_nutritionAnalysis.TotalFat:F1}g";

            // Update AI assessment
            AIAssessmentText.Text = _nutritionAnalysis.AnalysisSummary;

            // Update health alerts
            if (_healthAlerts != null)
            {
                HealthAlertsList.ItemsSource = _healthAlerts.Select(alert => new
                {
                    Type = alert.Type,
                    Message = alert.Message
                });
            }

            // Update recommendations
            if (_recommendations != null && _recommendations.Any())
            {
                var rec = _recommendations.First();
                var recommendationsText = string.Join("\n", rec.Suggestions.Select(s => $"• {s}"));
                if (rec.FoodsToAdd.Any())
                {
                    recommendationsText += "\n\nFoods to add:\n" + string.Join(", ", rec.FoodsToAdd);
                }
                if (rec.FoodsToReduce.Any())
                {
                    recommendationsText += "\n\nFoods to reduce:\n" + string.Join(", ", rec.FoodsToReduce);
                }
                RecommendationsText.Text = recommendationsText;
            }
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RecipeTextBox.Text))
            {
                MessageBox.Show("Please enter a recipe text to analyze.", "Input Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // This will be handled by the ViewModel
            // DialogResult = true; // Removed - this causes the exception
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_nutritionAnalysis == null)
            {
                MessageBox.Show("No analysis data to export.", "No Data", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var exportText = GenerateExportText();
                
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"Nutrition_Analysis_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, exportText);
                    MessageBox.Show("Nutrition analysis exported successfully!", "Export Complete", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateExportText()
        {
            if (_nutritionAnalysis == null) return "";

            var export = $"🤖 AI Nutrition Analysis Report\n";
            export += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n";
            
            export += $"📊 NUTRITION SUMMARY\n";
            export += $"Calories: {_nutritionAnalysis.TotalCalories:F0} kcal\n";
            export += $"Protein: {_nutritionAnalysis.TotalProtein:F1}g\n";
            export += $"Carbohydrates: {_nutritionAnalysis.TotalCarbs:F1}g\n";
            export += $"Fat: {_nutritionAnalysis.TotalFat:F1}g\n";
            export += $"Fiber: {_nutritionAnalysis.TotalFiber:F1}g\n";
            export += $"Sugar: {_nutritionAnalysis.TotalSugar:F1}g\n";
            export += $"Sodium: {_nutritionAnalysis.TotalSodium:F0}mg\n\n";
            
            export += $"🤖 AI HEALTH ASSESSMENT\n";
            export += $"{_nutritionAnalysis.AnalysisSummary}\n\n";
            
            if (_healthAlerts != null && _healthAlerts.Any())
            {
                export += $"⚠️ HEALTH ALERTS\n";
                foreach (var alert in _healthAlerts)
                {
                    export += $"{alert.Type}: {alert.Message}\n";
                }
                export += "\n";
            }
            
            if (_recommendations != null && _recommendations.Any())
            {
                export += $"💡 RECOMMENDATIONS\n";
                var rec = _recommendations.First();
                foreach (var suggestion in rec.Suggestions)
                {
                    export += $"• {suggestion}\n";
                }
                export += "\n";
            }
            
            return export;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
