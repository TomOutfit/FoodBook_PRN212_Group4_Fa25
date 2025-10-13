-- =============================================
-- FoodBook Database Schema
-- Created: 2025-01-08
-- Description: Complete database schema for FoodBook application
-- =============================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'FoodBook')
BEGIN
    CREATE DATABASE [FoodBook]
END
GO

USE [FoodBook]
GO

-- =============================================
-- Tables Creation
-- =============================================

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Username] [nvarchar](100) NOT NULL,
        [Email] [nvarchar](255) NOT NULL,
        [Password] [nvarchar](255) NOT NULL,
        [PasswordHash] [nvarchar](255) NOT NULL,
        [IsAdmin] [bit] NOT NULL DEFAULT(0),
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- Recipes Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Recipes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Recipes](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](1000) NULL,
        [Instructions] [nvarchar](max) NOT NULL,
        [CookTime] [int] NOT NULL,
        [Servings] [int] NOT NULL,
        [Difficulty] [nvarchar](50) NOT NULL,
        [Category] [nvarchar](100) NULL,
        [ImageUrl] [nvarchar](500) NULL,
        [UserId] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_Recipes] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- Ingredients Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ingredients]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Ingredients](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Category] [nvarchar](100) NULL,
        [NutritionInfo] [nvarchar](500) NULL,
        [Unit] [nvarchar](50) NULL,
        [Quantity] [decimal](10,2) NULL,
        [UserId] [int] NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_Ingredients] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- RecipeIngredients Table (Many-to-Many)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecipeIngredients]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RecipeIngredients](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RecipeId] [int] NOT NULL,
        [IngredientId] [int] NOT NULL,
        [Quantity] [decimal](10,2) NOT NULL,
        [Unit] [nvarchar](50) NOT NULL,
        [Notes] [nvarchar](200) NULL,
        CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- Ratings Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ratings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Ratings](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RecipeId] [int] NOT NULL,
        [UserId] [int] NOT NULL,
        [Rating] [int] NOT NULL,
        [Comment] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        CONSTRAINT [PK_Ratings] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- LogEntries Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LogEntries]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LogEntries](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Level] [nvarchar](50) NOT NULL,
        [Message] [nvarchar](max) NOT NULL,
        [Exception] [nvarchar](max) NULL,
        [Timestamp] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
        [Source] [nvarchar](200) NULL,
        [UserId] [int] NULL,
        [FeatureName] [nvarchar](100) NULL,
        [LogType] [nvarchar](50) NULL,
        [Duration] [time](7) NULL,
        [Details] [nvarchar](max) NULL,
        [Context] [nvarchar](max) NULL,
        [ExceptionType] [nvarchar](100) NULL,
        [StackTrace] [nvarchar](max) NULL,
        CONSTRAINT [PK_LogEntries] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- =============================================
-- Indexes Creation
-- =============================================

-- Users Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email] ASC)
END
GO

-- Recipes Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Recipes_UserId' AND object_id = OBJECT_ID('Recipes'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Recipes_UserId] ON [dbo].[Recipes] ([UserId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Recipes_Category' AND object_id = OBJECT_ID('Recipes'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Recipes_Category] ON [dbo].[Recipes] ([Category] ASC)
END
GO

-- RecipeIngredients Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RecipeIngredients_RecipeId' AND object_id = OBJECT_ID('RecipeIngredients'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RecipeIngredients_RecipeId] ON [dbo].[RecipeIngredients] ([RecipeId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RecipeIngredients_IngredientId' AND object_id = OBJECT_ID('RecipeIngredients'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_RecipeIngredients_IngredientId] ON [dbo].[RecipeIngredients] ([IngredientId] ASC)
END
GO

-- Ratings Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ratings_RecipeId' AND object_id = OBJECT_ID('Ratings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Ratings_RecipeId] ON [dbo].[Ratings] ([RecipeId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ratings_UserId' AND object_id = OBJECT_ID('Ratings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Ratings_UserId] ON [dbo].[Ratings] ([UserId] ASC)
END
GO

-- =============================================
-- Foreign Key Constraints
-- =============================================

-- Recipes -> Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Recipes_Users_UserId')
BEGIN
    ALTER TABLE [dbo].[Recipes] 
    ADD CONSTRAINT [FK_Recipes_Users_UserId] 
    FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
END
GO

-- RecipeIngredients -> Recipes
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RecipeIngredients_Recipes_RecipeId')
BEGIN
    ALTER TABLE [dbo].[RecipeIngredients] 
    ADD CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] 
    FOREIGN KEY([RecipeId]) REFERENCES [dbo].[Recipes] ([Id]) ON DELETE CASCADE
END
GO

-- RecipeIngredients -> Ingredients
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RecipeIngredients_Ingredients_IngredientId')
BEGIN
    ALTER TABLE [dbo].[RecipeIngredients] 
    ADD CONSTRAINT [FK_RecipeIngredients_Ingredients_IngredientId] 
    FOREIGN KEY([IngredientId]) REFERENCES [dbo].[Ingredients] ([Id]) ON DELETE CASCADE
END
GO

-- Ratings -> Recipes
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Ratings_Recipes_RecipeId')
BEGIN
    ALTER TABLE [dbo].[Ratings] 
    ADD CONSTRAINT [FK_Ratings_Recipes_RecipeId] 
    FOREIGN KEY([RecipeId]) REFERENCES [dbo].[Recipes] ([Id]) ON DELETE CASCADE
END
GO

-- Ratings -> Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Ratings_Users_UserId')
BEGIN
    ALTER TABLE [dbo].[Ratings] 
    ADD CONSTRAINT [FK_Ratings_Users_UserId] 
    FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION
END
GO

-- LogEntries -> Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LogEntries_Users_UserId')
BEGIN
    ALTER TABLE [dbo].[LogEntries] 
    ADD CONSTRAINT [FK_LogEntries_Users_UserId] 
    FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE SET NULL
END
GO

-- Ingredients -> Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Ingredients_Users_UserId')
BEGIN
    ALTER TABLE [dbo].[Ingredients] 
    ADD CONSTRAINT [FK_Ingredients_Users_UserId] 
    FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE SET NULL
END
GO

-- =============================================
-- RICH SAMPLE DATA - SUPER DIVERSE & ABUNDANT
-- =============================================

-- Insert Rich Users Data (100+ users from different cultures and backgrounds)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@foodbook.com')
BEGIN
    INSERT INTO Users (Username, Email, Password, PasswordHash, IsAdmin, CreatedAt)
    VALUES 
        -- Admin and Professional Chefs
        ('admin', 'admin@foodbook.com', 'admin123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 1, GETUTCDATE()),
        ('master_chef', 'chef@foodbook.com', 'chef123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('head_chef_michelin', 'michelin@foodbook.com', 'michelin123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('pastry_chef', 'pastry@foodbook.com', 'pastry123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('sous_chef', 'sous@foodbook.com', 'sous123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Asian Cuisine Specialists
        ('italian_cook', 'maria@foodbook.com', 'italy123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('asian_chef', 'li@foodbook.com', 'asia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('chinese_master', 'chen@foodbook.com', 'china123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('japanese_cook', 'yuki@foodbook.com', 'japan123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('korean_chef', 'minho@foodbook.com', 'korea123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('thai_chef', 'somchai@foodbook.com', 'thai123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('vietnamese_cook', 'linh@foodbook.com', 'vietnam123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('indian_cook', 'priya@foodbook.com', 'india123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('indonesian_chef', 'budi@foodbook.com', 'indonesia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('filipino_cook', 'filipino@foodbook.com', 'philippines123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- European Cuisine Specialists
        ('french_cook', 'pierre@foodbook.com', 'france123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('italian_master', 'giuseppe@foodbook.com', 'italy123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('spanish_cook', 'carmen@foodbook.com', 'spain123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('german_chef', 'hans@foodbook.com', 'germany123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('greek_chef', 'dimitri@foodbook.com', 'greece123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('british_cook', 'james@foodbook.com', 'britain123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('russian_chef', 'vladimir@foodbook.com', 'russia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('polish_cook', 'anna@foodbook.com', 'poland123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('dutch_chef', 'willem@foodbook.com', 'netherlands123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('swedish_cook', 'erik@foodbook.com', 'sweden123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Middle Eastern & African Cuisine
        ('turkish_cook', 'ahmet@foodbook.com', 'turkey123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('lebanese_cook', 'nour@foodbook.com', 'lebanon123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('persian_chef', 'darius@foodbook.com', 'persia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('moroccan_cook', 'fatima@foodbook.com', 'morocco123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('egyptian_chef', 'ahmed@foodbook.com', 'egypt123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('ethiopian_chef', 'abebe@foodbook.com', 'ethiopia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('nigerian_cook', 'chukwu@foodbook.com', 'nigeria123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('south_african_chef', 'thabo@foodbook.com', 'southafrica123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('kenyan_cook', 'wanjiku@foodbook.com', 'kenya123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('tunisian_chef', 'youssef@foodbook.com', 'tunisia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Latin American Cuisine
        ('mexican_chef', 'carlos@foodbook.com', 'mexico123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('brazilian_chef', 'joao@foodbook.com', 'brazil123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('peruvian_chef', 'diego@foodbook.com', 'peru123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('argentine_cook', 'argentine@foodbook.com', 'argentina123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('chilean_chef', 'pablo@foodbook.com', 'chile123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('colombian_cook', 'colombian@foodbook.com', 'colombia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('venezuelan_chef', 'jose@foodbook.com', 'venezuela123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('ecuadorian_cook', 'ana@foodbook.com', 'ecuador123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('bolivian_chef', 'luis@foodbook.com', 'bolivia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('uruguayan_cook', 'sofia@foodbook.com', 'uruguay123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- North American Cuisine
        ('american_chef', 'john@foodbook.com', 'america123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('canadian_cook', 'mike@foodbook.com', 'canada123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('cajun_chef', 'louis@foodbook.com', 'cajun123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('tex_mex_cook', 'pedro@foodbook.com', 'texmex123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('southern_chef', 'billy@foodbook.com', 'southern123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Home Cooks and Food Enthusiasts
        ('home_cook_lisa', 'lisa@foodbook.com', 'lisa123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('foodie_john', 'foodie@foodbook.com', 'john123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('baking_enthusiast', 'sarah@foodbook.com', 'baking123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('grill_master', 'tom@foodbook.com', 'grill123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('vegetarian_cook', 'emma@foodbook.com', 'veggie123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('vegan_chef', 'vegan@foodbook.com', 'vegan123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('keto_cook', 'keto@foodbook.com', 'keto123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('paleo_chef', 'jessica@foodbook.com', 'paleo123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('gluten_free_cook', 'glutenfree@foodbook.com', 'glutenfree123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('dairy_free_chef', 'rachel@foodbook.com', 'dairyfree123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- International Food Bloggers
        ('food_blogger_asia', 'blogger1@foodbook.com', 'blogger123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_blogger_europe', 'blogger2@foodbook.com', 'blogger123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_blogger_america', 'blogger3@foodbook.com', 'blogger123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_blogger_africa', 'blogger4@foodbook.com', 'blogger123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_blogger_middle_east', 'blogger5@foodbook.com', 'blogger123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Restaurant Owners and Managers
        ('restaurant_owner_1', 'owner1@foodbook.com', 'owner123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('restaurant_owner_2', 'owner2@foodbook.com', 'owner123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('restaurant_owner_3', 'owner3@foodbook.com', 'owner123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('restaurant_owner_4', 'owner4@foodbook.com', 'owner123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('restaurant_owner_5', 'owner5@foodbook.com', 'owner123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Culinary Students and Apprentices
        ('culinary_student_1', 'student1@foodbook.com', 'student123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('culinary_student_2', 'student2@foodbook.com', 'student123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('culinary_student_3', 'student3@foodbook.com', 'student123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('culinary_student_4', 'student4@foodbook.com', 'student123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('culinary_student_5', 'student5@foodbook.com', 'student123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Food Critics and Reviewers
        ('food_critic_1', 'critic1@foodbook.com', 'critic123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_critic_2', 'critic2@foodbook.com', 'critic123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_critic_3', 'critic3@foodbook.com', 'critic123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_critic_4', 'critic4@foodbook.com', 'critic123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_critic_5', 'critic5@foodbook.com', 'critic123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- More Diverse International Users
        ('australian_chef', 'aussie@foodbook.com', 'australia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('new_zealand_cook', 'kiwi@foodbook.com', 'newzealand123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('israeli_chef', 'moshe@foodbook.com', 'israel123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('armenian_cook', 'vardan@foodbook.com', 'armenia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('georgian_chef', 'giorgi@foodbook.com', 'georgia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('azerbaijani_cook', 'elvin@foodbook.com', 'azerbaijan123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('uzbek_chef', 'alisher@foodbook.com', 'uzbekistan123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('kazakh_cook', 'nurbol@foodbook.com', 'kazakhstan123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('mongolian_chef', 'batbayar@foodbook.com', 'mongolia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('tibetan_cook', 'tenzin@foodbook.com', 'tibet123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Additional Professional Chefs
        ('executive_chef_1', 'exec1@foodbook.com', 'exec123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('executive_chef_2', 'exec2@foodbook.com', 'exec123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('executive_chef_3', 'exec3@foodbook.com', 'exec123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('executive_chef_4', 'exec4@foodbook.com', 'exec123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('executive_chef_5', 'exec5@foodbook.com', 'exec123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Food Truck Owners
        ('food_truck_owner_1', 'truck1@foodbook.com', 'truck123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_truck_owner_2', 'truck2@foodbook.com', 'truck123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_truck_owner_3', 'truck3@foodbook.com', 'truck123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_truck_owner_4', 'truck4@foodbook.com', 'truck123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('food_truck_owner_5', 'truck5@foodbook.com', 'truck123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Catering Professionals
        ('caterer_1', 'cater1@foodbook.com', 'cater123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('caterer_2', 'cater2@foodbook.com', 'cater123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('caterer_3', 'cater3@foodbook.com', 'cater123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('caterer_4', 'cater4@foodbook.com', 'cater123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('caterer_5', 'cater5@foodbook.com', 'cater123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- More International Cuisine Specialists
        ('pakistani_chef', 'pakistani@foodbook.com', 'pakistan123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('bangladeshi_cook', 'rahman@foodbook.com', 'bangladesh123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('sri_lankan_chef', 'kumar@foodbook.com', 'srilanka123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('nepalese_cook', 'ram@foodbook.com', 'nepal123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('bhutanese_chef', 'pema@foodbook.com', 'bhutan123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('myanmar_cook', 'aung@foodbook.com', 'myanmar123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('cambodian_chef', 'sophea@foodbook.com', 'cambodia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('laotian_cook', 'bounmy@foodbook.com', 'laos123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('malaysian_chef', 'ahmad@foodbook.com', 'malaysia123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('singaporean_cook', 'lim@foodbook.com', 'singapore123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        
        -- Final batch to reach 100 users
        ('user_91', 'user91@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_92', 'user92@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_93', 'user93@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_94', 'user94@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_95', 'user95@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_96', 'user96@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_97', 'user97@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_98', 'user98@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_99', 'user99@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE()),
        ('user_100', 'user100@foodbook.com', 'user123', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 0, GETUTCDATE())
END
GO

-- Insert Rich Ingredients Data (100+ diverse ingredients)
IF NOT EXISTS (SELECT 1 FROM Ingredients WHERE Name = 'Chicken Breast')
BEGIN
    INSERT INTO Ingredients (Name, Category, NutritionInfo, Unit, Quantity, UserId, CreatedAt)
    VALUES 
        -- Proteins
        ('Chicken Breast', 'Protein', 'High protein, low fat, 165 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Salmon Fillet', 'Protein', 'Omega-3 fatty acids, 208 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Beef Tenderloin', 'Protein', 'Iron-rich, 250 cal/100g', 'kg', 0.8, 1, GETUTCDATE()),
        ('Pork Belly', 'Protein', 'High fat content, 518 cal/100g', 'kg', 0.6, 1, GETUTCDATE()),
        ('Lamb Chops', 'Protein', 'Zinc and B12, 294 cal/100g', 'kg', 0.4, 1, GETUTCDATE()),
        ('Duck Breast', 'Protein', 'Rich flavor, 337 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Tofu', 'Protein', 'Plant protein, 76 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Tempeh', 'Protein', 'Fermented soy, 192 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Shrimp', 'Protein', 'Low calorie, 99 cal/100g', 'kg', 0.4, 1, GETUTCDATE()),
        ('Crab', 'Protein', 'Vitamin B12, 97 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        
        -- Grains & Starches
        ('Jasmine Rice', 'Grain', 'Aromatic, 130 cal/100g', 'kg', 2.0, 1, GETUTCDATE()),
        ('Basmati Rice', 'Grain', 'Long grain, 130 cal/100g', 'kg', 1.5, 1, GETUTCDATE()),
        ('Brown Rice', 'Grain', 'Fiber-rich, 111 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Quinoa', 'Grain', 'Complete protein, 120 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Barley', 'Grain', 'Beta-glucan fiber, 352 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Wheat Flour', 'Grain', 'Gluten protein, 364 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Pasta', 'Grain', 'Durum wheat, 131 cal/100g', 'kg', 0.8, 1, GETUTCDATE()),
        ('Bread', 'Grain', 'Carbohydrates, 265 cal/100g', 'loaf', 2, 1, GETUTCDATE()),
        
        -- Vegetables
        ('Onion', 'Vegetable', 'Quercetin antioxidants, 40 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Garlic', 'Vegetable', 'Allicin compound, 149 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Tomato', 'Vegetable', 'Lycopene, 18 cal/100g', 'kg', 2.0, 1, GETUTCDATE()),
        ('Bell Pepper', 'Vegetable', 'Vitamin C, 31 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Carrot', 'Vegetable', 'Beta-carotene, 41 cal/100g', 'kg', 1.5, 1, GETUTCDATE()),
        ('Potato', 'Vegetable', 'Potassium, 77 cal/100g', 'kg', 2.0, 1, GETUTCDATE()),
        ('Sweet Potato', 'Vegetable', 'Vitamin A, 86 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Broccoli', 'Vegetable', 'Sulforaphane, 34 cal/100g', 'kg', 0.8, 1, GETUTCDATE()),
        ('Spinach', 'Vegetable', 'Iron and folate, 23 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Kale', 'Vegetable', 'Vitamin K, 49 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Eggplant', 'Vegetable', 'Nasunin antioxidant, 25 cal/100g', 'kg', 0.6, 1, GETUTCDATE()),
        ('Zucchini', 'Vegetable', 'Low calorie, 17 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Mushrooms', 'Vegetable', 'Selenium, 22 cal/100g', 'kg', 0.4, 1, GETUTCDATE()),
        ('Asparagus', 'Vegetable', 'Folate, 20 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        
        -- Fruits
        ('Apple', 'Fruit', 'Pectin fiber, 52 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Banana', 'Fruit', 'Potassium, 89 cal/100g', 'kg', 0.8, 1, GETUTCDATE()),
        ('Orange', 'Fruit', 'Vitamin C, 47 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Lemon', 'Fruit', 'Citric acid, 29 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Lime', 'Fruit', 'Vitamin C, 30 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Avocado', 'Fruit', 'Healthy fats, 160 cal/100g', 'kg', 0.6, 1, GETUTCDATE()),
        ('Mango', 'Fruit', 'Vitamin A, 60 cal/100g', 'kg', 0.8, 1, GETUTCDATE()),
        ('Pineapple', 'Fruit', 'Bromelain enzyme, 50 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        
        -- Dairy & Eggs
        ('Eggs', 'Dairy', 'Complete protein, 155 cal/100g', 'dozen', 2, 1, GETUTCDATE()),
        ('Milk', 'Dairy', 'Calcium, 42 cal/100g', 'liter', 2, 1, GETUTCDATE()),
        ('Cheese', 'Dairy', 'Calcium, 113 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Yogurt', 'Dairy', 'Probiotics, 59 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Butter', 'Dairy', 'Saturated fat, 717 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Cream', 'Dairy', 'High fat, 345 cal/100g', 'liter', 0.5, 1, GETUTCDATE()),
        
        -- Oils & Fats
        ('Olive Oil', 'Fat', 'Monounsaturated, 884 cal/100g', 'liter', 1.0, 1, GETUTCDATE()),
        ('Coconut Oil', 'Fat', 'Medium-chain triglycerides, 862 cal/100g', 'liter', 0.5, 1, GETUTCDATE()),
        ('Sesame Oil', 'Fat', 'Sesame flavor, 884 cal/100g', 'liter', 0.3, 1, GETUTCDATE()),
        ('Avocado Oil', 'Fat', 'High smoke point, 884 cal/100g', 'liter', 0.4, 1, GETUTCDATE()),
        
        -- Herbs & Spices
        ('Salt', 'Seasoning', 'Sodium chloride, 0 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Black Pepper', 'Seasoning', 'Piperine, 251 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Cumin', 'Seasoning', 'Iron, 375 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Coriander', 'Seasoning', 'Antioxidants, 298 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Turmeric', 'Seasoning', 'Curcumin, 354 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Ginger', 'Seasoning', 'Gingerol, 80 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Cinnamon', 'Seasoning', 'Cinnamaldehyde, 247 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Paprika', 'Seasoning', 'Capsaicin, 282 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Cayenne', 'Seasoning', 'Capsaicin, 318 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Oregano', 'Seasoning', 'Antioxidants, 265 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Thyme', 'Seasoning', 'Thymol, 276 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Rosemary', 'Seasoning', 'Rosmarinic acid, 331 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Basil', 'Seasoning', 'Eugenol, 22 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Parsley', 'Seasoning', 'Vitamin K, 36 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Cilantro', 'Seasoning', 'Antioxidants, 23 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Mint', 'Seasoning', 'Menthol, 70 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Bay Leaves', 'Seasoning', 'Essential oils, 313 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Star Anise', 'Seasoning', 'Anethole, 337 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Cardamom', 'Seasoning', 'Essential oils, 311 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Cloves', 'Seasoning', 'Eugenol, 274 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Nutmeg', 'Seasoning', 'Myristicin, 525 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Allspice', 'Seasoning', 'Eugenol, 263 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Fennel Seeds', 'Seasoning', 'Anethole, 345 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Mustard Seeds', 'Seasoning', 'Glucosinolates, 508 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Fenugreek', 'Seasoning', 'Saponins, 323 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Sumac', 'Seasoning', 'Tannins, 331 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        ('Zaatar', 'Seasoning', 'Middle Eastern blend, 300 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Harissa', 'Seasoning', 'Chili paste, 200 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Saffron', 'Seasoning', 'Crocin, 310 cal/100g', 'kg', 0.01, 1, GETUTCDATE()),
        ('Vanilla', 'Seasoning', 'Vanillin, 288 cal/100g', 'kg', 0.05, 1, GETUTCDATE()),
        
        -- Nuts & Seeds
        ('Almonds', 'Nuts', 'Vitamin E, 579 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Walnuts', 'Nuts', 'Omega-3, 654 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Cashews', 'Nuts', 'Magnesium, 553 cal/100g', 'kg', 0.4, 1, GETUTCDATE()),
        ('Pistachios', 'Nuts', 'Protein, 560 cal/100g', 'kg', 0.3, 1, GETUTCDATE()),
        ('Pecans', 'Nuts', 'Monounsaturated fat, 691 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Hazelnuts', 'Nuts', 'Folate, 628 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Macadamia', 'Nuts', 'Monounsaturated fat, 718 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Brazil Nuts', 'Nuts', 'Selenium, 659 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Pine Nuts', 'Nuts', 'Vitamin E, 673 cal/100g', 'kg', 0.1, 1, GETUTCDATE()),
        ('Sesame Seeds', 'Nuts', 'Calcium, 573 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Chia Seeds', 'Nuts', 'Omega-3, 486 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Flax Seeds', 'Nuts', 'Lignans, 534 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Pumpkin Seeds', 'Nuts', 'Zinc, 559 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        ('Sunflower Seeds', 'Nuts', 'Vitamin E, 584 cal/100g', 'kg', 0.2, 1, GETUTCDATE()),
        
        -- Legumes
        ('Lentils', 'Legume', 'Protein, 116 cal/100g', 'kg', 1.0, 1, GETUTCDATE()),
        ('Chickpeas', 'Legume', 'Fiber, 164 cal/100g', 'kg', 0.8, 1, GETUTCDATE()),
        ('Black Beans', 'Legume', 'Antioxidants, 132 cal/100g', 'kg', 0.6, 1, GETUTCDATE()),
        ('Kidney Beans', 'Legume', 'Folate, 127 cal/100g', 'kg', 0.6, 1, GETUTCDATE()),
        ('Navy Beans', 'Legume', 'Fiber, 140 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Pinto Beans', 'Legume', 'Protein, 143 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Cannellini Beans', 'Legume', 'Iron, 114 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Split Peas', 'Legume', 'Fiber, 118 cal/100g', 'kg', 0.5, 1, GETUTCDATE()),
        ('Mung Beans', 'Legume', 'Protein, 105 cal/100g', 'kg', 0.4, 1, GETUTCDATE()),
        ('Adzuki Beans', 'Legume', 'Antioxidants, 128 cal/100g', 'kg', 0.3, 1, GETUTCDATE())
END
GO

-- Insert Rich Recipes Data (50+ diverse recipes from different cuisines)
IF NOT EXISTS (SELECT 1 FROM Recipes WHERE Title = 'Classic Chicken Fried Rice')
BEGIN
    INSERT INTO Recipes (Title, Description, Instructions, CookTime, Servings, Difficulty, Category, ImageUrl, UserId, CreatedAt, UpdatedAt)
    VALUES 
        -- Asian Cuisine
        ('Classic Chicken Fried Rice', 'A delicious and easy-to-make fried rice with chicken and vegetables', 
         '1. Cook jasmine rice and let it cool\n2. Heat sesame oil in a large wok\n3. Add diced chicken and cook until golden\n4. Add garlic, ginger, and vegetables\n5. Add rice and soy sauce\n6. Stir-fry until well combined and serve hot', 
         25, 4, 'Easy', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Thai Green Curry', 'Authentic Thai curry with coconut milk and fresh herbs', 
         '1. Heat coconut oil in a pot\n2. Add green curry paste and cook until fragrant\n3. Add coconut milk and bring to simmer\n4. Add chicken and vegetables\n5. Season with fish sauce and palm sugar\n6. Garnish with basil and serve with jasmine rice', 
         30, 4, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Japanese Ramen', 'Rich and flavorful ramen with soft-boiled eggs', 
         '1. Prepare dashi stock with kombu and bonito flakes\n2. Make tare sauce with soy sauce and mirin\n3. Cook ramen noodles according to package\n4. Assemble with toppings: egg, nori, scallions\n5. Pour hot broth over noodles\n6. Serve immediately', 
         45, 2, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Korean Bibimbap', 'Mixed rice bowl with vegetables and gochujang', 
         '1. Cook short-grain rice\n2. Prepare vegetables: spinach, carrots, bean sprouts\n3. Make bulgogi beef or tofu\n4. Fry eggs sunny-side up\n5. Arrange in bowl with rice\n6. Top with gochujang and sesame oil', 
         40, 2, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Chinese Kung Pao Chicken', 'Spicy Sichuan dish with peanuts and vegetables', 
         '1. Marinate chicken with soy sauce and cornstarch\n2. Heat oil and stir-fry chicken until cooked\n3. Add vegetables and dried chilies\n4. Add sauce: soy sauce, vinegar, sugar\n5. Toss with roasted peanuts\n6. Serve over steamed rice', 
         25, 4, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Vietnamese Pho', 'Traditional Vietnamese noodle soup', 
         '1. Make beef bone broth with spices\n2. Simmer for 4-6 hours\n3. Cook rice noodles\n4. Slice raw beef thinly\n5. Assemble with herbs: basil, cilantro, mint\n6. Serve with lime and hoisin sauce', 
         300, 4, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Indian Butter Chicken', 'Creamy tomato-based curry with tender chicken', 
         '1. Marinate chicken in yogurt and spices\n2. Grill or bake chicken\n3. Make sauce with tomatoes, cream, and spices\n4. Add chicken to sauce\n5. Simmer until flavors meld\n6. Garnish with cilantro and serve with naan', 
         60, 4, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Japanese Sushi Rolls', 'Fresh sushi with various fillings', 
         '1. Cook sushi rice with vinegar seasoning\n2. Prepare fillings: fish, vegetables, avocado\n3. Lay nori on bamboo mat\n4. Spread rice evenly\n5. Add fillings and roll tightly\n6. Slice and serve with wasabi and soy sauce', 
         90, 6, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- European Cuisine
        ('Italian Spaghetti Carbonara', 'Classic Roman pasta with eggs and pancetta', 
         '1. Cook spaghetti in salted water\n2. Fry pancetta until crispy\n3. Beat eggs with parmesan cheese\n4. Toss hot pasta with pancetta\n5. Add egg mixture off heat\n6. Serve immediately with black pepper', 
         20, 4, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('French Coq au Vin', 'Traditional French chicken in wine sauce', 
         '1. Marinate chicken in red wine overnight\n2. Brown chicken in butter\n3. Add vegetables and herbs\n4. Deglaze with wine and stock\n5. Simmer for 1-2 hours\n6. Serve with crusty bread', 
         180, 6, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Spanish Paella', 'Valencian rice dish with seafood and saffron', 
         '1. Heat oil in large paella pan\n2. Add sofrito: onions, tomatoes, garlic\n3. Add rice and toast briefly\n4. Add hot stock and saffron\n5. Add seafood and cook without stirring\n6. Let rest before serving', 
         45, 8, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('German Sauerbraten', 'Marinated beef roast with sweet-sour sauce', 
         '1. Marinate beef in vinegar and spices for 3 days\n2. Brown meat in hot oil\n3. Add marinade and vegetables\n4. Braise for 3-4 hours\n5. Strain and thicken sauce\n6. Serve with red cabbage and dumplings', 
         300, 8, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Greek Moussaka', 'Layered eggplant casserole with meat sauce', 
         '1. Slice and fry eggplant\n2. Make meat sauce with lamb and tomatoes\n3. Make béchamel sauce\n4. Layer eggplant, meat, and sauce\n5. Top with béchamel and cheese\n6. Bake until golden brown', 
         90, 8, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('British Fish and Chips', 'Crispy battered fish with thick-cut fries', 
         '1. Cut potatoes into thick chips\n2. Double-fry chips until crispy\n3. Make beer batter\n4. Dip fish in batter and deep-fry\n5. Drain on paper towels\n6. Serve with malt vinegar and mushy peas', 
         45, 4, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- American Cuisine
        ('Classic American Burger', 'Juicy beef patty with all the fixings', 
         '1. Form ground beef into patties\n2. Season with salt and pepper\n3. Grill or pan-fry to desired doneness\n4. Toast buns\n5. Assemble with lettuce, tomato, onion\n6. Add condiments and serve', 
         20, 4, 'Easy', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Southern BBQ Ribs', 'Slow-cooked ribs with tangy barbecue sauce', 
         '1. Season ribs with dry rub\n2. Smoke or bake low and slow for 4-6 hours\n3. Make barbecue sauce\n4. Glaze ribs with sauce\n5. Finish on grill for char\n6. Serve with coleslaw and cornbread', 
         360, 6, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('New York Cheesecake', 'Rich and creamy classic cheesecake', 
         '1. Make graham cracker crust\n2. Beat cream cheese until smooth\n3. Add sugar, eggs, and vanilla\n4. Pour into springform pan\n5. Bake in water bath\n6. Chill overnight before serving', 
         120, 12, 'Medium', 'Dessert', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Tex-Mex Tacos', 'Spicy ground beef tacos with fresh toppings', 
         '1. Brown ground beef with taco seasoning\n2. Warm taco shells\n3. Prepare toppings: lettuce, tomato, cheese\n4. Fill shells with meat\n5. Add toppings and salsa\n6. Serve with sour cream and guacamole', 
         25, 6, 'Easy', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- Middle Eastern Cuisine
        ('Lebanese Hummus', 'Creamy chickpea dip with tahini', 
         '1. Soak chickpeas overnight\n2. Cook until very tender\n3. Blend with tahini, lemon, garlic\n4. Add olive oil gradually\n5. Season with salt and cumin\n6. Serve with pita bread and vegetables', 
         120, 8, 'Easy', 'Appetizer', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Persian Rice with Saffron', 'Fragrant basmati rice with crispy tahdig', 
         '1. Soak basmati rice for 30 minutes\n2. Parboil rice until half-cooked\n3. Make saffron water\n4. Layer rice in pot with butter\n5. Cook on high heat for crispy bottom\n6. Invert onto platter to show tahdig', 
         60, 6, 'Hard', 'Side Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Turkish Kebabs', 'Grilled meat skewers with vegetables', 
         '1. Marinate meat in yogurt and spices\n2. Thread onto skewers with vegetables\n3. Grill over high heat\n4. Turn frequently for even cooking\n5. Serve with rice pilaf\n6. Garnish with sumac and herbs', 
         30, 4, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Moroccan Tagine', 'Slow-cooked stew with aromatic spices', 
         '1. Brown meat in tagine\n2. Add onions and spices\n3. Add vegetables and dried fruits\n4. Add stock and cover\n5. Simmer for 2-3 hours\n6. Serve with couscous', 
         180, 6, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- African Cuisine
        ('Ethiopian Doro Wat', 'Spicy chicken stew with berbere spice', 
         '1. Make berbere spice blend\n2. Caramelize onions slowly\n3. Add berbere and cook until fragrant\n4. Add chicken and stock\n5. Simmer until tender\n6. Serve with injera bread', 
         90, 6, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('South African Bobotie', 'Spiced meat casserole with egg topping', 
         '1. Sauté onions and garlic\n2. Add ground meat and spices\n3. Add dried fruits and nuts\n4. Make egg custard topping\n5. Bake until set\n6. Serve with yellow rice', 
         75, 8, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- Latin American Cuisine
        ('Mexican Mole Poblano', 'Complex sauce with chocolate and chilies', 
         '1. Toast dried chilies and spices\n2. Blend with nuts and seeds\n3. Add chocolate and stock\n4. Simmer for hours until thick\n5. Strain through fine mesh\n6. Serve over chicken with rice', 
         240, 8, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Peruvian Ceviche', 'Fresh fish cured in citrus juice', 
         '1. Cut fresh fish into cubes\n2. Marinate in lime juice\n3. Add red onion and cilantro\n4. Season with salt and pepper\n5. Serve immediately with sweet potato\n6. Garnish with corn and chili', 
         30, 4, 'Easy', 'Appetizer', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Brazilian Feijoada', 'Black bean stew with various meats', 
         '1. Soak black beans overnight\n2. Cook with various meats\n3. Add vegetables and seasonings\n4. Simmer for 3-4 hours\n5. Serve with rice and farofa\n6. Garnish with orange slices', 
         300, 10, 'Hard', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Argentine Asado', 'Grilled beef with chimichurri sauce', 
         '1. Season beef with salt\n2. Grill over wood fire\n3. Make chimichurri sauce\n4. Cook to desired doneness\n5. Rest before slicing\n6. Serve with sauce and salad', 
         60, 8, 'Medium', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- Vegetarian & Vegan
        ('Mediterranean Buddha Bowl', 'Healthy grain bowl with vegetables', 
         '1. Cook quinoa or brown rice\n2. Roast vegetables with herbs\n3. Make tahini dressing\n4. Add chickpeas and avocado\n5. Arrange in bowl\n6. Drizzle with dressing', 
         45, 2, 'Easy', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Vegan Thai Curry', 'Coconut curry with tofu and vegetables', 
         '1. Press and cube tofu\n2. Make curry paste\n3. Heat coconut milk\n4. Add vegetables and tofu\n5. Simmer until tender\n6. Serve with jasmine rice', 
         30, 4, 'Easy', 'Main Dish', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Mediterranean Quinoa Salad', 'Protein-rich salad with fresh vegetables', 
         '1. Cook quinoa and cool\n2. Add chopped vegetables\n3. Add olives and feta\n4. Make lemon vinaigrette\n5. Toss everything together\n6. Chill before serving', 
         25, 6, 'Easy', 'Salad', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- Desserts
        ('French Macarons', 'Delicate almond cookies with filling', 
         '1. Make almond flour mixture\n2. Whip egg whites to stiff peaks\n3. Fold in dry ingredients\n4. Pipe onto baking sheets\n5. Bake until set\n6. Fill with ganache or buttercream', 
         120, 24, 'Hard', 'Dessert', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Italian Tiramisu', 'Coffee-flavored dessert with mascarpone', 
         '1. Make coffee mixture\n2. Beat mascarpone with sugar\n3. Dip ladyfingers in coffee\n4. Layer in dish\n5. Chill for 4 hours\n6. Dust with cocoa powder', 
         60, 8, 'Medium', 'Dessert', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Chocolate Lava Cake', 'Molten chocolate cake with warm center', 
         '1. Melt chocolate and butter\n2. Beat eggs with sugar\n3. Fold in chocolate mixture\n4. Add flour\n5. Bake in ramekins\n6. Serve warm with ice cream', 
         25, 4, 'Medium', 'Dessert', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Japanese Matcha Ice Cream', 'Green tea flavored ice cream', 
         '1. Heat milk and cream\n2. Whisk in matcha powder\n3. Add sugar and egg yolks\n4. Cook until thickened\n5. Chill and churn\n6. Freeze until firm', 
         180, 6, 'Medium', 'Dessert', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- Breakfast & Brunch
        ('French Toast', 'Classic breakfast with cinnamon and syrup', 
         '1. Beat eggs with milk and cinnamon\n2. Soak bread slices\n3. Cook in butter until golden\n4. Flip and cook other side\n5. Serve hot with syrup\n6. Garnish with berries', 
         15, 4, 'Easy', 'Breakfast', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Pancakes', 'Fluffy breakfast pancakes', 
         '1. Mix dry ingredients\n2. Whisk wet ingredients\n3. Combine until just mixed\n4. Cook on griddle\n5. Flip when bubbles form\n6. Serve with butter and syrup', 
         20, 6, 'Easy', 'Breakfast', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Eggs Benedict', 'Poached eggs with hollandaise sauce', 
         '1. Make hollandaise sauce\n2. Poach eggs\n3. Toast English muffins\n4. Add ham or bacon\n5. Top with eggs and sauce\n6. Garnish with chives', 
         30, 4, 'Medium', 'Breakfast', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Shakshuka', 'Middle Eastern eggs in tomato sauce', 
         '1. Sauté onions and peppers\n2. Add tomatoes and spices\n3. Simmer until thick\n4. Make wells for eggs\n5. Crack eggs into wells\n6. Cover and cook until set', 
         25, 4, 'Easy', 'Breakfast', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        
        -- Soups & Stews
        ('French Onion Soup', 'Rich soup with caramelized onions', 
         '1. Caramelize onions slowly\n2. Add wine and stock\n3. Simmer for 30 minutes\n4. Toast bread slices\n5. Add cheese and broil\n6. Serve hot', 
         90, 4, 'Medium', 'Soup', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Italian Minestrone', 'Vegetable soup with pasta and beans', 
         '1. Sauté vegetables\n2. Add tomatoes and stock\n3. Add beans and pasta\n4. Simmer until tender\n5. Add herbs and cheese\n6. Serve with bread', 
         60, 8, 'Easy', 'Soup', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Thai Tom Yum Soup', 'Spicy and sour soup with shrimp', 
         '1. Make stock with lemongrass\n2. Add galangal and kaffir lime\n3. Add mushrooms and shrimp\n4. Season with fish sauce and lime\n5. Add chilies for heat\n6. Garnish with cilantro', 
         25, 4, 'Easy', 'Soup', NULL, 1, GETUTCDATE(), GETUTCDATE()),
        ('Russian Borscht', 'Beet soup with sour cream', 
         '1. Cook beets until tender\n2. Add vegetables and stock\n3. Simmer for 1 hour\n4. Puree until smooth\n5. Add vinegar and sugar\n6. Serve with sour cream', 
         120, 6, 'Medium', 'Soup', NULL, 1, GETUTCDATE(), GETUTCDATE())
END
GO

-- Insert Rich Recipe Ingredients Data (connecting all recipes with ingredients)
IF NOT EXISTS (SELECT 1 FROM RecipeIngredients WHERE RecipeId = 1)
BEGIN
    INSERT INTO RecipeIngredients (RecipeId, IngredientId, Quantity, Unit, Notes)
    VALUES 
        -- Classic Chicken Fried Rice (Recipe 1)
        (1, 1, 500, 'g', 'Cut into small pieces'),
        (1, 2, 300, 'g', 'Cooked jasmine rice'),
        (1, 3, 100, 'g', 'Finely chopped'),
        (1, 4, 20, 'g', 'Minced'),
        (1, 5, 200, 'g', 'Diced'),
        (1, 6, 30, 'ml', 'Sesame oil for cooking'),
        (1, 7, 5, 'g', 'To taste'),
        (1, 8, 2, 'g', 'Freshly ground'),
        
        -- Thai Green Curry (Recipe 2)
        (2, 1, 400, 'g', 'Cut into bite-sized pieces'),
        (2, 2, 200, 'g', 'Jasmine rice'),
        (2, 3, 150, 'g', 'Sliced'),
        (2, 4, 30, 'g', 'Minced'),
        (2, 5, 100, 'g', 'Sliced bell peppers'),
        (2, 6, 200, 'g', 'Sliced carrots'),
        (2, 7, 100, 'g', 'Broccoli florets'),
        (2, 8, 40, 'ml', 'Coconut oil'),
        (2, 9, 8, 'g', 'To taste'),
        (2, 10, 3, 'g', 'Freshly ground'),
        
        -- Japanese Ramen (Recipe 3)
        (3, 1, 150, 'g', 'Ramen noodles'),
        (3, 2, 100, 'g', 'Sliced'),
        (3, 3, 20, 'g', 'Minced'),
        (3, 4, 200, 'g', 'Diced'),
        (3, 5, 100, 'g', 'Bean sprouts'),
        (3, 6, 50, 'g', 'Sliced mushrooms'),
        (3, 7, 30, 'ml', 'Sesame oil'),
        (3, 8, 5, 'g', 'To taste'),
        
        -- Korean Bibimbap (Recipe 4)
        (4, 1, 200, 'g', 'Short-grain rice'),
        (4, 2, 300, 'g', 'Bulgogi beef'),
        (4, 3, 100, 'g', 'Spinach'),
        (4, 4, 150, 'g', 'Julienned carrots'),
        (4, 5, 100, 'g', 'Bean sprouts'),
        (4, 6, 200, 'g', 'Diced'),
        (4, 7, 40, 'ml', 'Sesame oil'),
        (4, 8, 5, 'g', 'To taste'),
        
        -- Chinese Kung Pao Chicken (Recipe 5)
        (5, 1, 400, 'g', 'Cut into cubes'),
        (5, 2, 100, 'g', 'Diced'),
        (5, 3, 20, 'g', 'Minced'),
        (5, 4, 200, 'g', 'Diced'),
        (5, 5, 100, 'g', 'Bell peppers'),
        (5, 6, 50, 'g', 'Roasted peanuts'),
        (5, 7, 30, 'ml', 'Sesame oil'),
        (5, 8, 5, 'g', 'To taste'),
        (5, 9, 3, 'g', 'Freshly ground'),
        
        -- Vietnamese Pho (Recipe 6)
        (6, 1, 500, 'g', 'Beef bones for broth'),
        (6, 2, 200, 'g', 'Rice noodles'),
        (6, 3, 150, 'g', 'Sliced'),
        (6, 4, 30, 'g', 'Minced'),
        (6, 5, 100, 'g', 'Bean sprouts'),
        (6, 6, 200, 'g', 'Sliced'),
        (6, 7, 8, 'g', 'To taste'),
        (6, 8, 2, 'g', 'Freshly ground'),
        
        -- Indian Butter Chicken (Recipe 7)
        (7, 1, 500, 'g', 'Cut into pieces'),
        (7, 2, 200, 'g', 'Basmati rice'),
        (7, 3, 400, 'g', 'Crushed tomatoes'),
        (7, 4, 100, 'g', 'Sliced'),
        (7, 5, 30, 'g', 'Minced'),
        (7, 6, 40, 'ml', 'Ghee'),
        (7, 7, 8, 'g', 'To taste'),
        (7, 8, 3, 'g', 'Freshly ground'),
        
        -- Japanese Sushi Rolls (Recipe 8)
        (8, 2, 300, 'g', 'Sushi rice'),
        (8, 3, 200, 'g', 'Fresh fish'),
        (8, 4, 150, 'g', 'Sliced avocado'),
        (8, 5, 100, 'g', 'Cucumber strips'),
        (8, 6, 20, 'ml', 'Sesame oil'),
        (8, 7, 5, 'g', 'To taste'),
        
        -- Italian Spaghetti Carbonara (Recipe 9)
        (9, 2, 400, 'g', 'Spaghetti pasta'),
        (9, 1, 200, 'g', 'Pancetta'),
        (9, 3, 4, 'pieces', 'Large eggs'),
        (9, 4, 100, 'ml', 'Parmesan cheese'),
        (9, 6, 30, 'ml', 'Olive oil'),
        (9, 7, 5, 'g', 'To taste'),
        (9, 8, 2, 'g', 'Freshly ground'),
        
        -- French Coq au Vin (Recipe 10)
        (10, 1, 800, 'g', 'Chicken pieces'),
        (10, 2, 200, 'g', 'Pearl onions'),
        (10, 3, 30, 'g', 'Minced'),
        (10, 4, 300, 'g', 'Diced'),
        (10, 5, 200, 'g', 'Mushrooms'),
        (10, 6, 50, 'ml', 'Butter'),
        (10, 7, 8, 'g', 'To taste'),
        (10, 8, 3, 'g', 'Freshly ground'),
        
        -- Spanish Paella (Recipe 11)
        (11, 2, 400, 'g', 'Arborio rice'),
        (11, 9, 300, 'g', 'Mixed seafood'),
        (11, 1, 200, 'g', 'Chicken pieces'),
        (11, 3, 150, 'g', 'Sliced'),
        (11, 4, 20, 'g', 'Minced'),
        (11, 5, 200, 'g', 'Diced'),
        (11, 6, 40, 'ml', 'Olive oil'),
        (11, 7, 8, 'g', 'To taste'),
        (11, 8, 2, 'g', 'Freshly ground'),
        
        -- German Sauerbraten (Recipe 12)
        (12, 3, 1000, 'g', 'Beef roast'),
        (12, 3, 200, 'g', 'Sliced'),
        (12, 4, 30, 'g', 'Minced'),
        (12, 5, 300, 'g', 'Diced'),
        (12, 6, 200, 'g', 'Mushrooms'),
        (12, 6, 50, 'ml', 'Oil'),
        (12, 7, 10, 'g', 'To taste'),
        (12, 8, 3, 'g', 'Freshly ground'),
        
        -- Greek Moussaka (Recipe 13)
        (13, 7, 600, 'g', 'Sliced eggplant'),
        (13, 3, 400, 'g', 'Ground lamb'),
        (13, 4, 300, 'g', 'Crushed tomatoes'),
        (13, 3, 150, 'g', 'Diced'),
        (13, 4, 20, 'g', 'Minced'),
        (13, 5, 200, 'g', 'Feta cheese'),
        (13, 6, 40, 'ml', 'Olive oil'),
        (13, 7, 8, 'g', 'To taste'),
        (13, 8, 2, 'g', 'Freshly ground'),
        
        -- British Fish and Chips (Recipe 14)
        (14, 9, 600, 'g', 'White fish fillets'),
        (14, 6, 800, 'g', 'Potatoes'),
        (14, 2, 200, 'g', 'Flour for batter'),
        (14, 3, 2, 'pieces', 'Eggs'),
        (14, 6, 500, 'ml', 'Oil for frying'),
        (14, 7, 8, 'g', 'To taste'),
        (14, 8, 3, 'g', 'Freshly ground'),
        
        -- Classic American Burger (Recipe 15)
        (15, 3, 600, 'g', 'Ground beef'),
        (15, 2, 4, 'pieces', 'Burger buns'),
        (15, 4, 200, 'g', 'Lettuce leaves'),
        (15, 4, 150, 'g', 'Sliced tomatoes'),
        (15, 3, 100, 'g', 'Sliced onions'),
        (15, 5, 100, 'g', 'Cheese slices'),
        (15, 6, 30, 'ml', 'Oil for cooking'),
        (15, 7, 8, 'g', 'To taste'),
        (15, 8, 3, 'g', 'Freshly ground'),  
        
        -- Southern BBQ Ribs (Recipe 16)
        (16, 4, 1000, 'g', 'Pork ribs'),
        (16, 6, 50, 'ml', 'Oil for cooking'),
        (16, 7, 10, 'g', 'To taste'),
        (16, 8, 5, 'g', 'Freshly ground'),
        
        -- New York Cheesecake (Recipe 17)
        (17, 2, 200, 'g', 'Graham crackers'),
        (17, 3, 600, 'g', 'Cream cheese'),
        (17, 4, 4, 'pieces', 'Large eggs'),
        (17, 6, 100, 'g', 'Butter'),
        (17, 7, 5, 'g', 'To taste'),        
        
        -- Tex-Mex Tacos (Recipe 18)
        (18, 3, 400, 'g', 'Ground beef'),
        (18, 13, 8, 'pieces', 'Taco shells'),
        (18, 22, 200, 'g', 'Shredded lettuce'),
        (18, 22, 150, 'g', 'Diced tomatoes'),
        (18, 20, 100, 'g', 'Diced onions'),
        (18, 10, 100, 'g', 'Shredded cheese'),
        (18, 11, 30, 'ml', 'Oil for cooking'),
        (18, 12, 5, 'g', 'To taste'),
        (18, 13, 2, 'g', 'Freshly ground'),
        
        -- Lebanese Hummus (Recipe 19)
        (19, 14, 400, 'g', 'Cooked chickpeas'),
        (19, 15, 60, 'ml', 'Tahini'),
        (19, 16, 20, 'g', 'Minced garlic'),
        (19, 17, 30, 'ml', 'Lemon juice'),
        (19, 18, 40, 'ml', 'Olive oil'),
        (19, 19, 5, 'g', 'To taste'),
        (19, 20, 2, 'g', 'Ground cumin'),
        
        -- Persian Rice with Saffron (Recipe 20)
        (20, 12, 400, 'g', 'Basmati rice'),
        (20, 21, 60, 'ml', 'Butter'),
        (20, 22, 0.5, 'g', 'Saffron threads'),
        (20, 36, 5, 'g', 'To taste'),
        
        -- Turkish Kebabs (Recipe 21)
        (21, 3, 600, 'g', 'Lamb meat'),
        (21, 20, 200, 'g', 'Sliced onions'),
        (21, 22, 300, 'g', 'Bell peppers'),
        (21, 25, 200, 'g', 'Cherry tomatoes'),
        (21, 20, 40, 'ml', 'Olive oil'),
        (21, 36, 8, 'g', 'To taste'),
        (21, 37, 3, 'g', 'Freshly ground'),
        
        -- Moroccan Tagine (Recipe 22)
        (22, 1, 600, 'g', 'Chicken pieces'),
        (22, 20, 200, 'g', 'Sliced onions'),
        (22, 21, 30, 'g', 'Minced garlic'),
        (22, 22, 300, 'g', 'Diced tomatoes'),
        (22, 25, 200, 'g', 'Mushrooms'),
        (22, 20, 40, 'ml', 'Olive oil'),
        (22, 26, 8, 'g', 'To taste'),
        (22, 27, 3, 'g', 'Freshly ground'),
        
        -- Ethiopian Doro Wat (Recipe 23)
        (23, 1, 800, 'g', 'Chicken pieces'),
        (23, 20, 300, 'g', 'Sliced onions'),
        (23, 21, 40, 'g', 'Minced garlic'),
        (23, 22, 400, 'g', 'Crushed tomatoes'),
        (23, 20, 50, 'ml', 'Oil'),
        (23, 36, 10, 'g', 'To taste'),
        (23, 27, 5, 'g', 'Freshly ground'),
        
        -- South African Bobotie (Recipe 24)
        (24, 3, 500, 'g', 'Ground lamb'),
        (24, 20, 200, 'g', 'Diced onions'),
        (24, 21, 30, 'g', 'Minced garlic'),
        (24, 22, 200, 'g', 'Diced tomatoes'),
        (24, 2, 3, 'pieces', 'Large eggs'),
        (24, 30, 40, 'ml', 'Oil'),
        (24, 26, 8, 'g', 'To taste'),
        (24, 37, 3, 'g', 'Freshly ground'),
        
        -- Mexican Mole Poblano (Recipe 25)
        (25, 1, 800, 'g', 'Chicken pieces'),
        (25, 20, 200, 'g', 'Sliced onions'),
        (25, 21, 30, 'g', 'Minced garlic'),
        (25, 22, 400, 'g', 'Crushed tomatoes'),
        (25, 30, 50, 'ml', 'Oil'),
        (25, 36, 10, 'g', 'To taste'),
        (25, 37, 5, 'g', 'Freshly ground'),
        
        -- Peruvian Ceviche (Recipe 26)
        (26, 9, 600, 'g', 'Fresh white fish'),
        (26, 20, 150, 'g', 'Sliced red onions'),
        (26, 21, 20, 'g', 'Minced garlic'),
        (26, 17, 60, 'ml', 'Lime juice'),
        (26, 25, 100, 'g', 'Cilantro leaves'),
        (26, 86, 8, 'g', 'To taste'),
        (26, 37, 2, 'g', 'Freshly ground'),
        
        -- Brazilian Feijoada (Recipe 27)
        (27, 18, 500, 'g', 'Black beans'),
        (27, 3, 400, 'g', 'Various meats'),
        (27, 20, 200, 'g', 'Sliced onions'),
        (27, 21, 30, 'g', 'Minced garlic'),
        (27, 22, 300, 'g', 'Diced tomatoes'),
        (27, 6, 50, 'ml', 'Oil'),
        (27, 36, 10, 'g', 'To taste'),
        (27, 37, 5, 'g', 'Freshly ground'),
        
        -- Argentine Asado (Recipe 28)
        (28, 3, 1000, 'g', 'Beef cuts'),
        (28, 20, 200, 'g', 'Sliced onions'),
        (28, 21, 30, 'g', 'Minced garlic'),
        (28, 25, 200, 'g', 'Mixed herbs'),
        (28, 6, 60, 'ml', 'Oil'),
        (28, 36, 10, 'g', 'To taste'),
        (28, 37, 5, 'g', 'Freshly ground'),
        
        -- Mediterranean Buddha Bowl (Recipe 29)
        (29, 19, 200, 'g', 'Cooked quinoa'),
        (29, 25, 300, 'g', 'Mixed vegetables'),
        (29, 14, 200, 'g', 'Chickpeas'),
        (29, 37, 150, 'g', 'Sliced avocado'),
        (29, 6, 40, 'ml', 'Tahini dressing'),
        (29, 36, 5, 'g', 'To taste'),
        (29, 37, 2, 'g', 'Freshly ground'),
        
        -- Vegan Thai Curry (Recipe 30)
        (30, 7, 400, 'g', 'Cubed tofu'),
        (30, 12, 200, 'g', 'Jasmine rice'),
        (30, 25, 300, 'g', 'Mixed vegetables'),
        (30, 6, 40, 'ml', 'Coconut oil'),
        (30, 36, 8, 'g', 'To taste'),
        (30, 27, 3, 'g', 'Freshly ground'),
        
        -- Mediterranean Quinoa Salad (Recipe 31)
        (31, 19, 300, 'g', 'Cooked quinoa'),
        (31, 25, 400, 'g', 'Mixed vegetables'),
        (31, 75, 200, 'g', 'Chickpeas'),
        (31, 33, 150, 'g', 'Feta cheese'),
        (31, 6, 50, 'ml', 'Lemon vinaigrette'),
        (31, 26, 5, 'g', 'To taste'),
        (31, 27, 2, 'g', 'Freshly ground'),
        
        -- French Macarons (Recipe 32)
        (32, 2, 200, 'g', 'Almond flour'),
        (32, 13, 200, 'g', 'Powdered sugar'),
        (32, 2, 4, 'pieces', 'Egg whites'),
        (32, 26, 2, 'g', 'To taste'),
        
        -- Italian Tiramisu (Recipe 33)
        (33, 13, 200, 'g', 'Ladyfingers'),
        (33, 13, 500, 'g', 'Mascarpone cheese'),
        (33, 12, 4, 'pieces', 'Large eggs'),
        (33, 20, 100, 'g', 'Butter'),
        (33, 26, 5, 'g', 'To taste'),
        
        -- Chocolate Lava Cake (Recipe 34)
        (34, 13, 100, 'g', 'All-purpose flour'),
        (34, 20, 200, 'g', 'Dark chocolate'),
        (34, 30, 100, 'g', 'Butter'),
        (34, 32, 3, 'pieces', 'Large eggs'),
        (34, 36, 5, 'g', 'To taste'),
        
        -- Japanese Matcha Ice Cream (Recipe 35)
        (35, 13, 500, 'ml', 'Heavy cream'),
        (35, 33, 300, 'ml', 'Whole milk'),
        (35, 12, 4, 'pieces', 'Egg yolks'),
        (35, 26, 5, 'g', 'To taste'),
        
        -- French Toast (Recipe 36)
        (36, 13, 8, 'slices', 'Bread slices'),
        (36, 12, 4, 'pieces', 'Large eggs'),
        (36, 33, 200, 'ml', 'Milk'),
        (36, 30, 60, 'g', 'Butter'),
        (36, 36, 5, 'g', 'To taste'),
        (36, 32, 5, 'g', 'Cinnamon'),
        
        -- Pancakes (Recipe 37)
        (37, 13, 200, 'g', 'All-purpose flour'),
        (37, 12, 2, 'pieces', 'Large eggs'),
        (37, 13, 300, 'ml', 'Milk'),
        (37, 20, 40, 'g', 'Butter'),
        (37, 26, 5, 'g', 'To taste'),
        
        -- Eggs Benedict (Recipe 38)
        (38, 12, 4, 'pieces', 'Large eggs'),
        (38, 2, 2, 'pieces', 'English muffins'),
        (38, 1, 200, 'g', 'Ham slices'),
        (38, 20, 100, 'g', 'Butter'),
        (38, 26, 5, 'g', 'To taste'),
        
        -- Shakshuka (Recipe 39)
        (39, 12, 4, 'pieces', 'Large eggs'),
        (39, 20, 200, 'g', 'Sliced onions'),
        (39, 22, 400, 'g', 'Crushed tomatoes'),
        (39, 23, 150, 'g', 'Bell peppers'),
        (39, 6, 40, 'ml', 'Olive oil'),
        (39, 26, 8, 'g', 'To taste'),
        (39, 27, 3, 'g', 'Freshly ground'),
        
        -- French Onion Soup (Recipe 40)
        (40, 20, 800, 'g', 'Sliced onions'),
        (40, 13, 4, 'slices', 'Bread slices'),
        (40, 13, 200, 'g', 'Gruyere cheese'),
        (40, 6, 60, 'ml', 'Butter'),
        (40, 36, 8, 'g', 'To taste'),
        (40, 37, 2, 'g', 'Freshly ground'),
        
        -- Italian Minestrone (Recipe 41)
        (41, 25, 500, 'g', 'Mixed vegetables'),
        (41, 13, 100, 'g', 'Pasta'),
        (41, 14, 200, 'g', 'Beans'),
        (41, 22, 300, 'g', 'Crushed tomatoes'),
        (41, 20, 150, 'g', 'Diced onions'),
        (41, 21, 20, 'g', 'Minced garlic'),
        (41, 6, 40, 'ml', 'Olive oil'),
        (41, 36, 8, 'g', 'To taste'),
        (41, 37, 3, 'g', 'Freshly ground'),
        
        -- Thai Tom Yum Soup (Recipe 42)
        (42, 9, 300, 'g', 'Shrimp'),
        (42, 25, 200, 'g', 'Mushrooms'),
        (42, 20, 100, 'g', 'Sliced onions'),
        (42, 21, 20, 'g', 'Minced garlic'),
        (42, 17, 30, 'ml', 'Lime juice'),
        (42, 36, 8, 'g', 'To taste'),
        (42, 27, 3, 'g', 'Freshly ground'),
        
        -- Russian Borscht (Recipe 43)
        (43, 25, 600, 'g', 'Beets'),
        (43, 20, 200, 'g', 'Sliced onions'),
        (43, 21, 20, 'g', 'Minced garlic'),
        (43, 22, 300, 'g', 'Crushed tomatoes'),
        (43, 6, 40, 'ml', 'Oil'),
        (43, 36, 8, 'g', 'To taste'),
        (43, 27, 3, 'g', 'Freshly ground')
END
GO

-- Insert Rich Ratings Data (diverse ratings from different users)
IF NOT EXISTS (SELECT 1 FROM Ratings WHERE RecipeId = 1)
BEGIN
    INSERT INTO Ratings (RecipeId, UserId, Rating, Comment, CreatedAt)
    VALUES 
        -- Classic Chicken Fried Rice ratings
        (1, 2, 5, 'Excellent recipe! Very easy to follow and delicious.', GETUTCDATE()),
        (1, 3, 4, 'Great taste, will make again. Perfect for leftovers.', GETUTCDATE()),
        (1, 4, 5, 'Authentic flavor, my family loved it!', GETUTCDATE()),
        (1, 5, 4, 'Quick and easy weeknight meal.', GETUTCDATE()),
        (1, 6, 5, 'Best fried rice recipe I have tried!', GETUTCDATE()),
        
        -- Thai Green Curry ratings
        (2, 2, 5, 'Amazing curry! Spicy and flavorful.', GETUTCDATE()),
        (2, 3, 4, 'Great recipe, authentic Thai taste.', GETUTCDATE()),
        (2, 7, 5, 'Perfect balance of flavors.', GETUTCDATE()),
        (2, 8, 4, 'Delicious and easy to make.', GETUTCDATE()),
        (2, 9, 5, 'Restaurant quality at home!', GETUTCDATE()),
        
        -- Japanese Ramen ratings
        (3, 2, 5, 'Incredible depth of flavor in the broth.', GETUTCDATE()),
        (3, 4, 4, 'Complex but worth the effort.', GETUTCDATE()),
        (3, 10, 5, 'Authentic Japanese ramen experience.', GETUTCDATE()),
        (3, 11, 4, 'Rich and satisfying.', GETUTCDATE()),
        (3, 12, 5, 'Best ramen I have made at home.', GETUTCDATE()),
        
        -- Korean Bibimbap ratings
        (4, 3, 5, 'Beautiful presentation and great taste.', GETUTCDATE()),
        (4, 5, 4, 'Healthy and delicious.', GETUTCDATE()),
        (4, 13, 5, 'Authentic Korean flavors.', GETUTCDATE()),
        (4, 14, 4, 'Great way to use leftover vegetables.', GETUTCDATE()),
        (4, 15, 5, 'Perfect balance of textures.', GETUTCDATE()),
        
        -- Chinese Kung Pao Chicken ratings
        (5, 2, 4, 'Spicy and delicious.', GETUTCDATE()),
        (5, 6, 5, 'Authentic Sichuan flavors.', GETUTCDATE()),
        (5, 16, 4, 'Great heat level, perfect with rice.', GETUTCDATE()),
        (5, 17, 5, 'Restaurant quality dish.', GETUTCDATE()),
        (5, 18, 4, 'Love the peanuts in this dish.', GETUTCDATE()),
        
        -- Vietnamese Pho ratings
        (6, 3, 5, 'Incredible broth, worth the long cooking time.', GETUTCDATE()),
        (6, 7, 4, 'Authentic Vietnamese flavors.', GETUTCDATE()),
        (6, 19, 5, 'Best pho recipe I have found.', GETUTCDATE()),
        (6, 20, 4, 'Complex but delicious.', GETUTCDATE()),
        (6, 1, 5, 'Perfect comfort food.', GETUTCDATE()),
        
        -- Indian Butter Chicken ratings
        (7, 4, 5, 'Creamy and delicious curry.', GETUTCDATE()),
        (7, 8, 4, 'Great Indian flavors.', GETUTCDATE()),
        (7, 22, 5, 'Restaurant quality butter chicken.', GETUTCDATE()),
        (7, 23, 4, 'Perfect with naan bread.', GETUTCDATE()),
        (7, 24, 5, 'Amazing depth of flavor.', GETUTCDATE()),
        
        -- Japanese Sushi Rolls ratings
        (8, 5, 5, 'Perfect sushi rice and technique.', GETUTCDATE()),
        (8, 9, 4, 'Great for special occasions.', GETUTCDATE()),
        (8, 25, 5, 'Authentic Japanese sushi.', GETUTCDATE()),
        (8, 26, 4, 'Beautiful presentation.', GETUTCDATE()),
        (8, 27, 5, 'Best homemade sushi ever.', GETUTCDATE()),
        
        -- Italian Spaghetti Carbonara ratings
        (9, 6, 5, 'Creamy and authentic Italian dish.', GETUTCDATE()),
        (9, 10, 4, 'Simple but delicious.', GETUTCDATE()),
        (9, 28, 5, 'Perfect carbonara technique.', GETUTCDATE()),
        (9, 29, 4, 'Great comfort food.', GETUTCDATE()),
        (9, 30, 5, 'Restaurant quality pasta.', GETUTCDATE()),
        
        -- French Coq au Vin ratings
        (10, 7, 5, 'Elegant and sophisticated dish.', GETUTCDATE()),
        (10, 11, 4, 'Worth the long cooking time.', GETUTCDATE()),
        (10, 31, 5, 'Perfect for special dinners.', GETUTCDATE()),
        (10, 32, 4, 'Rich and complex flavors.', GETUTCDATE()),
        (10, 33, 5, 'Authentic French cuisine.', GETUTCDATE()),
        
        -- Spanish Paella ratings
        (11, 8, 5, 'Beautiful presentation and great taste.', GETUTCDATE()),
        (11, 12, 4, 'Perfect for entertaining.', GETUTCDATE()),
        (11, 34, 5, 'Authentic Spanish paella.', GETUTCDATE()),
        (11, 35, 4, 'Great seafood flavors.', GETUTCDATE()),
        (11, 36, 5, 'Restaurant quality dish.', GETUTCDATE()),
        
        -- German Sauerbraten ratings
        (12, 9, 5, 'Traditional German flavors.', GETUTCDATE()),
        (12, 13, 4, 'Worth the marinating time.', GETUTCDATE()),
        (12, 37, 5, 'Authentic German cuisine.', GETUTCDATE()),
        (12, 38, 4, 'Rich and hearty dish.', GETUTCDATE()),
        (12, 39, 5, 'Perfect for cold weather.', GETUTCDATE()),
        
        -- Greek Moussaka ratings
        (13, 10, 5, 'Layered perfection with great flavors.', GETUTCDATE()),
        (13, 14, 4, 'Authentic Greek dish.', GETUTCDATE()),
        (13, 40, 5, 'Restaurant quality moussaka.', GETUTCDATE()),
        (13, 41, 4, 'Great vegetarian option.', GETUTCDATE()),
        (13, 42, 5, 'Perfect comfort food.', GETUTCDATE()),
        
        -- British Fish and Chips ratings
        (14, 11, 4, 'Crispy and delicious.', GETUTCDATE()),
        (14, 15, 5, 'Perfect British comfort food.', GETUTCDATE()),
        (14, 43, 4, 'Great batter technique.', GETUTCDATE()),
        (14, 44, 5, 'Authentic fish and chips.', GETUTCDATE()),
        (14, 45, 4, 'Perfect with malt vinegar.', GETUTCDATE()),
        
        -- Classic American Burger ratings
        (15, 12, 5, 'Juicy and flavorful burger.', GETUTCDATE()),
        (15, 16, 4, 'Perfect for backyard BBQ.', GETUTCDATE()),
        (15, 46, 5, 'Best burger recipe ever.', GETUTCDATE()),
        (15, 47, 4, 'Great for family dinners.', GETUTCDATE()),
        (15, 48, 5, 'Restaurant quality burger.', GETUTCDATE()),
        
        -- Southern BBQ Ribs ratings
        (16, 13, 5, 'Fall-off-the-bone tender ribs.', GETUTCDATE()),
        (16, 17, 4, 'Great BBQ flavors.', GETUTCDATE()),
        (16, 49, 5, 'Perfect for summer cookouts.', GETUTCDATE()),
        (16, 50, 4, 'Worth the long cooking time.', GETUTCDATE()),
        (16, 51, 5, 'Authentic Southern BBQ.', GETUTCDATE()),
        
        -- New York Cheesecake ratings
        (17, 14, 5, 'Creamy and perfect texture.', GETUTCDATE()),
        (17, 18, 4, 'Great dessert for special occasions.', GETUTCDATE()),
        (17, 52, 5, 'Restaurant quality cheesecake.', GETUTCDATE()),
        (17, 53, 4, 'Perfect with berry sauce.', GETUTCDATE()),
        (17, 54, 5, 'Best cheesecake recipe.', GETUTCDATE()),
        
        -- Tex-Mex Tacos ratings
        (18, 15, 4, 'Spicy and flavorful tacos.', GETUTCDATE()),
        (18, 19, 5, 'Perfect for taco Tuesday.', GETUTCDATE()),
        (18, 55, 4, 'Great with fresh toppings.', GETUTCDATE()),
        (18, 56, 5, 'Authentic Tex-Mex flavors.', GETUTCDATE()),
        (18, 57, 4, 'Family favorite meal.', GETUTCDATE()),
        
        -- Lebanese Hummus ratings
        (19, 16, 5, 'Creamy and authentic hummus.', GETUTCDATE()),
        (19, 20, 4, 'Perfect with pita bread.', GETUTCDATE()),
        (19, 58, 5, 'Best hummus recipe.', GETUTCDATE()),
        (19, 59, 4, 'Great appetizer.', GETUTCDATE()),
        (19, 60, 5, 'Restaurant quality hummus.', GETUTCDATE()),
        
        -- Persian Rice with Saffron ratings
        (20, 17, 5, 'Fragrant and beautiful rice.', GETUTCDATE()),
        (20, 21, 4, 'Perfect with Persian dishes.', GETUTCDATE()),
        (20, 61, 5, 'Authentic Persian cuisine.', GETUTCDATE()),
        (20, 62, 4, 'Great technique for crispy bottom.', GETUTCDATE()),
        (20, 63, 5, 'Elegant and delicious.', GETUTCDATE()),
        
        -- Turkish Kebabs ratings
        (21, 18, 4, 'Grilled to perfection.', GETUTCDATE()),
        (21, 22, 5, 'Authentic Turkish flavors.', GETUTCDATE()),
        (21, 64, 4, 'Great for summer grilling.', GETUTCDATE()),
        (21, 65, 5, 'Perfect with rice pilaf.', GETUTCDATE()),
        (21, 66, 4, 'Delicious marinade.', GETUTCDATE()),
        
        -- Moroccan Tagine ratings
        (22, 19, 5, 'Aromatic and flavorful stew.', GETUTCDATE()),
        (22, 23, 4, 'Perfect for cold weather.', GETUTCDATE()),
        (22, 67, 5, 'Authentic Moroccan cuisine.', GETUTCDATE()),
        (22, 68, 4, 'Great with couscous.', GETUTCDATE()),
        (22, 69, 5, 'Complex and delicious flavors.', GETUTCDATE()),
        
        -- Ethiopian Doro Wat ratings
        (23, 20, 5, 'Spicy and authentic Ethiopian dish.', GETUTCDATE()),
        (23, 24, 4, 'Great with injera bread.', GETUTCDATE()),
        (23, 70, 5, 'Perfect spice blend.', GETUTCDATE()),
        (23, 71, 4, 'Worth the long cooking time.', GETUTCDATE()),
        (23, 72, 5, 'Restaurant quality Ethiopian food.', GETUTCDATE()),
        
        -- South African Bobotie ratings
        (24, 21, 4, 'Unique and flavorful casserole.', GETUTCDATE()),
        (24, 25, 5, 'Authentic South African dish.', GETUTCDATE()),
        (24, 73, 4, 'Great with yellow rice.', GETUTCDATE()),
        (24, 74, 5, 'Perfect comfort food.', GETUTCDATE()),
        (24, 75, 4, 'Interesting flavor combination.', GETUTCDATE()),
        
        -- Mexican Mole Poblano ratings
        (25, 22, 5, 'Complex and authentic mole sauce.', GETUTCDATE()),
        (25, 26, 4, 'Worth the long preparation time.', GETUTCDATE()),
        (25, 76, 5, 'Restaurant quality mole.', GETUTCDATE()),
        (25, 77, 4, 'Perfect with chicken and rice.', GETUTCDATE()),
        (25, 78, 5, 'Authentic Mexican cuisine.', GETUTCDATE()),
        
        -- Peruvian Ceviche ratings
        (26, 23, 5, 'Fresh and tangy ceviche.', GETUTCDATE()),
        (26, 27, 4, 'Perfect for summer.', GETUTCDATE()),
        (26, 79, 5, 'Authentic Peruvian flavors.', GETUTCDATE()),
        (26, 80, 4, 'Great with sweet potato.', GETUTCDATE()),
        (26, 81, 5, 'Best ceviche recipe.', GETUTCDATE()),
        
        -- Brazilian Feijoada ratings
        (27, 24, 5, 'Hearty and flavorful stew.', GETUTCDATE()),
        (27, 28, 4, 'Perfect for special occasions.', GETUTCDATE()),
        (27, 82, 5, 'Authentic Brazilian cuisine.', GETUTCDATE()),
        (27, 83, 4, 'Great with rice and farofa.', GETUTCDATE()),
        (27, 84, 5, 'Restaurant quality feijoada.', GETUTCDATE()),
        
        -- Argentine Asado ratings
        (28, 25, 5, 'Perfect grilled beef.', GETUTCDATE()),
        (28, 29, 4, 'Great for outdoor cooking.', GETUTCDATE()),
        (28, 85, 5, 'Authentic Argentine flavors.', GETUTCDATE()),
        (28, 86, 4, 'Perfect with chimichurri.', GETUTCDATE()),
        (28, 87, 5, 'Best grilled meat recipe.', GETUTCDATE()),
        
        -- Mediterranean Buddha Bowl ratings
        (29, 26, 4, 'Healthy and delicious bowl.', GETUTCDATE()),
        (29, 30, 5, 'Perfect for meal prep.', GETUTCDATE()),
        (29, 88, 4, 'Great vegetarian option.', GETUTCDATE()),
        (29, 89, 5, 'Beautiful presentation.', GETUTCDATE()),
        (29, 90, 4, 'Nutritious and satisfying.', GETUTCDATE()),
        
        -- Vegan Thai Curry ratings
        (30, 27, 5, 'Creamy and flavorful vegan curry.', GETUTCDATE()),
        (30, 31, 4, 'Great plant-based option.', GETUTCDATE()),
        (30, 91, 5, 'Perfect vegan Thai flavors.', GETUTCDATE()),
        (30, 92, 4, 'Great with jasmine rice.', GETUTCDATE()),
        (30, 93, 5, 'Restaurant quality vegan dish.', GETUTCDATE()),
        
        -- Mediterranean Quinoa Salad ratings
        (31, 28, 4, 'Fresh and healthy salad.', GETUTCDATE()),
        (31, 32, 5, 'Perfect for summer lunches.', GETUTCDATE()),
        (31, 94, 4, 'Great protein content.', GETUTCDATE()),
        (31, 95, 5, 'Beautiful and delicious.', GETUTCDATE()),
        (31, 96, 4, 'Perfect meal prep option.', GETUTCDATE()),
        
        -- French Macarons ratings
        (32, 29, 5, 'Delicate and perfect macarons.', GETUTCDATE()),
        (32, 33, 4, 'Great for special occasions.', GETUTCDATE()),
        (32, 97, 5, 'Restaurant quality macarons.', GETUTCDATE()),
        (32, 98, 4, 'Beautiful presentation.', GETUTCDATE()),
        (32, 99, 5, 'Best macaron recipe.', GETUTCDATE()),
        
        -- Italian Tiramisu ratings
        (33, 30, 5, 'Creamy and authentic tiramisu.', GETUTCDATE()),
        (33, 34, 4, 'Perfect dessert for dinner parties.', GETUTCDATE()),
        (33, 100, 5, 'Restaurant quality tiramisu.', GETUTCDATE()),
        (33, 100, 4, 'Great coffee flavor.', GETUTCDATE()),
        (33, 82, 5, 'Best tiramisu recipe.', GETUTCDATE()),
        
        -- Chocolate Lava Cake ratings
        (34, 31, 5, 'Perfect molten center.', GETUTCDATE()),
        (34, 35, 4, 'Great for date night.', GETUTCDATE()),
        (34, 83, 5, 'Restaurant quality dessert.', GETUTCDATE()),
        (34, 74, 4, 'Perfect with ice cream.', GETUTCDATE()),
        (34, 17, 5, 'Best chocolate dessert.', GETUTCDATE()),
        
        -- Japanese Matcha Ice Cream ratings
        (35, 32, 4, 'Smooth and creamy matcha flavor.', GETUTCDATE()),
        (35, 36, 5, 'Perfect Japanese dessert.', GETUTCDATE()),
        (35, 56, 4, 'Great matcha taste.', GETUTCDATE()),
        (35, 77, 5, 'Authentic Japanese flavors.', GETUTCDATE()),
        (35, 98, 4, 'Perfect for hot weather.', GETUTCDATE()),
        
        -- French Toast ratings
        (36, 33, 5, 'Perfect breakfast treat.', GETUTCDATE()),
        (36, 37, 4, 'Great for weekend brunch.', GETUTCDATE()),
        (36, 99, 5, 'Best French toast recipe.', GETUTCDATE()),
        (36, 90, 4, 'Perfect with maple syrup.', GETUTCDATE()),
        (36, 61, 5, 'Restaurant quality breakfast.', GETUTCDATE()),
        
        -- Pancakes ratings
        (37, 34, 4, 'Fluffy and delicious pancakes.', GETUTCDATE()),
        (37, 38, 5, 'Perfect for family breakfast.', GETUTCDATE()),
        (37, 12, 4, 'Great with butter and syrup.', GETUTCDATE()),
        (37, 73, 5, 'Best pancake recipe.', GETUTCDATE()),
        (37, 74, 4, 'Perfect for weekend mornings.', GETUTCDATE()),
        
        -- Eggs Benedict ratings
        (38, 35, 5, 'Perfect hollandaise sauce.', GETUTCDATE()),
        (38, 39, 4, 'Great for special brunch.', GETUTCDATE()),
        (38, 15, 5, 'Restaurant quality eggs benedict.', GETUTCDATE()),
        (38, 16, 4, 'Perfect poached eggs.', GETUTCDATE()),
        (38, 17, 5, 'Best brunch recipe.', GETUTCDATE()),
        
        -- Shakshuka ratings
        (39, 36, 4, 'Spicy and flavorful eggs.', GETUTCDATE()),
        (39, 40, 5, 'Perfect Middle Eastern breakfast.', GETUTCDATE()),
        (39, 18, 4, 'Great with crusty bread.', GETUTCDATE()),
        (39, 19, 5, 'Authentic Israeli dish.', GETUTCDATE()),
        (39, 10, 4, 'Perfect for brunch.', GETUTCDATE()),
        
        -- French Onion Soup ratings
        (40, 37, 5, 'Rich and flavorful soup.', GETUTCDATE()),
        (40, 41, 4, 'Perfect for cold weather.', GETUTCDATE()),
        (40, 21, 5, 'Restaurant quality soup.', GETUTCDATE()),
        (40, 22, 4, 'Great with crusty bread.', GETUTCDATE()),
        (40, 23, 5, 'Best onion soup recipe.', GETUTCDATE()),
        
        -- Italian Minestrone ratings
        (41, 38, 4, 'Hearty and healthy soup.', GETUTCDATE()),
        (41, 42, 5, 'Perfect for winter meals.', GETUTCDATE()),
        (41, 24, 4, 'Great with fresh vegetables.', GETUTCDATE()),
        (41, 25, 5, 'Authentic Italian soup.', GETUTCDATE()),
        (41, 26, 4, 'Perfect comfort food.', GETUTCDATE()),
        
        -- Thai Tom Yum Soup ratings
        (42, 39, 5, 'Spicy and sour perfection.', GETUTCDATE()),
        (42, 43, 4, 'Great Thai flavors.', GETUTCDATE()),
        (42, 27, 5, 'Authentic Tom Yum soup.', GETUTCDATE()),
        (42, 88, 4, 'Perfect for cold weather.', GETUTCDATE()),
        (42, 89, 5, 'Best Thai soup recipe.', GETUTCDATE()),
        
        -- Russian Borscht ratings
        (43, 40, 4, 'Traditional and hearty soup.', GETUTCDATE()),
        (43, 44, 5, 'Perfect for cold weather.', GETUTCDATE()),
        (43, 80, 4, 'Great with sour cream.', GETUTCDATE()),
        (43, 11, 5, 'Authentic Russian cuisine.', GETUTCDATE()),
        (43, 32, 4, 'Perfect comfort food.', GETUTCDATE())
END
GO

-- Insert Rich Log Entries Data (diverse logging from different activities)
IF NOT EXISTS (SELECT 1 FROM LogEntries WHERE Id = 1)
BEGIN
    INSERT INTO LogEntries (Level, Message, Exception, Timestamp, Source, UserId, FeatureName, LogType, Duration, Details, Context, ExceptionType, StackTrace)
    VALUES 
        -- Authentication Logs
        ('Information', 'User logged in successfully', NULL, GETUTCDATE(), 'FoodBook.Auth', 1, 'Login', 'UserActivity', '00:00:02', 'User admin logged in from IP 192.168.1.100', 'Web Browser', NULL, NULL),
        ('Information', 'User logged in successfully', NULL, GETUTCDATE(), 'FoodBook.Auth', 2, 'Login', 'UserActivity', '00:00:01', 'User master_chef logged in from IP 192.168.1.101', 'Mobile App', NULL, NULL),
        ('Information', 'User logged in successfully', NULL, GETUTCDATE(), 'FoodBook.Auth', 3, 'Login', 'UserActivity', '00:00:03', 'User italian_cook logged in from IP 192.168.1.102', 'Web Browser', NULL, NULL),
        ('Warning', 'Failed login attempt', 'Invalid credentials', GETUTCDATE(), 'FoodBook.Auth', NULL, 'Login', 'Security', '00:00:01', 'Failed login attempt for email: hacker@evil.com', 'Web Browser', 'InvalidOperationException', 'System.InvalidOperationException: Invalid credentials at AuthenticationService.LoginAsync'),
        ('Information', 'User logged out successfully', NULL, GETUTCDATE(), 'FoodBook.Auth', 1, 'Logout', 'UserActivity', '00:00:01', 'User admin logged out', 'Web Browser', NULL, NULL),
        
        -- Recipe Management Logs
        ('Information', 'Recipe created successfully', NULL, GETUTCDATE(), 'FoodBook.Recipe', 1, 'RecipeCreation', 'UserActivity', '00:00:05', 'Recipe "Classic Chicken Fried Rice" created with 8 ingredients', 'Web Browser', NULL, NULL),
        ('Information', 'Recipe created successfully', NULL, GETUTCDATE(), 'FoodBook.Recipe', 2, 'RecipeCreation', 'UserActivity', '00:00:07', 'Recipe "Thai Green Curry" created with 10 ingredients', 'Mobile App', NULL, NULL),
        ('Information', 'Recipe updated successfully', NULL, GETUTCDATE(), 'FoodBook.Recipe', 3, 'RecipeUpdate', 'UserActivity', '00:00:03', 'Recipe "Japanese Ramen" updated with new instructions', 'Web Browser', NULL, NULL),
        ('Information', 'Recipe deleted successfully', NULL, GETUTCDATE(), 'FoodBook.Recipe', 1, 'RecipeDeletion', 'UserActivity', '00:00:02', 'Recipe "Test Recipe" deleted by user', 'Web Browser', NULL, NULL),
        ('Error', 'Recipe validation failed', 'Invalid ingredient list', GETUTCDATE(), 'FoodBook.Recipe', 1, 'RecipeValidation', 'Error', '00:00:01', 'Recipe validation failed: Empty ingredient list', 'Web Browser', 'ValidationException', 'System.ValidationException: Recipe must have at least one ingredient'),
        
        -- AI Service Logs
        ('Information', 'AI recipe generation completed', NULL, GETUTCDATE(), 'FoodBook.AI', 1, 'AIRecipeGeneration', 'AIActivity', '00:00:15', 'Generated recipe for ingredients: chicken, rice, vegetables', 'AI Service', NULL, NULL),
        ('Information', 'AI dish analysis completed', NULL, GETUTCDATE(), 'FoodBook.AI', 2, 'AIDishAnalysis', 'AIActivity', '00:00:20', 'Analyzed dish image: Thai Green Curry', 'AI Service', NULL, NULL),
        ('Information', 'AI nutrition analysis completed', NULL, GETUTCDATE(), 'FoodBook.AI', 3, 'AINutritionAnalysis', 'AIActivity', '00:00:12', 'Analyzed nutrition for recipe: Mediterranean Buddha Bowl', 'AI Service', NULL, NULL),
        ('Warning', 'AI service timeout', 'Request timeout', GETUTCDATE(), 'FoodBook.AI', 1, 'AIService', 'Performance', '00:02:00', 'AI service request timed out after 2 minutes', 'AI Service', 'TimeoutException', 'System.TimeoutException: AI service request timed out'),
        ('Error', 'AI service unavailable', 'Service unavailable', GETUTCDATE(), 'FoodBook.AI', 2, 'AIService', 'Error', '00:00:05', 'AI service returned 503 Service Unavailable', 'AI Service', 'HttpRequestException', 'System.Net.Http.HttpRequestException: 503 Service Unavailable'),
        
        -- Performance Logs
        ('Information', 'Database query executed', NULL, GETUTCDATE(), 'FoodBook.Data', 1, 'DatabaseQuery', 'Performance', '00:00:01', 'Query executed in 1.2 seconds: SELECT * FROM Recipes WHERE UserId = 1', 'Database', NULL, NULL),
        ('Information', 'Database query executed', NULL, GETUTCDATE(), 'FoodBook.Data', 2, 'DatabaseQuery', 'Performance', '00:00:02', 'Query executed in 2.1 seconds: SELECT * FROM Ingredients WHERE Category = "Protein"', 'Database', NULL, NULL),
        ('Warning', 'Slow database query detected', NULL, GETUTCDATE(), 'FoodBook.Data', 3, 'DatabaseQuery', 'Performance', '00:00:05', 'Query executed in 5.3 seconds: Complex JOIN query on Recipes and Ingredients', 'Database', NULL, NULL),
        ('Information', 'Cache hit', NULL, GETUTCDATE(), 'FoodBook.Cache', 1, 'Cache', 'Performance', '00:00:00', 'Cache hit for key: recipes_user_1', 'Cache', NULL, NULL),
        ('Information', 'Cache miss', NULL, GETUTCDATE(), 'FoodBook.Cache', 2, 'Cache', 'Performance', '00:00:01', 'Cache miss for key: ingredients_category_protein', 'Cache', NULL, NULL),
        
        -- User Activity Logs
        ('Information', 'User profile updated', NULL, GETUTCDATE(), 'FoodBook.User', 1, 'ProfileUpdate', 'UserActivity', '00:00:03', 'User admin updated profile information', 'Web Browser', NULL, NULL),
        ('Information', 'User profile updated', NULL, GETUTCDATE(), 'FoodBook.User', 2, 'ProfileUpdate', 'UserActivity', '00:00:02', 'User master_chef updated profile picture', 'Mobile App', NULL, NULL),
        ('Information', 'User registered successfully', NULL, GETUTCDATE(), 'FoodBook.User', 4, 'UserRegistration', 'UserActivity', '00:00:05', 'New user french_cook registered with email pierre@foodbook.com', 'Web Browser', NULL, NULL),
        ('Information', 'User registered successfully', NULL, GETUTCDATE(), 'FoodBook.User', 5, 'UserRegistration', 'UserActivity', '00:00:04', 'New user mexican_chef registered with email carlos@foodbook.com', 'Mobile App', NULL, NULL),
        ('Warning', 'User registration failed', 'Email already exists', GETUTCDATE(), 'FoodBook.User', NULL, 'UserRegistration', 'Error', '00:00:01', 'Registration failed: Email admin@foodbook.com already exists', 'Web Browser', 'InvalidOperationException', 'System.InvalidOperationException: Email already exists'),
        
        -- Recipe Rating Logs
        ('Information', 'Recipe rated successfully', NULL, GETUTCDATE(), 'FoodBook.Rating', 1, 'RecipeRating', 'UserActivity', '00:00:01', 'User rated recipe "Classic Chicken Fried Rice" with 5 stars', 'Web Browser', NULL, NULL),
        ('Information', 'Recipe rated successfully', NULL, GETUTCDATE(), 'FoodBook.Rating', 2, 'RecipeRating', 'UserActivity', '00:00:01', 'User rated recipe "Thai Green Curry" with 4 stars', 'Mobile App', NULL, NULL),
        ('Information', 'Recipe rated successfully', NULL, GETUTCDATE(), 'FoodBook.Rating', 3, 'RecipeRating', 'UserActivity', '00:00:01', 'User rated recipe "Japanese Ramen" with 5 stars', 'Web Browser', NULL, NULL),
        ('Information', 'Recipe comment added', NULL, GETUTCDATE(), 'FoodBook.Rating', 1, 'RecipeComment', 'UserActivity', '00:00:02', 'User added comment to recipe "Classic Chicken Fried Rice"', 'Web Browser', NULL, NULL),
        ('Information', 'Recipe comment added', NULL, GETUTCDATE(), 'FoodBook.Rating', 2, 'RecipeComment', 'UserActivity', '00:00:03', 'User added comment to recipe "Thai Green Curry"', 'Mobile App', NULL, NULL),
        
        -- Shopping List Logs
        ('Information', 'Shopping list generated', NULL, GETUTCDATE(), 'FoodBook.Shopping', 1, 'ShoppingListGeneration', 'UserActivity', '00:00:08', 'Shopping list generated for 5 recipes with 15 ingredients', 'Web Browser', NULL, NULL),
        ('Information', 'Shopping list generated', NULL, GETUTCDATE(), 'FoodBook.Shopping', 2, 'ShoppingListGeneration', 'UserActivity', '00:00:06', 'Shopping list generated for 3 recipes with 12 ingredients', 'Mobile App', NULL, NULL),
        ('Information', 'Shopping list updated', NULL, GETUTCDATE(), 'FoodBook.Shopping', 1, 'ShoppingListUpdate', 'UserActivity', '00:00:02', 'Shopping list updated with new recipe ingredients', 'Web Browser', NULL, NULL),
        ('Information', 'Shopping list item checked off', NULL, GETUTCDATE(), 'FoodBook.Shopping', 1, 'ShoppingListUpdate', 'UserActivity', '00:00:01', 'User checked off "Chicken Breast" from shopping list', 'Mobile App', NULL, NULL),
        ('Information', 'Shopping list cleared', NULL, GETUTCDATE(), 'FoodBook.Shopping', 1, 'ShoppingListClear', 'UserActivity', '00:00:01', 'User cleared completed shopping list', 'Web Browser', NULL, NULL),
        
        -- Nutrition Analysis Logs
        ('Information', 'Nutrition analysis completed', NULL, GETUTCDATE(), 'FoodBook.Nutrition', 1, 'NutritionAnalysis', 'UserActivity', '00:00:10', 'Nutrition analysis completed for recipe "Classic Chicken Fried Rice"', 'Web Browser', NULL, NULL),
        ('Information', 'Nutrition analysis completed', NULL, GETUTCDATE(), 'FoodBook.Nutrition', 2, 'NutritionAnalysis', 'UserActivity', '00:00:12', 'Nutrition analysis completed for recipe "Thai Green Curry"', 'Mobile App', NULL, NULL),
        ('Information', 'Nutrition analysis completed', NULL, GETUTCDATE(), 'FoodBook.Nutrition', 3, 'NutritionAnalysis', 'UserActivity', '00:00:08', 'Nutrition analysis completed for recipe "Japanese Ramen"', 'Web Browser', NULL, NULL),
        ('Warning', 'Nutrition data incomplete', NULL, GETUTCDATE(), 'FoodBook.Nutrition', 1, 'NutritionAnalysis', 'Warning', '00:00:05', 'Nutrition analysis completed with missing data for some ingredients', 'Web Browser', NULL, NULL),
        ('Error', 'Nutrition analysis failed', 'Invalid ingredient data', GETUTCDATE(), 'FoodBook.Nutrition', 2, 'NutritionAnalysis', 'Error', '00:00:03', 'Nutrition analysis failed: Invalid ingredient "Unknown Spice"', 'Web Browser', 'ArgumentException', 'System.ArgumentException: Invalid ingredient data'),
        
        -- Image Management Logs
        ('Information', 'Recipe image uploaded', NULL, GETUTCDATE(), 'FoodBook.Image', 1, 'ImageUpload', 'UserActivity', '00:00:15', 'Recipe image uploaded for "Classic Chicken Fried Rice" (2.3MB)', 'Web Browser', NULL, NULL),
        ('Information', 'Recipe image uploaded', NULL, GETUTCDATE(), 'FoodBook.Image', 2, 'ImageUpload', 'UserActivity', '00:00:12', 'Recipe image uploaded for "Thai Green Curry" (1.8MB)', 'Mobile App', NULL, NULL),
        ('Information', 'Recipe image deleted', NULL, GETUTCDATE(), 'FoodBook.Image', 1, 'ImageDeletion', 'UserActivity', '00:00:02', 'Recipe image deleted for "Test Recipe"', 'Web Browser', NULL, NULL),
        ('Warning', 'Image upload failed', 'File too large', GETUTCDATE(), 'FoodBook.Image', 3, 'ImageUpload', 'Error', '00:00:05', 'Image upload failed: File size 15MB exceeds limit of 10MB', 'Web Browser', 'InvalidOperationException', 'System.InvalidOperationException: File too large'),
        ('Error', 'Image processing failed', 'Invalid image format', GETUTCDATE(), 'FoodBook.Image', 4, 'ImageProcessing', 'Error', '00:00:03', 'Image processing failed: Unsupported format .bmp', 'Web Browser', 'NotSupportedException', 'System.NotSupportedException: Unsupported image format'),
        
        -- System Logs
        ('Information', 'System startup completed', NULL, GETUTCDATE(), 'FoodBook.System', NULL, 'SystemStartup', 'System', '00:00:30', 'FoodBook application started successfully', 'System', NULL, NULL),
        ('Information', 'Database migration completed', NULL, GETUTCDATE(), 'FoodBook.Data', NULL, 'DatabaseMigration', 'System', '00:01:15', 'Database migration completed successfully', 'System', NULL, NULL),
        ('Information', 'Cache cleared', NULL, GETUTCDATE(), 'FoodBook.Cache', NULL, 'CacheClear', 'System', '00:00:05', 'Application cache cleared successfully', 'System', NULL, NULL),
        ('Warning', 'High memory usage detected', NULL, GETUTCDATE(), 'FoodBook.System', NULL, 'SystemMonitoring', 'Performance', '00:00:01', 'Memory usage: 85% of available memory', 'System', NULL, NULL),
        ('Error', 'Database connection lost', 'Connection timeout', GETUTCDATE(), 'FoodBook.Data', NULL, 'DatabaseConnection', 'Error', '00:00:10', 'Database connection lost: Connection timeout after 10 seconds', 'System', 'SqlException', 'System.Data.SqlClient.SqlException: Connection timeout'),
        
        -- Feature Usage Logs
        ('Information', 'Analytics page accessed', NULL, GETUTCDATE(), 'FoodBook.Analytics', 1, 'Analytics', 'FeatureUsage', '00:00:05', 'User accessed analytics page with charts and statistics', 'Web Browser', NULL, NULL),
        ('Information', 'Analytics page accessed', NULL, GETUTCDATE(), 'FoodBook.Analytics', 2, 'Analytics', 'FeatureUsage', '00:00:03', 'User accessed analytics page with charts and statistics', 'Mobile App', NULL, NULL),
        ('Information', 'Recipe search performed', NULL, GETUTCDATE(), 'FoodBook.Search', 1, 'RecipeSearch', 'FeatureUsage', '00:00:02', 'User searched for recipes with keyword "chicken"', 'Web Browser', NULL, NULL),
        ('Information', 'Recipe search performed', NULL, GETUTCDATE(), 'FoodBook.Search', 2, 'RecipeSearch', 'FeatureUsage', '00:00:01', 'User searched for recipes with keyword "thai"', 'Mobile App', NULL, NULL),
        ('Information', 'Ingredient filter applied', NULL, GETUTCDATE(), 'FoodBook.Filter', 1, 'IngredientFilter', 'FeatureUsage', '00:00:01', 'User filtered recipes by ingredient "rice"', 'Web Browser', NULL, NULL),
        
        -- Error Logs
        ('Error', 'Application error occurred', 'Null reference exception', GETUTCDATE(), 'FoodBook.App', 1, 'ApplicationError', 'Error', '00:00:01', 'Null reference exception in MainViewModel.LoadRecipesAsync', 'Web Browser', 'NullReferenceException', 'System.NullReferenceException: Object reference not set to an instance of an object at MainViewModel.LoadRecipesAsync'),
        ('Error', 'Application error occurred', 'Invalid operation exception', GETUTCDATE(), 'FoodBook.App', 2, 'ApplicationError', 'Error', '00:00:01', 'Invalid operation exception in RecipeService.CreateRecipeAsync', 'Mobile App', 'InvalidOperationException', 'System.InvalidOperationException: Recipe validation failed'),
        ('Error', 'Application error occurred', 'Timeout exception', GETUTCDATE(), 'FoodBook.App', 3, 'ApplicationError', 'Error', '00:00:05', 'Timeout exception in DatabaseService.ExecuteQueryAsync', 'Web Browser', 'TimeoutException', 'System.TimeoutException: Database query timed out'),
        ('Error', 'Application error occurred', 'Unauthorized access exception', GETUTCDATE(), 'FoodBook.App', NULL, 'ApplicationError', 'Error', '00:00:01', 'Unauthorized access exception in UserService.GetUserProfileAsync', 'Web Browser', 'UnauthorizedAccessException', 'System.UnauthorizedAccessException: Access denied'),
        ('Error', 'Application error occurred', 'Argument exception', GETUTCDATE(), 'FoodBook.App', 1, 'ApplicationError', 'Error', '00:00:01', 'Argument exception in RecipeService.ValidateRecipeAsync', 'Web Browser', 'ArgumentException', 'System.ArgumentException: Recipe title cannot be empty')
END
GO

PRINT 'FoodBook database created successfully with sample data!'
PRINT 'Database: FoodBook'
PRINT 'Tables: Users, Recipes, Ingredients, RecipeIngredients, Ratings, LogEntries'
PRINT 'Sample data inserted for testing'
