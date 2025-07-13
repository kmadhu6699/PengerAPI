using Microsoft.EntityFrameworkCore;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Currency?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<Currency?> GetBySymbolAsync(string symbol)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Symbol == symbol);
        }

        public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            return await _dbSet.AnyAsync(c => c.Code == code);
        }

        public async Task<bool> SymbolExistsAsync(string symbol)
        {
            return await _dbSet.AnyAsync(c => c.Symbol == symbol);
        }

        public async Task<IEnumerable<Currency>> GetActiveAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
