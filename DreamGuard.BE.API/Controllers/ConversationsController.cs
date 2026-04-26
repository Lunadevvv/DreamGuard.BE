using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _service;

        public ConversationsController(IConversationService service)
        {
            _service = service;
        }

        [HttpGet("{conversationId}/Messages")]
        [Authorize(Roles = $"{Role.Seller}, {Role.User}")]
        public async Task<IActionResult> GetMessageHistory(Guid conversationId, int pageNumber = 1, int pageSize = 30)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid currentUserID))
            {
                return Unauthorized(new ErrorResponse
                {
                    ErrorCode = StatusCodes.Status401Unauthorized,
                    Message = new List<string> { "Invalid token" }
                });
            }
            var result = await _service.GetMessageHistoryAsync(conversationId, currentUserID, pageNumber, pageSize);
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
        [HttpPatch("{conversationId}/mark-as-read")]
        [Authorize(Roles = $"{Role.Seller}, {Role.User}")]
        public async Task<IActionResult> MarkAsRead(Guid conversationId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid currentUserID))
            {
                return Unauthorized(new ErrorResponse
                {
                    ErrorCode = StatusCodes.Status401Unauthorized,
                    Message = new List<string> { "Invalid token" }
                });
            }
            var result = await _service.MarkAsReadAsync(conversationId, currentUserID);
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

        [HttpGet()]
        [Authorize(Roles = $"{Role.Seller}, {Role.User}")]
        public async Task<IActionResult> GetMyConversation(int pageNumber = 1, int pageSize = 4)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
            {
                return Unauthorized(new ErrorResponse
                {
                    ErrorCode = StatusCodes.Status401Unauthorized,
                    Message = new List<string> { "Invalid token" }
                });
            }
            var role = User.FindFirstValue(ClaimTypes.Role);
            var result = await _service.GetConversationAsync(userId, pageNumber, pageSize);
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
