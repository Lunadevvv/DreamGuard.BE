using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Implements;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DreamGuard.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public PaymentController(IPaymentService paymentService, IBackgroundJobClient backgroundJobClient)
        {
            _paymentService = paymentService;
            _backgroundJobClient = backgroundJobClient;
        }

        // Get payments for the current user
        [HttpGet]
        public async Task<IActionResult> GetMyPayments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] PaymentStatus? status = null)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _paymentService.GetPaymentsByUserAsync(userId, pageNumber, status);
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

        // Get a specific payment by ID
        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentById(Guid paymentId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _paymentService.GetPaymentByIdAsync(userId, paymentId);
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

        // Get payment by order ID
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { ErrorCode = 401, Message = new List<string> { "Invalid user token." } });
            }

            var result = await _paymentService.GetPaymentByOrderIdAsync(userId, orderId);
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

        // VnPay callback endpoint (no auth required - called by VnPay)
        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayCallback()
        {
            var result = await _paymentService.HandleVnPayCallbackAsync(Request.Query);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }

            return Redirect(result.Data!.RedirectUrl);
        }

        //VnPay callback endpoint IPN (no auth required - called by VnPay)
        [HttpPost("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIPN()
        {
            var result = await _paymentService.HandleVnPayCallbackAsync(Request.Query);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }

            return Redirect(result.Data!.RedirectUrl);
        }

        // [Admin] Get all payments with filters
        [HttpGet("admin")]
        [Authorize(Roles = "Admin, Manager, Seller")]
        public async Task<IActionResult> GetAllPaymentsForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] PaymentStatus? status = null,
            [FromQuery] PaymentMethod? method = null,
            [FromQuery] string? orderCode = null)
        {
            var result = await _paymentService.GetAllPaymentsForAdminAsync(pageNumber, status, method, orderCode);
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

        // [Admin] Get payment detail by ID
        [HttpGet("admin/{paymentId}")]
        [Authorize(Roles = "Admin, Manager, Seller")]
        public async Task<IActionResult> GetPaymentDetailForAdmin(Guid paymentId)
        {
            var result = await _paymentService.GetPaymentDetailForAdminAsync(paymentId);
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

        // [Admin] Update payment status (e.g. confirm COD payment)
        [HttpPut("admin/{paymentId}/status")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> UpdatePaymentStatus(Guid paymentId, [FromQuery] PaymentStatus status)
        {
            var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, status);
            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new ErrorResponse
                {
                    ErrorCode = result.StatusCode,
                    Message = new List<string> { result.Error! }
                });
            }
            return Ok(result.Message);
        }
    }
}
