using System;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Options;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly VnPayOptions _vnPayOptions;
        private readonly IOrderService _orderService;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITradeInOrderRepository _tradeInOrderRepository;
        public PaymentService(
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IVnPayService vnPayService,
            IUnitOfWork unitOfWork,
            IOptions<VnPayOptions> vnPayOptions,
            IOrderService orderService,
            ICustomerRepository customerRepository,
            ITradeInOrderRepository tradeInOrderRepository,
            IOrderItemRepository orderItemRepository,
            IInventoryRepository inventoryRepository)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _vnPayService = vnPayService;
            _unitOfWork = unitOfWork;
            _vnPayOptions = vnPayOptions.Value;
            _orderService = orderService;
            _customerRepository = customerRepository;
            _tradeInOrderRepository = tradeInOrderRepository;
            _orderItemRepository = orderItemRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<Result<CreatePaymentResponse>> CreatePaymentAsync(Guid orderId, PaymentMethod method, string ipAddress)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return Result<CreatePaymentResponse>.Failure("Order not found.", 404);
            }

            if (order.Status != OrderStatus.Pending)
            {
                return Result<CreatePaymentResponse>.Failure("Only pending orders can be paid.", 400);
            }

            // Check if a non-failed payment already exists for this order
            var existingPayment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
            if (existingPayment != null && existingPayment.Status != PaymentStatus.Failed)
            {
                return Result<CreatePaymentResponse>.Failure("A payment already exists for this order.", 400);
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderCode = order.OrderCode,
                POrderId = orderId,
                Status = PaymentStatus.Pending,
                Amount = order.TotalAmount,
                Description = $"Payment for Order {order.OrderCode}",
                PaymentMethod = method,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(_vnPayOptions.PaymentExpirationMinutes)
            };

            var createResult = await _paymentRepository.CreateAsync(payment);
            if (createResult < 0)
            {
                return Result<CreatePaymentResponse>.Failure("Failed to create payment.", 500);
            }

            string? paymentUrl = null;

            if (method == PaymentMethod.VnPay)
            {
                var vnPayRequest = new VnPaymentRequest
                {
                    PaymentId = payment.Id.ToString(),
                    OrderCode = order.OrderCode,
                    Description = payment.Description,
                    Amount = (int)payment.Amount,
                    IpAddress = ipAddress,
                    CreatedDate = payment.CreatedAt
                };

                paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest);
            }

            return Result<CreatePaymentResponse>.Success(new CreatePaymentResponse
            {
                PaymentId = payment.Id,
                OrderCode = order.OrderCode,
                PaymentMethod = method,
                PaymentType = payment.PaymentType,
                Status = payment.Status,
                Amount = payment.Amount,
                PaymentUrl = paymentUrl,
                ExpiredAt = payment.ExpiredAt
            });
        }

        public async Task<Result<VnPaymentResponse>> HandleVnPayCallbackAsync(
            Microsoft.AspNetCore.Http.IQueryCollection queryParams)
        {
            var vnPayResult = _vnPayService.GetPaymentResult(queryParams);

            if (!Guid.TryParse(vnPayResult.PaymentId, out var paymentId))
            {
                return Result<VnPaymentResponse>.Failure("Invalid payment reference.", 400);
            }

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return Result<VnPaymentResponse>.Failure("Payment not found.", 404);
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return Result<VnPaymentResponse>.Failure("Payment has already been processed.", 400);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (vnPayResult.Success)
                {
                    payment.Status = PaymentStatus.Paid;

                    // Update order status to Confirmed when payment succeeds
                    if (payment.POrderId.HasValue)
                    {
                        var order = await _orderRepository.GetByIdAsync(payment.POrderId.Value);
                        if (order != null && order.Status == OrderStatus.Pending)
                        {
                            order.Status = OrderStatus.Confirmed;
                            order.UpdatedAt = DateTime.UtcNow;
                            await _orderRepository.UpdateAsync(order);
                        }
                    }
                    //update trade-in order status to WAITING_FOR_STAFF when payment succeeds
                    if (payment.TradeInOrderId.HasValue)
                    {
                        var tradeInOrder = await _tradeInOrderRepository.GetByIdAsync(payment.TradeInOrderId.Value);
                        if (tradeInOrder != null && tradeInOrder.Status == TradeInOrderStatus.Pending)
                        {
                            tradeInOrder.Status = TradeInOrderStatus.WAITING_FOR_STAFF;
                            await _tradeInOrderRepository.UpdateAsync(tradeInOrder);
                        }
                    }
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                    if (payment.TradeInOrderId.HasValue)
                    {
                        var tradeInOrder = await _tradeInOrderRepository.GetOrderDetailById(payment.TradeInOrderId.Value);
                        if (tradeInOrder != null && tradeInOrder.Status == TradeInOrderStatus.Pending)
                        {
                            var tradeInResult = await _orderItemRepository.DecreaseTradeInUsedAmountAsync(tradeInOrder.OrderItem.Id); 
                            if(!tradeInResult)
                            {
                                return Result<VnPaymentResponse>.Failure("Failed to increase tradeinUsedAmount trade-in item.", 500);
                            }
                            var inventoryResult = await _inventoryRepository.IncreaseInventoryStock(tradeInOrder.ProductVariant.Id); // Restock the reserved item
                            if(inventoryResult == 0)
                            {
                                return Result<VnPaymentResponse>.Failure("Failed to restock inventory for trade-in item.", 500);
                            }
                        }
                    }
                }

                payment.UpdatedAt = DateTime.UtcNow;
                payment.Description = $"{payment.Description} | VnPay TxnId: {vnPayResult.VnpayTransactionId}, ResponseCode: {vnPayResult.VnPayResponseCode}";
                await _paymentRepository.UpdateAsync(payment);

                await transaction.CommitAsync();

                // Auto-cancel order when payment fails (runs in separate transaction)
                if (payment.Status == PaymentStatus.Failed && payment.POrderId.HasValue)
                {
                    await _orderService.UpdateOrderStatusAsync(payment.POrderId.Value, OrderStatus.Cancelled);
                }

                return Result<VnPaymentResponse>.Success(vnPayResult);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<VnPaymentResponse>.Failure("Failed to process payment callback.", 500);
            }
        }

        public async Task<Result<PaymentResponse>> GetPaymentByIdAsync(Guid userId, Guid paymentId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaymentResponse>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found.", 404);
            }

            if (payment.POrder == null || payment.POrder.CustomerId != customerId)
            {
                return Result<PaymentResponse>.Failure("Payment not found.", 404);
            }

            return Result<PaymentResponse>.Success(MapToResponse(payment));
        }

        public async Task<Result<PaymentResponse>> GetPaymentByOrderIdAsync(Guid userId, Guid orderId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaymentResponse>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found.", 404);
            }

            if (payment.POrder == null || payment.POrder.CustomerId != customerId)
            {
                return Result<PaymentResponse>.Failure("Payment not found.", 404);
            }

            return Result<PaymentResponse>.Success(MapToResponse(payment));
        }

        public async Task<Result<PaginatedList<PaymentSummaryResponse>>> GetPaymentsByUserAsync(
            Guid userId, int pageNumber, PaymentStatus? status)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaginatedList<PaymentSummaryResponse>>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var payments = await _paymentRepository.GetPaymentsByCustomerIdAsync(customerId, pageNumber, status);

            var responses = payments.Items.Select(p => new PaymentSummaryResponse
            {
                Id = p.Id,
                OrderCode = p.OrderCode,
                PaymentType = p.PaymentType,
                Status = p.Status,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                CreatedAt = p.CreatedAt
            }).ToList();

            return Result<PaginatedList<PaymentSummaryResponse>>.Success(
                new PaginatedList<PaymentSummaryResponse>(
                    responses, payments.TotalCount, payments.PageNumber, payments.PageSize));
        }

        public async Task<Result<PaginatedList<PaymentSummaryResponse>>> GetAllPaymentsForAdminAsync(
            int pageNumber, PaymentStatus? status, PaymentMethod? method, string? orderCode)
        {
            var payments = await _paymentRepository.GetAllPaymentsForAdminAsync(pageNumber, status, method, orderCode);

            var responses = payments.Items.Select(p => new PaymentSummaryResponse
            {
                Id = p.Id,
                OrderCode = p.OrderCode,
                PaymentType = p.PaymentType,
                Status = p.Status,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                CreatedAt = p.CreatedAt
            }).ToList();

            return Result<PaginatedList<PaymentSummaryResponse>>.Success(
                new PaginatedList<PaymentSummaryResponse>(
                    responses, payments.TotalCount, payments.PageNumber, payments.PageSize));
        }

        public async Task<Result<PaymentResponse>> GetPaymentDetailForAdminAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                return Result<PaymentResponse>.Failure("Payment not found.", 404);
            }

            return Result<PaymentResponse>.Success(MapToResponse(payment));
        }

        public async Task<Result> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return Result.Failure("Payment not found.", 404);
            }

            if (!IsValidStatusTransition(payment.Status, newStatus))
            {
                return Result.Failure(
                    $"Cannot transition payment status from '{payment.Status}' to '{newStatus}'.", 400);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                payment.Status = newStatus;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                // If marking as Paid (e.g. COD confirmed), also update order to Confirmed
                if (newStatus == PaymentStatus.Paid && payment.POrderId.HasValue)
                {
                    var order = await _orderRepository.GetByIdAsync(payment.POrderId.Value);
                    if (order != null && order.Status == OrderStatus.Pending)
                    {
                        order.Status = OrderStatus.Confirmed;
                        order.UpdatedAt = DateTime.UtcNow;
                        await _orderRepository.UpdateAsync(order);
                    }
                }

                await transaction.CommitAsync();

                // Auto-cancel order when payment is marked as Failed
                if (newStatus == PaymentStatus.Failed && payment.POrderId.HasValue)
                {
                    await _orderService.UpdateOrderStatusAsync(payment.POrderId.Value, OrderStatus.Cancelled);
                }

                return Result.Success($"Payment status updated to '{newStatus}'.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to update payment status.", 500);
            }
        }

        private static bool IsValidStatusTransition(PaymentStatus current, PaymentStatus next)
        {
            return (current, next) switch
            {
                (PaymentStatus.Pending, PaymentStatus.Paid) => true,
                (PaymentStatus.Pending, PaymentStatus.Failed) => true,
                _ => false
            };
        }

        private static PaymentResponse MapToResponse(Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                OrderCode = payment.OrderCode,
                POrderId = payment.POrderId,
                TradeInOrderId = payment.TradeInOrderId,
                PaymentType = payment.PaymentType,
                Status = payment.Status,
                Amount = payment.Amount,
                Description = payment.Description,
                PaymentMethod = payment.PaymentMethod,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
        public async Task<Result> ExpireTradeinPayment(Guid paymentId)
        {
            //có lỗi xảy ra => rollback và hangfire retry 
            var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
                if (payment != null && payment.Status == PaymentStatus.Pending)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    _paymentRepository.UpdateEntity(payment);
                    // Also update trade-in order status to Cancelled
                    if (payment.TradeInOrderId.HasValue)
                    {
                        var tradeInOrder = payment.TradeInOrder;

                        if (tradeInOrder != null && tradeInOrder.Status == TradeInOrderStatus.Pending)
                        {
                            await _orderItemRepository.DecreaseTradeInUsedAmountAsync(tradeInOrder.OrderItem.Id);
                            //update inventory atomically 
                            await _inventoryRepository.IncreaseInventoryStock(tradeInOrder.ProductVariant.Id);
                        }
                    }
                }
                var result = await _unitOfWork.SaveChangeAsync();
                await transaction.CommitAsync();
                return Result.Success($"Update sucessfully");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Result> ExpireProductOrderPayment(Guid paymentId)
        {
            //có lỗi xảy ra => rollback và hangfire retry 
            var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
                if (payment != null && payment.Status == PaymentStatus.Pending)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    _paymentRepository.UpdateEntity(payment);
                    // Also update product order status to Cancelled
                    if (payment.POrderId.HasValue)
                    {
                        var updatedResult = await _orderService.UpdateOrderStatusAsync(payment.POrderId.Value, OrderStatus.Cancelled);
                        if (!updatedResult.Succeeded){
                            await transaction.RollbackAsync();
                            return Result.Failure("Failed to cancel order after payment expiration.", 400);
                        }
                    }
                }
                var result = await _unitOfWork.SaveChangeAsync();
                await transaction.CommitAsync();
                return Result.Success($"Update sucessfully");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
