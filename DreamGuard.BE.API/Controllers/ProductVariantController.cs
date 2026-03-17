using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
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

        //Get all variants of a product (User can filter by size and color)
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetVariantsByProductIdAsync(Guid productId, [FromQuery]string? size, [FromQuery]string? color)
        {
            var result = await _productVariantService.GetVariantsByProductIdAsync(productId, size, color);
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

        //Get all variants of a product for admin (No filter)
        [HttpGet("admin/product/{productId}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> GetVariantsByProductIdForAdminAsync(Guid productId)
        {
            var result = await _productVariantService.GetVariantsByProductIdForAdminAsync(productId);
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
        public async Task<IActionResult> UpdateVariantStatusAsync(Guid id, [FromQuery] ProductStatus status)
        {
            var result = await _productVariantService.UpdateVariantStatusAsync(id, status);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok($"Updated variant's status to '{status}' successfully!");
        }
    }
}
