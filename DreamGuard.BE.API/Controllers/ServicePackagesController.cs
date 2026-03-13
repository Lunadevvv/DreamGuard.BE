using AutoMapper;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Implements;
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
    public class ServicePackagesController : ControllerBase
    {
        private readonly IServicePackageService _servicePackage;
        private readonly IMapper _mapper;
        public ServicePackagesController(IServicePackageService servicePackage, IMapper mapper)
        {
            _servicePackage = servicePackage;
            _mapper = mapper;
        }

        [HttpGet("Admin")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> GetAllByAdminAsync(int pageNumber = 1, bool isActive = true)
        {
            var result = await _servicePackage.GetAllByAdminAsync(pageNumber, isActive);
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

        [HttpGet("{servicePackageId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> GetByIdAsync(Guid servicePackageId)
        {
            var result = await _servicePackage.GetByIdAsync(servicePackageId);
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
        public async Task<IActionResult> CreateAsync([FromForm] ServicePackageCreateRequest servicePackageRequest)
        {
            var result = await _servicePackage.CreateAsync(servicePackageRequest);
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

        [HttpPost("{servicePackageId}/ReplaceImage")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ReplaceImage(Guid servicePackageId, [FromForm] PackageImageUploadRequest files)
        {
            var result = await _servicePackage.ReplaceImagesAsync(servicePackageId, files);
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
        [HttpDelete("{servicePackageId}/DeleteImage")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteImage(Guid servicePackageId)
        {
            var result = await _servicePackage.DeleteImageAsync(servicePackageId);
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

        [HttpPut("{servicePackageId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateAsync(Guid servicePackageId, [FromBody] ServicePackageUpdateRequest servicePackageRequest)
        {
            var result = await _servicePackage.UpdateAsync(servicePackageId, servicePackageRequest);
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

        [HttpDelete("{servicePackageId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ToggleActiveAsync(Guid servicePackageId)
        {
            var result = await _servicePackage.ToggleActiveAsync(servicePackageId);
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
