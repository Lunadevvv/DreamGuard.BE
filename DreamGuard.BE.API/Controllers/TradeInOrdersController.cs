using DreamGuard.BE.API.Hubs;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DreamGuard.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeInOrdersController : ControllerBase
    {
        private readonly ITradeInOrderService _service;
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public TradeInOrdersController(ITradeInOrderService service, IPaymentService paymentService, IBackgroundJobClient backgroundJobClient)
        {
            _service = service;
            _paymentService = paymentService;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpPost]
        [Authorize(Roles = $"{Role.User}")]
        public async Task<IActionResult> CreateTradeInOrder([FromBody] CreateTradeInOrderRequest request)
        {
            
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            string ipAddress = GetIpAddress();
            var result = await _service.TradeInOrderAsync(request, customerId, ipAddress);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            //hangfire background job to expirepayment
            _backgroundJobClient.Schedule<PaymentService>(job => job.ExpireTradeinPayment(result.Data.PaymentId), result.Data.ExpiredAt.AddSeconds(30));

            return Ok(result.Data);
        }

        [HttpPost("{tradeInOrderId}/ReOrderFailedTradeIn")]
        [Authorize(Roles = $"{Role.User}")]
        public async Task<IActionResult> ReOrderFailedTradeIn(Guid tradeInOrderId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            string ipAddress = GetIpAddress();
            var result = await _service.ReOrderTradeInAsync(tradeInOrderId, customerId, ipAddress);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            _backgroundJobClient.Schedule<PaymentService>(job => job.ExpireTradeinPayment(result.Data.PaymentId), result.Data.ExpiredAt.AddSeconds(30));

            return Ok(result.Data);
        }

        [HttpPost("{tradeInOrderId}/upload-image")]
        [Authorize(Roles = $"{Role.User}")]
        public async Task<IActionResult> UploadTradeInOrderImage(Guid tradeInOrderId, [FromForm] TradeInOrderImageCreateRequest request)
        {
            var result = await _service.UploadTradeInOrderImageAsync(tradeInOrderId, request);
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

        [HttpPost("calculate-trade-in-order-price")]
        [Authorize(Roles = $"{Role.User}")]
        public async Task<IActionResult> CalculateTradeInOrderPrice([FromBody] CalculateTradeInOrderPriceRequest request)
        {
            var result = await _service.CalculatePriceAsync(request);
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
        [HttpGet("{tradeInOrderId}")]
        [Authorize]
        public async Task<IActionResult> GetTradeInOrderDetailById(Guid tradeInOrderId)
        {
            var result = await _service.GetOrderDetailByIdAsync(tradeInOrderId);
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

        [HttpGet("my-orders")]
        [Authorize(Roles = $"{Role.User}")]
        public async Task<IActionResult> GetMyOrders(int pageNumber = 1, int pageSize = 4)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.GetMyOrdersAsync(customerId, pageNumber, pageSize);
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

        [HttpGet("waiting-orders")]
        [Authorize(Roles = $"{Role.Seller}, {Role.Manager}, {Role.Admin}")]
        public async Task<IActionResult> GetWaitingOrders(int pageNumber = 1, int pageSize = 4)
        {
            var result = await _service.GetWaitingOrdersAsync(pageNumber, pageSize);
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
        [HttpGet("AdminSearchTradeInOrder")]
        [Authorize(Roles = $"{Role.Manager}, {Role.Admin}")]
        public async Task<IActionResult> AdminSearchTradeInOrder([FromQuery] AdminSearchTradeInOrderRequest request,int pageNumber = 1, int pageSize = 4)
        {
            var result = await _service.AdminSearchTradeInOrder(
                request.CustomerId,
                request.ProductVariantId,
                request.Status,
                request.IsGood,
                request.TradeInPrice,
                request.AmountToPay,
                request.DepositAmount,
                request.PhoneNumber,
                pageNumber,
                pageSize
                );
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
        [HttpGet("get-trade-in-dash-board")]
        [Authorize(Roles = $"{Role.Manager}, {Role.Admin}")]
        public async Task<IActionResult> GetTradeInDashBoard([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            var result = await _service.GetTradeInDashBoardAsync(fromDate, toDate);
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

        [HttpPost("{tradeInOrderId}/confirm")]
        [Authorize(Roles = $"{Role.Seller}, {Role.Manager}, {Role.Admin}")]
        public async Task<IActionResult> Confirm(Guid tradeInOrderId, decimal tradeInPrice)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.ConfirmAsync(tradeInOrderId, tradeInPrice);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error }
                });
            }
            var auditLog = new AuditLog
            {
                UserId = staffId,
                ActionType = "ConfirmTradeInOrder",
                Message = $"seller confirm TradeInOrder: {tradeInOrderId} with tradeInPrice: {tradeInPrice}",
                UserRole = Role.Seller
            };
            _backgroundJobClient.Enqueue<AuditLogService>(job => job.LogAsync(auditLog));
            return Ok(result.Message);
        }

        [HttpPatch("{tradeInOrderId}/processing")]
        [Authorize(Roles = $"{Role.DeliveryStaff}, {Role.Manager}, {Role.Admin}")]
        public async Task<IActionResult> Processing(Guid tradeInOrderId, ProcessingTradeInOrderRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.ProcessingAsync(tradeInOrderId, staffId, request.ShippingDate);
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

        [HttpPatch("{tradeInOrderId}/cancel")]
        [Authorize(Roles = $"{Role.User}")]
        public async Task<IActionResult> CustomerCancel(Guid tradeInOrderId)
        {
            var result = await _service.CancelAsync(tradeInOrderId, false);
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

        [HttpPatch("{tradeInOrderId}/admin-cancel")]
        [Authorize(Roles = $"{Role.Manager}, {Role.Seller}, {Role.Admin}")]
        public async Task<IActionResult> AdminCancel(Guid tradeInOrderId)
        {
            var result = await _service.CancelAsync(tradeInOrderId, true);
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

        [HttpPatch("{tradeInOrderId}/delivered")]
        [Authorize(Roles = $"{Role.DeliveryStaff}, {Role.Manager}, {Role.Admin}")]
        public async Task<IActionResult> Delivered(Guid tradeInOrderId)
        {
            var result = await _service.DeliveredAsync(tradeInOrderId);
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

        [HttpPatch("{tradeInOrderId}/completed")]
        [Authorize(Roles = $"{Role.Manager}, {Role.Admin}")]
        public async Task<IActionResult> Complete(Guid tradeInOrderId)
        {
            var result = await _service.CompletedAsync(tradeInOrderId);
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

        [HttpPost("{tradeInOrderId}/CreateConversation")]
        [Authorize(Roles = $"{Role.Seller}")]
        public async Task<IActionResult> CreateConversation(Guid tradeInOrderId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }
            var result = await _service.CreateConversationAsync(tradeInOrderId, staffId);
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

        private string GetIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            return ipAddress ?? "127.0.0.1";
        }
        
    }
}
