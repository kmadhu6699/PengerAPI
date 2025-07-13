using System;
using System.Threading.Tasks;

namespace PengerAPI.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IAccountRepository Accounts { get; }
        ICurrencyRepository Currencies { get; }
        IAccountTypeRepository AccountTypes { get; }
        IOTPRepository OTPs { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
