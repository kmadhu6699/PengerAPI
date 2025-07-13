using Microsoft.EntityFrameworkCore;
using PengerAPI.DTOs;
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

        public async Task<User?> GetByIdWithAccountsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Currency)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.AccountType)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<PagedResult<User>> SearchAsync(string query, int pageNumber, int pageSize)
        {
            var queryable = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                queryable = queryable.Where(u =>
                    u.FirstName.Contains(query) ||
                    u.LastName.Contains(query) ||
                    u.Email.Contains(query) ||
                    u.Username.Contains(query));
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>(items, totalCount, pageNumber, pageSize);
        }
    }
}
