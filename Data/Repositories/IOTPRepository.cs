using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public interface IOTPRepository : IRepository<OTP>
    {
        Task<OTP?> GetValidOTPAsync(int userId, string code);
        Task<IEnumerable<OTP>> GetOTPsByUserIdAsync(int userId);
        Task<PagedResult<OTP>> GetByUserIdPagedAsync(int userId, int pageNumber, int pageSize);
        Task<OTP?> GetLatestOTPByUserIdAsync(int userId);
        Task<OTP?> GetActiveOTPAsync(int userId, string purpose);
        Task<OTP?> GetByCodeAsync(string code);
        Task<OTP?> GetRecentOTPAsync(int userId, string purpose, TimeSpan timeSpan);
        Task InvalidateOTPsForUserAsync(int userId);
        Task<bool> IsValidOTPAsync(int userId, string code);
        Task<int> DeleteExpiredOTPsAsync();
    }
}
