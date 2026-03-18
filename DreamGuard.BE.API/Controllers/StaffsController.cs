using AutoMapper;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffsController : ControllerBase
    {
        private readonly IStaffService _staffService;
        public StaffsController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        [Authorize(Roles = $"{Role.CleaningStaff}, {Role.Manager}, {Role.Seller}")]
        public async Task<IActionResult> GetByIdAsync()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _staffService.GetByIdAsync(userId);
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

        [HttpGet("GetAllAsync")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1, int pageSize = 4)
        {
            var result = await _staffService.GetAllAsync(pageNumber, pageSize);
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

        [HttpGet("{staffId}")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> GetByIdAsync(Guid staffId)
        {
            var result = await _staffService.GetByIdAsync(staffId);
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

        [HttpPut("{staffId}")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> UpdateAsync(Guid staffId, [FromBody] StaffUpdateRequest staffRequest)
        {
            var result = await _staffService.UpdateAsync(staffId, staffRequest);
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

        //Update Role for staff
        [HttpPut("{staffId}/UpdateRole")]
        [Authorize(Roles = $"{Role.Admin}")]
        public async Task<IActionResult> UpdateRoleAsync(Guid staffId, [FromQuery] string newRole)
        {
            var result = await _staffService.UpdateRoleAsync(staffId, newRole);
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

        //Update Staff's account information (email, phone number, password)
        [HttpPut("{staffId}/UpdateAccount")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> UpdateAccountAsync(Guid staffId, [FromBody] StaffAccountUpdateRequest staffRequest)
        {
            var result = await _staffService.UpdateAccountAsync(staffId, staffRequest);
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