using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using PengerAPI.Data.Repositories;

namespace PengerAPI.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Accounts = new AccountRepository(_context);
            Currencies = new CurrencyRepository(_context);
            AccountTypes = new AccountTypeRepository(_context);
            OTPs = new OTPRepository(_context);
        }

        public IUserRepository Users { get; private set; }
        public IAccountRepository Accounts { get; private set; }
        public ICurrencyRepository Currencies { get; private set; }
        public IAccountTypeRepository AccountTypes { get; private set; }
        public IOTPRepository OTPs { get; private set; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
