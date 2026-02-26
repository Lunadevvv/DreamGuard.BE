using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.BLL.Common;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.BLL.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<Result> AddInventoryStockAsync(Guid productVariantId, int quantity);
        Task<Result> ReduceInventoryStockAsync(Guid productVariantId, int quantity);
        Task<Result> CreateInventoryAsync(Inventory inventory);
        Task<Result> UpdateInventoryAsync(Inventory inventory);
    }
}