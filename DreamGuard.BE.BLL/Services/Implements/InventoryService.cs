using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductVariantRepository _productVariantRepository;
        public InventoryService(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork, IProductVariantRepository productVariantRepository)
        {
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
            _productVariantRepository = productVariantRepository;
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
    }
}