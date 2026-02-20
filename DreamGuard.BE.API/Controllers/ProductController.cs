using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Services.Interfaces;
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

        //GetAllWithPaging
        [HttpGet("category/{cateId}/page/{pageNumber}")]
        public async Task<IActionResult> GetAllProductByCategoryAsync(int cateId, int pageNumber = 1)
        {
            var result = await _productService.GetAllProductByCategoryAsync(cateId, pageNumber);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Message);
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
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok(result.Data);
        }
        //GetBySlug
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetProductDetailBySlugAsync(string slug)
        {
            var result = await _productService.GetProductDetailBySlugAsync(slug);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Message);
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
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok("Create product successfully");
        }
        //Update
        [HttpPut]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateProductAsync([FromBody] Product product)
        {
            var result = await _productService.UpdateProductAsync(product);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok("Update product id " + product.Id + "successfully");
        }
        //Delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteProductAsync(Guid id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Message);
            }
            return Ok("Delete product id " + id + "successfully");
        }
    }
}