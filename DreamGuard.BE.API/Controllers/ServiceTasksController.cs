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
    public class ServiceTasksController : ControllerBase
    {
        private readonly IServiceTaskService _service;
        private readonly IMapper _mapper;
        public ServiceTasksController(IServiceTaskService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        [HttpGet("{serviceTaskId}")]
        [Authorize(Roles = $"{Role.Admin}, {Role.CleaningStaff}, {Role.Manager}")]
        public async Task<IActionResult> GetByIdAsync(Guid serviceTaskId)
        {
            if(!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == null) 
            {                 
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user role." } });
            }
            var result = await _service.GetByIdAsync(serviceTaskId, staffId, role);
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

        [HttpGet("AdminSearchServiceTask")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> AdminSearchServiceTask([FromQuery]AdminSearchServiceTaskRequest searchRequest, int pageSize = 4, int pageNumber = 1)
        {
            var result = await _service.SearchAsync(searchRequest, pageNumber, pageSize);
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

        [HttpGet("GetByStaffId")]
        [Authorize(Roles = Role.CleaningStaff)]
        public async Task<IActionResult> GetByStaffId(int pageNumber = 1, int pageSize = 4)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.GetByStaffIdAsync(staffId, pageNumber, pageSize);
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
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> CreateAsync([FromBody] ServiceTaskCreateRequest serviceTaskCreateRequest)
        {
            var result = await _service.CreateAsync(serviceTaskCreateRequest);
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
        [HttpPost("reassign-staff-for-rescheduled-service-order")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> ReassignStaffForRescheduledServiceOrder([FromBody] ReassignStaffForRescheduledOrderRequest request)
        {
            var result = await _service.ReassignStaffForRescheduledOrder(request.ServiceOrderId, request.NewStaffId);
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

        [HttpPatch("{serviceTaskId}/updateCompletedStatus")]
        [Authorize(Roles = $"{Role.CleaningStaff}")]
        public async Task<IActionResult> UpdateCompletedStatusAsync(Guid serviceTaskId, ServiceTaskCompleteRequest request)
        {
            var result = await _service.UpdateCompletedStatusAsync(serviceTaskId, request);
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
        [HttpPatch("{serviceTaskId}/updateCheckedOutStatus")]
        [Authorize(Roles = Role.CleaningStaff)]
        public async Task<IActionResult> UpdateCheckedOutStatusAsync(Guid serviceTaskId, ServiceTaskCheckOutRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.UpdateCheckedOutStatusAsync(serviceTaskId, staffId, request);
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
        [HttpPatch("{serviceTaskId}/updateProcessingStatus")]
        [Authorize(Roles = Role.CleaningStaff)]
        public async Task<IActionResult> UpdateProcessingStatusAsync(Guid serviceTaskId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.UpdateProcessingStatusAsync(serviceTaskId, staffId);
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

        [HttpPatch("{serviceTaskId}/updateForcedCancelledStatus")]
        [Authorize(Roles = Role.CleaningStaff)]
        public async Task<IActionResult> UpdateForcedCancelledStatusAsync(Guid serviceTaskId, ForcedCancelledRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.UpdateForcedCancelledStatusAsync(serviceTaskId, staffId, request.StaffNote);
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

        [HttpPatch("{serviceTaskId}/updateCheckedInStatus")]
        [Authorize(Roles = Role.CleaningStaff)]
        public async Task<IActionResult> UpdateCheckedInStatusAsync(Guid serviceTaskId, ServiceTaskCheckInRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.UpdateCheckedInStatusAsync(serviceTaskId, staffId, request);
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