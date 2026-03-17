using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Options;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class OtpService : IOtpService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly OtpOptions _otpOptions;
        private readonly IBrevoEmailService _brevoEmailService;
        public OtpService(IOtpRepository otpRepository, IAuthRepository authRepository, IOptions<OtpOptions> otpOptions, IBrevoEmailService brevoEmailService)
        {
            _otpRepository = otpRepository;
            _authRepository = authRepository;
            _otpOptions = otpOptions.Value;
            _brevoEmailService = brevoEmailService;
        }
        public async Task CleanupExpiredOtpsAsync()
        {
            await _otpRepository.CleanupExpiredOtpsAsync();
        }

        public async Task<Result> GenerateRegisterOtpAsync(string phoneNumber, string email)
        {
            await CleanupExpiredOtpsAsync();

            //Check if phone number is registered
            var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            if (user != null)
            {
                return Result.Failure("Phone number is registered", 400);
            }

            return await GenerateAndSendOtpAsync(phoneNumber, email);
        }

        public async Task<Result> GenerateAndSendOtpAsync(string phoneNumber, string email)
        {
            await CleanupExpiredOtpsAsync();

            //Check existing OTP for the phone number
            var existingOtp = await _otpRepository.GetOtpByPhoneAsync(phoneNumber);

            if (existingOtp != null)
            {
                if (existingOtp.ExpiredAt > DateTime.UtcNow && !existingOtp.IsUsed)
                {
                    return Result.Failure("An OTP has already been sent to this phone number. Please wait before requesting a new one.", 400);
                }
            }

            //Generate OTP code
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            var otpHash = string.Empty;
            var salt = Array.Empty<byte>();
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
                ExpiredAt = DateTime.UtcNow.AddMinutes(_otpOptions.ExpiryInMinutes > 0 ? _otpOptions.ExpiryInMinutes : 5),
                IsUsed = false
            };

            //Save to database
            var result = await _otpRepository.GenerateOtpAsync(otp);
            if (!result)
            {
                return Result.Failure("Failed to generate OTP", 400);
            }

            //Send email with OTP code
            var emailContent = $@"
                    <html>
                    <body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f7fa;"">
                        <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #f5f7fa;"">
                            <tr>
                                <td style=""padding: 40px 20px;"">
                                    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);"">
                                        <tr>
                                            <td style=""padding: 40px 40px 30px; text-align: center; background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); border-radius: 12px 12px 0 0;"">
                                                <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;"">Mã xác thực của bạn</h1>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 40px;"">
                                                <p style=""margin: 0 0 24px; color: #1e293b; font-size: 16px; line-height: 1.6;"">
                                                    Xin chào,
                                                </p>
                                                <p style=""margin: 0 0 32px; color: #475569; font-size: 15px; line-height: 1.6;"">
                                                    Bạn đã yêu cầu mã xác thực OTP. Vui lòng sử dụng mã dưới đây để hoàn tất xác thực:
                                                </p>
                                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                                                    <tr>
                                                        <td style=""padding: 30px; background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%); border-radius: 12px; text-align: center; border: 2px solid #3b82f6;"">
                                                            <div style=""font-size: 42px; font-weight: 700; letter-spacing: 8px; color: #1e40af; font-family: 'Courier New', monospace;"">
                                                                {otpCode}
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <p style=""margin: 32px 0 24px; color: #475569; font-size: 15px; line-height: 1.6;"">
                                                    Mã này sẽ <strong style=""color: #1e293b;"">hết hạn sau 5 phút</strong> kể từ khi được gửi.
                                                </p>
                                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""margin-top: 32px;"">
                                                    <tr>
                                                        <td style=""padding: 20px; background-color: #fef3c7; border-left: 4px solid #f59e0b; border-radius: 8px;"">
                                                            <p style=""margin: 0; color: #92400e; font-size: 14px; line-height: 1.5;"">
                                                                <strong>⚠️ Lưu ý bảo mật:</strong> Không chia sẻ mã này với bất kỳ ai. Chúng tôi sẽ không bao giờ yêu cầu mã OTP qua điện thoại hoặc email.
                                                            </p>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <p style=""margin: 32px 0 0; color: #64748b; font-size: 14px; line-height: 1.6;"">
                                                    Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi nếu bạn có bất kỳ thắc mắc nào.
                                                </p>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 30px 40px; background-color: #f8fafc; border-radius: 0 0 12px 12px; border-top: 1px solid #e2e8f0;"">
                                                <p style=""margin: 0 0 8px; color: #64748b; font-size: 13px; line-height: 1.5; text-align: center;"">
                                                    Email này được gửi từ <strong style=""color: #1e293b;"">DreamGuard</strong>
                                                </p>
                                                <p style=""margin: 0; color: #94a3b8; font-size: 12px; line-height: 1.5; text-align: center;"">
                                                    © 2026 DreamGuard. All rights reserved.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>";
            await _brevoEmailService.SendEmailAsync(
                email,
                "DreamGuard OTP Code",
                emailContent
            );
            
            return Result.Success($"OTP is sent to {email} successfully");
        }

        public async Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string email, string code)
        {
            // var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            
            // if (user != null)
            // {
            //     return Result<bool>.Failure("User with this phone number existed", 400);
            // }

            var otp = await _otpRepository.GetOtpByPhoneAsync(phoneNumber);

            if (otp == null)
            {
                return Result<bool>.Failure("OTP not found or expired!", 400);
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