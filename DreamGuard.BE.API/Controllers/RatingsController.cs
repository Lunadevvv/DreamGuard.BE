using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }
        [HttpGet("GetAllRatingsByStaffId/{staffId}")]
        public async Task<IActionResult> GetAllRatingsByStaffId(Guid staffId, int pageNumber = 1, int pageSize = 4)
        {
            var result = await _ratingService.GetRatingsByStaffId(staffId, pageNumber, pageSize);
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

        [HttpGet("AdminSearchRatings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRatingsByAdminSearchdAsync([FromQuery] RatingAdminSearchRequest request)
        {
            if (request.CreatedAt.HasValue)
            {
                request.CreatedAt = DateTime.SpecifyKind(request.CreatedAt.Value, DateTimeKind.Utc);
            }
            if (request.UpdatedAt.HasValue)
            {
                request.UpdatedAt = DateTime.SpecifyKind(request.UpdatedAt.Value, DateTimeKind.Utc);
            }
            var result = await _ratingService.GetRatingsByAdminSearchdAsync(request.ServiceOrderId, request.StaffId, request.Score, request.CreatedAt, request.UpdatedAt, request.PageNumber, request.PageSize);
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



        [HttpGet("{ratingId}")]
        public async Task<IActionResult> GetByIdAsync(Guid ratingId)
        {
            var result = await _ratingService.GetRatingByIdAsync(ratingId);
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

        [HttpPost("{serviceOrderId}")]
        public async Task<IActionResult> CreateAsync(Guid serviceOrderId, [FromBody] RatingCreateRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _ratingService.CreateRatingAsync(serviceOrderId, userId, request);
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

        [HttpPut("{ratingId}")]
        public async Task<IActionResult> UpdateAsync(Guid ratingId, [FromBody] RatingUpdateRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _ratingService.UpdateRatingAsync(ratingId, userId, request);
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
