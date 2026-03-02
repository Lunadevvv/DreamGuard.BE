using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        //Add stock to inventory
        [HttpPost("add-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddStock([FromBody] UpdateInventoryStockRequest request)
        {
            var result = await _inventoryService.AddInventoryStockAsync(request.ProductVariantId, request.Quantity);
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

        //Reduce stock from inventory
        [HttpPost("reduce-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReduceStock([FromBody] UpdateInventoryStockRequest request)
        {
            var result = await _inventoryService.ReduceInventoryStockAsync(request.ProductVariantId, request.Quantity);
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

        //Update inventory details
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInventory([FromBody] Inventory inventory)
        {
            var result = await _inventoryService.UpdateInventoryAsync(inventory);
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