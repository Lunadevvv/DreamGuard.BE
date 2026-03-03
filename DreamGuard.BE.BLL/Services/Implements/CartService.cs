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

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IComboRepository _comboRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(
            ICartRepository cartRepository,
            IProductVariantRepository variantRepository,
            IComboRepository comboRepository,
            IInventoryRepository inventoryRepository,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _variantRepository = variantRepository;
            _comboRepository = comboRepository;
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CartResponse>> GetCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetCartWithItemsAsync(userId);

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
                TotalAmount = items.Where(i => i.IsAvailable).Sum(i => i.SubTotal),
                TotalItems = items.Count
            });
        }

        public async Task<Result> AddToCartAsync(Guid userId, AddToCartRequest request)
        {
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
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _cartRepository.CreateAsync(cart);
            }

            // Check if item already exists in cart
            var existingItem = await _cartRepository.GetCartItemAsync(
                cart.Id, request.ProductVariantId, request.ComboId);

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
                    AddedAt = DateTime.UtcNow
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
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
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
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
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
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
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
            if (request.Items == null || !request.Items.Any())
            {
                return await GetCartAsync(userId);
            }

            // Get or create cart
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _cartRepository.CreateAsync(cart);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    // Validate exactly one of ProductVariantId or ComboId
                    if (item.ProductVariantId.HasValue == item.ComboId.HasValue)
                        continue;

                    // Validate stock and availability - skip invalid items
                    int availableStock;
                    if (item.ProductVariantId.HasValue)
                    {
                        var validationResult = await ValidateVariantForCart(item.ProductVariantId.Value);
                        if (!validationResult.Succeeded) continue;
                        availableStock = validationResult.Data;
                    }
                    else
                    {
                        var validationResult = await ValidateComboForCart(item.ComboId!.Value);
                        if (!validationResult.Succeeded) continue;
                        availableStock = validationResult.Data;
                    }

                    // Clamp quantity to available stock
                    var quantity = Math.Min(item.Quantity, availableStock);
                    if (quantity <= 0) continue;

                    // Check if item already exists in cart
                    var existingItem = await _cartRepository.GetCartItemAsync(
                        cart.Id, item.ProductVariantId, item.ComboId);

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
                            AddedAt = DateTime.UtcNow
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

            var comboStock = CalculateComboStock(combo.ComboProductVariants);
            if (comboStock <= 0)
            {
                return Result<int>.Failure($"Combo '{comboId}' is out of stock.", 400);
            }

            return Result<int>.Success(comboStock);
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
                    IsAvailable = isAvailable
                };
            }
            else if (ci.ComboId.HasValue && ci.Combo != null)
            {
                var combo = ci.Combo;
                var comboStock = CalculateComboStock(combo.ComboProductVariants);
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
