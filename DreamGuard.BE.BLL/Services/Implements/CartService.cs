using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Requests;
using DreamGuard.BE.BLL.Responses;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductVariantRepository _variantRepo;
        private readonly IComboRepository _comboRepo;

        public CartService(
            ICartRepository cartRepo,
            IProductVariantRepository variantRepo,
            IComboRepository comboRepo)
        {
            _cartRepo = cartRepo;
            _variantRepo = variantRepo;
            _comboRepo = comboRepo;
        }

        public async Task<Result<CartResponse>> GetCartAsync(Guid userId)
        {
            var cart = await _cartRepo.GetCartWithItemsAsync(userId);
            if (cart == null)
            {
                return Result<CartResponse>.Success(new CartResponse());
            }

            var response = new CartResponse
            {
                Id = cart.Id,
                Items = cart.CartItems.Select(ci => new CartItemResponse
                {
                    Id = ci.Id,
                    ProductVariantId = ci.ProductVariantId,
                    ComboId = ci.ComboId,
                    ItemName = ci.ProductVariant != null
                        ? ci.ProductVariant.Product?.Name ?? string.Empty
                        : ci.Combo?.Name ?? string.Empty,
                    Sku = ci.ProductVariant?.Sku,
                    ImageUrl = ci.ProductVariant?.Product?.Assets?.FirstOrDefault()?.Url
                        ?? ci.Combo?.ImageUrl,
                    UnitPrice = ci.ProductVariant != null
                        ? ci.ProductVariant.SalePrice
                        : ci.Combo?.SalePrice ?? 0,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.Quantity * (ci.ProductVariant != null
                        ? ci.ProductVariant.SalePrice
                        : ci.Combo?.SalePrice ?? 0),
                    AvailableStock = ci.ProductVariant?.Inventory?.Quantity
                }).ToList(),
                TotalItems = cart.CartItems.Count
            };
            response.CartTotal = response.Items.Sum(i => i.TotalPrice);

            return Result<CartResponse>.Success(response);
        }

        public async Task<Result> AddToCartAsync(Guid userId, AddToCartRequest request)
        {
            if (request.ProductVariantId == null && request.ComboId == null)
                return Result.Failure("Either ProductVariantId or ComboId must be provided.", 400);
            if (request.ProductVariantId != null && request.ComboId != null)
                return Result.Failure("Cannot add both ProductVariant and Combo in one item.", 400);

            if (request.ProductVariantId.HasValue)
            {
                var variant = await _variantRepo.GetVariantByIdAsync(request.ProductVariantId.Value);
                if (variant == null || variant.Status != ProductStatus.Published)
                    return Result.Failure("Product variant not found or not available.", 404);
            }
            else
            {
                var combo = await _comboRepo.GetComboByIdAsync(request.ComboId!.Value);
                if (combo == null || combo.Status != ProductStatus.Published)
                    return Result.Failure("Combo not found or not available.", 404);
            }

            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepo.CreateAsync(cart);
            }

            var existingItem = await _cartRepo.GetCartItemAsync(
                cart.Id, request.ProductVariantId, request.ComboId);

            if (existingItem != null)
            {
                existingItem.Quantity = request.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _cartRepo.UpdateCartItemAsync(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = request.ProductVariantId,
                    ComboId = request.ComboId,
                    Quantity = request.Quantity
                };
                await _cartRepo.AddCartItemAsync(cartItem);
            }

            return Result.Success("Item added to cart successfully.");
        }

        public async Task<Result> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
                return Result.Failure("Cart not found.", 404);

            var cartItem = await _cartRepo.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.CartId != cart.Id)
                return Result.Failure("Cart item not found.", 404);

            cartItem.Quantity = request.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await _cartRepo.UpdateCartItemAsync(cartItem);

            return Result.Success("Cart item updated successfully.");
        }

        public async Task<Result> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
                return Result.Failure("Cart not found.", 404);

            var cartItem = await _cartRepo.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.CartId != cart.Id)
                return Result.Failure("Cart item not found.", 404);

            await _cartRepo.RemoveCartItemAsync(cartItem);

            return Result.Success("Cart item removed successfully.");
        }

        public async Task<Result> ClearCartAsync(Guid userId)
        {
            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
                return Result.Failure("Cart not found.", 404);

            await _cartRepo.RemoveAllCartItemsAsync(cart.Id);

            return Result.Success("Cart cleared successfully.");
        }

        public async Task<Result> SyncCartAsync(Guid userId, SyncCartRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return Result.Failure("Items list cannot be empty.", 400);

            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepo.CreateAsync(cart);
            }

            foreach (var item in request.Items)
            {
                if (item.ProductVariantId == null && item.ComboId == null)
                    continue;
                if (item.ProductVariantId != null && item.ComboId != null)
                    continue;

                // Validate entity exists
                if (item.ProductVariantId.HasValue)
                {
                    var variant = await _variantRepo.GetVariantByIdAsync(item.ProductVariantId.Value);
                    if (variant == null || variant.Status != ProductStatus.Published)
                        continue;
                }
                else
                {
                    var combo = await _comboRepo.GetComboByIdAsync(item.ComboId!.Value);
                    if (combo == null || combo.Status != ProductStatus.Published)
                        continue;
                }

                var existingItem = await _cartRepo.GetCartItemAsync(
                    cart.Id, item.ProductVariantId, item.ComboId);

                if (existingItem != null)
                {
                    // Replace quantity (per sync requirement)
                    existingItem.Quantity = item.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    await _cartRepo.UpdateCartItemAsync(existingItem);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductVariantId = item.ProductVariantId,
                        ComboId = item.ComboId,
                        Quantity = item.Quantity
                    };
                    await _cartRepo.AddCartItemAsync(cartItem);
                }
            }

            return Result.Success("Cart synced successfully.");
        }
    }
}
