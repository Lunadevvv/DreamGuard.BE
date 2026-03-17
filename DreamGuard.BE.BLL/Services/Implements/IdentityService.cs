using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Options;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class IdentityService : IIdentityService
    {
        private readonly IAuthRepository _authRepository;
        private readonly UserManager<User> _userManager;
        private readonly IBrevoEmailService _brevoEmailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtOptions _jwtOptions;
        private readonly IOtpService _otpService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IUnitOfWork _unitOfWork;
        public IdentityService(
            UserManager<User> userManager,
            IBrevoEmailService brevoEmailService,
            IAuthRepository authRepository,
            IHttpContextAccessor httpContextAccessor,
            IOptions<JwtOptions> jwtOptions,
            IOtpService otpService,
            ICustomerRepository customerRepository,
            IStaffRepository staffRepository,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _brevoEmailService = brevoEmailService;
            _authRepository = authRepository;
            _httpContextAccessor = httpContextAccessor;
            _jwtOptions = jwtOptions.Value;
            _otpService = otpService;
            _customerRepository = customerRepository;
            _staffRepository = staffRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginResponse>> LoginAsync(string phone, string password)
        {
            var user = await _authRepository.GetUserByPhoneAsync(phone);
            if(user == null)
                return Result<LoginResponse>.Failure("User không tồn tại", 401);
            if(await _userManager.CheckPasswordAsync(user, password))
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
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenValidityInDays > 0 ? _jwtOptions.RefreshTokenValidityInDays : 7);
                await _userManager.UpdateAsync(user);
                WriteAuthTokenAsHttpOnlyCookie("AccessToken", accessToken, DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenValidityInMinutes > 0 ? _jwtOptions.AccessTokenValidityInMinutes : 15));
                WriteAuthTokenAsHttpOnlyCookie("RefreshToken", refreshToken, DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenValidityInDays > 0 ? _jwtOptions.RefreshTokenValidityInDays : 7));
                return Result<LoginResponse>.Success(loginResponse);
            }
            return Result<LoginResponse>.Failure("Sai tài khoản hoặc mật khẩu", 401); ;
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
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenValidityInDays > 0 ? _jwtOptions.RefreshTokenValidityInDays : 7);
                await _userManager.UpdateAsync(user);
                WriteAuthTokenAsHttpOnlyCookie("AccessToken", accessToken, DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenValidityInMinutes > 0 ? _jwtOptions.AccessTokenValidityInMinutes : 15));
                WriteAuthTokenAsHttpOnlyCookie("RefreshToken", newRefreshToken, DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenValidityInDays > 0 ? _jwtOptions.RefreshTokenValidityInDays : 7));
                return Result<RefreshTokenResponse>.Success(refreshTokenResponse);
            }
            return Result<RefreshTokenResponse>.Failure("User không tồn tại", 404);
        }

        public async Task<Result<RegisterResponse>> RegisterAsync(string email, string password, string fullName, string phoneNumber, string gender, DateOnly dateOfBirth)
        {
            // Validate gender
            if (gender != Gender.Male && gender != Gender.Female)
                return Result<RegisterResponse>.Failure("Gender must be 'Male' or 'Female'", 400);

            // Check email uniqueness
            var existingByEmail = await _userManager.FindByEmailAsync(email);
            if (existingByEmail != null)
                return Result<RegisterResponse>.Failure("Email already registered", 400);

            // Check phone uniqueness
            var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            if (user != null)
                return Result<RegisterResponse>.Failure("Phone number already registered", 400);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                User newUser = new User
                {
                    Email = email,
                    UserName = phoneNumber,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                var result = await _userManager.CreateAsync(newUser, password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result<RegisterResponse>.Failure(result.Errors.First().Description, 400);
                }

                await _userManager.AddToRoleAsync(newUser, Role.User);

                var customer = new Customer
                {
                    CustomerId = newUser.Id,
                    FullName = fullName,
                    Gender = gender,
                    DateOfBirth = dateOfBirth,
                    AvatarUrl = string.Empty
                };
                await _customerRepository.CreateAsync(customer);

                await transaction.CommitAsync();

                await _brevoEmailService.SendEmailAsync(newUser.Email, "DreamGuard Registered", "Congratulations! Your account has been successfully created.");
                return Result<RegisterResponse>.Success(new RegisterResponse
                {
                    UserId = newUser.Id,
                    Message = "Registration successful"
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<RegisterResponse>.Failure("Registration failed. Please try again.", 500);
            }
        }

        public async Task<Result<RegisterResponse>> CreateStaffAsync(CreateStaffRequest request)
        {
            // Validate role - only Manager, Seller, CleaningStaff allowed
            var allowedRoles = new[] { Role.Manager, Role.Seller, Role.CleaningStaff };
            if (!allowedRoles.Contains(request.Role))
                return Result<RegisterResponse>.Failure("Role must be Manager, Seller, or CleaningStaff", 400);

            // Validate gender
            if (request.Gender != Gender.Male && request.Gender != Gender.Female)
                return Result<RegisterResponse>.Failure("Gender must be 'Male' or 'Female'", 400);

            // Check phone uniqueness
            var existingByPhone = await _authRepository.GetUserByPhoneAsync(request.PhoneNumber);
            if (existingByPhone != null)
                return Result<RegisterResponse>.Failure("Phone number already registered", 400);

            // Check email uniqueness
            var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingByEmail != null)
                return Result<RegisterResponse>.Failure("Email already registered", 400);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var newUser = new User
                {
                    Email = request.Email,
                    UserName = request.PhoneNumber,
                    PhoneNumber = request.PhoneNumber,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result<RegisterResponse>.Failure(result.Errors.First().Description, 400);
                }

                await _userManager.AddToRoleAsync(newUser, request.Role);

                var staff = new Staff
                {
                    StaffId = newUser.Id,
                    FullName = request.FullName,
                    Gender = request.Gender,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    Position = request.Position,
                    Status = "Active",
                    AvatarUrl = string.Empty
                };
                await _staffRepository.CreateAsync(staff);

                await transaction.CommitAsync();

                return Result<RegisterResponse>.Success(new RegisterResponse
                {
                    UserId = newUser.Id,
                    Message = "Staff account created successfully"
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<RegisterResponse>.Failure("Failed to create staff account. Please try again.", 500);
            }
        }

        private string GenerateJSONWebToken(User account, string roleName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtOptions.Issuer
                    , _jwtOptions.Audience
                    , new Claim[]
                    {
                    new(ClaimTypes.Name, account.UserName),
                    new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new(ClaimTypes.Role, roleName),
                    },
                    expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenValidityInMinutes > 0 ? _jwtOptions.AccessTokenValidityInMinutes : 15),
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

        public async Task<Result> LogoutAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
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
        public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // Secure = true chỉ khi production (HTTPS), false khi development (HTTP)
            var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

            //thêm mới cookie hoặc cập nhật cookie nếu đã tồn tại
            httpContext.Response.Cookies.Append(cookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Expires = expiration,
                IsEssential = true,
                Secure = true, // ✅ LUÔN true — vì SameSite=None yêu cầu Secure
                SameSite = isProduction 
                    ? SameSiteMode.None   // Cross-site (FE và BE khác domain)
                    : SameSiteMode.Lax,   // Dev thì dùng Lax cho an toàn
                Path = "/"
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
                Secure = true, // ✅ LUÔN true
                SameSite = isProduction 
                    ? SameSiteMode.None 
                    : SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(-1),
                IsEssential = true
            };
            httpContext.Response.Cookies.Delete(cookieName, cookieOptions);
        }

        public async Task<Result> ForgotPasswordAsync(string phoneNumber)
        {
            var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            if (user == null)
            {
                return Result.Failure("This phone number doesn't exist", 404);
            }

            var result = await _otpService.GenerateAndSendOtpAsync(phoneNumber, user.Email);
            if (!result.Succeeded)
            {
                return Result.Failure(result.Error, result.StatusCode);
            }

            return Result.Success("OTP has been sent to your phone number");
        }

        public async Task<Result> ResetPasswordAsync(string phoneNumber, string otpCode)
        {
            var user = await _authRepository.GetUserByPhoneAsync(phoneNumber);
            if (user == null)
            {
                return Result.Failure("This phone number doesn't exist", 404);
            }

            // Kiểm tra OTP
            var otpResult = await _otpService.VerifyOtpAsync(phoneNumber, user.Email, otpCode);
            if (!otpResult.Succeeded)
            {
                return Result.Failure("Invalid or expired OTP", 400);
            }

            // OTP hợp lệ, gửi về mail cho người dùng 1 mật khẩu random mới để họ đăng nhập vào app và đổi mật khẩu
            var newPassword = GenerateRandomPassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetResult.Succeeded)
            {
                return Result.Failure("Failed to reset password. Please try again.", 500);
            }

            await _brevoEmailService.SendEmailAsync(user.Email, "Password Reset", $"Your password has been reset. Your new temporary password is: {newPassword}. Please log in and change your password immediately.");

            return Result.Success("Password has been reset and sent to your email");
        }

        private string GenerateRandomPassword()
        {
            const int length = 8;
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            StringBuilder res = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (res.Length < length)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }

            return res.ToString();
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Result.Failure("User doesn't exist", 404);
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                return Result.Failure("Current password is incorrect or new password doesn't meet requirements", 400);
            }

            return Result.Success("Password has been changed successfully");
        }
    }
}
