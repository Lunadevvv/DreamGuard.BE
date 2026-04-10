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
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        //GetAllWithPaging (User)
        [HttpGet]
        public async Task<IActionResult> GetAllProductByCategoryAsync([FromQuery]int cateId, int pageNumber, decimal? maxPrice, string? color, int? maxAgeGroup)
        {
            var result = await _productService.GetAllProductByCategoryAsync(cateId, pageNumber, maxPrice, color, maxAgeGroup);
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

        //GetAllProductToTradeInWithPaging (User)
        [HttpGet("GetAllProductToTradeIn")]
        public async Task<IActionResult> GetAllProductToTradeIn([FromQuery] int? cateId, decimal? maxPrice, string? color, int? maxAgeGroup, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _productService.GetAllProductToTradeInAsync(cateId, pageNumber, pageSize, maxPrice, color, maxAgeGroup);
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

        //GetById
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductByIdAsync(Guid id)
        {
            var result = await _productService.GetProductByIdAsync(id);
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
        //GetBySlug (User)
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetProductDetailBySlugAsync(string slug)
        {
            var result = await _productService.GetProductDetailBySlugAsync(slug);
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

        //Get All With Paging (Admin)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> GetAllProductsForAdminAsync([FromQuery]int pageNumber, string? name)
        {
            var result = await _productService.GetAllProductsForAdminAsync(pageNumber, name);
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

        //Create new Product with fully customizable variant
        [HttpPost("fully-customize")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateFullyCustomizeProductAsync([FromBody] CreateFullyCustomizeProductRequest request)
        {
            var result = await _productService.CreateFullyCustomizeProductAsync(request);
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

        //Get fully customizable products
        [HttpGet("fully-customized")]
        public async Task<IActionResult> GetFullyCustomizedProductsAsync()
        {
            var result = await _productService.GetFullyCustomizedProductsAsync();
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

        //Create
        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductRequest product)
        {
            var result = await _productService.CreateProductAsync(product);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok($"Create product with slug '{product.Slug}' successfully!");
        }
        //Update
        [HttpPut()]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateProductAsync([FromBody] UpdateProductRequest product)
        {
            var result = await _productService.UpdateProductAsync(product);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok($"Updated product '{product.Slug}' information successfully!");
        }
        //Update status
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateProductStatusAsync(Guid id, [FromQuery] ProductStatus status)
        {
            var result = await _productService.UpdateProductStatusAsync(id, status);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok($"Updated successfully!");
        }
    }
}