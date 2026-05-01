using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/checkout-product-order")]
    [Authorize]
    public class CheckoutProductOrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public CheckoutProductOrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin, Manager, Seller")]
        public async Task<IActionResult> GetAllCheckoutOrdersForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] CheckoutOrderStatus? status = null,
            [FromQuery] string? orderCode = null)
        {
            var result = await _orderService.GetAllCheckoutOrdersForAdminAsync(pageNumber, status, orderCode);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllCheckoutOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] CheckoutOrderStatus? status = null,
            [FromQuery] string? orderCode = null)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _orderService.GetAllUserCheckoutOrdersAsync(pageNumber, status, orderCode, userId);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Data);
        }

        [HttpPatch("admin/{checkoutOrderId}/status")]
        [Authorize(Roles = "Admin, Manager, Seller")]
        public async Task<IActionResult> UpdateCheckoutOrderStatus(Guid checkoutOrderId, [FromBody] CheckoutOrderStatus newStatus)
        {
            var result = await _orderService.UpdateCheckoutOrderStatusAsync(checkoutOrderId, newStatus);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(new { Message = "Status updated successfully" });
        }
    }
}
