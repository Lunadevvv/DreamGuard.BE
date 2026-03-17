using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
        Task<Inventory?> GetInventoryByVariantIdAsync(Guid productVariantId);
        Task<Inventory?> GetInventoryByVariantIdForUpdateAsync(Guid productVariantId);
        Task<List<Inventory>> GetInventoriesByVariantIdsAsync(List<Guid> variantIds);
        Task<List<Inventory>> GetInventoriesByVariantIdsForUpdateAsync(List<Guid> variantIds);
    }
}