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

    public class ComboController : ControllerBase
    {
        private readonly IComboService _comboService;

        public ComboController(IComboService comboService)
        {
            _comboService = comboService;
        }

        //Get all combos with paging and filtering (User)  
        [HttpGet]
        public async Task<IActionResult> GetAllCombosAsync(
            [FromQuery] int pageNumber,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? maxAgeGroup,
            [FromQuery] string? color)
        {
            var result = await _comboService.GetAllCombosAsync(
                pageNumber, maxPrice, maxAgeGroup, color);
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

        //Get all combos with paging (Admin)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> GetAllCombosForAdminAsync([FromQuery] int pageNumber, [FromQuery] string? name, [FromQuery] ProductStatus? status)
        {
            var result = await _comboService.GetAllCombosForAdminAsync(pageNumber, name, status);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetComboByIdAsync(Guid id, [FromQuery]string? size, [FromQuery]string? color)
        {
            var result = await _comboService.GetComboByIdAsync(id, size, color);
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

        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> CreateComboAsync([FromBody] CreateComboRequest request)
        {
            var result = await _comboService.CreateComboAsync(request);
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateComboInfoAsync(
            Guid id, [FromBody] UpdateComboInfoRequest request)
        {
            var result = await _comboService.UpdateComboInfoAsync(id, request);
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

        [HttpPut("{id}/products")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateComboProductsAsync(
            Guid id, [FromBody] UpdateComboProductsRequest request)
        {
            var result = await _comboService.UpdateComboProductsAsync(id, request);
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdateComboStatusAsync(Guid id, ProductStatus status)
        {
            var result = await _comboService.UpdateComboStatusAsync(id, status);
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
