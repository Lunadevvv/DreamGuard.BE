using DreamGuard.BE.API.Implements;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingTasksController : ControllerBase
    {
        private readonly IShippingTaskService _shippingTaskService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public ShippingTasksController(IShippingTaskService shippingTaskService, IBackgroundJobClient backgroundJobClient)
        {
            _shippingTaskService = shippingTaskService;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin + "," + Role.Manager)]
        public async Task<IActionResult> CreateShippingTask([FromBody] ShippingTaskCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _shippingTaskService.CreateShippingTaskAsync(request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok(result.Data);
        }

        [HttpPut("{id}/reassign")]
        [Authorize(Roles = Role.Admin + "," + Role.Manager)]
        public async Task<IActionResult> ReassignStaff(Guid id, [FromBody] ReassignStaffRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _shippingTaskService.ReassignStaffAsync(id, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }

            return Ok();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.DeliveryStaff + "," + Role.Seller)]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var result = await _shippingTaskService.GetTaskByIdAsync(id);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.DeliveryStaff + "," + Role.Seller)]
        public async Task<IActionResult> GetTasks([FromQuery] int pageNumber = 1, [FromQuery] string? status = null, [FromQuery] Guid? orderId = null, [FromQuery] Guid? tradeInOrderId = null)
        {
            if (User.IsInRole(Role.Admin) || User.IsInRole(Role.Manager) || User.IsInRole(Role.Seller))
            {
                var result = await _shippingTaskService.GetAllTasksForAdminAsync(pageNumber, status, orderId, tradeInOrderId);
                return StatusCode(result.StatusCode, result.Data);
            }
            else if (User.IsInRole(Role.DeliveryStaff))
            {
                var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
                {
                    return Unauthorized("Invalid staff token.");
                }

                var result = await _shippingTaskService.GetTasksForStaffAsync(staffId, pageNumber);
                return StatusCode(result.StatusCode, result.Data);
            }
            
            return Forbid();
        }

        [HttpPut("{id}/delivering")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> UpdateToDelivering(Guid id, [FromBody] StartShippingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.UpdateTaskToDeliveringAsync(id, staffId, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }
        [HttpPut("{id}/delivering-for-tradeIn")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> UpdateToDeliveringForTradeIn(Guid id, [FromBody] StartShippingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.UpdateTaskToDeliveringForTradeInAsync(id, staffId, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }

        [HttpPut("{id}/arrived")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> UpdateToArrived(Guid id)
        {
            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.UpdateTaskToArrivedAsync(id, staffId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }

        [HttpPut("{id}/delivered")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> CompleteShipping(Guid id, [FromBody] CompleteShippingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.CompleteShippingAsync(id, staffId, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }

        [HttpPut("{id}/delivered-for-tradeIn")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> CompleteShippingForTradeIn(Guid id, [FromBody] CompleteShippingForTradeInRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.CompleteShippingForTradeInOrderAsync(id, staffId, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }

        [HttpPut("{id}/returned")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> ReturnOrderRequest(Guid id, [FromBody] FailShippingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.FailShippingAsync(id, staffId, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }
        
        [HttpPut("{id}/returned-for-TradeIn")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> FailShippingForTradeIn(Guid id, [FromBody] FailShippingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.FailShippingForTradeInAsync(id, staffId, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }
        
        [HttpPut("{id}/forced-cancelled-TradeIn")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> ForceCancelShipping(Guid id, [FromBody] FailShippingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffIdStr) || !Guid.TryParse(staffIdStr, out var staffId))
            {
                return Unauthorized("Invalid staff token.");
            }

            var result = await _shippingTaskService.ForcedCancelShippingForTradeInAsync(id, staffId, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }

        [HttpPost("{id}/process-returned")]
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.Seller)]
        public async Task<IActionResult> ProcessReturnedOrder(Guid id, [FromBody] ProcessReturnedRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var managerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _shippingTaskService.ProcessReturnedOrderAsync(id, request, managerId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }
        [HttpPost("{id}/process-returned-for-tradeIn")]
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.Seller)]
        public async Task<IActionResult> ProcessReturnedTradeInOrder(Guid id, [FromBody] ProcessReturnedTradeInRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var managerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var role = User.IsInRole(Role.Admin) ? Role.Admin : Role.Manager;
            var result = await _shippingTaskService.ProcessReturnedTradeInOrderAsync(id, request, managerId, role);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }

            return Ok();
        }

        [HttpPost("{id}/process-exchange")]
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.Seller)]
        public async Task<IActionResult> ProcessExchangeOrder(Guid id, [FromBody] ProcessExchangeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var managerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _shippingTaskService.ProcessExchangeOrderAsync(id, request, managerId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok(result.Message);
        }
        [HttpPost("{id}/process-exchange-for-tradeIn")]
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.Seller)]
        public async Task<IActionResult> ProcessExchangeTradeInOrder(Guid id, [FromBody] ProcessExchangeTradeInRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var managerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var role = User.IsInRole(Role.Admin) ? Role.Admin : Role.Manager;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _shippingTaskService.ProcessExchangeTradeInOrderAsync(id, request, managerId, role);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok(result.Message);
        }
    }
}
