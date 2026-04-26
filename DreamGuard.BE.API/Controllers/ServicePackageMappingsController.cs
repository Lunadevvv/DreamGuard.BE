using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicePackageMappingsController : ControllerBase
    {
        private readonly IServicePackageMappingService _service;
        public ServicePackageMappingsController(IServicePackageMappingService service)
        {
            _service = service;
        }
        [HttpGet]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1, int pageSize = 4)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize);
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
        [HttpGet("{servicePackageMappingId}")]
        public async Task<IActionResult> GetByIdAsync(Guid servicePackageMappingId)
        {
            var result = await _service.GetByIdAsync(servicePackageMappingId);
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

        [HttpGet("{productTypeId}/ProductType/{servicePackageId}/ServicePackage")]
        public async Task<IActionResult> GetByIdAsync(Guid productTypeId, Guid servicePackageId)
        {
            var result = await _service.GetByProductTypeIdAndServicePackageIdAsync(productTypeId, servicePackageId);
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

        [HttpPut("{servicePackageMappingId}")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> UpdateByIdAsync(Guid servicePackageMappingId, ServicePackageMappingUpdateRequest request)
        {
            var result = await _service.UpdateByIdAsync(servicePackageMappingId, request);
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
