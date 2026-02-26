using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.BLL.Services.Interfaces;
using DreamGuard.BE.DAL.Models;
using DreamGuard.BE.DAL.Repositories.Interfaces;

namespace DreamGuard.BE.BLL.Services.Implements
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
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
            var inventory = await _inventoryRepository.GetInventoryByVariantIdAsync(productVariantId);

            if (inventory == null)
            {
                return Result.Failure("Inventory not found for the specified product variant.", 404);
            }

            inventory.Quantity += quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            var result = await _inventoryRepository.UpdateAsync(inventory);

            if (result <= 0)
            {
                return Result.Failure("Failed to update inventory.", 400);
            }

            return Result.Success("Inventory updated successfully.");
        }

        public async Task<Result> ReduceInventoryStockAsync(Guid productVariantId, int quantity)
        {
            var inventory = await _inventoryRepository.GetInventoryByVariantIdAsync(productVariantId);

            if (inventory == null)
            {
                return Result.Failure("Inventory not found for the specified product variant.", 404);
            }

            if (inventory.Quantity < quantity)
            {
                return Result.Failure("Insufficient stock to reduce.", 400);
            }

            inventory.Quantity -= quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            var result = await _inventoryRepository.UpdateAsync(inventory);

            if (result <= 0)
            {
                return Result.Failure("Failed to update inventory.", 400);
            }

            return Result.Success("Inventory updated successfully.");
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