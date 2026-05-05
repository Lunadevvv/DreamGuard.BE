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
using DreamGuard.BE.DAL.Options;
using DreamGuard.BE.DAL.Repositories.Implements;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IHangFireService _hangFireService;
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
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly VnPayOptions _vnPayOptions;
        private readonly IVariantCustomizeTypeRepository _variantCustomizeTypeRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICheckoutProductOrderRepository _checkoutProductOrderRepository;

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
            ICustomerRepository customerRepository,
            ISystemConfigRepository systemConfigRepository,
            IOptions<VnPayOptions> vnPayOptions,
            IHangFireService hangFireService,
            IVariantCustomizeTypeRepository variantCustomizeTypeRepository,
            IProductRepository productRepository,
            ICheckoutProductOrderRepository checkoutProductOrderRepository
            )
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
            _systemConfigRepository = systemConfigRepository;
            _vnPayOptions = vnPayOptions.Value;
            _hangFireService = hangFireService;
            _variantCustomizeTypeRepository = variantCustomizeTypeRepository;
            _productRepository = productRepository;
            _checkoutProductOrderRepository = checkoutProductOrderRepository;
        }

        public async Task<Result<OrderResponse>> CreateOrderByAdminAsync(Guid adminId, CreateOrderByAdminRequest request, string ipAddress)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<OrderResponse>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var address = await _addressRepository.GetByIdAsync(customerId, request.AddressId);
            if (address == null)
                return Result<OrderResponse>.Failure("Address not found.", 404);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;
                decimal totalAddonPrice = 0;

                var variantIds = request.Items.Where(i => i.ProductVariantId.HasValue).Select(i => i.ProductVariantId!.Value).Distinct().ToList();
                var comboIds = request.Items.Where(i => i.ComboId.HasValue).Select(i => i.ComboId!.Value).Distinct().ToList();

                var variantsDict = (await _variantRepository.GetVariantsByIdsAsync(variantIds)).ToDictionary(v => v.Id);
                var inventoriesDict = (await _inventoryRepository.GetInventoriesByVariantIdsAsync(variantIds)).ToDictionary(i => i.ProductVariantId);
                var combosDict = (await _comboRepository.GetCombosWithProductsByIdsAsync(comboIds)).ToDictionary(c => c.Id);

                var allVariantCusList = await _variantCustomizeTypeRepository.GetByVariantIdsWithDetailsAsync(variantIds);
                var variantCusDict = allVariantCusList.GroupBy(vc => vc.ProductVariantId).ToDictionary(g => g.Key, g => g.ToList());

                var auditUnChanged = new AuditLog
                {
                    UserId = adminId,
                    ActionType = "CreateOrderFailed",
                    Message = $"Admin {adminId} create order for {customerId} failed. Stock remain unchanged",
                };

                foreach (var item in request.Items)
                {
                    if (item.ProductVariantId.HasValue)
                    {
                        variantsDict.TryGetValue(item.ProductVariantId.Value, out var variant);
                        if (variant == null || variant.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure($"Product variant '{item.ProductVariantId}' is no longer available.", 400);
                        }

                        inventoriesDict.TryGetValue(item.ProductVariantId.Value, out var inventory);
                        if (inventory == null || inventory.Quantity < item.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure($"Insufficient stock for product variant '{variant.Sku}'. Available: {inventory?.Quantity ?? 0}, Requested: {item.Quantity}.", 400);
                        }

                        var deductResult = await _inventoryService.DeductVariantStockAsync(item.ProductVariantId.Value, item.Quantity);
                        if (!deductResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(auditUnChanged));
                            return Result<OrderResponse>.Failure(deductResult.Error!, deductResult.StatusCode);
                        }

                        var audit = new AuditLog
                        {
                            UserId = adminId,
                            ActionType = "CreateOrder",
                            Message = $"Admin {adminId} deducted {item.Quantity} from stock of variant '{item.ProductVariantId.Value}' for order creation.",
                        };
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));

                        if (inventory != null)
                        {
                            inventory.Quantity -= item.Quantity;
                        }

                        List<VariantCustomizeType> itemVariantCustomizations = new();
                        variantCusDict.TryGetValue(item.ProductVariantId.Value, out itemVariantCustomizations);
                        itemVariantCustomizations ??= new List<VariantCustomizeType>();

                        var requestedCusIds = item.ProductCustomizeDetailRequest.Select(r => r.ProductCustomizeTypeId).ToList();
                        var validCustomizations = itemVariantCustomizations.Where(vc => requestedCusIds.Contains(vc.CusId)).ToList();
                        
                        var itemCustomizeDetails = item.ProductCustomizeDetailRequest.Select(d =>
                        {
                            var vc = validCustomizations.FirstOrDefault(vc => vc.CusId == d.ProductCustomizeTypeId);
                            if (vc == null) return null;

                            decimal addOnPrice = 0;
                            var type = vc.ProductCustomizeType;
                            if (type.CalculationMode == PriceCalculationMode.Multiplier)
                            {
                                double multiplier = vc.OverrideMultiplier ?? type.DefaultMultiplier ?? 1.0;
                                if (multiplier > 1.0)
                                    addOnPrice = variant.SalePrice * (decimal)(multiplier - 1.0);
                            }
                            else
                            {
                                addOnPrice = vc.OverridePrice ?? type.DefaultPrice;
                            }

                            return new ProductCustomizeDetail
                            {
                                CustomizeTypeName = type.Name,
                                CustomizeContent = d.CustomizeContent,
                                AddOnPrice = addOnPrice
                            };
                        }).Where(d => d != null).Select(d => d!).ToList();

                        var itemAddonPrice = itemCustomizeDetails.Sum(d => d.AddOnPrice);
                        totalAddonPrice += itemAddonPrice * item.Quantity;

                        var productName = variant.Product?.Name ?? "Unknown";
                        var itemPrice = variant.SalePrice;
                        orderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = item.ProductVariantId,
                            ComboId = null,
                            Quantity = item.Quantity,
                            UnitPrice = itemPrice,
                            TotalPrice = itemPrice * item.Quantity,
                            ItemName = $"{productName} - {variant.Size}",
                            CustomizeHash = string.Empty,
                            ProductCustomizeDetails = itemCustomizeDetails
                        });
                        subTotal += itemPrice * item.Quantity;
                    }
                    else if (item.ComboId.HasValue)
                    {
                        combosDict.TryGetValue(item.ComboId.Value, out var combo);
                        if (combo == null || combo.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure($"Combo '{item.ComboId}' is no longer available.", 400);
                        }

                        var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
                        if (comboStock < item.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return Result<OrderResponse>.Failure($"Insufficient stock for combo '{combo.Name}'. Available: {comboStock}, Requested: {item.Quantity}.", 400);
                        }

                        var deductResult = await _inventoryService.DeductComboStockAsync(item.ComboId.Value, item.Quantity);
                        if (!deductResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(auditUnChanged));
                            return Result<OrderResponse>.Failure(deductResult.Error!, deductResult.StatusCode);
                        }

                        var audit = new AuditLog
                        {
                            UserId = adminId,
                            ActionType = "CreateOrder",
                            Message = $"Admin {adminId} deducted combo:{item.ComboId.Value} with quantity: {item.Quantity} for order creation.",
                        };
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));

                        var itemPrice = combo.SalePrice;
                        orderItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = null,
                            ComboId = item.ComboId,
                            Quantity = item.Quantity,
                            UnitPrice = itemPrice,
                            TotalPrice = itemPrice * item.Quantity,
                            ItemName = combo.Name
                        });
                        subTotal += itemPrice * item.Quantity;
                    }
                }

                decimal discountAmount = 0;
                if (request.UserVoucherId.HasValue)
                {
                    var userVoucher = await _userVoucherRepository.GetByIdAsync(request.UserVoucherId.Value);
                    if (userVoucher == null || userVoucher.CustomerId != customerId || userVoucher.IsUsed || userVoucher.ExpiredAt < DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("Voucher invalid or expired.", 400);
                    }

                    var voucher = userVoucher.Voucher;
                    if (voucher == null || !voucher.IsActive || voucher.EndDate < DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("Voucher is no longer active.", 400);
                    }

                    discountAmount = subTotal * voucher.DiscountValue;
                    discountAmount = Math.Min(discountAmount, voucher.MaxDiscountAmount);
                    discountAmount = Math.Min(discountAmount, subTotal);

                    if (voucher.VoucherType == VoucherType.Service)
                    {
                        await transaction.RollbackAsync();
                        return Result<OrderResponse>.Failure("This voucher is specifically for services only.", 400);
                    }

                    userVoucher.IsUsed = true;
                    userVoucher.UsedAt = DateTime.UtcNow;
                    await _userVoucherRepository.UpdateAsync(userVoucher);
                }

                var totalAmount = Math.Max(0, subTotal + totalAddonPrice - discountAmount);
                var orderCode = GenerateOrderCode();

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

                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                }
                await _orderRepository.AddOrderItemsAsync(orderItems);

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
                    UpdatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(_vnPayOptions.PaymentExpirationMinutes)
                };

                await _paymentRepository.CreateAsync(payment);
                await transaction.CommitAsync();

                Notification notification = new Notification
                {
                    UserId = customerId,
                    ActionType = "Create order",
                    Message = $"Your order {order.OrderCode} has been created by our staff",
                };
                _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));

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
                    CreatedAt = order.CreatedAt,
                    PaymentId = payment.Id,
                    PaymentExpiredAt = payment.ExpiredAt
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                var audit = new AuditLog
                {
                    UserId = adminId,
                    ActionType = "CreateOrderFailed",
                    Message = $"Admin {adminId} create order for CustomerId:{request.CustomerId} failed. Stock remain unchanged",
                };
                _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                return Result<OrderResponse>.Failure("Failed to create order.", 500);
            }
        }

        public async Task<Result<CheckoutProductOrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request, string ipAddress)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<CheckoutProductOrderResponse>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var cart = await _cartRepository.GetCartWithItemsAsync(customerId);
            if (cart == null || !cart.CartItems.Any())
            {
                return Result<CheckoutProductOrderResponse>.Failure("Cart is empty.", 400);
            }

            var address = await _addressRepository.GetByIdAsync(customerId, request.AddressId);
            if (address == null)
            {
                return Result<CheckoutProductOrderResponse>.Failure("Address not found.", 404);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var normalItems = new List<OrderItem>();
                var customizeItems = new List<OrderItem>();
                decimal subTotal = 0;
                decimal totalAddonPrice = 0;

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

                var auditUnChanged = new AuditLog
                {
                    UserId = userId,
                    ActionType = "CreateOrderFailed",
                    Message = $"user: {userId} create order failed. Stock remain unchanged",
                };

                foreach (var cartItem in cart.CartItems)
                {
                    OrderItem orderItem = null;
                    bool isCustomize = false;

                    if (cartItem.ProductVariantId.HasValue)
                    {
                        variantsDict.TryGetValue(cartItem.ProductVariantId.Value, out var variant);
                        if (variant == null || variant.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<CheckoutProductOrderResponse>.Failure($"Product variant '{cartItem.ProductVariantId}' is no longer available.", 400);
                        }

                        inventoriesDict.TryGetValue(cartItem.ProductVariantId.Value, out var inventory);
                        if (inventory == null || inventory.Quantity < cartItem.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return Result<CheckoutProductOrderResponse>.Failure($"Insufficient stock for product variant '{variant.Sku}'. Available: {inventory?.Quantity ?? 0}, Requested: {cartItem.Quantity}.", 400);
                        }

                        var deductResult = await _inventoryService.DeductVariantStockAsync(cartItem.ProductVariantId.Value, cartItem.Quantity);
                        if (!deductResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(auditUnChanged));
                            return Result<CheckoutProductOrderResponse>.Failure(deductResult.Error!, deductResult.StatusCode);
                        }

                        var audit = new AuditLog
                        {
                            UserId = userId,
                            ActionType = "CreateOrder",
                            Message = $"user: {userId} Deducted {cartItem.Quantity} from stock of variant '{cartItem.ProductVariantId.Value}' for order creation.",
                        };
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));

                        if (inventory != null)
                        {
                            inventory.Quantity -= cartItem.Quantity;
                        }

                        var productName = cartItem.ProductVariant?.Product?.Name ?? "Unknown";
                        var itemPrice = variant.SalePrice;
                        var itemAddonPrice = cartItem.ProductCustomizeDetails?.Sum(d => d.AddOnPrice) ?? 0;
                        
                        isCustomize = cartItem.ProductCustomizeDetails != null && cartItem.ProductCustomizeDetails.Any();

                        orderItem = new OrderItem
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
                        };
                        
                        subTotal += itemPrice * cartItem.Quantity;
                        totalAddonPrice += itemAddonPrice * cartItem.Quantity;
                    }
                    else if (cartItem.ComboId.HasValue)
                    {
                        combosDict.TryGetValue(cartItem.ComboId.Value, out var combo);
                        if (combo == null || combo.Status != ProductStatus.Published)
                        {
                            await transaction.RollbackAsync();
                            return Result<CheckoutProductOrderResponse>.Failure($"Combo '{cartItem.ComboId}' is no longer available.", 400);
                        }

                        var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
                        if (comboStock < cartItem.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return Result<CheckoutProductOrderResponse>.Failure($"Insufficient stock for combo '{combo.Name}'. Available: {comboStock}, Requested: {cartItem.Quantity}.", 400);
                        }

                        var deductResult = await _inventoryService.DeductComboStockAsync(cartItem.ComboId.Value, cartItem.Quantity);
                        if (!deductResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(auditUnChanged));
                            return Result<CheckoutProductOrderResponse>.Failure(deductResult.Error!, deductResult.StatusCode);
                        }

                        var audit = new AuditLog
                        {
                            UserId = userId,
                            ActionType = "CreateOrder",
                            Message = $"user: {userId} Deducted combo:{cartItem.ComboId.Value} with quantity: {cartItem.Quantity} for order creation.",
                        };
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));

                        var itemPrice = combo.SalePrice;
                        orderItem = new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = null,
                            ComboId = cartItem.ComboId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = itemPrice,
                            TotalPrice = itemPrice * cartItem.Quantity,
                            ItemName = combo.Name
                        };
                        subTotal += itemPrice * cartItem.Quantity;
                    }

                    if (orderItem != null)
                    {
                        if (isCustomize)
                        {
                            customizeItems.Add(orderItem);
                        }
                        else
                        {
                            normalItems.Add(orderItem);
                        }
                    }
                }

                if (customizeItems.Any() && request.PaymentMethod == PaymentMethod.COD)
                {
                    await transaction.RollbackAsync();
                    return Result<CheckoutProductOrderResponse>.Failure("COD is not able for customize order", 400);
                }

                decimal discountAmount = 0;
                UserVoucher? userVoucher = null;

                if (request.UserVoucherId.HasValue)
                {
                    userVoucher = await _userVoucherRepository.GetByIdAsync(request.UserVoucherId.Value);
                    if (userVoucher == null || userVoucher.CustomerId != customerId || userVoucher.IsUsed || userVoucher.ExpiredAt < DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result<CheckoutProductOrderResponse>.Failure("Voucher invalid or expired.", 400);
                    }

                    var voucher = userVoucher.Voucher;
                    if (voucher == null || !voucher.IsActive || voucher.EndDate < DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result<CheckoutProductOrderResponse>.Failure("Voucher is no longer active.", 400);
                    }

                    discountAmount = subTotal * voucher.DiscountValue;
                    discountAmount = Math.Min(discountAmount, voucher.MaxDiscountAmount);
                    discountAmount = Math.Min(discountAmount, subTotal); 

                    if (voucher.VoucherType == VoucherType.Service)
                    {
                        await transaction.RollbackAsync();
                        return Result<CheckoutProductOrderResponse>.Failure("This voucher is specifically for services only.", 400);
                    }

                    userVoucher.IsUsed = true;
                    userVoucher.UsedAt = DateTime.UtcNow;
                    await _userVoucherRepository.UpdateAsync(userVoucher);
                }

                decimal shippingFee = request.ShippingFee; // Defaulting to 0 as per logic
                var totalAmount = Math.Max(0, subTotal + totalAddonPrice - discountAmount + shippingFee);

                var checkoutOrderCode = GenerateOrderCode();

                var checkoutProductOrder = new CheckoutProductOrder
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    CheckoutOrderCode = checkoutOrderCode,
                    Status = CheckoutOrderStatus.Pending,
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    TotalAddonPrice = totalAddonPrice,
                    ShippingFee = shippingFee,
                    UserVoucherId = request.UserVoucherId,
                    Note = request.Note,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _checkoutProductOrderRepository.CreateAsync(checkoutProductOrder);

                var createdOrders = new List<Order>();

                async Task CreateChildOrder(List<OrderItem> items, string suffix)
                {
                    if (!items.Any()) return;

                    decimal childSubTotal = items.Sum(i => i.TotalPrice);
                    decimal childAddonPrice = items.Sum(i => i.ProductCustomizeDetails?.Sum(d => d.AddOnPrice * i.Quantity) ?? 0);
                    
                    decimal childDiscount = 0;
                    if (subTotal > 0 && discountAmount > 0)
                    {
                        childDiscount = Math.Round((childSubTotal / subTotal) * discountAmount, 0); // Proportional
                    }
                    
                    decimal childShipping = shippingFee > 0 ? shippingFee / 2 : 0; // Proportional

                    decimal childTotal = Math.Max(0, childSubTotal + childAddonPrice - childDiscount + childShipping);

                    var childOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customerId,
                        CheckoutProductOrderId = checkoutProductOrder.Id,
                        OrderCode = $"{checkoutOrderCode}-{suffix}",
                        Status = OrderStatus.Pending,
                        ReceiverName = address.ReceiverName,
                        PhoneNumber = address.PhoneNumber,
                        Street = address.Street,
                        City = address.City,
                        District = address.District,
                        Ward = address.Ward,
                        Province = address.Province,
                        SubTotal = childSubTotal,
                        DiscountAmount = childDiscount,
                        TotalAmount = childTotal,
                        TotalAddonPrice = childAddonPrice,
                        ShippingFee = childShipping,
                        UserVoucherId = request.UserVoucherId,
                        Note = request.Note,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _orderRepository.CreateAsync(childOrder);

                    foreach (var item in items)
                    {
                        item.OrderId = childOrder.Id;
                    }
                    await _orderRepository.AddOrderItemsAsync(items);
                    createdOrders.Add(childOrder);
                }

                await CreateChildOrder(normalItems, "N");
                await CreateChildOrder(customizeItems, "C");

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderCode = checkoutOrderCode,
                    CheckoutProductOrderId = checkoutProductOrder.Id,
                    Status = PaymentStatus.Pending,
                    Amount = totalAmount,
                    Description = $"Payment for Checkout Order {checkoutOrderCode}",
                    PaymentMethod = request.PaymentMethod,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(_vnPayOptions.PaymentExpirationMinutes)
                };

                await _paymentRepository.CreateAsync(payment);

                await _cartRepository.ClearCartItemsAsync(cart.Id);

                await transaction.CommitAsync();

                foreach (var childOrder in createdOrders)
                {
                    Notification notification = new Notification
                    {
                        UserId = customerId,
                        ActionType = "Create order",
                        Message = $"Your order {childOrder.OrderCode} has been created successfully",
                    };
                    _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));
                }

                string? paymentUrl = null;
                if (request.PaymentMethod == PaymentMethod.VnPay)
                {
                    var vnPayRequest = new VnPaymentRequest
                    {
                        PaymentId = payment.Id.ToString(),
                        OrderCode = checkoutOrderCode,
                        Description = payment.Description,
                        Amount = totalAmount,
                        IpAddress = ipAddress,
                        CreatedDate = payment.CreatedAt
                    };
                    paymentUrl = _vnPayService.CreatePaymentUrl(vnPayRequest);
                }

                return Result<CheckoutProductOrderResponse>.Success(new CheckoutProductOrderResponse
                {
                    Id = checkoutProductOrder.Id,
                    CheckoutOrderCode = checkoutProductOrder.CheckoutOrderCode,
                    Status = checkoutProductOrder.Status,
                    SubTotal = checkoutProductOrder.SubTotal,
                    DiscountAmount = checkoutProductOrder.DiscountAmount,
                    TotalAmount = checkoutProductOrder.TotalAmount,
                    TotalAddonPrice = checkoutProductOrder.TotalAddonPrice,
                    ShippingFee = checkoutProductOrder.ShippingFee,
                    PaymentMethod = request.PaymentMethod,
                    PaymentUrl = paymentUrl,
                    CreatedAt = checkoutProductOrder.CreatedAt,
                    PaymentId = payment.Id,
                    PaymentExpiredAt = payment.ExpiredAt,
                    ChildOrders = createdOrders.Select(o => new OrderResponse
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        Status = o.Status,
                        SubTotal = o.SubTotal,
                        ShippingFee = o.ShippingFee,
                        DiscountAmount = o.DiscountAmount,
                        TotalAmount = o.TotalAmount,
                        TotalAddonPrice = o.TotalAddonPrice,
                        PaymentMethod = request.PaymentMethod,
                        PaymentUrl = paymentUrl,
                        CreatedAt = o.CreatedAt,
                        PaymentId = payment.Id,
                        PaymentExpiredAt = payment.ExpiredAt
                    }).ToList()
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                var audit = new AuditLog
                {
                    UserId = userId,
                    ActionType = "CreateOrderFailed",
                    Message = $"user: {userId} create order failed. Stock remain unchanged",
                };
                _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                return Result<CheckoutProductOrderResponse>.Failure("Failed to create order.", 500);
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
            var salePrice = variant.SalePrice > 0 ? variant.SalePrice : variant.BasePrice;
            var basePriceWithDepositReduce = salePrice - variant.Product.DepositAmount;
            var orderItems = await _orderRepository.GetOrdersToTradeInAsync(customerId, categoryParentId.Value, salePrice, variant.Product.DepositAmount);
            var orderItemResponseList = orderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductVariantId = oi.ProductVariantId,
                ProductVariantImageUrl = oi.ProductVariant?.Product?.Assets?.FirstOrDefault()?.Url ?? "",
                ComboId = oi.ComboId,
                ItemName = oi.ItemName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice,
                CustomizeHash = oi.CustomizeHash,
                TradeInUsedAmount = oi.TradeInUsedAmount,
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
        public async Task<Result<PaginatedList<CheckoutProductOrderAdminSummaryResponse>>> GetAllCheckoutOrdersForAdminAsync(
            int pageNumber, CheckoutOrderStatus? status, string? orderCode)
        {
            var checkoutOrders = await _checkoutProductOrderRepository.GetAllForAdminAsync(pageNumber, status, orderCode);

            var responses = checkoutOrders.Items.Select(c => new CheckoutProductOrderAdminSummaryResponse
            {
                Id = c.Id,
                CheckoutOrderCode = c.CheckoutOrderCode,
                Status = c.Status,
                TotalAmount = c.TotalAmount,
                RefundingAmount = c.RefundingAmount,
                RefundedAmount = c.RefundedAmount,
                CreatedAt = c.CreatedAt,
                ChildOrders = c.Orders.Select(o => new OrderSummaryResponse
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    Status = o.Status,
                    ItemCount = o.OrderItems?.Count ?? 0,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt
                }).ToList()
            }).ToList();

            return Result<PaginatedList<CheckoutProductOrderAdminSummaryResponse>>.Success(
                new PaginatedList<CheckoutProductOrderAdminSummaryResponse>(
                    responses, checkoutOrders.TotalCount, checkoutOrders.PageNumber, checkoutOrders.PageSize));
        }

        public async Task<Result<PaginatedList<CheckoutProductOrderAdminSummaryResponse>>> GetAllUserCheckoutOrdersAsync(
            int pageNumber, CheckoutOrderStatus? status, string? orderCode, Guid userId)
        {
            var checkoutOrders = await _checkoutProductOrderRepository.GetAllForUserAsync(pageNumber, status, orderCode, userId);

            var responses = checkoutOrders.Items.Select(c => new CheckoutProductOrderAdminSummaryResponse
            {
                Id = c.Id,
                CheckoutOrderCode = c.CheckoutOrderCode,
                Status = c.Status,
                TotalAmount = c.TotalAmount,
                RefundingAmount = c.RefundingAmount,
                RefundedAmount = c.RefundedAmount,
                CreatedAt = c.CreatedAt,
                ChildOrders = c.Orders.Select(o => new OrderSummaryResponse
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    Status = o.Status,
                    ItemCount = o.OrderItems?.Count ?? 0,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt
                }).ToList()
            }).ToList();

            return Result<PaginatedList<CheckoutProductOrderAdminSummaryResponse>>.Success(
                new PaginatedList<CheckoutProductOrderAdminSummaryResponse>(
                    responses, checkoutOrders.TotalCount, checkoutOrders.PageNumber, checkoutOrders.PageSize));
        }

        public async Task<Result> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetOrderWithItemsForUpdateAsync(orderId);
            if (order == null)
            {
                return Result.Failure("Order not found.", 404);
            }

            // if (newStatus == OrderStatus.Confirmed && order.CheckoutProductOrderId.HasValue)
            // {
            //     return Result.Failure("Child orders cannot be confirmed directly. Please confirm the CheckoutOrder instead.", 400);
            // }

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

            // Award points if order becomes Completed
            if (order.Status != OrderStatus.Completed && newStatus == OrderStatus.Completed)
            {
                var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
                if (customer != null)
                {
                    var config = await _systemConfigRepository.GetByKeyAsync("OrderCoinPercent");
                    decimal percent = 1.0m; // default 1%
                    if (config != null && decimal.TryParse(config.ConfigValue, out decimal parsed))
                    {
                        percent = parsed;
                    }
                    int coinsEarned = (int)(order.TotalAmount * percent / 100);
                    customer.MemberCoin += coinsEarned;
                    _customerRepository.UpdateEntity(customer);
                }
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            _orderRepository.UpdateEntity(order);

            if(newStatus == OrderStatus.Confirmed && order.CheckoutProductOrderId.HasValue)
            {
                var checkoutOrder = await _checkoutProductOrderRepository.GetByIdAsync(order.CheckoutProductOrderId.Value);
                if (checkoutOrder != null && checkoutOrder.Status == CheckoutOrderStatus.Pending)
                {
                    checkoutOrder.Status = CheckoutOrderStatus.Confirmed;
                    checkoutOrder.UpdatedAt = DateTime.UtcNow;
                    _checkoutProductOrderRepository.UpdateEntity(checkoutOrder);
                }
            }
            
            await _unitOfWork.SaveChangeAsync();
            // notification
            Notification notification = new Notification
            {
                UserId = order.CustomerId,
                ActionType = "Update order status",
                Message = $"Order:{order.Id} has been updated to {newStatus}",
            };
            _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));
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

            if (order.CheckoutProductOrderId.HasValue)
            {
                // Use GetByIdAsync to avoid loading Orders graph (current 'order' is already tracked)
                var checkoutOrder = await _checkoutProductOrderRepository.GetByIdAsync(order.CheckoutProductOrderId.Value);
                if (checkoutOrder != null)
                {
                    if (checkoutOrder.Status == CheckoutOrderStatus.Pending)
                    {
                        return Result.Failure("Cannot cancel child order while checkout order is pending. Please cancel the checkout order instead.", 400);
                    }
                    var payment = await _paymentRepository.GetLatestNonRefundPaymentByCheckoutOrderIdAsync(order.CheckoutProductOrderId.Value);
                    if (payment != null && payment.PaymentMethod == PaymentMethod.COD)
                    {
                        return Result.Failure("Cannot partially cancel a COD order. You must cancel the entire checkout order.", 400);
                    }
                }
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
                        var audit = new AuditLog
                        {
                            UserId = order.CustomerId,
                            ActionType = "CancelOrder",
                            Message = $"user: {order.CustomerId} cancel order and restore product: {item.ProductVariantId.Value} with quantity {item.Quantity}",
                        };


                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                            return Result.Failure(result.Error!, result.StatusCode);
                        }
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                    }
                    else if (item.ComboId.HasValue)
                    {
                        var result = await _inventoryService.RestoreComboStockAsync(
                            item.ComboId.Value, item.Quantity);
                        var audit = new AuditLog
                        {
                            UserId = order.CustomerId,
                            ActionType = "CancelOrder",
                            Message = $"user: {order.CustomerId} cancel order and restore combo: {item.ComboId.Value} with quantity {item.Quantity}",
                        };
                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
                            return Result.Failure(result.Error!, result.StatusCode);
                        }
                        _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));

                    }
                }

                // Handle voucher restore and refund creation
                if (order.CheckoutProductOrderId.HasValue)
                {
                    var checkoutOrder = await _checkoutProductOrderRepository.GetByIdAsync(order.CheckoutProductOrderId.Value);
                    if (checkoutOrder != null)
                    {
                        if (checkoutOrder.Status == CheckoutOrderStatus.Confirmed
                            || checkoutOrder.Status == CheckoutOrderStatus.PartialRefunding
                            || checkoutOrder.Status == CheckoutOrderStatus.PartialRefunded)
                        {
                            checkoutOrder.RefundingAmount += order.TotalAmount;
                            checkoutOrder.Status = CheckoutOrderStatus.PartialRefunding;
                            checkoutOrder.UpdatedAt = DateTime.UtcNow;
                            await _checkoutProductOrderRepository.UpdateAsync(checkoutOrder);

                            var lastPayment = await _paymentRepository.GetPaidPaymentByCheckoutOrderIdAsync(order.CheckoutProductOrderId.Value);
                            if (lastPayment != null && order.TotalAmount > 0)
                            {
                                var refundPayment = new Payment
                                {
                                    Id = Guid.NewGuid(),
                                    POrderId = order.Id,
                                    OrderCode = order.OrderCode,
                                    Status = PaymentStatus.Refunding,
                                    PaymentType = PaymentType.Refund,
                                    Amount = order.TotalAmount,
                                    Description = $"Refund for cancelled child Order {order.OrderCode}.",
                                    PaymentMethod = PaymentMethod.Other,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow,
                                    ExpiredAt = DateTime.UtcNow.AddMinutes(5)
                                };
                                await _paymentRepository.CreateAsync(refundPayment);
                            }
                        }

                        // Restore voucher ONLY if ALL other child orders are already cancelled
                        var siblingOrders = await _orderRepository.GetOrdersByCheckoutOrderIdAsync(order.CheckoutProductOrderId.Value);
                        bool allOtherCancelled = siblingOrders.Where(o => o.Id != order.Id).All(o => o.Status == OrderStatus.Cancelled);
                        if (allOtherCancelled && checkoutOrder.UserVoucherId.HasValue)
                        {
                            var userVoucher = await _userVoucherRepository.GetByIdAsync(checkoutOrder.UserVoucherId.Value);
                            if (userVoucher != null)
                            {
                                userVoucher.IsUsed = false;
                                userVoucher.UsedAt = null;
                                await _userVoucherRepository.UpdateAsync(userVoucher);
                            }
                        }
                    }
                }
                else
                {
                    // Fallback for non-checkout orders (if any)
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

                    var payment = await _paymentRepository.GetPaymentByOrderIdForUpdateAsync(order.Id);
                    if (payment != null && payment.Status == PaymentStatus.Pending)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepository.UpdateAsync(payment);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);
                // notification
                Notification notification = new Notification
                {
                    UserId = order.CustomerId,
                    ActionType = "cancel order",
                    Message = $"Order:{order.Id} has been cancelled",
                };
                _hangFireService.Enqueue<INotificationService>(t => t.SendNotificationAsync(notification));
                await transaction.CommitAsync();
                return Result.Success("Order cancelled successfully.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                // audit log for fail stock deduction
                var audit = new AuditLog
                {
                    UserId = order.CustomerId,
                    ActionType = "CreateOrder",
                    Message = $"user: {order.CustomerId} failed cancel order. stock remain unchanged",
                };
                _hangFireService.Enqueue<IAuditLogService>(t => t.LogAsync(audit));
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
                (OrderStatus.Shipping, OrderStatus.Returned) => true,
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
                    TradeInUsedAmount = oi.TradeInUsedAmount,
                    ExchangeRequestedQuantity = oi.ExchangeRequestedQuantity,
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
                UpdatedAt = order.UpdatedAt,
                PaymentMethod = order.Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.PaymentMethod ?? PaymentMethod.COD,
                PaymentStatus = order.Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.Status ?? PaymentStatus.Pending,
                ShippingStaffName = order.ShippingTasks?.OrderByDescending(st => st.CreatedAt).FirstOrDefault(st => st.OrderId == order.Id)?.Staff?.FullName ?? "N/A",
                ShippingStatus = order.ShippingTasks?.OrderByDescending(st => st.CreatedAt).FirstOrDefault(st => st.OrderId == order.Id)?.Status.ToString() ?? "N/A",
                ShippingStaffAvatarUrl = order.ShippingTasks?.OrderByDescending(st => st.CreatedAt).FirstOrDefault(st => st.OrderId == order.Id)?.Staff?.AvatarUrl ?? string.Empty,
                ShippingFee = order.ShippingFee
            };
        }

        public async Task<Result<OrderDashBoardResponse>> GetOrderDashBoardAsync(DateOnly fromDate, DateOnly toDate)
        {
            var from = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var to = toDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
            var data = await _orderRepository.GetOrderDashBoardAsync(from, to);
            if (data == null || !data.Any())
            {
                return Result<OrderDashBoardResponse>.Success(new OrderDashBoardResponse());
            }
            decimal totalAmount = 0;
            decimal totalCODAmount = 0;
            decimal totalRefundAmount = 0;
            decimal totalVnPayAmount = 0;
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
                        totalCODAmount += p.Amount;
                    }
                    if (p.PaymentType == PaymentType.Purchase && p.Status == PaymentStatus.Paid)
                    {
                        totalAmount += p.Amount;
                        totalVnPayAmount += p.Amount;
                    }
                    if (p.PaymentType == PaymentType.Refund && p.Status == PaymentStatus.Refunded)
                    {
                        totalRefundAmount += p.Amount;
                    }
                });
            }
            //take 5 best-seller products
            var products = await _orderRepository.GetBestSellerProductsAsync(5);
            var productResponses = new List<TopProductResponse>();
            foreach (var p in products)
            {
                var hasVariants = p.Product.Variants != null && p.Product.Variants.Any();
                var productResponse = new ProductResponse
                {
                    Id = p.Product.Id,
                    Name = p.Product.Name,
                    Summary = p.Product.Summary,
                    Slug = p.Product.Slug,
                    Material = p.Product.Material,
                    AgeGroup = p.Product.AgeGroup,
                    AverageRating = p.Product.AverageRating,
                    BasePrice = hasVariants ? p.Product.Variants.Min(v => v.BasePrice) : 0,
                    SalePrice = hasVariants ? p.Product.Variants.Min(v => v.SalePrice) : 0,
                    IsTradeInEligible = p.Product.IsTradeInEligible,
                    MinTradeInPrice = p.Product.MinTradeInPrice,
                    DepositAmount = p.Product.DepositAmount,
                    ImageUrls = p.Product.Assets.Select(a => a.Url).ToList()
                };
                productResponses.Add(new TopProductResponse
                {
                    Product = productResponse,
                    TotalQuantity = p.TotalQuantity
                });
            }

            var response = new OrderDashBoardResponse
            {
                TotalOrders = data.Count,
                TotalCompletedOrders = data.Where(ti => ti.Status == OrderStatus.Completed).Count(),
                TotalCancelledOrders = data.Where(t1 => t1.Status == OrderStatus.Cancelled).Count(),
                TotalRefundedOrders = data.Where(ti => ti.Status == OrderStatus.ReturnedAndRefunded).Count(),
                TotalAmount = totalAmount,
                TotalCODAmount = totalCODAmount,
                TotalRefundAmount = totalRefundAmount,
                TotalVnPayAmount = totalVnPayAmount,
                FromDate = fromDate,
                ToDate = toDate,
                TopSellingProducts = productResponses
            };
            return Result<OrderDashBoardResponse>.Success(response);
        }

        public async Task<Result<List<TotalAmountLineChartResponse>>> GetTotalAmountLineChartAsync(DateOnly fromDate , DateOnly toDate)
        {
            var from = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var to = toDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
            var data = await _paymentRepository.GetTotalAmountLineChartDataAsync(from, to);
            
            var groupedData = data
                .GroupBy(d => d.CreatedAt.Date)
                .ToDictionary(
                    g => DateOnly.FromDateTime(g.Key),
                    g => g.Sum(p => p.Amount)
                );
            // đảm bảo data có full date từ fromDate đến toDate, nếu ko có thì thêm vào với total amount = 0
            var result = new List<TotalAmountLineChartResponse>();
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                result.Add(new TotalAmountLineChartResponse
                {
                    Date = date,
                    TotalAmount = groupedData.TryGetValue(date, out var amount) ? amount : 0
                });
            }

            return Result<List<TotalAmountLineChartResponse>>.Success(result);
        }
    
    //////////////////////////////////Checkout order logic//////////////////////////////////////
        public async Task<Result> ConfirmCheckoutOrderAsync(Guid checkoutOrderId)
        {
            var checkoutOrder = await _checkoutProductOrderRepository.GetWithOrdersAndPaymentsByIdAsync(checkoutOrderId);
            if (checkoutOrder == null)
            {
                return Result.Failure("Checkout order not found.", 404);
            }

            if (!checkoutOrder.Payments.Any(p => p.PaymentType == PaymentType.Purchase && p.PaymentMethod == PaymentMethod.COD))
            {
                return Result.Failure("Only COD checkout orders can be confirmed.", 400);
            }
            if (checkoutOrder.Status != CheckoutOrderStatus.Pending)
            {
                return Result.Failure("Only pending checkout orders can be confirmed.", 400);
            }

            checkoutOrder.Status = CheckoutOrderStatus.Confirmed;
            checkoutOrder.UpdatedAt = DateTime.UtcNow;
            _checkoutProductOrderRepository.UpdateEntity(checkoutOrder);

            foreach(var childOrder in checkoutOrder.Orders)
            {
                if (childOrder.Status == OrderStatus.Pending)
                {
                    childOrder.Status = OrderStatus.Confirmed;
                    childOrder.UpdatedAt = DateTime.UtcNow;
                    _orderRepository.UpdateEntity(childOrder);
                }
            }

            await _unitOfWork.SaveChangeAsync();
            return Result.Success($"Checkout order status updated to 'Confirmed'.");
        }
    
        public async Task<Result> CancelCheckoutOrderByAdminAsync(Guid checkoutOrderId)
        {
            var checkoutOrder = await _checkoutProductOrderRepository.GetWithOrdersAndPaymentsByIdAsync(checkoutOrderId);
            if (checkoutOrder == null)
            {
                return Result.Failure("Checkout order not found.", 404);
            }

            if (checkoutOrder.Status == CheckoutOrderStatus.Cancelled || checkoutOrder.Status == CheckoutOrderStatus.CancelledAndRefunding || checkoutOrder.Status == CheckoutOrderStatus.CancelledAndRefunded)
            {
                return Result.Failure("Checkout order is already cancelled.", 400);
            }

            // Check if any child order is already confirmed or beyond
            // if (checkoutOrder.Orders.Any(o => o.Status != OrderStatus.Pending))
            // {
            //     return Result.Failure("Cannot cancel checkout order when some child orders are already confirmed or beyond. Please cancel child orders first.", 400);
            // }
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Cancel all child orders
                foreach(var childOrder in checkoutOrder.Orders)
                {
                    if (childOrder.Status >= OrderStatus.Delivered && childOrder.Status <= OrderStatus.ReturnedAndRefunded)
                    {
                        childOrder.Status = OrderStatus.Cancelled;
                        childOrder.UpdatedAt = DateTime.UtcNow;
                        _orderRepository.UpdateEntity(childOrder);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure($"Cannot cancel child order in {childOrder.Status}, please contact to staff!", 400);
                    }
                }

                var lastPurchasePayment = checkoutOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault(p => p.PaymentType == PaymentType.Purchase);
                if (lastPurchasePayment != null && lastPurchasePayment.Status == PaymentStatus.Paid)
                {
                    var refundPayment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        CheckoutProductOrderId = checkoutOrder.Id,
                        Amount = lastPurchasePayment.Amount,
                        OrderCode = checkoutOrder.CheckoutOrderCode,
                        PaymentType = PaymentType.Refund,
                        PaymentMethod = PaymentMethod.Other,
                        Status = PaymentStatus.Refunding,
                        Description = $"Refund for cancelled CheckoutOrder {checkoutOrder.CheckoutOrderCode}.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ExpiredAt = DateTime.UtcNow.AddMinutes(5)
                    };
                    _paymentRepository.AddEntity(refundPayment);

                    checkoutOrder.RefundingAmount = refundPayment.Amount;
                    checkoutOrder.Status = CheckoutOrderStatus.CancelledAndRefunding;
                }
                else
                {
                    checkoutOrder.Status = CheckoutOrderStatus.Cancelled;
                }
                checkoutOrder.UpdatedAt = DateTime.UtcNow;
                _checkoutProductOrderRepository.UpdateEntity(checkoutOrder);

                HandleVoucherRestore(checkoutOrder);

                await _unitOfWork.SaveChangeAsync();
                await transaction.CommitAsync();
            }catch(Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to cancel checkout order.", 500);
            }
            
            return Result.Success($"Checkout order and its child orders have been cancelled.");
        }

        private void HandleVoucherRestore(CheckoutProductOrder checkoutOrder)
        {
            if (checkoutOrder.UserVoucherId.HasValue)
            {
                var userVoucher = _userVoucherRepository.GetByIdAsync(checkoutOrder.UserVoucherId.Value).Result;
                if (userVoucher != null)
                {
                    userVoucher.IsUsed = false;
                    userVoucher.UsedAt = null;
                    _userVoucherRepository.UpdateEntity(userVoucher);
                }
            }
        }

        public async Task<Result> CancelCheckoutOrderByUserAsync(Guid checkoutOrderId, Guid customerId)
        {
            var checkoutOrder = await _checkoutProductOrderRepository.GetWithOrdersAndPaymentsByIdAsync(checkoutOrderId);
            if (checkoutOrder == null)
            {
                return Result.Failure("Checkout order not found.", 404);
            }

            if (checkoutOrder.CustomerId != customerId)
            {
                return Result.Failure("Checkout order not found.", 404);
            }

            if (checkoutOrder.Status == CheckoutOrderStatus.Cancelled)
            {
                return Result.Failure("Checkout order is already cancelled.", 400);
            }

            var lastPurchasePayment = checkoutOrder.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault(p => p.PaymentType == PaymentType.Purchase);
            if(lastPurchasePayment == null)
            {
                return Result.Failure("Cannot find purchase payment for this checkout order.", 400);
            }

            if (checkoutOrder.Status > CheckoutOrderStatus.Confirmed)
            {
                return Result.Failure($"Cannot Cancel check out order in {checkoutOrder.Status}, please contact to staff!", 400);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Cancel child orders that belong to the user
                foreach(var childOrder in checkoutOrder.Orders.Where(o => o.CustomerId == customerId))
                {
                    if (childOrder.Status <= OrderStatus.Confirmed)
                    {
                        childOrder.Status = OrderStatus.Cancelled;
                        childOrder.UpdatedAt = DateTime.UtcNow;
                        _orderRepository.UpdateEntity(childOrder);
                    }else
                    {
                        return Result.Failure($"Cannot cancel child order in {childOrder.Status}, please contact to staff!", 400);
                    }
                }
                if (lastPurchasePayment.Status == PaymentStatus.Pending)
                {
                    lastPurchasePayment.Status = PaymentStatus.Failed;
                    lastPurchasePayment.UpdatedAt = DateTime.UtcNow;
                    _paymentRepository.UpdateEntity(lastPurchasePayment);
                }

                if (lastPurchasePayment.Status == PaymentStatus.Paid)
                {
                    var refundPayment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        CheckoutProductOrderId = checkoutOrder.Id,
                        Amount = lastPurchasePayment.Amount,
                        OrderCode = checkoutOrder.CheckoutOrderCode,
                        PaymentType = PaymentType.Refund,
                        PaymentMethod = PaymentMethod.Other,
                        Status = PaymentStatus.Refunding,
                        Description = $"Refund for cancelled CheckoutOrder {checkoutOrder.CheckoutOrderCode}.",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ExpiredAt = DateTime.UtcNow.AddMinutes(5)
                    };
                    _paymentRepository.AddEntity(refundPayment);

                    checkoutOrder.RefundingAmount = refundPayment.Amount;
                    checkoutOrder.Status = CheckoutOrderStatus.CancelledAndRefunding;
                }
                else
                {
                    checkoutOrder.Status = CheckoutOrderStatus.Cancelled;
                }
                checkoutOrder.UpdatedAt = DateTime.UtcNow;
                _checkoutProductOrderRepository.UpdateEntity(checkoutOrder);

                HandleVoucherRestore(checkoutOrder);

                await _unitOfWork.SaveChangeAsync();
                await transaction.CommitAsync();
            }catch(Exception)
            {
                await transaction.RollbackAsync();
                return Result.Failure("Failed to cancel checkout order.", 500);
            }
            
            return Result.Success($"Checkout order and its child orders have been cancelled.");
        }
    
    }
}
