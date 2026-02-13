using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
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

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class IdentityService : IIdentityService
    {
        private readonly IAuthRepository _authRepository;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IBrevoEmailService _brevoEmailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IdentityService(UserManager<User> userManager, IConfiguration config, IBrevoEmailService brevoEmailService, IAuthRepository authRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _config = config;
            _brevoEmailService = brevoEmailService;
            _authRepository = authRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<LoginResponse>> LoginAsync(string phone, string password)
        {
            var user = await _authRepository.GetUserByPhoneAsync(phone);
            if(user == null)
                return Result<LoginResponse>.Failure("User không tồn tại", 404);
            if(await _userManager.CheckPasswordAsync(user, password) && user.EmailConfirmed)
            {
                var role = await _userManager.GetRolesAsync(user);
                var accessToken = GenerateJSONWebToken(user, role[0] ?? Role.User);
                var refreshToken = GenerateRefreshToken();
                var loginResponse = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    RoleName = role[0] ?? Role.User
                };
                user.RefreshToken = loginResponse.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_config["Jwt:RefreshTokenValidityInDays"] != null ? Convert.ToInt32(_config["Jwt:RefreshTokenValidityInDays"]) : 7);
                await _userManager.UpdateAsync(user);
                WriteAuthTokenAsHttpOnlyCookie("AccessToken", accessToken, DateTime.UtcNow.AddMinutes(_config["Jwt:AccessTokenValidityInMinutes"] != null ? Convert.ToDouble(_config["Jwt:AccessTokenValidityInMinutes"]) : 15));
                WriteAuthTokenAsHttpOnlyCookie("RefreshToken", refreshToken, DateTime.UtcNow.AddDays(_config["Jwt:RefreshTokenValidityInDays"] != null ? Convert.ToInt32(_config["Jwt:RefreshTokenValidityInDays"]) : 7));
                return Result<LoginResponse>.Success(loginResponse);
            }
            return Result<LoginResponse>.Failure("Sai tài khoản hoặc mật khẩu", 404); ;
        }

        public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string refreshToken)
        {
            var user = await _authRepository.GetUserByRefreshTokenAsync(refreshToken);
            if (user != null)
            {
                var role = await _userManager.GetRolesAsync(user);
                var accessToken = GenerateJSONWebToken(user, role[0] ?? Role.User);
                var newRefreshToken = GenerateRefreshToken();
                var refreshTokenResponse = new RefreshTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken
                };
                user.RefreshToken = refreshTokenResponse.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_config["Jwt:RefreshTokenValidityInDays"] != null ? Convert.ToInt32(_config["Jwt:RefreshTokenValidityInDays"]) : 7);
                await _userManager.UpdateAsync(user);
                WriteAuthTokenAsHttpOnlyCookie("AccessToken", accessToken, DateTime.UtcNow.AddMinutes(_config["Jwt:AccessTokenValidityInMinutes"] != null ? Convert.ToDouble(_config["Jwt:AccessTokenValidityInMinutes"]) : 15));
                WriteAuthTokenAsHttpOnlyCookie("RefreshToken", refreshToken, DateTime.UtcNow.AddDays(_config["Jwt:RefreshTokenValidityInDays"] != null ? Convert.ToInt32(_config["Jwt:RefreshTokenValidityInDays"]) : 7));
                return Result<RefreshTokenResponse>.Success(refreshTokenResponse);
            }
            return Result<RefreshTokenResponse>.Failure("User không tồn tại", 404);
        }

        public async Task<Result<RegisterResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string phoneNumber, string gender, DateOnly dateOfBirth)
        {
            var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            //kiểm tra số điện thoại đã được đăng ký chưa
            if (user != null)
            {
                return Result<RegisterResponse>.Failure("Số điện thoại đã được đăng ký", 400);
            }
            //đăng ký user
            User newUser = new User
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                UserName = firstName + lastName,
                PhoneNumber = phoneNumber,
                Gender = gender,
                DateOfBirth = dateOfBirth,
            };
            var result = await _userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, Role.User);
                await _brevoEmailService.SendCustomEmailAsync(newUser.Email, "DreamGuard Registered", "Congratulations! Your account has been successfully created.");
                var registerResponse = new RegisterResponse
                {
                    UserId = newUser.Id,
                    Message = "Đăng ký thành công"
                };
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
                    expires: DateTime.UtcNow.AddMinutes(_config["Jwt:AccessTokenValidityInMinutes"] != null ? Convert.ToDouble(_config["Jwt:AccessTokenValidityInMinutes"]) : 15),
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

        public async Task<Result> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User không tồn tại", 404);
            }   
            // 1. Xóa refresh token khỏi database
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            
            // 2. Tăng SecurityStamp để invalidate tất cả token cũ
            await _userManager.UpdateSecurityStampAsync(user);
            
            // 3. Cập nhật user
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result.Failure("Failed to logout user.", 400);
            }

            // 4. Xóa cookies
            DeleteAuthCookie("AccessToken");
            DeleteAuthCookie("RefreshToken");
            return Result.Success("Log out successfully");
        }

        //viết token vào cookie gửi lên client
        public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token,
            DateTime expiration)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // Secure = true chỉ khi production (HTTPS), false khi development (HTTP)
            var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

            //thêm mới cookie hoặc cập nhật cookie nếu đã tồn tại
            httpContext.Response.Cookies.Append(cookieName, token, new CookieOptions
            {
                HttpOnly = true, // ngăn javascript truy cập cookie này
                Expires = expiration,
                IsEssential = true,
                Secure = isProduction, // true cho production, false cho development
                SameSite = SameSiteMode.None,
                Path = "/" // Đảm bảo cookie available cho toàn bộ application
            });
        }

        // Xóa cookie
        public void DeleteAuthCookie(string cookieName)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

            // Xóa cookie với cùng options như lúc tạo
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(-1),
                IsEssential = true
            };

            httpContext.Response.Cookies.Delete(cookieName, cookieOptions);
        }
    }
}
