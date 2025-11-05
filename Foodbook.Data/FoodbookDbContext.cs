using Microsoft.EntityFrameworkCore;
using Foodbook.Data.Entities;
using BCrypt.Net;

namespace Foodbook.Data
{
    public class FoodbookDbContext : DbContext
    {
        public FoodbookDbContext(DbContextOptions<FoodbookDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Recipe entity
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Recipes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Ingredient entity
            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Ingredients)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure RecipeIngredient (many-to-many relationship)
            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.HasKey(e => new { e.RecipeId, e.IngredientId });
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                
                entity.HasOne(e => e.Recipe)
                      .WithMany(r => r.RecipeIngredients)
                      .HasForeignKey(e => e.RecipeId)
                      .OnDelete(DeleteBehavior.NoAction);
                      
                entity.HasOne(e => e.Ingredient)
                      .WithMany(i => i.RecipeIngredients)
                      .HasForeignKey(e => e.IngredientId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Rating entity
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Ratings)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
                      
                entity.HasOne(e => e.Recipe)
                      .WithMany(r => r.Ratings)
                      .HasForeignKey(e => e.RecipeId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Ensure one user can only rate a recipe once
                entity.HasIndex(e => new { e.UserId, e.RecipeId }).IsUnique();
            });

            // Note: Seed data removed - using FoodBook.sql database instead
        }
    }
}