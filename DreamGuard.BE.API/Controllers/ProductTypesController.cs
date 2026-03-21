using AutoMapper;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypesController : ControllerBase
    {
        private readonly IProductTypeService _productTypeService;
        private readonly IMapper _mapper;
        public ProductTypesController(IProductTypeService productTypeService, IMapper mapper)
        {
            _productTypeService = productTypeService;
            _mapper = mapper;
        }

        [HttpGet("{productTypeId}/service-package-mapping")]
        public async Task<IActionResult> GetPackageMappingsByProductTypeIdAsync(Guid productTypeId)
        {
            var result = await _productTypeService.GetMappingsByProductTypeIdAsync(productTypeId);
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

        //ko cần thì xóa, để tạm đây mai mốt có gì xài thì lấy ra dùng :v
        //[HttpGet("/service-package")]
        //public async Task<IActionResult> GetPackageByProductTypeIdsAsync([FromQuery]List<Guid> productTypeIds)
        //{
        //    var result = await _productTypeService.GetPackagesByProductTypeIdsAsync(productTypeIds);
        //    if (!result.Succeeded)
        //    {
        //        return StatusCode(result.StatusCode, new ErrorResponse
        //        {
        //            ErrorCode = result.StatusCode,
        //            Message = new List<string> { result.Error }
        //        });
        //    }
        //    return Ok(result.Data);
        //}

        [HttpGet("{productTypeId}/service-package")]
        public async Task<IActionResult> GetAllPackageByProductTypeIdAsync(Guid productTypeId)
        {
            var result = await _productTypeService.GetAllPackageByProductTypeIdAsync(productTypeId);
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

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1, int pageSize = 4)
        {
            var result = await _productTypeService.GetAllAsync(pageNumber, pageSize);
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

        [HttpGet("AdminSearchProductType")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> SearchProductTypeByAdminAsync([FromQuery]SearchProductTypeByAdminRequest searchRequest)
        {
            var result = await _productTypeService.GetAllByAdminAsync(searchRequest.pageNumber, searchRequest.pageSize, searchRequest.isActive);
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

        [HttpGet("{productTypeId}")]
        public async Task<IActionResult> GetByIdAsync(Guid productTypeId)
        {
            var result = await _productTypeService.GetByIdAsync(productTypeId);
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
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> CreateAsync([FromBody] ProductTypeCreateRequest serviceRequest)
        {
            var result = await _productTypeService.CreateAsync(serviceRequest);
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

        [HttpPut("{productTypeId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateAsync(Guid productTypeId, [FromBody] ProductTypeUpdateRequest serviceRequest)
        {
            var result = await _productTypeService.UpdateAsync(productTypeId, serviceRequest);
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

        [HttpDelete("{productTypeId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ToggleActiveAsync(Guid productTypeId)
        {
            var result = await _productTypeService.ToggleActiveAsync(productTypeId);
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

        [HttpPost("{productTypeId}/packages")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AssignPackages(Guid productTypeId, AssignServicePackagesRequest request)
        {
            var result = await _productTypeService.AssignPackagesAsync(productTypeId, request);
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
        [HttpDelete("{productTypeId}/packages")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> RemovePackages(Guid productTypeId, RemoveServicePackagesRequest request)
        {
            var result = await _productTypeService.RemovePackagesAsync(productTypeId, request);
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
