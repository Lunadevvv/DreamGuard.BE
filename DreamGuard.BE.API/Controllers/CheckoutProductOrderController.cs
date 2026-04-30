using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
