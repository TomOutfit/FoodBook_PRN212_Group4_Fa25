using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Foodbook.Business.Services;
using Foodbook.Business.Interfaces;
using Foodbook.Business.Models;

namespace Foodbook.Tests
{
    /// <summary>
    /// Unit tests for FoodBook application
    /// Tests cover entities, services, and database operations
    /// </summary>
    public class UnitTest1
    {
        #region Entity Tests - User

        [Fact]
        public void User_ShouldHaveRequiredProperties()
        {
            // Arrange & Act
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                PasswordHash = "hashedpassword",
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            user.Should().NotBeNull();
            user.Username.Should().Be("testuser");
            user.Email.Should().Be("test@example.com");
            user.Password.Should().Be("password123");
            user.IsAdmin.Should().BeFalse();
            user.Id.Should().Be(1);
        }

        [Fact]
        public void User_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var user = new User
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "pass123",
                PasswordHash = "hash123"
            };

            // Assert
            user.IsAdmin.Should().BeFalse();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            user.Recipes.Should().NotBeNull().And.BeEmpty();
            user.Ingredients.Should().NotBeNull().And.BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void User_ShouldAcceptAdminFlag(bool isAdmin)
        {
            // Arrange & Act
            var user = new User
            {
                Username = "adminuser",
                Email = "admin@example.com",
                Password = "pass123",
                PasswordHash = "hash123",
                IsAdmin = isAdmin
            };

            // Assert
            user.IsAdmin.Should().Be(isAdmin);
        }

        #endregion

        #region Entity Tests - Recipe

        [Fact]
        public void Recipe_ShouldHaveRequiredProperties()
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                Id = 1,
                Title = "Test Recipe",
                Description = "A test recipe",
                Instructions = "Step 1, Step 2, Step 3",
                CookTime = 30,
                Servings = 4,
                Difficulty = "Medium",
                Category = "Main Course",
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            recipe.Should().NotBeNull();
            recipe.Title.Should().Be("Test Recipe");
            recipe.CookTime.Should().Be(30);
            recipe.Servings.Should().Be(4);
            recipe.Difficulty.Should().Be("Medium");
            recipe.UserId.Should().Be(1);
        }

        [Fact]
        public void Recipe_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                Title = "New Recipe",
                Instructions = "Cook it",
                UserId = 1
            };

            // Assert
            recipe.IsAIGenerated.Should().BeFalse();
            recipe.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            recipe.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            recipe.RecipeIngredients.Should().NotBeNull().And.BeEmpty();
            recipe.Ratings.Should().NotBeNull().And.BeEmpty();
        }

        [Theory]
        [InlineData("Easy")]
        [InlineData("Medium")]
        [InlineData("Hard")]
        public void Recipe_ShouldAcceptDifferentDifficulties(string difficulty)
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                Title = "Test Recipe",
                Instructions = "Instructions",
                Difficulty = difficulty,
                UserId = 1
            };

            // Assert
            recipe.Difficulty.Should().Be(difficulty);
        }

        #endregion

        #region Entity Tests - Ingredient

        [Fact]
        public void Ingredient_ShouldHaveRequiredProperties()
        {
            // Arrange & Act
            var ingredient = new Ingredient
            {
                Id = 1,
                Name = "Tomato",
                Category = "Vegetable",
                Unit = "piece",
                Quantity = 5,
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            ingredient.Should().NotBeNull();
            ingredient.Name.Should().Be("Tomato");
            ingredient.Category.Should().Be("Vegetable");
            ingredient.Unit.Should().Be("piece");
            ingredient.Quantity.Should().Be(5);
        }

        [Theory]
        [InlineData("piece")]
        [InlineData("gram")]
        [InlineData("ml")]
        [InlineData("cup")]
        public void Ingredient_ShouldAcceptDifferentUnits(string unit)
        {
            // Arrange & Act
            var ingredient = new Ingredient
            {
                Name = "Test Ingredient",
                Unit = unit
            };

            // Assert
            ingredient.Unit.Should().Be(unit);
        }

        #endregion

        #region AuthenticationService Tests

        [Fact]
        public async Task AuthenticationService_LoginAsync_ShouldReturnUser_WhenCredentialsAreValid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            // Use plain text password - AuthenticationService accepts both plain text and hash
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                PasswordHash = "hashedpassword123" // Service accepts plain text comparison
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Don't use settings service to avoid mock setup issues
            var authService = new AuthenticationService(context);

            var loginModel = new LoginModel
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = await authService.LoginAsync(loginModel);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@example.com");
            result.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task AuthenticationService_LoginAsync_ShouldReturnNull_WhenEmailNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            var authService = new AuthenticationService(context);

            var loginModel = new LoginModel
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            // Act
            var result = await authService.LoginAsync(loginModel);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticationService_LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "correctpassword",
                PasswordHash = "hashedcorrectpassword"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthenticationService(context);

            var loginModel = new LoginModel
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            // Act
            var result = await authService.LoginAsync(loginModel);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticationService_RegisterAsync_ShouldCreateNewUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            var authService = new AuthenticationService(context);

            var registerModel = new RegisterModel
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            // Act
            var result = await authService.RegisterAsync(registerModel);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("newuser");
            result.Email.Should().Be("newuser@example.com");
            
            // Verify user was saved to database
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            savedUser.Should().NotBeNull();
            savedUser!.Username.Should().Be("newuser");
        }

        [Fact]
        public async Task AuthenticationService_RegisterAsync_ShouldReturnNull_WhenEmailAlreadyExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            var existingUser = new User
            {
                Username = "existinguser",
                Email = "existing@example.com",
                Password = "password123",
                PasswordHash = "hash123"
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthenticationService(context);

            var registerModel = new RegisterModel
            {
                Username = "newuser",
                Email = "existing@example.com", // Same email
                Password = "password123"
            };

            // Act
            var result = await authService.RegisterAsync(registerModel);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region RecipeService Tests

        [Fact]
        public async Task RecipeService_GetAllRecipesAsync_ShouldReturnAllRecipes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "pass123",
                PasswordHash = "hash123"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var recipe1 = new Recipe
            {
                Title = "Recipe 1",
                Instructions = "Cook it",
                UserId = user.Id
            };
            var recipe2 = new Recipe
            {
                Title = "Recipe 2",
                Instructions = "Cook it too",
                UserId = user.Id
            };
            context.Recipes.AddRange(recipe1, recipe2);
            await context.SaveChangesAsync();

            var recipeService = new RecipeService(context);

            // Act
            var result = await recipeService.GetAllRecipesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.Title == "Recipe 1");
            result.Should().Contain(r => r.Title == "Recipe 2");
        }

        [Fact]
        public async Task RecipeService_GetRecipeByIdAsync_ShouldReturnRecipe_WhenExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "pass123",
                PasswordHash = "hash123"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Test Recipe",
                Instructions = "Cook it",
                UserId = user.Id
            };
            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();

            var recipeService = new RecipeService(context);

            // Act
            var result = await recipeService.GetRecipeByIdAsync(recipe.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Test Recipe");
            result.Id.Should().Be(recipe.Id);
        }

        [Fact]
        public async Task RecipeService_GetRecipeByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            var recipeService = new RecipeService(context);

            // Act
            var result = await recipeService.GetRecipeByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Database Integration Tests

        [Fact]
        public async Task Database_ShouldSaveUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            var user = new User
            {
                Username = "dbuser",
                Email = "db@example.com",
                Password = "password123",
                PasswordHash = "hash123"
            };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "db@example.com");
            savedUser.Should().NotBeNull();
            savedUser!.Username.Should().Be("dbuser");
            savedUser.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Database_ShouldSaveRecipe()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            
            var user = new User
            {
                Username = "recipeuser",
                Email = "recipe@example.com",
                Password = "pass123",
                PasswordHash = "hash123"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var recipe = new Recipe
            {
                Title = "Database Recipe",
                Instructions = "Cook in database",
                UserId = user.Id
            };

            // Act
            context.Recipes.Add(recipe);
            await context.SaveChangesAsync();

            // Assert
            var savedRecipe = await context.Recipes.FirstOrDefaultAsync(r => r.Title == "Database Recipe");
            savedRecipe.Should().NotBeNull();
            savedRecipe!.UserId.Should().Be(user.Id);
            savedRecipe.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Database_ShouldQueryRecipesByUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new FoodbookDbContext(options);
            
            var user1 = new User
            {
                Username = "user1",
                Email = "user1@example.com",
                Password = "pass123",
                PasswordHash = "hash123"
            };
            var user2 = new User
            {
                Username = "user2",
                Email = "user2@example.com",
                Password = "pass123",
                PasswordHash = "hash123"
            };
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var recipe1 = new Recipe { Title = "Recipe 1", Instructions = "Cook", UserId = user1.Id };
            var recipe2 = new Recipe { Title = "Recipe 2", Instructions = "Cook", UserId = user1.Id };
            var recipe3 = new Recipe { Title = "Recipe 3", Instructions = "Cook", UserId = user2.Id };
            context.Recipes.AddRange(recipe1, recipe2, recipe3);
            await context.SaveChangesAsync();

            // Act
            var user1Recipes = await context.Recipes
                .Where(r => r.UserId == user1.Id)
                .ToListAsync();

            // Assert
            user1Recipes.Should().HaveCount(2);
            user1Recipes.Should().OnlyContain(r => r.UserId == user1.Id);
        }

        #endregion

        #region Helper Methods Tests

        [Fact]
        public void User_PasswordHash_ShouldNotBeEmpty_WhenUserCreated()
        {
            // Arrange & Act
            var user = new User
            {
                Username = "hasheduser",
                Email = "hashed@example.com",
                Password = "plaintext",
                PasswordHash = "hashedpasswordvalue"
            };

            // Assert
            user.PasswordHash.Should().NotBeNullOrEmpty();
            user.PasswordHash.Should().NotBe(user.Password);
        }

        [Theory]
        [InlineData(30)]
        [InlineData(45)]
        [InlineData(60)]
        public void Recipe_ShouldStoreCookTimeCorrectly(int cookTime)
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                Title = "Time Test Recipe",
                Instructions = "Test instructions",
                CookTime = cookTime,
                UserId = 1
            };

            // Assert
            recipe.CookTime.Should().Be(cookTime);
            recipe.CookTime.Should().BeGreaterThan(0);
        }

        #endregion
    }
}

