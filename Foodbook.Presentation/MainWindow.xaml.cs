using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Foodbook.Business;
using Foodbook.Business.Interfaces;
using Foodbook.Presentation.ViewModels;
using Foodbook.Presentation.Views;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading;

namespace Foodbook.Presentation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private IServiceScope? _serviceScope;
    private bool _isSidebarOpen = true;

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
            var errorTitle = GetLocalized("ErrorTitle", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "vi" ? "Lỗi" : "Error");
            var initError = GetLocalized("InitMainWindowError", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "vi" ? "Lỗi khởi tạo cửa sổ chính" : "Error initializing main window");
            MessageBox.Show($"{initError}: {ex.Message}", errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Load settings when window loads
        if (DataContext is MainViewModel vm && vm.LoadSettingsCommand.CanExecute(null))
        {
            vm.LoadSettingsCommand.Execute(null);

            // Refresh CurrentUser to ensure it's up-to-date after login
            await vm.LoadCurrentUserAsync();

            // Apply loaded settings immediately - no background task or async dispatch
            ApplyTheme(vm.SelectedTheme);
            ApplyLanguage(vm.SelectedLanguage);
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

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        _isSidebarOpen = !_isSidebarOpen;

        // Look up the ColumnDefinition named "SidebarColumn" from XAML at runtime.
        var sidebarColumn = FindName("SidebarColumn") as ColumnDefinition;

        if (_isSidebarOpen)
        {
            if (sidebarColumn != null)
                sidebarColumn.Width = new GridLength(240);

            var sidebar = FindName("Sidebar") as UIElement;
            if (sidebar != null)
                sidebar.Visibility = Visibility.Visible;
        }
        else
        {
            if (sidebarColumn != null)
                sidebarColumn.Width = new GridLength(80);

            // When closed, collapse the sidebar to free space
            var sidebar = FindName("Sidebar") as UIElement;
            if (sidebar != null)
                sidebar.Visibility = Visibility.Collapsed;
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
        }
        else
        {
            WindowState = WindowState.Maximized;
        }
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var confirmTitle = GetLocalized("LogoutConfirmTitle", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "vi" ? "Xác nhận đăng xuất" : "Logout Confirmation");
            var confirmMsg = GetLocalized("LogoutConfirmMessage", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "vi" ? "Bạn có chắc muốn đăng xuất?" : "Are you sure you want to logout?");
            var result = MessageBox.Show(confirmMsg, confirmTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var authService = ServiceContainer.GetService<IAuthenticationService>();
                await authService.LogoutAsync();

                // Exit the application immediately after successful logout
                Application.Current.Shutdown();
            }
        }
        catch (Exception ex)
        {
            var errorTitle = GetLocalized("ErrorTitle", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "vi" ? "Lỗi" : "Error");
            var logoutError = GetLocalized("LogoutError", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "vi" ? "Lỗi khi đăng xuất" : "Error during logout");
            MessageBox.Show($"{logoutError}: {ex.Message}", errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Apply theme & language only after saving settings
    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Apply current selections (which SaveSettingsCommand just persisted)
            ApplyTheme(vm.SelectedTheme);
            ApplyLanguage(vm.SelectedLanguage);
        }
    }

    public void ApplyTheme(string? theme)
    {
        try
        {
            // Helper to set/replace a resource brush by key (always replace to avoid frozen brushes)
            static void SetBrush(string key, string hex)
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                var newBrush = new System.Windows.Media.SolidColorBrush(color);
                Application.Current.Resources[key] = newBrush;
            }

            // Map legacy values: Dark -> Night, Light -> Day
            var t = (theme ?? "").Trim();
            var isNight = string.Equals(t, "Night", StringComparison.OrdinalIgnoreCase)
                || string.Equals(t, "Dark", StringComparison.OrdinalIgnoreCase);

            if (isNight)
            {
                // Deep, contrasty palette for dark mode with high contrast text colors
                SetBrush("AppBackgroundBrush", "#0F172A");
                SetBrush("SurfaceBrush", "#111827");
                SetBrush("CardBackgroundBrush", "#1F2937");
                SetBrush("SidebarBackgroundBrush", "#0B1220");
                SetBrush("DividerBrush", "#475569");
                // High contrast text colors for better readability on dark background
                SetBrush("TextPrimaryBrush", "#FFFFFF"); // Pure white for maximum contrast
                SetBrush("TextSecondaryBrush", "#E2E8F0"); // Light gray-blue for secondary text
                // Vibrant accent colors that stand out
                SetBrush("AccentBrush", "#22C55E"); // Bright green
                SetBrush("AccentLowBrush", "#143C2A");
                SetBrush("AccentHighBrush", "#86EFAC");
                this.Background = (System.Windows.Media.Brush)Application.Current.Resources["AppBackgroundBrush"];
            }
            else
            {
                // Bright, high-clarity palette for light mode
                // Backgrounds
                SetBrush("AppBackgroundBrush", "#F8FAFF");   // very light slate
                SetBrush("SurfaceBrush", "#FFFFFF");          // pure white panels
                SetBrush("CardBackgroundBrush", "#F3F4F6");   // gray-100 cards
                SetBrush("SidebarBackgroundBrush", "#F1F5F9"); // slate-100 sidebar
                SetBrush("DividerBrush", "#E5E7EB");          // gray-200
                // Text
                SetBrush("TextPrimaryBrush", "#111827");      // gray-900
                SetBrush("TextSecondaryBrush", "#374151");    // gray-700
                // Accent (more vivid in light theme)
                SetBrush("AccentBrush", "#16A34A");          // green-600
                SetBrush("AccentLowBrush", "#DCFCE7");        // green-100
                SetBrush("AccentHighBrush", "#22C55E");       // green-500
                this.Background = (System.Windows.Media.Brush)Application.Current.Resources["AppBackgroundBrush"];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme '{theme}': {ex.Message}");
            try
            {
                this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.WhiteSmoke);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to apply fallback theme");
            }
        }
    }

    public void ApplyLanguage(string? language)
    {
        var code = (language ?? "").Trim();
        var isVi = string.Equals(code, "VI", StringComparison.OrdinalIgnoreCase)
            || string.Equals(code, "Vietnamese", StringComparison.OrdinalIgnoreCase)
            || string.Equals(code, "vi-VN", StringComparison.OrdinalIgnoreCase);

        // Set UI culture
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(isVi ? "vi-VN" : "en-US");

        // Notify localization service to switch dictionaries
        try
        {
            var loc = ServiceContainer.GetService<ILocalizationService>();
            loc?.ChangeLanguage(isVi ? "Vietnamese" : "English");
            // Force-refresh localized bindings across the shell
            if (DataContext is MainViewModel shellVm)
            {
                shellVm.RefreshLocalization();
                shellVm.SettingsVM?.RefreshLocalization();
            }
        }
        catch { }
    }

    private static string GetLocalized(string key, string fallback)
    {
        try
        {
            var loc = ServiceContainer.GetService<ILocalizationService>();
            var text = loc?.GetString(key);
            // If service returns null/empty OR just echoes the key, use fallback
            if (!string.IsNullOrWhiteSpace(text) && !string.Equals(text, key, StringComparison.OrdinalIgnoreCase))
                return text!;
        }
        catch { }
        return fallback;
    }
}