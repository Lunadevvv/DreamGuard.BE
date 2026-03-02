using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IOtpService
    {
        Task<Result> GenerateRegisterOtpAsync(string phoneNumber, string email);
        Task<Result> GenerateAndSendOtpAsync(string phoneNumber, string email);
        Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string email, string code);
        Task CleanupExpiredOtpsAsync();
    }
}