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
        
        // Ensure database is created and migrated
        try
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=FoodbookDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
            }
            
            services.AddBusinessServices(connectionString);
            var serviceProvider = services.BuildServiceProvider();
            
            // Initialize ServiceContainer
            ServiceContainer.Initialize(serviceProvider);
            
            // Ensure database exists
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Foodbook.Data.FoodbookDbContext>();
            context.Database.EnsureCreated();
            
            // Show login window first
            var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
            var loginWindow = new LoginWindow(authService);
            
            if (loginWindow.ShowDialog() == true)
            {
                // Login successful, show main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // Login cancelled or failed, close application
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

