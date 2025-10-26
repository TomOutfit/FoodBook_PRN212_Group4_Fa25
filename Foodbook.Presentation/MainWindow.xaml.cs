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
    private IServiceScope? _serviceScope;

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            // Create a scope for the MainViewModel
            _serviceScope = ServiceContainer.CreateScope();
            
            // Get services from ServiceContainer and create ViewModel manually
            var recipeService = _serviceScope.ServiceProvider.GetRequiredService<IRecipeService>();
            var ingredientService = _serviceScope.ServiceProvider.GetRequiredService<IIngredientService>();
            var aiService = _serviceScope.ServiceProvider.GetRequiredService<IAIService>();
            var userService = _serviceScope.ServiceProvider.GetRequiredService<IUserService>();
            var shoppingListService = _serviceScope.ServiceProvider.GetRequiredService<IShoppingListService>();
            var nutritionService = _serviceScope.ServiceProvider.GetRequiredService<INutritionService>();
            var loggingService = _serviceScope.ServiceProvider.GetRequiredService<ILoggingService>();
            var settingsService = _serviceScope.ServiceProvider.GetRequiredService<ISettingsService>();
            var localizationService = _serviceScope.ServiceProvider.GetRequiredService<ILocalizationService>();
            var authenticationService = _serviceScope.ServiceProvider.GetRequiredService<IAuthenticationService>();

            var viewModel = new MainViewModel(
                recipeService, ingredientService, aiService, userService,
                shoppingListService, nutritionService, loggingService, settingsService, localizationService, authenticationService);
            DataContext = viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing main window: {ex}");
            // Fallback: create a simple ViewModel without DI
            MessageBox.Show($"Error initializing main window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // Create a simple ViewModel as fallback
            DataContext = new MainViewModel();
            _serviceScope?.Dispose();
        }

        // Load settings when window loads
        Loaded += MainWindow_Loaded;
    }

    protected override void OnClosed(EventArgs e)
    {
        // Dispose the scope when the window is closed
        _serviceScope?.Dispose();
        base.OnClosed(e);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Load settings when window loads
        if (DataContext is MainViewModel vm && vm.LoadSettingsCommand.CanExecute(null))
        {
            vm.LoadSettingsCommand.Execute(null);
            
            // Apply loaded settings immediately
            Task.Run(async () =>
            {
                await Task.Delay(100); // Small delay to ensure settings are loaded
                Dispatcher.Invoke(() =>
                {
                    ApplyTheme(vm.SelectedTheme);
                    ApplyLanguage(vm.SelectedLanguage);
                });
            });
        }
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
            MaximizeButton.Content = "🗖";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeButton.Content = "🗗";
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

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var theme = selectedItem.Tag?.ToString();
            if (DataContext is MainViewModel vm)
            {
                vm.SelectedTheme = theme ?? "Light";
                // Apply theme immediately
                ApplyTheme(theme);
            }
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var language = selectedItem.Tag?.ToString();
            if (DataContext is MainViewModel vm)
            {
                vm.SelectedLanguage = language ?? "English";
                // Apply language immediately
                ApplyLanguage(language);
            }
        }
    }

    private void ApplyTheme(string? theme)
    {
        try
        {
            if (theme == "Dark")
            {
                // Apply dark theme
                this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkSlateGray);
                // You can add more dark theme styling here
            }
            else
            {
                // Apply light theme (default)
                this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.WhiteSmoke);
                // You can add more light theme styling here
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme '{theme}': {ex.Message}");
            // Fallback to default theme
            try
            {
                this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.WhiteSmoke);
            }
            catch
            {
                // If even the fallback fails, just log it
                System.Diagnostics.Debug.WriteLine("Failed to apply fallback theme");
            }
        }
    }

    private void ApplyLanguage(string? language)
    {
        if (language == "Vietnamese")
        {
            // Apply Vietnamese localization
            // You can implement localization logic here
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("vi-VN");
        }
        else
        {
            // Apply English localization (default)
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        }
    }
}