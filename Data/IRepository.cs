using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PengerAPI.Models;

namespace PengerAPI.Data
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}