using EventGenie.Data;
using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.EntityFrameworkCore;

namespace EventGenie.Repositorys
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var normalized = email.Trim();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserEmail != null &&
                    u.UserEmail.Trim().ToLower() == normalized.ToLower());
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser != null)
            {
                existingUser.UserName = user.UserName;
                existingUser.UserSurname = user.UserSurname;
                existingUser.UserEmail = user.UserEmail;
                existingUser.UserPassword = user.UserPassword;
                existingUser.UserDateOfBirth = user.UserDateOfBirth;
                existingUser.Preferences = user.Preferences;
                existingUser.EventRange = user.EventRange;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.UserId == id);
        }
    }
}