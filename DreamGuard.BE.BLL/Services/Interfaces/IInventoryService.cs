using System;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IInventoryService
    {
        // CRUD
        Task<Result> CreateInventoryAsync(Inventory inventory);
        Task<Result> UpdateInventoryAsync(Inventory inventory);

        // Admin manual stock management
        Task<Result> AddInventoryStockAsync(Guid productVariantId, int quantity);
        Task<Result> ReduceInventoryStockAsync(Guid productVariantId, int quantity);

        // Order stock operations
        Task<Result> DeductVariantStockAsync(Guid productVariantId, int quantity);
        Task<Result> DeductComboStockAsync(Guid comboId, int orderQuantity);
        Task<Result> RestoreVariantStockAsync(Guid productVariantId, int quantity);
        Task<Result> RestoreComboStockAsync(Guid comboId, int orderQuantity);

        // Defect stock operations
        Task<Result> AddDefectStockAsync(Guid productVariantId, int quantity);
        Task<Result> ReduceDefectStockAsync(Guid productVariantId, int quantity);
        Task<Result> RestoreDefectVariantStockAsync(Guid productVariantId, int quantity);
        Task<Result> RestoreDefectComboStockAsync(Guid comboId, int orderQuantity);
    }
}