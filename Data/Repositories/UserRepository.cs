using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserWithAccountsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Accounts)
                    .ThenInclude(a => a.Currency)
                .Include(u => u.Accounts)
                    .ThenInclude(a => a.AccountType)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserWithOTPsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.OTPs)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
