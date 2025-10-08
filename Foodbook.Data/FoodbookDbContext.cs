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

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            // Seed static users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@foodbook.com",
                    PasswordHash = "$2a$11$rQZ8K9vQZ8K9vQZ8K9vQZ8O",
                    IsAdmin = true,
                    CreatedAt = seedDate
                },
                new User
                {
                    Id = 2,
                    Username = "chef_mario",
                    Email = "mario@foodbook.com",
                    PasswordHash = "$2a$11$rQZ8K9vQZ8K9vQZ8K9vQZ8O",
                    IsAdmin = false,
                    CreatedAt = seedDate.AddDays(-30)
                },
                new User
                {
                    Id = 3,
                    Username = "home_cook_lisa",
                    Email = "lisa@foodbook.com",
                    PasswordHash = "$2a$11$rQZ8K9vQZ8K9vQZ8K9vQZ8O",
                    IsAdmin = false,
                    CreatedAt = seedDate.AddDays(-15)
                },
                new User
                {
                    Id = 4,
                    Username = "foodie_john",
                    Email = "john@foodbook.com",
                    PasswordHash = "$2a$11$rQZ8K9vQZ8K9vQZ8K9vQZ8O",
                    IsAdmin = false,
                    CreatedAt = seedDate.AddDays(-7)
                }
            );

            // Seed diverse ingredients
            SeedIngredients(modelBuilder, seedDate);

            // Seed diverse recipes
            SeedRecipes(modelBuilder, seedDate);

            // Seed recipe ingredients relationships
            SeedRecipeIngredients(modelBuilder, seedDate);

            // Seed ratings
            SeedRatings(modelBuilder, seedDate);
        }

        private void SeedIngredients(ModelBuilder modelBuilder, DateTime seedDate)
        {
            var ingredients = new List<Ingredient>();
            var ingredientId = 1;

            // Vegetables
            var vegetables = new[] { "Tomato", "Onion", "Garlic", "Carrot", "Potato", "Bell Pepper", "Spinach", "Broccoli", "Mushroom", "Cucumber" };
            foreach (var veg in vegetables)
            {
                ingredients.Add(new Ingredient
                {
                    Id = ingredientId++,
                    Name = veg,
                    Unit = "piece",
                    Quantity = 5,
                    UserId = 1,
                    CreatedAt = seedDate.AddDays(-10)
                });
            }

            // Proteins
            var proteins = new[] { "Chicken Breast", "Beef", "Salmon", "Tofu", "Eggs", "Pork", "Shrimp", "Turkey", "Lamb", "Fish" };
            foreach (var protein in proteins)
            {
                ingredients.Add(new Ingredient
                {
                    Id = ingredientId++,
                    Name = protein,
                    Unit = "gram",
                    Quantity = 500,
                    UserId = 2,
                    CreatedAt = seedDate.AddDays(-5)
                });
            }

            // Grains & Starches
            var grains = new[] { "Rice", "Pasta", "Bread", "Quinoa", "Oats", "Barley", "Couscous", "Noodles", "Tortilla", "Crackers" };
            foreach (var grain in grains)
            {
                ingredients.Add(new Ingredient
                {
                    Id = ingredientId++,
                    Name = grain,
                    Unit = "cup",
                    Quantity = 2,
                    UserId = 3,
                    CreatedAt = seedDate.AddDays(-3)
                });
            }

            // Dairy
            var dairy = new[] { "Milk", "Cheese", "Yogurt", "Butter", "Cream", "Sour Cream", "Cottage Cheese", "Mozzarella", "Parmesan", "Ricotta" };
            foreach (var item in dairy)
            {
                ingredients.Add(new Ingredient
                {
                    Id = ingredientId++,
                    Name = item,
                    Unit = "cup",
                    Quantity = 1,
                    UserId = 4,
                    CreatedAt = seedDate.AddDays(-1)
                });
            }

            // Spices & Herbs
            var spices = new[] { "Salt", "Black Pepper", "Garlic Powder", "Oregano", "Basil", "Thyme", "Rosemary", "Paprika", "Cumin", "Cinnamon" };
            foreach (var spice in spices)
            {
                ingredients.Add(new Ingredient
                {
                    Id = ingredientId++,
                    Name = spice,
                    Unit = "teaspoon",
                    Quantity = 5,
                    UserId = 1,
                    CreatedAt = seedDate.AddDays(-7)
                });
            }

            modelBuilder.Entity<Ingredient>().HasData(ingredients);
        }

        private void SeedRecipes(ModelBuilder modelBuilder, DateTime seedDate)
        {
            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Id = 1,
                    Title = "Classic Spaghetti Carbonara",
                    Description = "Creamy Italian pasta with eggs and cheese",
                    Instructions = "1. Cook pasta according to package directions\n2. Whisk eggs with cheese in a bowl\n3. Cook pancetta until crispy\n4. Toss hot pasta with egg mixture\n5. Add pancetta and serve immediately",
                    Difficulty = "Medium",
                    CookTime = 25,
                    Servings = 4,
                    ImageUrl = "https://example.com/images/classic-spaghetti-carbonara.jpg",
                    UserId = 1,
                    CreatedAt = seedDate.AddDays(-20),
                    UpdatedAt = seedDate.AddDays(-5)
                },
                new Recipe
                {
                    Id = 2,
                    Title = "Grilled Salmon with Herbs",
                    Description = "Fresh salmon grilled with aromatic herbs",
                    Instructions = "1. Season salmon with herbs and olive oil\n2. Preheat grill to medium-high\n3. Grill salmon 4-5 minutes per side\n4. Let rest before serving\n5. Garnish with fresh herbs",
                    Difficulty = "Easy",
                    CookTime = 20,
                    Servings = 2,
                    ImageUrl = "https://example.com/images/grilled-salmon-with-herbs.jpg",
                    UserId = 2,
                    CreatedAt = seedDate.AddDays(-15),
                    UpdatedAt = seedDate.AddDays(-3)
                },
                new Recipe
                {
                    Id = 3,
                    Title = "Vegetarian Buddha Bowl",
                    Description = "Healthy bowl with quinoa, vegetables, and tahini dressing",
                    Instructions = "1. Cook quinoa according to package directions\n2. Roast vegetables in oven\n3. Prepare tahini dressing\n4. Assemble bowl with quinoa, vegetables\n5. Drizzle with dressing and serve",
                    Difficulty = "Easy",
                    CookTime = 30,
                    Servings = 2,
                    ImageUrl = "https://example.com/images/vegetarian-buddha-bowl.jpg",
                    UserId = 3,
                    CreatedAt = seedDate.AddDays(-10),
                    UpdatedAt = seedDate.AddDays(-1)
                },
                new Recipe
                {
                    Id = 4,
                    Title = "Beef Wellington",
                    Description = "Elegant beef tenderloin wrapped in puff pastry",
                    Instructions = "1. Season and sear beef tenderloin\n2. Wrap in mushroom duxelles\n3. Wrap in puff pastry\n4. Bake at 400°F for 25-30 minutes\n5. Let rest before slicing",
                    Difficulty = "Hard",
                    CookTime = 120,
                    Servings = 6,
                    ImageUrl = "https://example.com/images/beef-wellington.jpg",
                    UserId = 1,
                    CreatedAt = seedDate.AddDays(-25),
                    UpdatedAt = seedDate.AddDays(-8)
                },
                new Recipe
                {
                    Id = 5,
                    Title = "Thai Green Curry",
                    Description = "Spicy coconut curry with vegetables and rice",
                    Instructions = "1. Heat coconut milk in large pot\n2. Add curry paste and stir\n3. Add vegetables and protein\n4. Simmer until cooked through\n5. Serve over rice with fresh herbs",
                    Difficulty = "Medium",
                    CookTime = 35,
                    Servings = 4,
                    ImageUrl = "https://example.com/images/thai-green-curry.jpg",
                    UserId = 4,
                    CreatedAt = seedDate.AddDays(-12),
                    UpdatedAt = seedDate.AddDays(-2)
                }
            };

            modelBuilder.Entity<Recipe>().HasData(recipes);
        }

        private void SeedRecipeIngredients(ModelBuilder modelBuilder, DateTime seedDate)
        {
            var recipeIngredients = new List<RecipeIngredient>
            {
                // Spaghetti Carbonara ingredients
                new RecipeIngredient { RecipeId = 1, IngredientId = 1, Quantity = 2, Unit = "cup" }, // Pasta
                new RecipeIngredient { RecipeId = 1, IngredientId = 2, Quantity = 3, Unit = "piece" }, // Eggs
                new RecipeIngredient { RecipeId = 1, IngredientId = 3, Quantity = 1, Unit = "cup" }, // Cheese
                new RecipeIngredient { RecipeId = 1, IngredientId = 4, Quantity = 2, Unit = "piece" }, // Garlic
                
                // Grilled Salmon ingredients
                new RecipeIngredient { RecipeId = 2, IngredientId = 5, Quantity = 300, Unit = "gram" }, // Salmon
                new RecipeIngredient { RecipeId = 2, IngredientId = 6, Quantity = 2, Unit = "teaspoon" }, // Salt
                new RecipeIngredient { RecipeId = 2, IngredientId = 7, Quantity = 1, Unit = "teaspoon" }, // Black Pepper
                new RecipeIngredient { RecipeId = 2, IngredientId = 8, Quantity = 1, Unit = "teaspoon" }, // Oregano
                
                // Buddha Bowl ingredients
                new RecipeIngredient { RecipeId = 3, IngredientId = 9, Quantity = 1, Unit = "cup" }, // Quinoa
                new RecipeIngredient { RecipeId = 3, IngredientId = 10, Quantity = 2, Unit = "piece" }, // Carrot
                new RecipeIngredient { RecipeId = 3, IngredientId = 11, Quantity = 1, Unit = "piece" }, // Bell Pepper
                new RecipeIngredient { RecipeId = 3, IngredientId = 12, Quantity = 1, Unit = "cup" }, // Spinach
                
                // Beef Wellington ingredients
                new RecipeIngredient { RecipeId = 4, IngredientId = 13, Quantity = 800, Unit = "gram" }, // Beef
                new RecipeIngredient { RecipeId = 4, IngredientId = 14, Quantity = 2, Unit = "piece" }, // Mushroom
                new RecipeIngredient { RecipeId = 4, IngredientId = 15, Quantity = 1, Unit = "piece" }, // Onion
                new RecipeIngredient { RecipeId = 4, IngredientId = 16, Quantity = 1, Unit = "piece" }, // Garlic
                
                // Thai Green Curry ingredients
                new RecipeIngredient { RecipeId = 5, IngredientId = 17, Quantity = 1, Unit = "cup" }, // Rice
                new RecipeIngredient { RecipeId = 5, IngredientId = 18, Quantity = 2, Unit = "piece" }, // Bell Pepper
                new RecipeIngredient { RecipeId = 5, IngredientId = 19, Quantity = 1, Unit = "piece" }, // Onion
                new RecipeIngredient { RecipeId = 5, IngredientId = 20, Quantity = 1, Unit = "piece" }, // Garlic
            };

            modelBuilder.Entity<RecipeIngredient>().HasData(recipeIngredients);
        }

        private void SeedRatings(ModelBuilder modelBuilder, DateTime seedDate)
        {
            var ratings = new List<Rating>
            {
                new Rating
                {
                    Id = 1,
                    RecipeId = 1,
                    UserId = 2,
                    Score = 5,
                    Comment = "Absolutely delicious! Will definitely make again.",
                    CreatedAt = seedDate.AddDays(-15)
                },
                new Rating
                {
                    Id = 2,
                    RecipeId = 1,
                    UserId = 3,
                    Score = 4,
                    Comment = "Great recipe, easy to follow instructions.",
                    CreatedAt = seedDate.AddDays(-10)
                },
                new Rating
                {
                    Id = 3,
                    RecipeId = 2,
                    UserId = 1,
                    Score = 5,
                    Comment = "Perfect for a family dinner.",
                    CreatedAt = seedDate.AddDays(-12)
                },
                new Rating
                {
                    Id = 4,
                    RecipeId = 2,
                    UserId = 4,
                    Score = 4,
                    Comment = "Amazing flavors, highly recommend!",
                    CreatedAt = seedDate.AddDays(-8)
                },
                new Rating
                {
                    Id = 5,
                    RecipeId = 3,
                    UserId = 1,
                    Score = 5,
                    Comment = "Quick and easy, perfect for weeknights.",
                    CreatedAt = seedDate.AddDays(-5)
                },
                new Rating
                {
                    Id = 6,
                    RecipeId = 3,
                    UserId = 2,
                    Score = 4,
                    Comment = "Excellent dish, everyone loved it.",
                    CreatedAt = seedDate.AddDays(-3)
                },
                new Rating
                {
                    Id = 7,
                    RecipeId = 4,
                    UserId = 3,
                    Score = 5,
                    Comment = "Great taste, will cook this again.",
                    CreatedAt = seedDate.AddDays(-20)
                },
                new Rating
                {
                    Id = 8,
                    RecipeId = 4,
                    UserId = 4,
                    Score = 5,
                    Comment = "Simple but very tasty recipe.",
                    CreatedAt = seedDate.AddDays(-18)
                },
                new Rating
                {
                    Id = 9,
                    RecipeId = 5,
                    UserId = 1,
                    Score = 4,
                    Comment = "Perfect balance of flavors.",
                    CreatedAt = seedDate.AddDays(-7)
                },
                new Rating
                {
                    Id = 10,
                    RecipeId = 5,
                    UserId = 2,
                    Score = 5,
                    Comment = "Great recipe for beginners.",
                    CreatedAt = seedDate.AddDays(-4)
                }
            };

            modelBuilder.Entity<Rating>().HasData(ratings);
        }
    }
}
