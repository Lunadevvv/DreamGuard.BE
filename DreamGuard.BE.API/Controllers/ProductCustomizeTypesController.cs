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
    [Route("api/customize-types")]
    [Authorize(Roles = Role.Admin + "," + Role.Manager)]
    public class ProductCustomizeTypesController : ControllerBase
    {
        private readonly IProductCustomizeTypeService _productCustomizeTypeService;

        public ProductCustomizeTypesController(IProductCustomizeTypeService productCustomizeTypeService)
        {
            _productCustomizeTypeService = productCustomizeTypeService;
        }

        //get all customize types of product
        [HttpPost("get-all")]
        public async Task<IActionResult> GetAllCustomizeTypes([FromBody] GetCustomizeTypeRequest request)
        {
            var result = await _productCustomizeTypeService.GetProductCustomizeTypesAsync(request.PageNumber, request.PageSize, request.ExceedProductCustomizeIds);
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

        //get customize types by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomizeTypeById(Guid id)
        {
            var result = await _productCustomizeTypeService.GetProductCustomizeTypeByIdAsync(id);
            if (!result.Succeeded)            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Data);
        }

        //create new customize type
        [HttpPost]
        public async Task<IActionResult> CreateCustomizeType([FromBody] ProductCustomizeType customizeType)
        {
            var result = await _productCustomizeTypeService.CreateProductCustomizeTypeAsync(customizeType);
            if (!result.Succeeded)            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            return Ok(result.Message);
        }

        //update customize type
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomizeType(Guid id, [FromBody] ProductCustomizeType customizeType)
        {
            var result = await _productCustomizeTypeService.UpdateProductCustomizeTypeAsync(id, customizeType);
            if (!result.Succeeded)            {
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