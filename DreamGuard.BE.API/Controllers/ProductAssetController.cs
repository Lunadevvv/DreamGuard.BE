using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Requests;
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
        public async Task<IActionResult> UploadImage(Guid productId, [FromForm]UploadProductImageRequest request)
        {
            var result = await _cloudinaryService.UploadImageAsync(productId, request.File);
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

        //Upload image for product without save to database
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImageWithoutSaveDb([FromForm] UploadProductImageRequest request)
        {
            var result = await _cloudinaryService.UploadImageWithoutSaveDbAsync(request.File);
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
        public async Task<IActionResult> UpdateImage(Guid assetId, [FromForm]UploadProductImageRequest request)
        {
            var result = await _cloudinaryService.UpdateImageAsync(assetId, request.File);
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
            return Ok(result.Message);
        }
    }
}