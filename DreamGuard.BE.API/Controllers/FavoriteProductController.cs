using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteProductController : ControllerBase
    {
        private readonly IFavoriteProductService _favoriteProductService;

        public FavoriteProductController(IFavoriteProductService favoriteProductService)
        {
            _favoriteProductService = favoriteProductService;
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToFavorite(Guid productId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _favoriteProductService.AddToFavoriteAsync(userId, productId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }

        [HttpPost("combo/{comboId}")]
        public async Task<IActionResult> AddComboToFavorite(Guid comboId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _favoriteProductService.AddComboToFavoriteAsync(userId, comboId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromFavorite(Guid productId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _favoriteProductService.RemoveFromFavoriteAsync(userId, productId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }

        [HttpDelete("combo/{comboId}")]
        public async Task<IActionResult> RemoveComboFromFavorite(Guid comboId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _favoriteProductService.RemoveComboFromFavoriteAsync(userId, comboId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites([FromQuery] int pageNumber = 1)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _favoriteProductService.GetFavoriteProductsAsync(userId, pageNumber);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Data);
        }
    }
}
