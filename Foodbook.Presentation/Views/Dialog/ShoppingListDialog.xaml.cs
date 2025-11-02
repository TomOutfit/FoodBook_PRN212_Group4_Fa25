using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Foodbook.Business.Interfaces;
using System.Threading.Tasks;
using System.IO;
using Foodbook.Presentation.ViewModels;

namespace Foodbook.Presentation.Views
{
    public partial class ShoppingListDialog : Window
    {
        private ShoppingListResult? _currentShoppingList;
        private readonly IShoppingListService? _shoppingListService;
        private ShoppingListDialogViewModel? _viewModel;

        public ShoppingListDialog()
        {
            InitializeComponent();
        }

        public ShoppingListDialog(IShoppingListService shoppingListService, IIngredientService? ingredientService = null, int userId = 1) : this()
        {
            _shoppingListService = shoppingListService;
            _viewModel = new ShoppingListDialogViewModel(shoppingListService, ingredientService, userId);
            _viewModel.CloseRequested += () => { this.DialogResult = true; this.Close(); };
            _viewModel.GenerateRequested += () => { /* Refresh UI if needed */ };
            DataContext = _viewModel;
            
            // Handle generate errors
            Loaded += Window_Loaded;
        }

        public void SetShoppingList(ShoppingListResult shoppingList)
        {
            _currentShoppingList = shoppingList;
            
            // Use ViewModel if available
            if (_viewModel != null)
            {
                _viewModel.SetShoppingList(shoppingList);
            }
            else
            {
                // Fallback to direct UI update
                if (TotalItemsText != null)
                    TotalItemsText.Text = shoppingList.TotalItems.ToString();
                if (EstimatedCostText != null)
                    EstimatedCostText.Text = $"${shoppingList.EstimatedCost:F2}";
                if (ShoppingTimeText != null)
                    ShoppingTimeText.Text = $"{shoppingList.EstimatedShoppingTime.TotalMinutes:F0} min";

                // Set categories with enhanced information
                if (CategoriesList != null)
                    CategoriesList.ItemsSource = shoppingList.Categories;
                
                // Set tips and suggestions
                if (TipsList != null)
                    TipsList.ItemsSource = shoppingList.Tips;
                if (SuggestionsList != null)
                    SuggestionsList.ItemsSource = shoppingList.StoreSuggestions;
            }

            // Update window title with list name
            if (!string.IsNullOrEmpty(shoppingList.ListName))
            {
                this.Title = $"🛒 {shoppingList.ListName}";
            }

            // Show potential savings if available
            if (shoppingList.PotentialSavings > 0 && TotalItemsText != null)
            {
                if (_viewModel == null && EstimatedCostText != null)
                {
                    EstimatedCostText.Text += $" (Save: ${shoppingList.PotentialSavings:F2})";
                    EstimatedCostText.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
        }

        private ShoppingListResult? GetCurrentShoppingList()
        {
            // Try to get from ViewModel first
            if (_viewModel?.ShoppingList != null)
                return _viewModel.ShoppingList;
            
            // Fallback to _currentShoppingList
            return _currentShoppingList;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Show export options panel
            if (_viewModel != null)
            {
                _viewModel.ShowExportOptions = true;
            }
        }

        private async void ConfirmExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shoppingList = GetCurrentShoppingList();
                if (shoppingList == null)
                {
                    MessageBox.Show("No shopping list to export.", "Export Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_viewModel == null || _shoppingListService == null) return;

                var exportType = _viewModel.SelectedExportType;
                var fileName = _viewModel.ExportFileName;

                ExportType exportTypeEnum = ExportType.TextFile;
                if (exportType == "Notes Integration")
                    exportTypeEnum = ExportType.Notes;
                else if (exportType == "PDF Document")
                    exportTypeEnum = ExportType.PDF;

                switch (exportTypeEnum)
                {
                    case ExportType.Notes:
                        var notesFileName = await _shoppingListService.ExportShoppingListToNotesAsync(shoppingList, fileName);
                        MessageBox.Show($"Shopping list exported to Notes as: {notesFileName}", 
                                      "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;

                    case ExportType.TextFile:
                        await ExportToTextFile(fileName, shoppingList);
                        break;

                    case ExportType.PDF:
                        MessageBox.Show("PDF export functionality would be implemented here.", 
                                      "PDF Export", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }

                // Hide export options panel
                if (_viewModel != null)
                {
                    _viewModel.ShowExportOptions = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting shopping list: {ex.Message}", 
                              "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelExport_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.ShowExportOptions = false;
            }
        }

        private void ConfirmIngredientSelection_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null || IngredientsListBox == null) return;

            var selected = IngredientsListBox.SelectedItems.Cast<string>().ToList();
            if (!selected.Any())
            {
                MessageBox.Show("Vui lòng chọn ít nhất một nguyên liệu.", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _viewModel.SelectedIngredients = new System.Collections.ObjectModel.ObservableCollection<string>(selected);
            _viewModel.ShowIngredientSelection = false;
            
            // Trigger generate
            _viewModel.GenerateCommand.Execute(null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure DataContext is set
            if (DataContext == null && _viewModel != null)
            {
                DataContext = _viewModel;
            }
        }

        private async Task ExportToTextFile(string fileName, ShoppingListResult shoppingList)
        {
            try
            {
                var content = GenerateTextContent(shoppingList);
                
                // Kiểm tra và tạo thư mục trên ổ D
                string driveD = "D:\\";
                string exportFolder = Path.Combine(driveD, "ShoppingLists");
                
                // Kiểm tra xem ổ D có tồn tại không
                if (!Directory.Exists(driveD))
                {
                    MessageBox.Show("Ổ D không tồn tại. Đang lưu vào Desktop thay thế.", 
                                  "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    exportFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
                else
                {
                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(exportFolder))
                    {
                        Directory.CreateDirectory(exportFolder);
                    }
                }
                
                var filePath = Path.Combine(exportFolder, $"{fileName}.txt");
                
                await File.WriteAllTextAsync(filePath, content);
                
                MessageBox.Show($"✅ Shopping list đã được xuất thành công!\n\nĐường dẫn: {filePath}", 
                              "Xuất File Thành Công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi lưu file: {ex.Message}", 
                              "Lỗi Xuất File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateTextContent(ShoppingListResult shoppingList)
        {
            var content = new System.Text.StringBuilder();
            
            content.AppendLine($"🛒 SMART SHOPPING LIST - {shoppingList.GeneratedAt:dd/MM/yyyy HH:mm}");
            content.AppendLine($"📋 Generated for: {string.Join(", ", shoppingList.RecipeNames)}");
            content.AppendLine($"💰 Estimated Cost: ${shoppingList.EstimatedCost:F2}");
            content.AppendLine($"⏱️ Estimated Time: {shoppingList.EstimatedShoppingTime.TotalMinutes:F0} minutes");
            content.AppendLine($"📦 Total Items: {shoppingList.TotalItems}");
            content.AppendLine();
            
            foreach (var category in shoppingList.Categories)
            {
                content.AppendLine($"🏷️ {category.Name} ({category.Icon}) - {category.StoreSection}");
                content.AppendLine($"   Total: ${category.CategoryTotal:F2} | Items: {category.ItemCount}");
                content.AppendLine($"   {category.ShoppingOrder}");
                content.AppendLine();
                
                foreach (var item in category.Items)
                {
                    var priority = item.Priority == 1 ? "🔥" : item.Priority == 2 ? "⭐" : "📝";
                    var bulk = item.IsBulkPurchase ? " (BULK)" : "";
                    var checkedStatus = item.IsChecked ? "✅" : "⬜";
                    
                    content.AppendLine($"   {checkedStatus} {priority} {item.Name} - {item.Quantity} {item.Unit} - ${item.EstimatedPrice:F2}{bulk}");
                    
                    if (!string.IsNullOrEmpty(item.Notes))
                        content.AppendLine($"      💡 {item.Notes}");
                    
                    if (item.Substitutions.Any())
                        content.AppendLine($"      🔄 Alternatives: {string.Join(", ", item.Substitutions)}");
                    
                    if (!string.IsNullOrEmpty(item.NutritionalInfo))
                        content.AppendLine($"      🥗 {item.NutritionalInfo}");
                    
                    content.AppendLine();
                }
                content.AppendLine();
            }
            
            if (shoppingList.StoreSuggestions.Any())
            {
                content.AppendLine("🗺️ STORE NAVIGATION TIPS:");
                foreach (var suggestion in shoppingList.StoreSuggestions)
                {
                    content.AppendLine($"   • {suggestion}");
                }
                content.AppendLine();
            }
            
            if (shoppingList.Tips.Any())
            {
                content.AppendLine("💡 SHOPPING TIPS:");
                foreach (var tip in shoppingList.Tips)
                {
                    content.AppendLine($"   • {tip}");
                }
                content.AppendLine();
            }
            
            if (shoppingList.PotentialSavings > 0)
            {
                content.AppendLine($"💰 POTENTIAL SAVINGS: ${shoppingList.PotentialSavings:F2}");
                content.AppendLine();
            }
            
            content.AppendLine("Generated by FoodBook Smart Shopping List 🍽️");
            
            return content.ToString();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ItemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is ShoppingItem item)
            {
                item.IsChecked = checkBox.IsChecked == true;
                
                // Update ViewModel if available
                if (_viewModel != null)
                {
                    _viewModel.ToggleItemChecked(item);
                }
                
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            var shoppingList = GetCurrentShoppingList();
            if (shoppingList == null) return;

            var checkedItems = shoppingList.Items.Count(item => item.IsChecked);
            var totalItems = shoppingList.TotalItems;
            var progressPercentage = totalItems > 0 ? (double)checkedItems / totalItems * 100 : 0;

            // Update progress display (you could add a progress bar to the UI)
            Console.WriteLine($"Shopping Progress: {checkedItems}/{totalItems} ({progressPercentage:F1}%)");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Search is now handled by ViewModel through binding
            // This method can be kept for backward compatibility if needed
            if (_viewModel != null)
            {
                _viewModel.FilterItems();
            }
            else
            {
                // Fallback to direct filtering if ViewModel is not available
                try
                {
                    var searchText = SearchTextBox.Text?.Trim();
                var shoppingList = GetCurrentShoppingList();
                if (string.IsNullOrEmpty(searchText))
                {
                    if (CategoriesList != null)
                        CategoriesList.ItemsSource = shoppingList?.Categories;
                    return;
                }

                if (shoppingList == null) return;

                    var filteredCategories = shoppingList.Categories
                        .Where(category => 
                            category.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                            category.Items.Any(item => 
                                item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                item.Notes?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true))
                        .Select(category => new
                        {
                            category.Name,
                            category.Icon,
                            category.StoreSection,
                            category.CategoryTotal,
                            category.ItemCount,
                            category.ShoppingOrder,
                            Items = category.Items.Where(item => 
                                item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                item.Notes?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                                .ToList()
                        })
                        .Where(category => category.Items.Any())
                        .ToList();

                    CategoriesList.ItemsSource = filteredCategories;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching: {ex.Message}", "Search Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shoppingList = GetCurrentShoppingList();
                if (shoppingList == null)
                {
                    MessageBox.Show("No shopping list to print. Please generate a shopping list first.", "Print Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Print button clicked. Shopping list has {shoppingList.TotalItems} items.");

                // Create a print dialog
                var printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Generate print content
                    var printContent = GeneratePrintContent(shoppingList);
                    
                    // Create a FlowDocument for printing
                    var flowDoc = new System.Windows.Documents.FlowDocument();
                    flowDoc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(printContent)));
                    
                    // Print the document
                    printDialog.PrintDocument(((IDocumentPaginatorSource)flowDoc).DocumentPaginator, "Shopping List");
                    
                    MessageBox.Show("Shopping list sent to printer successfully!", "Print Successful", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing shopping list: {ex.Message}", "Print Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GeneratePrintContent(ShoppingListResult shoppingList)
        {
            var content = new System.Text.StringBuilder();
            
            content.AppendLine($"🛒 SMART SHOPPING LIST - {shoppingList.GeneratedAt:dd/MM/yyyy HH:mm}");
            content.AppendLine($"📋 Generated for: {string.Join(", ", shoppingList.RecipeNames)}");
            content.AppendLine($"💰 Estimated Cost: ${shoppingList.EstimatedCost:F2}");
            content.AppendLine($"⏱️ Estimated Time: {shoppingList.EstimatedShoppingTime.TotalMinutes:F0} minutes");
            content.AppendLine($"📦 Total Items: {shoppingList.TotalItems}");
            content.AppendLine();
            
            foreach (var category in shoppingList.Categories)
            {
                content.AppendLine($"🏷️ {category.Name} ({category.Icon}) - {category.StoreSection}");
                content.AppendLine($"   Total: ${category.CategoryTotal:F2} | Items: {category.ItemCount}");
                content.AppendLine($"   {category.ShoppingOrder}");
                content.AppendLine();
                
                foreach (var item in category.Items)
                {
                    var priority = item.Priority == 1 ? "🔥" : item.Priority == 2 ? "⭐" : "📝";
                    var bulk = item.IsBulkPurchase ? " (BULK)" : "";
                    var checkedStatus = item.IsChecked ? "✅" : "⬜";
                    
                    content.AppendLine($"   {checkedStatus} {priority} {item.Name} - {item.Quantity} {item.Unit} - ${item.EstimatedPrice:F2}{bulk}");
                    
                    if (!string.IsNullOrEmpty(item.Notes))
                        content.AppendLine($"      💡 {item.Notes}");
                    
                    if (item.Substitutions.Any())
                        content.AppendLine($"      🔄 Alternatives: {string.Join(", ", item.Substitutions)}");
                    
                    if (!string.IsNullOrEmpty(item.NutritionalInfo))
                        content.AppendLine($"      🥗 {item.NutritionalInfo}");
                    
                    content.AppendLine();
                }
                content.AppendLine();
            }
            
            if (shoppingList.StoreSuggestions.Any())
            {
                content.AppendLine("🗺️ STORE NAVIGATION TIPS:");
                foreach (var suggestion in shoppingList.StoreSuggestions)
                {
                    content.AppendLine($"   • {suggestion}");
                }
                content.AppendLine();
            }
            
            if (shoppingList.Tips.Any())
            {
                content.AppendLine("💡 SHOPPING TIPS:");
                foreach (var tip in shoppingList.Tips)
                {
                    content.AppendLine($"   • {tip}");
                }
                content.AppendLine();
            }
            
            if (shoppingList.PotentialSavings > 0)
            {
                content.AppendLine($"💰 POTENTIAL SAVINGS: ${shoppingList.PotentialSavings:F2}");
                content.AppendLine();
            }
            
            content.AppendLine("Generated by FoodBook Smart Shopping List 🍽️");
            
            return content.ToString();
        }
    }

    public enum ExportType
    {
        TextFile,
        Notes,
        PDF
    }
}
