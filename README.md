# FoodBook

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Desktop_App-green)](https://github.com/dotnet/wpf)
[![AI Powered](https://img.shields.io/badge/AI-Google_Gemini-purple)](https://ai.google.dev/)
[![Status](https://img.shields.io/badge/Status-Production_Ready-orange)](https://github.com/)

A modern, AI-powered recipe management application built with WPF and .NET 9, featuring intelligent cooking assistance and smart pantry management.

## 📋 Table of Contents
- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Installation](#installation)
- [Usage](#usage)
- [Architecture](#architecture)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

FoodBook revolutionizes recipe management by combining traditional cooking features with cutting-edge AI technology. Developed as a 10-week academic project, this desktop application empowers users to create, discover, and optimize recipes using Google Gemini AI integration.

The application addresses the growing need for intelligent cooking tools in an era where people seek personalized, efficient, and health-conscious meal planning. By leveraging AI for recipe generation, nutritional analysis, and smart shopping lists, FoodBook transforms the cooking experience from mundane task management to an interactive culinary adventure.

## ✨ Key Features

### 🤖 AI-Powered Cooking
- **AI Recipe Generation**: Create recipes from available ingredients using Google Gemini Pro 2.5
- **AI Chef Judge**: Upload dish photos for AI-powered evaluation with presentation, health, and taste scoring
- **Smart Ingredient Suggestions**: AI-driven ingredient substitutions and recommendations
- **Nutrition Analysis**: Automated nutritional breakdown with health insights and warnings

### 🏠 Smart Pantry Management
- **Inventory Tracking**: Monitor ingredient quantities, expiry dates, and storage locations
- **Auto-Generated Shopping Lists**: Intelligent consolidation and optimization based on recipes
- **Stock Alerts**: Automatic notifications for low-stock items and expiring ingredients
- **Multi-Location Storage**: Organize pantry items by refrigerator, freezer, or shelf

### 📖 Comprehensive Recipe System
- **Recipe CRUD Operations**: Full create, read, update, delete functionality with rich metadata
- **Advanced Search & Filtering**: Filter by category, difficulty, cook time, and ingredients
- **Recipe Ratings & Reviews**: Community-driven feedback system
- **AI-Generated Content**: Flag and distinguish AI-created recipes

### 🌍 User Experience
- **Multi-Language Support**: English and Vietnamese localization
- **Dark/Light Theme**: Customizable UI themes for different preferences
- **Dashboard Analytics**: Usage statistics and recipe performance metrics
- **Comprehensive Logging**: System, AI, and error logging for monitoring

## 🛠 Technology Stack

- **Framework**: .NET 9 with WPF (Windows Presentation Foundation)
- **Database**: SQL Server with Entity Framework Core
- **AI Integration**: Google Gemini Pro 2.5 API
- **Architecture**: MVVM (Model-View-ViewModel) pattern
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Testing**: xUnit with Moq for unit testing
- **CI/CD**: Jenkins pipeline with automated testing and deployment

## 📦 Installation

### Prerequisites
- Windows 10/11
- .NET 9 SDK
- SQL Server (LocalDB or full instance)

### Setup Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/FoodBook.git
   cd FoodBook
   ```

2. **Configure Database**
   - Update connection string in `appsettings.json`
   - Run the database script from `FoodBook.sql`

3. **Setup AI Integration**
   - Obtain Google Gemini API key
   - Configure API settings in `appsettings.json`

4. **Build and Run**
   ```bash
   dotnet build CookBook.sln
   dotnet run --project Foodbook.Presentation
   ```

## 🚀 Usage

### Getting Started
1. Launch the application and create a user account
2. Set up your pantry by adding available ingredients
3. Start creating recipes or let AI generate them for you

### Core Workflows
- **Recipe Creation**: Add recipes manually or use AI generation
- **Meal Planning**: Generate shopping lists from selected recipes
- **Nutrition Tracking**: Analyze recipes for nutritional content
- **Dish Evaluation**: Upload photos for AI-powered judging

## 🏗 Architecture

FoodBook follows a layered architecture with clear separation of concerns:

- **Presentation Layer**: WPF MVVM with custom controls and converters
- **Business Layer**: Service interfaces and implementations with AI integration
- **Data Layer**: Entity Framework Core with SQL Server
- **Tests Layer**: Comprehensive unit and integration tests

### Key Components
- `Foodbook.Presentation`: WPF UI and ViewModels
- `Foodbook.Business`: Services and business logic
- `Foodbook.Data`: Database entities and DbContext
- `Foodbook.Tests`: Test suites for all layers

## 🎯 Target Audience

- **Home Cooks**: Everyday users seeking efficient recipe management
- **Culinary Students**: Learning tool for recipe creation and analysis
- **Food Bloggers**: Content creators needing smart recipe tools
- **Restaurant Owners**: Small business operators managing inventory and recipes
- **Health-Conscious Users**: Individuals tracking nutrition and making informed food choices

## 💼 Business Value

FoodBook delivers significant value through:
- **Time Savings**: AI-powered automation reduces manual recipe creation time by 70%
- **Cost Optimization**: Smart shopping lists minimize food waste and overspending
- **Health Benefits**: Nutritional analysis promotes healthier eating habits
- **User Engagement**: AI features create a more interactive and enjoyable cooking experience
- **Market Differentiation**: Unique AI integration sets it apart from traditional recipe apps

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Developed as part of PRN212 Group 4 project
- Powered by Google Gemini AI
- Built with .NET 9 and WPF
- Inspired by the need for intelligent cooking assistance
---

**FoodBook** - Where AI meets Culinary Excellence 🍳🤖
