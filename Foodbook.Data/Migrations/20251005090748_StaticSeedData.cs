using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Foodbook.Data.Migrations
{
    /// <inheritdoc />
    public partial class StaticSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Name", "Quantity", "Unit" },
                values: new object[] { new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Tomato", 5m, "piece" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Name", "Quantity", "Unit" },
                values: new object[] { new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Onion", 5m, "piece" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Name" },
                values: new object[] { new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Garlic" });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CreatedAt", "Name", "Quantity", "Unit", "UserId" },
                values: new object[,]
                {
                    { 4, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Carrot", 5m, "piece", 1 },
                    { 5, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Potato", 5m, "piece", 1 },
                    { 6, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Bell Pepper", 5m, "piece", 1 },
                    { 7, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Spinach", 5m, "piece", 1 },
                    { 8, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Broccoli", 5m, "piece", 1 },
                    { 9, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Mushroom", 5m, "piece", 1 },
                    { 10, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Cucumber", 5m, "piece", 1 },
                    { 41, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Salt", 5m, "teaspoon", 1 },
                    { 42, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Black Pepper", 5m, "teaspoon", 1 },
                    { 43, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Garlic Powder", 5m, "teaspoon", 1 },
                    { 44, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Oregano", 5m, "teaspoon", 1 },
                    { 45, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Basil", 5m, "teaspoon", 1 },
                    { 46, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Thyme", 5m, "teaspoon", 1 },
                    { 47, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Rosemary", 5m, "teaspoon", 1 },
                    { 48, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Paprika", 5m, "teaspoon", 1 },
                    { 49, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Cumin", 5m, "teaspoon", 1 },
                    { 50, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Cinnamon", 5m, "teaspoon", 1 }
                });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "CookTime", "CreatedAt", "Description", "Difficulty", "ImageUrl", "Instructions", "Servings", "Title", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, 25, new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Creamy Italian pasta with eggs and cheese", "Medium", "https://example.com/images/classic-spaghetti-carbonara.jpg", "1. Cook pasta according to package directions\n2. Whisk eggs with cheese in a bowl\n3. Cook pancetta until crispy\n4. Toss hot pasta with egg mixture\n5. Add pancetta and serve immediately", 4, "Classic Spaghetti Carbonara", new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 4, 120, new DateTime(2023, 12, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Elegant beef tenderloin wrapped in puff pastry", "Hard", "https://example.com/images/beef-wellington.jpg", "1. Season and sear beef tenderloin\n2. Wrap in mushroom duxelles\n3. Wrap in puff pastry\n4. Bake at 400°F for 25-30 minutes\n5. Let rest before slicing", 6, "Beef Wellington", new DateTime(2023, 12, 24, 0, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsAdmin", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { 2, new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), "mario@foodbook.com", false, "$2a$11$rQZ8K9vQZ8K9vQZ8K9vQZ8O", "chef_mario" },
                    { 3, new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), "lisa@foodbook.com", false, "$2a$11$rQZ8K9vQZ8K9vQZ8K9vQZ8O", "home_cook_lisa" },
                    { 4, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), "john@foodbook.com", false, "$2a$11$rQZ8K9vQZ8K9vQZ8K9vQZ8O", "foodie_john" }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CreatedAt", "Name", "Quantity", "Unit", "UserId" },
                values: new object[,]
                {
                    { 11, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken Breast", 500m, "gram", 2 },
                    { 12, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Beef", 500m, "gram", 2 },
                    { 13, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Salmon", 500m, "gram", 2 },
                    { 14, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Tofu", 500m, "gram", 2 },
                    { 15, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Eggs", 500m, "gram", 2 },
                    { 16, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Pork", 500m, "gram", 2 },
                    { 17, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Shrimp", 500m, "gram", 2 },
                    { 18, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Turkey", 500m, "gram", 2 },
                    { 19, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Lamb", 500m, "gram", 2 },
                    { 20, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Fish", 500m, "gram", 2 },
                    { 21, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Rice", 2m, "cup", 3 },
                    { 22, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Pasta", 2m, "cup", 3 },
                    { 23, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Bread", 2m, "cup", 3 },
                    { 24, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Quinoa", 2m, "cup", 3 },
                    { 25, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Oats", 2m, "cup", 3 },
                    { 26, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Barley", 2m, "cup", 3 },
                    { 27, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Couscous", 2m, "cup", 3 },
                    { 28, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Noodles", 2m, "cup", 3 },
                    { 29, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Tortilla", 2m, "cup", 3 },
                    { 30, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), "Crackers", 2m, "cup", 3 },
                    { 31, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Milk", 1m, "cup", 4 },
                    { 32, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Cheese", 1m, "cup", 4 },
                    { 33, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Yogurt", 1m, "cup", 4 },
                    { 34, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Butter", 1m, "cup", 4 },
                    { 35, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Cream", 1m, "cup", 4 },
                    { 36, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Sour Cream", 1m, "cup", 4 },
                    { 37, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Cottage Cheese", 1m, "cup", 4 },
                    { 38, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Mozzarella", 1m, "cup", 4 },
                    { 39, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Parmesan", 1m, "cup", 4 },
                    { 40, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Ricotta", 1m, "cup", 4 }
                });

            migrationBuilder.InsertData(
                table: "Ratings",
                columns: new[] { "Id", "Comment", "CreatedAt", "RecipeId", "Score", "UserId" },
                values: new object[,]
                {
                    { 1, "Absolutely delicious! Will definitely make again.", new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5, 2 },
                    { 2, "Great recipe, easy to follow instructions.", new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, 3 },
                    { 7, "Great taste, will cook this again.", new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), 4, 5, 3 },
                    { 8, "Simple but very tasty recipe.", new DateTime(2023, 12, 14, 0, 0, 0, 0, DateTimeKind.Utc), 4, 5, 4 }
                });

            migrationBuilder.InsertData(
                table: "RecipeIngredients",
                columns: new[] { "IngredientId", "RecipeId", "Notes", "Quantity", "Unit" },
                values: new object[,]
                {
                    { 1, 1, null, 2m, "cup" },
                    { 2, 1, null, 3m, "piece" },
                    { 3, 1, null, 1m, "cup" },
                    { 4, 1, null, 2m, "piece" }
                });

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "CookTime", "CreatedAt", "Description", "Difficulty", "ImageUrl", "Instructions", "Servings", "Title", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 2, 20, new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Fresh salmon grilled with aromatic herbs", "Easy", "https://example.com/images/grilled-salmon-with-herbs.jpg", "1. Season salmon with herbs and olive oil\n2. Preheat grill to medium-high\n3. Grill salmon 4-5 minutes per side\n4. Let rest before serving\n5. Garnish with fresh herbs", 2, "Grilled Salmon with Herbs", new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 3, 30, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Healthy bowl with quinoa, vegetables, and tahini dressing", "Easy", "https://example.com/images/vegetarian-buddha-bowl.jpg", "1. Cook quinoa according to package directions\n2. Roast vegetables in oven\n3. Prepare tahini dressing\n4. Assemble bowl with quinoa, vegetables\n5. Drizzle with dressing and serve", 2, "Vegetarian Buddha Bowl", new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 5, 35, new DateTime(2023, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Spicy coconut curry with vegetables and rice", "Medium", "https://example.com/images/thai-green-curry.jpg", "1. Heat coconut milk in large pot\n2. Add curry paste and stir\n3. Add vegetables and protein\n4. Simmer until cooked through\n5. Serve over rice with fresh herbs", 4, "Thai Green Curry", new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), 4 }
                });

            migrationBuilder.InsertData(
                table: "Ratings",
                columns: new[] { "Id", "Comment", "CreatedAt", "RecipeId", "Score", "UserId" },
                values: new object[,]
                {
                    { 3, "Perfect for a family dinner.", new DateTime(2023, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), 2, 5, 1 },
                    { 4, "Amazing flavors, highly recommend!", new DateTime(2023, 12, 24, 0, 0, 0, 0, DateTimeKind.Utc), 2, 4, 4 },
                    { 5, "Quick and easy, perfect for weeknights.", new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), 3, 5, 1 },
                    { 6, "Excellent dish, everyone loved it.", new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), 3, 4, 2 },
                    { 9, "Perfect balance of flavors.", new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Utc), 5, 4, 1 },
                    { 10, "Great recipe for beginners.", new DateTime(2023, 12, 28, 0, 0, 0, 0, DateTimeKind.Utc), 5, 5, 2 }
                });

            migrationBuilder.InsertData(
                table: "RecipeIngredients",
                columns: new[] { "IngredientId", "RecipeId", "Notes", "Quantity", "Unit" },
                values: new object[,]
                {
                    { 5, 2, null, 300m, "gram" },
                    { 6, 2, null, 2m, "teaspoon" },
                    { 7, 2, null, 1m, "teaspoon" },
                    { 8, 2, null, 1m, "teaspoon" },
                    { 9, 3, null, 1m, "cup" },
                    { 10, 3, null, 2m, "piece" },
                    { 11, 3, null, 1m, "piece" },
                    { 12, 3, null, 1m, "cup" },
                    { 13, 4, null, 800m, "gram" },
                    { 14, 4, null, 2m, "piece" },
                    { 15, 4, null, 1m, "piece" },
                    { 16, 4, null, 1m, "piece" },
                    { 17, 5, null, 1m, "cup" },
                    { 18, 5, null, 2m, "piece" },
                    { 19, 5, null, 1m, "piece" },
                    { 20, 5, null, 1m, "piece" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Ratings",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 6, 2 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 8, 2 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 9, 3 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 10, 3 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 11, 3 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 12, 3 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 13, 4 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 14, 4 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 15, 4 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 16, 4 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 17, 5 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 18, 5 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 19, 5 });

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 20, 5 });

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Name", "Quantity", "Unit" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rice", 10m, "cup" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Name", "Quantity", "Unit" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chicken", 500m, "gram" });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Name" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Onion" });
        }
    }
}
