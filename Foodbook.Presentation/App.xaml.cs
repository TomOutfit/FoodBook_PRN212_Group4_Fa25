using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Foodbook.Business;
using Foodbook.Business.Interfaces;
using Foodbook.Presentation.ViewModels;
using Foodbook.Presentation.Views;

namespace Foodbook.Presentation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set ShutdownMode early to prevent issues
        if (Application.Current != null)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        
        // Ensure database is created and migrated
        try
        {
            var services = new ServiceCollection();
            // Get the root directory of the solution
            var solutionRoot = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            while (solutionRoot != null && !System.IO.File.Exists(System.IO.Path.Combine(solutionRoot, "appsettings.json")))
            {
                solutionRoot = System.IO.Directory.GetParent(solutionRoot)?.FullName;
            }
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(solutionRoot ?? System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
            }
            
            // Add configuration to DI container
            services.AddSingleton<IConfiguration>(configuration);
            
            services.AddBusinessServices(connectionString);
            var serviceProvider = services.BuildServiceProvider();
            
            // Initialize ServiceContainer
            ServiceContainer.Initialize(serviceProvider);
            
            // Ensure database exists
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Foodbook.Data.FoodbookDbContext>();
            context.Database.EnsureCreated();
            
            // Create MainWindow first but don't show it yet
            System.Diagnostics.Debug.WriteLine("Creating MainWindow...");
            var mainWindow = new MainWindow();
            System.Diagnostics.Debug.WriteLine("MainWindow created successfully");
            
            if (Application.Current != null)
            {
                Application.Current.MainWindow = mainWindow;
            }
            
            // Show login window first
            var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
            var loginWindow = new LoginWindow(authService);
            
            // Handle login success event
            loginWindow.LoginSuccessful += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Login successful, showing MainWindow...");
                
                // Ensure MainWindow is visible and focused
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Topmost = true;
                mainWindow.Show();
                mainWindow.Activate();
                mainWindow.Topmost = false;
                mainWindow.Focus();
                
                System.Diagnostics.Debug.WriteLine("MainWindow shown successfully");
                
                // Close login window
                loginWindow.Close();
            };
            
            // Handle login window closed event
            loginWindow.Closed += (s, e) =>
            {
                // If login window closes without successful login, shutdown app
                if (!mainWindow.IsVisible)
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Shutdown();
                    }
                }
            };
            
            // Show login window
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

