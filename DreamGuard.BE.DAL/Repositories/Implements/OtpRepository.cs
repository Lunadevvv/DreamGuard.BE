using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.DbContext;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.DAL.Repositories.Implements
{
    public class OtpRepository : IOtpRepository
    {
        private readonly DreamGuardContext _context;
        public OtpRepository(DreamGuardContext context)
        {
            _context = context;
        }
        public async Task CleanupExpiredOtpsAsync()
        {
            var expiredOtps = await _context.Otps
                .Where(o => o.ExpiredAt <= DateTime.UtcNow || o.IsUsed)
                .ToListAsync();

            _context.Otps.RemoveRange(expiredOtps);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GenerateOtpAsync(Otp otp)
        {
            try
            {
                await _context.Otps.AddAsync(otp);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }catch(Exception)
            {
                return false;
            }
        }

        public async Task<Otp?> GetOtpByPhoneAsync(string phoneNumber)
        {
            var otp = await _context.Otps
                .Where(o => o.Phone == phoneNumber && !o.IsUsed && o.ExpiredAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
                
            return otp;
        }

        public async Task VerifiedOtpAsync(Otp otp)
        {
            otp.IsUsed = true;
            _context.Otps.Update(otp);
            await _context.SaveChangesAsync();
        }
    }
}