using System;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.BLL.Utilities;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IComboRepository _comboRepository;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IUnitOfWork unitOfWork,
            IProductVariantRepository productVariantRepository,
            IProductRepository productRepository,
            IComboRepository comboRepository)
        {
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
            _productVariantRepository = productVariantRepository;
            _productRepository = productRepository;
            _comboRepository = comboRepository;
        }

        public async Task<Result> CreateInventoryAsync(Inventory inventory)
        {
            var result = await _inventoryRepository.CreateAsync(inventory);

            if (result <= 0)
            {
                return Result.Failure("Failed to create inventory.", 400);
            }

            return Result.Success("Inventory created successfully.");
        }

        public async Task<Result> AddInventoryStockAsync(Guid productVariantId, int quantity)
        {
            if (quantity <= 0)
            {
                return Result.Failure("Quantity must be greater than 0.", 400);
            }

            var inventory = await _inventoryRepository.GetInventoryByVariantIdForUpdateAsync(productVariantId);

            if (inventory == null)
            {
                return Result.Failure("Inventory not found for the specified product variant.", 404);
            }

            var productVariant = await _productVariantRepository.GetByIdAsync(productVariantId);
            if (productVariant == null)
            {
                return Result.Failure("Product variant not found.", 404);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                inventory.Quantity += quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                var result = await _inventoryRepository.UpdateAsync(inventory);

                if (result <= 0)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("Failed to update inventory.", 400);
                }

                //Update product variant status if it was out of stock
                if (productVariant.Status == ProductStatus.OutOfStock && inventory.Quantity > 0)
                {
                    productVariant.Status = ProductStatus.Published;
                    var updateVariantResult = await _productVariantRepository.UpdateAsync(productVariant);
                    if (updateVariantResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure("Failed to update product variant status.", 400);
                    }
                }

                // Auto-update OutOfStock combos that contain this variant
                await UpdateComboStatusesAfterStockIncreaseAsync(productVariantId);

                await transaction.CommitAsync();
                return Result.Success("Inventory updated successfully.");
            }catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure($"An error occurred while updating inventory: {ex.Message}", 500);
            }
        }

        public async Task<Result> ReduceInventoryStockAsync(Guid productVariantId, int quantity)
        {
            if (quantity <= 0)
            {
                return Result.Failure("Quantity must be greater than 0.", 400);
            }

            var inventory = await _inventoryRepository.GetInventoryByVariantIdForUpdateAsync(productVariantId);

            if (inventory == null)
            {
                return Result.Failure("Inventory not found for the specified product variant.", 404);
            }

            if (inventory.Quantity < quantity)
            {
                return Result.Failure("Insufficient stock to reduce.", 400);
            }

            var productVariant = await _productVariantRepository.GetByIdAsync(productVariantId);
            if (productVariant == null)
            {
                return Result.Failure("Product variant not found.", 404);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                inventory.Quantity -= quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                var result = await _inventoryRepository.UpdateAsync(inventory);

                if (result <= 0)
                {
                    return Result.Failure("Failed to update inventory.", 400);
                }

                //Update product variant status if it goes out of stock
                if (inventory.Quantity == 0 && productVariant.Status != ProductStatus.OutOfStock)
                {
                    productVariant.Status = ProductStatus.OutOfStock;
                    var updateVariantResult = await _productVariantRepository.UpdateAsync(productVariant);
                    if (updateVariantResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure("Failed to update product variant status.", 400);
                    }
                }

                await transaction.CommitAsync();
                return Result.Success("Inventory updated successfully.");
            }catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure($"An error occurred while updating inventory: {ex.Message}", 500);
            }
        }

        public async Task<Result> UpdateInventoryAsync(Inventory inventory)
        {
            var existingInventory = await _inventoryRepository.GetByIdAsync(inventory.Id);

            if (existingInventory == null)
            {
                return Result.Failure("Inventory not found.", 404);
            }

            existingInventory.Quantity = inventory.Quantity;
            existingInventory.LowStockThreshold = inventory.LowStockThreshold;
            existingInventory.UpdatedAt = DateTime.UtcNow;

            var result = await _inventoryRepository.UpdateAsync(existingInventory);
            if (result <= 0)
            {
                return Result.Failure("Failed to update inventory.", 400);
            }

            return Result.Success("Inventory updated successfully.");
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
                    var variant = await _productVariantRepository.GetVariantByIdForUpdateAsync(productVariantId);
                    if (variant != null && variant.Status == ProductStatus.Published)
                    {
                        variant.Status = ProductStatus.OutOfStock;
                        await _productVariantRepository.UpdateAsync(variant);

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

            // Validate and deduct stock from each component variant using already-loaded tracked entities
            var productIdsToCheckOutOfStock = new HashSet<Guid>();

            foreach (var cpv in combo.ComboProductVariants)
            {
                var inventory = cpv.ProductVariant?.Inventory;
                if (inventory == null)
                {
                    return Result.Failure($"Inventory not found for variant '{cpv.ProductVariantId}'.", 404);
                }

                var deductQuantity = cpv.Quantity * orderQuantity;
                if (inventory.Quantity < deductQuantity)
                {
                    return Result.Failure($"Insufficient stock for variant '{cpv.ProductVariantId}'. Available: {inventory.Quantity}, Requested: {deductQuantity}.", 400);
                }

                inventory.Quantity -= deductQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                // Update variant status if out of stock
                if (inventory.Quantity == 0 && cpv.ProductVariant!.Status == ProductStatus.Published)
                {
                    cpv.ProductVariant.Status = ProductStatus.OutOfStock;
                    productIdsToCheckOutOfStock.Add(cpv.ProductVariant.ProductId);
                }
            }

            try
            {
                await _unitOfWork.SaveChangeAsync();

                // Check if all variants of affected products are out of stock
                foreach (var productId in productIdsToCheckOutOfStock)
                {
                    await UpdateProductStatusIfAllVariantsOutOfStockAsync(productId);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure("Concurrent stock update detected. Please retry.", 409);
            }

            // Check if combo is now out of stock
            var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
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
                    var variant = await _productVariantRepository.GetVariantByIdForUpdateAsync(productVariantId);
                    if (variant != null && variant.Status == ProductStatus.OutOfStock)
                    {
                        variant.Status = ProductStatus.Published;
                        await _productVariantRepository.UpdateAsync(variant);

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

            // Restore stock for each component variant using already-loaded tracked entities
            var variantIdsToRestoreStatus = new List<(Guid ProductVariantId, Guid ProductId)>();

            foreach (var cpv in combo.ComboProductVariants)
            {
                var inventory = cpv.ProductVariant?.Inventory;
                if (inventory == null)
                {
                    return Result.Failure($"Inventory not found for variant '{cpv.ProductVariantId}'.", 404);
                }

                var previousQuantity = inventory.Quantity;
                var restoreQuantity = cpv.Quantity * orderQuantity;
                inventory.Quantity += restoreQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                // If was out of stock, mark for status restore
                if (previousQuantity == 0 && cpv.ProductVariant!.Status == ProductStatus.OutOfStock)
                {
                    cpv.ProductVariant.Status = ProductStatus.Published;
                    variantIdsToRestoreStatus.Add((cpv.ProductVariantId, cpv.ProductVariant.ProductId));
                }
            }

            try
            {
                await _unitOfWork.SaveChangeAsync();

                // Restore product status if it was OutOfStock
                var productIdsToRestore = variantIdsToRestoreStatus.Select(v => v.ProductId).Distinct();
                foreach (var productId in productIdsToRestore)
                {
                    var product = await _productRepository.GetProductByIdForUpdateAsync(productId);
                    if (product != null && product.Status == ProductStatus.OutOfStock)
                    {
                        product.Status = ProductStatus.Published;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure("Concurrent stock update detected. Please retry.", 409);
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
            var variants = await _productVariantRepository.GetVariantsByProductIdForStockCheckAsync(productId);
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

        private async Task UpdateComboStatusesAfterStockIncreaseAsync(Guid productVariantId)
        {
            var outOfStockCombos = await _comboRepository.GetOutOfStockCombosByVariantIdAsync(productVariantId);

            var parentComboIdsToRestore = new HashSet<Guid>();

            foreach (var combo in outOfStockCombos)
            {
                var comboStock = StockCalculator.CalculateComboStock(combo.ComboProductVariants);
                if (comboStock > 0)
                {
                    combo.Status = ProductStatus.Published;

                    if (combo.ComboParentId.HasValue)
                    {
                        parentComboIdsToRestore.Add(combo.ComboParentId.Value);
                    }
                }
            }

            // Batch save all combo status changes
            await _unitOfWork.SaveChangeAsync();

            // Restore parent combo statuses (deduplicated)
            foreach (var parentComboId in parentComboIdsToRestore)
            {
                await RestoreParentComboStatusIfNeededAsync(parentComboId);
            }
        }

        private async Task RestoreParentComboStatusIfNeededAsync(Guid parentComboId)
        {
            var parentCombo = await _comboRepository.GetComboByIdForUpdateAsync(parentComboId);
            if (parentCombo == null || parentCombo.Status != ProductStatus.OutOfStock)
                return;

            var allChildren = await _comboRepository.GetAllChildrenOfParentAsync(parentComboId);
            if (allChildren.Any(c => c.Status == ProductStatus.Published))
            {
                parentCombo.Status = ProductStatus.Published;
                await _comboRepository.UpdateAsync(parentCombo);
            }
        }

        public async Task<Result> AddDefectStockAsync(Guid productVariantId, int quantity)
        {
            if (quantity <= 0) return Result.Failure("Quantity must be greater than 0.", 400);

            var inventory = await _inventoryRepository.GetInventoryByVariantIdForUpdateAsync(productVariantId);
            if (inventory == null) return Result.Failure("Inventory not found.", 404);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                inventory.DefectQuantity += quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                var res = await _inventoryRepository.UpdateAsync(inventory);
                if (res <= 0)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("Failed to update inventory defect stock.", 400);
                }
                await transaction.CommitAsync();
                return Result.Success("Defect stock updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure($"An error occurred while updating defect inventory: {ex.Message}", 500);
            }
        }

        public async Task<Result> ReduceDefectStockAsync(Guid productVariantId, int quantity)
        {
            if (quantity <= 0) return Result.Failure("Quantity must be greater than 0.", 400);

            var inventory = await _inventoryRepository.GetInventoryByVariantIdForUpdateAsync(productVariantId);
            if (inventory == null) return Result.Failure("Inventory not found.", 404);

            if (inventory.DefectQuantity < quantity)
                return Result.Failure("Insufficient defect stock to reduce.", 400);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                inventory.DefectQuantity -= quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                var res = await _inventoryRepository.UpdateAsync(inventory);
                if (res <= 0)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure("Failed to update inventory defect stock.", 400);
                }
                await transaction.CommitAsync();
                return Result.Success("Defect stock updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure($"An error occurred while updating defect inventory: {ex.Message}", 500);
            }
        }

        public async Task<Result> RestoreDefectVariantStockAsync(Guid productVariantId, int quantity)
        {
            var inventory = await _inventoryRepository.GetInventoryByVariantIdForUpdateAsync(productVariantId);
            if (inventory == null) return Result.Failure($"Inventory not found for variant '{productVariantId}'.", 404);

            try
            {
                inventory.DefectQuantity += quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);
                // Note: We DO NOT change ProductStatus since this is defect stock
                return Result.Success("Defect stock restored successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure("Concurrent stock update detected. Please retry.", 409);
            }
        }

        public async Task<Result> RestoreDefectComboStockAsync(Guid comboId, int orderQuantity)
        {
            var combo = await _comboRepository.GetComboWithProductsForUpdateAsync(comboId);
            if (combo == null) return Result.Failure("Combo not found.", 404);

            foreach (var cpv in combo.ComboProductVariants)
            {
                var inventory = cpv.ProductVariant?.Inventory;
                if (inventory == null) return Result.Failure($"Inventory not found for variant '{cpv.ProductVariantId}'.", 404);

                var restoreQuantity = cpv.Quantity * orderQuantity;
                inventory.DefectQuantity += restoreQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                // Note: We DO NOT track or restore ProductStatus for defect stock
            }

            try
            {
                await _unitOfWork.SaveChangeAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure("Concurrent stock update detected. Please retry.", 409);
            }

            return Result.Success("Defect combo stock restored successfully.");
        }
    }
}
