using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Foodbook.Business.Services;
using Foodbook.Business.Interfaces;
using Xunit;

namespace Foodbook.Tests
{
    public class RecipeServiceTests
    {
        private static FoodbookDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<FoodbookDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            return new FoodbookDbContext(options);
        }

        private static (FoodbookDbContext ctx, Mock<IUnsplashImageService> imageMock, RecipeService service) CreateService(string dbName)
        {
            var ctx = CreateInMemoryContext(dbName);
            var imageMock = new Mock<IUnsplashImageService>(MockBehavior.Strict);
            var service = new RecipeService(ctx, imageMock.Object);
            return (ctx, imageMock, service);
        }

        private static User CreateUser(int id = 1)
        {
            return new User
            {
                Id = id,
                Username = $"user{id}",
                Email = $"user{id}@example.com",
                PasswordHash = "hash"
            };
        }

        private static Ingredient CreateIngredient(int id, string name)
        {
            return new Ingredient
            {
                Id = id,
                Name = name
            };
        }

        private static Recipe CreateRecipe(int id, int userId, string title = "Pasta", string difficulty = "Easy", int cookTime = 15, string? imageUrl = null, string? category = null)
        {
            return new Recipe
            {
                Id = id,
                Title = title,
                Description = "Tasty",
                Instructions = "Do it",
                CookTime = cookTime,
                Servings = 2,
                Difficulty = difficulty,
                Category = category,
                ImageUrl = imageUrl,
                UserId = userId
            };
        }

        [Fact]
        public async Task GetAllRecipesAsync_LoadsMissingImages()
        {
            var (ctx, imageMock, service) = CreateService(nameof(GetAllRecipesAsync_LoadsMissingImages));

            var r1 = CreateRecipe(1, 1, title: "Pasta", imageUrl: null);
            var r2 = CreateRecipe(2, 1, title: "Soup", imageUrl: "http://has.image/2.jpg");

            var ing = CreateIngredient(1, "Tomato");
            ctx.Users.Add(CreateUser(1));
            ctx.Recipes.AddRange(r1, r2);
            ctx.Ingredients.Add(ing);
            ctx.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 1, IngredientId = 1, Quantity = 1, Unit = "pcs", Recipe = r1, Ingredient = ing });
            ctx.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 2, IngredientId = 1, Quantity = 2, Unit = "pcs", Recipe = r2, Ingredient = ing });
            await ctx.SaveChangesAsync();

            imageMock.Setup(m => m.SearchFoodImageAsync("Pasta", It.IsAny<int>(), It.IsAny<int>()))
                     .ReturnsAsync("http://img/pasta.jpg");

            var result = await service.GetAllRecipesAsync();

            result.Should().HaveCount(2);
            result.First(r => r.Id == 1).ImageUrl.Should().Be("http://img/pasta.jpg");
            result.First(r => r.Id == 2).ImageUrl.Should().Be("http://has.image/2.jpg");

            imageMock.Verify(m => m.SearchFoodImageAsync("Pasta", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            imageMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetRecipeByIdAsync_ReturnsNull_WhenNotFound()
        {
            var (_, _, service) = CreateService(nameof(GetRecipeByIdAsync_ReturnsNull_WhenNotFound));
            var recipe = await service.GetRecipeByIdAsync(999);
            recipe.Should().BeNull();
        }

        [Fact]
        public async Task GetRecipeByIdAsync_LoadsImage_WhenMissing()
        {
            var (ctx, imageMock, service) = CreateService(nameof(GetRecipeByIdAsync_LoadsImage_WhenMissing));
            var r = CreateRecipe(1, 1, title: "Pho", imageUrl: null);
            ctx.Users.Add(CreateUser(1));
            ctx.Recipes.Add(r);
            await ctx.SaveChangesAsync();

            imageMock.Setup(m => m.SearchFoodImageAsync("Pho", It.IsAny<int>(), It.IsAny<int>()))
                     .ReturnsAsync("http://img/pho.jpg");

            var recipe = await service.GetRecipeByIdAsync(1);

            recipe.Should().NotBeNull();
            recipe!.ImageUrl.Should().Be("http://img/pho.jpg");
            imageMock.Verify(m => m.SearchFoodImageAsync("Pho", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            imageMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SearchRecipesAsync_AppliesAllFilters()
        {
            var (ctx, imageMock, service) = CreateService(nameof(SearchRecipesAsync_AppliesAllFilters));

            var ingTomato = CreateIngredient(1, "Tomato");
            var ingBeef = CreateIngredient(2, "Beef");
            var r1 = CreateRecipe(1, 1, title: "Tomato Soup", difficulty: "Easy", cookTime: 10, imageUrl: null, category: "Soup");
            var r2 = CreateRecipe(2, 2, title: "Beef Stew", difficulty: "Hard", cookTime: 90, imageUrl: null, category: "Stew");

            ctx.Users.Add(CreateUser(1));
            ctx.Users.Add(CreateUser(2));
            ctx.Ingredients.AddRange(ingTomato, ingBeef);
            ctx.Recipes.AddRange(r1, r2);
            ctx.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = 1, IngredientId = 1, Quantity = 2, Unit = "pcs", Recipe = r1, Ingredient = ingTomato },
                new RecipeIngredient { RecipeId = 2, IngredientId = 2, Quantity = 1, Unit = "kg", Recipe = r2, Ingredient = ingBeef }
            );
            await ctx.SaveChangesAsync();

            imageMock.Setup(m => m.SearchFoodImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                     .ReturnsAsync((string s, int _, int _) => $"http://img/{s.Replace(' ', '_')}.jpg");

            var results = await service.SearchRecipesAsync(name: "Tomato", ingredient: "Tomato", cookTime: 30, difficulty: "Easy");

            results.Should().ContainSingle(r => r.Id == 1);
            results.First().ImageUrl.Should().Be("http://img/Tomato_Soup.jpg");
            imageMock.Verify(m => m.SearchFoodImageAsync("Tomato Soup", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task CreateRecipeAsync_SetsTimestamps_AndPersists()
        {
            var (ctx, _, service) = CreateService(nameof(CreateRecipeAsync_SetsTimestamps_AndPersists));
            var nowBefore = DateTime.UtcNow.AddSeconds(-1);
            var recipe = CreateRecipe(0, 1, title: "New Dish");
            ctx.Users.Add(CreateUser(1));

            var created = await service.CreateRecipeAsync(recipe);

            created.Id.Should().BeGreaterThan(0);
            created.CreatedAt.Should().BeOnOrAfter(nowBefore);
            created.UpdatedAt.Should().BeOnOrAfter(nowBefore);

            (await ctx.Recipes.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task UpdateRecipeAsync_UpdatesTimestamp_AndPersistsChanges()
        {
            var (ctx, _, service) = CreateService(nameof(UpdateRecipeAsync_UpdatesTimestamp_AndPersistsChanges));
            var r = CreateRecipe(1, 1, title: "Old");
            ctx.Users.Add(CreateUser(1));
            ctx.Recipes.Add(r);
            await ctx.SaveChangesAsync();

            var prevUpdated = r.UpdatedAt;
            r.Title = "Updated";

            var updated = await service.UpdateRecipeAsync(r);

            updated.Title.Should().Be("Updated");
            updated.UpdatedAt.Should().BeAfter(prevUpdated);
            (await ctx.Recipes.FirstAsync()).Title.Should().Be("Updated");
        }

        [Fact]
        public async Task DeleteRecipeAsync_ReturnsFalse_WhenNotFound()
        {
            var (_, _, service) = CreateService(nameof(DeleteRecipeAsync_ReturnsFalse_WhenNotFound));
            var ok = await service.DeleteRecipeAsync(123);
            ok.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteRecipeAsync_RemovesEntity_WhenFound()
        {
            var (ctx, _, service) = CreateService(nameof(DeleteRecipeAsync_RemovesEntity_WhenFound));
            var r = CreateRecipe(1, 1);
            ctx.Users.Add(CreateUser(1));
            ctx.Recipes.Add(r);
            await ctx.SaveChangesAsync();

            var ok = await service.DeleteRecipeAsync(1);
            ok.Should().BeTrue();
            (await ctx.Recipes.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task AdjustServingsAsync_Throws_WhenRecipeNotFound()
        {
            var (_, _, service) = CreateService(nameof(AdjustServingsAsync_Throws_WhenRecipeNotFound));
            var act = async () => await service.AdjustServingsAsync(999, 4);
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Recipe not found");
        }

        [Fact]
        public async Task AdjustServingsAsync_AdjustsQuantities_AndUpdatesServings()
        {
            var (ctx, _, service) = CreateService(nameof(AdjustServingsAsync_AdjustsQuantities_AndUpdatesServings));
            var ing = CreateIngredient(1, "Rice");
            var r = CreateRecipe(1, 1, title: "Rice Bowl");
            r.Servings = 2;
            var ri = new RecipeIngredient { RecipeId = 1, IngredientId = 1, Quantity = 100m, Unit = "g", Recipe = r, Ingredient = ing };
            r.RecipeIngredients.Add(ri);

            ctx.Users.Add(CreateUser(1));
            ctx.Ingredients.Add(ing);
            ctx.Recipes.Add(r);
            ctx.RecipeIngredients.Add(ri);
            await ctx.SaveChangesAsync();

            var updated = await service.AdjustServingsAsync(1, 4);

            updated.Servings.Should().Be(4);
            updated.RecipeIngredients.First().Quantity.Should().Be(200m);
        }

        [Fact]
        public async Task GetRecipesByUserIdAsync_FiltersByUser()
        {
            var (ctx, imageMock, service) = CreateService(nameof(GetRecipesByUserIdAsync_FiltersByUser));
            var r1 = CreateRecipe(1, 1, title: "Dish A", imageUrl: null);
            var r2 = CreateRecipe(2, 2, title: "Dish B", imageUrl: null);
            ctx.Users.Add(CreateUser(1));
            ctx.Users.Add(CreateUser(2));
            ctx.Recipes.AddRange(r1, r2);
            await ctx.SaveChangesAsync();

            imageMock.Setup(m => m.SearchFoodImageAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                     .ReturnsAsync("http://img/any.jpg");

            var user1Recipes = await service.GetRecipesByUserIdAsync(1);

            user1Recipes.Should().ContainSingle(r => r.UserId == 1);
            imageMock.Verify(m => m.SearchFoodImageAsync("Dish A", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetAverageRatingAsync_ReturnsZero_WhenNoRatings()
        {
            var (ctx, _, service) = CreateService(nameof(GetAverageRatingAsync_ReturnsZero_WhenNoRatings));
            var r = CreateRecipe(1, 1);
            ctx.Users.Add(CreateUser(1));
            ctx.Recipes.Add(r);
            await ctx.SaveChangesAsync();

            var avg = await service.GetAverageRatingAsync(1);
            avg.Should().Be(0.0);
        }

        [Fact]
        public async Task GetAverageRatingAsync_ReturnsAverage_WhenRatingsExist()
        {
            var (ctx, _, service) = CreateService(nameof(GetAverageRatingAsync_ReturnsAverage_WhenRatingsExist));
            var r = CreateRecipe(1, 1);
            var u = CreateUser(1);
            ctx.Users.Add(u);
            ctx.Recipes.Add(r);
            ctx.Ratings.AddRange(
                new Rating { Id = 1, RecipeId = 1, UserId = 1, Score = 4, Recipe = r, User = u },
                new Rating { Id = 2, RecipeId = 1, UserId = 1, Score = 2, Recipe = r, User = u }
            );
            await ctx.SaveChangesAsync();

            var avg = await service.GetAverageRatingAsync(1);
            avg.Should().BeApproximately(3.0, 0.0001);
        }
    }
}


