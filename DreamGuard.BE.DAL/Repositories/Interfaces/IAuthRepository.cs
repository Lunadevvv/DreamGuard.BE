using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByPhoneAsync(string phone);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    }
}