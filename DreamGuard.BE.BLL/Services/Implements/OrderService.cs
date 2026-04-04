using CloudinaryDotNet.Actions;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.BLL.Utilities;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.ModelExtensions;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IComboRepository _comboRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductVariantRepository variantRepository,
            IComboRepository comboRepository,
            IInventoryRepository inventoryRepository,
            IAddressRepository addressRepository,
            IUserVoucherRepository userVoucherRepository,
            IInventoryService inventoryService,
            IPaymentRepository paymentRepository,
            IVnPayService vnPayService,
            IUnitOfWork unitOfWork,
            ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _variantRepository = variantRepository;
            _comboRepository = comboRepository;
            _inventoryRepository = inventoryRepository;
            _addressRepository = addressRepository;
            _userVoucherRepository = userVoucherRepository;
            _inventoryService = inventoryService;
            _paymentRepository = paymentRepository;
            _vnPayService = vnPayService;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
        }

        public async Task<Result<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request, string ipAddress)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<OrderResponse>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            // Load cart with items
            var cart = await _cartRepository.GetCartWithItemsAsync(customerId);
            if (cart == null || !cart.CartItems.Any())
            {
                return Result<OrderResponse>.Failure("Cart is empty.", 400);
            }

            // Validate address belongs to user
            var address = await _addressRepository.GetByIdAsync(customerId, request.AddressId);
            if (address == null)
            {
                return Result<OrderResponse>.Failure("Address not found.", 404);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                // Pre-load all variants and inventories
                var variantIds = cart.CartItems
                    .Where(ci => ci.ProductVariantId.HasValue)
                    .Select(ci => ci.ProductVariantId!.Value)
                    .Distinct()
                    .ToList();
                
                var comboIds = cart.CartItems
                    .Where(ci => ci.ComboId.HasValue)
                    .Select(ci => ci.ComboId!.Value)
                    .Distinct()
                    .ToList();

                var variantsDict = (await _variantRepository.GetVariantsByIdsAsync(variantIds))
                    .ToDictionary(v => v.Id);
                var inventoriesDict = (await _inventoryRepository.GetInventoriesByVariantIdsAsync(variantIds))
                    .ToDictionary(i => i.ProductVariantId);
                var combosDict = (await _comboRepository.GetCombosWithProductsByIdsAsync(comboIds))
                    .ToDictionary(c => c.Id);

                // Validate and prepare each cart item
                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.ProductVariantId.HasValue)
                    {
                        variantsDict.TryGetValue(cartItem.ProductVariantId.Value, out var variant);
                        if (variant == null || variant.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(
                                $"Product variant '{cartItem.ProductVariantId}' is no longer available.", 400);
                        }

                        inventoriesDict.TryGetValue(cartItem.ProductVariantId.Value, out var inventory);
                        if (inventory == null || inventory.Quantity < cartItem.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(
                                $"Insufficient stock for product variant '{variant.Sku}'. Available: {inventory?.Quantity ?? 0}, Requested: {cartItem.Quantity}.", 400);
                        }

                        // Deduct stock
                        var deductResult = await _inventoryService.DeductVariantStockAsync(
                            cartItem.ProductVariantId.Value, cartItem.Quantity);
                        if (!deductResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(deductResult.Error!, deductResult.StatusCode);
                        }

                        // Update RAM inventory state for next consecutive identical items
                        if (inventory != null)
                        {
                            inventory.Quantity -= cartItem.Quantity;
                        }

                        var productName = cartItem.ProductVariant?.Product?.Name ?? "Unknown";
                        var itemPrice = variant.SalePrice;
                        var itemAddonPrice = cartItem.ProductCustomizeDetails?.Sum(d => d.AddOnPrice) ?? 0;
                        orderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = cartItem.ProductVariantId,
                            ComboId = null,
                            Quantity = cartItem.Quantity,
                            UnitPrice = itemPrice,
                            TotalPrice = itemPrice * cartItem.Quantity,
                            ItemName = $"{productName} - {variant.Size}",
                            CustomizeHash = cartItem.CustomizeHash,
                            ProductCustomizeDetails = cartItem.ProductCustomizeDetails?.Select(d => new ProductCustomizeDetail
                            {
                                CustomizeTypeName = d.CustomizeTypeName,
                                CustomizeContent = d.CustomizeContent,
                                AddOnPrice = d.AddOnPrice
                            }).ToList() ?? new List<ProductCustomizeDetail>()
                        });
                        subTotal += itemPrice * cartItem.Quantity;
                    }
                    else if (cartItem.ComboId.HasValue)
                    {
                        combosDict.TryGetValue(cartItem.ComboId.Value, out var combo);
                        if (combo == null || combo.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(
                                $"Combo '{cartItem.ComboId}' is no longer available.", 400);
                        }

                        // Validate combo stock
                        var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
                        if (comboStock < cartItem.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(
                                $"Insufficient stock for combo '{combo.Name}'. Available: {comboStock}, Requested: {cartItem.Quantity}.", 400);
                        }

                        // Deduct combo stock
                        var deductResult = await _inventoryService.DeductComboStockAsync(
                            cartItem.ComboId.Value, cartItem.Quantity);
                        if (!deductResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(deductResult.Error!, deductResult.StatusCode);
                        }

                        var itemPrice = combo.SalePrice;
                        orderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = null,
                            ComboId = cartItem.ComboId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = itemPrice,
                            TotalPrice = itemPrice * cartItem.Quantity,
                            ItemName = combo.Name
                        });
                        subTotal += itemPrice * cartItem.Quantity;
                    }
                }

                // Handle voucher
                decimal discountAmount = 0;
                UserVoucher? userVoucher = null;

                if (request.UserVoucherId.HasValue)
                {
                    userVoucher = await _userVoucherRepository.GetByIdAsync(request.UserVoucherId.Value);
                    if (userVoucher == null)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("Voucher not found.", 404);
                    }

                    if (userVoucher.CustomerId != customerId)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("Voucher does not belong to this user.", 403);
                    }

                    if (userVoucher.IsUsed)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("Voucher has already been used.", 400);
                    }

                    if (userVoucher.ExpiredAt < DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("Voucher has expired.", 400);
                    }

                    // Load voucher details
                    var voucher = userVoucher.Voucher;
                    if (voucher == null || !voucher.IsActive || voucher.EndDate < DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("Voucher is no longer active.", 400);
                    }

                    // Calculate discount
                    discountAmount = subTotal * voucher.DiscountValue;
                    discountAmount = Math.Max(voucher.MinDiscountAmount, Math.Min(discountAmount, voucher.MaxDiscountAmount));
                    discountAmount = Math.Min(discountAmount, subTotal); // Discount cannot exceed subtotal

                    // Mark voucher as used
                    userVoucher.IsUsed = true;
                    userVoucher.UsedAt = DateTime.UtcNow;
                    await _userVoucherRepository.UpdateAsync(userVoucher);
                }

                // Calculate TotalAddonPrice from cart items' customize details
                decimal totalAddonPrice = 0;
                var allCustomizeDetails = new List<ProductCustomizeDetail>();
                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.ProductCustomizeDetails != null && cartItem.ProductCustomizeDetails.Any())
                    {
                        var itemAddon = cartItem.ProductCustomizeDetails.Sum(d => d.AddOnPrice) * cartItem.Quantity;
                        totalAddonPrice += itemAddon;
                        allCustomizeDetails.AddRange(cartItem.ProductCustomizeDetails);
                    }
                }

                var totalAmount = Math.Max(0, subTotal + totalAddonPrice - discountAmount);

                // Generate order code
                var orderCode = GenerateOrderCode();

                // Create order with address snapshot
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    OrderCode = orderCode,
                    Status = OrderStatus.Pending,
                    ReceiverName = address.ReceiverName,
                    PhoneNumber = address.PhoneNumber,
                    Street = address.Street,
                    City = address.City,
                    District = address.District,
                    Ward = address.Ward,
                    Province = address.Province,
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    TotalAddonPrice = totalAddonPrice,
                    UserVoucherId = request.UserVoucherId,
                    Note = request.Note,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _orderRepository.CreateAsync(order);
                if (createResult < 0)
                {
                    await transaction.RollbackAsync();
                    return Result<OrderResponse>.Failure("Failed to create order.", 500);
                }

                // Set OrderId for items and save
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                }
                await _orderRepository.AddOrderItemsAsync(orderItems);

                // Create Payment record
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderCode = order.OrderCode,
                    POrderId = order.Id,
                    Status = PaymentStatus.Pending,
                    Amount = totalAmount,
                    Description = $"Payment for Order {order.OrderCode}",
                    PaymentMethod = request.PaymentMethod,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var paymentCreateResult = await _paymentRepository.CreateAsync(payment);
                if (paymentCreateResult < 0)
                {
                    await transaction.RollbackAsync();
                    return Result<OrderResponse>.Failure("Failed to create payment.", 500);
                }

                // Clear cart
                await _cartRepository.ClearCartItemsAsync(cart.Id);

                await transaction.CommitAsync();

                // Generate VnPay URL after commit (external call, should not be inside transaction)
                string? paymentUrl = null;
                if (request.PaymentMethod == PaymentMethod.VnPay)
                {
                    var vnPayRequest = new VnPaymentRequest
                    {
                        PaymentId = payment.Id.ToString(),
                        OrderCode = order.OrderCode,
                        Description = payment.Description,
                        Amount = totalAmount,
                        IpAddress = ipAddress,
                        CreatedDate = payment.CreatedAt
                    };
                    paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest);
                }

                return Result<OrderResponse>.Success(new OrderResponse
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    Status = order.Status,
                    SubTotal = order.SubTotal,
                    DiscountAmount = order.DiscountAmount,
                    TotalAmount = order.TotalAmount,
                    TotalAddonPrice = order.TotalAddonPrice,
                    PaymentMethod = request.PaymentMethod,
                    PaymentUrl = paymentUrl,
                    CreatedAt = order.CreatedAt
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<OrderResponse>.Failure("Failed to create order.", 500);
            }
        }

        public async Task<Result<OrderDetailResponse>> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null)
            {
                return Result<OrderDetailResponse>.Failure("Order not found.", 404);
            }

            return Result<OrderDetailResponse>.Success(MapToDetailResponse(order));
        }

        public async Task<Result<PaginatedList<OrderSummaryResponse>>> GetOrdersAsync(
            Guid userId, int pageNumber, OrderStatus? status)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<PaginatedList<OrderSummaryResponse>>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId, pageNumber, status);

            var responses = orders.Items.Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                Status = o.Status,
                ItemCount = o.OrderItems?.Count ?? 0,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            }).ToList();

            return Result<PaginatedList<OrderSummaryResponse>>.Success(
                new PaginatedList<OrderSummaryResponse>(
                    responses, orders.TotalCount, orders.PageNumber, orders.PageSize));
        }
        public async Task<Result<List<OrderItemResponse>>> GetOrdersToTradeInAsync(Guid customerId, Guid productVariantId)
        {
            //check if it is customer
            var customer = await _customerRepository.GetByUserIdAsync(customerId);
            if (customer == null)
                return Result<List<OrderItemResponse>>.Failure("Customer profile not found.", 404);
            //check if product variant exists
            var variant = await _variantRepository.GetVariantByIdAsync(productVariantId);
            if (variant == null)
                return Result<List<OrderItemResponse>>.Failure("Product variant not found.", 404);
            //check if product variant has category parent
            var categoryParentId = variant.Product!.Category!.CateParentId;
            if (categoryParentId == null)
                return Result<List<OrderItemResponse>>.Failure("Product category parent not found.", 404);
            var basePriceWithDepositReduce = variant.BasePrice - variant.Product.DepositAmount;
            var orderItems = await _orderRepository.GetOrdersToTradeInAsync(customerId, categoryParentId.Value, basePriceWithDepositReduce);
            var orderItemResponseList = orderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductVariantId = oi.ProductVariantId,
                ComboId = oi.ComboId,
                ItemName = oi.ItemName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice,
                CustomizeHash = oi.CustomizeHash,
                IsTradeInUsed = oi.IsTradeInUsed,
                ProductCustomizeDetails = oi.ProductCustomizeDetails?.Select(d => new ProductCustomizeDetail
                {
                    CustomizeTypeName = d.CustomizeTypeName,
                    CustomizeContent = d.CustomizeContent,
                    AddOnPrice = d.AddOnPrice
                }).ToList() ?? new List<ProductCustomizeDetail>()
            }).ToList();
            return Result<List<OrderItemResponse>>.Success(orderItemResponseList);
        }

        public async Task<Result<PaginatedList<OrderSummaryResponse>>> GetAllOrdersForAdminAsync(
            int pageNumber, OrderStatus? status, string? orderCode)
        {
            var orders = await _orderRepository.GetAllOrdersForAdminAsync(pageNumber, status, orderCode);

            var responses = orders.Items.Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                Status = o.Status,
                ItemCount = o.OrderItems?.Count ?? 0,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            }).ToList();

            return Result<PaginatedList<OrderSummaryResponse>>.Success(
                new PaginatedList<OrderSummaryResponse>(
                    responses, orders.TotalCount, orders.PageNumber, orders.PageSize));
        }


        public async Task<Result> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(orderId);
            if (order == null)
            {
                return Result.Failure("Order not found.", 404);
            }

            // Validate status transition
            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                return Result.Failure(
                    $"Cannot transition from '{order.Status}' to '{newStatus}'.", 400);
            }

            // If cancelling, restore stock and voucher
            if (newStatus == OrderStatus.Cancelled)
            {
                return await CancelOrderInternalAsync(order);
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            return Result.Success($"Order status updated to '{newStatus}'.");
        }

        public async Task<Result> CancelOrderAsync(Guid userId, Guid orderId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(orderId);
            if (order == null)
            {
                return Result.Failure("Order not found.", 404);
            }

            if (order.CustomerId != customer.CustomerId)
            {
                return Result.Failure("Order not found.", 404);
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return Result.Failure(
                    $"Cannot cancel order with status '{order.Status}'. Only Pending or Confirmed orders can be cancelled.", 400);
            }

            return await CancelOrderInternalAsync(order);
        }

        private async Task<Result> CancelOrderInternalAsync(Order order)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Restore stock for each order item
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductVariantId.HasValue)
                    {
                        var result = await _inventoryService.RestoreVariantStockAsync(
                            item.ProductVariantId.Value, item.Quantity);
                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return Result.Failure(result.Error!, result.StatusCode);
                        }
                    }
                    else if (item.ComboId.HasValue)
                    {
                        var result = await _inventoryService.RestoreComboStockAsync(
                            item.ComboId.Value, item.Quantity);
                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return Result.Failure(result.Error!, result.StatusCode);
                        }
                    }
                }

                // Restore voucher if used
                if (order.UserVoucherId.HasValue)
                {
                    var userVoucher = await _userVoucherRepository.GetByIdAsync(order.UserVoucherId.Value);
                    if (userVoucher != null)
                    {
                        userVoucher.IsUsed = false;
                        userVoucher.UsedAt = null;
                        await _userVoucherRepository.UpdateAsync(userVoucher);
                    }
                }

                // Mark associated payment as Failed
                var payment = await _paymentRepository.GetPaymentByOrderIdForUpdateAsync(order.Id);
                if (payment != null && payment.Status == PaymentStatus.Pending)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _paymentRepository.UpdateAsync(payment);
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                await transaction.CommitAsync();
                return Result.Success("Order cancelled successfully.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to cancel order.", 500);
            }
        }

        private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
        {
            return (current, next) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Processing) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.Processing, OrderStatus.Shipping) => true,
                (OrderStatus.Shipping, OrderStatus.Delivered) => true,
                (OrderStatus.Delivered, OrderStatus.Completed) => true,
                _ => false
            };
        }

        private static string GenerateOrderCode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"DG-{datePart}-{randomPart}";
        }

        private static OrderDetailResponse MapToDetailResponse(Order order)
        {
            return new OrderDetailResponse
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Status = order.Status,
                ReceiverName = order.ReceiverName,
                PhoneNumber = order.PhoneNumber,
                Street = order.Street,
                City = order.City,
                District = order.District,
                Ward = order.Ward,
                Province = order.Province,
                Items = order.OrderItems?.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductVariantId = oi.ProductVariantId,
                    ComboId = oi.ComboId,
                    ItemName = oi.ItemName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    CustomizeHash = oi.CustomizeHash,
                    IsTradeInUsed = oi.IsTradeInUsed,
                    ProductCustomizeDetails = oi.ProductCustomizeDetails?.Select(d => new ProductCustomizeDetail
                    {
                        CustomizeTypeName = d.CustomizeTypeName,
                        CustomizeContent = d.CustomizeContent,
                        AddOnPrice = d.AddOnPrice
                    }).ToList() ?? new List<ProductCustomizeDetail>()
                }).ToList() ?? new List<OrderItemResponse>(),
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                TotalAddonPrice = order.TotalAddonPrice,
                VoucherCode = order.UserVoucher?.Voucher?.Code,
                VoucherDiscountValue = order.UserVoucher?.Voucher?.DiscountValue,
                Note = order.Note,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

    }
}
