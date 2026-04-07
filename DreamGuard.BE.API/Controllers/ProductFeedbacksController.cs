using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductFeedbacksController : ControllerBase
    {
        private readonly IProductFeedbackService _feedbackService;

        public ProductFeedbacksController(IProductFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Customer creates feedback for a product in a completed order via order item.
        /// </summary>
        [HttpPost("{orderItemId}")]
        [Authorize]
        public async Task<IActionResult> CreateFeedbackAsync(
            Guid orderItemId, [FromBody] ProductFeedbackCreateRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse
                {
                    ErrorCode = 401,
                    Message = new List<string> { "Invalid user token." }
                });
            }

            var result = await _feedbackService.CreateFeedbackAsync(orderItemId, userId, request);
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

        /// <summary>
        /// Get all visible feedbacks for a product with optional rating filter and paging.
        /// </summary>
        [HttpGet("products/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeedbacksByProductIdAsync(
            Guid productId,
            [FromQuery] int? rating,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _feedbackService.GetFeedbacksByProductIdAsync(
                productId, rating, pageNumber, pageSize);

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

        /// <summary>
        /// Admin hides or shows a feedback by updating its status.
        /// </summary>
        [HttpPut("{feedbackId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFeedbackStatusAsync(
            Guid feedbackId, [FromQuery] string status)
        {
            var result = await _feedbackService.UpdateFeedbackStatusAsync(feedbackId, status);
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
