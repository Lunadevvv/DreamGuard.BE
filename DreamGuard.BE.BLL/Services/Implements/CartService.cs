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
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using DreamGuard.BE.BLL.Utilities;
using DreamGuard.BE.DAL.ModelExtensions;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IComboRepository _comboRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductCustomizeTypeRepository _productCustomizeTypeRepository;
        private readonly IVariantCustomizeTypeRepository _variantCustomizeTypeRepository;

        public CartService(
            ICartRepository cartRepository,
            IProductVariantRepository variantRepository,
            IComboRepository comboRepository,
            IInventoryRepository inventoryRepository,
            IUnitOfWork unitOfWork,
            ICustomerRepository customerRepository,
            IProductCustomizeTypeRepository productCustomizeTypeRepository,
            IVariantCustomizeTypeRepository variantCustomizeTypeRepository)
        {
            _cartRepository = cartRepository;
            _variantRepository = variantRepository;
            _comboRepository = comboRepository;
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _productCustomizeTypeRepository = productCustomizeTypeRepository;
            _variantCustomizeTypeRepository = variantCustomizeTypeRepository;
        }

        public async Task<Result<CartResponse>> GetCartAsync(Guid userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<CartResponse>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var cart = await _cartRepository.GetCartWithItemsAsync(customerId);

            if (cart == null || !cart.CartItems.Any())
            {
                return Result<CartResponse>.Success(new CartResponse
                {
                    CartId = cart?.Id ?? Guid.Empty,
                    Items = new List<CartItemResponse>(),
                    TotalAmount = 0,
                    TotalItems = 0
                });
            }

            var items = cart.CartItems.Select(MapCartItemToResponse).ToList();

            return Result<CartResponse>.Success(new CartResponse
            {
                CartId = cart.Id,
                Items = items,
                TotalAmount = items.Where(i => i.IsAvailable).Sum(i => i.SubTotal + i.TotalAddOnPrice),
                TotalItems = items.Count
            });
        }

        public async Task<Result> AddToCartAsync(Guid userId, AddToCartRequest request)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            // Validate exactly one of ProductVariantId or ComboId
            if (request.ProductVariantId.HasValue == request.ComboId.HasValue)
            {
                return Result.Failure("Exactly one of ProductVariantId or ComboId must be provided.", 400);
            }

            // Validate stock and availability
            int availableStock;
            if (request.ProductVariantId.HasValue)
            {
                var validationResult = await ValidateVariantForCart(request.ProductVariantId.Value);
                if (!validationResult.Succeeded)
                    return Result.Failure(validationResult.Error!, validationResult.StatusCode);
                availableStock = validationResult.Data;
            }
            else
            {
                var validationResult = await ValidateComboForCart(request.ComboId!.Value);
                if (!validationResult.Succeeded)
                    return Result.Failure(validationResult.Error!, validationResult.StatusCode);
                availableStock = validationResult.Data;
            }

            // Get or create cart
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _cartRepository.CreateAsync(cart);
            }

            List<VariantCustomizeType> variantCustomizations = new();
            if (request.ProductVariantId.HasValue)
            {
                variantCustomizations = await _variantCustomizeTypeRepository.GetByVariantIdWithDetailsAsync(request.ProductVariantId.Value);
            }

            var requestedCusIds = request.ProductCustomizeDetailRequest.Select(r => r.ProductCustomizeTypeId).ToList();
            var validCustomizations = variantCustomizations.Where(vc => requestedCusIds.Contains(vc.CusId)).ToList();
            
            var materialsCount = validCustomizations.Count(vc => vc.ProductCustomizeType?.Category == CustomizeCategory.Material);
            var colorsCount = validCustomizations.Count(vc => vc.ProductCustomizeType?.Category == CustomizeCategory.Color);
            var patternsCount = validCustomizations.Count(vc => vc.ProductCustomizeType?.Category == CustomizeCategory.Pattern);

            if (materialsCount > 1 || colorsCount > 1 || patternsCount > 1)
            {
                 return Result.Failure("Only max 1 Material, 1 Color, and 1 Pattern can be selected.", 400);
            }

            decimal variantSalePrice = 0;
            if (request.ProductVariantId.HasValue)
            {
                var variantInfo = await _variantRepository.GetVariantByIdAsync(request.ProductVariantId.Value);
                if (variantInfo != null) variantSalePrice = variantInfo.SalePrice;
            }

            var customizeDetails = request.ProductCustomizeDetailRequest.Select(d =>
            {
                var vc = validCustomizations.FirstOrDefault(vc => vc.CusId == d.ProductCustomizeTypeId);
                if (vc == null) return null;

                decimal addOnPrice = 0;
                var type = vc.ProductCustomizeType;
                if (type.CalculationMode == PriceCalculationMode.Multiplier)
                {
                    double multiplier = vc.OverrideMultiplier ?? type.DefaultMultiplier ?? 1.0;
                    if (multiplier > 1.0)
                    {
                        addOnPrice = variantSalePrice * (decimal)(multiplier - 1.0);
                    }
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

            var customizeHash = GenerateCustomizeHash(customizeDetails);

            // Check if item already exists in cart
            var existingItem = await _cartRepository.GetCartItemAsync(
                cart.Id, request.ProductVariantId, request.ComboId, customizeHash);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + request.Quantity;
                if (newQuantity > availableStock)
                {
                    return Result.Failure(
                        $"Cannot add {request.Quantity} more. Current in cart: {existingItem.Quantity}, Available stock: {availableStock}.", 400);
                }

                existingItem.Quantity = newQuantity;
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                if (request.Quantity > availableStock)
                {
                    return Result.Failure(
                        $"Requested quantity ({request.Quantity}) exceeds available stock ({availableStock}).", 400);
                }

                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductVariantId = request.ProductVariantId,
                    ComboId = request.ComboId,
                    Quantity = request.Quantity,
                    AddedAt = DateTime.UtcNow,
                    ProductCustomizeDetails = customizeDetails,
                    CustomizeHash = customizeHash
                };
                await _cartRepository.AddCartItemAsync(cartItem);
            }

            // Update cart timestamp
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return Result.Success("Item added to cart successfully.");
        }

        public async Task<Result> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                return Result.Failure("Cart not found.", 404);
            }

            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.CartId != cart.Id)
            {
                return Result.Failure("Cart item not found.", 404);
            }

            // Validate stock
            int availableStock;
            if (cartItem.ProductVariantId.HasValue)
            {
                var validationResult = await ValidateVariantForCart(cartItem.ProductVariantId.Value);
                if (!validationResult.Succeeded)
                    return Result.Failure(validationResult.Error!, validationResult.StatusCode);
                availableStock = validationResult.Data;
            }
            else
            {
                var validationResult = await ValidateComboForCart(cartItem.ComboId!.Value);
                if (!validationResult.Succeeded)
                    return Result.Failure(validationResult.Error!, validationResult.StatusCode);
                availableStock = validationResult.Data;
            }

            if (request.Quantity > availableStock)
            {
                return Result.Failure(
                    $"Requested quantity ({request.Quantity}) exceeds available stock ({availableStock}).", 400);
            }

            cartItem.Quantity = request.Quantity;
            await _cartRepository.UpdateCartItemAsync(cartItem);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return Result.Success("Cart item updated successfully.");
        }

        public async Task<Result> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                return Result.Failure("Cart not found.", 404);
            }

            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.CartId != cart.Id)
            {
                return Result.Failure("Cart item not found.", 404);
            }

            await _cartRepository.RemoveCartItemAsync(cartItem);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return Result.Success("Cart item removed successfully.");
        }

        public async Task<Result> ClearCartAsync(Guid userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                return Result.Success("Cart is already empty.");
            }

            await _cartRepository.ClearCartItemsAsync(cart.Id);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return Result.Success("Cart cleared successfully.");
        }

        public async Task<Result<CartResponse>> SyncCartAsync(Guid userId, SyncCartRequest request)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                return Result<CartResponse>.Failure("Customer profile not found.", 404);

            var customerId = customer.CustomerId;

            if (request.Items == null || !request.Items.Any())
            {
                return await GetCartAsync(userId);
            }

            // Get or create cart
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _cartRepository.CreateAsync(cart);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Pre-load all variants, inventories, combos, and existing cart items to avoid N+1 queries
                var variantIds = request.Items
                    .Where(i => i.ProductVariantId.HasValue)
                    .Select(i => i.ProductVariantId!.Value).ToList();
                var comboIds = request.Items
                    .Where(i => i.ComboId.HasValue)
                    .Select(i => i.ComboId!.Value)
                    .Distinct()
                    .ToList();

                var combosDict = (await _comboRepository.GetCombosWithProductsByIdsAsync(comboIds))
                    .ToDictionary(c => c.Id);

                var variantsDict = (await _variantRepository.GetVariantsByIdsAsync(variantIds))
                    .ToDictionary(v => v.Id);
                var inventoriesDict = (await _inventoryRepository.GetInventoriesByVariantIdsAsync(variantIds))
                    .ToDictionary(i => i.ProductVariantId);
                var existingCartItems = (await _cartRepository.GetCartItemsByCartIdAsync(cart.Id))
                    .GroupBy(ci => (ci.ProductVariantId, ci.ComboId, ci.CustomizeHash))
                    .ToDictionary(g => g.Key, g => g.First());

                var allVariantCusList = await _variantCustomizeTypeRepository.GetByVariantIdsWithDetailsAsync(variantIds);
                var variantCusDict = allVariantCusList.GroupBy(vc => vc.ProductVariantId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in request.Items)
                {
                    // Validate exactly one of ProductVariantId or ComboId
                    if (item.ProductVariantId.HasValue == item.ComboId.HasValue)
                        continue;

                    // Validate stock and availability - skip invalid items using pre-loaded data
                    int availableStock;
                    if (item.ProductVariantId.HasValue)
                    {
                        variantsDict.TryGetValue(item.ProductVariantId.Value, out var variant);
                        if (variant == null || variant.Status != ProductStatus.Published)
                            continue;

                        inventoriesDict.TryGetValue(item.ProductVariantId.Value, out var inventory);
                        if (inventory == null || inventory.Quantity <= 0)
                            continue;

                        availableStock = inventory.Quantity;
                    }
                    else
                    {
                        combosDict.TryGetValue(item.ComboId!.Value, out var combo);
                        if (combo == null || combo.ComboParentId == null || combo.Status != ProductStatus.Published) continue;

                        var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
                        if (comboStock <= 0) continue;
                        
                        availableStock = comboStock;
                    }

                    // Clamp quantity to available stock
                    var quantity = Math.Min(item.Quantity, availableStock);
                    if (quantity <= 0) continue;

                    List<VariantCustomizeType> itemVariantCustomizations = new();
                    decimal variantSalePrice = 0;
                    if (item.ProductVariantId.HasValue)
                    {
                        variantCusDict.TryGetValue(item.ProductVariantId.Value, out itemVariantCustomizations);
                        itemVariantCustomizations ??= new List<VariantCustomizeType>();
                        
                        variantsDict.TryGetValue(item.ProductVariantId.Value, out var variantInfo);
                        if (variantInfo != null) variantSalePrice = variantInfo.SalePrice;
                    }

                    var requestedCusIds = item.ProductCustomizeDetailRequest.Select(r => r.ProductCustomizeTypeId).ToList();
                    var validCustomizations = itemVariantCustomizations.Where(vc => requestedCusIds.Contains(vc.CusId)).ToList();
                    
                    var materialsCount = validCustomizations.Count(vc => vc.ProductCustomizeType?.Category == CustomizeCategory.Material);
                    var colorsCount = validCustomizations.Count(vc => vc.ProductCustomizeType?.Category == CustomizeCategory.Color);
                    var patternsCount = validCustomizations.Count(vc => vc.ProductCustomizeType?.Category == CustomizeCategory.Pattern);

                    if (materialsCount > 1 || colorsCount > 1 || patternsCount > 1) continue;

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
                            {
                                addOnPrice = variantSalePrice * (decimal)(multiplier - 1.0);
                            }
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
                    var itemHash = GenerateCustomizeHash(itemCustomizeDetails);

                    // Check if item already exists in cart using pre-loaded data
                    existingCartItems.TryGetValue((item.ProductVariantId, item.ComboId, itemHash), out var existingItem);

                    if (existingItem != null)
                    {
                        // Replace quantity with guest cart's quantity (per requirements)
                        existingItem.Quantity = quantity;
                        await _cartRepository.UpdateCartItemAsync(existingItem);
                    }
                    else
                    {
                        var cartItem = new CartItem
                        {
                            Id = Guid.NewGuid(),
                            CartId = cart.Id,
                            ProductVariantId = item.ProductVariantId,
                            ComboId = item.ComboId,
                            Quantity = quantity,
                            AddedAt = DateTime.UtcNow,
                            ProductCustomizeDetails = itemCustomizeDetails,
                            CustomizeHash = itemHash
                        };
                        await _cartRepository.AddCartItemAsync(cartItem);
                    }
                }

                cart.UpdatedAt = DateTime.UtcNow;
                await _cartRepository.UpdateAsync(cart);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<CartResponse>.Failure("Failed to sync cart.", 500);
            }

            return await GetCartAsync(userId);
        }

        private async Task<Result<int>> ValidateVariantForCart(Guid productVariantId)
        {
            var variant = await _variantRepository.GetVariantByIdAsync(productVariantId);
            if (variant == null)
            {
                return Result<int>.Failure($"Product variant '{productVariantId}' not found.", 404);
            }

            if (variant.Status != ProductStatus.Published)
            {
                return Result<int>.Failure($"Product variant '{productVariantId}' is not available.", 400);
            }

            var inventory = await _inventoryRepository.GetInventoryByVariantIdAsync(productVariantId);
            if (inventory == null || inventory.Quantity <= 0)
            {
                return Result<int>.Failure($"Product variant '{productVariantId}' is out of stock.", 400);
            }

            return Result<int>.Success(inventory.Quantity);
        }

        private async Task<Result<int>> ValidateComboForCart(Guid comboId)
        {
            var combo = await _comboRepository.GetComboWithProductsAsync(comboId);
            if (combo == null)
            {
                return Result<int>.Failure($"Combo '{comboId}' not found.", 404);
            }

            if (combo.ComboParentId == null)
            {
                return Result<int>.Failure("Only child combos can be added to cart.", 400);
            }

            if (combo.Status != ProductStatus.Published)
            {
                return Result<int>.Failure($"Combo '{comboId}' is not available.", 400);
            }

            var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
            if (comboStock <= 0)
            {
                return Result<int>.Failure($"Combo '{comboId}' is out of stock.", 400);
            }

            return Result<int>.Success(comboStock);
        }

        private static string GenerateCustomizeHash(List<ProductCustomizeDetail> details)
        {
            if (details == null || !details.Any()) return string.Empty;

            var sortedDetails = details
                .OrderBy(d => d.CustomizeTypeName)
                .ThenBy(d => d.CustomizeContent)
                .ToList();

            var combinedString = string.Join("|", sortedDetails.Select(d => $"{d.CustomizeTypeName}:{d.CustomizeContent}"));

            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(combinedString);
            var hashBytes = md5.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        private static CartItemResponse MapCartItemToResponse(CartItem ci)
        {
            if (ci.ProductVariantId.HasValue && ci.ProductVariant != null)
            {
                var variant = ci.ProductVariant;
                var stock = variant.Inventory?.Quantity ?? 0;
                var isAvailable = variant.Status == ProductStatus.Published && stock > 0;
                var imageUrl = variant.Product?.Assets?.FirstOrDefault()?.Url;

                return new CartItemResponse
                {
                    Id = ci.Id,
                    ProductVariantId = ci.ProductVariantId,
                    ComboId = null,
                    ItemName = $"{variant.Product?.Name ?? "Unknown"} - {variant.Size}",
                    Sku = variant.Sku,
                    ImageUrl = imageUrl,
                    UnitPrice = variant.SalePrice,
                    Quantity = ci.Quantity,
                    SubTotal = variant.SalePrice * ci.Quantity,
                    AvailableStock = stock,
                    IsAvailable = isAvailable,
                    ProductCustomizeDetails = ci.ProductCustomizeDetails ?? new(),
                    TotalAddOnPrice = ci.ProductCustomizeDetails?.Sum(d => d.AddOnPrice) * ci.Quantity ?? 0
                };
            }
            else if (ci.ComboId.HasValue && ci.Combo != null)
            {
                var combo = ci.Combo;
                var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
                var isAvailable = combo.Status == ProductStatus.Published && comboStock > 0;

                return new CartItemResponse
                {
                    Id = ci.Id,
                    ProductVariantId = null,
                    ComboId = ci.ComboId,
                    ItemName = combo.Name,
                    Sku = null,
                    ImageUrl = combo.ImageUrl,
                    UnitPrice = combo.SalePrice,
                    Quantity = ci.Quantity,
                    SubTotal = combo.SalePrice * ci.Quantity,
                    AvailableStock = comboStock,
                    IsAvailable = isAvailable
                };
            }

            // Orphaned cart item (product/combo deleted)
            return new CartItemResponse
            {
                Id = ci.Id,
                ProductVariantId = ci.ProductVariantId,
                ComboId = ci.ComboId,
                ItemName = "Item no longer available",
                Quantity = ci.Quantity,
                IsAvailable = false
            };
        }
    }
}
