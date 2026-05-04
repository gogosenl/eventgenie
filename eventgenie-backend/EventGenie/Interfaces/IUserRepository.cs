using EventGenie.Models;

namespace EventGenie.Interfaces
{
    public interface IUserRepository
    {      
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(int id);

        Task AddUserAsync(User user);

        Task UpdateUserAsync(User user);

        Task DeleteUserAsync(int id);

        Task<bool> UserExistsAsync(int id);

        // Login işlemleri için kritik 
        Task<User?> GetUserByEmailAsync(string email);
    }
}
