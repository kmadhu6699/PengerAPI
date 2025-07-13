using System.Collections.Generic;
using System.Threading.Tasks;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public interface IOTPRepository : IRepository<OTP>
    {
        Task<OTP?> GetValidOTPAsync(int userId, string code);
        Task<IEnumerable<OTP>> GetOTPsByUserIdAsync(int userId);
        Task<OTP?> GetLatestOTPByUserIdAsync(int userId);
        Task InvalidateOTPsForUserAsync(int userId);
        Task<bool> IsValidOTPAsync(int userId, string code);
        Task CleanupExpiredOTPsAsync();
    }
}
