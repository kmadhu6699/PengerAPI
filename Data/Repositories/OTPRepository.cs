using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task CleanupExpiredOTPsAsync()
        {
            var expiredOTPs = await _dbSet
                .Where(otp => otp.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _dbSet.RemoveRange(expiredOTPs);
            await _context.SaveChangesAsync();
        }
    }
}
