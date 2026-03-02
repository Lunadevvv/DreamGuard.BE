using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });

            var result = await _orderService.CheckoutAsync(userId, request);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new ErrorResponse { ErrorCode = result.StatusCode, Message = new List<string> { result.Error! } });

            return Ok(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders(int pageNumber = 1, OrderStatus? status = null)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });

            var result = await _orderService.GetMyOrdersAsync(userId, pageNumber, status);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new ErrorResponse { ErrorCode = result.StatusCode, Message = new List<string> { result.Error! } });

            return Ok(result.Data);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });

            var result = await _orderService.GetOrderByIdAsync(userId, orderId);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new ErrorResponse { ErrorCode = result.StatusCode, Message = new List<string> { result.Error! } });

            return Ok(result.Data);
        }

        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });

            var result = await _orderService.CancelOrderAsync(userId, orderId);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new ErrorResponse { ErrorCode = result.StatusCode, Message = new List<string> { result.Error! } });

            return Ok(result.Message);
        }

        // ======= ADMIN ENDPOINTS =======

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders(int pageNumber = 1, OrderStatus? status = null)
        {
            var result = await _orderService.GetAllOrdersAsync(pageNumber, status);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new ErrorResponse { ErrorCode = result.StatusCode, Message = new List<string> { result.Error! } });

            return Ok(result.Data);
        }

        [HttpGet("admin/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderByIdForAdmin(Guid orderId)
        {
            var result = await _orderService.GetOrderByIdForAdminAsync(orderId);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new ErrorResponse { ErrorCode = result.StatusCode, Message = new List<string> { result.Error! } });

            return Ok(result.Data);
        }

        [HttpPut("admin/{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderId, request);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new ErrorResponse { ErrorCode = result.StatusCode, Message = new List<string> { result.Error! } });

            return Ok(result.Message);
        }
    }
}
