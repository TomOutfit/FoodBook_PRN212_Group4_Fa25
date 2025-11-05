using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Foodbook.Data;
using Foodbook.Business.Interfaces;
using Foodbook.Business.Services;

namespace Foodbook.Business
{
    public static class ServiceContainer
    {
        private static IServiceProvider? _serviceProvider;

        public static IServiceCollection AddBusinessServices(this IServiceCollection services, string connectionString)
        {
            // Add DbContext as Scoped
            services.AddDbContext<FoodbookDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add Services as Scoped (they depend on DbContext)
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IAIService, AIService>();
            services.AddScoped<IUnsplashImageService, UnsplashImageService>();
            services.AddScoped<IShoppingListService, ShoppingListService>();
            services.AddScoped<INutritionService, NutritionService>();
            services.AddScoped<ILoggingService, LoggingService>();
            
            // SettingsService doesn't use DbContext, so register it as Singleton
            services.AddSingleton<ISettingsService, SettingsService>();
            
            // LocalizationService doesn't use DbContext, register as Singleton
            services.AddSingleton<ILocalizationService, LocalizationService>();
            
            // AuthenticationService depends on DbContext
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceContainer not initialized. Call Initialize() first.");

            try
            {
                // Create a scope for scoped services
                var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<T>();
                
                // Keep scope alive by storing it with the service
                // We'll dispose it when MainViewModel is disposed
                if (service is IDisposable || service is DbContext)
                {
                    // For scoped services, we return them as-is
                    // The scope needs to be managed at the caller level
                    return service;
                }
                
                return service;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get service {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }

        // Helper to create a scope for the MainViewModel
        public static IServiceScope CreateScope()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceContainer not initialized. Call Initialize() first.");
            
            return _serviceProvider.CreateScope();
        }
    }
}