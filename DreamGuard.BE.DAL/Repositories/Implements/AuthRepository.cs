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
    public class AuthRepository : IAuthRepository
    {
        private readonly DreamGuardContext _context;
        public AuthRepository(DreamGuardContext context)
        {
            _context = context;
        }
        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            return await _context.Users
                .Where(u => u.PhoneNumber == phone && !u.IsRevoked)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Where(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime >= DateTime.UtcNow && !u.IsRevoked)
                .FirstOrDefaultAsync();
        }
    }
}