using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/asset/product")]
    [Authorize(Roles = "Admin")]
    public class ProductAssetController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        public ProductAssetController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        //Upload image for product
        [HttpPost("upload/{productId}")]
        public async Task<IActionResult> UploadImage(Guid productId, [FromForm]IFormFile file)
        {
            var result = await _cloudinaryService.UploadImageAsync(productId, file);
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
        //Update image for product
        [HttpPut("{assetId}")]
        public async Task<IActionResult> UpdateImage(Guid assetId, [FromForm]IFormFile file)
        {
            var result = await _cloudinaryService.UpdateImageAsync(assetId, file);
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
        //Delete image for product
        [HttpDelete("{assetId}")]
        public async Task<IActionResult> DeleteImage(Guid assetId)
        {
            var result = await _cloudinaryService.DeleteImageAsync(assetId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(new { Message = "Image deleted successfully." });
        }
    }
}