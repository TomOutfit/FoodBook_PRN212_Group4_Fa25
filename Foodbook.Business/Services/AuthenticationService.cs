using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Foodbook.Business.Interfaces;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Foodbook.Business.Models;

namespace Foodbook.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly FoodbookDbContext _context;
        private User? _currentUser;
        private readonly ISettingsService? _settingsService;

        public AuthenticationService(FoodbookDbContext context, ISettingsService? settingsService = null)
        {
            _context = context;
            _settingsService = settingsService;
        }

        public async Task<User?> LoginAsync(LoginModel loginModel)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == loginModel.Email.ToLower());

                if (user == null)
                    return null;

                // Kiểm tra cả plain text và hash
                bool isValidPassword = loginModel.Password == user.Password || 
                                    VerifyPassword(loginModel.Password, user.PasswordHash);

                if (!isValidPassword)
                    return null;

                _currentUser = user;
                
                // Lưu user ID vào settings để có thể lấy lại sau
                if (_settingsService != null)
                {
                    var settings = await _settingsService.GetSettingsAsync();
                    settings.CurrentUserId = user.Id;
                    await _settingsService.SaveSettingsAsync(settings);
                }
                
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<User?> RegisterAsync(RegisterModel registerModel)
        {
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == registerModel.Email.ToLower()))
                    return null;

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username.ToLower() == registerModel.Username.ToLower()))
                    return null;

                var user = new User
                {
                    Username = registerModel.Username,
                    Email = registerModel.Email,
                    Password = registerModel.Password, // Lưu plain text
                    PasswordHash = HashPassword(registerModel.Password), // Lưu hash
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _currentUser = user;
                
                // Lưu user ID vào settings để có thể lấy lại sau
                if (_settingsService != null)
                {
                    var settings = await _settingsService.GetSettingsAsync();
                    settings.CurrentUserId = user.Id;
                    await _settingsService.SaveSettingsAsync(settings);
                }
                
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                return !await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ValidateUsernameAsync(string username)
        {
            try
            {
                return !await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            _currentUser = null;
            
            // Xóa user ID khỏi settings
            if (_settingsService != null)
            {
                try
                {
                    var settings = await _settingsService.GetSettingsAsync();
                    settings.CurrentUserId = null;
                    await _settingsService.SaveSettingsAsync(settings);
                }
                catch
                {
                    // Nếu có lỗi khi xóa settings, vẫn trả về true
                }
            }
            
            return true;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            // Nếu đã có user trong memory, trả về
            if (_currentUser != null)
                return _currentUser;
            
            // Nếu không có, thử lấy từ settings và database
            if (_settingsService != null)
            {
                try
                {
                    var settings = await _settingsService.GetSettingsAsync();
                    if (settings.CurrentUserId.HasValue)
                    {
                        var user = await _context.Users.FindAsync(settings.CurrentUserId.Value);
                        if (user != null)
                        {
                            _currentUser = user;
                            return user;
                        }
                    }
                }
                catch
                {
                    // Nếu có lỗi, trả về null
                }
            }
            
            return null;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
                    return false;

                user.PasswordHash = HashPassword(newPassword);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                    return false;

                // In a real application, you would send a password reset email here
                // For now, we'll just return true
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }

        public async Task<User?> GetAdminUserAsync()
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.IsAdmin == true);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
