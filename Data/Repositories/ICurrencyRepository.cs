using System.Collections.Generic;
using System.Threading.Tasks;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        Task<Currency?> GetByCodeAsync(string code);
        Task<Currency?> GetBySymbolAsync(string symbol);
        Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
        Task<IEnumerable<Currency>> GetActiveAsync();
        Task<bool> CodeExistsAsync(string code);
        Task<bool> SymbolExistsAsync(string symbol);
    }
}
