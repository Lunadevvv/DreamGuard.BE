using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public InventoryController(IInventoryService inventoryService, IBackgroundJobClient backgroundJobClient)
        {
            _inventoryService = inventoryService;
            _backgroundJobClient = backgroundJobClient;
        }

        //Add stock to inventory
        [HttpPost("add-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddStock([FromBody] UpdateInventoryStockRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _inventoryService.AddInventoryStockAsync(request.ProductVariantId, request.Quantity);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            AuditLog audit = new AuditLog
            {
                UserId = adminId,
                ActionType = $"Admin AddStock",
                Message = $"Admin: {adminId} add stock {request.ProductVariantId} with {request.Quantity} quantity"
            };
            _backgroundJobClient.Enqueue<IAuditLogService>(x => x.LogAsync(audit));
            return Ok(result.Message);
        }

        //Reduce stock from inventory
        [HttpPost("reduce-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReduceStock([FromBody] UpdateInventoryStockRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _inventoryService.ReduceInventoryStockAsync(request.ProductVariantId, request.Quantity);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            AuditLog audit = new AuditLog
            {
                UserId = adminId,
                ActionType = $"Admin reduce stock",
                Message = $"Admin: {adminId} reduce stock {request.ProductVariantId} with {request.Quantity} quantity"
            };
            _backgroundJobClient.Enqueue<IAuditLogService>(x => x.LogAsync(audit));
            return Ok(result.Message);
        }

        //Update inventory details
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInventory([FromBody] Inventory inventory)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _inventoryService.UpdateInventoryAsync(inventory);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            AuditLog audit = new AuditLog
            {
                UserId = adminId,
                ActionType = $"Admin reduce stock",
                Message = $"Admin: {adminId} update inventory: {inventory.Id} with quantity: {inventory.Quantity} and defectQuantity = {inventory.DefectQuantity}"
            };
            _backgroundJobClient.Enqueue<IAuditLogService>(x => x.LogAsync(audit));
            return Ok(result.Message);
        }

        [HttpPost("add-defect-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDefectStock([FromBody] UpdateInventoryStockRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _inventoryService.AddDefectStockAsync(request.ProductVariantId, request.Quantity);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            AuditLog audit = new AuditLog
            {
                UserId = adminId,
                ActionType = $"Admin add defect stock",
                Message = $"Admin: {adminId} add defect stock {request.ProductVariantId} with DefectQuantity: {request.Quantity}"
            };
            _backgroundJobClient.Enqueue<IAuditLogService>(x => x.LogAsync(audit));
            return Ok(result.Message);
        }

        [HttpPost("reduce-defect-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReduceDefectStock([FromBody] UpdateInventoryStockRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _inventoryService.ReduceDefectStockAsync(request.ProductVariantId, request.Quantity);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            AuditLog audit = new AuditLog
            {
                UserId = adminId,
                ActionType = $"Admin reduce defect stock",
                Message = $"Admin: {adminId} reduce defect stock {request.ProductVariantId} with DefectQuantity: {request.Quantity}"
            };
            _backgroundJobClient.Enqueue<IAuditLogService>(x => x.LogAsync(audit));
            return Ok(result.Message);
        }
    }
}