using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Account>> GetAccountsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(a => a.Currency)
                .Include(a => a.AccountType)
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<Account?> GetAccountWithDetailsAsync(int accountId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.Currency)
                .Include(a => a.AccountType)
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<IEnumerable<Account>> GetAccountsByCurrencyAsync(int currencyId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.AccountType)
                .Where(a => a.CurrencyId == currencyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountsByTypeAsync(int accountTypeId)
        {
            return await _dbSet
                .Include(a => a.User)
                .Include(a => a.Currency)
                .Where(a => a.AccountTypeId == accountTypeId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalBalanceByUserAsync(int userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.Balance);
        }

        public async Task<decimal> GetTotalBalanceByUserAndCurrencyAsync(int userId, int currencyId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId && a.CurrencyId == currencyId)
                .SumAsync(a => a.Balance);
        }

        public async Task<bool> AccountExistsForUserAsync(int userId, string accountName)
        {
            return await _dbSet.AnyAsync(a => a.UserId == userId && a.Name == accountName);
        }
    }
}
