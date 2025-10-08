using System.Windows;
using Foodbook.Data.Entities;

namespace Foodbook.Presentation.Views
{
    /// <summary>
    /// Interaction logic for IngredientDialog.xaml
    /// </summary>
    public partial class IngredientDialog : Window
    {
        public Ingredient? Ingredient { get; private set; }
        public bool IsEditMode { get; private set; }

        public IngredientDialog(Ingredient? ingredient = null)
        {
            InitializeComponent();
            
            IsEditMode = ingredient != null;
            Ingredient = ingredient ?? new Ingredient
            {
                Name = "",
                Quantity = 1,
                Unit = "gram",
                UserId = 1 // Demo user
            };

            DataContext = Ingredient;
            
            // Set focus to name textbox
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Ingredient?.Name))
            {
                MessageBox.Show("Please enter a name for the ingredient.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (Ingredient.Quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity (greater than 0).", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Ingredient.Unit))
            {
                MessageBox.Show("Please select a unit.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                // UnitComboBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

