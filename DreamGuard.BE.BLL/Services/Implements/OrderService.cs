using System;
using System.Collections.Generic;
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
using DreamGuard.BE.DAL.Repositories.Interfaces;

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
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductVariantRepository variantRepository,
            IComboRepository comboRepository,
            IInventoryRepository inventoryRepository,
            IAddressRepository addressRepository,
            IUserVoucherRepository userVoucherRepository,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _variantRepository = variantRepository;
            _comboRepository = comboRepository;
            _inventoryRepository = inventoryRepository;
            _addressRepository = addressRepository;
            _userVoucherRepository = userVoucherRepository;
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request)
        {
            // Load cart with items
            var cart = await _cartRepository.GetCartWithItemsAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                return Result<OrderResponse>.Failure("Cart is empty.", 400);
            }

            // Validate address belongs to user
            var address = await _addressRepository.GetByIdAsync(userId, request.AddressId);
            if (address == null)
            {
                return Result<OrderResponse>.Failure("Address not found.", 404);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {   
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                // Validate and prepare each cart item
                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.ProductVariantId.HasValue)
                    {
                        var variant = await _variantRepository.GetVariantByIdAsync(cartItem.ProductVariantId.Value);
                        if (variant == null || variant.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(
                                $"Product variant '{cartItem.ProductVariantId}' is no longer available.", 400);
                        }

                        var inventory = await _inventoryRepository.GetInventoryByVariantIdAsync(cartItem.ProductVariantId.Value);
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

                        var productName = cartItem.ProductVariant?.Product?.Name ?? "Unknown";
                        var itemPrice = variant.SalePrice;
                        orderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = cartItem.ProductVariantId,
                            ComboId = null,
                            Quantity = cartItem.Quantity,
                            UnitPrice = itemPrice,
                            TotalPrice = itemPrice * cartItem.Quantity,
                            ItemName = $"{productName} - {variant.Size}"
                        });
                        subTotal += itemPrice * cartItem.Quantity;
                    }
                    else if (cartItem.ComboId.HasValue)
                    {
                        var combo = await _comboRepository.GetComboWithProductsAsync(cartItem.ComboId.Value);
                        if (combo == null || combo.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure(
                                $"Combo '{cartItem.ComboId}' is no longer available.", 400);
                        }

                        // Validate combo stock
                        var comboStock = CalculateComboStock(combo.ComboProductVariants);
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

                    if (userVoucher.UserId != userId)
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

                    // Mark voucher as used
                    userVoucher.IsUsed = true;
                    userVoucher.UsedAt = DateTime.UtcNow;
                    await _userVoucherRepository.UpdateAsync(userVoucher);
                }

                var totalAmount = Math.Max(0, subTotal - discountAmount);

                // Generate order code
                var orderCode = GenerateOrderCode();

                // Create order with address snapshot
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
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

                // Clear cart
                await _cartRepository.ClearCartItemsAsync(cart.Id);

                await transaction.CommitAsync();

                return Result<OrderResponse>.Success(new OrderResponse
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    Status = order.Status,
                    SubTotal = order.SubTotal,
                    DiscountAmount = order.DiscountAmount,
                    TotalAmount = order.TotalAmount,
                    CreatedAt = order.CreatedAt
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<OrderResponse>.Failure("Failed to create order.", 500);
            }
        }

        public async Task<Result<OrderDetailResponse>> GetOrderByIdAsync(Guid userId, Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if (order == null)
            {
                return Result<OrderDetailResponse>.Failure("Order not found.", 404);
            }

            if (order.UserId != userId)
            {
                return Result<OrderDetailResponse>.Failure("Order not found.", 404);
            }

            return Result<OrderDetailResponse>.Success(MapToDetailResponse(order));
        }

        public async Task<Result<PaginatedList<OrderSummaryResponse>>> GetOrdersAsync(
            Guid userId, int pageNumber, OrderStatus? status)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId, pageNumber, status);

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
            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(orderId);
            if (order == null)
            {
                return Result.Failure("Order not found.", 404);
            }

            if (order.UserId != userId)
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
                    TotalPrice = oi.TotalPrice
                }).ToList() ?? new List<OrderItemResponse>(),
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                VoucherCode = order.UserVoucher?.Voucher?.Code,
                VoucherDiscountValue = order.UserVoucher?.Voucher?.DiscountValue,
                Note = order.Note,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        private static int CalculateComboStock(List<ComboProductVariant> comboProductVariants)
        {
            if (comboProductVariants == null || !comboProductVariants.Any())
                return 0;

            int minStock = int.MaxValue;
            foreach (var cpv in comboProductVariants)
            {
                if (cpv.Quantity <= 0) continue;
                int inventoryQuantity = cpv.ProductVariant?.Inventory?.Quantity ?? 0;
                int possibleSets = inventoryQuantity / cpv.Quantity;
                minStock = Math.Min(minStock, possibleSets);
            }

            return minStock == int.MaxValue ? 0 : minStock;
        }
    }
}
