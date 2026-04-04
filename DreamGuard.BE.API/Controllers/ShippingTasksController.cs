using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingTasksController : ControllerBase
    {
        private readonly IShippingTaskService _shippingTaskService;

        public ShippingTasksController(IShippingTaskService shippingTaskService)
        {
            _shippingTaskService = shippingTaskService;
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
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.DeliveryStaff)]
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
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.DeliveryStaff)]
        public async Task<IActionResult> GetTasks([FromQuery] int pageNumber = 1, [FromQuery] string? status = null)
        {
            if (User.IsInRole(Role.Admin) || User.IsInRole(Role.Manager))
            {
                var result = await _shippingTaskService.GetAllTasksForAdminAsync(pageNumber, status);
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

        [HttpPut("{id}/returned")]
        [Authorize(Roles = Role.DeliveryStaff)]
        public async Task<IActionResult> FailShipping(Guid id, [FromBody] FailShippingRequest request)
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

        [HttpPost("{id}/process-returned")]
        [Authorize(Roles = Role.Admin + "," + Role.Manager + "," + Role.Seller)]
        public async Task<IActionResult> ProcessReturnedOrder(Guid id, [FromBody] ProcessReturnedRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _shippingTaskService.ProcessReturnedOrderAsync(id, request);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Error);
            }
            return Ok();
        }
    }
}
