using System.Threading.Tasks;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<User?> GetUserWithAccountsAsync(int userId);
        Task<User?> GetUserWithOTPsAsync(int userId);
    }
}
