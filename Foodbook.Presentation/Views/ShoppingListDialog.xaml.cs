using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Foodbook.Business.Interfaces;

namespace Foodbook.Presentation.Views
{
    public partial class ShoppingListDialog : Window
    {
        public ShoppingListDialog()
        {
            InitializeComponent();
        }

        public void SetShoppingList(ShoppingListResult shoppingList)
        {
            // Set summary information
            TotalItemsText.Text = shoppingList.TotalItems.ToString();
            EstimatedCostText.Text = $"${shoppingList.EstimatedCost:F2}";
            ShoppingTimeText.Text = $"{shoppingList.EstimatedShoppingTime.TotalMinutes:F0} min";

            // Set categories
            CategoriesList.ItemsSource = shoppingList.Categories;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Simple export functionality - could be enhanced
                MessageBox.Show("Export functionality would be implemented here.\n\n" +
                              "This would save the shopping list to a text file or PDF.", 
                              "Export Shopping List", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting shopping list: {ex.Message}", 
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
