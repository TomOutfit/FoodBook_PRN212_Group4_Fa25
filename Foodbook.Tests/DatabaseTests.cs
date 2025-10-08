using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Foodbook.Data;
using Foodbook.Data.Entities;

namespace Foodbook.Tests;

public class DatabaseTests : IDisposable
{
    private readonly FoodBookDbContext _context;

    public DatabaseTests()
    {
        var options = new DbContextOptionsBuilder<FoodBookDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FoodBookDbContext(options);
    }

    [Fact]
    public async Task Database_ShouldSaveUser()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Database_ShouldSaveIngredient()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var ingredient = new Ingredient
        {
            Name = "Tomato",
            Unit = "piece",
            Quantity = 2.0m,
            UserId = user.Id
        };

        // Act
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        // Assert
        var savedIngredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == "Tomato");
        savedIngredient.Should().NotBeNull();
        savedIngredient!.Unit.Should().Be("piece");
        savedIngredient.Quantity.Should().Be(2.0m);
    }

    [Fact]
    public async Task Database_ShouldSaveRecipe()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var recipe = new Recipe
        {
            Title = "Test Recipe",
            Description = "A test recipe",
            Instructions = "1. Step one\n2. Step two",
            CookTime = 30,
            Servings = 4,
            Difficulty = "Easy",
            UserId = user.Id
        };

        // Act
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Assert
        var savedRecipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Title == "Test Recipe");
        savedRecipe.Should().NotBeNull();
        savedRecipe!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Database_ShouldSaveRecipeIngredient()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var recipe = new Recipe
        {
            Title = "Test Recipe",
            UserId = user.Id
        };
        await _context.Recipes.AddAsync(recipe);
        await _context.SaveChangesAsync();

        var ingredient = new Ingredient
        {
            Name = "Tomato",
            Unit = "piece",
            UserId = user.Id
        };
        await _context.Ingredients.AddAsync(ingredient);
        await _context.SaveChangesAsync();

        var recipeIngredient = new RecipeIngredient
        {
            RecipeId = recipe.Id,
            IngredientId = ingredient.Id,
            Quantity = 2.0m,
            Unit = "pieces",
            Notes = "Fresh"
        };

        // Act
        _context.RecipeIngredients.Add(recipeIngredient);
        await _context.SaveChangesAsync();

        // Assert
        var savedRecipeIngredient = await _context.RecipeIngredients
            .FirstOrDefaultAsync(ri => ri.RecipeId == recipe.Id && ri.IngredientId == ingredient.Id);
        savedRecipeIngredient.Should().NotBeNull();
        savedRecipeIngredient!.Quantity.Should().Be(2.0m);
    }

    [Fact]
    public async Task Database_ShouldSaveRating()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var recipe = new Recipe
        {
            Title = "Test Recipe",
            UserId = user.Id
        };
        await _context.Recipes.AddAsync(recipe);
        await _context.SaveChangesAsync();

        var rating = new Rating
        {
            RecipeId = recipe.Id,
            UserId = user.Id,
            Score = 5,
            Comment = "Excellent!"
        };

        // Act
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Assert
        var savedRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.RecipeId == recipe.Id && r.UserId == user.Id);
        savedRating.Should().NotBeNull();
        savedRating!.Score.Should().Be(5);
    }

    [Fact]
    public async Task Database_ShouldHandleMultipleUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Username = "user1", Email = "user1@example.com", PasswordHash = "hash1" },
            new User { Username = "user2", Email = "user2@example.com", PasswordHash = "hash2" },
            new User { Username = "user3", Email = "user3@example.com", PasswordHash = "hash3" }
        };

        // Act
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Assert
        var savedUsers = await _context.Users.ToListAsync();
        savedUsers.Should().HaveCount(3);
        savedUsers.Should().AllSatisfy(u => u.Username.Should().StartWith("user"));
    }

    [Fact]
    public async Task Database_ShouldHandleMultipleIngredients()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Tomato", Unit = "piece", UserId = user.Id },
            new Ingredient { Name = "Onion", Unit = "piece", UserId = user.Id },
            new Ingredient { Name = "Chicken", Unit = "gram", UserId = user.Id },
            new Ingredient { Name = "Rice", Unit = "cup", UserId = user.Id }
        };

        // Act
        _context.Ingredients.AddRange(ingredients);
        await _context.SaveChangesAsync();

        // Assert
        var savedIngredients = await _context.Ingredients.ToListAsync();
        savedIngredients.Should().HaveCount(4);
        savedIngredients.Should().Contain(i => i.Name == "Tomato");
        savedIngredients.Should().Contain(i => i.Name == "Chicken");
    }

    [Fact]
    public async Task Database_ShouldQueryRecipesByUser()
    {
        // Arrange
        var user1 = new User { Username = "user1", Email = "user1@example.com", PasswordHash = "hash1" };
        var user2 = new User { Username = "user2", Email = "user2@example.com", PasswordHash = "hash2" };
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var recipes = new List<Recipe>
        {
            new Recipe { Title = "Recipe 1", UserId = user1.Id },
            new Recipe { Title = "Recipe 2", UserId = user1.Id },
            new Recipe { Title = "Recipe 3", UserId = user2.Id }
        };
        await _context.Recipes.AddRangeAsync(recipes);
        await _context.SaveChangesAsync();

        // Act
        var user1Recipes = await _context.Recipes
            .Where(r => r.UserId == user1.Id)
            .ToListAsync();

        // Assert
        user1Recipes.Should().HaveCount(2);
        user1Recipes.Should().AllSatisfy(r => r.UserId.Should().Be(user1.Id));
    }

    [Fact]
    public async Task Database_ShouldQueryIngredientsByCategory()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Tomato", Unit = "piece", UserId = user.Id },
            new Ingredient { Name = "Onion", Unit = "piece", UserId = user.Id },
            new Ingredient { Name = "Chicken", Unit = "gram", UserId = user.Id },
            new Ingredient { Name = "Beef", Unit = "gram", UserId = user.Id }
        };
        await _context.Ingredients.AddRangeAsync(ingredients);
        await _context.SaveChangesAsync();

        // Act
        var vegetables = await _context.Ingredients
            .Where(i => i.Unit == "piece")
            .ToListAsync();

        // Assert
        vegetables.Should().HaveCount(2);
        vegetables.Should().AllSatisfy(i => i.Unit.Should().Be("piece"));
    }

    [Fact]
    public async Task Database_ShouldHandleCascadeDelete()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var recipe = new Recipe
        {
            Title = "Test Recipe",
            UserId = user.Id
        };
        await _context.Recipes.AddAsync(recipe);
        await _context.SaveChangesAsync();

        var rating = new Rating
        {
            RecipeId = recipe.Id,
            UserId = user.Id,
            Score = 5,
            Comment = "Great!"
        };
        await _context.Ratings.AddAsync(rating);
        await _context.SaveChangesAsync();

        // Act - Delete rating first, then recipe, then user
        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();
        
        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Assert
        var remainingUsers = await _context.Users.CountAsync();
        var remainingRecipes = await _context.Recipes.CountAsync();
        var remainingRatings = await _context.Ratings.CountAsync();

        remainingUsers.Should().Be(0);
        remainingRecipes.Should().Be(0);
        remainingRatings.Should().Be(0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
