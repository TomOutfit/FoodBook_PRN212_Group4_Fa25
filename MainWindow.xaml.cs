using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Foodbook.Business;
using Foodbook.Business.Interfaces;
using Foodbook.Presentation.ViewModels;
using Foodbook.Presentation.Views;
using System.Windows.Input;
using System.Windows.Controls;

namespace Foodbook.Presentation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        System.Diagnostics.Debug.WriteLine("MainWindow constructor started");
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine("InitializeComponent completed");
        
        try
        {
            System.Diagnostics.Debug.WriteLine("Getting services from ServiceContainer...");
            // Get services from ServiceContainer and create ViewModel manually
            var recipeService = ServiceContainer.GetService<IRecipeService>();
            var ingredientService = ServiceContainer.GetService<IIngredientService>();
            var aiService = ServiceContainer.GetService<IAIService>();
            var userService = ServiceContainer.GetService<IUserService>();
            var shoppingListService = ServiceContainer.GetService<IShoppingListService>();
            var nutritionService = ServiceContainer.GetService<INutritionService>();
            var loggingService = ServiceContainer.GetService<ILoggingService>();
            var settingsService = ServiceContainer.GetService<ISettingsService>();
            var authenticationService = ServiceContainer.GetService<IAuthenticationService>();
            
            System.Diagnostics.Debug.WriteLine("Services retrieved, creating MainViewModel...");
            var viewModel = new MainViewModel(
                recipeService, ingredientService, aiService, userService,
                shoppingListService, nutritionService, loggingService, settingsService, authenticationService);
            DataContext = viewModel;
            System.Diagnostics.Debug.WriteLine("MainViewModel created and set as DataContext");
            
            // Load recipes immediately after setting DataContext
            if (viewModel.LoadRecipesCommand.CanExecute(null))
            {
                viewModel.LoadRecipesCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing main window: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            
            // Try to create a minimal working ViewModel with database access
            try
            {
                System.Diagnostics.Debug.WriteLine("Attempting to create ViewModel with database access...");
                var recipeService = ServiceContainer.GetService<IRecipeService>();
                if (recipeService != null)
                {
                    var viewModel = new MainViewModel();
                    // Manually set the recipe service
                    var field = typeof(MainViewModel).GetField("_recipeService", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(viewModel, recipeService);
                    
                    DataContext = viewModel;
                    System.Diagnostics.Debug.WriteLine("Fallback ViewModel created with database access");
                    
                    // Load recipes
                    if (viewModel.LoadRecipesCommand.CanExecute(null))
                    {
                        viewModel.LoadRecipesCommand.Execute(null);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Recipe service not available");
                }
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback also failed: {fallbackEx.Message}");
                MessageBox.Show($"Error initializing main window: {ex.Message}\n\nFallback also failed: {fallbackEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DataContext = new MainViewModel();
            }
        }
        
        // Load settings and recipes when window loads
        Loaded += (s, e) => 
        {
            if (DataContext is MainViewModel vm)
            {
                if (vm.LoadSettingsCommand.CanExecute(null))
                {
                    vm.LoadSettingsCommand.Execute(null);
                }
                
                // Ensure recipes are loaded
                if (vm.LoadRecipesCommand.CanExecute(null))
                {
                    vm.LoadRecipesCommand.Execute(null);
                }
            }
        };
    }

    private void OpenImageAnalysis_Click(object sender, RoutedEventArgs e)
    {
        var imageWindow = new ImageManagementWindow();
        imageWindow.ShowDialog();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
        }
        else
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Button button)
        {
            button.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        }
    }

    private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Button button)
        {
            button.Background = System.Windows.Media.Brushes.Transparent;
        }
    }

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaximizeButton.Content = "??";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeButton.Content = "??";
        }
    }

    private void ViewLogsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var loggingService = ((MainViewModel)DataContext).LoggingService;
            var logViewer = new Views.LogViewerWindow(loggingService);
            logViewer.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening log viewer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout Confirmation", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var authService = ServiceContainer.GetService<IAuthenticationService>();
                await authService.LogoutAsync();
                
                // Close main window and show login window
                var loginWindow = new LoginWindow(authService);
                loginWindow.Owner = this;
                
                if (loginWindow.ShowDialog() == true)
                {
                    // Reload current user in ViewModel
                    if (DataContext is MainViewModel vm)
                    {
                        await vm.LoadCurrentUserAsync();
                    }
                }
                else
                {
                    // Login cancelled, close application
                    Application.Current.Shutdown();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
