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
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.BLL.Common;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Role.Admin)]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly IMapper _mapper;
        public VoucherController(IVoucherService VoucherService, IMapper mapper)
        {
            _voucherService = VoucherService;
            _mapper = mapper;
        }
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllByAdminAsync(int pageNumber = 1)
        {
            var result = await _voucherService.GetAllByAdminAsync(pageNumber);
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllForUserAsync(int pageNumber = 1)
        {
            Result<PaginatedList<VoucherResponse>> result;
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                result = await _voucherService.GetAllByAdminAsync(pageNumber);
            }
            else
            {
                result = await _voucherService.GetAllForUserAsync(userId, pageNumber);
            }
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
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdAsync(Guid voucherId)
        {
            var result = await _voucherService.GetByIdAsync(voucherId);
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
     
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] VoucherCreateRequest VoucherRequest)
        {
            var voucher = _mapper.Map<Voucher>(VoucherRequest);
            var result = await _voucherService.CreateAsync(voucher);
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

		[HttpPut("{voucherId}")]
        public async Task<IActionResult> UpdateAsync(Guid voucherId, [FromBody] VoucherUpdateRequest VoucherRequest)
        {
            var result = await _voucherService.UpdateAsync(voucherId, VoucherRequest);
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

        [HttpDelete("{voucherId}")]
        public async Task<IActionResult> ToggleActiveAsync(Guid voucherId)
        {
            var result = await _voucherService.ToggleActiveAsync(voucherId);
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
