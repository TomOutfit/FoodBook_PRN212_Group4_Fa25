using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Foodbook.Business.Interfaces;

namespace Foodbook.Presentation.Views
{
    public partial class NutritionAnalysisDialog : Window
    {
        public NutritionAnalysisDialog()
        {
            InitializeComponent();
        }

        public void SetNutritionAnalysis(NutritionAnalysisResult analysis, IEnumerable<HealthAlert> alerts, IEnumerable<NutritionRecommendation> recommendations)
        {
            // Set nutrition summary
            TotalCaloriesText.Text = analysis.TotalCalories.ToString("F0");
            TotalProteinText.Text = $"{analysis.TotalProtein:F1}g";
            TotalCarbsText.Text = $"{analysis.TotalCarbs:F1}g";
            TotalFatText.Text = $"{analysis.TotalFat:F1}g";

            // Set nutrition grade
            GradeText.Text = analysis.Rating.Grade;
            GradeDescriptionText.Text = analysis.Rating.Description;
            GradeScoreText.Text = $"Overall Score: {analysis.Rating.OverallScore}/100";

            // Set health alerts
            HealthAlertsList.ItemsSource = alerts;

            // Set recommendations
            var recommendationText = string.Join("\n• ", recommendations.SelectMany(r => r.Suggestions));
            RecommendationsText.Text = "• " + recommendationText;

            // Set detailed nutrition facts - create a list from vitamins and minerals
            var nutritionFacts = new List<object>();
            nutritionFacts.AddRange(analysis.Vitamins.Select(v => new { Name = v.Name, Amount = v.Amount, Unit = v.Unit, Percentage = v.DailyValue }));
            nutritionFacts.AddRange(analysis.Minerals.Select(m => new { Name = m.Name, Amount = m.Amount, Unit = m.Unit, Percentage = m.DailyValue }));
            NutritionFactsList.ItemsSource = nutritionFacts;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Simple export functionality - could be enhanced
                MessageBox.Show("Export functionality would be implemented here.\n\n" +
                              "This would save the nutrition analysis to a PDF report.", 
                              "Export Nutrition Report", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting nutrition report: {ex.Message}", 
                              "Export Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
