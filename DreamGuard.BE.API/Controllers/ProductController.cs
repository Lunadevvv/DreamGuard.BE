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
        public async Task<IActionResult> GetAllProductByCategoryAsync([FromQuery]int cateId, int pageNumber, double? maxPrice, string? color, int? maxAgeGroup)
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

        //Create
        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> CreateProductAsync([FromBody] Product product)
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
            return Ok("Create product with name " + product.Name + " successfully");
        }
        //Update
        [HttpPut]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateProductAsync([FromBody] Product product)
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
            return Ok("Update product id '" + product.Id + "' successfully");
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
            return Ok(string.Format("Update product id '{0}' status to '{1}' successfully", id, status));
        }
    }
}