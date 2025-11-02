using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
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
            
            var connectionString = configuration.GetConnectionString("DBDefault")
                ?? configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DBDefault' (or 'DefaultConnection') not found in appsettings.json");
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
            
            try
            {
                // Ensure database schema is created/updated
                context.Database.EnsureCreated();
                
                // Apply migrations for new columns (if database already exists)
                ApplyMigrations(context);
            }
            catch (Exception dbEx)
            {
                // If there's a database connection issue, show a more specific error
                MessageBox.Show($"Database connection error: {dbEx.Message}\n\nPlease check your SQL Server installation and connection string.", 
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Create MainWindow first but don't show it yet
            System.Diagnostics.Debug.WriteLine("Creating MainWindow...");
            var mainWindow = new MainWindow();
            System.Diagnostics.Debug.WriteLine("MainWindow created successfully");
            
            if (Application.Current != null)
            {
                Application.Current.MainWindow = mainWindow;
            }
            
            // Show login window first
            IAuthenticationService authService;
            try
            {
                authService = serviceProvider.GetRequiredService<IAuthenticationService>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize authentication service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
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
    
    /// <summary>
    /// Apply database migrations for schema updates (e.g., adding new columns)
    /// </summary>
    private static void ApplyMigrations(Foodbook.Data.FoodbookDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            connection.Open();
            
            try
            {
                // Check if IsAIGenerated column exists in Recipes table
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[Recipes]') 
                        AND name = 'IsAIGenerated'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Recipes]
                        ADD [IsAIGenerated] [bit] NOT NULL DEFAULT(0);
                        PRINT 'Added IsAIGenerated column to Recipes table';
                    END";
                
                command.ExecuteNonQuery();
                
                // Update existing recipes to have IsAIGenerated = 0
                command.CommandText = @"
                    UPDATE [dbo].[Recipes]
                    SET [IsAIGenerated] = 0
                    WHERE [IsAIGenerated] IS NULL";
                
                command.ExecuteNonQuery();
                
                System.Diagnostics.Debug.WriteLine("✅ Database migrations applied successfully");
            }
            finally
            {
                connection.Close();
            }
        }
        catch (DbException dbEx)
        {
            // Handle database-specific errors
            var errorMessage = dbEx.Message;
            if (errorMessage.Contains("already exists") || errorMessage.Contains("Cannot insert duplicate key"))
            {
                System.Diagnostics.Debug.WriteLine("IsAIGenerated column already exists. Skipping migration.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Migration warning: {dbEx.Message}");
                // Don't throw - let the app continue even if migration fails
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Migration error: {ex.Message}");
            // Don't throw - let the app continue even if migration fails
        }
    }
}

