using CloudinaryDotNet.Actions;
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
using System;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IHangFireService _hangFireService;
        private readonly IServiceOrderRepository _serviceOrderRepository;
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
            IInventoryRepository inventoryRepository,
            IHangFireService hangFireService,
            IServiceOrderRepository serviceOrderRepository)
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
            _hangFireService = hangFireService;
            _serviceOrderRepository = serviceOrderRepository;
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
                    if (payment.SoId.HasValue)
                    {
                        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(payment.SoId.Value);
                        AuditLog audit = new AuditLog
                        {
                            UserId = serviceOrder.CustomerId,
                            ActionType = $"Confirmed by VnPayGateWay",
                            Message = $"ServiceOrder:{serviceOrder.SoId} confirmed via VnPay callback."
                        };
                        Notification notification = new Notification
                        {
                            UserId = serviceOrder.CustomerId,
                            ActionType = "Confirmed By VnPayGateWay",
                            Message = $"ServiceOrder: {serviceOrder.SoId} has been paid sucessfully. we will contact you soon",
                        };
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                        _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));
                    }
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
                        AuditLog audit = new AuditLog
                        {
                            UserId = order.CustomerId,
                            ActionType = $"Confirmed by VnPayGateWay",
                            Message = $"Order:{order.Id} confirmed via VnPay callback. order status is now Confirmed",
                        };
                        Notification notification = new Notification
                        {
                            UserId = order.CustomerId,
                            ActionType = "Confirmed By VnPayGateWay",
                            Message = $"Your order {order.Id} has been confirmed after successful payment.",
                        };
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                        _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));

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
                        AuditLog audit = new AuditLog
                        {
                            UserId = tradeInOrder.CustomerId,
                            ActionType = $"Confirmed by VnPayGateWay",
                            Message = $"TradeInOrder:{tradeInOrder.TradeInOrderId} confirmed via VnPay callback. TradeInOrder status is now WATING_FOR_STAFF"
                        };
                        Notification notification = new Notification
                        {
                            UserId = tradeInOrder.CustomerId,
                            ActionType = "Confirmed By VnPayGateWay",
                            Message = $"Your TradeInOrder {tradeInOrder.TradeInOrderId} has been confirmed after successful payment. we will contact you soon",
                        };
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                        _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));

                    }
                    
                }
                else
                {
                    //ĐANG DÙNG CHUNG CHO TRADE IN ORDER VÀ CẢ ORDER  và ServiceOrder !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    payment.Status = PaymentStatus.Failed;

                    //tradeinorder payment failed
                    if (payment.TradeInOrderId.HasValue)
                    {
                        var tradeInOrder = await _tradeInOrderRepository.GetOrderDetailById(payment.TradeInOrderId.Value);
                        if (tradeInOrder != null && tradeInOrder.Status == TradeInOrderStatus.Pending)
                        {
                            var tradeInResult = await _orderItemRepository.DecreaseTradeInUsedAmountAsync(tradeInOrder.OrderItem.Id);
                            if (!tradeInResult)
                            {
                                return Result<VnPaymentResponse>.Failure("Failed to increase tradeinUsedAmount trade-in item.", 500);
                            }
                            var inventoryResult = await _inventoryRepository.IncreaseInventoryStock(tradeInOrder.ProductVariant.Id); // Restock the reserved item
                            if (inventoryResult == 0)
                            {
                                return Result<VnPaymentResponse>.Failure("Failed to restock inventory for trade-in item.", 500);
                            }
                            AuditLog audit = new AuditLog
                            {
                                UserId = tradeInOrder.CustomerId,
                                ActionType = $"Confirmed by VnPayGateWay",
                                Message = $"TradeInOrder:{tradeInOrder.TradeInOrderId} failed for payment. ProductVariantId:{tradeInOrder.ProductVariant.Id} stock increased"
                            };
                            Notification notification = new Notification
                            {
                                UserId = tradeInOrder.CustomerId,
                                ActionType = "Failed By VnPayGateWay",
                                Message = $"Your TradeInOrder {tradeInOrder.TradeInOrderId} is pending after failed payment. please try again",
                            };
                            _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                            _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));
                        }
                    }
                    //service order payment failed
                    if (payment.SoId.HasValue)
                    {
                        var serviceOrder = await _serviceOrderRepository.GetByIdAsync(payment.SoId.Value);
                        Notification notification = new Notification
                        {
                            UserId = serviceOrder.CustomerId,
                            ActionType = "Failed By VnPayGateWay",
                            Message = $"ServiceOrder: {serviceOrder.SoId} payment has failed. Please go to your order history to complete the payment again.",
                        };
                        _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));
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
            Guid userId, int pageNumber, PaymentStatus? status, string? orderCode)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaginatedList<PaymentSummaryResponse>>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var payments = await _paymentRepository.GetPaymentsByCustomerIdAsync(customerId, pageNumber, status, orderCode);

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

        public async Task<Result> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus, Guid managerId, string userRole, string? evidenceUrl = null)
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
                // If marking as Paid (e.g. COD confirmed), also update order to Confirmed
                if (newStatus == PaymentStatus.Paid && payment.POrderId.HasValue)
                {
                    payment.Status = newStatus;
                    payment.UpdatedAt = DateTime.UtcNow;
                    var order = await _orderRepository.GetByIdAsync(payment.POrderId.Value);
                    if (order != null && order.Status == OrderStatus.Pending)
                    {
                        order.Status = OrderStatus.Confirmed;
                        order.UpdatedAt = DateTime.UtcNow;
                        await _orderRepository.UpdateAsync(order);
                    }
                }

                // if payment type is refund, also add evidence url for refund proof
                if (payment.PaymentType == PaymentType.Refund && newStatus == PaymentStatus.Refunded)
                {
                    if (string.IsNullOrEmpty(evidenceUrl))
                    {
                        return Result.Failure("Evidence URL is required when marking a refund payment as Refunded.", 400);
                    }
                    payment.Status = newStatus;
                    payment.UpdatedAt = DateTime.UtcNow;
                    payment.EvidenceUrl = evidenceUrl;
                }

                // if marking as CODPaid, also update order with Delivered status to Completed
                if (newStatus == PaymentStatus.CODPaid && payment.POrderId.HasValue)
                {   
                    payment.Status = newStatus;
                    payment.UpdatedAt = DateTime.UtcNow;
                    var order = await _orderRepository.GetByIdAsync(payment.POrderId.Value);
                    if (order != null && order.Status == OrderStatus.Delivered)
                    {
                        order.Status = OrderStatus.Completed;
                        order.UpdatedAt = DateTime.UtcNow;
                        await _orderRepository.UpdateAsync(order);
                    }
                }

                await _paymentRepository.UpdateAsync(payment);

                await transaction.CommitAsync();

                // Auto-cancel order when payment is marked as Failed
                if (newStatus == PaymentStatus.Failed && payment.POrderId.HasValue)
                {
                    await _orderService.UpdateOrderStatusAsync(payment.POrderId.Value, OrderStatus.Cancelled);
                }

                // Log audit
                var auditLog = new AuditLog
                {
                    UserId = managerId,
                    UserRole = userRole,
                    ActionType = "UpdatePaymentStatus",
                    Message = $"Updated payment status to '{newStatus}' for PaymentId: {paymentId}"
                };
                _hangFireService.Enqueue<IAuditLogService>(job => job.LogAsync(auditLog));

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
                (PaymentStatus.Pending, PaymentStatus.CODPaid) => true,
                (PaymentStatus.Refunding, PaymentStatus.Refunded) => true,
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
                UpdatedAt = payment.UpdatedAt,
                EvidenceUrl = payment.EvidenceUrl
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
                    //also restock inventory and back tradeinUsedAmount when trade-in payment expires 
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
        public async Task<Result> ExpireServiceOrderPayment(Guid paymentId)
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

        public async Task<Result> CreateRefundPaymentAsync(RefundPaymentRequest request, Guid managerId, string userRole)
        {
            if (request.OrderId != null)
            {
                return await CreateRefundForProductOrderAsync(request.OrderId.Value, request.Amount, request.Reason, managerId, userRole);
            }
            else if (request.TradeInOrderId != null)
            {
                return await CreateRefundForTradeInOrderAsync(request.TradeInOrderId.Value, request.Amount, request.Reason, managerId, userRole);
            }
            else if (request.SoId != null)
            {
                return await CreateRefundForServiceOrderAsync(request.SoId.Value, request.Amount, request.Reason, managerId, userRole);
            }
            else
            {
                return Result.Failure("Must provide either orderId or tradeInOrderId.", 400);
            }
        }

        private async Task<Result> CreateRefundForServiceOrderAsync(Guid soId, decimal amount, string reason, Guid managerId, string userRole)
        {
            var serviceOrder = await _serviceOrderRepository.GetByIdWithServiceTask(soId);
            if (serviceOrder == null)
            {
                return Result.Failure("Service order not found", 404);
            }

            var lastPayment = serviceOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (serviceOrder.Status != OrderServiceStatus.Cancelled && serviceOrder.Status != OrderServiceStatus.ForcedCancelled && serviceOrder.Status != OrderServiceStatus.Rejected)
            {
                return Result.Failure("Refund can be created for cancelled or ForcedCancelled or Rejected service orders.", 400);
            }

            if (lastPayment != null && lastPayment.Status == PaymentStatus.Paid)
            {
                if (amount <= 0 || amount > lastPayment.Amount)
                {
                    return Result.Failure("Refund amount must be greater than 0 and less than or equal to the original payment amount.", 400);
                }

                var paymentRefund = new Payment
                {
                    SoId = serviceOrder.SoId,
                    Amount = amount,
                    OrderCode = lastPayment.OrderCode,
                    PaymentType = PaymentType.Refund,
                    PaymentMethod = PaymentMethod.VnPay,
                    Status = PaymentStatus.Refunding,
                    Description = $"Refund for cancelled ServiceOrder {lastPayment.OrderCode}",
                };
                
                await _paymentRepository.CreateAsync(paymentRefund);

                var auditLog = new AuditLog
                {
                    UserId = managerId,
                    UserRole = userRole,
                    ActionType = "CreateRefundPayment",
                    Message = $"Created refund payment for ServiceOrderId: {serviceOrder.SoId}, RefundPaymentId: {paymentRefund.Id}, Amount: {amount}, Reason: {reason}"
                };
                _hangFireService.Enqueue<IAuditLogService>(job => job.LogAsync(auditLog));
            }
            else
            {
                return Result.Failure("No successful payment found for this service order to determine refund method.", 400);
            }

            return Result.Success("Refund payment created successfully.");
        }

        public async Task<Result> CreateRefundForProductOrderAsync(Guid orderId, decimal amount, string reason, Guid managerId, string userRole)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return Result.Failure("Order not found.", 404);
            }

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Completed || order.Status == OrderStatus.ExchangeRequested)
            {
                return Result.Failure("Cannot create refund for delivered or completed or ExchangeRequested orders.", 400);
            }

            // var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
            // if (payment == null)
            // {
            //     return Result.Failure("No payment found for this order.", 400);
            // }

            var lastPaymentPaid = order.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.PaymentType != PaymentType.Refund && p.PaymentMethod == PaymentMethod.VnPay)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            if (lastPaymentPaid == null)
            {
                return Result.Failure("No successful payment found for this order to determine refund method.", 400);
            }

            if (amount <= 0 || amount > lastPaymentPaid.Amount)
            {
                return Result.Failure("Refund amount must be greater than 0 and less than or equal to the original payment amount.", 400);
            }

            var refundPayment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderCode = order.OrderCode,
                POrderId = order.Id,
                Status = PaymentStatus.Refunding,
                PaymentType = PaymentType.Refund,
                Amount = amount,
                Description = $"Refund for Order {order.OrderCode}.",
                PaymentMethod = PaymentMethod.VnPay,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5)
            };

            var result = await _paymentRepository.CreateAsync(refundPayment);
            if (result > 0)
            {
                var auditLog = new AuditLog
                {
                    UserId = managerId,
                    UserRole = userRole,
                    ActionType = "CreateRefundPayment",
                    Message = $"Created refund payment for ProductOrderId: {refundPayment.Id}, OrderCode: {refundPayment.OrderCode}, RefundPaymentId: {refundPayment.Id}, Amount: {amount}, Reason: {reason}"
                };
            _hangFireService.Enqueue<IAuditLogService>(job => job.LogAsync(auditLog));
            }

            return Result.Success("Refund payment created successfully.");
        }

        public async Task<Result> CreateRefundForTradeInOrderAsync(Guid tradeInOrderId, decimal amount, string reason, Guid managerId, string userRole)
        {
            var tradeInOrder = await _tradeInOrderRepository.GetOrderDetailById(tradeInOrderId);
            if (tradeInOrder == null)
            {
                return Result.Failure("Trade-in order not found.", 404);
            }

            if (tradeInOrder.Status != TradeInOrderStatus.CANCELLED && tradeInOrder.Status != TradeInOrderStatus.FORCED_CANCELLED && tradeInOrder.Status != TradeInOrderStatus.ADMINCANCELLED)
            {
                return Result.Failure("Refund can be created for cancelled or ForcedCancelled or Rejected trade-in orders.", 400);
            }

            // Check if deposit was paid
            var lastPaymentPaid = tradeInOrder.Payments
                .Where(p => p.PaymentType == PaymentType.Deposit && p.Status == PaymentStatus.Paid)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            if (lastPaymentPaid == null)
            {
                return Result.Failure("No successful deposit payment found for this trade-in order to determine refund method.", 400);
            }

            if (amount <= 0 || amount > lastPaymentPaid.Amount)
            {
                return Result.Failure("Refund amount must be greater than 0 and less than or equal to the original deposit payment amount.", 400);
            }

            if (lastPaymentPaid != null)
            {
                var paymentRefund = new Payment
                {
                    TradeInOrderId = tradeInOrder.TradeInOrderId,
                    Amount = amount,
                    OrderCode = tradeInOrder.OrderCode,
                    PaymentType = PaymentType.Refund,
                    PaymentMethod = lastPaymentPaid.PaymentMethod,
                    Status = PaymentStatus.Refunding,
                    Description = $"Refund for ProcessReturned TradeInOrder {tradeInOrder.OrderCode}",
                };
                var result = await _paymentRepository.CreateAsync(paymentRefund);
                if (result > 0)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = managerId,
                        UserRole = userRole,
                        ActionType = "CreateRefundPayment",
                        Message = $"Created refund payment for TradeInOrderId: {tradeInOrder.TradeInOrderId}, RefundPaymentId: {paymentRefund.Id}, Amount: {amount}, Reason: {reason}"
                    };
                _hangFireService.Enqueue<IAuditLogService>(job => job.LogAsync(auditLog));
                }
            }

            return Result.Success("Refund payment created successfully.");
        }
    }
}
