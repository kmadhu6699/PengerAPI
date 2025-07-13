using System.Collections.Generic;
using System.Threading.Tasks;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public interface IAccountTypeRepository : IRepository<AccountType>
    {
        Task<AccountType?> GetByNameAsync(string name);
        Task<IEnumerable<AccountType>> GetActiveAccountTypesAsync();
        Task<bool> NameExistsAsync(string name);
    }
}
