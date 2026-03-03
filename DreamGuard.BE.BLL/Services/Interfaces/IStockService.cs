using System;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IStockService
    {
        Task<Result> DeductVariantStockAsync(Guid productVariantId, int quantity);
        Task<Result> DeductComboStockAsync(Guid comboId, int orderQuantity);
        Task<Result> RestoreVariantStockAsync(Guid productVariantId, int quantity);
        Task<Result> RestoreComboStockAsync(Guid comboId, int orderQuantity);
    }
}
