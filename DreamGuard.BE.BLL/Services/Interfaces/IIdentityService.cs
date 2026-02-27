using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<Result<LoginResponse>> LoginAsync(string phone, string password);
        Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken);
        Task<Result<RegisterResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string phoneNumber, string gender, DateOnly dateOfBirth);
        Task<Result> LogoutAsync(Guid userId);
    }
}
