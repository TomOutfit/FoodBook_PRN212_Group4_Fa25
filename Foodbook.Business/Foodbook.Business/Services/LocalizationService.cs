using System;
using System.Collections.Generic;
using Foodbook.Business.Interfaces;

namespace Foodbook.Business.Services
{
    public class LocalizationService : ILocalizationService
    {
        private string _currentLanguage = "English";
        private Dictionary<string, Dictionary<string, string>> _translations = null!;

        public string CurrentLanguage 
        { 
            get => _currentLanguage; 
            set 
            { 
                _currentLanguage = value;
                OnLanguageChanged?.Invoke();
            } 
        }

        public event Action? OnLanguageChanged;

        public LocalizationService()
        {
            InitializeTranslations();
        }

        private void InitializeTranslations()
        {
            // Initialize with translations
            var translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["English"] = new Dictionary<string, string>
                {
                    // Sidebar
                    ["Dashboard"] = "Dashboard",
                    ["My Recipes"] = "My Recipes",
                    ["Ingredients"] = "Ingredients",
                    ["Shopping List"] = "Shopping List",
                    ["Analytics"] = "Analytics",
                    ["Settings"] = "Settings",
                    
                    // Dashboard
                    ["Total Recipes"] = "Total Recipes",
                    ["Main Dishes"] = "Main Dishes",
                    ["Desserts"] = "Desserts",
                    ["Quick Meals"] = "Quick Meals",
                    ["Average Cook Time"] = "Average Cook Time",
                    ["Most Used Ingredient"] = "Most Used Ingredient",
                    ["AI Recipes"] = "AI Recipes",
                    ["Favorite Recipes"] = "Favorite Recipes",
                    
                    // Settings
                    ["Application Settings"] = "Application Settings",
                    ["Theme"] = "Theme",
                    ["Language"] = "Language",
                    ["Default Servings"] = "Default Servings",
                    ["Enable Notifications"] = "Enable Notifications",
                    ["Save Settings"] = "Save Settings",
                    ["User Profile"] = "User Profile",
                    ["Username"] = "Username",
                    ["Email"] = "Email",
                    
                    // Recipe Collection
                    ["All Recipes"] = "All Recipes",
                    ["Search Recipes"] = "Search Recipes",
                    ["Add Recipe"] = "Add Recipe",
                    ["Sort By"] = "Sort By",
                    
                    // Common
                    ["Loading"] = "Loading...",
                    ["Error"] = "Error",
                    ["Success"] = "Success",
                    ["Confirm"] = "Confirm",
                    ["Cancel"] = "Cancel",
                    ["Close"] = "Close",
                },
                
                ["Vietnamese"] = new Dictionary<string, string>
                {
                    // Sidebar
                    ["Dashboard"] = "Bảng Điều Khiển",
                    ["My Recipes"] = "Công Thức Của Tôi",
                    ["Ingredients"] = "Nguyên Liệu",
                    ["Shopping List"] = "Danh Sách Mua Sắm",
                    ["Analytics"] = "Phân Tích",
                    ["Settings"] = "Cài Đặt",
                    
                    // Dashboard
                    ["Total Recipes"] = "Tổng Số Công Thức",
                    ["Main Dishes"] = "Món Chính",
                    ["Desserts"] = "Món Tráng Miệng",
                    ["Quick Meals"] = "Bữa Ăn Nhanh",
                    ["Average Cook Time"] = "Thời Gian Nấu Trung Bình",
                    ["Most Used Ingredient"] = "Nguyên Liệu Dùng Nhiều Nhất",
                    ["AI Recipes"] = "Công Thức AI",
                    ["Favorite Recipes"] = "Công Thức Yêu Thích",
                    
                    // Settings
                    ["Application Settings"] = "Cài Đặt Ứng Dụng",
                    ["Theme"] = "Giao Diện",
                    ["Language"] = "Ngôn Ngữ",
                    ["Default Servings"] = "Khẩu Phần Mặc Định",
                    ["Enable Notifications"] = "Bật Thông Báo",
                    ["Save Settings"] = "Lưu Cài Đặt",
                    ["User Profile"] = "Hồ Sơ Người Dùng",
                    ["Username"] = "Tên Người Dùng",
                    ["Email"] = "Email",
                    
                    // Recipe Collection
                    ["All Recipes"] = "Tất Cả Công Thức",
                    ["Search Recipes"] = "Tìm Kiếm Công Thức",
                    ["Add Recipe"] = "Thêm Công Thức",
                    ["Sort By"] = "Sắp Xếp Theo",
                    
                    // Common
                    ["Loading"] = "Đang Tải...",
                    ["Error"] = "Lỗi",
                    ["Success"] = "Thành Công",
                    ["Confirm"] = "Xác Nhận",
                    ["Cancel"] = "Hủy",
                    ["Close"] = "Đóng",
                }
            };
            
            _translations = translations;
        }

        public string GetString(string key)
        {
            if (_translations.TryGetValue(_currentLanguage, out var langDict) && 
                langDict.TryGetValue(key, out var value))
            {
                return value;
            }
            
            // Fallback to English if translation not found
            if (_translations.TryGetValue("English", out var engDict) && 
                engDict.TryGetValue(key, out var engValue))
            {
                return engValue;
            }
            
            return key; // Return key if not found in any language
        }

        public void ChangeLanguage(string language)
        {
            if (_translations.ContainsKey(language))
            {
                CurrentLanguage = language;
            }
        }
    }
}

