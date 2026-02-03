using DreamGuard.BE.API.Dtos;
using DreamGuard.BE.API.Requests;
using DreamGuard.BE.BLL.Services;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        public AuthsController(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _identityService.LoginAsync(loginRequest.Email, loginRequest.Password);
            if(!result.Succeeded)
            {
                return StatusCode(result.StatusCode,new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var result = await _identityService
                .RegisterAsync(registerRequest.Email,
                registerRequest.Password,
                registerRequest.UserName,
                registerRequest.PhoneNumber,
                registerRequest.Gender,
                registerRequest.DateOfBirth);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }
        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest verifyOtpRequest)
        {
            var result = await _identityService.VerifyOtpAsync(verifyOtpRequest.UserId, verifyOtpRequest.OtpCode);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok("Xác thực OTP thành công");
        }
        [HttpPost("ResendOtp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest resendOtpRequest)
        {
            var result = await _identityService.ReSendOtpAsync(resendOtpRequest.UserId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok("Gửi lại mã OTP thành công");
        }
        [Authorize(Roles = $"{Role.Admin},{Role.User}")]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _identityService.LogoutAsync(userId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok("Đăng xuất thành công");
        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var result = await _identityService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }

    }
}
