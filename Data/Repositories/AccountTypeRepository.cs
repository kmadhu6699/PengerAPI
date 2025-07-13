using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public class AccountTypeRepository : Repository<AccountType>, IAccountTypeRepository
    {
        public AccountTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<AccountType?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(at => at.Name == name);
        }

        public async Task<IEnumerable<AccountType>> GetActiveAccountTypesAsync()
        {
            return await _dbSet
                .Where(at => at.IsActive)
                .OrderBy(at => at.Name)
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _dbSet.AnyAsync(at => at.Name == name);
        }
    }
}
