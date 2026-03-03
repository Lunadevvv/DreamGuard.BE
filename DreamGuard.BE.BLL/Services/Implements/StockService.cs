using System;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class StockService : IStockService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IComboRepository _comboRepository;

        public StockService(
            IInventoryRepository inventoryRepository,
            IProductVariantRepository variantRepository,
            IProductRepository productRepository,
            IComboRepository comboRepository)
        {
            _inventoryRepository = inventoryRepository;
            _variantRepository = variantRepository;
            _productRepository = productRepository;
            _comboRepository = comboRepository;
        }

        public async Task<Result> DeductVariantStockAsync(Guid productVariantId, int quantity)
        {
            var inventory = await _inventoryRepository.GetInventoryByVariantIdForUpdateAsync(productVariantId);
            if (inventory == null)
            {
                return Result.Failure($"Inventory not found for variant '{productVariantId}'.", 404);
            }

            if (inventory.Quantity < quantity)
            {
                return Result.Failure($"Insufficient stock for variant '{productVariantId}'. Available: {inventory.Quantity}, Requested: {quantity}.", 400);
            }

            try
            {
                inventory.Quantity -= quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);

                // Update variant status if out of stock
                if (inventory.Quantity == 0)
                {
                    var variant = await _variantRepository.GetVariantByIdForUpdateAsync(productVariantId);
                    if (variant != null && variant.Status == ProductStatus.Published)
                    {
                        variant.Status = ProductStatus.OutOfStock;
                        await _variantRepository.UpdateAsync(variant);

                        // Check if all variants of the product are out of stock
                        await UpdateProductStatusIfAllVariantsOutOfStockAsync(variant.ProductId);
                    }
                }

                return Result.Success("Stock deducted successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure("Concurrent stock update detected. Please retry.", 409);
            }
        }

        public async Task<Result> DeductComboStockAsync(Guid comboId, int orderQuantity)
        {
            var combo = await _comboRepository.GetComboWithProductsForUpdateAsync(comboId);
            if (combo == null)
            {
                return Result.Failure("Combo not found.", 404);
            }

            if (combo.ComboParentId == null)
            {
                return Result.Failure("Only child combos can have stock deducted.", 400);
            }

            // Deduct stock from each component variant
            foreach (var cpv in combo.ComboProductVariants)
            {
                var deductQuantity = cpv.Quantity * orderQuantity;
                var result = await DeductVariantStockAsync(cpv.ProductVariantId, deductQuantity);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            // Check if combo is now out of stock
            var comboStock = CalculateComboStock(combo);
            if (comboStock == 0 && combo.Status == ProductStatus.Published)
            {
                combo.Status = ProductStatus.OutOfStock;
                await _comboRepository.UpdateAsync(combo);
            }

            return Result.Success("Combo stock deducted successfully.");
        }

        public async Task<Result> RestoreVariantStockAsync(Guid productVariantId, int quantity)
        {
            var inventory = await _inventoryRepository.GetInventoryByVariantIdForUpdateAsync(productVariantId);
            if (inventory == null)
            {
                return Result.Failure($"Inventory not found for variant '{productVariantId}'.", 404);
            }

            try
            {
                var previousQuantity = inventory.Quantity;
                inventory.Quantity += quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);

                // If was out of stock, restore status to Published
                if (previousQuantity == 0)
                {
                    var variant = await _variantRepository.GetVariantByIdForUpdateAsync(productVariantId);
                    if (variant != null && variant.Status == ProductStatus.OutOfStock)
                    {
                        variant.Status = ProductStatus.Published;
                        await _variantRepository.UpdateAsync(variant);

                        // Restore product status if it was OutOfStock
                        var product = await _productRepository.GetProductByIdForUpdateAsync(variant.ProductId);
                        if (product != null && product.Status == ProductStatus.OutOfStock)
                        {
                            product.Status = ProductStatus.Published;
                            await _productRepository.UpdateAsync(product);
                        }
                    }
                }

                return Result.Success("Stock restored successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure("Concurrent stock update detected. Please retry.", 409);
            }
        }

        public async Task<Result> RestoreComboStockAsync(Guid comboId, int orderQuantity)
        {
            var combo = await _comboRepository.GetComboWithProductsForUpdateAsync(comboId);
            if (combo == null)
            {
                return Result.Failure("Combo not found.", 404);
            }

            // Restore stock for each component variant
            foreach (var cpv in combo.ComboProductVariants)
            {
                var restoreQuantity = cpv.Quantity * orderQuantity;
                var result = await RestoreVariantStockAsync(cpv.ProductVariantId, restoreQuantity);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            // If combo was out of stock, restore to Published
            if (combo.Status == ProductStatus.OutOfStock)
            {
                combo.Status = ProductStatus.Published;
                await _comboRepository.UpdateAsync(combo);
            }

            return Result.Success("Combo stock restored successfully.");
        }

        private async Task UpdateProductStatusIfAllVariantsOutOfStockAsync(Guid productId)
        {
            var variants = await _variantRepository.GetVariantsByProductIdForStockCheckAsync(productId);
            var allOutOfStock = variants
                .Where(v => v.Status != ProductStatus.Draft && v.Status != ProductStatus.Hidden)
                .All(v => v.Status == ProductStatus.OutOfStock);

            if (allOutOfStock)
            {
                var product = await _productRepository.GetProductByIdForUpdateAsync(productId);
                if (product != null && product.Status == ProductStatus.Published)
                {
                    product.Status = ProductStatus.OutOfStock;
                    await _productRepository.UpdateAsync(product);
                }
            }
        }

        private static int CalculateComboStock(DAL.Models.Combo combo)
        {
            if (combo.ComboProductVariants == null || !combo.ComboProductVariants.Any())
            {
                return 0;
            }

            int minStock = int.MaxValue;
            foreach (var cpv in combo.ComboProductVariants)
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
