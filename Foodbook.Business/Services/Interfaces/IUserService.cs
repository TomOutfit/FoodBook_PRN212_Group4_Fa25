using Foodbook.Data.Entities;

namespace Foodbook.Business.Interfaces
{
    public interface IUserService
    {
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ValidatePasswordAsync(string password, string hashedPassword);
        Task<string> HashPasswordAsync(string password);
    }
}
