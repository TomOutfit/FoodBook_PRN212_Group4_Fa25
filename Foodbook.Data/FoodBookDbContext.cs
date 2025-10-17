using Microsoft.EntityFrameworkCore;
using Foodbook.Data.Entities;

namespace Foodbook.Data
{
    public class FoodBookDbContext : DbContext
    {
        public FoodBookDbContext(DbContextOptions<FoodBookDbContext> options) : base(options)
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
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            });

            // Configure Recipe entity
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Instructions).IsRequired();
                entity.Property(e => e.Difficulty).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Recipes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Ingredient entity
            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.NutritionInfo).HasMaxLength(500);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.Quantity).HasPrecision(10, 2);
                
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure RecipeIngredient (many-to-many relationship)
            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasPrecision(10, 2);
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(200);
                
                entity.HasOne(e => e.Recipe)
                      .WithMany(r => r.RecipeIngredients)
                      .HasForeignKey(e => e.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.Ingredient)
                      .WithMany(i => i.RecipeIngredients)
                      .HasForeignKey(e => e.IngredientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Rating entity
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Score).IsRequired();
                entity.Property(e => e.Comment).HasMaxLength(500);
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Ratings)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.Recipe)
                      .WithMany(r => r.Ratings)
                      .HasForeignKey(e => e.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Ensure one user can only rate a recipe once
                entity.HasIndex(e => new { e.UserId, e.RecipeId }).IsUnique();
            });

            // Configure LogEntry entity
            modelBuilder.Entity<LogEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Source).HasMaxLength(200);
                entity.Property(e => e.FeatureName).HasMaxLength(100);
                entity.Property(e => e.LogType).HasMaxLength(50);
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.LogEntries)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
