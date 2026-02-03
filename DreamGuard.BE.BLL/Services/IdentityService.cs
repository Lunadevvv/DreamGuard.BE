using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IBrevoEmailService _brevoEmailService;
        private readonly Random _random = new();
        public IdentityService(UserManager<User> userManager, IConfiguration config, IBrevoEmailService brevoEmailService)
        {
            _userManager = userManager;
            _config = config;
            _brevoEmailService = brevoEmailService;
        }
        public async Task<Result<LoginResponse>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if(user == null)
                return Result<LoginResponse>.Failure("User không tồn tại", 404);
            if(await _userManager.CheckPasswordAsync(user, password) && user.EmailConfirmed)
            {
                var role = await _userManager.GetRolesAsync(user);
                var loginResponse = new LoginResponse
                {
                    AccessToken = GenerateJSONWebToken(user, role[0]),
                    RefreshToken = GenerateRefreshToken(),
                    RoleName = role[0]
                };
                user.RefreshToken = loginResponse.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);
                return Result<LoginResponse>.Success(loginResponse);
            }
            return Result<LoginResponse>.Failure("Sai tài khoản hoặc mật khẩu", 404); ;
        }

        public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime >= DateTime.UtcNow);
            if (user != null)
            {
                var role = await _userManager.GetRolesAsync(user);
                var refreshTokenResponse = new RefreshTokenResponse
                {
                    AccessToken = GenerateJSONWebToken(user, role[0]),
                    RefreshToken = GenerateRefreshToken()
                };
                user.RefreshToken = refreshTokenResponse.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);
                return Result<RefreshTokenResponse>.Success(refreshTokenResponse);
            }
            return Result<RefreshTokenResponse>.Failure("User không tồn tại", 404);
        }

        public async Task<Result<RegisterResponse>> RegisterAsync(string email, string password, string userName, string phoneNumber, string gender, DateOnly dateOfBirth)
        {
            var user = await _userManager.FindByEmailAsync(email);
            //email đã tồn tại và dc kích hoạt
            if (user != null && user.EmailConfirmed == true)
                return Result<RegisterResponse>.Failure("Email đã được sử dụng", 400);
            //đăng ký user
            User newUser = new User
            {
                Email = email,
                UserName = userName,
                PhoneNumber = phoneNumber,
                Gender = gender,
                DateOfBirth = dateOfBirth,
            };
            var result = await _userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, Role.User);
                var registerResponse = new RegisterResponse
                {
                    UserId = newUser.Id,
                    Message = "Vui lòng nhập mã OTP gửi qua email đăng ký để kích hoạt tài khoản"
                };
                //send email confirmation
                var otp = _random.Next(10000, 99999).ToString();
                newUser.OtpCode = otp;
                newUser.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
                newUser.OtpAttempts = 0;
                await _brevoEmailService.ActivateEmailAsync(email, otp);
                await _userManager.UpdateAsync(newUser);
                return Result<RegisterResponse>.Success(registerResponse);
            }
            //chỗ này chỉ lấy lỗi đầu tiên, :v thật ra tính đổi cái result.error sang list mà lười quá huhu T_T
            return Result<RegisterResponse>.Failure(result.Errors.First().Description, 400);            
        }

        private string GenerateJSONWebToken(User account, string roleName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"]
                    , _config["Jwt:Audience"]
                    , new Claim[]
                    {
                    new(ClaimTypes.Name, account.UserName),
                    new(ClaimTypes.NameIdentifier, account.Id),
                    new(ClaimTypes.Role, roleName),
                    },
                    expires: DateTime.UtcNow.AddMinutes(120),
                    signingCredentials: credentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public async Task<Result> ReSendOtpAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User không tồn tại", 404);
            }

            var otp = _random.Next(10000, 99999).ToString();
            user.OtpCode = otp;
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
            user.OtpAttempts = 0;
            await _brevoEmailService.ActivateEmailAsync(user.Email, otp);
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }
        //verify OTP max 3 lần
        public async Task<Result> VerifyOtpAsync(string userId, string otpCode)
        {
            var user = await _userManager.FindByIdAsync(userId);
            // user không tồn tại
            if (user == null)
                return Result.Failure("User không tồn tại", 404);
            if (user.OtpExpiryTime == null || user.OtpExpiryTime < DateTime.UtcNow)
            {
                return Result.Failure("OTP không tồn tại hoặc đã hết hạn. Vui lòng nhấn gửi lại OTP", 400);
            }
            // OTP đã sai 3 lần
            const int maxAttempts = 3;
            if (user.OtpAttempts >= maxAttempts)
            {
                user.OtpCode = null;
                user.OtpExpiryTime = null;
                user.OtpAttempts = 0;
                await _userManager.UpdateAsync(user);
                return Result.Failure("Bạn đã nhập hết số lần thử OTP. Vui lòng nhấn gửi lại OTP", 400);
            }
            // mã OTP sai
            if (user.OtpCode != otpCode)
            {
                user.OtpAttempts++;
                await _userManager.UpdateAsync(user);
                return Result.Failure("Sai OTP", 400);
            }
            // OTP đúng
            user.EmailConfirmed = true;
            // xoá OTP (tránh reuse)
            user.OtpCode = null;
            user.OtpExpiryTime = null;
            user.OtpAttempts = 0;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User không tồn tại", 404);
            }   
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }
    }
}
