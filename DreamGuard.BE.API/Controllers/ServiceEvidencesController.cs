using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
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
    [Authorize]
    public class ServiceEvidencesController : ControllerBase
    {
        private readonly IServiceEvidenceService _service;
        public ServiceEvidencesController(IServiceEvidenceService service)
        {
            _service = service;
        }
        [HttpGet("{serviceEvidenceId}")]
        [Authorize(Roles = $"{Role.Admin}, {Role.CleaningStaff}, {Role.Manager}")]
        public async Task<IActionResult> GetByIdAsync(Guid serviceEvidenceId)
        {
            var result = await _service.GetByIdAsync(serviceEvidenceId);
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
        [HttpGet("service-tasks/{serviceTaskId}/service-evidences")]
        [Authorize(Roles = $"{Role.Admin}, {Role.CleaningStaff}, {Role.Manager}")]
        public async Task<IActionResult> GetByServiceTaskIdAsync(Guid serviceTaskId, int pageNumber = 1, int pageSize = 4)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.GetAllAsync(staffId, pageNumber, pageSize, serviceTaskId);
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
        [HttpGet("AdminSearchSe")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> AdminSearchSeAsync([FromQuery]AdminSearchSeRequest adminSearchSeRequest)
        {
            var result = await _service.AdminSearchSeAsync(adminSearchSeRequest.PageNumber, adminSearchSeRequest.PageSize, adminSearchSeRequest);
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
        [Authorize(Roles = $"{Role.Admin}, {Role.CleaningStaff}, {Role.Manager}")]
        public async Task<IActionResult> CreateAsync(ServiceEvidenceCreateRequest createRequest)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.CreateAsync(staffId, createRequest);
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
