using AutoMapper;
using CloudinaryDotNet.Actions;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class TradeInOrderService : ITradeInOrderService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly ITradeInOrderRepository _tradeInOrderRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ITradeInImageRepository _tradeInImageRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IShippingTaskRepository _shippingTaskRepository;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IHangFireService _hangFireService;
        public TradeInOrderService(IProductVariantRepository productVariantRepository, ITradeInOrderRepository tradeInOrderRepository, IMapper mapper, IUnitOfWork unitOfWork, IOrderRepository orderRepository, ICloudinaryService cloudinaryService, ITradeInImageRepository tradeInImageRepository, IOrderItemRepository orderItemRepository, IVnPayService vnPayService, IPaymentRepository paymentRepository, ICustomerRepository customerRepository, IConversationRepository conversationRepository, IInventoryRepository inventoryRepository, IShippingTaskRepository shippingTaskRepository, ISystemConfigRepository systemConfigRepository, IHangFireService hangFireService)
        {
            _productVariantRepository = productVariantRepository;
            _tradeInOrderRepository = tradeInOrderRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _cloudinaryService = cloudinaryService;
            _tradeInImageRepository = tradeInImageRepository;
            _orderItemRepository = orderItemRepository;
            _vnPayService = vnPayService;
            _paymentRepository = paymentRepository;
            _customerRepository = customerRepository;
            _conversationRepository = conversationRepository;
            _inventoryRepository = inventoryRepository;
            _shippingTaskRepository = shippingTaskRepository;
            _systemConfigRepository = systemConfigRepository;
            _hangFireService = hangFireService;
        }
        private string GenerateOrderCode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..4].ToUpper();
            return $"TIOD-{datePart}-{randomPart}";
        }
        public async Task<Result<CreateTradeInOrderResponse>> TradeInOrderAsync(CreateTradeInOrderRequest request, Guid customerId, string ipAddress)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                //check if customer exists
                if (await _customerRepository.GetByIdAsync(customerId) == null)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Customer not found", 404);
                }
                //productvariant check
                var productVariant = await _productVariantRepository.GetVariantByIdAsync(request.ProductVariantId);
                if (productVariant == null)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("ProductVariant not found", 404);
                }
                //old product variant check
                var orderItem = await _orderRepository.GetOrderItemByIdAsync(request.POrderItemId);
                if (orderItem == null)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("OrderItem not found", 404);
                }
                if(orderItem.Order.Payments.Any(p => p.PaymentType == PaymentType.Purchase && p.Status == PaymentStatus.Paid) == false)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("The order of this OrderItem has not been paid", 400);
                }
                //check if order item is already used for trade-in
                if (orderItem.TradeInUsedAmount == orderItem.Quantity)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("This OrderItem has already been used for trade-in", 400);
                }
                //check if they are reordering the failed trade-in order
                var pendingTradeInOrder = orderItem.TradeInOrders.FirstOrDefault(ti => ti.Status == TradeInOrderStatus.Pending);
                if (pendingTradeInOrder != null)
                {
                    string errorMessage = $"please cancel or reOrder this order to be able to make a new order {pendingTradeInOrder.TradeInOrderId}";
                    return Result<CreateTradeInOrderResponse>.Failure(errorMessage, 409);
                }
                //check if the order item belongs to the customer
                if (orderItem.Order!.CustomerId != customerId)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("This is not your order", 400);
                }

                var oldProductVariant = orderItem.ProductVariant;
                //check if old product variant is eligible for trade-in
                if (oldProductVariant!.Product!.IsTradeInEligible == false)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("The product of this OrderItem is not eligible for trade-in", 400);
                }
                //check if the new product variant is in the same category parent as the old product variant
                var oldCategoryParentId = oldProductVariant.Product.Category!.CateParentId;
                if (oldCategoryParentId == null)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("The oldCategoryParentId is null", 400);
                }
                if (oldProductVariant.Product.Category!.CateParentId != productVariant.Product!.Category!.CateParentId)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("The new product must be in the same category parent as the old product", 400);
                }
                var minTradeInPrice = oldProductVariant.Product.MinTradeInPrice;
                var depositAmount = productVariant.Product!.DepositAmount;
                var salePrice = productVariant.SalePrice > 0 ? productVariant.SalePrice : productVariant.BasePrice;
                var amountToPay = salePrice - minTradeInPrice - depositAmount;
                if (amountToPay < 0)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("The amount to pay cannot be negative. Please choose a different product variant or check the trade-in price of your old product.", 400);
                }
                //trừ tồn kho
                var result = await _inventoryRepository.ReduceInventoryStock(productVariant.Id);
                if (result == 0)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("This product is out of stock", 400);
                }
                //cập nhật lượt trade-in đã dùng của order item
                var increaseResult = await _orderItemRepository.IncreaseTradeInUsedAmountAsync(orderItem.Id);
                if (!increaseResult)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Failed to IncreaseTradeInUsedAmount, please try again", 500);
                }
                var tradeInOrder = new TradeInOrder
                {
                    CustomerId = customerId,
                    ProductVariantId = request.ProductVariantId,
                    POrderItemId = request.POrderItemId,
                    OrderCode = GenerateOrderCode(),
                    IsGood = request.IsGood,
                    Description = request.Description,
                    ReceiverName = request.ReceiverName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    TradeInPrice = minTradeInPrice,
                    DepositAmount = depositAmount,
                    AmountToPay = amountToPay,
                };


                Payment payment = new Payment
                {
                    TradeInOrderId = tradeInOrder.TradeInOrderId,
                    Amount = tradeInOrder.DepositAmount,
                    OrderCode = tradeInOrder.OrderCode,
                    PaymentType = PaymentType.Deposit,
                    PaymentMethod = PaymentMethod.VnPay,
                    Status = PaymentStatus.Pending,
                    Description = $"Deposit Payment for TradeInOrder {tradeInOrder.OrderCode}",
                    ExpiredAt = DateTime.UtcNow.AddMinutes(5)
                };
                tradeInOrder.Payments.Add(payment);
                await _tradeInOrderRepository.CreateAsync(tradeInOrder);
                await transaction.CommitAsync();

                // Generate VnPay URL after commit
                // (external call, should not be inside transaction)
                string? paymentUrl = null;

                var vnPayRequest = new VnPaymentRequest
                {
                    PaymentId = payment.Id.ToString(),
                    OrderCode = tradeInOrder.OrderCode,
                    Description = payment.Description,
                    Amount = (int)tradeInOrder.DepositAmount,
                    IpAddress = ipAddress,
                    CreatedDate = payment.CreatedAt,
                    ExpiredAt = payment.ExpiredAt.AddHours(7) // Convert to UTC+7 for VnPay
                };
                paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest);
                if (string.IsNullOrEmpty(paymentUrl))
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Failed to create payment URL", 500);
                }
                var response = new CreateTradeInOrderResponse
                {
                    TradeInOrderId = tradeInOrder.TradeInOrderId,
                    PaymentId = payment.Id,
                    TradeInPrice = tradeInOrder.TradeInPrice,
                    DepositAmount = tradeInOrder.DepositAmount,
                    AmountToPay = tradeInOrder.AmountToPay,
                    PaymentUrl = paymentUrl,
                    ExpiredAt = payment.ExpiredAt
                };
                var auditLog = new AuditLog
                {
                    UserId = customerId,
                    ActionType = "CreateTradeInOrder",
                    Message = $"customer: {customerId} create TradeInOrder: {tradeInOrder.TradeInOrderId} with productVariant: {request.ProductVariantId}, reduce stock by 1",
                    UserRole = DAL.Constants.Role.User
                };
                _hangFireService.Enqueue<AuditLogService>(job => job.LogAsync(auditLog));
                return Result<CreateTradeInOrderResponse>.Success(response);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<Result<CreateTradeInOrderResponse>> ReOrderTradeInAsync(Guid tradeInOrderId, Guid customerId, string ipAddress)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var tradeInOrder = await _tradeInOrderRepository.GetTradeInByIdAsync(tradeInOrderId);
                if (tradeInOrder == null)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Trade in order not found", 404);
                }
                if (tradeInOrder.CustomerId != customerId)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("You are not the owner of this order", 403);
                }
                if (tradeInOrder.Status != TradeInOrderStatus.Pending)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Only pending order can be reordered", 400);
                }
                //check if order item is already used for trade-in
                if (tradeInOrder.OrderItem.TradeInUsedAmount == tradeInOrder.OrderItem.Quantity)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("This OrderItem has already been used for trade-in", 400);
                }
                var lastPayment = tradeInOrder.Payments.Where(p => p.PaymentType == PaymentType.Deposit).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
                if (lastPayment == null)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Deposit payment not found for this order", 404);
                }
                if (lastPayment.Status == PaymentStatus.Paid)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Deposit has already been paid for this order", 400);
                }
                if (lastPayment.Status != PaymentStatus.Failed)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Only orders with failed deposit payment can be reordered", 400);
                }
                //trừ tồn kho
                var result = await _inventoryRepository.ReduceInventoryStock(tradeInOrder.ProductVariant.Id);
                if (result == 0)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("This product is out of stock", 400);
                }
                //cập nhật lượt trade-in đã dùng của order item
                var increaseResult = await _orderItemRepository.IncreaseTradeInUsedAmountAsync(tradeInOrder.OrderItem.Id);
                if (!increaseResult)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Failed to IncreaseTradeInUsedAmount, please try again", 500);
                }
                //check if customer exist
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Customer not found", 404);
                }
                Payment payment = new Payment
                {
                    PaymentType = PaymentType.Deposit,
                    Amount = tradeInOrder.DepositAmount,
                    OrderCode = tradeInOrder.OrderCode,
                    PaymentMethod = lastPayment.PaymentMethod,
                    Status = PaymentStatus.Pending,
                    Description = $"Payment for TradeInOrder {tradeInOrder.OrderCode}",
                    ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                    TradeInOrderId = tradeInOrder.TradeInOrderId,
                };
                await _paymentRepository.CreateAsync(payment);
                await transaction.CommitAsync();

                // Generate VnPay URL after commit (external call, should not be inside transaction)
                string? paymentUrl = null;
                var vnPayRequest = new VnPaymentRequest
                {
                    PaymentId = payment.Id.ToString(),
                    OrderCode = payment.OrderCode,
                    Description = payment.Description,
                    Amount = (int)tradeInOrder.DepositAmount,
                    IpAddress = ipAddress,
                    CreatedDate = payment.CreatedAt,
                    ExpiredAt = payment.ExpiredAt.AddHours(7) // Convert to UTC+7 for VnPay
                };
                paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest);
                if (string.IsNullOrEmpty(paymentUrl))
                {
                    return Result<CreateTradeInOrderResponse>.Failure("Failed to create payment URL", 500);
                }
                var response = new CreateTradeInOrderResponse
                {
                    TradeInOrderId = tradeInOrder.TradeInOrderId,
                    PaymentId = payment.Id,
                    TradeInPrice = tradeInOrder.TradeInPrice,
                    DepositAmount = tradeInOrder.DepositAmount,
                    AmountToPay = tradeInOrder.AmountToPay,
                    PaymentUrl = paymentUrl,
                    ExpiredAt = payment.ExpiredAt
                };
                var auditLog = new AuditLog
                {
                    UserId = customerId,
                    ActionType = "ReOrderTradeInOrder",
                    Message = $"customer: {customerId} reorder TradeInOrder: {tradeInOrder.TradeInOrderId} reduce associated productVariant: {tradeInOrder.ProductVariant.Id} stock by 1",
                    UserRole = DAL.Constants.Role.User
                };
                _hangFireService.Enqueue<AuditLogService>(job => job.LogAsync(auditLog));
                return Result<CreateTradeInOrderResponse>.Success(response);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<Result> UploadTradeInOrderImageAsync(Guid tradeInOrderId, TradeInOrderImageCreateRequest request)
        {
            var tradeInOrder = await _tradeInOrderRepository.GetByIdAsync(tradeInOrderId);
            if (tradeInOrder == null)
            {
                return Result.Failure("tradeInOrder not found", 404);
            }
            if (request.Files == null || !request.Files.Any())
            {
                return Result.Failure("No files to upload", 400);
            }
            var files = request.Files;

            foreach (var file in files)
            {
                var uploadImageResult = await _cloudinaryService.UploadImageAsync(file, "TRADE_IN_ORDER_FOLDER");
                if (!uploadImageResult.Succeeded)
                {
                    return Result.Failure($"Failed to upload image: {uploadImageResult.Error}", 500);
                }
                var image = new TradeInImage
                {
                    TradeInOrderId = tradeInOrderId,
                    ImageUrl = uploadImageResult.Data!.Url,
                    PublicId = uploadImageResult.Data.PublicId
                };
                _tradeInImageRepository.AddEntity(image);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            if (result == 0)
            {
                return Result.Failure("Failed to save uploaded images", 500);
            }
            return Result.Success($"TradeInOrder Images upload sucessfully");
        }
        public async Task<Result<CalculateTradeInOrderPriceResponse>> CalculatePriceAsync(CalculateTradeInOrderPriceRequest request)
        {
            var productVariant = await _productVariantRepository.GetVariantByIdAsync(request.ProductVariantId);
            if (productVariant == null)
            {
                return Result<CalculateTradeInOrderPriceResponse>.Failure("ProductVariant not found", 404);
            }
            var oldProductVariant = await _productVariantRepository.GetVariantByIdAsync(request.OldProductVariantId);
            if (oldProductVariant == null)
            {
                return Result<CalculateTradeInOrderPriceResponse>.Failure("Old ProductVariant not found", 404);
            }
            var salePrice = productVariant.SalePrice > 0 ? productVariant.SalePrice : productVariant.BasePrice;
            var calculateResponse = new CalculateTradeInOrderPriceResponse
            {
                TradeInPrice = oldProductVariant.Product.MinTradeInPrice,
                DepositAmount = productVariant.Product.DepositAmount,
                AmountToPay = salePrice - oldProductVariant.Product.MinTradeInPrice - productVariant.Product.DepositAmount
            };
            return Result<CalculateTradeInOrderPriceResponse>.Success(calculateResponse);
        }

        public async Task<Result<TradeInOrderDetailResponse>> GetOrderDetailByIdAsync(Guid tradeInOrderId)
        {
            var tradeInOrder = await _tradeInOrderRepository.GetOrderDetailById(tradeInOrderId);
            if (tradeInOrder == null)
            {
                return Result<TradeInOrderDetailResponse>.Failure("TradeInOrder not found", 404);
            }
            var response = _mapper.Map<TradeInOrderDetailResponse>(tradeInOrder);
            response.NewProductVariantUrl = tradeInOrder.ProductVariant?.Product?.Assets?.FirstOrDefault()?.Url ?? string.Empty;
            response.OldProductVariantUrl = tradeInOrder.OrderItem.ProductVariant?.Product?.Assets?.FirstOrDefault()?.Url ?? string.Empty;
            //với mỗi payment type lấy ra cái mới nhất
            response.Payments = response.Payments.GroupBy(p => p.PaymentType).Select(g => g.OrderByDescending(p => p.CreatedAt).First()).ToList();
            return Result<TradeInOrderDetailResponse>.Success(response);
        }

        public async Task<Result<PaginatedList<TradeInOrderSummaryResponse>>> GetMyOrdersAsync(Guid customerId, int pageNumber, int pageSize)
        {
            var tradeInOrders = await _tradeInOrderRepository.GetMyOrdersAsync(customerId, pageNumber, pageSize);
            
            var response = _mapper.Map<List<TradeInOrderSummaryResponse>>(tradeInOrders.Items);
            var paginatedResponse = new PaginatedList<TradeInOrderSummaryResponse>(response, tradeInOrders.TotalCount, pageNumber, pageSize);
            return Result<PaginatedList<TradeInOrderSummaryResponse>>.Success(paginatedResponse);
        }
        public async Task<Result<PaginatedList<TradeInOrderSummaryResponse>>> AdminSearchTradeInOrder(Guid? customerId, Guid? productVariantId, TradeInOrderStatus? status, bool? isGood, decimal? tradeInPrice, decimal? amountToPay, decimal? depositAmount, string? phoneNumber, int pageNumber, int pageSize)
        {
            var tradeInOrders = await _tradeInOrderRepository.AdminSearchOrderAsync(customerId, productVariantId, status, isGood, tradeInPrice, amountToPay, depositAmount, phoneNumber, pageNumber, pageSize);
            var response = _mapper.Map<List<TradeInOrderSummaryResponse>>(tradeInOrders.Items);
            var paginatedResponse = new PaginatedList<TradeInOrderSummaryResponse>(response, tradeInOrders.TotalCount, pageNumber, pageSize);
            return Result<PaginatedList<TradeInOrderSummaryResponse>>.Success(paginatedResponse);
        }
        public async Task<Result<PaginatedList<TradeInOrderSummaryResponse>>> GetWaitingOrdersAsync(int pageNumber, int pageSize)
        {
            var tradeInOrders = await _tradeInOrderRepository.GetWaitingOrdersAsync(pageNumber, pageSize);
            var response = _mapper.Map<List<TradeInOrderSummaryResponse>>(tradeInOrders.Items);
            var paginatedResponse = new PaginatedList<TradeInOrderSummaryResponse>(response, tradeInOrders.TotalCount, pageNumber, pageSize);
            return Result<PaginatedList<TradeInOrderSummaryResponse>>.Success(paginatedResponse);
        }

        public async Task<Result> CancelAsync(Guid tradeInOrderId, bool isAdmin)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var tradeInOrder = await _tradeInOrderRepository.GetTradeInByIdAsync(tradeInOrderId);
                if (tradeInOrder == null)
                {
                    return Result.Failure("TradeInOrder not found", 404);
                }
                var firstStatus = tradeInOrder.Status;
                // Define allowed statuses
                var cancellableStatuses = new List<TradeInOrderStatus>
                {
                    TradeInOrderStatus.Pending,
                    TradeInOrderStatus.WAITING_FOR_STAFF,
                    TradeInOrderStatus.NEGOTIATING,
                    TradeInOrderStatus.CONFIRMED
                };

                //Try update status at DB level (ANTI RACE CONDITION), lúc này gọi inventory sẽ ko bị double nếu có race condition
                var statusToUpdate = isAdmin ? TradeInOrderStatus.ADMINCANCELLED : TradeInOrderStatus.CANCELLED;
                var affected = await _tradeInOrderRepository.UpdateStatusIfMatch(
                    tradeInOrderId,
                    statusToUpdate,
                    cancellableStatuses
                );

                if (affected == 0)
                {
                    return Result.Failure("Order already updated or not cancellable", 409);
                }


                // Check if deposit was paid
                var lastPaymentPaid = tradeInOrder.Payments
                    .Where(p => p.PaymentType == PaymentType.Deposit && p.Status == PaymentStatus.Paid)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();
                var isLastPaymentFailed = tradeInOrder.Payments
                    .Where(p => p.PaymentType == PaymentType.Deposit)
                    .OrderByDescending(p => p.CreatedAt).Select(p => p.Status).FirstOrDefault() == PaymentStatus.Failed;


                var preConfirmedStatuses = new[] {
                    TradeInOrderStatus.Pending,
                    TradeInOrderStatus.NEGOTIATING,
                    TradeInOrderStatus.WAITING_FOR_STAFF
                };

                bool isRefund = preConfirmedStatuses.Contains(firstStatus);

                //Nếu đã trả tiền và  tradeInOrder firstStatus là các status trước CONFIRMED thì tạo payment REFUND
                //luồng refund (bỏ)
                //if (lastPaymentPaid != null && isRefund)
                //{
                //    var paymentRefund = new Payment
                //    {
                //        TradeInOrderId = tradeInOrder.TradeInOrderId,
                //        Amount = lastPaymentPaid.Amount,
                //        OrderCode = tradeInOrder.OrderCode,
                //        PaymentType = PaymentType.Refund,
                //        PaymentMethod = lastPaymentPaid.PaymentMethod,
                //        Status = PaymentStatus.Refunded,
                //        Description = $"Refund for cancelled TradeInOrder {tradeInOrder.OrderCode}",
                //    };
                //    tradeInOrder.Status = TradeInOrderStatus.REFUNDED;
                //    VnPaymentRefundRequest vnPayRefundRequest = new VnPaymentRefundRequest
                //    {
                //        OrderId = lastPaymentPaid.Id.ToString(),
                //        Amount = lastPaymentPaid.Amount,
                //        PaymentDate = lastPaymentPaid.CreatedAt,
                //    };
                //    //var refundResult = await _vnPayService.RefundPaymentAsync(vnPayRefundRequest);
                //    await _paymentRepository.CreateAsync(paymentRefund);
                //}

                // Update inventory và tradeInUsedAmount
                //tradeInOrder có status là pending và payment failed thì ko cộng inventory và trừ TradeInUsedAmount vì đơn failed đã trừ tồn và trừ TradeInUsedAmount rồi
                var isStockIncreased = true;
                if (!(firstStatus == TradeInOrderStatus.Pending && isLastPaymentFailed))
                {
                    var inventory = tradeInOrder.ProductVariant!.Inventory;
                    // trả lượt tradein cho order item
                    var increaseResult = await _orderItemRepository.DecreaseTradeInUsedAmountAsync(tradeInOrder.OrderItem.Id);
                    if (!increaseResult)
                    {
                        return Result.Failure("Failed to decrease TradeInUsedAmount, please try again", 500);
                    }
                    var inventoryResult = await _inventoryRepository.IncreaseInventoryStock(tradeInOrder.ProductVariantId);
                    if (inventoryResult == 0)
                    {
                        isStockIncreased = false;
                        return Result.Failure("Failed to update inventory", 500);
                    }
                }
                // Nếu đã confirm thì hủy task giao hàng nếu có
                if (firstStatus == TradeInOrderStatus.CONFIRMED)
                {
                    tradeInOrder.ShippingTasks.ToList().ForEach(st =>
                    {
                        st.Status = ShippingTaskStatus.Cancelled;
                        _shippingTaskRepository.UpdateEntity(st);
                    });
                    await _unitOfWork.SaveChangeAsync();
                    //ko save(fixed)
                }
                
                await transaction.CommitAsync();
                if(isStockIncreased == true)
                {
                    var log = new AuditLog
                    {
                        ActionType = "Cancel TradeInOrder",
                        Message = $"TradeInOrder {tradeInOrder.TradeInOrderId} cancelled. Inventory for ProductVariant {tradeInOrder.ProductVariantId} increased.",
                        UserId = tradeInOrder.CustomerId,
                        UserRole = DAL.Constants.Role.User
                    };
                    _hangFireService.Enqueue<AuditLogService>(job => job.LogAsync(log));
                }
                var notification = new Notification
                {
                    UserId = tradeInOrder.CustomerId,
                    ActionType = "Trade-in Order Cancel",
                    Message = $"Your trade-in order {tradeInOrderId} has been cancelled",
                };
                _hangFireService.Enqueue<NotificationService>(job => job.SendNotificationAsync(notification));
                return Result.Success("Trade-in order cancelled successfully");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Result> ConfirmAsync(Guid tradeInOrderId, decimal tradeInPrice)
        {
            var tradeInOrder = await _tradeInOrderRepository.GetTradeInByIdAsync(tradeInOrderId);
            //check if trade-in order exists
            if (tradeInOrder == null)
            {
                return Result.Failure("TradeInOrder not found", 404);
            }
            //chỉ có những order ở trạng thái NEGOTIATING mới được confirm
            if (tradeInOrder.Status != TradeInOrderStatus.NEGOTIATING)
            {
                return Result.Failure("Only orders in NEGOTIATING status can be confirmed", 400);
            }
            //check if the trade-in price is lower than the minimum price
            if (tradeInPrice < tradeInOrder.ProductVariant.Product.MinTradeInPrice)
            {
                return Result.Failure("Trade-in price cannot be lower than the minimum price", 400);
            }
            tradeInOrder.Status = TradeInOrderStatus.CONFIRMED;
            tradeInOrder.TradeInPrice = tradeInPrice;
            tradeInOrder.AmountToPay = tradeInOrder.ProductVariant!.BasePrice - tradeInPrice - tradeInOrder.DepositAmount;
            Payment payment = new Payment
            {
                TradeInOrderId = tradeInOrder.TradeInOrderId,
                Amount = tradeInOrder.AmountToPay,
                OrderCode = tradeInOrder.OrderCode,
                PaymentType = PaymentType.Purchase,
                PaymentMethod = PaymentMethod.COD,
                Status = PaymentStatus.COD,
                Description = $"Final payment for TradeInOrder {tradeInOrder.OrderCode}",
            };
            _paymentRepository.AddEntity(payment);
            _tradeInOrderRepository.UpdateEntity(tradeInOrder);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result == 0)
            {
                return Result.Failure("Failed to confirm trade-in order", 500);
            }
            var notification = new Notification
            {
                UserId = tradeInOrder.CustomerId,
                ActionType = "Trade-in Order Confirmed",
                Message = $"Your trade-in order {tradeInOrderId} has been confirmed with trade-in price:{tradeInPrice}.",
            };
            _hangFireService.Enqueue<NotificationService>(job => job.SendNotificationAsync(notification));
            return Result.Success("Trade-in order confirmed successfully");
        }

        public async Task<Result> ProcessingAsync(Guid tradeInOrderId, Guid staffId, DateTime shippingDate)
        {
            var tradeInOrder = await _tradeInOrderRepository.GetTradeInByIdAsync(tradeInOrderId);
            if(tradeInOrder == null)
            {
                return Result.Failure("TradeInOrder not found", 404);
            }
            //chỉ có những order ở trạng thái CONFIRMED và shipping_replacement mới được chuyển sang PROCESSING
            if (tradeInOrder.Status != TradeInOrderStatus.CONFIRMED && tradeInOrder.Status != TradeInOrderStatus.Shipping_Replacement)
            {
                return Result.Failure("Only orders in CONFIRMED status can be moved to PROCESSING", 400);
            }
            var shippingTask = tradeInOrder.ShippingTasks.FirstOrDefault(st => st.Status == ShippingTaskStatus.Pending);
            if (shippingTask == null)
            {
                return Result.Failure("No pending shipping task found for this order", 404);
            }
            if (shippingTask.StaffId != staffId)
            {
                return Result.Failure("You are not assigned to this shipping task", 403);
            }
            if (shippingDate < DateTime.UtcNow)
            {
                return Result.Failure("Shipping date cannot be in the past", 400);
            }

            //update shipping task shipping date 
            shippingTask.ShippingDate = shippingDate;
            //ko save(fixed)
            _shippingTaskRepository.UpdateEntity(shippingTask);
            //update tradein order status
            tradeInOrder.Status = TradeInOrderStatus.PROCESSING;
            _tradeInOrderRepository.UpdateEntity(tradeInOrder);
            var result = await _unitOfWork.SaveChangeAsync();
            if (result == 0)
            {
                return Result.Failure("Failed to update trade-in order status", 500);
            }
            var notification = new Notification
            {
                UserId = tradeInOrder.CustomerId,
                ActionType = "Trade-in Order Processing",
                Message = $"Your trade-in order {tradeInOrderId} has been Processing by delivery staff",
            };
            _hangFireService.Enqueue<NotificationService>(job => job.SendNotificationAsync(notification));
            return Result.Success("Trade-in order status updated to PROCESSING successfully");
        }

        public async Task<Result> DeliveredAsync(Guid tradeInOrderId)
        {
            var tradeInOrder = await _tradeInOrderRepository.GetByIdAsync(tradeInOrderId);
            if (tradeInOrder == null)
            {
                return Result.Failure("TradeInOrder not found", 404);
            }
            if (tradeInOrder.Status != TradeInOrderStatus.PROCESSING)
            {
                return Result.Failure("Only orders in PROCESSING status can be moved to DELIVERED", 400);
            }
            tradeInOrder.Status = TradeInOrderStatus.DELIVERED;
            var result = await _tradeInOrderRepository.UpdateAsync(tradeInOrder);
            if (result == 0)
            {
                return Result.Failure("Failed to update trade-in order status", 500);
            }
            var notification = new Notification
            {
                UserId = tradeInOrder.CustomerId,
                ActionType = "Trade-in Order Delivered",
                Message = $"Your trade-in order {tradeInOrderId} has been delivered",
            };
            _hangFireService.Enqueue<NotificationService>(job => job.SendNotificationAsync(notification));
            return Result.Success("Trade-in order status updated to DELIVERED successfully");
        }

        public async Task<Result> CompletedAsync(Guid tradeInOrderId)
        {
            var tradeInOrder = await _tradeInOrderRepository.GetTradeInByIdAsync(tradeInOrderId);
            if (tradeInOrder == null)
            {
                return Result.Failure("TradeInOrder not found", 404);
            }
            if (tradeInOrder.Status != TradeInOrderStatus.DELIVERED)
            {
                return Result.Failure("Only orders in DELIVERED status can be moved to COMPLETED", 400);
            }
            tradeInOrder.Status = TradeInOrderStatus.COMPLETED;
            _tradeInOrderRepository.UpdateEntity(tradeInOrder);
            var lastFinalPayment = tradeInOrder.Payments.Where(p => p.PaymentType == PaymentType.Purchase && p.Status == PaymentStatus.COD).OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (lastFinalPayment == null)
            {
                return Result.Failure("Final payment not found for this order", 404);
            }
            //ko save(fixed)
            lastFinalPayment.Status = PaymentStatus.CODPaid;
            lastFinalPayment.UpdatedAt = DateTime.UtcNow;
            _paymentRepository.UpdateEntity(lastFinalPayment); _paymentRepository.UpdateEntity(lastFinalPayment);

            //award coins for customer
            if (tradeInOrder.Status == TradeInOrderStatus.COMPLETED)
            {
                var customer = await _customerRepository.GetByIdAsync(tradeInOrder.CustomerId);
                if (customer != null)
                {
                    var config = await _systemConfigRepository.GetByKeyAsync("OrderCoinPercent");
                    decimal percent = 1.0m; // default 1%
                    if (config != null && decimal.TryParse(config.ConfigValue, out decimal parsed))
                    {
                        percent = parsed;
                    }
                    int coinsEarned = (int)(tradeInOrder.AmountToPay * percent / 100);
                    customer.MemberCoin += coinsEarned;
                    _customerRepository.UpdateEntity(customer);
                }
            }

                var result = await _unitOfWork.SaveChangeAsync();
            if (result == 0)
            {
                return Result.Failure("Failed to update trade-in order status", 500);
            }
            var notification = new Notification
            {
                UserId = tradeInOrder.CustomerId,
                ActionType = "Trade-in Order Delivered",
                Message = $"Your trade-in order {tradeInOrderId} has been completed",
            };
            _hangFireService.Enqueue<NotificationService>(job => job.SendNotificationAsync(notification));
            return Result.Success("Trade-in order status updated to COMPLETED successfully");
        }

        public async Task<Result> CreateConversationAsync(Guid tradeInOrderId, Guid staffId)
        {
           var updateResult = await _conversationRepository.UpdateNegotiatingAsync(tradeInOrderId, staffId);
           if (updateResult == 0)
           {
                return Result.Failure("Only orders in WAITING_FOR_STAFF status can be created", 400);
           }
           var tradeInOrder = await _tradeInOrderRepository.GetByIdAsync(tradeInOrderId);
           if(tradeInOrder == null)
           {
                return Result.Failure("TradeInOrder not found", 404);
           }
            var conversation = new Conversation
            {
                TradeInOrderId = tradeInOrderId,
                StaffId = staffId,
                CreatedAt = DateTime.UtcNow,
                CustomerId = tradeInOrder.CustomerId
            };
            var result = await _conversationRepository.CreateAsync(conversation);
            if (result == 0)
            {
                return Result.Failure("Failed to create conversation", 500);
            }
            var notification = new Notification
            {
                UserId = tradeInOrder.CustomerId,
                ActionType = "Trade-in Order Delivered",
                Message = $"Please join conversation to negotiating trade in price with our seller about your trade-in order {tradeInOrderId}",
            };
            _hangFireService.Enqueue<NotificationService>(job => job.SendNotificationAsync(notification));
            return Result.Success("Conversation created successfully");
        }

        public async Task<Result<TradeInOrderDashBoardResponse>> GetTradeInDashBoardAsync(DateOnly fromDate, DateOnly toDate)
        {
            var from = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var to = toDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
            var data = await _tradeInOrderRepository.GetTradeInOrderDashBoardAsync(from, to);
            if (data == null || !data.Any())
            {
                return Result<TradeInOrderDashBoardResponse>.Success(new TradeInOrderDashBoardResponse());
            }
            decimal totalAmount = 0;
            decimal totalDepositAmount = 0;
            decimal totalCODAmount = 0;
            decimal totalRefundAmount = 0;
            decimal totalVnPayAmount = 0;
            decimal totalPurchaseAmount = 0;
            foreach (var item in data)
            {
                if (item.Payments == null || !item.Payments.Any())
                {
                    continue;
                }
                item.Payments.ForEach(p =>
                {
                    if (p.PaymentType == PaymentType.Purchase && p.Status == PaymentStatus.CODPaid)
                    {
                        totalAmount += p.Amount;
                        totalPurchaseAmount += p.Amount;
                        totalCODAmount += p.Amount;
                    }
                    if (p.PaymentType == PaymentType.Deposit && p.Status == PaymentStatus.Paid)
                    {
                        totalAmount += p.Amount;
                        totalVnPayAmount += p.Amount;
                        totalDepositAmount += p.Amount;
                    }
                    if (p.PaymentType == PaymentType.Refund && p.Status == PaymentStatus.Refunded)
                    {

                        totalRefundAmount += p.Amount;
                    }
                });
            }
            var response = new TradeInOrderDashBoardResponse
            {
                TotalTradeInOrders = data.Count,
                TotalCompletedTradeInOrders = data.Where(ti => ti.Status == TradeInOrderStatus.COMPLETED).Count(),
                TotalCancelledTradeInOrders = data.Where(t1 => t1.Status == TradeInOrderStatus.CANCELLED || t1.Status == TradeInOrderStatus.FORCED_CANCELLED || t1.Status == TradeInOrderStatus.ADMINCANCELLED).Count(),
                TotalRefundedTradeInOrders = data.Where(ti => ti.Status == TradeInOrderStatus.REFUNDED || ti.Status == TradeInOrderStatus.RefundedAndDamaged || ti.Status == TradeInOrderStatus.RefundedAndRestocked).Count(),
                TotalAmount = totalAmount,
                TotalDepositAmount = totalDepositAmount,
                TotalCODAmount = totalCODAmount,
                TotalRefundAmount = totalRefundAmount,
                TotalVnPayAmount = totalVnPayAmount,
                TotalPurchaseAmount = totalPurchaseAmount,
                FromDate = fromDate,
                ToDate = toDate,
            };
            return Result<TradeInOrderDashBoardResponse>.Success(response);
        }
    }
}
