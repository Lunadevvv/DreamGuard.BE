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
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IServiceOrderService _serviceOrderService;
        private readonly IMapper _mapper;
        public ServiceOrdersController(IServiceOrderService serviceOrderService, IMapper mapper)
        {
            _serviceOrderService = serviceOrderService;
            _mapper = mapper;
        }

        [HttpPost("OrderService")]
        public async Task<IActionResult> OrderServiceAsync([FromBody] ServiceOrderCreateRequest serviceOrderRequest)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            string ipAddress = GetIpAddress();
            var result = await _serviceOrderService.OrderServiceAsync(serviceOrderRequest, customerId, ipAddress);
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
        [HttpPost("ReorderFailedService")]
        public async Task<IActionResult> ReOrderFailedServiceAsync(Guid SoId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            string ipAddress = GetIpAddress();
            var result = await _serviceOrderService.ReOrderServiceAsync(SoId, customerId, ipAddress);
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
        private string GetIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            return ipAddress ?? "127.0.0.1";
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1, int pageSize = 4)
        {
            var result = await _serviceOrderService.GetAllAsync(pageNumber, pageSize);
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

        [HttpPost("AdminSearchOrderService")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AdminSearchOrderServiceAsync([FromQuery]ServiceOrderSearchRequest searchRequest, int pageNumber = 1, int pageSize = 4)
        {
            var result = await _serviceOrderService.GetAllByAdminAsync(pageNumber, pageSize, searchRequest);
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

        [HttpGet("{serviceOrderId}")]
        public async Task<IActionResult> GetByIdAsync(Guid serviceOrderId)
        {
            var result = await _serviceOrderService.GetByIdAsync(serviceOrderId);
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
        [HttpPut("{serviceOrderId}")]
        public async Task<IActionResult> UpdateServiceOrderAsync(Guid serviceOrderId, [FromBody] ServiceOrderUpdateRequest updateRequest)
        {
            var result = await _serviceOrderService.UpdateServiceOrderAsync(serviceOrderId, updateRequest);
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
        [HttpPatch("{serviceOrderId}/cancel")]
        public async Task<IActionResult> CancelPendingServiceOrderAsync(Guid serviceOrderId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _serviceOrderService.CancelPendingServiceOrderAsync(customerId, serviceOrderId);
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
        [HttpPatch("{serviceOrderId}/manager-cancel")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> ManagerCancelConfirmedServiceOrderAsync(Guid serviceOrderId)
        {
            var result = await _serviceOrderService.ManagerCancelConfirmedServiceOrderAsync(serviceOrderId);
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

        [HttpPatch("{serviceOrderId}/manager-force-cancel")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> ManagerCancelProcessingServiceOrderAsync(Guid serviceOrderId)
        {
            var result = await _serviceOrderService.ManagerCancelProcessingServiceOrderAsync(serviceOrderId);
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

        [HttpPatch("{serviceOrderId}/confirm")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> ConfirmPendingServiceOrderAsync(Guid serviceOrderId)
        {
            var result = await _serviceOrderService.ConfirmPendingServiceOrderAsync(serviceOrderId);
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
        [HttpPatch("{serviceOrderId}/reject")]
        [Authorize(Roles = $"{Role.Admin}, {Role.Manager}")]
        public async Task<IActionResult> RejectPendingServiceOrderAsync(Guid serviceOrderId)
        {
            var result = await _serviceOrderService.RejectPendingServiceOrderAsync(serviceOrderId);
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