using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class OtpService : IOtpService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IConfiguration _configuration;
        private readonly IBrevoEmailService _brevoEmailService;
        public OtpService(IOtpRepository otpRepository, IAuthRepository authRepository, IConfiguration configuration, IBrevoEmailService brevoEmailService)
        {
            _otpRepository = otpRepository;
            _authRepository = authRepository;
            _configuration = configuration;
            _brevoEmailService = brevoEmailService;
        }
        public async Task CleanupExpiredOtpsAsync()
        {
            await _otpRepository.CleanupExpiredOtpsAsync();
        }

        public async Task<Result> GenerateAndSendOtpAsync(string phoneNumber, string email)
        {
            await CleanupExpiredOtpsAsync();

            //tại sao lại check phone number ở đây ??? phải check ở hàm register chứ 
            //Check if phone number is registered
            //var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            //if (user != null)
            //{
            //    return Result.Failure("Phone number is registered", 400);
            //}

            //Generate OTP code
            var otpCode = new Random().Next(100000, 999999).ToString();
            var otpHash = string.Empty;
            var salt = Array.Empty<byte>();
            Console.WriteLine($"Generated OTP Code: {otpCode}");
            try
            {
                //Hash Otp Code
                using (var hmac = new HMACSHA512())
                {
                    salt = hmac.Key;
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otpCode));
                    otpHash = Convert.ToBase64String(hash);
                }
            }
            catch (Exception)
            {
                return Result.Failure("Hash OTP Service Error", 400);
            }
            
            //Create Otp entity
            var otp = new Otp
            {
                OtpId = Guid.NewGuid(),
                Phone = phoneNumber,
                Email = email,
                CodeHashed = otpHash,
                Salt = salt,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(_configuration["OtpExpiryInMinutes"] != null ? Convert.ToDouble(_configuration["OtpExpiryInMinutes"]) : 5),
                IsUsed = false
            };

            //Save to database
            var result = await _otpRepository.GenerateOtpAsync(otp);
            if (!result)
            {
                return Result.Failure("Failed to generate OTP", 400);
            }

            //Send email with OTP code
            await _brevoEmailService.ActivateEmailAsync(email, otpCode);
            return Result.Success($"Gửi mã OTP về {email} thành công");
        }

        public async Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string email, string code)
        {
            var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            //tại sao lại check phone number ở đây ??? phải check ở hàm register chứ 
            //if (user != null)
            //{
            //    return Result<bool>.Failure("User with this phone number existed", 400);
            //}
            var otp = await _otpRepository.GetOtpByPhoneAsync(phoneNumber);

            if (otp == null)
            {
                return Result<bool>.Failure("OTP not found", 400);
            }
            var otpHash = string.Empty;
            //Hash the provided code
            using (var hmac = new HMACSHA512(otp.Salt))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
                otpHash = Convert.ToBase64String(hash);
            }
            if (otpHash != otp.CodeHashed)
            {
                return Result<bool>.Failure("Invalid OTP code", 400);
            }

            await _otpRepository.VerifiedOtpAsync(otp);
            return Result<bool>.Success(true);
        }
    }
}