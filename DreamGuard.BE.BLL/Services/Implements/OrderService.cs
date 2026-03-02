using AutoMapper;
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
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IComboRepository _comboRepo;
        private readonly IAddressRepository _addressRepo;
        private readonly IUserVoucherRepository _userVoucherRepo;
        private readonly IVoucherRepository _voucherRepo;
        private readonly IProductVariantRepository _variantRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepo,
            ICartRepository cartRepo,
            IInventoryRepository inventoryRepo,
            IComboRepository comboRepo,
            IAddressRepository addressRepo,
            IUserVoucherRepository userVoucherRepo,
            IVoucherRepository voucherRepo,
            IProductVariantRepository variantRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
            _inventoryRepo = inventoryRepo;
            _comboRepo = comboRepo;
            _addressRepo = addressRepo;
            _userVoucherRepo = userVoucherRepo;
            _voucherRepo = voucherRepo;
            _variantRepo = variantRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<OrderResponse>> CheckoutAsync(Guid userId, CheckoutRequest request)
        {
            // 1. Get cart with items
            var cart = await _cartRepo.GetCartWithItemsAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                return Result<OrderResponse>.Failure("Cart is empty.", 400);

            // 2. Get and validate address
            var address = await _addressRepo.GetByIdAsync(userId, request.AddressId);
            if (address == null)
                return Result<OrderResponse>.Failure("Address not found.", 404);

            // 3. Validate voucher if provided
            UserVoucher? userVoucher = null;
            Voucher? voucher = null;
            if (request.UserVoucherId.HasValue)
            {
                userVoucher = await _userVoucherRepo.GetByIdAsync(request.UserVoucherId.Value);
                if (userVoucher == null || userVoucher.UserId != userId)
                    return Result<OrderResponse>.Failure("Voucher not found.", 404);
                if (userVoucher.IsUsed)
                    return Result<OrderResponse>.Failure("Voucher already used.", 400);
                if (userVoucher.ExpiredAt < DateTime.UtcNow)
                    return Result<OrderResponse>.Failure("Voucher has expired.", 400);

                voucher = await _voucherRepo.GetByIdAsync(userVoucher.VoucherId);
                if (voucher == null || !voucher.IsActive)
                    return Result<OrderResponse>.Failure("Voucher is not active.", 400);
            }

            // 4. Begin transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 5. Build OrderItems + check/deduct inventory
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.ProductVariantId.HasValue)
                    {
                        var variant = cartItem.ProductVariant;
                        if (variant == null)
                        {
                            variant = await _variantRepo.GetVariantByIdAsync(cartItem.ProductVariantId.Value);
                            if (variant == null)
                                throw new InvalidOperationException("Product variant not found.");
                        }

                        var inv = await _inventoryRepo.GetInventoryByVariantIdAsync(variant.Id);
                        if (inv == null || inv.Quantity < cartItem.Quantity)
                            throw new InvalidOperationException($"Insufficient stock for {variant.Product?.Name ?? variant.Sku ?? "product"}.");

                        inv.Quantity -= cartItem.Quantity;
                        inv.UpdatedAt = DateTime.UtcNow;
                        await _inventoryRepo.UpdateAsync(inv);

                        var unitPrice = variant.SalePrice;
                        orderItems.Add(new OrderItem
                        {
                            ProductVariantId = variant.Id,
                            ProductName = variant.Product?.Name ?? string.Empty,
                            Sku = variant.Sku,
                            UnitPrice = unitPrice,
                            Quantity = cartItem.Quantity,
                            TotalPrice = unitPrice * cartItem.Quantity
                        });
                        subTotal += unitPrice * cartItem.Quantity;
                    }
                    else if (cartItem.ComboId.HasValue)
                    {
                        var combo = await _comboRepo.GetComboWithProductsAsync(cartItem.ComboId.Value);
                        if (combo == null)
                            throw new InvalidOperationException("Combo not found.");

                        foreach (var cpv in combo.ComboProductVariants)
                        {
                            var requiredQty = cpv.Quantity * cartItem.Quantity;
                            var inv = await _inventoryRepo.GetInventoryByVariantIdAsync(cpv.ProductVariantId);
                            if (inv == null || inv.Quantity < requiredQty)
                                throw new InvalidOperationException($"Insufficient stock for variant in combo {combo.Name}.");

                            inv.Quantity -= requiredQty;
                            inv.UpdatedAt = DateTime.UtcNow;
                            await _inventoryRepo.UpdateAsync(inv);
                        }

                        var unitPrice = combo.SalePrice;
                        orderItems.Add(new OrderItem
                        {
                            ComboId = combo.Id,
                            ProductName = combo.Name,
                            UnitPrice = unitPrice,
                            Quantity = cartItem.Quantity,
                            TotalPrice = unitPrice * cartItem.Quantity
                        });
                        subTotal += unitPrice * cartItem.Quantity;
                    }
                }

                // 6. Calculate discount
                decimal discountAmount = 0;
                if (voucher != null)
                {
                    discountAmount = subTotal * voucher.DiscountValue;
                    if (discountAmount > voucher.MaxDiscountAmount)
                        discountAmount = voucher.MaxDiscountAmount;
                    if (subTotal < voucher.MinDiscountAmount)
                        discountAmount = 0;
                }

                // 7. Create Order with snapshot address
                var order = new Order
                {
                    UserId = userId,
                    ReceiverName = address.ReceiverName,
                    PhoneNumber = address.PhoneNumber,
                    Street = address.Street,
                    City = address.City,
                    District = address.District,
                    Ward = address.Ward,
                    Province = address.Province,
                    Note = request.Note,
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    TotalAmount = subTotal - discountAmount,
                    Status = OrderStatus.Pending,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = PaymentStatus.Pending,
                    UserVoucherId = request.UserVoucherId
                };
                await _orderRepo.CreateAsync(order);

                // 8. Set OrderId on all items and save
                foreach (var item in orderItems)
                    item.OrderId = order.Id;
                await _orderRepo.AddOrderItemsAsync(orderItems);

                // 9. Mark voucher as used
                if (userVoucher != null)
                {
                    userVoucher.IsUsed = true;
                    userVoucher.UsedAt = DateTime.UtcNow;
                    await _userVoucherRepo.UpdateAsync(userVoucher);
                }

                // 10. Clear cart
                await _cartRepo.RemoveAllCartItemsAsync(cart.Id);

                await transaction.CommitAsync();

                // 11. Map and return
                order.OrderItems = orderItems;
                var response = _mapper.Map<OrderResponse>(order);
                return Result<OrderResponse>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<OrderResponse>.Failure(ex.Message, 400);
            }
        }

        public async Task<Result<OrderResponse>> GetOrderByIdAsync(Guid userId, Guid orderId)
        {
            var order = await _orderRepo.GetOrderWithItemsAsync(orderId);
            if (order == null)
                return Result<OrderResponse>.Failure("Order not found.", 404);
            if (order.UserId != userId)
                return Result<OrderResponse>.Failure("You do not have permission to view this order.", 403);

            var response = _mapper.Map<OrderResponse>(order);
            return Result<OrderResponse>.Success(response);
        }

        public async Task<Result<PaginatedList<OrderListResponse>>> GetMyOrdersAsync(
            Guid userId, int pageNumber, OrderStatus? status)
        {
            var orders = await _orderRepo.GetOrdersByUserIdAsync(userId, pageNumber, status);
            if (orders.TotalCount == 0)
                return Result<PaginatedList<OrderListResponse>>.Failure("No orders found.", 404);

            var mappedItems = _mapper.Map<List<OrderListResponse>>(orders.Items);
            var paginatedResult = new PaginatedList<OrderListResponse>(
                mappedItems, orders.TotalCount, orders.PageNumber, orders.PageSize);

            return Result<PaginatedList<OrderListResponse>>.Success(paginatedResult);
        }

        public async Task<Result<PaginatedList<OrderListResponse>>> GetAllOrdersAsync(
            int pageNumber, OrderStatus? status)
        {
            var orders = await _orderRepo.GetAllOrdersAsync(pageNumber, status);
            if (orders.TotalCount == 0)
                return Result<PaginatedList<OrderListResponse>>.Failure("No orders found.", 404);

            var mappedItems = _mapper.Map<List<OrderListResponse>>(orders.Items);
            var paginatedResult = new PaginatedList<OrderListResponse>(
                mappedItems, orders.TotalCount, orders.PageNumber, orders.PageSize);

            return Result<PaginatedList<OrderListResponse>>.Success(paginatedResult);
        }

        public async Task<Result<OrderResponse>> GetOrderByIdForAdminAsync(Guid orderId)
        {
            var order = await _orderRepo.GetOrderWithItemsAsync(orderId);
            if (order == null)
                return Result<OrderResponse>.Failure("Order not found.", 404);

            var response = _mapper.Map<OrderResponse>(order);
            return Result<OrderResponse>.Success(response);
        }

        public async Task<Result> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
                return Result.Failure("Order not found.", 404);

            // If transitioning to Cancelled or Returned, restore inventory
            if (request.Status == OrderStatus.Cancelled || request.Status == OrderStatus.Returned)
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await RestoreInventoryForOrderAsync(orderId);

                    order.Status = request.Status;
                    order.UpdatedAt = DateTime.UtcNow;

                    if (request.Status == OrderStatus.Returned)
                        order.PaymentStatus = PaymentStatus.Refunded;

                    await _orderRepo.UpdateAsync(order);

                    // Restore voucher if used
                    if (order.UserVoucherId.HasValue)
                    {
                        var uv = await _userVoucherRepo.GetByIdAsync(order.UserVoucherId.Value);
                        if (uv != null)
                        {
                            uv.IsUsed = false;
                            uv.UsedAt = null;
                            await _userVoucherRepo.UpdateAsync(uv);
                        }
                    }

                    await transaction.CommitAsync();
                    return Result.Success("Order status updated successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure($"Failed to update order status: {ex.Message}", 400);
                }
            }

            // Normal status transition
            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            // For Delivered with COD, mark payment as Paid
            if (request.Status == OrderStatus.Delivered && order.PaymentMethod == PaymentMethod.COD)
                order.PaymentStatus = PaymentStatus.Paid;

            await _orderRepo.UpdateAsync(order);
            return Result.Success("Order status updated successfully.");
        }

        public async Task<Result> CancelOrderAsync(Guid userId, Guid orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
                return Result.Failure("Order not found.", 404);
            if (order.UserId != userId)
                return Result.Failure("You do not have permission to cancel this order.", 403);
            if (order.Status != OrderStatus.Pending)
                return Result.Failure("Only pending orders can be cancelled.", 400);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepo.UpdateAsync(order);

                await RestoreInventoryForOrderAsync(orderId);

                // Restore voucher if used
                if (order.UserVoucherId.HasValue)
                {
                    var uv = await _userVoucherRepo.GetByIdAsync(order.UserVoucherId.Value);
                    if (uv != null)
                    {
                        uv.IsUsed = false;
                        uv.UsedAt = null;
                        await _userVoucherRepo.UpdateAsync(uv);
                    }
                }

                await transaction.CommitAsync();
                return Result.Success("Order cancelled successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure($"Failed to cancel order: {ex.Message}", 400);
            }
        }

        private async Task RestoreInventoryForOrderAsync(Guid orderId)
        {
            var order = await _orderRepo.GetOrderWithItemsAsync(orderId);
            if (order == null) return;

            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var inv = await _inventoryRepo.GetInventoryByVariantIdAsync(item.ProductVariantId.Value);
                    if (inv != null)
                    {
                        inv.Quantity += item.Quantity;
                        inv.UpdatedAt = DateTime.UtcNow;
                        await _inventoryRepo.UpdateAsync(inv);
                    }
                }
                else if (item.ComboId.HasValue)
                {
                    var combo = await _comboRepo.GetComboWithProductsAsync(item.ComboId.Value);
                    if (combo != null)
                    {
                        foreach (var cpv in combo.ComboProductVariants)
                        {
                            var inv = await _inventoryRepo.GetInventoryByVariantIdAsync(cpv.ProductVariantId);
                            if (inv != null)
                            {
                                inv.Quantity += cpv.Quantity * item.Quantity;
                                inv.UpdatedAt = DateTime.UtcNow;
                                await _inventoryRepo.UpdateAsync(inv);
                            }
                        }
                    }
                }
            }
        }
    }
}
