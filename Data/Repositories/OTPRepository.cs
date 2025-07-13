using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Data.Repositories
{
    public class OTPRepository : Repository<OTP>, IOTPRepository
    {
        public OTPRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<OTP?> GetValidOTPAsync(int userId, string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(otp => 
                    otp.UserId == userId && 
                    otp.Code == code && 
                    otp.ExpiresAt > DateTime.UtcNow && 
                    !otp.IsUsed);
        }

        public async Task<IEnumerable<OTP>> GetOTPsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(otp => otp.UserId == userId)
                .OrderByDescending(otp => otp.CreatedAt)
                .ToListAsync();
        }

        public async Task<OTP?> GetLatestOTPByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(otp => otp.UserId == userId)
                .OrderByDescending(otp => otp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidateOTPsForUserAsync(int userId)
        {
            var otps = await _dbSet
                .Where(otp => otp.UserId == userId && !otp.IsUsed)
                .ToListAsync();

            foreach (var otp in otps)
            {
                otp.IsUsed = true;
                otp.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsValidOTPAsync(int userId, string code)
        {
            return await _dbSet.AnyAsync(otp => 
                otp.UserId == userId && 
                otp.Code == code && 
                otp.ExpiresAt > DateTime.UtcNow && 
                !otp.IsUsed);
        }

        public async Task<PagedResult<OTP>> GetByUserIdPagedAsync(int userId, int pageNumber, int pageSize)
        {
            var totalCount = await _dbSet.CountAsync(otp => otp.UserId == userId);
            var items = await _dbSet
                .Where(otp => otp.UserId == userId)
                .OrderByDescending(otp => otp.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PagedResult<OTP>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<OTP?> GetActiveOTPAsync(int userId, string purpose)
        {
            return await _dbSet
                .FirstOrDefaultAsync(otp => 
                    otp.UserId == userId && 
                    otp.Purpose == purpose && 
                    otp.ExpiresAt > DateTime.UtcNow && 
                    !otp.IsUsed);
        }

        public async Task<OTP?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(otp => otp.Code == code);
        }

        public async Task<OTP?> GetRecentOTPAsync(int userId, string purpose, TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(timeSpan);
            return await _dbSet
                .Where(otp => 
                    otp.UserId == userId && 
                    otp.Purpose == purpose && 
                    otp.CreatedAt >= cutoffTime)
                .OrderByDescending(otp => otp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> DeleteExpiredOTPsAsync()
        {
            var expiredOTPs = await _dbSet
                .Where(otp => otp.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            var count = expiredOTPs.Count;
            _dbSet.RemoveRange(expiredOTPs);
            return count;
        }
    }
}
