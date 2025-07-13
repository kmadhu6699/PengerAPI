using System.Collections.Generic;
using System.Threading.Tasks;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<IEnumerable<Account>> GetAccountsByUserIdAsync(int userId);
        Task<IEnumerable<Account>> GetByUserIdAsync(int userId);
        Task<Account?> GetAccountWithDetailsAsync(int accountId);
        Task<Account?> GetByIdWithDetailsAsync(int accountId);
        Task<IEnumerable<Account>> GetAccountsByCurrencyAsync(int currencyId);
        Task<IEnumerable<Account>> GetAccountsByTypeAsync(int accountTypeId);
        Task<decimal> GetTotalBalanceByUserAsync(int userId);
        Task<decimal> GetTotalBalanceByUserAndCurrencyAsync(int userId, int currencyId);
        Task<bool> AccountExistsForUserAsync(int userId, string accountName);
    }
}
