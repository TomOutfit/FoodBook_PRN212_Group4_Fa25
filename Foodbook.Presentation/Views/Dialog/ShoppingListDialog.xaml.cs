using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Foodbook.Business.Interfaces;
using System.Threading.Tasks;
using System.IO;

namespace Foodbook.Presentation.Views
{
    public partial class ShoppingListDialog : Window
    {
        private ShoppingListResult? _currentShoppingList;
        private readonly IShoppingListService? _shoppingListService;

        public ShoppingListDialog()
        {
            InitializeComponent();
        }

        public ShoppingListDialog(IShoppingListService shoppingListService) : this()
        {
            _shoppingListService = shoppingListService;
        }

        public void SetShoppingList(ShoppingListResult shoppingList)
        {
            _currentShoppingList = shoppingList;
            
            // Set summary information
            TotalItemsText.Text = shoppingList.TotalItems.ToString();
            EstimatedCostText.Text = $"${shoppingList.EstimatedCost:F2}";
            ShoppingTimeText.Text = $"{shoppingList.EstimatedShoppingTime.TotalMinutes:F0} min";

            // Set categories with enhanced information
            CategoriesList.ItemsSource = shoppingList.Categories;
            
            // Set tips and suggestions
            TipsList.ItemsSource = shoppingList.Tips;
            SuggestionsList.ItemsSource = shoppingList.StoreSuggestions;

            // Update window title with list name
            if (!string.IsNullOrEmpty(shoppingList.ListName))
            {
                this.Title = $"🛒 {shoppingList.ListName}";
            }

            // Show potential savings if available
            if (shoppingList.PotentialSavings > 0)
            {
                EstimatedCostText.Text += $" (Save: ${shoppingList.PotentialSavings:F2})";
                EstimatedCostText.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentShoppingList == null)
                {
                    MessageBox.Show("No shopping list to export.", "Export Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show export options
                var exportDialog = new ExportOptionsDialog();
                if (exportDialog.ShowDialog() == true)
                {
                    var exportType = exportDialog.SelectedExportType;
                    var fileName = exportDialog.FileName;

                    switch (exportType)
                    {
                        case ExportType.Notes:
                            if (_shoppingListService != null)
                            {
                                var notesFileName = await _shoppingListService.ExportShoppingListToNotesAsync(_currentShoppingList, fileName);
                                MessageBox.Show($"Shopping list exported to Notes as: {notesFileName}", 
                                              "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Notes export service not available.", "Export Error", 
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;

                        case ExportType.TextFile:
                            await ExportToTextFile(fileName);
                            break;

                        case ExportType.PDF:
                            MessageBox.Show("PDF export functionality would be implemented here.", 
                                          "PDF Export", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting shopping list: {ex.Message}", 
                              "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToTextFile(string fileName)
        {
            try
            {
                var content = GenerateTextContent(_currentShoppingList!);
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{fileName}.txt");
                
                await File.WriteAllTextAsync(filePath, content);
                
                MessageBox.Show($"Shopping list exported to: {filePath}", 
                              "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving text file: {ex.Message}", 
                              "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            if (_currentShoppingList == null) return;

            var checkedItems = _currentShoppingList.Items.Count(item => item.IsChecked);
            var totalItems = _currentShoppingList.TotalItems;
            var progressPercentage = totalItems > 0 ? (double)checkedItems / totalItems * 100 : 0;

            // Update progress display (you could add a progress bar to the UI)
            Console.WriteLine($"Shopping Progress: {checkedItems}/{totalItems} ({progressPercentage:F1}%)");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchText = SearchTextBox.Text?.Trim();
                if (string.IsNullOrEmpty(searchText))
                {
                    // Show all items if search is empty
                    CategoriesList.ItemsSource = _currentShoppingList?.Categories;
                    return;
                }

                if (_currentShoppingList == null) return;

                // Filter categories and items based on search text
                var filteredCategories = _currentShoppingList.Categories
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

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentShoppingList == null)
                {
                    MessageBox.Show("No shopping list to print.", "Print Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create a print dialog
                var printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Generate print content
                    var printContent = GeneratePrintContent(_currentShoppingList);
                    
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

    // Export options dialog
    public partial class ExportOptionsDialog : Window
    {
        public ExportType SelectedExportType { get; private set; } = ExportType.TextFile;
        public string FileName { get; private set; } = "ShoppingList";

        public ExportOptionsDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Title = "Export Shopping List";
            this.Width = 400;
            this.Height = 300;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid();
            grid.Margin = new Thickness(20);

            // File name input
            var fileNameLabel = new Label { Content = "File Name:", Margin = new Thickness(0, 0, 0, 5) };
            var fileNameTextBox = new TextBox 
            { 
                Text = FileName, 
                Margin = new Thickness(0, 0, 0, 15),
                Name = "FileNameTextBox"
            };

            // Export type selection
            var typeLabel = new Label { Content = "Export Type:", Margin = new Thickness(0, 0, 0, 5) };
            var typeComboBox = new ComboBox 
            { 
                Margin = new Thickness(0, 0, 0, 20),
                Name = "TypeComboBox"
            };
            typeComboBox.Items.Add("Text File (.txt)");
            typeComboBox.Items.Add("Notes Integration");
            typeComboBox.Items.Add("PDF Document");
            typeComboBox.SelectedIndex = 0;

            // Buttons
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var exportButton = new Button 
            { 
                Content = "Export", 
                Width = 80, 
                Height = 30, 
                Margin = new Thickness(0, 0, 10, 0),
                Name = "ExportButton"
            };
            var cancelButton = new Button 
            { 
                Content = "Cancel", 
                Width = 80, 
                Height = 30,
                Name = "CancelButton"
            };

            buttonPanel.Children.Add(exportButton);
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(fileNameLabel);
            grid.Children.Add(fileNameTextBox);
            grid.Children.Add(typeLabel);
            grid.Children.Add(typeComboBox);
            grid.Children.Add(buttonPanel);

            this.Content = grid;

            // Event handlers
            exportButton.Click += (s, e) =>
            {
                FileName = fileNameTextBox.Text;
                SelectedExportType = (ExportType)typeComboBox.SelectedIndex;
                this.DialogResult = true;
                this.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                this.DialogResult = false;
                this.Close();
            };
        }
    }

    public enum ExportType
    {
        TextFile,
        Notes,
        PDF
    }
}
