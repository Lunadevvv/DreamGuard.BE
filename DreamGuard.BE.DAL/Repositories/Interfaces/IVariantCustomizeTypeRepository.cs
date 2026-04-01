using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamGuard.BE.DAL.Basic;
using DreamGuard.BE.DAL.Models;

namespace DreamGuard.BE.DAL.Repositories.Interfaces
{
    public interface IVariantCustomizeTypeRepository : IGenericRepository<VariantCustomizeType>
    {
        Task<VariantCustomizeType?> GetByCompositeKeyAsync(Guid cusId, Guid productVariantId);
        Task<List<VariantCustomizeType>> GetByVariantIdAsync(Guid productVariantId);
        Task<List<VariantCustomizeType>> GetByVariantIdWithDetailsAsync(Guid productVariantId);
        Task<List<VariantCustomizeType>> GetByVariantIdsWithDetailsAsync(IEnumerable<Guid> productVariantIds);
        Task AddRangeAsync(IEnumerable<VariantCustomizeType> entities);
    }
}
