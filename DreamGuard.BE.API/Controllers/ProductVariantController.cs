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
    [Route("api/variants")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _productVariantService;

        public ProductVariantController(IProductVariantService productVariantService)
        {
            _productVariantService = productVariantService;
        }

        //Get all variants of a product
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetVariantsByProductIdAsync(Guid productId)
        {
            var result = await _productVariantService.GetVariantsByProductIdAsync(productId);
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

        //Get variant by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVariantByIdAsync(Guid id)
        {
            var result = await _productVariantService.GetVariantByIdAsync(id);
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

        //Create new variant for a product
        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> CreateVariantAsync([FromBody] CreateProductVariantRequest request)
        {
            var result = await _productVariantService.CreateVariantAsync(request);
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

        //Update variant
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateVariantAsync(Guid id, [FromBody] UpdateProductVariantRequest request)
        {
            var result = await _productVariantService.UpdateVariantAsync(id, request);
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

        //Update variant's status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateVariantStatusAsync(Guid id, [FromQuery] bool isActive)
        {
            var result = await _productVariantService.UpdateVariantStatusAsync(id, isActive);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(new { Message = "Variant status updated successfully." });
        }

        //Delete variant
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteVariantAsync(Guid id)
        {
            var result = await _productVariantService.DeleteVariantAsync(id);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(new { Message = "Variant deleted successfully." });
        }
    }
}
