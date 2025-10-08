using Foodbook.Data.Entities;
using Foodbook.Business.Models;

namespace Foodbook.Business.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User?> LoginAsync(LoginModel loginModel);
        Task<User?> RegisterAsync(RegisterModel registerModel);
        Task<bool> ValidateEmailAsync(string email);
        Task<bool> ValidateUsernameAsync(string username);
        Task<bool> LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
    }
}
