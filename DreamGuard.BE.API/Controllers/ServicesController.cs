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
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly IMapper _mapper;
        public ServicesController(IServiceService serviceService, IMapper mapper)
        {
            _serviceService = serviceService;
            _mapper = mapper;
        }

        [HttpGet("{serviceId}/service-package")]
        public async Task<IActionResult> GetPackageByServiceIdAsync(Guid serviceId)
        {
            var result = await _serviceService.GetPackagesByServiceIdAsync(serviceId);
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
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1)
        {
            var result = await _serviceService.GetAllAsync(pageNumber);
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

        [HttpGet("Admin")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> GetAllByAdminAsync(int pageNumber = 1, bool isActive = true)
        {
            var result = await _serviceService.GetAllByAdminAsync(pageNumber, isActive);
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

        [HttpGet("{serviceId}")]
        public async Task<IActionResult> GetByIdAsync(Guid serviceId)
        {
            var result = await _serviceService.GetByIdAsync(serviceId);
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
        public async Task<IActionResult> CreateAsync([FromForm] ServiceCreateRequest serviceRequest)
        {
            var result = await _serviceService.CreateAsync(serviceRequest);
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
        [HttpPost("{serviceId}/assets")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AddImages(Guid serviceId, [FromForm] ImageUploadRequest files)
        {
            var result = await _serviceService.AddImagesAsync(serviceId, files);
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
        [HttpDelete("{serviceId}/assets/{assetId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteImage(Guid serviceId, Guid assetId)
        {
            var result = await _serviceService.DeleteImageAsync(serviceId, assetId);
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

        [HttpPut("{serviceId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateAsync(Guid serviceId, [FromBody] ServiceUpdateRequest serviceRequest)
        {
            var result = await _serviceService.UpdateAsync(serviceId, serviceRequest);
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

        [HttpDelete("{serviceId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ToggleActiveAsync(Guid serviceId)
        {
            var result = await _serviceService.ToggleActiveAsync(serviceId);
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

        [HttpPost("{serviceId}/packages")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AssignPackages(Guid serviceId, AssignServicePackagesRequest request)
        {
            var result = await _serviceService.AssignPackagesAsync(serviceId, request);
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
        [HttpDelete("{serviceId}/packages")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> RemovePackages(Guid serviceId, RemoveServicePackagesRequest request)
        {
            var result = await _serviceService.RemovePackagesAsync(serviceId, request);
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
