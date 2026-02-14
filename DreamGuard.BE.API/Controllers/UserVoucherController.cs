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
        private readonly IMapper _mapper;
        public UserVoucherController(IVoucherService VoucherService, IMapper mapper)
        {
            _voucherService = VoucherService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _voucherService.GetAllAsync(userId, pageNumber);
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
        [HttpGet("{voucherId}")]
        public async Task<IActionResult> GetByIdAsync(Guid voucherId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _voucherService.GetByIdAsync(userId, voucherId);
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
        [HttpPost("ClaimVoucher")]
        public async Task<IActionResult> ClaimVoucherAsync([FromBody] VoucherClaimRequest voucherClaimRequest)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _voucherService.ClaimVoucherAsync(userId, voucherClaimRequest.Code);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result);
        }
    }
}
