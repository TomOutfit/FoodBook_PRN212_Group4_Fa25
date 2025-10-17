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
            // Add DbContext
            services.AddDbContext<FoodBookDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IAIService, AIService>();
            services.AddScoped<IShoppingListService, ShoppingListService>();
            services.AddScoped<INutritionService, NutritionService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<ISettingsService, SettingsService>();
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

            return _serviceProvider.GetRequiredService<T>();
        }
    }
}