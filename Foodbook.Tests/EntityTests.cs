using Xunit;
using FluentAssertions;
using Foodbook.Data.Entities;

namespace Foodbook.Tests;

public class EntityTests
{
    [Fact]
    public void User_ShouldHaveRequiredProperties()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        // Assert
        user.Id.Should().Be(1);
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashedpassword");
    }

    [Fact]
    public void Ingredient_ShouldHaveRequiredProperties()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Id = 1,
            Name = "Tomato",
            Unit = "piece",
            Quantity = 2.0m,
            UserId = 1
        };

        // Assert
        ingredient.Id.Should().Be(1);
        ingredient.Name.Should().Be("Tomato");
        ingredient.Unit.Should().Be("piece");
        ingredient.Quantity.Should().Be(2.0m);
        ingredient.UserId.Should().Be(1);
    }

    [Fact]
    public void Recipe_ShouldHaveRequiredProperties()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Title = "Test Recipe",
            Description = "A test recipe",
            Instructions = "1. Step one\n2. Step two",
            CookTime = 30,
            Servings = 4,
            Difficulty = "Easy",
            UserId = 1
        };

        // Assert
        recipe.Id.Should().Be(1);
        recipe.Title.Should().Be("Test Recipe");
        recipe.Description.Should().Be("A test recipe");
        recipe.Instructions.Should().Be("1. Step one\n2. Step two");
        recipe.CookTime.Should().Be(30);
        recipe.Servings.Should().Be(4);
        recipe.Difficulty.Should().Be("Easy");
        recipe.UserId.Should().Be(1);
    }

    [Fact]
    public void RecipeIngredient_ShouldHaveRequiredProperties()
    {
        // Arrange
        var recipeIngredient = new RecipeIngredient
        {
            RecipeId = 1,
            IngredientId = 1,
            Quantity = 1.5m,
            Unit = "cup",
            Notes = "Fresh"
        };

        // Assert
        recipeIngredient.RecipeId.Should().Be(1);
        recipeIngredient.IngredientId.Should().Be(1);
        recipeIngredient.Quantity.Should().Be(1.5m);
        recipeIngredient.Unit.Should().Be("cup");
        recipeIngredient.Notes.Should().Be("Fresh");
    }

    [Fact]
    public void Rating_ShouldHaveRequiredProperties()
    {
        // Arrange
        var rating = new Rating
        {
            Id = 1,
            RecipeId = 1,
            UserId = 1,
            Score = 5,
            Comment = "Excellent!"
        };

        // Assert
        rating.Id.Should().Be(1);
        rating.RecipeId.Should().Be(1);
        rating.UserId.Should().Be(1);
        rating.Score.Should().Be(5);
        rating.Comment.Should().Be("Excellent!");
    }

    [Theory]
    [InlineData(1, "Easy")]
    [InlineData(2, "Medium")]
    [InlineData(3, "Hard")]
    public void Recipe_ShouldAcceptDifferentDifficulties(int id, string difficulty)
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = id,
            Title = $"Recipe {id}",
            Difficulty = difficulty,
            UserId = 1
        };

        // Assert
        recipe.Id.Should().Be(id);
        recipe.Difficulty.Should().Be(difficulty);
    }

    [Theory]
    [InlineData("piece")]
    [InlineData("gram")]
    [InlineData("ml")]
    [InlineData("cup")]
    public void Ingredient_ShouldAcceptDifferentUnits(string unit)
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Id = 1,
            Name = "Test Ingredient",
            Unit = unit,
            UserId = 1
        };

        // Assert
        ingredient.Unit.Should().Be(unit);
    }

    [Fact]
    public void User_ShouldHaveDefaultValues()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        // Assert
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Ingredient_ShouldHaveDefaultValues()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Name = "Test Ingredient",
            UserId = 1
        };

        // Assert
        ingredient.Name.Should().Be("Test Ingredient");
        ingredient.Unit.Should().Be("piece");
        ingredient.Quantity.Should().Be(0);
        ingredient.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Recipe_ShouldHaveDefaultValues()
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = "Test Recipe",
            UserId = 1
        };

        // Assert
        recipe.Title.Should().Be("Test Recipe");
        recipe.Difficulty.Should().Be("Easy");
        recipe.Servings.Should().Be(4);
        recipe.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        recipe.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    [InlineData(3, 1)]
    public void Rating_ShouldAcceptDifferentScores(int id, int score)
    {
        // Arrange
        var rating = new Rating
        {
            Id = id,
            RecipeId = 1,
            UserId = 1,
            Score = score,
            Comment = $"Rating {score}"
        };

        // Assert
        rating.Id.Should().Be(id);
        rating.Score.Should().Be(score);
        rating.Comment.Should().Be($"Rating {score}");
    }

    [Fact]
    public void RecipeIngredient_ShouldHaveDefaultUnit()
    {
        // Arrange
        var recipeIngredient = new RecipeIngredient
        {
            RecipeId = 1,
            IngredientId = 1,
            Quantity = 2.0m
        };

        // Assert
        recipeIngredient.Unit.Should().Be("piece");
        recipeIngredient.Quantity.Should().Be(2.0m);
    }
}
