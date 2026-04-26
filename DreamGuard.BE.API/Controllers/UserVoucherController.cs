using AutoMapper;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.DAL.Constants;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserVoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly IUserVoucherService _userVoucherService;
        public UserVoucherController(IVoucherService VoucherService, IUserVoucherService userVoucherService)
        {
            _voucherService = VoucherService;
            _userVoucherService = userVoucherService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1, bool? isUsed = null)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _userVoucherService.GetAllByUserAsync(userId, pageNumber, isUsed);
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
        //Get all vouchers of user for admin
        [HttpGet("admin")]
        [Authorize(Roles = $"{Role.Admin} + {Role.Seller} + {Role.Manager}")]
        public async Task<IActionResult> GetAllByAdminAsync(Guid userId, int pageNumber = 1, bool? isUsed = null)
        {
            var result = await _userVoucherService.GetAllByUserAsync(userId, pageNumber, isUsed);
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
        // [HttpGet("{voucherId}")]
        // public async Task<IActionResult> GetByIdAsync(Guid voucherId)
        // {
        //     if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        //     {
        //         return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
        //     }
        //     var result = await _voucherService.GetByIdAsync(userId, voucherId);
        //     if (!result.Succeeded)
        //     {
        //         return StatusCode(result.StatusCode, new ErrorResponse
        //         {
        //             ErrorCode = result.StatusCode,
        //             Message = new List<string> { result.Error }
        //         });
        //     }
        //     return Ok(result.Data);
        // }
        [HttpPost("ClaimVoucher")]
        public async Task<IActionResult> ClaimVoucherAsync([FromBody] VoucherClaimRequest voucherClaimRequest)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _voucherService.ClaimVoucherAsync(userId, voucherClaimRequest.Code);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Message);
        }
    }
}
