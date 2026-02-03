using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services
{
    public interface IIdentityService
    {
        Task<Result<LoginResponse>> LoginAsync(string email, string password);
        Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken);
        Task<Result<RegisterResponse>> RegisterAsync(string email, string password, string userName, string phoneNumber, string gender, DateOnly dateOfBirth);
        Task<Result> VerifyOtpAsync(string userId, string otpCode);
        Task<Result> ReSendOtpAsync(string userId);
        Task<Result> LogoutAsync(string userId);
    }
}
